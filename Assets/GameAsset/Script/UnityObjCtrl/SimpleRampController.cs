using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// 変数とタイマの状況により描画のON/OFFを制御する
public class SimpleRampController : MonoBehaviour
{
	// Singleton
	SlotDataSingleton			slotData;	// スロット基本情報
	SlotTimerManagerSingleton	timer;		// タイマー
	
	// 制御用変数
	[SerializeField] string VariableName;	// 条件判定を行う変数名。指定なしで判定しない
	[SerializeField] int    RangeA;			// 条件判定値A
	[SerializeField] int    RangeB;			// 条件判定値B
	[SerializeField] bool   EqualFlag;		// 判定条件に等号を含むか
	
	[SerializeField] string TimerName;		// 時間判定を行うタイマ名。指定なしで判定しない
	[SerializeField] float  TimeBegin;		// 時間下限値
	
	// 制御フィールド
	
    // Start is called before the first frame update
    void Start()
    {
		slotData = SlotDataSingleton.GetInstance();
		timer    = SlotTimerManagerSingleton.GetInstance();
    }

    // Update is called once per frame
    void Update()
    {
    	bool activated = true;	// ボタンを点灯させるか

    	// 変数の検証
        if (VariableName != string.Empty){
        	int min = Math.Min(RangeA, RangeB);
        	int max = Math.Max(RangeA, RangeB);
        	// bool?(null許容型)が戻り値。VariableNameが存在しない場合は無効判定
        	activated &= slotData.valManager.GetVariable(VariableName)?.CheckRange(min, max, EqualFlag) == true;
        }
        // タイマの検証(タイマが見つからなかった場合無効判定)
        if (TimerName != string.Empty){
        	var elem = timer.GetTimer(TimerName);
        	if (elem == null) activated = false;
        	else {
        		if (!elem.isActivate) activated = false;		// タイマが無効な場合無効判定
        		else activated &= elem.elapsedTime > TimeBegin;	// 指定時間を超過しているか
        	}
        }
        
        // 計算の結果、ボタンを点灯"させない"場合にオブジェクトを表示する
        this.gameObject.GetComponent<SpriteRenderer>().enabled = !activated;
    }
}
