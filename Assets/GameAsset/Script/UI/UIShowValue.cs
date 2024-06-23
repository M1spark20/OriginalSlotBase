using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIShowValue : MonoBehaviour
{
	[SerializeField] protected string DataName;
	[SerializeField] protected string DispVariable;
	[SerializeField, Min(0)] protected int ShowDigitRound;	// showDigitからnケタ分を小数にする
	[SerializeField] protected bool ShowPlusMinus;
	[SerializeField] protected string Suffix;
	[SerializeField] protected string BlinkCondVar;
	[SerializeField] protected int BlinkCondMin;
	[SerializeField] protected int BlinkCondMax;
	[SerializeField] protected float BlinkCycle;
	
	private TextMeshProUGUI Title;
	private TextMeshProUGUI Value;
	private Color red;

	void Start(){
		Title = (TextMeshProUGUI)transform.Find("Title").gameObject.GetComponent<TextMeshProUGUI>();
		Value = (TextMeshProUGUI)transform.Find("Value").gameObject.GetComponent<TextMeshProUGUI>();
		Title.text = DataName;
		red = new Color(1f, 0.3f, 0.3f);
	}

	// Update is called once per frame
	void Update()
	{
		Title.text = DataName;
		Value.text = string.Empty;
		Value.color = Color.white;
		
		var varData = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance().valManager;
		int? showVal = varData.GetVariable(DispVariable)?.val;
		if (!showVal.HasValue) return;
		
		if (ShowPlusMinus){
			if ((int)showVal > 0) Value.text += "+";
			if ((int)showVal < 0) Value.color = red;
		}
		
		float sh = (float)showVal / Mathf.Pow(10f, ShowDigitRound);
		Value.text += sh.ToString("F" + ShowDigitRound.ToString()) + Suffix;
		
		// 点滅条件
		int? condVal = varData.GetVariable(BlinkCondVar)?.val;
		if (!condVal.HasValue) return;
		if ((int)condVal >= BlinkCondMin && (int)condVal <= BlinkCondMax){
			var tm = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance().timerData.GetTimer("general");
    		if (tm != null) { if (tm.elapsedTime % BlinkCycle > BlinkCycle / 2f) Value.text = string.Empty; }
    	}
	}
}
