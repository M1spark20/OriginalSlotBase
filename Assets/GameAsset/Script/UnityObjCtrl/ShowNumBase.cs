using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ShowNumBase : IMultiImageWriter
{
	[SerializeField] protected string DispVariable;
	[SerializeField, Min(1)] protected int ShowDigit;
	[SerializeField] string TimerName;		// 時間判定を行うタイマ名。指定なしで判定しない
	[SerializeField] float  TimeBegin;		// 時間下限値
	[SerializeField] bool   TimeInvert;		// 条件を満たすときに表示するか(true: 表示しない)

	// 定義すべき関数: テクスチャの使用番号を取得する
	abstract protected int? GetTextureIndex(int val, uint getDigit);	// valに対しdigitのindexを決める。表示しない場合null

    // Update is called once per frame
    protected override void Update()
    {
    	// 表示させる値を取得する
		var varData = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance().valManager;
		int? showVal = varData.GetVariable(DispVariable)?.val;
		if (!showVal.HasValue) return;
		
		for (uint i=0; i<ShowDigit; ++i){
			int? spID = GetTextureIndex((int)showVal, i);
			SpriteRenderer sp = mComaInstance[i].GetComponent<SpriteRenderer>();
			sp.enabled = spID.HasValue && CheckTimer();
			if (spID.HasValue) sp.sprite = mImageBuilder.Extract((int)spID);
		}
    }
    
	bool CheckTimer(){
		// 時間点灯条件判定
        if (TimerName == string.Empty) return true;
        
		var slotData = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance();
        bool activated = true;
    	var elem = slotData.timerData.GetTimer(TimerName);
    	if (elem == null) activated = false;
    	else {
    		if (!elem.isActivate) activated = false;		// タイマが無効な場合無効判定
    		else activated &= elem.elapsedTime > TimeBegin;	// 指定時間を超過しているか
    	}
    	return activated ^ TimeInvert;
	}
}
