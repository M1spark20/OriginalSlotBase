using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SlotTimer : SlotMaker2022.ILocalDataInterface
{
	const float DISABLE_TIME = -1f;

	public string timerName  { get; private set; }	// タイマーの名前、呼び出し時の識別子になる
	public float elapsedTime { get; private set; }	// 経過時間、Time.deltaTimeの積算で表現する。無効時: -1
	public bool isActivate   { get; private set; }	// このタイマーが有効か
	public bool isPaused     { get; private set; }	// このタイマーを一時停止しているか
	
	public SlotTimer(ref BinaryReader fs, int version){
		// データを外部から読み込んだ時のコンストラクタ: ReadData関数を直接動かしてデータを読む
		ReadData(ref fs, version);
	}
	public SlotTimer(string pTimerName){
		// タイマを新規に作成するときのコンストラクタ: タイマ名を指定して新規作成する。
		// 呼び出し前にタイマ名が重複しないことを確認すること
		timerName   = pTimerName;
		elapsedTime = DISABLE_TIME;
		isActivate  = false;
		isPaused    = false;
	}
	public bool StoreData(ref BinaryWriter fs, int version){
		fs.Write(timerName);
		fs.Write(elapsedTime);
		fs.Write(isActivate);
		fs.Write(isPaused);
		return true;
	}
	public bool ReadData(ref BinaryReader fs, int version){
		timerName   = fs.ReadString();
		elapsedTime = fs.ReadSingle();
		isActivate  = fs.ReadBoolean();
		isPaused    = fs.ReadBoolean();
		return true;
	}
	
	// 処理系関数
	// タイマを有効にしてカウントを開始する。有効化済みの場合は何もしない
	public void Activate(float offset){
		if (isActivate) return;
		isActivate = true;
		Reset(offset);
		Debug.Log("Timer has Activated: " + timerName + " >" + elapsedTime);
	}
	public void Activate() { Activate(0f); }
	
	// タイマの経過時間をリセットする
	public void Reset(float offset){
		if (!isActivate) return;
		elapsedTime = 0f;
		if (offset > 0f) elapsedTime = offset;
	}
	public void Reset() { Reset(0f); }
	
	// カウントを一時中断するか指定する
	public void SetPaused(bool pauseFlag){
		if (!isActivate) return;
		isPaused = pauseFlag;
	}
	
	// タイマーを無効にする
	public void SetDisabled(){
		isActivate = false;
		isPaused = false;
		elapsedTime = DISABLE_TIME;
	}
	
	// タイマを更新する
	public void Update(float deltaTime){
		if (!isActivate || isPaused) return;
		elapsedTime += deltaTime;
	}
}
