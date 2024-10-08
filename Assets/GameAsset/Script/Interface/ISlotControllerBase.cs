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
	ISlotControllerBase ProcessAfterInput(Action pSaveCallBack, Action pSteamAPICallBack);
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
	SlotEffectMaker2023.Action.SlotTimerManager timer;
	SlotEffectMaker2023.Singleton.SlotDataSingleton slotData;
	
	// コンストラクタ: applyBet初期化
	public SCWaitBet(){
		applyBet = 0;
		nextAddBetTime = -1;
		reelActivate = false;
		
		// Singleton取得
		mainROM  = SlotMaker2022.MainROMDataManagerSingleton.GetInstance();
		slotData = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance();
		timer    = slotData.timerData;
		
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
	
	public ISlotControllerBase ProcessAfterInput(Action pSaveCallBack, Action pSteamAPICallBack){
		// reelActivateが有効ならリールを始動させる
		if (reelActivate){
			// タイマ関係の停止処理
			timer.GetTimer("betWait").SetDisabled();
			timer.GetTimer("betInput").SetDisabled();
			timer.GetTimer("betShot").SetDisabled();
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
			// タイマを更新する
			timer.GetTimer("betShot")?.Activate();
			timer.GetTimer("betShot")?.Reset();
			// BET終了判定
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
	int waitTime;

	// 使用Singleton
	SlotMaker2022.MainROMDataManagerSingleton mainROM;
	SlotEffectMaker2023.Action.SlotTimerManager timer;
	SlotEffectMaker2023.Singleton.SlotDataSingleton slotData;

	// SC移行時処理
	public SCWaitBeforeReelStart(){
		// Singleton取得
		mainROM  = SlotMaker2022.MainROMDataManagerSingleton.GetInstance();
		slotData = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance();
		timer    = slotData.timerData;
		
		// タイマ処理
		var sys = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance().sysData;
		const int WAITCUT_TIME = 10;

		timer.GetTimer("waitStart").Activate();
		waitTime = sys.WaitCut ? WAITCUT_TIME : WAIT_MAX;
		
		// BET消化処理
		slotData.basicData.LatchBet();
		// 履歴関係処理(成立後G数)
		slotData.historyManager.ReelStart();
		
		// 乱数抽選処理
		SetCastFlag();
	}
	
	public void OnGetKeyDown(EGameButtonID pKeyID){ /* None */ }
	public void OnGetKey(EGameButtonID pKeyID){ /* None */ }
	public ISlotControllerBase ProcessAfterInput(Action pSaveCallBack, Action pSteamAPICallBack){
		// Wait終了判定を行う(waitEndが無効か、最大wait時間以上経過しているとき)
		bool waitEnd = !timer.GetTimer("waitEnd").isActivate;
		if(!waitEnd) waitEnd = timer.GetTimer("waitEnd").elapsedTime > (float)waitTime / 1000f;
		
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
		var basicData = slotData.basicData;
		var randList  = mainROM.FlagRandData;
		byte castFlag  = 0;
		byte bonusFlag = 0;
		
		// フリーズ抽選用に前回のデータを保持
		byte lastBonus = basicData.bonusFlag;
		byte lastRT = basicData.RTMode;
		
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
		basicData.SetCastFlag(bonusFlag, castFlag, mainROM.CastCommonData, mainROM.RTCommonData, timer);
		// フリーズ抽選
		slotData.freezeManager.SetFreezeFlag(mainROM.FreezeControlData, mainROM.FreezeTimeData, castFlag, bonusFlag != lastBonus);
		slotData.freezeManager.SetFreezeRT(mainROM.FreezeControlData, mainROM.FreezeTimeData, lastRT, basicData.RTMode);
		// フリーズ取得(before)
		int beforeWait = slotData.freezeManager.GetFreeze(SlotMaker2022.LocalDataSet.FreezeControlData.FreezeTiming.BeforeWait);
		waitTime += beforeWait;
		// フリーズ取得(after)
		int afterWait = slotData.freezeManager.GetFreeze(SlotMaker2022.LocalDataSet.FreezeControlData.FreezeTiming.AfterWait);
		if (afterWait > 0){
			if (!timer.GetTimer("waitEnd").isActivate) {
				timer.GetTimer("waitEnd").Activate();
				waitTime = afterWait;
			} else {
				int overTime = (int)(timer.GetTimer("waitEnd").elapsedTime * 1000f) - waitTime;
				waitTime = waitTime + afterWait + overTime > 0 ? overTime : 0;
			}
		}
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
	int   reelFreezeTime;	// フリーズ時間(入力スキップを行う)
	
	SlotMaker2022.main_function.MainReelManager reelManager;	// リール制御クラス
	
	// 使用Singleton
	SlotMaker2022.MainROMDataManagerSingleton mainROM;
	SlotEffectMaker2023.Action.SlotTimerManager timer;
	SlotEffectMaker2023.Singleton.SlotDataSingleton slotData;
	SlotEffectMaker2023.Singleton.EffectDataManagerSingleton subROM;
	
	public SCReelOperation(){
		// Singleton取得
		mainROM  = SlotMaker2022.MainROMDataManagerSingleton.GetInstance();
		slotData = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance();
		timer    = slotData.timerData;
		subROM   = SlotEffectMaker2023.Singleton.EffectDataManagerSingleton.GetInstance();
		
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
		reelFreezeTime = 0;
		
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
		isAllReleased = false;
	}
	public ISlotControllerBase ProcessAfterInput(Action pSaveCallBack, Action pSteamAPICallBack){
		// 各リールの処理を行い、停止済みか判定を行う
		bool isAllStopped = true;
		for(int i=0; i<reelNum; ++i){
			var tm = timer.GetTimer("reelStart");
			var checkReel = slotData.reelData[i];
			float elapsed = (tm.elapsedTime - tm.lastElapsed) ?? 0f;	// ??: null許容型がnullの場合のキャスト
			checkReel.Process(elapsed);
			isAllStopped &= !checkReel.isRotate;
			
			// タイマ関係の処理
			if (!checkReel.isRotate){
				// Pos & Common
				var checkTimer = timer.GetTimer("reelStopPos[" + i + "]");
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
		
		// 全リールが停止済みの場合、ねじり・フリーズがなければフェーズ移行
		if (isAllStopped && isAllReleased && !timer.GetTimer("reelPushFreeze").isActivate){
			// タイマ処理
			timer.GetTimer("allReelStop").Activate();
			timer.GetTimer("reelStart").SetDisabled();
			ResetPushStopTimer();
			// 移行先: SCJudgeAndPayout(20240816ADD:引数追加)
			return new SCJudgeAndPayout(pSteamAPICallBack);
		}
		
		// フリーズタイマ処理
		if (timer.GetTimer("reelPushFreeze").isActivate){
			if (timer.GetTimer("reelPushFreeze").elapsedTime > (float)reelFreezeTime / 1000f){
				reelFreezeTime = 0;
				timer.GetTimer("reelPushFreeze").SetDisabled();
			}
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
		// フリーズ中は停止処理をかけない
		if (timer.GetTimer("reelPushFreeze").isActivate) return;
		
		// リール制御データからすべりコマ数を算出する
		var bs = slotData.basicData;
		// 今回停止するリールにはstopHistoryにpushPosを入れて計算する
		stopHistory[reelIndex] = stopReel.GetReelComaIDFixed();
		int slipNum = reelManager.GetReelControl3R(reelIndex, stopHistory, stop1st, slip1st, bs.betCount-1, bs.gameMode, bs.bonusFlag, bs.castFlag);
		
		stopReel.SetStopPos(slipNum, stopReelCount);
		//Debug.Log("Push: " + stopReel.pushPos.ToString() + ", Stop: " + stopReel.stopPos.ToString() + " (" + stopReel.slipCount.ToString() + ")");
		// 変数を制御する
		stopHistory[reelIndex] = stopReel.stopPos;
		stopOrder[stopReelCount] = reelIndex;
		if (stopReelCount == 0) {
			stop1st = reelIndex * comaNum + stopReel.stopPos;
			slip1st = stopReel.slipCount;
		}
		
		// テンパイ判定(=入賞可能性のあるボーナス判定)を行う
		// 配当をmanagerから得る(フラグ関係は指定なし:-1)
		var castResult = reelManager.GetCast(stopHistory, bs.betCount-1, bs.gameMode, -1, -1);
		bs.CheckTenpai(castResult, mainROM.CastCommonData);
		
		// タイマ処理を行う
		timer.GetTimer("anyReelPush").Activate();
		timer.GetTimer("anyReelPush").Reset();
		timer.GetTimer("reelPushPos[" + reelIndex + "]").Activate();
		timer.GetTimer("reelPushOrder[" + stopReelCount + "]").Activate();
		
		// ストップ数加算
		++stopReelCount;
		
		// リーチ目判定(ただし全停止後の判定はモード移行終了後:SCJudgeAndPayoutのコンストラクタにて行う)
		if (stopReelCount < reelNum)
			slotData.collectionManager.JudgeCollection(subROM.Collection, slotData.reelData, mainROM.ReelArray, slotData.valManager, slotData.basicData, false);
		
		// フリーズ取得
		if (stopReelCount == 1) reelFreezeTime += slotData.freezeManager.GetFreeze(SlotMaker2022.LocalDataSet.FreezeControlData.FreezeTiming.Stop1st);
		if (stopReelCount == 2) reelFreezeTime += slotData.freezeManager.GetFreeze(SlotMaker2022.LocalDataSet.FreezeControlData.FreezeTiming.Stop2nd);
		if (reelFreezeTime > 0){
			timer.GetTimer("reelPushFreeze").Activate();
		}
	}
}

// 停止後入賞役判定 & 払い出し
public class SCJudgeAndPayout : ISlotControllerBase {
	// 変数
	int mPayoutNum;		// 描画払出枚数(-1:rep)
	int mNextPayTime;	// 次回払出描画時間[ms]
	int mNextPayBase;	// 払出間隔[ms]
	int mFreezeBefore;	// 払出前フリーズ時間
	int mFreezeAfter;	// 払出後フリーズ時間(リプレイ時は払出前と同時作動)
	
	SlotMaker2022.main_function.MainReelManager reelManager;	// リール制御クラス
	
	// 使用Singleton
	SlotMaker2022.MainROMDataManagerSingleton mainROM;
	SlotEffectMaker2023.Action.SlotTimerManager timer;
	SlotEffectMaker2023.Singleton.SlotDataSingleton slotData;
	SlotEffectMaker2023.Singleton.EffectDataManagerSingleton subROM;

	// SC移行時処理(20240816ADD: Steam実績確認)
	public SCJudgeAndPayout(Action pSteamAPICallBack){
		// 変数初期化
		mPayoutNum   = 0;
		mNextPayTime = 0;
		mFreezeBefore = 0;
		mFreezeAfter = 0;
		
		// Singleton取得
		mainROM  = SlotMaker2022.MainROMDataManagerSingleton.GetInstance();
		slotData = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance();
		timer    = slotData.timerData;
		subROM   = SlotEffectMaker2023.Singleton.EffectDataManagerSingleton.GetInstance();
		
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
		
		// タイマ処理：フリーズ有無で最初に稼働させるタイマを変える。ただしbeforePayFreezeは強制稼働。
		timer.GetTimer("beforePayFreeze").Activate();
		if (mFreezeBefore > 0){
			// リプレイ時はafterも同時稼働させる
			if (slotData.basicData.isReplay) timer.GetTimer("afterPayFreeze").Activate();
		} else {
			if(mPayoutNum != 0) timer.GetTimer("payoutTime").Activate();
			timer.GetTimer("Pay-Bet").Activate();
			timer.GetTimer("Pay-Lever").Activate();
		}
		
		// 実績確認
		if (pSteamAPICallBack != null) pSteamAPICallBack();
		
		// フラグカウントを行う
		var FlagCounterCond = subROM.CounterCond;
		var EfCond = subROM.Timeline.GetCondFromName(FlagCounterCond.CountCond);
		if (EfCond.Evaluate()){
			var vm = slotData.valManager;
			var basicData = slotData.basicData;
			foreach (var item in FlagCounterCond.elemData) {
				if (!(basicData.castFlag >= item.FlagMin && basicData.castFlag <= item.FlagMax)) continue;
				var addFor = vm.GetVariable(item.OutVar);
				addFor.val += 1;
			}
		}
	}
	
	// SC移行時処理(Steam実績確認なし)
	public SCJudgeAndPayout() : this(null) { /* None */ }
	
	public void OnGetKeyDown(EGameButtonID pKeyID){ /* None */ }
	public void OnGetKey(EGameButtonID pKeyID){ /* None */ }
	public ISlotControllerBase ProcessAfterInput(Action pSaveCallBack, Action pSteamAPICallBack){
		const float divMS = 1000f;
		
		// beforeフリーズ消化判定
		if (timer.GetTimer("beforePayFreeze").isActivate){
			if (timer.GetTimer("beforePayFreeze").elapsedTime > (float)mFreezeBefore / divMS){
				// 払出開始処理
				if(mPayoutNum != 0) timer.GetTimer("payoutTime").Activate();
				timer.GetTimer("Pay-Bet").Activate();
				timer.GetTimer("Pay-Lever").Activate();
				timer.GetTimer("beforePayFreeze").SetDisabled();
				timer.GetTimer("afterPayFreeze").SetDisabled();
			}
		}
		
		// elapsedが値を持つ場合(=payoutTimeが有効かつbeforeフリーズ後)に処理を行う
		if (timer.GetTimer("payoutTime").isActivate){
			float elapsed = (float)timer.GetTimer("payoutTime").elapsedTime;
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
		if(mPayoutNum == 0 && !timer.GetTimer("beforePayFreeze").isActivate) {
			// 払出タイマを無効にする
			timer.GetTimer("payoutTime").SetDisabled();
			// afterフリーズがある場合は作動させる
			if (mFreezeAfter > 0) timer.GetTimer("afterPayFreeze").Activate();
			// なければモード移行する
			else return ModeChange(pSaveCallBack);
		}
		
		// afterフリーズ消化判定
		if (timer.GetTimer("afterPayFreeze").isActivate && !timer.GetTimer("beforePayFreeze").isActivate){
			// 時間の消化が完了したらモード移行する
			if (timer.GetTimer("afterPayFreeze").elapsedTime > (float)mFreezeAfter / divMS) return ModeChange(pSaveCallBack);
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
		
		// フリーズ抽選用に現在のモードを得る
		byte lastMode = basicData.gameMode;
		byte lastRT = basicData.RTMode;
		
		// 配当をbasicDataに転送する。戻り値として払出枚数を受ける
		mPayoutNum = basicData.SetPayout(castResult, mainROM.CastCommonData, slotData.historyManager, slotData.reelData, slotData.valManager);
		// モードとRTの変更処理を行う。変更された場合はタイマを作動させる。
		// SetPayoutより後で呼び出すことで当該Gのゲーム数減算をさせない。
		basicData.ModeChange(castResult, mainROM.CastCommonData, mainROM.RTCommonData, mainROM.RTMoveData, timer, slotData.historyManager, slotData.collectionManager, slotData.valManager);
		// モード移行処理(終了側)を行う
		slotData.basicData.ModeReset(mainROM.CastCommonData, mainROM.RTCommonData, mainROM.RTMoveData, timer, mPayoutNum < 0 ? 0 : mPayoutNum, slotData.historyManager);
		// gameModeを更新した後に全停止時のリーチ目コレクション処理を行う(モード変化時にはマスクをかける)
		slotData.collectionManager.JudgeCollection(subROM.Collection, slotData.reelData, mainROM.ReelArray, slotData.valManager, basicData, lastMode != basicData.gameMode);
		// 1G内の達成状況をリセットする
		slotData.collectionManager.EndGame();
		
		// フリーズ抽選
		slotData.freezeManager.SetFreezeMode(mainROM.FreezeControlData, mainROM.FreezeTimeData, lastMode, basicData.gameMode);
		slotData.freezeManager.SetFreezeRT(mainROM.FreezeControlData, mainROM.FreezeTimeData, lastRT, basicData.RTMode);
		// フリーズ取得
		mFreezeBefore = slotData.freezeManager.GetFreeze(SlotMaker2022.LocalDataSet.FreezeControlData.FreezeTiming.BeforePay);
		int afterWait = slotData.freezeManager.GetFreeze(SlotMaker2022.LocalDataSet.FreezeControlData.FreezeTiming.AfterPay);
		if (slotData.basicData.isReplay) mFreezeBefore += afterWait;
		else mFreezeAfter = afterWait;
		// Debug.Log("b: " + mFreezeBefore.ToString() + " / a: " + mFreezeAfter.ToString());
	}
	
	// モード移行時処理
	private ISlotControllerBase ModeChange(Action pSaveCallBack){
		// 移行前にグラフを記録する
		slotData.historyManager.OnPayoutEnd(slotData.basicData);
		// データをセーブする
		pSaveCallBack();
		// BETに処理を移す
		return new SCWaitBet();
	}
}
