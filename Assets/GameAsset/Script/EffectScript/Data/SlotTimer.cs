using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace SlotEffectMaker2023.Data
{
	// ユーザが作成するタイマデータ(Sys: TimerListから読み書き)
	public class UserTimerData : IEffectNameInterface
	{
		public string UserTimerName { get; set; }   // タイマ名
		public bool StoreActivation { get; set; }   // 有効状況を保存するか
		public string Usage { get; set; }		    // 用途

		public UserTimerData()
        {
			UserTimerName = "$";
			StoreActivation = true;
			Usage = string.Empty;
        }
		public bool StoreData(ref BinaryWriter fs, int version)
		{
			fs.Write(UserTimerName);
			fs.Write(StoreActivation);
			fs.Write(Usage);
			return true;
		}
		public bool ReadData(ref BinaryReader fs, int version)
		{
			UserTimerName = fs.ReadString();
			StoreActivation = fs.ReadBoolean();
			Usage = fs.ReadString();
			return true;
		}
		public void Rename(EChangeNameType type, string src, string dst) { }
	}

	// タイマ一覧を管理するクラス(Sys)
	public class TimerList : IEffectNameInterface
	{
		// タイマのリストを生成。システムタイマ(識別子なし)/ユーザタイマ($)/サウンドタイマ(#)
		public List<UserTimerData> SysTimer { get; private set; }
		public List<UserTimerData> UserData { get; private set; }

		// システムタイマを登録する
		public TimerList()
		{
			SysTimer = new List<UserTimerData>(); 
			UserData = new List<UserTimerData>();      // タイマ一覧のインスタンス生成

			CreateSysTimer("general", true);   // ゲーム開始からの経過時間
			CreateSysTimer("betWait", true);   // BET待ち開始からの経過時間
			CreateSysTimer("betInput", true);  // BET開始からの経過時間
			CreateSysTimer("betShot", true);   // 1BET処理からの経過時間
			CreateSysTimer("leverAvailable", true);    // レバー有効化からの経過時間
			CreateSysTimer("waitStart", true); // wait開始からの経過時間
			CreateSysTimer("waitEnd", true);   // wait終了からの経過時間(次Gのwait算出に使用)
			CreateSysTimer("reelStart", true); // リール始動からの経過時間
			CreateSysTimer("anyReelPush", true);   // いずれかの停止ボタン押下からの経過時間
			CreateSysTimer("anyReelStop", true);   // いずれかのリール停止からの経過時間
			CreateSysTimer("allReelStop", true);   // 全リール停止、ねじり終了からの経過時間
			CreateSysTimer("payoutTime", true);    // ペイアウト開始からの経過時間(pay完了まで有効)
			CreateSysTimer("Pay-Bet", true);   // ペイアウト開始からの経過時間(次回BETまで有効)
			CreateSysTimer("Pay-Lever", true); // ペイアウト開始からの経過時間(次ゲームレバーオンまで有効)
			CreateSysTimer("changeMode", true);    // モード移行からの経過時間
			CreateSysTimer("changeRT", true);  // RT移行からの経過時間
			CreateSysTimer("resetMode", true); // モードリセットからの経過時間
			CreateSysTimer("resetRT", true);   // RTリセットからの経過時間

			for (int i = 0; i < SlotMaker2022.LocalDataSet.REEL_MAX; ++i)
			{
				CreateSysTimer("reelPushPos[" + i + "]", true);    // 特定リール[0-reelMax)停止ボタン押下からの定義時間
				CreateSysTimer("reelStopPos[" + i + "]", true);    // 特定リール[0-reelMax)停止からの定義時間
				CreateSysTimer("reelPushOrder[" + i + "]", true);  // 第n停止ボタン押下からの定義時間
				CreateSysTimer("reelStopOrder[" + i + "]", true);  // 第n停止からの定義時間
			}
		}
		public bool StoreData(ref BinaryWriter fs, int version)
		{
			// ユーザタイマ($から始まるデータ)のみ保存
			fs.Write(UserData.Count);
			foreach (var item in UserData)
				item.StoreData(ref fs, version);
			return true;
		}
		public bool ReadData(ref BinaryReader fs, int version)
		{
			int dataCount = fs.ReadInt32();
			for(int i=0; i<dataCount; ++i)
            {
				UserTimerData ut = new UserTimerData();
				ut.ReadData(ref fs, version);
				CreateTimer(ut);
            }
			return true;
		}
		public void Rename(EChangeNameType type, string src, string dst)
        {
			foreach (var tm in SysTimer) tm.Rename(type, src, dst);
			foreach (var tm in UserData) tm.Rename(type, src, dst);
        }

		public void CreateTimer(string name, bool storeActivation)
		{
            UserTimerData data = new UserTimerData
            {
                UserTimerName = name,
                StoreActivation = storeActivation
            };
			CreateTimer(data);
		}
		public void CreateTimer(UserTimerData pData)
        {
			UserData.Add(pData);
        }
		private void CreateSysTimer(string name, bool storeActivation)
        {
			UserTimerData data = new UserTimerData
			{
				UserTimerName = name,
				StoreActivation = storeActivation
			};
			SysTimer.Add(data);
		}
		public bool CheckTimerExist(string name)
        {
			foreach (var item in SysTimer)
				if (item.UserTimerName.Equals(name)) return true;

			foreach (var item in UserData)
				if (item.UserTimerName.Equals(name)) return true;

			// 音源関係のタイマを引っ張ってくる
			var soundPlay = Singleton.EffectDataManagerSingleton.GetInstance().SoundPlayList;
			foreach (var item in soundPlay)
			{
				if (item.GetShotTimerName().Equals(name)) return true;
				if (item.GetLoopTimerName().Equals(name)) return true;
			}

			return false;
		}
		public string[] GetTimerNameList()
		{
			List<string> ans = new List<string>();
			foreach (var item in SysTimer)
				ans.Add(item.UserTimerName);

			foreach (var item in UserData)
				ans.Add(item.UserTimerName);

			// 音源関係のタイマを引っ張ってくる
			var soundPlay = Singleton.EffectDataManagerSingleton.GetInstance().SoundPlayList;
			foreach (var item in soundPlay) {
				ans.Add(item.GetShotTimerName());
				ans.Add(item.GetLoopTimerName());
			}

			return ans.ToArray();
		}
	}
}