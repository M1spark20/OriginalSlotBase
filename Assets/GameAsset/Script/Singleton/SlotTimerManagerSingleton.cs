using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlotTimerManagerSingleton
{
	// タイマ一覧
	public List<SlotTimer> timerData { get; set; }
	
	// Singletonインスタンス
	private static SlotTimerManagerSingleton ins = new SlotTimerManagerSingleton();

	/// <summary>
	/// インスタンスの初期化を行います。Singletonであるためprivateメンバです
	/// </summary>
	private SlotTimerManagerSingleton()
	{
		timerData = new List<SlotTimer>();
	}
	
	/// <summary>
	/// インスタンスの取得を行います。
	/// </summary>
	public static SlotTimerManagerSingleton GetInstance() { return ins; }
	
	// timerDataの読み込みを行う
	public bool ReadData(){
		// 読み込み処理(あとで実装)
		
		// タイマを新規作成する。読み込み処理で作成済みの場合は重複定義しない
		AddSystemTimer();
		return true;
	}
	
	// 名前に重複がないことを確認してタイマを新規作成する。
	// [ret]タイマを追加したか
	public bool CreateTimer(string pTimerName){
		for(int i=0; i<timerData.Count; ++i){
			if (timerData[i].timerName == pTimerName) return false;
		}
		timerData.Add(new SlotTimer(pTimerName));
		return true;
	}
	// 名前に一致したタイマを取得する
	// [ret]タイマのインスタンス, 見つからない場合はnull
	public SlotTimer GetTimer(string pTimerName){
		for(int i=0; i<timerData.Count; ++i){
			if (timerData[i].timerName == pTimerName) return timerData[i];
		}
		return null;
	}
	// タイマの加算処理を行う
	public void Process(float deltaTime){
		for(int i=0; i<timerData.Count; ++i) timerData[i].Update(deltaTime);
	}
	
	// スロット本体側で追加するタイマ一覧
	private void AddSystemTimer(){
		CreateTimer("general");			// ゲーム開始からの経過時間
		CreateTimer("betWait");			// BET待ち開始からの経過時間
		CreateTimer("betInput");		// BET開始からの経過時間
		CreateTimer("leverAvailable");	// レバー有効化からの経過時間
		CreateTimer("waitStart");		// wait開始からの経過時間
		CreateTimer("waitEnd");			// wait終了からの経過時間(次Gのwait算出に使用)
		CreateTimer("reelStart");		// リール始動からの経過時間
		CreateTimer("anyReelPush");		// いずれかの停止ボタン押下からの経過時間
		CreateTimer("anyReelStop");		// いずれかのリール停止からの経過時間
		CreateTimer("allReelStop");		// 全リール停止、ねじり終了からの経過時間
		CreateTimer("payoutStart");		// ペイアウト開始からの定義時間(完了後に無効になる)
		
		for(int i=0; i<SlotMaker2022.LocalDataSet.REEL_MAX; ++i){
			CreateTimer("reelPushPos[" + i + "]");		// 特定リール[0-reelMax)停止ボタン押下からの定義時間
			CreateTimer("reelStopPos[" + i + "]");		// 特定リール[0-reelMax)停止からの定義時間
			CreateTimer("reelPushOrder[" + i + "]");	// 第n停止ボタン押下からの定義時間
			CreateTimer("reelStopOrder[" + i + "]");	// 第n停止からの定義時間
		}
		
		// generalのみActivateする
		GetTimer("general")?.Activate();
	}
}
