using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UISingle1ButtonMod : MonoBehaviour
{
	[SerializeField] private Button BaseButton;
	[SerializeField] private Vector2 Interval;
	[SerializeField] private byte[] OrderID;
	[SerializeField,Multiline] private string PayInfo;

	private Button[] selector;
	private Image[] im;
	private TextMeshProUGUI[] tx;
	
	private string[] infoArray;
	SlotEffectMaker2023.Action.SystemData sys;

    // Start is called before the first frame update
    private void Start()
    {
    	int sz = OrderID.Length;
    	
		selector = new Button[sz];
		im = new Image[sz];
		tx = new TextMeshProUGUI[sz];
		
		sys = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance().sysData;
		infoArray = PayInfo.Split(",");
		
		// Instantiateする
		for(int i=0; i<sz; ++i){
			selector[i] = i == 0 ? BaseButton : Instantiate(BaseButton, this.transform);
			im[i] = selector[i].GetComponent<Image>();
			tx[i] = selector[i].transform.Find("IDText").GetComponent<TextMeshProUGUI>();
			
			selector[i].GetComponent<RectTransform>().anchoredPosition += new Vector2(Interval.x * i, 0);
			tx[i].text = infoArray[i];
			
			// ボタン押下時のスクリプト登録
        	byte prm = OrderID[i];	// 変数に入れないとデルタがうまく動かないらしい…
        	selector[i].onClick.AddListener(() => OnClickButton(prm));
		}
    }

	private void Update(){
		for (int i=0; i<selector.Length; ++i){
			Color cl = sys.Order1Button == OrderID[i] ? Color.yellow : Color.white;
			im[i].color = cl;
			tx[i].color = cl;
		}
	}
	
	public void OnClickButton(byte index){
		sys.Order1Button = index;
	}
}
