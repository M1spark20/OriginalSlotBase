using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class SlotBasicData : SlotMaker2022.ILocalDataInterface
{
	// 定数
	public const byte CREDIT_MAX = 50;

	public byte slotSetting { get; private set; }	// 設定
	
	public uint inCount { get; private set; }		// IN枚数
	public uint outCount { get; private set; }		// OUT枚数
	
	public byte betCount { get; private set; }		// 現在のBET数
	public byte creditShow { get; private set; }	// 現在のクレジット数
	public byte payoutShow { get; private set; }	// 現在の払い出し数
	public bool isBetLatched { get; private set; }	// BETを用いてゲームを開始したか
	public bool isReplay { get; private set; }		// 入賞役がリプレイか
	
	public byte gameMode { get; private set; }		// 現在のスロットのモード
	public byte modeGameCount { get; private set; }	// モード残りゲーム数
	public byte modeJacCount { get; private set; }	// モード残り入賞数
	public int  modeMedalCount { get; private set; }// モード残り入賞数(負数で設定なし)
	public byte RTMode { get; private set; }		// 現在のRTモード
	public bool RTOverride { get; private set; }	// RT上書き可否状態
	public int  RTGameCount { get; private set; }	// RT残りゲーム数(負数で無限)
	
	public byte bonusFlag { get; private set; }		// 現在成立中のボーナス
	public byte castFlag { get; private set; }		// 現在成立中の子役
	
	// 配当データ(使用するものだけ抽出)
	public SlotMaker2022.UserBaseData castLines { get; private set; }	// 配当入賞ライン
	public string                     castName  { get; private set; }	// 配当入賞した配当名
	
	public SlotBasicData(){
		slotSetting		=  5;
		inCount			=  0;
		outCount		=  0;
		betCount		=  0;
		creditShow		= 50;
		payoutShow		=  0;
		isBetLatched	= false;
		isReplay		= false;
		
		gameMode		=  0;
		modeGameCount	=  0;
		modeJacCount	=  0;
		modeMedalCount	= -1;
		
		RTMode			=  0;
		RTOverride		= true;
		RTGameCount		= -1;
		
		bonusFlag		=  0;
		castFlag		=  0;
		
		const int lineNum = SlotMaker2022.LocalDataSet.PAYLINE_MAX;
		castLines   = new SlotMaker2022.UserBaseData(1, true, lineNum);
		castName    = string.Empty;
	}
	
	public bool StoreData(ref BinaryWriter fs, int version){
		fs.Write(inCount);
		fs.Write(outCount);
		fs.Write(betCount);
		fs.Write(creditShow);
		fs.Write(payoutShow);
		fs.Write(isBetLatched);
		fs.Write(isReplay);
		fs.Write(gameMode);
		fs.Write(modeGameCount);
		fs.Write(modeJacCount);
		fs.Write(modeMedalCount);
		fs.Write(RTMode);
		fs.Write(RTOverride);
		fs.Write(RTGameCount);
		fs.Write(bonusFlag);
		fs.Write(castFlag);
		fs.Write(decimal.ToByte(castLines.Export()));
		fs.Write(castName);
		return true;
	}
	public bool ReadData(ref BinaryReader fs, int version){
		inCount			= fs.ReadUInt32();
		outCount		= fs.ReadUInt32();
		betCount		= fs.ReadByte();
		creditShow		= fs.ReadByte();
		payoutShow		= fs.ReadByte();
		isBetLatched	= fs.ReadBoolean();
		isReplay		= fs.ReadBoolean();
		gameMode		= fs.ReadByte();
		modeGameCount	= fs.ReadByte();
		modeJacCount	= fs.ReadByte();
		modeMedalCount	= fs.ReadInt32();
		RTMode			= fs.ReadByte();
		RTOverride		= fs.ReadBoolean();
		RTGameCount		= fs.ReadInt32();
		bonusFlag		= fs.ReadByte();
		castFlag		= fs.ReadByte();
		castLines.Import(fs.ReadByte());
		castName    	= fs.ReadString();
		return true;
	}
	
	// 変数設定用メソッド
	// BETをクレジットから1転送する。BETLatchをfalse(=未消化)にする
	// リプレイの場合はクレジット数を減算しない
	public void AddBetCount(){
		++betCount;
		if(!isReplay) creditShow = (byte)Math.Max(0, creditShow - 1);
		payoutShow = 0;
		isBetLatched = false;
	}
	// BETをクリアする。ゲーム消化されていなければクレジットにメダルを戻す
	public void ClearBetCount(){
		if(!isBetLatched) creditShow = (byte)Math.Min(creditShow + betCount, CREDIT_MAX);
		betCount = 0;
	}
	// BETを用いてリールを回転させる
	public void LatchBet() {
		isBetLatched = true;				// BETを消化済みにする
		inCount += betCount;				// INメダル枚数を加算する
		if(isReplay) outCount += betCount;	// リプレイの場合、OUTメダル枚数も加算する
		// ゲーム数更新(あとで)
	}
	// フラグ設定
	public void SetCastFlag(byte pBonusFlag, byte pCastFlag){
		if (bonusFlag == 0 && pBonusFlag > 0){
			// ボーナス初当たり時はRT遷移を確認する
			var mainROM = SlotMaker2022.MainROMDataManagerSingleton.GetInstance();
			bonusFlag = pBonusFlag;
			uint hitRT = mainROM.RTCommonData.MoveByBonus.GetData((uint)(pBonusFlag - 1));
			if (hitRT < SlotMaker2022.LocalDataSet.RTMODE_MAX) SetRT((byte)hitRT, false, false);
		}
		castFlag = pCastFlag;
		Debug.Log("FlagSet: bonus->" + bonusFlag.ToString() + " cast->" + castFlag.ToString());
	}
	// 配当設定(必要なものだけ設定する)
	// ret: 当該配当での総払出枚数(0-max), replay(-1)
	public int SetPayout(SlotMaker2022.main_function.MainReelManager.GetCastResult castResult){
		// データリセット
		castLines.Import(0);
		castName = string.Empty;
		isReplay = false;
		int payCount = 0;
		
		// Singleton読込
		var mainROM = SlotMaker2022.MainROMDataManagerSingleton.GetInstance();
		
		for(int castC = 0; castC<castResult.payLine.Count; ++castC){
			// 入賞ラインと配当名を読み込む
			castLines.SetData((uint)castResult.payLine[castC], 1);
			castName += castResult.matchCast[castC].FlagName;
			
			// 配当読込(リプレイ・小役判定) payoutID == 0は配当なしとして処理しない
			uint payoutID = castResult.matchCast[castC].PayoutNumID.GetData((uint)(betCount - 1));
			if (payoutID > SlotMaker2022.LocalDataSet.CastCommonData.PAYOUT_DATA_MAX)
				isReplay = true;
			else if (payoutID > 0)
				payCount += (int)mainROM.CastCommonData.PayoutData.GetData((uint)(payoutID - 1));
		}
		
		// 払出枚数の丸め処理
		int maxPay = (int)mainROM.CastCommonData.MaxPayout.GetData((uint)(betCount - 1));
		payCount = Math.Min(maxPay, payCount);
		// デバッグ出力
		Debug.Log("SetCast: " + castLines.Export().ToString() + " - " + castName + " - " + payCount.ToString() + " - rep:" + isReplay.ToString());
		// モード/RTゲーム数減算
		if (RTGameCount   > 0) --RTGameCount;
		if (modeGameCount > 0) --modeGameCount;
		if (modeJacCount  > 0 && payCount > 0 && !isReplay) --modeJacCount;
		// replayなら-1、それ以外なら配当枚数を返す
		return isReplay ? -1 : payCount;
	}
	// 払出枚数カウントアップ
	public void AddPayout(){
		++outCount;
		++payoutShow;
		if (modeMedalCount > 0) --modeMedalCount;
		creditShow = (byte)Math.Min(creditShow + 1, CREDIT_MAX);
	}
	// モード移行処理(入賞による) modeChangeとRTChangeで状態変化結果を返す
	public void ModeChange(SlotMaker2022.main_function.MainReelManager.GetCastResult castResult){
		for(int castC = 0; castC<castResult.matchCast.Count; ++castC){
			var checkData = castResult.matchCast[castC];
			// モード更新とこれに伴うRT更新
			if(checkData.ChangeGameModeFlag) {
				bonusFlag = 0;
				SetMode(checkData.ChangeGameModeDest, checkData.BonusPayoutMaxID, checkData.BonusGameMaxID);
			}
			// 入賞に伴うRT更新
			if (RTOverride && checkData.ChangeRTFlag)
				SetRT(checkData.ChangeRTDest, checkData.CanOverwriteRT, false);
		}
	}
	// モードリセット処理
	public void ModeReset(){
		if (gameMode != 0) {
			// モードのリセット: 払出残数=0または残ゲーム数=0orJAC数=0
			if (modeMedalCount == 0) SetMode(0, 0, 0);
			if (modeMedalCount < 0 && (modeGameCount <= 0 || modeJacCount<= 0)) SetMode(0, 0, 0);
			Debug.Log("ModeChk: Mode=" + gameMode + " Limit(Game/Jac/Medal)=" + modeGameCount + "/" + modeJacCount + "/" + modeMedalCount);
		}
		if (RTMode != 0) {
			// RTのリセット: 残ゲーム数=0
			if (RTGameCount == 0) SetRT(0, true, true);
			Debug.Log("RTChk: RT=" + RTMode + " Game=" + RTGameCount);
		}
	}
	// 内部mode移行処理
	private void SetMode(byte ModeDest, byte payIndex, byte gameIndex) {
		var mainROM = SlotMaker2022.MainROMDataManagerSingleton.GetInstance();
		var timer   = SlotTimerManagerSingleton.GetInstance();
		
		byte lastMode = gameMode;
		gameMode = ModeDest;
		// 条件装置指定
		if (gameIndex == 0){
			if (payIndex == 0) modeMedalCount = -1;
			modeMedalCount = (int)mainROM.CastCommonData.BonusPayData.GetData((uint)(payIndex - 1));
			if (modeMedalCount <= 0) modeMedalCount = -1;
			modeGameCount  = 0;
			modeJacCount   = 0;
		} else {
			modeMedalCount = -1;
			modeGameCount  = (byte)mainROM.CastCommonData.GameNumData.GetData((uint)(gameIndex - 1));
			if (payIndex == 0) modeJacCount = modeGameCount;
			else modeJacCount = (byte)mainROM.CastCommonData.BonusPayData.GetData((uint)(payIndex - 1));
		}
		// タイマ作動
		timer.GetTimer("changeMode").Activate();
		timer.GetTimer("changeMode").Reset();
		Debug.Log("ModeSet: Mode=" + ModeDest + " Limit(Game/Jac/Medal)=" + modeGameCount + "/" + modeJacCount + "/" + modeMedalCount);
		// モード移行によるRT移行チェック
		foreach(var item in mainROM.RTMoveData) {
			if (item.ModeSrc != lastMode || item.ModeDst != gameMode) continue;
			SetRT(item.Destination, item.CanOverride, true);
			break;
		}
	}
	// 内部RT移行処理(ret:RTを上書きしたか)
	private void SetRT(byte RTDest, bool ovrDef, bool ovrGame) {
		var mainROM = SlotMaker2022.MainROMDataManagerSingleton.GetInstance();
		var timer   = SlotTimerManagerSingleton.GetInstance();
		
		// 移行処理
		RTMode = RTDest;
		RTOverride = ovrDef;
		uint gameIdx = mainROM.RTCommonData.ContGameNum.GetData((uint)RTDest);
		if (gameIdx == 0) RTGameCount = -1;
		else {
			RTOverride &= ovrGame;
			RTGameCount = (int)mainROM.CastCommonData.GameNumData.GetData(gameIdx - 1u);
		}
		// タイマ作動
		timer.GetTimer("changeRT").Activate();
		timer.GetTimer("changeRT").Reset();
		Debug.Log("RTSet: RT=" + RTDest + " Game=" + RTGameCount);
	}
}
