using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// ゲーム内のボタンIDを定義する列挙型です。
/// </summary>
public enum EGameButtonID
{
	/// <summary>最大BET&スタートボタン</summary>
	eMaxBetAndStart,
	/// <summary>1BETボタン</summary>
	e1Bet,
	/// <summary>最大BETボタン</summary>
	eMaxBet,
	/// <summary>第1リール停止ボタン</summary>
	e1Reel,
	/// <summary>第2リール停止ボタン</summary>
	e2Reel,
	/// <summary>第3リール停止ボタン</summary>
	e3Reel,
	/// <summary>ボタン最大数</summary>
	eButtonMax
}

/// <summary>
/// スロットコントローラの基本インターフェースです。
/// 入力イベントと入力後の定常処理を実装し、次フレームで使用するコントローラを返します。
/// </summary>
public interface ISlotControllerBase
{
	// 初めてボタンが押された場合(GetKeyDown)の処理
	/// <summary>
	/// ボタンが押された瞬間に呼び出される処理を実装します。
	/// </summary>
	/// <param name="pKeyID">押されたボタンのID</param>
	void OnGetKeyDown(EGameButtonID pKeyID);

	// 1フレーム目を含め、キーが押され続けている場合(GetKey)の処理
	/// <summary>
	/// ボタンが押され続けている間に毎フレーム呼び出される処理を実装します。
	/// </summary>
	/// <param name="pKeyID">押され続けているボタンのID</param>
	void OnGetKey(EGameButtonID pKeyID);

	// 定常処理、キー入力を受け付けた後に実施する
	// 戻値：次フレームの処理に使用するインスタンス(変更なしの場合this)
	/// <summary>
	/// 入力処理後の定常処理を実装します。
	/// </summary>
	/// <param name="pSaveCallBack">データ保存用のコールバック</param>
	/// <param name="pSteamAPICallBack">Steam API 呼び出し用のコールバック</param>
	/// <returns>次フレームで使用するコントローラインスタンス</returns>
	ISlotControllerBase ProcessAfterInput(Action pSaveCallBack, Action pSteamAPICallBack);
}

// BET入力を待つ状態
/// <summary>
/// BET入力待機フェーズを管理するクラスです。
/// BET数の増減、リプレイ対応、レバー起動条件、タイマ制御を行います。
/// </summary>
public class SCWaitBet : ISlotControllerBase
{
	// 定数
	const int BET_SPAN_BASIC = 75;
	const int BET_SPAN_REP = 150;

	byte applyBet;      // 適用先BET数
	int nextAddBetTime; // 次回BET加算時間[ms], 無効時:-1
	bool reelActivate;  // リール起動フラグ

	// 使用Singleton
	SlotMaker2022.MainROMDataManagerSingleton mainROM;
	SlotEffectMaker2023.Action.SlotTimerManager timer;
	SlotEffectMaker2023.Singleton.SlotDataSingleton slotData;

	/// <summary>
	/// コンストラクタ。applyBetを初期化し、リプレイ時の前回BET反映、BET待機タイマを起動します。
	/// </summary>
	public SCWaitBet()
	{
		applyBet = 0;
		nextAddBetTime = -1;
		reelActivate = false;

		// Singleton取得
		mainROM = SlotMaker2022.MainROMDataManagerSingleton.GetInstance();
		slotData = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance();
		timer = slotData.timerData;

		// リプレイ時は前回BET数を即適用
		if (slotData.basicData.isReplay) applyBet = slotData.basicData.betCount;

		// 前回BET状況をクリア
		slotData.basicData.ClearBetCount();

		// BET待機タイマ起動
		timer.GetTimer("betWait").Activate();

		SetBetTime();
	}

	/// <summary>
	/// 目標BET数に応じてBET入力タイマを設定し、必要に応じて状態をリセットします。
	/// </summary>
	private void SetBetTime()
	{
		// BET数変化なしなら何もしない
		if (slotData.basicData.betCount == applyBet) return;

		nextAddBetTime = 0;
		timer.GetTimer("leverAvailable").SetDisabled();
		timer.GetTimer("Pay-Bet").SetDisabled();
		timer.GetTimer("betInput").Activate();
		timer.GetTimer("betInput").Reset();

		// 前回BET数より小さくなった場合はクリア
		if (applyBet < slotData.basicData.betCount)
			slotData.basicData.ClearBetCount();
	}

