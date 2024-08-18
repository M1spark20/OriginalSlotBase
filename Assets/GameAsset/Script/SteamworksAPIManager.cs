using UnityEngine;
using System.Collections;
#if UNITY_ANDROID
#else
using Steamworks;
#endif

public class SteamworksAPIManager : MonoBehaviour {
#if UNITY_ANDROID
	public void OnGameStateChange() {}
#else
	// Our GameID
	private CGameID m_GameID;
	
	// Did we get the stats from Steam?
	private bool m_bRequestedStats;
	private bool m_bStatsValid;

	// Should we store stats this frame?
	private bool m_bStoreStats;
	
	// Callbacks
	protected Callback<UserStatsReceived_t> m_UserStatsReceived;
	protected Callback<UserStatsStored_t> m_UserStatsStored;
	protected Callback<UserAchievementStored_t> m_UserAchievementStored;
	
	// A-Rabbit's Singleton
	SlotEffectMaker2023.Singleton.SlotDataSingleton slotData;
	SlotEffectMaker2023.Singleton.EffectDataManagerSingleton subROM;
		
    void Start() {
        if(SteamManager.Initialized) {
            string name = SteamFriends.GetPersonaName();
            Debug.Log(name);
        }
    }
    
	void OnEnable() {
		if (!SteamManager.Initialized) return;

		// Get Singleton Instance
		slotData = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance();
		subROM   = SlotEffectMaker2023.Singleton.EffectDataManagerSingleton.GetInstance();
		
		// Cache the GameID for use in the Callbacks
		m_GameID = new CGameID(SteamUtils.GetAppID());

		m_UserStatsReceived = Callback<UserStatsReceived_t>.Create(OnUserStatsReceived);
		m_UserStatsStored = Callback<UserStatsStored_t>.Create(OnUserStatsStored);
		m_UserAchievementStored = Callback<UserAchievementStored_t>.Create(OnAchievementStored);

		// These need to be reset to get the stats upon an Assembly reload in the Editor.
		m_bRequestedStats = false;
		m_bStatsValid = false;
	}
	
	private void Update() {
		if (!SteamManager.Initialized) return;

		if (!m_bRequestedStats) {
			// Is Steam Loaded? if no, can't get stats, done
			if (!SteamManager.Initialized) {
				m_bRequestedStats = true;
				return;
			}
			
			// (debug)呼び出し前にリセットをかける
			SteamUserStats.ResetAllStats(true);
			// If yes, request our stats (この返答としてコールバックが呼ばれる?)
			bool bSuccess = SteamUserStats.RequestCurrentStats();

			// This function should only return false if we weren't logged in, and we already checked that.
			// But handle it being false again anyway, just ask again later.
			m_bRequestedStats = bSuccess;
		}

		if (!m_bStatsValid) return;

		//Store stats in the Steam database if necessary (Modified by OnGameStateChange())
		if (m_bStoreStats) {
			// Send to steam
			bool bSuccess = SteamUserStats.StoreStats();
			// If this failed, we never sent anything to the server, try
			// again later.
			m_bStoreStats = !bSuccess;
		}
	}

	//-----------------------------------------------------------------------------
	// Purpose: Unlock this achievement
	//-----------------------------------------------------------------------------
	private void UnlockAchievement(SlotEffectMaker2023.Data.GameAchievement ac) {
		ac.IsAchieved = true;

		// the icon may change once it's unlocked
		//achievement.m_iIconImage = 0;

		// mark it down
		SteamUserStats.SetAchievement(ac.DataID);

		// Store stats end of/next frame
		m_bStoreStats = true;
	}

