using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using SlotMaker2022;
using SlotMaker2022.main_function;

namespace SlotEffectMaker2023.Action
{
	/// <summary>
	/// スロットの内部データを管理するクラス。
	/// クレジット数、BET数、モード状態、配当データなどを保持・操作します。
	/// </summary>
	public class SlotBasicData : ILocalDataInterface
	{
		// スロットの内部データを管理する(Sav)
		// 定数
		public const byte CREDIT_MAX = 50;

		public byte slotSetting { get; private set; }   // 設定
		public bool setRandom { get; private set; }     // 設定ランダムを適用しているか(20241020ADD)

		public uint inCount { get; private set; }       // IN枚数
		public uint outCount { get; private set; }      // OUT枚数

		public byte betCount { get; private set; }      // 現在のBET数
		public byte creditShow { get; private set; }    // 現在のクレジット数
		public byte payoutShow { get; private set; }    // 現在の払い出し数
		public bool isBetLatched { get; private set; }  // BETを用いてゲームを開始したか
		public bool isReplay { get; private set; }      // 入賞役がリプレイか

		public byte gameMode { get; private set; }      // 現在のスロットのモード
		public byte modeGameCount { get; private set; } // モード残りゲーム数
		public byte modeJacCount { get; private set; }  // モード残り入賞数
		public int modeMedalCount { get; private set; }// モード残り入賞数(負数で設定なし)
		public byte RTMode { get; private set; }        // 現在のRTモード
		public bool RTOverride { get; private set; }    // RT上書き可否状態
		public int RTGameCount { get; private set; }    // RT残りゲーム数(負数で無限)

		public byte bonusFlag { get; private set; }     // 現在成立中のボーナス
		public byte castFlag { get; private set; }      // 現在成立中の子役

		// 配当データ(使用するものだけ抽出)
		public UserBaseData castLines { get; private set; }   // 配当入賞ライン
		public int castBonusID { get; private set; }          // 入賞したボーナスのID

		/// <summary>
		/// コンストラクタ。初期値を設定します。
		/// </summary>
		public SlotBasicData()
		{
			slotSetting = 5;
			setRandom = false;
			inCount = 0;
			outCount = 0;
			betCount = 0;
			creditShow = CREDIT_MAX;
			payoutShow = 0;
			isBetLatched = false;
			isReplay = false;

			gameMode = 0;
			modeGameCount = 0;
			modeJacCount = 0;
			modeMedalCount = -1;

			RTMode = 0;
			RTOverride = true;
			RTGameCount = -1;

			bonusFlag = 0;
			castFlag = 0;

			const int lineNum = LocalDataSet.PAYLINE_MAX;
			castLines = new UserBaseData(1, true, lineNum);
			castBonusID = 0;
		}

		/// <summary>
		/// 内部データをバイナリ形式で保存します。
		/// </summary>
		/// <param name="fs">BinaryWriter の参照</param>
		/// <param name="version">保存バージョン</param>
		/// <returns>保存に成功したか（常に true）</returns>
		public bool StoreData(ref BinaryWriter fs, int version)
		{
			fs.Write(inCount);
			fs.Write(outCount);
			fs.Write(betCount);
			fs.Write(creditShow);
			fs.Write(payoutShow);
			fs.Write(isBetLatched);
			fs.Write(isReplay);
			fs.Write(gameMode);
			fs.Write(modeGameCount);
			fs.Write(modeJacCount);
			fs.Write(modeMedalCount);
			fs.Write(RTMode);
			fs.Write(RTOverride);
			fs.Write(RTGameCount);
			fs.Write(bonusFlag);
			fs.Write(castFlag);
			fs.Write(decimal.ToByte(castLines.Export()));
			fs.Write(castBonusID);
			if (version >= 1)
			{
				fs.Write(slotSetting);
				fs.Write(setRandom);
			}
			return true;
		}