	/// <summary>
	/// ボタンが押された瞬間の処理を実装します。
	/// レバー操作やBET数変更を行います。
	/// </summary>
	/// <param name="pKeyID">押されたボタンのID</param>
	public void OnGetKeyDown(EGameButtonID pKeyID)
	{
		const int betMax = SlotMaker2022.LocalDataSet.BET_MAX;

		// BET加算中は入力を無視
		if (!(nextAddBetTime < 0)) return;

		// レバー起動処理
		if (pKeyID == EGameButtonID.eMaxBetAndStart && applyBet > 0 && timer.GetTimer("leverAvailable").isActivate)
		{
			reelActivate = true;
			return;
		}

		// リプレイ以外はBET入力を受け付け
		if (!slotData.basicData.isReplay)
		{
			// モードごとに使用可能な最大BETを検索
			byte currentMaxBet = 0;
			for (int betC = betMax; betC > 0; --betC)
			{
				if (CheckBet((byte)betC))
				{
					currentMaxBet = (byte)betC;
					break;
				}
			}

			// MaxBet設定
			if (pKeyID == EGameButtonID.eMaxBetAndStart || pKeyID == EGameButtonID.eMaxBet)
				applyBet = currentMaxBet;

			// 1BET増加
			if (pKeyID == EGameButtonID.e1Bet)
			{
				applyBet += 1;
				if (applyBet > currentMaxBet)
					applyBet = 1;
			}
		}

		// BET変更時にタイマ再設定
		SetBetTime();
	}

	/// <summary>
	/// 押し続け中の処理は不要のため実装なし。
	/// </summary>
	/// <param name="pKeyID">押され続けているボタンのID</param>
	public void OnGetKey(EGameButtonID pKeyID) { }

	/// <summary>
	/// 入力後の定常処理を実装します。
	/// BETショットカウントやリール起動への移行を行います。
	/// </summary>
	/// <param name="pSaveCallBack">データ保存用コールバック</param>
	/// <param name="pSteamAPICallBack">Steam API 呼び出し用コールバック</param>
	/// <returns>次フレームで使用するコントローラインスタンス</returns>
	public ISlotControllerBase ProcessAfterInput(Action pSaveCallBack, Action pSteamAPICallBack)
	{
		// リール起動条件満たす場合
		if (reelActivate)
		{
			// 関連タイマ停止
			timer.GetTimer("betWait").SetDisabled();
			timer.GetTimer("betInput").SetDisabled();
			timer.GetTimer("betShot").SetDisabled();
			timer.GetTimer("leverAvailable").SetDisabled();
			timer.GetTimer("Pay-Lever").SetDisabled();

			// 次フェーズへ
			return new SCWaitBeforeReelStart();
		}

		// BET入力タイマが動作中のみカウント
		if (!timer.GetTimer("betInput").isActivate)
			return this;
		// タイマの時刻を取得してBET加算処理を行う
		float betInput = (float)timer.GetTimer("betInput").elapsedTime;
		while (nextAddBetTime >= 0 && betInput > (float)nextAddBetTime / 1000f)
		{
			slotData.basicData.AddBetCount();
			// タイマを更新する
			timer.GetTimer("betShot")?.Activate();
			timer.GetTimer("betShot")?.Reset();
			// BET終了判定
			if (slotData.basicData.betCount == applyBet)
			{
				// BET処理終了
				nextAddBetTime = -1;
				// レバー有効かの判定、有効ならタイマを作動させる
				if (CheckBet(applyBet)) timer.GetTimer("leverAvailable").Activate();
			}
			else
			{
				// 次BETの時間取得
				nextAddBetTime += slotData.basicData.isReplay ? BET_SPAN_REP : BET_SPAN_BASIC;
			}
		}

		return this;
	}

