using System.Collections;
using System.Collections.Generic;

namespace SlotEffectMaker2023.Singleton {
	public sealed class SlotDataSingleton
	{   // スロット上で動くデータを定義する
		// ファイルバージョン
		const int FILE_VERSION = 0;
		const int FILE_VERSION_SYS = 0;

		public List<Action.ReelBasicData>	reelData  { get; set; }
		public Action.SlotBasicData			basicData { get; set; }
		public Action.SlotTimerManager		timerData { get; set; }
		// エフェクト用変数
		public Action.SlotValManager		valManager { get; set; }
		// 音源データ
		public Action.DataShifterManager<Data.SoundPlayData>	soundData { get; set; }
		// カラーマップデータ
		public Action.DataShifterManager<Data.ColorMapShifter>	colorMapData { get; set; }
		// フリーズ関連データ
		public Action.FreezeManager			freezeManager { get; set; }
		// 履歴関連データ
		public Action.HistoryManager		historyManager { get; set; }
		public Action.CollectionLogger		collectionManager { get; set; }
		// システムデータ
		public Action.SystemData			sysData { get; set; }
	
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
			soundData = new Action.DataShifterManager<Data.SoundPlayData>();
			colorMapData = new Action.DataShifterManager<Data.ColorMapShifter>();
			freezeManager = new Action.FreezeManager();
			historyManager = new Action.HistoryManager();
			collectionManager = new Action.CollectionLogger();
			sysData = new Action.SystemData();
		}

		public void Init(List<Data.SoundPlayData> pSoundPlayData, Data.TimerList pTimer, Data.VarList pVar, List<Data.ColorMapShifter> pMapPlayData, Data.CollectionData pColle)
        {   // 各データへの初期値設定を行う
			timerData.Init(pTimer);
			valManager.Init(pVar);
			soundData.Init(pSoundPlayData);
			colorMapData.Init(pMapPlayData);
			historyManager.Init();
			collectionManager.Init(pColle);
        }
	
		/// <summary>
		/// インスタンスの取得を行います。
		/// </summary>
		public static SlotDataSingleton GetInstance() { return ins; }
	
		public bool ReadData(string pPath)
		{   // Unity用
			var rd = new SlotMaker2022.ProgressRead();
			bool ans = true;

			if (rd.OpenFile(pPath))
			{
				if (!ReadAction(rd)) return false;
				rd.Close();
			}
			else 
			{
				ans = false;
			}

			// データが読み込めなかった場合にリール情報を新規生成する
			if (reelData.Count == 0){
				for (int i=0; i<SlotMaker2022.LocalDataSet.REEL_MAX; ++i)
					reelData.Add(new Action.ReelBasicData(12));
			}
			return ans;
		}

		// データ読込本体
		private bool ReadAction(SlotMaker2022.ProgressRead rd)
        {
			if (!rd.ReadData(timerData)) return false;
			if (!rd.ReadData(reelData)) return false;
			if (!rd.ReadData(basicData)) return false;
			if (!rd.ReadData(valManager)) return false;
			if (!rd.ReadData(soundData)) return false;
			if (!rd.ReadData(colorMapData)) return false;
			if (!rd.ReadData(freezeManager)) return false;
			if (!rd.ReadData(historyManager)) return false;
			if (!rd.ReadData(collectionManager)) return false;
			return true;
		}
		public bool SaveData(string pPath)
		{
			var sw = new SlotMaker2022.ProgressWrite();
			if (sw.OpenFile(pPath, FILE_VERSION))
			{
				WriteOut(sw);
				sw.Flush();
				sw.Close();
			}
			return true;
		}
		private bool WriteOut(SlotMaker2022.ProgressWrite sw)
        {
			sw.WriteData(timerData);
			sw.WriteData(reelData);
			sw.WriteData(basicData);
			sw.WriteData(valManager);
			sw.WriteData(soundData);
			sw.WriteData(colorMapData);
			sw.WriteData(freezeManager);
			sw.WriteData(historyManager);
			sw.WriteData(collectionManager);
			return true;
		}

		/// <summary>
		/// システム変数を更新します。
		/// </summary>
		public void Process(){
			UpdateSysVar();

			// タイムラインを運用する
			var timeline = EffectDataManagerSingleton.GetInstance().Timeline.timerData;
			foreach (var item in timeline) item.Action();
		}

		public void UpdateSysVar()
        {
			valManager.GetVariable("_slotSetting")		.val = basicData.slotSetting;
			valManager.GetVariable("_inCount")			.val = (int)basicData.inCount;
			valManager.GetVariable("_outCount")			.val = (int)basicData.outCount;
			valManager.GetVariable("_betCount")			.val = basicData.betCount;
			valManager.GetVariable("_creditCount")		.val = basicData.creditShow;
			valManager.GetVariable("_payoutCount")		.val = basicData.payoutShow;
			valManager.GetVariable("_isBetLatched")		.SetBool(basicData.isBetLatched);
			valManager.GetVariable("_isReplay")			.SetBool(basicData.isReplay);
			valManager.GetVariable("_gameMode")			.val = basicData.gameMode;
			valManager.GetVariable("_modeGameCount")	.val = basicData.modeGameCount;
			valManager.GetVariable("_modeJacCount")		.val = basicData.modeJacCount;
			valManager.GetVariable("_modeMedalCount")	.val = basicData.modeMedalCount;
			valManager.GetVariable("_RTMode")			.val = basicData.RTMode;
			valManager.GetVariable("_RTOverride")		.SetBool(basicData.RTOverride);
			valManager.GetVariable("_RTGameCount")		.val = basicData.RTGameCount;
			valManager.GetVariable("_flagID")			.val = basicData.castFlag;
			valManager.GetVariable("_bonusID")			.val = basicData.bonusFlag;
			valManager.GetVariable("_castBonusID")		.val = basicData.castBonusID;
			valManager.GetVariable("_payLine")			.val = decimal.ToInt32(basicData.castLines.Export());
			valManager.GetVariable("_unlockColleNum")	.val = collectionManager.GetAchievedCount();

			for (int i = 0; i < SlotMaker2022.LocalDataSet.REEL_MAX; ++i)
            {
				valManager.GetVariable("_reelPushPos[" + i + "]").val = reelData[i].pushPos;
				valManager.GetVariable("_reelStopPos[" + i + "]").val = reelData[i].stopPos;
				valManager.GetVariable("_reelStopOrder[" + i + "]").val = reelData[i].stopOrder;
            }
			var colleData = EffectDataManagerSingleton.GetInstance().Collection;
			for (int i = 0; i < Data.CollectionDataElem.COLLECTION_LEVEL_MAX; ++i)
				valManager.GetVariable("_unlockColleNumLv[" + i + "]").val = collectionManager.GetAchievedCount(colleData, i+1);

			// ボーナス回数を更新する
			historyManager.Process(valManager);
        }

		// ゲームデータと独立してシステムデータを読み書きする
		public bool ReadSysData(string pPath)
		{   // Unity用
			var rd = new SlotMaker2022.ProgressRead();
			bool ans = true;

			if (rd.OpenFile(pPath))
			{
				if (!rd.ReadData(sysData)) return false;
				rd.Close();
			}
			else
			{
				ans = false;
			}

			return ans;
		}
		public bool SaveSysData(string pPath)
		{
			var sw = new SlotMaker2022.ProgressWrite();
			if (sw.OpenFile(pPath, FILE_VERSION_SYS))
			{
				sw.WriteData(sysData);
				sw.Flush();
				sw.Close();
			}
			return true;
		}
	}
}