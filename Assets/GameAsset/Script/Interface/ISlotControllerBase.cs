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
		timer.GetTimer("leverAvailable").SetDisabled();
		timer.GetTimer("betInput").Activate();
		timer.GetTimer("betInput").Reset();
		
		// 前回BET数より設定BETが小さくなる場合はリセットする
		if (applyBet < slotData.basicData.betCount) slotData.basicData.ClearBetCount();
	}
	
	public void OnGetKeyDown(EGameButtonID pKeyID){
		// 定数,Singleton取得
		const int betMax = SlotMaker2022.LocalDataSet.BET_MAX;
		
		// BETアクション中は入力をカットする
		if (!(nextAddBetTime < 0)) return;
		
		// レバー時の処理: レバー有効ならリールを始動させる
		if (pKeyID == EGameButtonID.eMaxBetAndStart && applyBet > 0 && timer.GetTimer("leverAvailable").isActivate){
			reelActivate = true; return;
		}
		
		// 現在モードでのMaxBetを取得する
		byte currentMaxBet = 0;
		byte currentGameMode = slotData.basicData.gameMode;
		for (int betC=betMax; betC > 0; --betC){
			if (CheckBet((byte)betC)) { currentMaxBet = (byte)betC; break; }
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
			// タイマ関係の停止処理
			timer.GetTimer("betWait").SetDisabled();
			timer.GetTimer("betInput").SetDisabled();
			timer.GetTimer("leverAvailable").SetDisabled();
			
			// 次フレームはSCWaitBeforeReelStartへ移行
			return new SCWaitBeforeReelStart();
		}
		
		// タイマの時刻を取得してBET加算処理を行う
		float betInput = timer.GetTimer("betInput").elapsedTime;
		while (nextAddBetTime >= 0 && betInput > (float)nextAddBetTime / 1000f){
			slotData.basicData.AddBetCount();
			Debug.Log("Bet Countup: " + slotData.basicData.betCount.ToString());
			if (slotData.basicData.betCount == applyBet) {
				// BET処理終了
				nextAddBetTime = -1;
				// レバー有効かの判定、有効ならタイマを作動させる
				if(CheckBet(applyBet)) timer.GetTimer("leverAvailable").Activate();
			} else {
				// 次BETの時間取得
				nextAddBetTime += BET_SPAN_BASIC;
			}
		}
		
		return this;
	}
	
	// 現モードで指定したBet数が有効か取得する
	private bool CheckBet(byte betNum){
		byte currentGameMode = slotData.basicData.gameMode;
		const int betMax = SlotMaker2022.LocalDataSet.BET_MAX;
		
		uint checkIndex = (uint)(betMax * currentGameMode + betNum - 1);
		return (mainROM.CastCommonData.CanUseBet.GetData(checkIndex) != 0);
	}
}

// レバー後Wait処理
public class SCWaitBeforeReelStart : ISlotControllerBase {
	// 定数
	const int WAIT_MAX = 4100;	// 最大wait時間[ms]

	// 使用Singleton
	SlotTimerManagerSingleton timer;

	// SC移行時処理
	public SCWaitBeforeReelStart(){
		// Singleton取得
		timer = SlotTimerManagerSingleton.GetInstance();
		
		// タイマ処理
		timer.GetTimer("waitStart").Activate();
		
		// 乱数抽選処理
		SetCastFlag();
	}
	
	public void OnGetKeyDown(EGameButtonID pKeyID){ /* None */ }
	public void OnGetKey(EGameButtonID pKeyID){ /* None */ }
	public ISlotControllerBase ProcessAfterInput(){
		// Wait終了判定を行う(waitEndが無効か、最大wait時間以上経過しているとき)
		bool waitEnd = !timer.GetTimer("waitEnd").isActivate;
		if(!waitEnd) waitEnd = timer.GetTimer("waitEnd").elapsedTime > (float)WAIT_MAX / 1000f;
		return this;
	}
	
	// フラグ抽選処理を行う
	private void SetCastFlag(){
		// 乱数を取得する
		int randValue = UnityEngine.Random.Range(0, SlotMaker2022.LocalDataSet.FlagCommonData.RAND_MAX);
		
		// Singleton取得, 変数初期化
		var basicData = SlotDataSingleton.GetInstance().basicData;
		var randList  = SlotMaker2022.MainROMDataManagerSingleton.GetInstance().FlagRandData;
		byte castFlag  = 0;
		byte bonusFlag = 0;
		
		// 現在の条件に合った乱数のみrandValueから引いていく
		for (int randC=0; randC < randList.Count; ++randC){
			var randItem = randList[randC];
			if (randItem.CondBet+1 != basicData.betCount) continue;		// BET(CondBet側は0-2で定義)
			if (randItem.CondGameMode != basicData.gameMode) continue;	// Mode
			// RT(共通設定の場合は比較省略)
			if (randItem.CondRTMode != basicData.RTMode && randItem.CondRTMode < SlotMaker2022.LocalDataSet.RTMODE_MAX) continue;
			
			// 減算する乱数を抽出して減算する
			uint checkIndex = randItem.CommonSet ? 0u : (uint)basicData.slotSetting;
			randValue -= (int)randItem.RandValue.GetData(checkIndex);
			Debug.Log("randV: " + randValue.ToString() + " / dec: " + randItem.RandValue.GetData(checkIndex).ToString());
			
			// 乱数が0を下回ったらフラグ確定
			if (randValue < 0){
				castFlag  = randItem.LaunchFlagID;
				bonusFlag = randItem.BonusFlag;
				break;
			}
		}
		
		// basicDataにフラグを設定する
		basicData.SetCastFlag(bonusFlag, castFlag);
	}
}
