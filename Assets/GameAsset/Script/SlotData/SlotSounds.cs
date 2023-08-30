using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

// サウンドID情報。IDにより管理する
public class SoundID : SlotMaker2022.ILocalDataInterface
{
	public string DataName    { get; set; }	// サウンド定義名
	public string ShotResName { get; set; }	// 単音 or イントロ音源のリソース名
	public string LoopResName { get; set; }	// ループ音源のリソース名
	public int    LoopBegin   { get; set; }	// ループ音源の開始遅延時間[ms]
	
	public SoundID(){
		DataName    = string.Empty;
		ShotResName = string.Empty;
		LoopResName = string.Empty;
		LoopBegin   = -1;
	}
	
	public bool StoreData(ref BinaryWriter fs, int version){
		fs.Write(DataName);
		fs.Write(ShotResName);
		fs.Write(LoopResName);
		fs.Write(LoopBegin);
		return true;
	}
	public bool ReadData(ref BinaryReader fs, int version){
		DataName    = fs.ReadString();
		ShotResName = fs.ReadString();
		LoopResName = fs.ReadString();
		LoopBegin   = fs.ReadInt32();
		return true;
	}
}

// 音を鳴らす単体データ
public class SoundPlayData : SlotMaker2022.ILocalDataInterface
{
	// 定数
	public const float TIME_DIV = 1000f;
	const string SHOT_HEADER = "#SS_";
	const string LOOP_HEADER = "#SL_";
	
	// 変数
	public string PlayerName     { get; set; }	// プレイヤー名。デフォルトタイマ名がこの名前で生成される
	
	public string UseTimerName   { get; set; }	// 制御に使用するタイマー名
	public int    BeginTime      { get; set; }	// 鳴動開始時間[ms]
	public int    StopTime       { get; set; }	// 鳴動終了時間[ms] (※UseTimer基準)
	public int    DefaultSoundID { get; set; }	// デフォルトで鳴らすサウンドのID: 外部から変更可能
	
	public SoundPlayData(){
		PlayerName     = string.Empty;
		UseTimerName   = string.Empty;
		BeginTime      = 0;
		StopTime       = -1;
		DefaultSoundID = 0;
	}
	
	public bool StoreData(ref BinaryWriter fs, int version){
		fs.Write(PlayerName);
		fs.Write(UseTimerName);
		fs.Write(BeginTime);
		fs.Write(StopTime);
		fs.Write(DefaultSoundID);
		return true;
	}
	public bool ReadData(ref BinaryReader fs, int version){
		PlayerName     = fs.ReadString();
		UseTimerName   = fs.ReadString();
		BeginTime      = fs.ReadInt32();
		StopTime       = fs.ReadInt32();
		DefaultSoundID = fs.ReadInt32();
		return true;
	}
	
	// データに対する同期用タイマを作成する
	public void MakeTimer(ref TimerList pList){
		pList.CreateTimer(GetShotTimerName(), false);
		pList.CreateTimer(GetLoopTimerName(), true);
	}
	// タイマ名を取得する
	public string GetShotTimerName() { return SHOT_HEADER + PlayerName; }
	public string GetLoopTimerName() { return LOOP_HEADER + PlayerName; }
}

// サウンドデータ管理クラス
public class SoundDataList : SlotMaker2022.ILocalDataInterface
{
	// 変数
	public List<SoundID>       IDList   { get; set; }
	public List<SoundPlayData> PlayList { get; set; }
	public List<int>           SoundID  { get; set; }
	
	public SoundDataList(){
		IDList   = new List<SoundID>();
		PlayList = new List<SoundPlayData>();
		SoundID  = new List<int>();
	}
	
