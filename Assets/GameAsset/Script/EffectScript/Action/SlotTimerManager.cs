using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace SlotEffectMaker2023.Action
{
	/// <summary>
	/// 単一のタイマーを管理するクラス。
	/// 名前、経過時間、状態制御を保持します。
	/// </summary>
	public class SlotTimer
	{
		// タイマ制御データ(Sav) このクラスは保存対象としない
		public string timerName { get; private set; }   // タイマーの名前、呼び出し時の識別子になる
		public float? elapsedTime { get; private set; } // 経過時間、Time.deltaTimeの積算で表現する。無効時:null
		public float? lastElapsed { get; private set; } // 前回経過時間。無効時:null
		public bool isActivate { get; private set; }    // このタイマーが有効か
		public bool isPaused { get; private set; }      // このタイマーを一時停止しているか
		public float resumeTime { get; private set; }   // このタイマーの再開時間

		private bool isStoreFlag;   // このタイマーの作動状況を保存するか

		/// <summary>
		/// コンストラクタ。タイマー名と保存フラグを設定します。
		/// </summary>
		/// <param name="pTimerName">タイマーの識別名</param>
		/// <param name="pStoreActivate">稼働状況を保存するかどうか</param>
		public SlotTimer(string pTimerName, bool pStoreActivate)
		{
			// タイマを新規に作成するときのコンストラクタ: タイマ名を指定して新規作成する。
			// 作成時に作動状況を保存するか選択する(pStoreActivate)
			// 呼び出し前にタイマ名が重複しないことを確認すること
			timerName = pTimerName;
			elapsedTime = null;
			lastElapsed = null;
			isActivate = false;
			isPaused = false;
			isStoreFlag = pStoreActivate;
			resumeTime = 0f;
		}

		/// <summary>
		/// タイマーを有効化してカウントを開始します。
		/// </summary>
		/// <param name="offset">開始時のオフセット時間</param>
		public void Activate(float offset)
		{
			if (isActivate) return;
			isActivate = true;
			Reset(offset);
		}

		/// <summary>
		/// タイマーを有効化してカウントを開始します（オフセットなし）。
		/// </summary>
		public void Activate() { Activate(0f); }

		/// <summary>
		/// タイマーの経過時間をリセットします。
		/// </summary>
		/// <param name="offset">リセット後のオフセット時間</param>
		public void Reset(float offset)
		{
			if (!isActivate) return;
			elapsedTime = 0f;
			if (offset > 0f) elapsedTime = offset;
			lastElapsed = float.MinValue;
		}

		/// <summary>
		/// タイマーの経過時間をリセットします（オフセットなし）。
		/// </summary>
		public void Reset() { Reset(0f); }

		/// <summary>
		/// タイマーの一時停止状態を設定します。
		/// </summary>
		/// <param name="pauseFlag">一時停止する場合は true</param>
		public void SetPaused(bool pauseFlag)
		{
			if (!isActivate) return;
			isPaused = pauseFlag;
		}

		/// <summary>
		/// タイマーを無効化します。
		/// </summary>
		public void SetDisabled()
		{
			isActivate = false;
			isPaused = false;
			elapsedTime = null;
			lastElapsed = null;
		}

		/// <summary>
		/// タイマーを更新し、経過時間を積算します。
		/// </summary>
		/// <param name="deltaTime">前フレームからの経過時間</param>
		public void Update(float deltaTime)
		{
			if (!isActivate || isPaused) return;
			lastElapsed = elapsedTime;
			elapsedTime += deltaTime;
		}

		/// <summary>
		/// 経過時間が判定時間を超えたか取得します。
		/// </summary>
		/// <param name="judgeTime">判定基準時間</param>
		/// <param name="trigHold">トリガーホールドフラグ</param>
		/// <returns>条件を満たす場合は true</returns>
		public bool GetActionFlag(float judgeTime, bool trigHold)
		{
			if (!isActivate) return false;
			// 比較演算子を揃えることで2回Trueになることがないようにする
			return elapsedTime >= judgeTime && (!(lastElapsed >= judgeTime) || trigHold);
		}

		/// <summary>
		/// タイマー保存時に保存フラグが有効かを確認します。
		/// </summary>
		/// <returns>保存対象なら true</returns>
		public bool GetStoreFlag()
		{
			return isStoreFlag && isActivate;
		}

		/// <summary>
		/// 再開時間を設定します。
		/// </summary>
		/// <param name="pResumeTime">再開時の経過時間オフセット</param>
		public void SetResumeTimeOnReload(float pResumeTime)
		{
			resumeTime = pResumeTime;
		}
	}

	/// <summary>
	/// 複数の SlotTimer を管理し、保存・読込を行うクラス。
	/// </summary>
	public class SlotTimerManager : SlotMaker2022.ILocalDataInterface
	{
		// タイマ管理クラス(Sav)
		// タイマ一覧データ
		Data.TimerList timerList;
		// ゲーム上タイマデータ
		public List<SlotTimer> timerData { get; set; }
		// Resume時再起動データ
		private List<string> resTimerName;
		private List<float> resTimerOffset;

		/// <summary>
		/// コンストラクタ。内部リストを初期化します。
		/// </summary>
		public SlotTimerManager()
		{
			timerData = new List<SlotTimer>();
			resTimerName = new List<string>();
			resTimerOffset = new List<float>();
		}

		/// <summary>
		/// タイマーリストを受け取り、タイマーを初期化します。
		/// 読み込んだ再開データを有効化します。
		/// </summary>
		/// <param name="pList">システム定義およびユーザー定義タイマーリスト</param>
		public void Init(Data.TimerList pList)
		{
			// リストをインポートしてタイマを作成する
			timerList = pList;
			foreach (var data in timerList.SysTimer)
				CreateTimer(data.UserTimerName, data.StoreActivation);
			foreach (var data in timerList.UserData)
				CreateTimer(data.UserTimerName, data.StoreActivation);
			var soundPlay = Singleton.EffectDataManagerSingleton.GetInstance().SoundPlayList;
			foreach (var item in soundPlay)
			{
				CreateTimer(item.GetShotTimerName(), false);
				CreateTimer(item.GetLoopTimerName(), true);
			}
			// タイマのResumeを行う
			int dataNum = resTimerName.Count;
			for (int i = 0; i < dataNum; ++i)
				GetTimer(resTimerName[i])?.Activate(resTimerOffset[i]);
			// generalのみ無条件でActivateする
			GetTimer("general")?.Activate();
		}

		/// <summary>
		/// 保存対象のタイマー名とオフセットを抽出し、バイナリ形式で書き込みます。
		/// </summary>
		/// <param name="fs">BinaryWriter の参照</param>
		/// <param name="version">保存バージョン</param>
		/// <returns>保存処理が成功したか（常に true）</returns>
		public bool StoreData(ref BinaryWriter fs, int version)
		{
			resTimerName.Clear();
			resTimerOffset.Clear();
			// 保存するデータを選別する
			foreach (var item in timerData)
			{
				if (!item.GetStoreFlag()) continue;
				resTimerName.Add(item.timerName);
				resTimerOffset.Add(item.resumeTime);
			}
			int dataNum = resTimerName.Count;
			fs.Write(dataNum);
			for (int i = 0; i < dataNum; ++i)
			{
				fs.Write(resTimerName[i]);
				fs.Write(resTimerOffset[i]);
			}
			return true;
		}

		/// <summary>
		/// 保存データから再開用のタイマー名とオフセットを読み込みます。
		/// </summary>
		/// <param name="fs">BinaryReader の参照</param>
		/// <param name="version">保存バージョン</param>
		/// <returns>読み込み処理が成功したか（常に true）</returns>
		public bool ReadData(ref BinaryReader fs, int version)
		{
			resTimerName.Clear();
			resTimerOffset.Clear();
			int dataNum = fs.ReadInt32();
			for (int i = 0; i < dataNum; ++i)
			{
				resTimerName.Add(fs.ReadString());
				resTimerOffset.Add(fs.ReadSingle());
			}
			return true;
		}

		/// <summary>
		/// 重複しない名前で新規タイマーを作成します。
		/// </summary>
		/// <param name="pTimerName">タイマー名</param>
		/// <param name="pStoreActivate">保存フラグ</param>
		/// <returns>作成に成功したか</returns>
		public bool CreateTimer(string pTimerName, bool pStoreActivate)
		{
			for (int i = 0; i < timerData.Count; ++i)
			{
				if (timerData[i].timerName == pTimerName) return false;
			}
			timerData.Add(new SlotTimer(pTimerName, pStoreActivate));
			return true;
		}

		/// <summary>
		/// 指定のタイマーを取得します。
		/// </summary>
		/// <param name="pTimerName">タイマー名</param>
		/// <returns>SlotTimer インスタンス、存在しない場合は null</returns>
		public SlotTimer GetTimer(string pTimerName)
		{
			for (int i = 0; i < timerData.Count; ++i)
			{
				if (timerData[i].timerName == pTimerName) return timerData[i];
			}
			return null;
		}

		/// <summary>
		/// 全タイマーを更新し、経過時間を積算します。
		/// </summary>
		/// <param name="deltaTime">前フレームからの経過時間</param>
		public void Process(float deltaTime)
		{
			for (int i = 0; i < timerData.Count; ++i) timerData[i].Update(deltaTime);
		}
	}
}