	/// <summary>
	/// 現在のモードで指定BET数が使用可能か判定します。
	/// </summary>
	/// <param name="betNum">判定するBET数</param>
	/// <returns>使用可能なら true</returns>
	private bool CheckBet(byte betNum)
	{
		byte currentGameMode = slotData.basicData.gameMode;
		const int betMaxVal = SlotMaker2022.LocalDataSet.BET_MAX;
		uint index = (uint)(betMaxVal * currentGameMode + betNum - 1);
		return mainROM.CastCommonData.CanUseBet.GetData(index) != 0;
	}
}

/// <summary>
/// レバー操作後、リール始動までの待機フェーズを管理します。
/// フラグ抽選、履歴更新、タイマ制御を行います。
/// </summary>
public class SCWaitBeforeReelStart : ISlotControllerBase
{
	// 定数
	const int WAIT_MAX = 4100; // 最大待機時間[ms]
	int waitTime;

	// 使用Singleton
	SlotMaker2022.MainROMDataManagerSingleton mainROM;
	SlotEffectMaker2023.Action.SlotTimerManager timer;
	SlotEffectMaker2023.Singleton.SlotDataSingleton slotData;

	/// <summary>
	/// コンストラクタ。
	/// シングルトン取得、タイマ起動、BET確定履歴処理、乱数抽選を実施します。
	/// </summary>
	public SCWaitBeforeReelStart()
	{
		// Singleton取得
		mainROM = SlotMaker2022.MainROMDataManagerSingleton.GetInstance();
		slotData = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance();
		timer = slotData.timerData;

		// タイマ起動
		var sys = slotData.sysData;
		const int WAITCUT_TIME = 10;
		timer.GetTimer("waitStart").Activate();
		waitTime = sys.WaitCut ? WAITCUT_TIME : WAIT_MAX;

		// BET確定処理
		slotData.basicData.LatchBet();
		// 履歴処理
		slotData.historyManager.ReelStart();

		// 乱数抽選処理
		SetCastFlag();
	}

	/// <inheritdoc/>
	public void OnGetKeyDown(EGameButtonID pKeyID)
	{
		// 入力なし
	}

	/// <inheritdoc/>
	public void OnGetKey(EGameButtonID pKeyID)
	{
		// 入力なし
	}

	/// <summary>
	/// 待機終了判定を行い、条件を満たせば次フェーズ(SCReelOperation)へ移行します。
	/// </summary>
	/// <param name="pSaveCallBack">データ保存コールバック</param>
	/// <param name="pSteamAPICallBack">Steam API呼び出しコールバック</param>
	/// <returns>次フレームで使用するコントローラインスタンス</returns>
	public ISlotControllerBase ProcessAfterInput(Action pSaveCallBack, Action pSteamAPICallBack)
	{
		bool waitEnd = !timer.GetTimer("waitEnd").isActivate
			|| timer.GetTimer("waitEnd").elapsedTime > waitTime / 1000f;

		if (waitEnd)
		{
			// タイマクリアして次フェーズへ
			timer.GetTimer("waitStart").SetDisabled();
			timer.GetTimer("waitEnd").Activate();
			timer.GetTimer("waitEnd").Reset();
			return new SCReelOperation();
		}

		return this;
	}

