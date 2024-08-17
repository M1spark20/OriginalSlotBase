using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIPlotPosSender : MonoBehaviour
{
	[SerializeField] private Button BaseButton;
	[SerializeField] private Vector2 Interval;
	[SerializeField] private Vector2Int NumCount;
	
	private Button[] selector;
	private Image[] im;
	private TextMeshProUGUI[] tx;
	private SlotEffectMaker2023.Action.SystemData sys;
	
	private void Start(){
		int sz = NumCount.x * NumCount.y;
		selector = new Button[sz];
		im = new Image[sz];
		tx = new TextMeshProUGUI[sz];
		sys = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance().sysData;
		
		// Instantiateする
		for(int i=0; i<sz; ++i){
			selector[i] = i == 0 ? BaseButton : Instantiate(BaseButton, this.transform);
			im[i] = selector[i].GetComponent<Image>();
			tx[i] = selector[i].transform.Find("IDText").GetComponent<TextMeshProUGUI>();
			
			selector[i].GetComponent<RectTransform>().anchoredPosition += new Vector2(Interval.x * (i % NumCount.x), Interval.y * (i / NumCount.x));
			tx[i].text = (i+1).ToString();
			
			// ボタン押下時のスクリプト登録
        	var prm = i;	// 変数に入れないとデルタがうまく動かないらしい…
        	selector[i].onClick.AddListener(() => OnClickButton(prm));
		}
	}
	
	private void Update(){
		for (int i=0; i<selector.Length; ++i){
			Color cl = sys.InfoPos == i ? Color.yellow : Color.white;
			im[i].color = cl;
			tx[i].color = cl;
		}
	}
	
	public void OnClickButton(int index){
		sys.InfoPos = index;
	}
}
