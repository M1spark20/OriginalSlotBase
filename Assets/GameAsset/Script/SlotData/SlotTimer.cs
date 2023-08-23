using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SlotTimer
{
	public string timerName   { get; private set; }	// タイマーの名前、呼び出し時の識別子になる
	public float? elapsedTime { get; private set; }	// 経過時間、Time.deltaTimeの積算で表現する。無効時:null
	public bool isActivate    { get; private set; }	// このタイマーが有効か
	public bool isPaused      { get; private set; }	// このタイマーを一時停止しているか
	
	public SlotTimer(string pTimerName){
		// タイマを新規に作成するときのコンストラクタ: タイマ名を指定して新規作成する。
		// 呼び出し前にタイマ名が重複しないことを確認すること
		timerName   = pTimerName;
		elapsedTime = null;
		isActivate  = false;
		isPaused    = false;
	}
	// 処理系関数
	// タイマを有効にしてカウントを開始する。有効化済みの場合は何もしない
	public void Activate(float offset){
		if (isActivate) return;
		isActivate = true;
		Reset(offset);
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
		elapsedTime = null;
	}
	
	// タイマを更新する
	public void Update(float deltaTime){
		if (!isActivate || isPaused) return;
		elapsedTime += deltaTime;
	}
}