	/// <summary>
	/// フラグ抽選処理を行います。
	/// 条件に応じて乱数を減算し、確定フラグを設定します。
	/// </summary>
	private void SetCastFlag()
	{
		// 乱数取得
		int randValue = UnityEngine.Random.Range(0, SlotMaker2022.LocalDataSet.FlagCommonData.RAND_MAX);

		// 初期化
		var basicData = slotData.basicData;
		var randList = mainROM.FlagRandData;
		byte castFlag = 0;
		byte bonusFlag = 0;
		byte lastBonus = basicData.bonusFlag;
		byte lastRT = basicData.RTMode;

		// 条件に沿って乱数減算
		foreach (var randItem in randList)
		{
			if (randItem.CondBet + 1 != basicData.betCount) continue;
			if (randItem.CondGameMode != basicData.gameMode) continue;
			if (randItem.CondRTMode != basicData.RTMode && randItem.CondRTMode < SlotMaker2022.LocalDataSet.RTMODE_MAX) continue;

			uint index = randItem.CommonSet ? 0u : (uint)basicData.slotSetting;
			randValue -= (int)randItem.RandValue.GetData(index);
			if (randValue < 0)
			{
				castFlag = randItem.LaunchFlagID;
				bonusFlag = randItem.BonusFlag;
				break;
			}
		}

		// フラグ設定
		basicData.SetCastFlag(bonusFlag, castFlag, mainROM.CastCommonData, mainROM.RTCommonData, timer);
		slotData.freezeManager.SetFreezeFlag(mainROM.FreezeControlData, mainROM.FreezeTimeData, castFlag, bonusFlag != lastBonus);
		slotData.freezeManager.SetFreezeRT(mainROM.FreezeControlData, mainROM.FreezeTimeData, lastRT, basicData.RTMode);

		// フリーズ待機時間追加
		int before = slotData.freezeManager.GetFreeze(SlotMaker2022.LocalDataSet.FreezeControlData.FreezeTiming.BeforeWait);
		waitTime += before;
		int after = slotData.freezeManager.GetFreeze(SlotMaker2022.LocalDataSet.FreezeControlData.FreezeTiming.AfterWait);
		if (after > 0)
		{
			if (!timer.GetTimer("waitEnd").isActivate)
			{
				timer.GetTimer("waitEnd").Activate();
				waitTime = after;
			}
			else
			{
				int over = (int)(timer.GetTimer("waitEnd").elapsedTime * 1000f) - waitTime;
				waitTime = Math.Max(0, over + after);
			}
		}
	}
}


/// <summary>
/// リール回転中の処理フェーズを管理します。
/// リール停止入力、各リールの停止判定、タイマ管理、次フェーズへの移行を行います。
/// </summary>
public class SCReelOperation : ISlotControllerBase
{
	// 定数
	const int reelNum = SlotMaker2022.LocalDataSet.REEL_MAX;
	const int comaNum = SlotMaker2022.LocalDataSet.COMA_MAX;

	// 変数
	bool isAllReleased;          // 停止ボタン押下解除状態か
	int[] stopHistory;           // 各リールの停止履歴
	int[] stopOrder;             // リール停止順
	int stop1st;                 // 第1停止インデックス
	int slip1st;                 // 第1停止すべり数
	int stopReelCount;           // 停止済みリール数
	int reelFreezeTime;          // フリーズ時間[ms]

	SlotMaker2022.main_function.MainReelManager reelManager; // リール制御クラス

	// 使用Singleton
	SlotMaker2022.MainROMDataManagerSingleton mainROM;
	SlotEffectMaker2023.Action.SlotTimerManager timer;
	SlotEffectMaker2023.Singleton.SlotDataSingleton slotData;
	SlotEffectMaker2023.Singleton.EffectDataManagerSingleton subROM;

	/// <summary>
	/// コンストラクタ。
	/// シングルトン取得後、リール制御初期化、全リール始動、タイマ起動を実施します。
	/// </summary>
	public SCReelOperation()
	{
		// Singleton取得
		mainROM = SlotMaker2022.MainROMDataManagerSingleton.GetInstance();
		slotData = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance();
		timer = slotData.timerData;
		subROM = SlotEffectMaker2023.Singleton.EffectDataManagerSingleton.GetInstance();

		// 制御用変数を初期化
		isAllReleased = false;
		stopHistory = new int[reelNum];
		stopOrder = new int[reelNum];
		for (int i = 0; i < reelNum; ++i)
		{
			stopHistory[i] = -1;
			stopOrder[i] = -1;
		}
		stop1st = reelNum * comaNum;
		slip1st = -1;
		stopReelCount = 0;
		reelFreezeTime = 0;

		// リール制御クラス初期化
		reelManager = new SlotMaker2022.main_function.MainReelManager();

		// 全リールを始動
		for (int i = 0; i < reelNum; ++i)
			slotData.reelData[i].Start();

		// タイマ初期化
		timer.GetTimer("reelStart").Activate();
		ResetPushStopTimer();
	}