		/// <summary>
		/// バイナリ形式から内部データを読み込みます。
		/// </summary>
		/// <param name="fs">BinaryReader の参照</param>
		/// <param name="version">保存バージョン</param>
		/// <returns>読込に成功したか（常に true）</returns>
		public bool ReadData(ref BinaryReader fs, int version)
		{
			inCount = fs.ReadUInt32();
			outCount = fs.ReadUInt32();
			betCount = fs.ReadByte();
			creditShow = fs.ReadByte();
			payoutShow = fs.ReadByte();
			isBetLatched = fs.ReadBoolean();
			isReplay = fs.ReadBoolean();
			gameMode = fs.ReadByte();
			modeGameCount = fs.ReadByte();
			modeJacCount = fs.ReadByte();
			modeMedalCount = fs.ReadInt32();
			RTMode = fs.ReadByte();
			RTOverride = fs.ReadBoolean();
			RTGameCount = fs.ReadInt32();
			bonusFlag = fs.ReadByte();
			castFlag = fs.ReadByte();
			castLines.Import(fs.ReadByte());
			castBonusID = fs.ReadInt32();
			if (version >= 1)
			{
				slotSetting = fs.ReadByte();
				setRandom = fs.ReadBoolean();
			}
			return true;
		}

		/// <summary>
		/// BETを1追加し、クレジットから減算します。リプレイ時は減算しない。
		/// </summary>
		public void AddBetCount()
		{
			++betCount;
			if (!isReplay) creditShow = (byte)Math.Max(0, creditShow - 1);
			payoutShow = 0;
			isBetLatched = false;
		}

		/// <summary>
		/// BETをクリアし、未消化時はクレジットに戻します。
		/// </summary>
		public void ClearBetCount()
		{
			if (!isBetLatched) creditShow = (byte)Math.Min(creditShow + betCount, CREDIT_MAX);
			betCount = 0;
		}

		/// <summary>
		/// リール始動時にBETを消化し、IN/OUTを更新します。
		/// </summary>
		public void LatchBet()
		{
			isBetLatched = true;                // BETを消化済みにする
			inCount += betCount;                // INメダル枚数を加算する
			if (isReplay) outCount += betCount; // リプレイの場合、OUTメダル枚数も加算する
		}

		/// <summary>
		/// フラグを設定し、必要に応じてモードとRTを遷移させます。
		/// </summary>
		/// <param name="pBonusFlag">ボーナスフラグ</param>
		/// <param name="pCastFlag">子役フラグ</param>
		/// <param name="cc">CastCommonData の参照</param>
		/// <param name="rtc">RTCommonData の参照</param>
		/// <param name="tm">SlotTimerManager の参照</param>
		public void SetCastFlag(byte pBonusFlag, byte pCastFlag, LocalDataSet.CastCommonData cc, LocalDataSet.RTCommonData rtc, SlotTimerManager tm)
		{
			if (bonusFlag == 0 && pBonusFlag > 0)
			{
				// ボーナス初当たり時はRT遷移を確認する
				bonusFlag = pBonusFlag;
				uint hitRT = rtc.MoveByBonus.GetData((uint)(pBonusFlag - 1));
				if (hitRT < LocalDataSet.RTMODE_MAX) SetRT((byte)hitRT, false, false, cc, rtc, tm);
			}
			castFlag = pCastFlag;
			// if (gameMode == 0) bonusFlag = 4;
			// Debug.Log("FlagSet: bonus->" + bonusFlag.ToString() + " cast->" + castFlag.ToString());
		}

		/// <summary>
		/// 配当を設定し、払い出し枚数を計算します。
		/// </summary>
		/// <param name="castResult">GetCastResult の参照</param>
		/// <param name="cc">CastCommonData の参照</param>
		/// <param name="hm">HistoryManager の参照</param>
		/// <param name="rd">リールデータ一覧</param>
		/// <param name="vm">SlotValManager の参照</param>
		/// <returns>リプレイ時は -1、それ以外は払い出し枚数</returns>
		public int SetPayout(MainReelManager.GetCastResult castResult, LocalDataSet.CastCommonData cc, HistoryManager hm, List<ReelBasicData> rd, SlotValManager vm)
		{
			// データリセット
			castLines.Import(0);
			castBonusID = 0;
			isReplay = false;
			int payCount = 0;

			// 出目履歴/成立時出目登録
			hm.LatchHist(this, rd, vm);

			for (int castC = 0; castC < castResult.payLine.Count; ++castC)
			{
				// 入賞ラインと配当名を読み込む
				castLines.SetData((uint)castResult.payLine[castC], 1);
				int bid = castResult.matchCast[castC].ValidateBonusFlag;
				if (bid > 0) castBonusID += bid;

				// 配当読込(リプレイ・小役判定)
				uint payoutID = castResult.matchCast[castC].PayoutNumID.GetData((uint)(betCount - 1));
				if (payoutID > LocalDataSet.CastCommonData.PAYOUT_DATA_MAX)
					isReplay = true;
				else if (payoutID > 0)
					payCount += (int)cc.PayoutData.GetData((uint)(payoutID - 1));
			}

			// 払出枚数の丸め処理
			int maxPay = (int)cc.MaxPayout.GetData((uint)(betCount - 1));
			payCount = Math.Min(maxPay, payCount);
			// モード/RTゲーム数減算
			if (RTGameCount > 0) --RTGameCount;
			if (modeGameCount > 0) --modeGameCount;
			if (modeJacCount > 0 && payCount > 0 && !isReplay) --modeJacCount;
			// replayなら-1、それ以外なら配当枚数を返す
			return isReplay ? -1 : payCount;
		}