	public bool StoreData(ref BinaryWriter fs, int version){
		fs.Write(IDList.Count);
		for (int i = 0; i < IDList.Count; ++i) 
			if (!IDList[i].StoreData(ref fs, version))   return false;
			
		fs.Write(PlayList.Count);
		for (int i = 0; i < PlayList.Count; ++i) 
			if (!PlayList[i].StoreData(ref fs, version)) return false;
			
		return true;
	}
	public bool ReadData(ref BinaryReader fs, int version){
		int dataSize = fs.ReadInt32();
		for (int i = 0; i < dataSize; ++i) {
		    var newData = new SoundID();
		    if (!newData.ReadData(ref fs, version)) return false;
		    IDList.Add(newData);
		}
		
		dataSize = fs.ReadInt32();
		for (int i = 0; i < dataSize; ++i) {
		    var newData = new SoundPlayData();
		    if (!newData.ReadData(ref fs, version)) return false;
		    PlayList.Add(newData);
		}

		return true;
	}
	
	// データ編集用関数
	public void AddID(SoundID data) { IDList.Add(data); }
	public void AddPlayData(SoundPlayData data, ref TimerList timerList) {
		data.MakeTimer(ref timerList);
		PlayList.Add(data);
		SoundID.Add(data.DefaultSoundID);
	}
	public void ChangeSoundID(int pPlayID, int pSoundID){
		if (pPlayID  < 0 || pPlayID  >= SoundID.Count) return;
		if (pSoundID < 0 || pSoundID >= IDList.Count)  return;
		SoundID[pPlayID] = pSoundID;
	}
	
	// 音源データを仮生成する
	public void DataStab(ref TimerList timerList){
		// 音のデータ
		SoundID sid = new SoundID();
		sid.DataName    = "None";
		AddID(sid);
		
		sid = new SoundID();
		sid.DataName    = "Bet";
		sid.ShotResName = "SE_Bet";
		sid.LoopBegin   = 0;
		AddID(sid);
		
		sid = new SoundID();
		sid.DataName    = "Start";
		sid.ShotResName = "SE_Start";
		AddID(sid);
		
		sid = new SoundID();
		sid.DataName    = "StopDef";
		sid.ShotResName = "SE_Stop";
		AddID(sid);
		
		sid = new SoundID();
		sid.DataName    = "ReplayIn";
		sid.ShotResName = "SE_ReplayIn";
		AddID(sid);
		
		sid = new SoundID();
		sid.DataName    = "PayoutCherry";
		sid.ShotResName = "SE_Payout";
		AddID(sid);
		
		sid = new SoundID();
		sid.DataName    = "Wait";
		sid.LoopResName = "SE_Wait";
		sid.LoopBegin   = 0;
		AddID(sid);
		
		sid = new SoundID();
		sid.DataName    = "PayoutBell";
		sid.LoopResName = "SE_Payout";
		sid.LoopBegin   = 0;
		AddID(sid);
		
		sid = new SoundID();
		sid.DataName    = "PayoutGrape";
		sid.LoopResName = "SE_PayoutGrape";
		sid.LoopBegin   = 0;
		AddID(sid);
		
		// 再生側のデータ
		SoundPlayData pid  = new SoundPlayData();
		pid.PlayerName     = "Bet";
		pid.UseTimerName   = "betShot";
		pid.DefaultSoundID = 1;
		AddPlayData(pid, ref timerList);
		
		pid  = new SoundPlayData();
		pid.PlayerName     = "Wait";
		pid.UseTimerName   = "waitStart";
		pid.DefaultSoundID = 6;
		AddPlayData(pid, ref timerList);
		
		pid  = new SoundPlayData();
		pid.PlayerName     = "Start";
		pid.UseTimerName   = "reelStart";
		pid.DefaultSoundID = 2;
		AddPlayData(pid, ref timerList);
		
		pid  = new SoundPlayData();
		pid.PlayerName     = "Stop";
		pid.UseTimerName   = "anyReelPush";
		pid.DefaultSoundID = 3;
		AddPlayData(pid, ref timerList);
		
		pid  = new SoundPlayData();
		pid.PlayerName     = "Payout";
		pid.UseTimerName   = "payoutTime";
		pid.DefaultSoundID = 4;
		AddPlayData(pid, ref timerList);
	}
}
