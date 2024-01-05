using System.Collections;
using System.Collections.Generic;

namespace SlotEffectMaker2023.Singleton {
	public sealed class SlotDataSingleton
	{	// スロット上で動くデータを定義する
		public List<Action.ReelBasicData>	reelData  { get; set; }
		public Action.SlotBasicData			basicData { get; set; }
		public Action.SlotTimerManager		timerData { get; set; }
		// エフェクト用変数
		public Action.SlotValManager		valManager { get; set; }
		// 音源データ
		public Action.SoundDataManager			soundData { get; set; }
	
		// Singletonインスタンス
		private static SlotDataSingleton ins = new SlotDataSingleton();
    
		/// <summary>
		/// インスタンスの初期化を行います。Singletonであるためprivateメンバです
		/// </summary>
		private SlotDataSingleton()
		{
			timerData = new Action.SlotTimerManager();
			reelData = new List<Action.ReelBasicData>();
			basicData = new Action.SlotBasicData();
			valManager = new Action.SlotValManager();
			soundData = new Action.SoundDataManager();
		}

		public void Init(List<Data.SoundPlayData> pPlayData, Data.TimerList pTimer, Data.VarList pVar)
        {   // 各データへの初期値設定を行う
			timerData.Init(pTimer);
			valManager.Init(pVar);
			soundData.Init(pPlayData);
        }
	
		/// <summary>
		/// インスタンスの取得を行います。
		/// </summary>
		public static SlotDataSingleton GetInstance() { return ins; }
	
		public bool ReadData(ref Data.TimerList timerList){
			// 読み込み処理
			// reelData
			// basicData
			valManager.ReadData();
		
			// データが読み込めなかった場合にリール情報を新規生成する
			if (reelData.Count == 0){
				for (int i=0; i<SlotMaker2022.LocalDataSet.REEL_MAX; ++i){
					reelData.Add(new Action.ReelBasicData(12));
				}
			}
		
			// soundData
		
			return true;
		}
	
		/// <summary>
		/// システム変数を更新します。
		/// </summary>
		public void Process(){
			valManager.GetVariable("_betCount")		.val = basicData.betCount;
			valManager.GetVariable("_creditCount")	.val = basicData.creditShow;
			valManager.GetVariable("_payoutCount")	.val = basicData.payoutShow;
			valManager.GetVariable("_isReplay")		.SetBool(basicData.isReplay);
			valManager.GetVariable("_flagID")		.val = basicData.castFlag;
			valManager.GetVariable("_bonusID")		.val = basicData.bonusFlag;

			// タイムラインを運用する
			var timeline = EffectDataManagerSingleton.GetInstance().Timeline.timerData;
			foreach (var item in timeline) item.Action();
		}
	}
}