		/// <summary>
		/// テンパイ状態を確認し、ボーナスフラグを設定します。
		/// </summary>
		/// <param name="castResult">GetCastResult の参照</param>
		/// <param name="cc">CastCommonData の参照</param>
		public void CheckTenpai(MainReelManager.GetCastResult castResult, LocalDataSet.CastCommonData cc)
		{
			// データリセット
			castBonusID = 0;
			for (int castC = 0; castC < castResult.payLine.Count; ++castC)
			{
				// テンパイ状況を読み込む
				int bid = castResult.matchCast[castC].ValidateBonusFlag;
				if (bid > castBonusID) castBonusID = bid;
			}
		}

		/// <summary>
		/// 払い出しを1増加させ、クレジットとモード残数を更新します。
		/// </summary>
		public void AddPayout()
		{
			++outCount;
			++payoutShow;
			if (modeMedalCount > 0) --modeMedalCount;
			if (modeMedalCount <= 0) modeMedalCount = -1;
			creditShow = (byte)Math.Min(creditShow + 1, CREDIT_MAX);
		}

		/// <summary>
		/// キャスト結果に応じてモードとRTを更新します。
		/// </summary>
		/// <param name="castResult">GetCastResult の参照</param>
		/// <param name="cc">CastCommonData の参照</param>
		/// <param name="rtc">RTCommonData の参照</param>
		/// <param name="rmList">RT移動設定一覧</param>
		/// <param name="tm">SlotTimerManager の参照</param>
		/// <param name="hm">HistoryManager の参照</param>
		/// <param name="cl">CollectionLogger の参照</param>
		/// <param name="vm">SlotValManager の参照</param>
		public void ModeChange(MainReelManager.GetCastResult castResult, LocalDataSet.CastCommonData cc, LocalDataSet.RTCommonData rtc, List<LocalDataSet.RTMoveData> rmList, SlotTimerManager tm, HistoryManager hm, CollectionLogger cl, SlotValManager vm)
		{
			for (int castC = 0; castC < castResult.matchCast.Count; ++castC)
			{
				var checkData = castResult.matchCast[castC];
				// モード更新とこれに伴うRT更新
				if (checkData.ChangeGameModeFlag)
				{
					hm.StartBonus(this, vm);    // ボーナス履歴更新
					cl.ClearLatch();           // コレクションLatchクリア
					bonusFlag = 0;
					SetMode(checkData.ChangeGameModeDest, checkData.BonusPayoutMaxID, checkData.BonusGameMaxID, cc, rtc, rmList, tm);
				}
				// 入賞に伴うRT更新
				if (RTOverride && checkData.ChangeRTFlag)
					SetRT(checkData.ChangeRTDest, checkData.CanOverwriteRT, false, cc, rtc, tm);
			}
		}

