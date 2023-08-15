using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum EGameButtonID {
	eMaxBetAndStart, e1Bet, eMaxBet, e1Reel, e2Reel, e3Reel, eButtonMax
}

public interface ISlotControllerBase
{
	// 初めてボタンが押された場合(GetKeyDown)の処理
	void OnGetKeyDown(EGameButtonID pKeyID);
	// 1フレーム目を含め、キーが押され続けている場合(GetKey)の処理
	void OnGetKey(EGameButtonID pKeyID);
	// 定常処理、キー入力を受け付けた後に実施する
	// 戻値：次フレームの処理に使用するインスタンス(変更なしの場合this)
	ISlotControllerBase ProcessAfterInput();
}

// BET入力を待つ状態
public class SCWaitBet : ISlotControllerBase {
	// 定数
	const int BET_SPAN_BASIC =  75;
	const int BET_SPAN_REP   = 150;
	
	byte applyBet;			// 適用先BET数
	int  nextAddBetTime;	// 次回BET加算時間[ms], 無効時:-1
	bool reelActivate;		// リールスタートフラグ
	
	// 使用Singleton
	SlotMaker2022.MainROMDataManagerSingleton mainROM;
	SlotTimerManagerSingleton timer;
	SlotDataSingleton slotData;
	
	// コンストラクタ: applyBet初期化
	public SCWaitBet(){
		applyBet = 0;
		nextAddBetTime = -1;
		reelActivate = false;
		
		// Singleton取得
		mainROM  = SlotMaker2022.MainROMDataManagerSingleton.GetInstance();
		timer    = SlotTimerManagerSingleton.GetInstance();
		slotData = SlotDataSingleton.GetInstance();
		
		// タイマをActivateする
		timer.GetTimer("betWait").Activate();
	}
	
	// 目標BET数に応じてタイマを設定する
	private void SetBetTime(){
		// BET数が前フレームから変わる場合はBetTimeをセット、タイマをリセットする
		if (slotData.basicData.betCount == applyBet) return;
		
		nextAddBetTime = 0;
		reelActivate = false;
		timer.GetTimer("betInput").Activate();
		timer.GetTimer("betInput").Reset();
		
		// 前回BET数より設定BETが小さくなる場合はリセットする
		if (applyBet < slotData.basicData.betCount) slotData.basicData.ClearBetCount();
	}
	
	public void OnGetKeyDown(EGameButtonID pKeyID){
		// 定数,Singleton取得
		const int betMax = SlotMaker2022.LocalDataSet.BET_MAX;
		byte currentGameMode = slotData.basicData.gameMode;
		
		// BETアクション中は入力をカットする
		if (!(nextAddBetTime < 0)) return;
		
		// レバー時の処理: BET処理が完了していればリールを始動させる
		if (pKeyID == EGameButtonID.eMaxBetAndStart && applyBet > 0){
			uint checkIndex = (uint)(betMax * currentGameMode + applyBet - 1);
			if (mainROM.CastCommonData.CanUseBet.GetData(checkIndex) != 0) { reelActivate = true; return; }
		}
		
		// 現在モードでのMaxBetを取得する
		byte currentMaxBet = 0;
		for (int betC=betMax; betC > 0; --betC){
			uint checkIndex = (uint)(betMax * currentGameMode + betC - 1);
			if (mainROM.CastCommonData.CanUseBet.GetData(checkIndex) != 0) { currentMaxBet = (byte)betC; break; }
		}
		
		// MaxBet押下時の処理: 選択モードでの最大BET数を指定
		if (pKeyID == EGameButtonID.eMaxBetAndStart || pKeyID == EGameButtonID.eMaxBet){ applyBet = currentMaxBet; }
		
		// 1BET押下時の処理: 有効/無効はさておき+1BETする。ただし最大値を超えた場合は1BETに戻す
		if (pKeyID == EGameButtonID.e1Bet){
			applyBet += 1;
			if (applyBet > currentMaxBet) applyBet = 1;
		}
		
		// BET変化時処理を行う
		SetBetTime();
	}
	
	public void OnGetKey(EGameButtonID pKeyID){ /* None */ }
	
	public ISlotControllerBase ProcessAfterInput(){
		// reelActivateが有効ならリールを始動させる
		if (reelActivate){
			for(int i=0; i<slotData.reelData.Count; ++i){ slotData.reelData[i].Start(); }
			reelActivate = false;
		}
		
		// タイマの時刻を取得してBET加算処理を行う
		float betInput = timer.GetTimer("betInput").elapsedTime;
		while (nextAddBetTime >= 0 && betInput > (float)nextAddBetTime / 1000f){
			slotData.basicData.AddBetCount();
			Debug.Log("Bet Countup: " + slotData.basicData.betCount.ToString());
			if (slotData.basicData.betCount == applyBet) nextAddBetTime = -1;
			else nextAddBetTime += BET_SPAN_BASIC;
		}
		
		return this;
	}
}

