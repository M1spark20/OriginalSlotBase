using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace SlotEffectMaker2023.Data
{
	/// <summary>
	/// ユーザが作成するタイマーデータを保持するクラスです。（Sys: TimerListから読み書き）
	/// </summary>
	public class UserTimerData : IEffectNameInterface
	{
		/// <summary>タイマ名</summary>
		public string UserTimerName { get; set; }   // タイマ名

		/// <summary>有効状況を保存するかどうか</summary>
		public bool StoreActivation { get; set; }   // 有効状況を保存するか

		/// <summary>用途を示す文字列</summary>
		public string Usage { get; set; }           // 用途

		/// <summary>デフォルトコンストラクタ。プロパティを初期化します。</summary>
		public UserTimerData()
		{
			UserTimerName = "$";
			StoreActivation = true;
			Usage = string.Empty;
		}

		/// <summary>
		/// このインスタンスをバイナリに書き込みます。
		/// </summary>
		/// <param name="fs">書き込み先の BinaryWriter（ref）</param>
		/// <param name="version">データのバージョン</param>
		/// <returns>書き込み成功時に true を返します。</returns>
		public bool StoreData(ref BinaryWriter fs, int version)
		{
			fs.Write(UserTimerName);
			fs.Write(StoreActivation);
			fs.Write(Usage);
			return true;
		}

		/// <summary>
		/// バイナリからこのインスタンスを読み込みます。
		/// </summary>
		/// <param name="fs">読み込み元の BinaryReader（ref）</param>
		/// <param name="version">データのバージョン</param>
		/// <returns>読み込み成功時に true を返します。</returns>
		public bool ReadData(ref BinaryReader fs, int version)
		{
			UserTimerName = fs.ReadString();
			StoreActivation = fs.ReadBoolean();
			Usage = fs.ReadString();
			return true;
		}

		/// <summary>
		/// 名前変更時に呼び出されますが、本クラスでは特に処理を行いません。
		/// </summary>
		/// <param name="type">変更の種類</param>
		/// <param name="src">元の名前</param>
		/// <param name="dst">新しい名前</param>
		public void Rename(EChangeNameType type, string src, string dst)
		{
			// 処理なし
		}
	}

	/// <summary>
	/// システムおよびユーザタイマーの一覧を管理するクラスです。（Sys）
	/// </summary>
	public class TimerList : IEffectNameInterface
	{
		/// <summary>システムタイマーのリスト</summary>
		public List<UserTimerData> SysTimer { get; private set; }  // タイマのリストを生成。システムタイマ(識別子なし)/ユーザタイマ($)/サウンドタイマ(#)

		/// <summary>ユーザ定義タイマーのリスト</summary>
		public List<UserTimerData> UserData { get; private set; }

		/// <summary>
		/// デフォルトコンストラクタ。システムタイマーを登録し、リストを初期化します。
		/// </summary>
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
			CreateSysTimer("waitEnd", false);   // wait終了からの経過時間(次Gのwait算出に使用)
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
			CreateSysTimer("reelPushFreeze", true);   // リール回転中フリーズ時間
			CreateSysTimer("beforePayFreeze", true);  // 払い出し前フリーズ時間
			CreateSysTimer("afterPayFreeze", true);   // 払い出し後フリーズ時間(リプレイより前)
			CreateSysTimer("AssistSound", false);   // 目押しアシスト音

			for (int i = 0; i < SlotMaker2022.LocalDataSet.REEL_MAX; ++i)
			{
				CreateSysTimer("reelPushPos[" + i + "]", true);    // 特定リール[0-reelMax)停止ボタン押下からの定義時間
				CreateSysTimer("reelStopPos[" + i + "]", true);    // 特定リール[0-reelMax)停止からの定義時間
				CreateSysTimer("reelPushOrder[" + i + "]", true);  // 第n停止ボタン押下からの定義時間
				CreateSysTimer("reelStopOrder[" + i + "]", true);  // 第n停止からの定義時間
			}
		}

		/// <summary>
		/// ユーザタイマーのみをバイナリに書き込みます。
		/// </summary>
		/// <param name="fs">書き込み先の BinaryWriter（ref）</param>
		/// <param name="version">データのバージョン</param>
		/// <returns>書き込み成功時に true を返します。</returns>
		public bool StoreData(ref BinaryWriter fs, int version)
		{
			// ユーザタイマ($から始まるデータ)のみ保存
			fs.Write(UserData.Count);
			foreach (var item in UserData)
				item.StoreData(ref fs, version);
			return true;
		}

		/// <summary>
		/// バイナリからユーザタイマーを読み込み、リストに追加します。
		/// </summary>
		/// <param name="fs">読み込み元の BinaryReader（ref）</param>
		/// <param name="version">データのバージョン</param>
		/// <returns>読み込み成功時に true を返します。</returns>
		public bool ReadData(ref BinaryReader fs, int version)
		{
			int dataCount = fs.ReadInt32();
			for (int i = 0; i < dataCount; ++i)
			{
				UserTimerData ut = new UserTimerData();
				ut.ReadData(ref fs, version);
				CreateTimer(ut);
			}
			return true;
		}

		/// <summary>
		/// 名前変更時にすべてのタイマーで Rename を呼び出します。
		/// </summary>
		/// <param name="type">変更の種類</param>
		/// <param name="src">元の名前</param>
		/// <param name="dst">新しい名前</param>
		public void Rename(EChangeNameType type, string src, string dst)
		{
			foreach (var tm in SysTimer) tm.Rename(type, src, dst);
			foreach (var tm in UserData) tm.Rename(type, src, dst);
		}

		/// <summary>
		/// 新しいユーザタイマーを作成しリストに追加します。
		/// </summary>
		/// <param name="name">タイマ名</param>
		/// <param name="storeActivation">有効状況保持フラグ</param>
		public void CreateTimer(string name, bool storeActivation)
		{
			UserTimerData data = new UserTimerData
			{
				UserTimerName = name,
				StoreActivation = storeActivation
			};
			CreateTimer(data);
		}

		/// <summary>
		/// UserTimerData インスタンスを直接リストに追加します。
		/// </summary>
		/// <param name="pData">追加する UserTimerData</param>
		public void CreateTimer(UserTimerData pData)
		{
			UserData.Add(pData);
		}

		/// <summary>
		/// システムタイマーを作成し SysTimer リストに追加します。
		/// </summary>
		/// <param name="name">タイマ名</param>
		/// <param name="storeActivation">有効状況保持フラグ</param>
		private void CreateSysTimer(string name, bool storeActivation)
		{
			UserTimerData data = new UserTimerData
			{
				UserTimerName = name,
				StoreActivation = storeActivation
			};
			SysTimer.Add(data);
		}

		/// <summary>
		/// 指定した名前のタイマーが存在するか確認します。
		/// </summary>
		/// <param name="name">検索するタイマ名</param>
		/// <returns>存在すれば true、存在しなければ false を返します。</returns>
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

		/// <summary>
		/// すべてのタイマ名を配列で取得します。
		/// </summary>
		/// <returns>タイマ名の配列</returns>
		public string[] GetTimerNameList()
		{
			List<string> ans = new List<string>();
			foreach (var item in SysTimer)
				ans.Add(item.UserTimerName);

			foreach (var item in UserData)
				ans.Add(item.UserTimerName);

			// 音源関係のタイマを引っ張ってくる
			var soundPlay = Singleton.EffectDataManagerSingleton.GetInstance().SoundPlayList;
			foreach (var item in soundPlay)
			{
				ans.Add(item.GetShotTimerName());
				ans.Add(item.GetLoopTimerName());
			}

			return ans.ToArray();
		}
	}
}