		/// <summary>
		/// モードおよびRTをリセットします。
		/// </summary>
		/// <param name="cc">CastCommonData の参照</param>
		/// <param name="rtc">RTCommonData の参照</param>
		/// <param name="rmList">RT移動設定一覧</param>
		/// <param name="tm">SlotTimerManager の参照</param>
		/// <param name="nowPayout":現在の払い出し枚数</param>
		/// <param name="hm":HistoryManager の参照</param>
		public void ModeReset(LocalDataSet.CastCommonData cc, LocalDataSet.RTCommonData rtc, List<LocalDataSet.RTMoveData> rmList, SlotTimerManager tm, int nowPayout, HistoryManager hm)
		{
			if (gameMode != 0)
			{
				// モードのリセット: 払出残数=0または残ゲーム数=0orJAC数=0
				if (modeMedalCount >= 0 && modeMedalCount - nowPayout <= 0)
				{
					// ボーナス履歴更新
					hm.FinishBonus(this, nowPayout);
					SetMode(0, 0, 0, cc, rtc, rmList, tm);
				}
				if (modeMedalCount < 0 && (modeGameCount <= 0 || modeJacCount <= 0))
				{
					// ボーナス履歴更新
					hm.FinishBonus(this, nowPayout);
					SetMode(0, 0, 0, cc, rtc, rmList, tm);
				}
			}
			if (RTMode != 0 && RTGameCount == 0)
				// RTのリセット: 残ゲーム数=0
				SetRT(0, true, true, cc, rtc, tm);
		}

		/// <summary>
		/// スロット設定を変更します。
		/// </summary>
		/// <param name="val">設定値</param>
		/// <param name="isRandom">ランダム設定フラグ</param>
		public void ChangeSlotSetting(byte val, bool isRandom)
		{
			if (val >= LocalDataSet.SETTING_MAX) return;
			slotSetting = val;
			setRandom = isRandom;
		}

		/// <summary>
		/// モード移行を内部で処理します。
		/// </summary>
		/// <param name="ModeDest">遷移先モード</param>
		/// <param name="payIndex">配当インデックス</param>
		/// <param name="gameIndex">ゲームインデックス</param>
		/// <param name="cc">CastCommonData の参照</param>
		/// <param name="rtc">RTCommonData の参照</param>
		/// <param name="rmList">RT移動設定一覧</param>
		/// <param name="tm">SlotTimerManager の参照</param>
		private void SetMode(byte ModeDest, byte payIndex, byte gameIndex, LocalDataSet.CastCommonData cc, LocalDataSet.RTCommonData rtc, List<LocalDataSet.RTMoveData> rmList, SlotTimerManager tm)
		{
			byte lastMode = gameMode;
			gameMode = ModeDest;
			if (gameIndex == 0)
			{
				if (payIndex > 0)
				{
					int valMedal = (int)cc.BonusPayData.GetData((uint)(payIndex - 1));
					if (valMedal > 0) modeMedalCount = valMedal;
				}
				modeGameCount = 0;
				modeJacCount = 0;
			}
			else
			{
				modeGameCount = (byte)cc.GameNumData.GetData((uint)(gameIndex - 1));
				modeJacCount = payIndex == 0 ? modeGameCount : (byte)cc.BonusPayData.GetData((uint)(payIndex - 1));
			}
			tm.GetTimer("changeMode").Activate();
			tm.GetTimer("changeMode").Reset();
			foreach (var item in rmList)
			{
				if (item.ModeSrc != lastMode || item.ModeDst != gameMode) continue;
				SetRT(item.Destination, item.CanOverride, true, cc, rtc, tm);
				break;
			}
		}

		/// <summary>
		/// RT移行を内部で処理します。
		/// </summary>
		/// <param name="RTDest">遷移先RT</param>
		/// <param name="ovrDef">デフォルト上書き可否フラグ</param>
		/// <param name="ovrGame">ゲーム中上書きフラグ</param>
		/// <param name="cc">CastCommonData の参照</param>
		/// <param name="rtc">RTCommonData の参照</param>
		/// <param name="tm">SlotTimerManager の参照</param>
		private void SetRT(byte RTDest, bool ovrDef, bool ovrGame, LocalDataSet.CastCommonData cc, LocalDataSet.RTCommonData rtc, SlotTimerManager tm)
		{
			RTMode = RTDest;
			RTOverride = ovrDef;
			uint gameIdx = rtc.ContGameNum.GetData((uint)RTDest);
			RTGameCount = gameIdx == 0 ? -1 : (int)cc.GameNumData.GetData(gameIdx - 1u);
			RTOverride &= ovrGame;
			tm.GetTimer("changeRT").Activate();
			tm.GetTimer("changeRT").Reset();
		}
	}
}