	/// <inheritdoc/>
	public void OnGetKeyDown(EGameButtonID pKeyID)
	{
		// リール停止入力
		if (pKeyID == EGameButtonID.e1Reel) StopReel(0);
		if (pKeyID == EGameButtonID.e2Reel) StopReel(1);
		if (pKeyID == EGameButtonID.e3Reel) StopReel(2);
	}

	/// <inheritdoc/>
	public void OnGetKey(EGameButtonID pKeyID)
	{
		// ねじり解除
		isAllReleased = false;
	}

	/// <summary>
	/// リール処理を実行し、全停止完了時に次フェーズへ移行判定を行います。
	/// </summary>
	/// <param name="pSaveCallBack">データ保存用コールバック</param>
	/// <param name="pSteamAPICallBack">Steam API呼び出しコールバック</param>
	/// <returns>次フレームで使用するコントローラインスタンス</returns>
	public ISlotControllerBase ProcessAfterInput(Action pSaveCallBack, Action pSteamAPICallBack)
	{
		bool isAllStopped = true;
		for (int i = 0; i < reelNum; ++i)
		{
			var tm = timer.GetTimer("reelStart");
			var checkReel = slotData.reelData[i];
			float elapsed = (tm.elapsedTime - tm.lastElapsed) ?? 0f;
			checkReel.Process(elapsed);
			isAllStopped &= !checkReel.isRotate;

			if (!checkReel.isRotate)
			{
				var t = timer.GetTimer($"reelStopPos[{i}]");
				if (!t.isActivate)
				{
					t.Activate();
					timer.GetTimer("anyReelStop").Activate();
					timer.GetTimer("anyReelStop").Reset();
				}
				int orderPos;
				for (orderPos = 0; orderPos < reelNum; ++orderPos)
					if (stopOrder[orderPos] == i) break;
				t = timer.GetTimer($"reelStopOrder[{orderPos}]");
				if (!t.isActivate) t.Activate();
			}
		}

		if (isAllStopped && isAllReleased && !timer.GetTimer("reelPushFreeze").isActivate)
		{
			timer.GetTimer("allReelStop").Activate();
			timer.GetTimer("reelStart").SetDisabled();
			ResetPushStopTimer();
			return new SCJudgeAndPayout(pSteamAPICallBack);
		}

		if (timer.GetTimer("reelPushFreeze").isActivate &&
			timer.GetTimer("reelPushFreeze").elapsedTime > reelFreezeTime / 1000f)
		{
			reelFreezeTime = 0;
			timer.GetTimer("reelPushFreeze").SetDisabled();
		}

		isAllReleased = true;
		return this;
	}

	/// <summary>
	/// 全リールのプッシュ/ストップタイマをリセットします。
	/// </summary>
	private void ResetPushStopTimer()
	{
		timer.GetTimer("anyReelPush").SetDisabled();
		timer.GetTimer("anyReelStop").SetDisabled();
		timer.GetTimer("allReelStop").SetDisabled();
		for (int i = 0; i < reelNum; ++i)
		{
			timer.GetTimer($"reelPushPos[{i}]").SetDisabled();   // 特定リール[0-reelMax)停止ボタン押下からの定義時間
			timer.GetTimer($"reelStopPos[{i}]").SetDisabled();   // 特定リール[0-reelMax)停止からの定義時間
			timer.GetTimer($"reelPushOrder[{i}]").SetDisabled(); // 第n停止ボタン押下からの定義時間
			timer.GetTimer($"reelStopOrder[{i}]").SetDisabled(); // 第n停止からの定義時間
		}
	}

