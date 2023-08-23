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
		
		// リプレイ入賞時は即時前回のBET入力を行う(basicDataには前GのBET数が入っている)
		if (slotData.basicData.isReplay) applyBet = slotData.basicData.betCount;
		
		// 前回BET状況を初期化する - basicData.betCount=0としてSetBetTime()の処理を有効化する
		slotData.basicData.ClearBetCount();
		
		// タイマをActivateする
		timer.GetTimer("betWait").Activate();
		
		SetBetTime();
	}
	
	// 目標BET数に応じてタイマを設定する
	private void SetBetTime(){
		// BET数が前フレームから変わる場合はBetTimeをセット、タイマをリセットする
		if (slotData.basicData.betCount == applyBet) return;
		
		nextAddBetTime = 0;
		timer.GetTimer("leverAvailable").SetDisabled();
		timer.GetTimer("Pay-Bet").SetDisabled();
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
		
		// リプレイ以外ならBETに関する入力を受け付ける
		if (!slotData.basicData.isReplay){
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
			timer.GetTimer("Pay-Lever").SetDisabled();
			
			// 次フレームはSCWaitBeforeReelStartへ移行
			return new SCWaitBeforeReelStart();
		}
		
		// bet入力なしなら以降の操作を行わない
		if (!timer.GetTimer("betInput").isActivate) return this;
		// タイマの時刻を取得してBET加算処理を行う
		float betInput = (float)timer.GetTimer("betInput").elapsedTime;
		while (nextAddBetTime >= 0 && betInput > (float)nextAddBetTime / 1000f){
			slotData.basicData.AddBetCount();
			if (slotData.basicData.betCount == applyBet) {
				// BET処理終了
				nextAddBetTime = -1;
				// レバー有効かの判定、有効ならタイマを作動させる
				if(CheckBet(applyBet)) timer.GetTimer("leverAvailable").Activate();
			} else {
				// 次BETの時間取得
				nextAddBetTime += slotData.basicData.isReplay ? BET_SPAN_REP : BET_SPAN_BASIC;
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
	SlotDataSingleton slotData;

	// SC移行時処理
	public SCWaitBeforeReelStart(){
		// Singleton取得
		timer = SlotTimerManagerSingleton.GetInstance();
		slotData = SlotDataSingleton.GetInstance();
		
		// タイマ処理
		timer.GetTimer("waitStart").Activate();
		
		// BET消化処理
		slotData.basicData.LatchBet();
		
		// 乱数抽選処理
		SetCastFlag();
	}
	
	public void OnGetKeyDown(EGameButtonID pKeyID){ /* None */ }
	public void OnGetKey(EGameButtonID pKeyID){ /* None */ }
	public ISlotControllerBase ProcessAfterInput(){
		// Wait終了判定を行う(waitEndが無効か、最大wait時間以上経過しているとき)
		bool waitEnd = !timer.GetTimer("waitEnd").isActivate;
		if(!waitEnd) waitEnd = timer.GetTimer("waitEnd").elapsedTime > (float)WAIT_MAX / 1000f;
		
		if (waitEnd) {
			// タイマをクリアしてSCReelOperationへ移行
			timer.GetTimer("waitStart").SetDisabled();
			timer.GetTimer("waitEnd").Activate();
			timer.GetTimer("waitEnd").Reset();
			return new SCReelOperation();
		}
		
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

// リール回転中処理を行う
public class SCReelOperation : ISlotControllerBase {
	// 定数
	const int reelNum = SlotMaker2022.LocalDataSet.REEL_MAX;
	const int comaNum = SlotMaker2022.LocalDataSet.COMA_MAX;
	
	// 変数
	bool  isAllReleased;	// すべての停止ボタンが押下されていない状態か
	int[] stopHistory;		// 各リールの停止履歴
	int[] stopOrder;		// リール停止順
	int   stop1st;			// 第1停止停止箇所(0-62,63:1st)
	int   slip1st;			// 第1停止すべり数
	int   stopReelCount;	// 停止処理済みリール数
	
	SlotMaker2022.main_function.MainReelManager reelManager;	// リール制御クラス
	
	// 使用Singleton
	SlotMaker2022.MainROMDataManagerSingleton mainROM;
	SlotTimerManagerSingleton timer;
	SlotDataSingleton slotData;
	
	public SCReelOperation(){
		// Singleton取得
		mainROM  = SlotMaker2022.MainROMDataManagerSingleton.GetInstance();
		timer    = SlotTimerManagerSingleton.GetInstance();
		slotData = SlotDataSingleton.GetInstance();
		
		// 制御用変数初期化
		isAllReleased = false;
		stopHistory = new int[reelNum];
		stopOrder = new int[reelNum];
		for (int i=0; i<stopHistory.Length; ++i){
			stopHistory[i] = -1;
			stopOrder[i]   = -1;
		}
		stop1st = reelNum * comaNum;
		slip1st = -1;
		stopReelCount = 0;
		
		// リール制御クラス初期化
		reelManager = new SlotMaker2022.main_function.MainReelManager();
		
		// 全リールを始動させる
		for(int i=0; i<reelNum; ++i){ slotData.reelData[i].Start(); }
		
		// タイマを初期化する
		timer.GetTimer("reelStart").Activate();
		ResetPushStopTimer();
	}
	
	public void OnGetKeyDown(EGameButtonID pKeyID){
		// リール停止入力を行う
		if (pKeyID == EGameButtonID.e1Reel) StopReel(0);
		if (pKeyID == EGameButtonID.e2Reel) StopReel(1);
		if (pKeyID == EGameButtonID.e3Reel) StopReel(2);
	}
	public void OnGetKey(EGameButtonID pKeyID){
		// ねじり処理を行う
		isAllReleased &= !(pKeyID == EGameButtonID.e1Reel);
		isAllReleased &= !(pKeyID == EGameButtonID.e2Reel);
		isAllReleased &= !(pKeyID == EGameButtonID.e3Reel);
	}
	public ISlotControllerBase ProcessAfterInput(){
		// 各リールの処理を行い、停止済みか判定を行う
		bool isAllStopped = true;
		for(int i=0; i<reelNum; ++i){
			var checkReel = slotData.reelData[i];
			checkReel.Process();
			isAllStopped &= !checkReel.isRotate;
			
			// タイマ関係の処理
			if (!checkReel.isRotate){
				// Pos & Common
				SlotTimer checkTimer = timer.GetTimer("reelStopPos[" + i + "]");
				if (!checkTimer.isActivate) {
					checkTimer.Activate();
					timer.GetTimer("anyReelStop").Activate();
					timer.GetTimer("anyReelStop").Reset();
				}
				// Order
				int orderPos;
				for(orderPos = 0; orderPos < reelNum; ++orderPos) { if(stopOrder[orderPos] == i) break; }
				checkTimer = timer.GetTimer("reelStopOrder[" + orderPos + "]");
				if (!checkTimer.isActivate) checkTimer.Activate();
			}
		}
		
		// 全リールが停止済みの場合、ねじりがなければフェーズ移行
		if (isAllStopped && isAllReleased){
			// タイマ処理
			timer.GetTimer("allReelStop").Activate();
			timer.GetTimer("reelStart").SetDisabled();
			ResetPushStopTimer();
			// 移行先: SCJudgeAndPayout
			return new SCJudgeAndPayout();
		}
		
		// 次回のProcessに備えisAllReleasedを初期化
		isAllReleased = true;
		return this;
	}
	
	// 全リールのpush/stopタイマをリセットする
	private void ResetPushStopTimer(){
		timer.GetTimer("anyReelPush").SetDisabled();
		timer.GetTimer("anyReelStop").SetDisabled();
		timer.GetTimer("allReelStop").SetDisabled();
		for(int i=0; i<reelNum; ++i){
			timer.GetTimer("reelPushPos[" + i + "]").SetDisabled();		// 特定リール[0-reelMax)停止ボタン押下からの定義時間
			timer.GetTimer("reelStopPos[" + i + "]").SetDisabled();		// 特定リール[0-reelMax)停止からの定義時間
			timer.GetTimer("reelPushOrder[" + i + "]").SetDisabled();	// 第n停止ボタン押下からの定義時間
			timer.GetTimer("reelStopOrder[" + i + "]").SetDisabled();	// 第n停止からの定義時間
		}
	}
	
	// 指定したリールを停止させる
	private void StopReel(int reelIndex){
		var stopReel = slotData.reelData[reelIndex];
		
		// リールが停止制御できる状態にない場合処理をしない
		if (!stopReel.CanStop()) return;
		
		// リール制御データからすべりコマ数を算出する
		var bs = slotData.basicData;
		// 今回停止するリールにはstopHistoryにpushPosを入れて計算する
		stopHistory[reelIndex] = stopReel.GetReelComaIDFixed();
		int slipNum = reelManager.GetReelControl3R(reelIndex, stopHistory, stop1st, slip1st, bs.betCount-1, bs.gameMode, bs.bonusFlag, bs.castFlag);
		
		stopReel.SetStopPos(slipNum);
		//Debug.Log("Push: " + stopReel.pushPos.ToString() + ", Stop: " + stopReel.stopPos.ToString() + " (" + stopReel.slipCount.ToString() + ")");
		// 変数を制御する
		stopHistory[reelIndex] = stopReel.stopPos;
		stopOrder[stopReelCount] = reelIndex;
		if (stopReelCount == 0) {
			stop1st = reelIndex * comaNum + stopReel.stopPos;
			slip1st = stopReel.slipCount;
		}
		
		// タイマ処理を行う
		timer.GetTimer("reelPushPos[" + reelIndex + "]").Activate();
		timer.GetTimer("reelPushOrder[" + stopReelCount + "]").Activate();
		
		// ストップ数加算
		++stopReelCount;
	}
}

// 停止後入賞役判定 & 払い出し
public class SCJudgeAndPayout : ISlotControllerBase {
	// 変数
	int mPayoutNum;		// 描画払出枚数(-1:rep)
	int mNextPayTime;	// 次回払出描画時間[ms]
	int mNextPayBase;	// 払出間隔[ms]
	
	SlotMaker2022.main_function.MainReelManager reelManager;	// リール制御クラス
	
	// 使用Singleton
	SlotMaker2022.MainROMDataManagerSingleton mainROM;
	SlotTimerManagerSingleton timer;
	SlotDataSingleton slotData;

	// SC移行時処理
	public SCJudgeAndPayout(){
		// 変数初期化
		mPayoutNum   = 0;
		mNextPayTime = 0;
		
		// Singleton取得
		mainROM  = SlotMaker2022.MainROMDataManagerSingleton.GetInstance();
		timer    = SlotTimerManagerSingleton.GetInstance();
		slotData = SlotDataSingleton.GetInstance();
		
		// リール制御クラス初期化
		reelManager = new SlotMaker2022.main_function.MainReelManager();
		
		// 配当取得処理
		GetCast();
		
		// 払出間隔設定
		var castCommon = mainROM.CastCommonData;
		mNextPayBase = (int)castCommon.IntervalPay;		// 払出待ち時間
		if (slotData.basicData.isReplay){
			mNextPayTime = (int)castCommon.IntervalRep;	// リプ待ち時間(1回のみのためpayTimeに直接指定)
		}
		
		// タイマ処理
		if(mPayoutNum != 0) timer.GetTimer("payoutTime").Activate();
		timer.GetTimer("Pay-Bet").Activate();
		timer.GetTimer("Pay-Lever").Activate();
	}
	
	public void OnGetKeyDown(EGameButtonID pKeyID){ /* None */ }
	public void OnGetKey(EGameButtonID pKeyID){ /* None */ }
	public ISlotControllerBase ProcessAfterInput(){
		// 現在経過時刻取得
		float? elapsedNullable = timer.GetTimer("payoutTime").elapsedTime;
		const float divMS = 1000f;
		
		// elapsedが値を持つ場合(=payoutTimeが有効)に処理を行う
		if (elapsedNullable.HasValue){
			float elapsed = (float)elapsedNullable;
			// リプレイ: 指定時間経過後にmPayoutNumリセット
			if (mPayoutNum < 0 && elapsed > mNextPayTime / divMS) mPayoutNum = 0;
			// 払出: 時間経過でカウントを増やす
			while (mPayoutNum > 0 && elapsed > mNextPayTime / divMS){
				slotData.basicData.AddPayout();
				--mPayoutNum;
				mNextPayTime += mNextPayBase;
			}
		}
	
		// 払出なし or 払い出し完了: 描画終了時に処理をBETに戻す
		if(mPayoutNum == 0) {
			timer.GetTimer("payoutTime").SetDisabled();
			return new SCWaitBet();
		}
		
		return this;
	}
	
	// 配当取得を行う
	private void GetCast() {
		// 各リールの座標を得る
		const int reelNum = SlotMaker2022.LocalDataSet.REEL_MAX;
		int[] reelPos = new int[reelNum];
		for (int i=0; i<reelNum; ++i) reelPos[i] = slotData.reelData[i].GetReelComaIDFixed();
		
		// 配当をmanagerから得る(フラグ関係は指定なし:-1)
		var basicData = slotData.basicData;
		var castResult = reelManager.GetCast(reelPos, basicData.betCount-1, basicData.gameMode, -1, -1);
		
		// 配当をbasicDataに転送する。戻り値として払出枚数を受ける
		mPayoutNum = basicData.SetPayout(castResult);
	}
}
