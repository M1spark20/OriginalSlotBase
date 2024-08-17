using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UISlotSetModifier : MonoBehaviour
{
	[SerializeField] private Button BaseButton;
	[SerializeField] private Vector2 Interval;
	[SerializeField,Multiline] private string PayInfo;
	[SerializeField] private TextMeshProUGUI PayInfoShow;

	private Button[] selector;
	private Image[] im;
	private TextMeshProUGUI[] tx;
	
	private SlotEffectMaker2023.Action.SlotBasicData bs;
	private string[] infoArray;

    // Start is called before the first frame update
    private void Start()
    {
		int sz = SlotMaker2022.LocalDataSet.SETTING_MAX;
		selector = new Button[sz];
		im = new Image[sz];
		tx = new TextMeshProUGUI[sz];
		
		bs = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance().basicData;
		infoArray = PayInfo.Split(",");
		
		// Instantiateする
		for(int i=0; i<sz; ++i){
			selector[i] = i == 0 ? BaseButton : Instantiate(BaseButton, this.transform);
			im[i] = selector[i].GetComponent<Image>();
			tx[i] = selector[i].transform.Find("IDText").GetComponent<TextMeshProUGUI>();
			
			selector[i].GetComponent<RectTransform>().anchoredPosition += new Vector2(Interval.x * i, 0);
			tx[i].text = (i+1).ToString();
			
			// ボタン押下時のスクリプト登録
        	byte prm = (byte)i;	// 変数に入れないとデルタがうまく動かないらしい…
        	selector[i].onClick.AddListener(() => OnClickButton(prm));
		}
    }

	private void Update(){
		for (int i=0; i<selector.Length; ++i){
			Color cl = bs.slotSetting == i ? Color.yellow : Color.white;
			im[i].color = cl;
			tx[i].color = cl;
		}
		PayInfoShow.text = infoArray[bs.slotSetting];
	}
	
	public void OnClickButton(byte index){
		bs.ChangeSlotSetting(index);
	}
}
