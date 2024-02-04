using System.Collections;
using System.Collections.Generic;

namespace SlotEffectMaker2023.Action
{
	public class SlotTimer
	{	// タイマ制御データ(Sav)
		public string timerName { get; private set; }   // タイマーの名前、呼び出し時の識別子になる
		public float? elapsedTime { get; private set; } // 経過時間、Time.deltaTimeの積算で表現する。無効時:null
		public float? lastElapsed { get; private set; } // 前回経過時間。無効時:null
		public bool isActivate { get; private set; }    // このタイマーが有効か
		public bool isPaused { get; private set; }  // このタイマーを一時停止しているか

		private bool isStoreFlag;   // このタイマーの作動状況を保存するか

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
		}
		// 処理系関数
		// タイマを有効にしてカウントを開始する。有効化済みの場合は何もしない
		public void Activate(float offset)
		{
			if (isActivate) return;
			isActivate = true;
			Reset(offset);
		}
		public void Activate() { Activate(0f); }

		// タイマの経過時間をリセットする
		public void Reset(float offset)
		{
			if (!isActivate) return;
			elapsedTime = 0f;
			if (offset > 0f) elapsedTime = offset;
			lastElapsed = float.MinValue;
		}
		public void Reset() { Reset(0f); }

		// カウントを一時中断するか指定する
		public void SetPaused(bool pauseFlag)
		{
			if (!isActivate) return;
			isPaused = pauseFlag;
		}

		// タイマーを無効にする
		public void SetDisabled()
		{
			isActivate = false;
			isPaused = false;
			elapsedTime = null;
			lastElapsed = null;
		}

		// タイマを更新する
		public void Update(float deltaTime)
		{
			if (!isActivate || isPaused) return;
			lastElapsed = elapsedTime;
			elapsedTime += deltaTime;
		}

		// タイマの経過判定結果を取得する
		public bool GetActionFlag(float judgeTime, bool trigHold)
        {
			if (!isActivate) return false;
			// 比較演算子を揃えることで2回Trueになることがないようにする
			return elapsedTime >= judgeTime && (!(lastElapsed >= judgeTime) || trigHold);
        }

		// タイマの保存条件「タイマが稼働しており、保存フラグが有効か」を確認する
		public bool GetStoreFlag()
		{
			return isStoreFlag && isActivate;
		}
	}

	public class SlotTimerManager
	{	// タイマ管理クラス(Sav)
		// タイマ一覧データ
		Data.TimerList timerList;
		// ゲーム上タイマデータ
		public List<SlotTimer> timerData { get; set; }

		/// <summary>
		/// インスタンスの初期化を行います。
		/// timerDataの読み込みをpListから行います
		/// </summary>
		public SlotTimerManager()
		{
			timerData = new List<SlotTimer>();
		}
		// タイマの初期化を行う
		public void Init(Data.TimerList pList)
        {	// リストをインポートしてタイマを作成する
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
			// generalのみ無条件でActivateする
			GetTimer("general")?.Activate();
		}
		// 前回終了時に有効だったタイマをActivateする(セーブデータ)
		public bool ReadData()
		{
			return true;
		}

		// 名前に重複がないことを確認してタイマを新規作成する。
		// [ret]タイマを追加したか
		public bool CreateTimer(string pTimerName, bool pStoreActivate)
		{
			for (int i = 0; i < timerData.Count; ++i)
			{
				if (timerData[i].timerName == pTimerName) return false;
			}
			timerData.Add(new SlotTimer(pTimerName, pStoreActivate));
			return true;
		}
		// 名前に一致したタイマを取得する
		// [ret]タイマのインスタンス, 見つからない場合はnull
		public SlotTimer GetTimer(string pTimerName)
		{
			for (int i = 0; i < timerData.Count; ++i)
			{
				if (timerData[i].timerName == pTimerName) return timerData[i];
			}
			return null;
		}
		// タイマの加算処理を行う
		public void Process(float deltaTime)
		{
			for (int i = 0; i < timerData.Count; ++i) timerData[i].Update(deltaTime);
		}
	}
}