	/// <summary>
	/// 指定リールを停止させます。
	/// リール制御からすべり数を算出し、停止命令を発行します。
	/// </summary>
	/// <param name="reelIndex">停止対象のリール番号</param>
	private void StopReel(int reelIndex)
	{
		var stopReel = slotData.reelData[reelIndex];
		// リールが停止制御できる状態にない場合処理をしない
		if (!stopReel.CanStop()) return;
		// フリーズ中は停止処理をかけない
		if (timer.GetTimer("reelPushFreeze").isActivate) return;

		// リール制御データからすべりコマ数を算出する
		var bs = slotData.basicData;
		// 今回停止するリールにはstopHistoryにpushPosを入れて計算する
		stopHistory[reelIndex] = stopReel.GetReelComaIDFixed();
		int slipNum = reelManager.GetReelControl3R(reelIndex, stopHistory, stop1st, slip1st,
			bs.betCount - 1, bs.gameMode, bs.bonusFlag, bs.castFlag);

		stopReel.SetStopPos(slipNum, stopReelCount);
		stopHistory[reelIndex] = stopReel.stopPos;
		stopOrder[stopReelCount] = reelIndex;
		if (stopReelCount == 0)
		{
			stop1st = reelIndex * comaNum + stopReel.stopPos;
			slip1st = stopReel.slipCount;
		}

		// テンパイ判定(=入賞可能性のあるボーナス判定)を行う
		// 配当をmanagerから得る(フラグ関係は指定なし:-1)
		var castResult = reelManager.GetCast(stopHistory, bs.betCount - 1, bs.gameMode, -1, -1);
		bs.CheckTenpai(castResult, mainROM.CastCommonData);

		// タイマ処理を行う
		timer.GetTimer("anyReelPush").Activate();
		timer.GetTimer("anyReelPush").Reset();
		timer.GetTimer($"reelPushPos[{reelIndex}]").Activate();
		timer.GetTimer($"reelPushOrder[{stopReelCount}]").Activate();

		// ストップ数加算
		stopReelCount++;

		// リーチ目判定(ただし全停止後の判定はモード移行終了後:SCJudgeAndPayoutのコンストラクタにて行う)
		if (stopReelCount < reelNum)
			slotData.collectionManager.JudgeCollection(subROM.Collection,
				slotData.reelData, mainROM.ReelArray,
				slotData.valManager, slotData.basicData, false);

		// フリーズ取得
		if (stopReelCount == 1)
			reelFreezeTime += slotData.freezeManager.GetFreeze(
				SlotMaker2022.LocalDataSet.FreezeControlData.FreezeTiming.Stop1st);
		if (stopReelCount == 2)
			reelFreezeTime += slotData.freezeManager.GetFreeze(
				SlotMaker2022.LocalDataSet.FreezeControlData.FreezeTiming.Stop2nd);
		if (reelFreezeTime > 0)
			timer.GetTimer("reelPushFreeze").Activate();
	}
}

/// <summary>
/// 停止後の入賞役判定と払い出しフェーズを管理します。
/// 配当取得、フリーズ・払い出しタイマ制御、Steam実績確認、モード移行を実施します。
/// </summary>
public class SCJudgeAndPayout : ISlotControllerBase
{
	// 変数
	int mPayoutNum;    // 描画払出枚数(-1:rep)
	int mNextPayTime;  // 次回払出描画時間[ms]
	int mNextPayBase;  // 払出間隔[ms]
	int mFreezeBefore; // 払出前フリーズ時間[ms]
	int mFreezeAfter;  // 払出後フリーズ時間[ms] (リプレイ時はbeforeと同時)

	SlotMaker2022.main_function.MainReelManager reelManager; // リール制御クラス

	// 使用Singleton
	SlotMaker2022.MainROMDataManagerSingleton mainROM;
	SlotEffectMaker2023.Action.SlotTimerManager timer;
	SlotEffectMaker2023.Singleton.SlotDataSingleton slotData;
	SlotEffectMaker2023.Singleton.EffectDataManagerSingleton subROM;

