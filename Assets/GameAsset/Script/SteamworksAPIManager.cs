using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_ANDROID
#else
using Steamworks;
#endif

/// <summary>
/// Steamと連携し、スロットゲームにおける実績および統計情報の取得・保存・更新を一括管理する。
/// Androidでは無効化される。
/// </summary>
public class SteamworksAPIManager : MonoBehaviour
{
#if UNITY_ANDROID
    /// <summary>
    /// AndroidではSteam連携は無効。
    /// </summary>
    public void OnGameStateChange() {}
#else
	// Steam Game ID
	private CGameID m_GameID;

	// 統計データ取得済みかどうかのフラグ
	private bool m_bRequestedStats;
	private bool m_bStatsValid;

	// 統計データを保存すべきかどうかのフラグ
	private bool m_bStoreStats;

	// Steamコールバック
	protected Callback<UserStatsReceived_t> m_UserStatsReceived;
	protected Callback<UserStatsStored_t> m_UserStatsStored;
	protected Callback<UserAchievementStored_t> m_UserAchievementStored;

	// スロットデータ管理シングルトン
	SlotEffectMaker2023.Singleton.SlotDataSingleton slotData;
	SlotEffectMaker2023.Singleton.EffectDataManagerSingleton subROM;

	// 実績の送信時に利用する補正値（変数との差分など）
	List<Tuple<string, int>> OffsetOverrides;

	/// <summary>
	/// 初期化処理：Steamが有効な場合はユーザー名をログ出力。
	/// </summary>
	void Start()
	{
		OffsetOverrides = new List<Tuple<string, int>>();
		if (SteamManager.Initialized)
		{
			string name = SteamFriends.GetPersonaName();
			Debug.Log(name);
		}
	}

	/// <summary>
	/// コンポーネントの有効化時にSteamコールバックを登録し、必要な初期化を行う。
	/// </summary>
	void OnEnable()
	{
		if (!SteamManager.Initialized) return;

		// Get Singleton Instance
		slotData = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance();
		subROM = SlotEffectMaker2023.Singleton.EffectDataManagerSingleton.GetInstance();

		// Cache the GameID for use in the Callbacks
		m_GameID = new CGameID(SteamUtils.GetAppID());

		m_UserStatsReceived = Callback<UserStatsReceived_t>.Create(OnUserStatsReceived);
		m_UserStatsStored = Callback<UserStatsStored_t>.Create(OnUserStatsStored);
		m_UserAchievementStored = Callback<UserAchievementStored_t>.Create(OnAchievementStored);

		// These need to be reset to get the stats upon an Assembly reload in the Editor.
		m_bRequestedStats = false;
		m_bStatsValid = false;
	}

	/// <summary>
	/// 毎フレーム呼び出され、統計データ取得と保存処理を行う。
	/// </summary>
	private void Update()
	{
		if (!SteamManager.Initialized) return;

		if (!m_bRequestedStats)
		{
			if (!SteamManager.Initialized)
			{
				m_bRequestedStats = true;
				return;
			}

			// (Editorのみ)呼び出し前にリセットをかける
			AchieveReset();

			// 現在の統計情報を取得
			bool bSuccess = SteamUserStats.RequestCurrentStats();

			m_bRequestedStats = bSuccess;
		}

		if (!m_bStatsValid) return;

		if (m_bStoreStats)
		{
			// 統計情報をSteamに保存
			bool bSuccess = SteamUserStats.StoreStats();
			m_bStoreStats = !bSuccess;
		}
	}

	/// <summary>
	/// 実績を達成状態にし、Steamに保存要求を送る。
	/// </summary>
	/// <param name="ac">達成する実績データ。</param>
	private void UnlockAchievement(SlotEffectMaker2023.Data.GameAchievement ac)
	{
		ac.IsAchieved = true;

		// the icon may change once it's unlocked
		//achievement.m_iIconImage = 0;

		// mark it down
		SteamUserStats.SetAchievement(ac.DataID);

		// Store stats end of/next frame
		m_bStoreStats = true;
	}

	/// <summary>
	/// ゲーム状態の変化に応じて実績や統計情報を評価・更新する。
	/// </summary>
	public void OnGameStateChange()
	{
		if (!m_bStatsValid) return;

		var vm = slotData.valManager;
		var bs = slotData.basicData;

		foreach (var item in subROM.GameAchieve.elemData)
		{
			if (bs.gameMode == 0 && item.UpdateOnlyBonusIn) continue;

			if (item.Type == SlotEffectMaker2023.Data.AchieveDataType.Flag)
			{
				if (item.IsAchieved) continue;
				// 変数条件比較: 新規達成OKなら実績通知
				var cond = subROM.Timeline.GetCondFromName(item.RefData);
				if (!cond.Evaluate(OffsetOverrides)) continue;
				// 実績解除
				UnlockAchievement(item);
			}

			if (item.Type == SlotEffectMaker2023.Data.AchieveDataType.Num)
			{
				var valData = vm.GetVariable(item.RefData);
				if (valData != null) SteamUserStats.SetStat(item.DataID, valData.val + item.Offset);
				Debug.Log("SendData: " + item.RefData + " - " + (valData.val + item.Offset).ToString());
			}
		}

		m_bStoreStats = true;
	}

