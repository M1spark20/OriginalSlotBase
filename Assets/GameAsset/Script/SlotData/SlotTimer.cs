using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

// ユーザが作成するタイマデータ
public class UserTimerData : SlotMaker2022.ILocalDataInterface {
	public string UserTimerName   { get; set; }	// タイマ名
	public bool   StoreActivation { get; set; }	// 有効状況を保存するか
	
	public bool StoreData(ref BinaryWriter fs, int version){
		fs.Write(UserTimerName);
		fs.Write(StoreActivation);
		return true;
	}
	public bool ReadData(ref BinaryReader fs, int version){
		UserTimerName = fs.ReadString();
		StoreActivation = fs.ReadBoolean();
		return true;
	}
}

// タイマ一覧を管理するクラス
public class TimerList {
	// タイマのリストを生成。システムタイマ/ユーザタイマ/サウンドタイマ
	public List<UserTimerData> TData { get; private set; }
	
	// システムタイマを登録する
	public TimerList(){
		TData = new List<UserTimerData>();		// タイマ一覧のインスタンス生成
		
		CreateTimer("general",        true);	// ゲーム開始からの経過時間
		CreateTimer("betWait",        true);	// BET待ち開始からの経過時間
		CreateTimer("betInput",       true);	// BET開始からの経過時間
		CreateTimer("leverAvailable", true);	// レバー有効化からの経過時間
		CreateTimer("waitStart",      true);	// wait開始からの経過時間
		CreateTimer("waitEnd",        true);	// wait終了からの経過時間(次Gのwait算出に使用)
		CreateTimer("reelStart",      true);	// リール始動からの経過時間
		CreateTimer("anyReelPush",    true);	// いずれかの停止ボタン押下からの経過時間
		CreateTimer("anyReelStop",    true);	// いずれかのリール停止からの経過時間
		CreateTimer("allReelStop",    true);	// 全リール停止、ねじり終了からの経過時間
		CreateTimer("payoutTime",     true);	// ペイアウト開始からの経過時間(pay完了まで有効)
		CreateTimer("Pay-Bet",        true);	// ペイアウト開始からの経過時間(次回BETまで有効)
		CreateTimer("Pay-Lever",      true);	// ペイアウト開始からの経過時間(次ゲームレバーオンまで有効)
		
		for(int i=0; i<SlotMaker2022.LocalDataSet.REEL_MAX; ++i){
			CreateTimer("reelPushPos[" + i + "]",   true);	// 特定リール[0-reelMax)停止ボタン押下からの定義時間
			CreateTimer("reelStopPos[" + i + "]",   true);	// 特定リール[0-reelMax)停止からの定義時間
			CreateTimer("reelPushOrder[" + i + "]", true);	// 第n停止ボタン押下からの定義時間
			CreateTimer("reelStopOrder[" + i + "]", true);	// 第n停止からの定義時間
		}
	}
	
	public void CreateTimer(string name, bool storeActivation){
		UserTimerData data = new UserTimerData();
		data.UserTimerName = name;
		data.StoreActivation = storeActivation;
		TData.Add(data);
	}
	
	public string[] GetTimerName(){
		string[] ans = new string[TData.Count];
		for(int i=0; i<TData.Count; ++i) ans[i] = TData[i].UserTimerName;
		return ans;
	}
}
