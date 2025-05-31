using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace SlotEffectMaker2023.Action
{
	/// <summary>
	/// ゲーム中のリールの状態を管理するクラス。
	/// 位置、速度、停止位置、押下位置、停止順、すべりコマ数などを保持します。
	/// </summary>
	public class ReelBasicData : SlotMaker2022.ILocalDataInterface
	{   // ゲーム中のリール状態を定義する(Sav)
		// 定数定義
		public const byte REEL_NPOS = byte.MaxValue;
		const float acc = 160.0f;            // リール加速度[rpm]
		const float maxSpeed = 79.5f;        // リール最高速度[rpm]

		// 定義変数
		public float reelPos { get; private set; }    // 現在のリール座標[0, COMA_MAX)
		public float reelSpeed { get; private set; }  // 現在のリール速度[rpm](+:下向き)
		public byte stopPos { get; private set; }     // 停止目標
		public byte pushPos { get; private set; }     // リール押下位置
		public byte stopOrder { get; private set; }   // リールを停止させた順番
		public byte slipCount { get; private set; }   // 停止時すべりコマ数
		public bool isRotate { get; private set; }    // リールが回転中か
		public bool accEnd { get; private set; }      // リールが加速を終えたか

		/// <summary>
		/// デフォルトコンストラクタ。全ての状態を初期化します。
		/// </summary>
		public ReelBasicData()
		{
			reelPos = 0.0f;
			reelSpeed = 0.0f;
			stopPos = 0;
			pushPos = 0;
			stopOrder = 0;
			slipCount = 0;
			isRotate = false;
			accEnd = false;
		}

		/// <summary>
		/// 初期停止位置を指定してコンストラクタを呼び出します。
		/// </summary>
		/// <param name="defaultPos">初期停止位置</param>
		public ReelBasicData(byte defaultPos)
		{
			reelPos = defaultPos;
			reelSpeed = 0.0f;
			stopPos = defaultPos;
			pushPos = defaultPos;
			stopOrder = 0;
			slipCount = 0;
			isRotate = false;
			accEnd = false;
		}

		/// <summary>
		/// 現在のリール状態をバイナリ形式で保存します。
		/// </summary>
		/// <param name="fs">BinaryWriter の参照</param>
		/// <param name="version">保存バージョン</param>
		/// <returns>保存に成功したか（常に true）</returns>
		public bool StoreData(ref BinaryWriter fs, int version)
		{
			fs.Write(reelPos);
			fs.Write(reelSpeed);
			fs.Write(stopPos);
			fs.Write(pushPos);
			fs.Write(stopOrder);
			fs.Write(slipCount);
			fs.Write(isRotate);
			fs.Write(accEnd);
			return true;
		}

		/// <summary>
		/// バイナリ形式からリール状態を読み込みます。
		/// </summary>
		/// <param name="fs">BinaryReader の参照</param>
		/// <param name="version">保存バージョン</param>
		/// <returns>読み込みに成功したか（常に true）</returns>
		public bool ReadData(ref BinaryReader fs, int version)
		{
			reelPos = fs.ReadSingle();
			reelSpeed = fs.ReadSingle();
			stopPos = fs.ReadByte();
			pushPos = fs.ReadByte();
			stopOrder = fs.ReadByte();
			slipCount = fs.ReadByte();
			isRotate = fs.ReadBoolean();
			accEnd = fs.ReadBoolean();
			return true;
		}

		// 制御系変数 //

		/// <summary>
		/// リールの回転を開始します。
		/// </summary>
		public void Start()
		{   // リールを始動させる
			isRotate = true;
			accEnd = false;
			stopPos = REEL_NPOS;
			pushPos = REEL_NPOS;
			stopOrder = REEL_NPOS;
			slipCount = REEL_NPOS;
		}

		/// <summary>
		/// リールが参照するコマIDを取得します（位置補正なし、回転用）。
		/// </summary>
		/// <returns>コマID</returns>
		public byte GetReelComaID()
		{
			return reelSpeed >= 0 ? (byte)Math.Ceiling(reelPos) : (byte)Math.Floor(reelPos);
		}

		/// <summary>
		/// リールが参照するコマIDを取得します（位置補正あり、制御用）。
		/// </summary>
		/// <returns>補正後のコマID</returns>
		public byte GetReelComaIDFixed()
		{
			int ans = GetReelComaID();
			const int comaNum = SlotMaker2022.LocalDataSet.COMA_MAX;
			while (ans >= comaNum) ans -= comaNum;
			while (ans < 0) ans += comaNum;
			return (byte)ans;
		}

		/// <summary>
		/// リールが停止可能かを判定します。
		/// </summary>
		/// <returns>停止可能なら true</returns>
		public bool CanStop()
		{
			// リール回転中 and 速度一定 and 停止制御未実施なら、停止処理可能
			return isRotate && accEnd && pushPos == REEL_NPOS;
		}

		/// <summary>
		/// 停止制御を行い、停目およびすべりコマ数を設定します。
		/// </summary>
		/// <param name="pSlipCount">すべりコマ数</param>
		/// <param name="pStopOrder">停止順（0始まり）</param>
		public void SetStopPos(int pSlipCount, int pStopOrder)
		{
			// リールが回転していない or 速度が一定でない or 停止制御済みの場合、処理を行わない
			if (!CanStop()) return;
			const int comaNum = SlotMaker2022.LocalDataSet.COMA_MAX;
			slipCount = (byte)pSlipCount;
			pushPos = GetReelComaIDFixed();
			stopPos = (byte)((pushPos + slipCount) % comaNum);
			stopOrder = (byte)(pStopOrder + 1);
		}

		/// <summary>
		/// リールの回転処理を1フレーム分実行します。
		/// </summary>
		/// <param name="dt">前フレームからの経過時間（秒）</param>
		public void Process(float dt)
		{
			// リールが回転中でない場合は処理を行わない
			if (!isRotate) return;
			const int comaNum = SlotMaker2022.LocalDataSet.COMA_MAX;

			float y0 = reelPos;   // 座標更新前のリール位置

			/* リール速度から座標を変化させる */
			{
				float accTime = 0.0f;             // 等加速度運動時間
				float v0 = reelSpeed;             // 初速度
				if (Math.Abs(reelSpeed) < Math.Abs(maxSpeed))
				{
					float accDuration = (maxSpeed - reelSpeed) / acc;
					if (accDuration < dt) { accTime = accDuration; reelSpeed = maxSpeed; }
					else { accTime = dt; reelSpeed += acc * accTime; }
				}
				// 位置の増分を計算(rpmから換算して計算)
				const float speedBase = comaNum / 60.0f;
				reelPos += (v0 * speedBase * accTime + acc * speedBase * accTime * accTime / 2.0f);

				// 等速直線運動成分を計算(rpmから換算)
				float slipTime = dt - accTime;
				reelPos += slipTime * reelSpeed * speedBase;
				// 等速直線運動要素があった場合、accEndをtrueにする
				accEnd = slipTime > 0f;
			}

			// リール停止制御判定
			if (stopPos != REEL_NPOS)
			{
				float targetPos = (float)stopPos;
				// 前回リール位置y0に対し、targetPosをリール回転方向の前方にあるように補正する
				// 速度が正で、targetPosが負の方向にある場合
				while (reelSpeed > 0 && targetPos < y0) targetPos += (float)comaNum;
				// 速度が負で、targetPosが正の方向にある場合
				while (reelSpeed < 0 && targetPos > y0) targetPos -= (float)comaNum;

				// 「現在速度によるリール移動距離」が「前回リール位置から停目までの距離」を上回った場合にリールを停止させる(absを用いて正負とも判定する)
				if (Math.Abs(reelPos - y0) >= Math.Abs(targetPos - y0))
				{
					reelPos = targetPos;
					reelSpeed = 0.0f;
					isRotate = false;
				}
			}

			// リール位置を補正する
			while (reelPos >= (float)comaNum) reelPos -= (float)comaNum;
			while (reelPos < 0.0f) reelPos += (float)comaNum;
		}
	}
}