	/// <summary>
	/// コンストラクタ。
	/// Steam実績確認用コールバックを受け取り、初期処理を実行します。
	/// </summary>
	/// <param name="pSteamAPICallBack">Steam API 実績確認用コールバック</param>
	public SCJudgeAndPayout(Action pSteamAPICallBack)
	{
		// 変数初期化
		mPayoutNum = 0;
		mNextPayTime = 0;
		mFreezeBefore = 0;
		mFreezeAfter = 0;

		// Singleton取得
		mainROM = SlotMaker2022.MainROMDataManagerSingleton.GetInstance();
		slotData = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance();
		timer = slotData.timerData;
		subROM = SlotEffectMaker2023.Singleton.EffectDataManagerSingleton.GetInstance();

		// リール制御クラス初期化
		reelManager = new SlotMaker2022.main_function.MainReelManager();

		// 配当取得処理
		GetCast();

		// 払出間隔設定
		var castCommon = mainROM.CastCommonData;
		mNextPayBase = (int)castCommon.IntervalPay; // 払出待ち時間
		if (slotData.basicData.isReplay)
		{
			mNextPayTime = (int)castCommon.IntervalRep; // リプレイ待ち時間
		}

		// タイマ処理：beforePayFreezeは常時稼働、afterは必要時のみ
		timer.GetTimer("beforePayFreeze").Activate();
		if (mFreezeBefore > 0 && slotData.basicData.isReplay)
		{
			timer.GetTimer("afterPayFreeze").Activate();
		}
		else
		{
			if (mPayoutNum != 0) timer.GetTimer("payoutTime").Activate();
			timer.GetTimer("Pay-Bet").Activate();
			timer.GetTimer("Pay-Lever").Activate();
		}

		// 実績確認
		pSteamAPICallBack?.Invoke();

		// フラグカウント
		var flagCond = subROM.CounterCond;
		var efCond = subROM.Timeline.GetCondFromName(flagCond.CountCond);
		if (efCond.Evaluate())
		{
			var vm = slotData.valManager;
			var bd = slotData.basicData;
			foreach (var item in flagCond.elemData)
			{
				if (bd.castFlag < item.FlagMin || bd.castFlag > item.FlagMax) continue;
				vm.GetVariable(item.OutVar).val++;
			}
		}
	}

	/// <summary>
	/// コンストラクタ。
	/// Steam実績確認なしのバージョンを呼び出します。
	/// </summary>
	public SCJudgeAndPayout() : this(null) { }

	/// <inheritdoc/>
	public void OnGetKeyDown(EGameButtonID pKeyID)
	{
		/* None */
	}

	/// <inheritdoc/>
	public void OnGetKey(EGameButtonID pKeyID)
	{
		/* None */
	}

	/// <summary>
	/// 払い出しおよびフリーズタイマ処理を行い、完了後はBETフェーズへ移行します。
	/// </summary>
	/// <param name="pSaveCallBack">データ保存用コールバック</param>
	/// <param name="pSteamAPICallBack">Steam API コールバック</param>
	/// <returns>次フレームで使用するコントローラインスタンス</returns>
	public ISlotControllerBase ProcessAfterInput(Action pSaveCallBack, Action pSteamAPICallBack)
	{
		const float divMS = 1000f;

		// before フリーズ消化判定
		if (timer.GetTimer("beforePayFreeze").isActivate &&
			timer.GetTimer("beforePayFreeze").elapsedTime > mFreezeBefore / divMS)
		{
			if (mPayoutNum != 0) timer.GetTimer("payoutTime").Activate();
			timer.GetTimer("Pay-Bet").Activate();
			timer.GetTimer("Pay-Lever").Activate();
			timer.GetTimer("beforePayFreeze").SetDisabled();
			timer.GetTimer("afterPayFreeze").SetDisabled();
		}

		// 払い出し処理
		if (timer.GetTimer("payoutTime").isActivate)
		{
			var elapsed = timer.GetTimer("payoutTime").elapsedTime;
			if (mPayoutNum < 0 && elapsed > mNextPayTime / divMS) mPayoutNum = 0;
			while (mPayoutNum > 0 && elapsed > mNextPayTime / divMS)
			{
				slotData.basicData.AddPayout();
				mPayoutNum--;
				mNextPayTime += mNextPayBase;
			}
		}

		// 払い出し完了判定
		if (mPayoutNum == 0 && !timer.GetTimer("beforePayFreeze").isActivate)
		{
			timer.GetTimer("payoutTime").SetDisabled();
			if (mFreezeAfter > 0) timer.GetTimer("afterPayFreeze").Activate();
			else return ModeChange(pSaveCallBack);
		}

		// after フリーズ消化判定
		if (timer.GetTimer("afterPayFreeze").isActivate &&
			!timer.GetTimer("beforePayFreeze").isActivate &&
			timer.GetTimer("afterPayFreeze").elapsedTime > mFreezeAfter / divMS)
		{
			return ModeChange(pSaveCallBack);
		}

		return this;
	}