	/// <summary>
	/// Steamから取得した統計情報を内部に反映し、実績データを更新する。
	/// </summary>
	/// <param name="pCallback">ユーザーの統計情報取得結果。</param>
	private void OnUserStatsReceived(UserStatsReceived_t pCallback)
	{
		if (!SteamManager.Initialized) return;

		if ((ulong)m_GameID == pCallback.m_nGameID)
		{
			if (EResult.k_EResultOK == pCallback.m_eResult)
			{
				Debug.Log("Received stats and achievements from Steam\n");

				var vm = slotData.valManager;
				m_bStatsValid = true;

				foreach (var item in subROM.GameAchieve.elemData)
				{
					if (item.Type == SlotEffectMaker2023.Data.AchieveDataType.Flag)
					{
						bool isAchieved;
						bool ret = SteamUserStats.GetAchievement(item.DataID, out isAchieved);
						if (ret)
						{
							string Name = SteamUserStats.GetAchievementDisplayAttribute(item.DataID, "name");
							string Desc = SteamUserStats.GetAchievementDisplayAttribute(item.DataID, "desc");
							item.SetDetail(isAchieved, Name, Desc);
							Debug.Log("Stat Read: ID=" + item.DataID + ", Name=" + item.Title + ", Desc=" + item.Desc + ", Flag=" + item.IsAchieved.ToString());
						}
						else
						{
							Debug.LogWarning("SteamUserStats.GetAchievement failed for Achievement " + item.DataID);
						}
					}
					else if (item.Type == SlotEffectMaker2023.Data.AchieveDataType.Num)
					{
						int st;
						SteamUserStats.GetStat(item.DataID, out st);
						item.StartVal = st;
						var valData = vm.GetVariable(item.RefData);
						if (valData != null)
						{
							var tup = new Tuple<string, int>(item.RefData, item.StartVal - valData.val);
							Debug.Log("Stat Read: ID=" + item.DataID + ", Val=" + item.StartVal.ToString() + ", Offset=" + tup.Item2);
							OffsetOverrides.Add(tup);
							item.Offset = tup.Item2;
						}
					}
				}
			}
			else
			{
				Debug.Log("RequestStats - failed, " + pCallback.m_eResult);
			}
		}
	}

	/// <summary>
	/// 統計情報がSteamに保存された際に呼ばれる処理。
	/// </summary>
	/// <param name="pCallback">保存結果に関するコールバック。</param>
	private void OnUserStatsStored(UserStatsStored_t pCallback)
	{
		if ((ulong)m_GameID == pCallback.m_nGameID)
		{
			if (EResult.k_EResultOK == pCallback.m_eResult)
			{
				Debug.Log("StoreStats - success");
			}
			else if (EResult.k_EResultInvalidParam == pCallback.m_eResult)
			{
				Debug.Log("StoreStats - some failed to validate");
				UserStatsReceived_t callback = new UserStatsReceived_t();
				callback.m_eResult = EResult.k_EResultOK;
				callback.m_nGameID = (ulong)m_GameID;
				OnUserStatsReceived(callback);
			}
			else
			{
				Debug.Log("StoreStats - failed, " + pCallback.m_eResult);
			}
		}
	}

	/// <summary>
	/// 実績がSteamに保存された際に呼ばれるコールバック。
	/// </summary>
	/// <param name="pCallback">実績保存処理に関する情報。</param>
	private void OnAchievementStored(UserAchievementStored_t pCallback)
	{
		if ((ulong)m_GameID == pCallback.m_nGameID)
		{
			if (0 == pCallback.m_nMaxProgress)
			{
				Debug.Log("Achievement '" + pCallback.m_rgchAchievementName + "' unlocked!");
			}
			else
			{
				Debug.Log("Achievement '" + pCallback.m_rgchAchievementName + "' progress callback, (" + pCallback.m_nCurProgress + "," + pCallback.m_nMaxProgress + ")");
			}
		}
	}
#endif
	/// <summary>
	/// 開発用：エディタ上で実績情報をリセットする。
	/// </summary>
	private void AchieveReset()
	{
#if UNITY_EDITOR
        //SteamUserStats.ResetAllStats(true);
#endif
	}
}
