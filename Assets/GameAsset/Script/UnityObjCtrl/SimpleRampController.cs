using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// 変数とタイマの状況により描画のON/OFFを制御する
public class SimpleRampController : MonoBehaviour
{
	// Singleton
	SlotEffectMaker2023.Singleton.SlotDataSingleton	slotData;	// スロット基本情報
	
	// 制御用変数
	[SerializeField] string VariableName;	// 条件判定を行う変数名。指定なしで判定しない
	[SerializeField] int    RangeA;			// 条件判定値A
	[SerializeField] int    RangeB;			// 条件判定値B
	[SerializeField] bool   EqualFlag;		// 判定条件に等号を含むか
	[SerializeField] bool   VarInvert;		// 条件を満たすときに表示するか(true: 表示しない)
	
	[SerializeField] string TimerName;		// 時間判定を行うタイマ名。指定なしで判定しない
	[SerializeField] float  TimeBegin;		// 時間下限値
	[SerializeField] bool   TimeInvert;		// 条件を満たすときに表示するか(true: 表示しない)
	
	[SerializeField] int    BlinkCount;		// 点滅回数
	[SerializeField] float  BlinkCycle;		// 点滅周期
	
	// 制御フィールド
	
    // Start is called before the first frame update
    void Start()
    {
		slotData = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance();
    }

    // Update is called once per frame
    void Update()
    {
    	// ボタンを点灯させるか
    	bool activated = true;
    	bool condVar = true;
    	bool condTimer = true;

    	// 変数の検証
        if (VariableName != string.Empty){
        	int min = Math.Min(RangeA, RangeB);
        	int max = Math.Max(RangeA, RangeB);
        	// bool?(null許容型)が戻り値。VariableNameが存在しない場合は無効判定
        	activated = false;
        	activated = slotData.valManager.GetVariable(VariableName)?.CheckRange(min, max, EqualFlag) == true;
        	condVar = activated ^ VarInvert;
        }
        
        // タイマの検証(タイマが見つからなかった場合無効判定)
        activated = true;
        if (TimerName != string.Empty){
        	var elem = slotData.timerData.GetTimer(TimerName);
        	if (elem == null) condTimer = false;
        	else {
        		if (!elem.isActivate) condTimer = false;		// タイマが無効な場合無効判定
        		else activated = elem.elapsedTime > TimeBegin;	// 指定時間を超過しているか
        	}
        	// 絶対点灯させない条件
        	if (!condTimer) { this.gameObject.GetComponent<SpriteRenderer>().enabled = true; return; }
        	condTimer = activated ^ TimeInvert;
        }
        
        // 計算の結果、ボタンを点灯"させない"場合にオブジェクトを表示する
        this.gameObject.GetComponent<SpriteRenderer>().enabled = true;
        if (condVar && condTimer) {
        	this.gameObject.GetComponent<SpriteRenderer>().enabled = false;
	        // 点滅処理を行う
	        if (!(BlinkCycle > 0) || BlinkCount == 0) return;
	        if (TimerName != string.Empty){
	        	var elem = slotData.timerData.GetTimer(TimerName);
	        	if (elem == null) return;
	        	if (!elem.isActivate) return;
	        	// 消灯条件
	        	float checkTime = (float)elem.elapsedTime - TimeBegin;
	        	if ( checkTime % BlinkCycle > BlinkCycle / 2f && (BlinkCount < 0 || (int)(checkTime / BlinkCycle) < BlinkCount) )
	        		this.gameObject.GetComponent<SpriteRenderer>().enabled = true;
	        }
        }
    }
}
