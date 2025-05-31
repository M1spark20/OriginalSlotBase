using System.Collections;
using System.Collections.Generic;

namespace SlotEffectMaker2023.Singleton
{
	// スロット上で動くデータを定義する
	/// <summary>
	/// ゲーム動作時に共有するデータを管理するシングルトン。
	/// 各種演出やシステム変数、タイマー、リール状態などを保持します。
	/// </summary>
	public sealed class SlotDataSingleton
	{
		// ファイルバージョン
		/// <summary>データ読み書き用ファイルのバージョン。</summary>
		public const int FILE_VERSION = 1;      // v1:20241020
		/// <summary>システムデータ読み書き用ファイルのバージョン。</summary>
		public const int FILE_VERSION_SYS = 3;  // v3:20241014

		/// <summary>リールの基本データリスト。</summary>
		public List<Action.ReelBasicData> reelData { get; set; }
		/// <summary>スロットの基本データ。</summary>
		public Action.SlotBasicData basicData { get; set; }
		/// <summary>スロット用タイマーマネージャー。</summary>
		public Action.SlotTimerManager timerData { get; set; }
		// エフェクト用変数
		/// <summary>エフェクト用の変数マネージャー。</summary>
		public Action.SlotValManager valManager { get; set; }
		// 音源データ
		/// <summary>サウンド再生データのシフターマネージャー。</summary>
		public Action.DataShifterManager<Data.SoundPlayData> soundData { get; set; }
		// カラーマップデータ
		/// <summary>カラーマップのシフターマネージャー。</summary>
		public Action.DataShifterManager<Data.ColorMapShifter> colorMapData { get; set; }
		// フリーズ関連データ
		/// <summary>フリーズ演出管理マネージャー。</summary>
		public Action.FreezeManager freezeManager { get; set; }
		// 履歴関連データ
		/// <summary>ヒストリーマネージャー。</summary>
		public Action.HistoryManager historyManager { get; set; }
		/// <summary>コレクションログ管理マネージャー。</summary>
		public Action.CollectionLogger collectionManager { get; set; }
		// システムデータ
		/// <summary>システムデータ。</summary>
		public Action.SystemData sysData { get; set; }

		// Singletonインスタンス
		private static SlotDataSingleton ins = new SlotDataSingleton();

		/// <summary>
		/// プライベートコンストラクタ。インスタンスを初期化します。
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

		/// <summary>
		/// 各種データに初期値を設定します。
		/// </summary>
		/// <param name="pSoundPlayData">初期化用サウンド再生データリスト。</param>
		/// <param name="pTimer">初期化用タイマーリスト。</param>
		/// <param name="pVar">初期化用変数リスト。</param>
		/// <param name="pMapPlayData">初期化用カラーマップシフターデータ。</param>
		/// <param name="pColle">初期化用コレクションデータ。</param>
		public void Init(List<Data.SoundPlayData> pSoundPlayData, Data.TimerList pTimer, Data.VarList pVar, List<Data.ColorMapShifter> pMapPlayData, Data.CollectionData pColle)
		{   // 各データへの初期値設定を行う
			// データが読み込めなかった場合にリール情報を新規生成する
			if (reelData.Count == 0)
			{
				for (int i = 0; i < SlotMaker2022.LocalDataSet.REEL_MAX; ++i)
					reelData.Add(new Action.ReelBasicData(12));
			}
			timerData.Init(pTimer);
			valManager.Init(pVar);
			soundData.Init(pSoundPlayData);
			colorMapData.Init(pMapPlayData);
			historyManager.Init();
			collectionManager.Init(pColle);
		}

		/// <summary>
		/// シングルトンインスタンスを取得します。
		/// </summary>
		/// <returns>唯一の SlotDataSingleton インスタンス。</returns>
		public static SlotDataSingleton GetInstance() { return ins; }

		/// <summary>
		/// データファイルを読み込みます。
		/// </summary>
		/// <param name="pPath">読み込むファイルパス。</param>
		/// <returns>読み込みに成功した場合は true。</returns>
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

			return ans;
		}