	/// <summary>
	/// 配当取得を行い、BasicDataへの反映、モード変化、フリーズ設定を行います。
	/// </summary>
	private void GetCast()
	{
		// 各リールの座標を得る
		const int reelNumConst = SlotMaker2022.LocalDataSet.REEL_MAX;
		int[] reelPos = new int[reelNumConst];
		for (int i = 0; i < reelNumConst; i++)
			reelPos[i] = slotData.reelData[i].GetReelComaIDFixed();

		// 配当をmanagerから得る(フラグ関係は指定なし:-1)
		var bd = slotData.basicData;
		var castRes = reelManager.GetCast(reelPos, bd.betCount - 1, bd.gameMode, -1, -1);

		// フリーズ抽選用に現在のモードを得る
		byte lastMode = bd.gameMode;
		byte lastRT = bd.RTMode;

		// 配当をbasicDataに転送する。戻り値として払出枚数を受ける
		mPayoutNum = bd.SetPayout(castRes, mainROM.CastCommonData,
			slotData.historyManager, slotData.reelData, slotData.valManager);
		// モードとRTの変更処理を行う。変更された場合はタイマを作動させる。
		// SetPayoutより後で呼び出すことで当該Gのゲーム数減算をさせない。
		bd.ModeChange(castRes, mainROM.CastCommonData, mainROM.RTCommonData,
			mainROM.RTMoveData, timer, slotData.historyManager, slotData.collectionManager, slotData.valManager);
		// モード移行処理(終了側)を行う
		bd.ModeReset(mainROM.CastCommonData, mainROM.RTCommonData,
			mainROM.RTMoveData, timer, Mathf.Max(0, mPayoutNum), slotData.historyManager);
		// gameModeを更新した後に全停止時のリーチ目コレクション処理を行う(モード変化時にはマスクをかける)
		slotData.collectionManager.JudgeCollection(subROM.Collection,
			slotData.reelData, mainROM.ReelArray,
			slotData.valManager, bd, lastMode != bd.gameMode);
		// 1G内の達成状況をリセットする
		slotData.collectionManager.EndGame();

		// フリーズ抽選
		slotData.freezeManager.SetFreezeMode(mainROM.FreezeControlData,
			mainROM.FreezeTimeData, lastMode, bd.gameMode);
		slotData.freezeManager.SetFreezeRT(mainROM.FreezeControlData,
			mainROM.FreezeTimeData, lastRT, bd.RTMode);
		// フリーズ取得

		mFreezeBefore = slotData.freezeManager.GetFreeze(
			SlotMaker2022.LocalDataSet.FreezeControlData.FreezeTiming.BeforePay);
		int afterWait = slotData.freezeManager.GetFreeze(
			SlotMaker2022.LocalDataSet.FreezeControlData.FreezeTiming.AfterPay);
		if (bd.isReplay) mFreezeBefore += afterWait;
		else mFreezeAfter = afterWait;
	}

	/// <summary>
	/// モード移行処理を行い、BETフェーズへ戻します。
	/// </summary>
	/// <param name="pSaveCallBack">データ保存コールバック</param>
	/// <returns>BET待機フェーズのコントローラ</returns>
	private ISlotControllerBase ModeChange(Action pSaveCallBack)
	{
		// 移行前にグラフを記録する
		slotData.historyManager.OnPayoutEnd(slotData.basicData);
		// データをセーブする
		pSaveCallBack();
		// BETに処理を移す
		return new SCWaitBet();
	}
}