	//-----------------------------------------------------------------------------
	// Purpose: Game state has changed
	//-----------------------------------------------------------------------------
	public void OnGameStateChange() {
		if (!m_bStatsValid) return;

		// Update Action
		var vm = slotData.valManager;
		var bs = slotData.basicData;
		
		foreach (var item in subROM.GameAchieve.elemData) {
			if (bs.gameMode == 0 && item.UpdateOnlyBonusIn) continue;
			if (item.Type == SlotEffectMaker2023.Data.AchieveDataType.Flag){
				if (item.IsAchieved) continue;
				// 変数条件比較: 新規達成OKなら実績通知
				var cond = subROM.Timeline.GetCondFromName(item.RefData);
				if (!cond.Evaluate()) continue;
				// 実績解除
				UnlockAchievement(item);
			}
			if (item.Type == SlotEffectMaker2023.Data.AchieveDataType.Num){
				var valData = vm.GetVariable(item.RefData);
				if (valData != null) SteamUserStats.SetStat(item.DataID, valData.val);
			}
		}

		// We want to update stats the next frame.
		m_bStoreStats = true;
	}
	
	//-----------------------------------------------------------------------------
	// Purpose: We have stats data from Steam. It is authoritative, so update
	//			our data with those results now.
	//-----------------------------------------------------------------------------
	private void OnUserStatsReceived(UserStatsReceived_t pCallback) {
		if (!SteamManager.Initialized)
			return;

		// we may get callbacks for other games' stats arriving, ignore them
		if ((ulong)m_GameID == pCallback.m_nGameID) {
			if (EResult.k_EResultOK == pCallback.m_eResult) {
				Debug.Log("Received stats and achievements from Steam\n");

				m_bStatsValid = true;

				// load achievements / stats (Flagのみ)
				foreach (var item in subROM.GameAchieve.elemData) {
					if (item.Type != SlotEffectMaker2023.Data.AchieveDataType.Flag) continue;
					// データを読み込む
					bool isAchieved;	// 当該実績が達成済みか
					bool ret = SteamUserStats.GetAchievement(item.DataID, out isAchieved);
					if (ret) {
						string Name = SteamUserStats.GetAchievementDisplayAttribute(item.DataID, "name");
						string Desc = SteamUserStats.GetAchievementDisplayAttribute(item.DataID, "desc");
						item.SetDetail(isAchieved, Name, Desc);
						Debug.Log("Stat Read: ID=" + item.DataID + ", Name=" + item.Title + ", Desc=" + item.Desc + ", Flag=" + item.IsAchieved.ToString());
					}
					else {
						Debug.LogWarning("SteamUserStats.GetAchievement failed for Achievement " + item.DataID + "\nIs it registered in the Steam Partner site?");
					}
				}
			}
			else {
				Debug.Log("RequestStats - failed, " + pCallback.m_eResult);
			}
		}
	}
	
	//-----------------------------------------------------------------------------
	// Purpose: Our stats data was stored!
	//-----------------------------------------------------------------------------
	private void OnUserStatsStored(UserStatsStored_t pCallback) {
		// we may get callbacks for other games' stats arriving, ignore them
		if ((ulong)m_GameID == pCallback.m_nGameID) {
			if (EResult.k_EResultOK == pCallback.m_eResult) {
				Debug.Log("StoreStats - success");
			}
			else if (EResult.k_EResultInvalidParam == pCallback.m_eResult) {
				// One or more stats we set broke a constraint. They've been reverted,
				// and we should re-iterate the values now to keep in sync.
				Debug.Log("StoreStats - some failed to validate");
				// Fake up a callback here so that we re-load the values.
				UserStatsReceived_t callback = new UserStatsReceived_t();
				callback.m_eResult = EResult.k_EResultOK;
				callback.m_nGameID = (ulong)m_GameID;
				OnUserStatsReceived(callback);
			}
			else {
				Debug.Log("StoreStats - failed, " + pCallback.m_eResult);
			}
		}
	}

	//-----------------------------------------------------------------------------
	// Purpose: An achievement was stored
	//-----------------------------------------------------------------------------
	private void OnAchievementStored(UserAchievementStored_t pCallback) {
		// We may get callbacks for other games' stats arriving, ignore them
		if ((ulong)m_GameID == pCallback.m_nGameID) {
			if (0 == pCallback.m_nMaxProgress) {
				Debug.Log("Achievement '" + pCallback.m_rgchAchievementName + "' unlocked!");
			}
			else {
				Debug.Log("Achievement '" + pCallback.m_rgchAchievementName + "' progress callback, (" + pCallback.m_nCurProgress + "," + pCallback.m_nMaxProgress + ")");
			}
		}
	}
#endif
}