		// データ読込本体
		/// <summary>
		/// 各データセットの読み込み処理を行います。
		/// </summary>
		/// <param name="rd">読み込み用 ProgressRead インスタンス。</param>
		/// <returns>成功した場合は true。</returns>
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
			if (rd.FileVersion == 0)
			{
				// SysDataで実績がある場合は二重読込を行わない
				if (collectionManager.Achievements.Count == 0)
				{
					if (!rd.ReadData(collectionManager)) return false;
				}
				else
				{   // ダミーにデータを流してバッファを進める
					var dum = new Action.CollectionLogger();
					if (!rd.ReadData(dum)) return false;
				}
			}
			return true;
		}

		/// <summary>
		/// データを指定ファイルへ書き出します。
		/// </summary>
		/// <param name="pPath">出力先ファイルパス。</param>
		/// <returns>常に true を返します。</returns>
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

		/// <summary>
		/// ProgressWrite を使用して各データセットを書き出します。
		/// </summary>
		/// <param name="sw">書き出し用 ProgressWrite インスタンス。</param>
		/// <returns>成功した場合は true。</returns>
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
			//sw.WriteData(collectionManager);
			return true;
		}

		// データをリセットする。Initの前に呼び出すこと。(20241014実装)
		/// <summary>
		/// システムデータを除き、全データを初期状態にリセットします。
		/// </summary>
		/// <param name="pBackupPath">バックアップファイルのパス。</param>
		public void ResetData(string pBackupPath)
		{
			// 削除条件確認
			if (!sysData.ResetFlag) return;
			// collectionManager, sysData以外を初期化
			timerData = new Action.SlotTimerManager();
			reelData = new List<Action.ReelBasicData>();
			basicData = new Action.SlotBasicData();
			valManager = new Action.SlotValManager();
			soundData = new Action.DataShifterManager<Data.SoundPlayData>();
			colorMapData = new Action.DataShifterManager<Data.ColorMapShifter>();
			freezeManager = new Action.FreezeManager();
			historyManager = new Action.HistoryManager();
			// resetFlagを倒す
			sysData.ResetFlag = false;
		}

		/// <summary>
		/// システム処理を一巡実行します。
		/// タイマーやタイムラインのアクションを処理します。
		/// </summary>
		public void Process()
		{
			UpdateSysVar();

			// タイムラインを運用する
			var timeline = EffectDataManagerSingleton.GetInstance().Timeline.timerData;
			foreach (var item in timeline) item.Action();
		}

		/// <summary>
		/// システム変数を最新の状態に更新します。
		/// </summary>
		public void UpdateSysVar()
		{
			valManager.GetVariable("_slotSetting").val = basicData.slotSetting;
			valManager.GetVariable("_inCount").val = (int)basicData.inCount;
			valManager.GetVariable("_outCount").val = (int)basicData.outCount;
			valManager.GetVariable("_betCount").val = basicData.betCount;
			valManager.GetVariable("_creditCount").val = basicData.creditShow;
			valManager.GetVariable("_payoutCount").val = basicData.payoutShow;
			valManager.GetVariable("_isBetLatched").SetBool(basicData.isBetLatched);
			valManager.GetVariable("_isReplay").SetBool(basicData.isReplay);
			valManager.GetVariable("_gameMode").val = basicData.gameMode;
			valManager.GetVariable("_modeGameCount").val = basicData.modeGameCount;
			valManager.GetVariable("_modeJacCount").val = basicData.modeJacCount;
			valManager.GetVariable("_modeMedalCount").val = basicData.modeMedalCount;
			valManager.GetVariable("_RTMode").val = basicData.RTMode;
			valManager.GetVariable("_RTOverride").SetBool(basicData.RTOverride);
			valManager.GetVariable("_RTGameCount").val = basicData.RTGameCount;
			valManager.GetVariable("_flagID").val = basicData.castFlag;
			valManager.GetVariable("_bonusID").val = basicData.bonusFlag;
			valManager.GetVariable("_castBonusID").val = basicData.castBonusID;
			valManager.GetVariable("_payLine").val = decimal.ToInt32(basicData.castLines.Export());
			valManager.GetVariable("_unlockColleNum").val = collectionManager.GetAchievedCount();

			for (int i = 0; i < SlotMaker2022.LocalDataSet.REEL_MAX; ++i)
			{
				valManager.GetVariable("_reelPushPos[" + i + "]").val = reelData[i].pushPos;
				valManager.GetVariable("_reelStopPos[" + i + "]").val = reelData[i].stopPos;
				valManager.GetVariable("_reelStopOrder[" + i + "]").val = reelData[i].stopOrder;
			}
			var colleData = EffectDataManagerSingleton.GetInstance().Collection;
			for (int i = 0; i < Data.CollectionDataElem.COLLECTION_LEVEL_MAX; ++i)
				valManager.GetVariable("_unlockColleNumLv[" + i + "]").val = collectionManager.GetAchievedCount(colleData, i + 1);

			// ボーナス回数を更新する
			historyManager.Process(valManager);
		}

		/// <summary>
		/// システム用データファイルを読み込みます。
		/// </summary>
		/// <param name="pPath">読み込むファイルパス。</param>
		/// <returns>読み込みに成功した場合は true。</returns>
		public bool ReadSysData(string pPath)
		{   // Unity用
			var rd = new SlotMaker2022.ProgressRead();
			bool ans = true;

			if (rd.OpenFile(pPath))
			{
				if (!rd.ReadData(sysData)) return false;
				if (rd.FileVersion >= 3)
				{
					if (!rd.ReadData(collectionManager)) return false;
				}
				rd.Close();
			}
			else
			{
				ans = false;
			}

			return ans;
		}

		/// <summary>
		/// システム用データをファイルに書き出します。
		/// </summary>
		/// <param name="pPath">出力先ファイルパス。</param>
		/// <returns>常に true を返します。</returns>
		public bool SaveSysData(string pPath)
		{
			var sw = new SlotMaker2022.ProgressWrite();
			if (sw.OpenFile(pPath, FILE_VERSION_SYS))
			{
				sw.WriteData(sysData);
				if (FILE_VERSION_SYS >= 3) sw.WriteData(collectionManager);
				sw.Flush();
				sw.Close();
			}
			return true;
		}
	}
}
