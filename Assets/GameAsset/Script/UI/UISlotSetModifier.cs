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

	[SerializeField] private GameObject RandAnsBtnShow;
	[SerializeField] private TextMeshProUGUI RandomAnswer;

	private Button[] selector;
	private Image[] im;
	private TextMeshProUGUI[] tx;
	private Canvas ansBtnC;
	private GraphicRaycaster ansBtnR;
	
	private SlotEffectMaker2023.Action.SlotBasicData bs;
	private string[] infoArray;
	
	private byte btnIndex;
	const byte sz = (byte)SlotMaker2022.LocalDataSet.SETTING_MAX;

    // Start is called before the first frame update
    private void Start()
    {
		selector = new Button[sz+1];
		im = new Image[sz+1];
		tx = new TextMeshProUGUI[sz+1];
		
		bs = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance().basicData;
		infoArray = PayInfo.Split(",");
		
		// Instantiateする
		for(int i=0; i<sz+1; ++i){
			selector[i] = i == 0 ? BaseButton : Instantiate(BaseButton, this.transform);
			im[i] = selector[i].GetComponent<Image>();
			tx[i] = selector[i].transform.Find("IDText").GetComponent<TextMeshProUGUI>();
			
			selector[i].GetComponent<RectTransform>().anchoredPosition += new Vector2(Interval.x * i, 0);
			tx[i].text = i == sz ? "?" : (i+1).ToString();
			
			// ボタン押下時のスクリプト登録
        	byte prm = (byte)i;	// 変数に入れないとデルタがうまく動かないらしい…
        	selector[i].onClick.AddListener(() => OnClickButton(prm));
		}
		
		// ボタン位置初期設定
		btnIndex = bs.setRandom ? (byte)sz : bs.slotSetting;
    }

	private void Update(){
		for (int i=0; i<selector.Length; ++i){
			Color cl = btnIndex == i ? Color.yellow : Color.white;
			im[i].color = cl;
			tx[i].color = cl;
		}
		if (btnIndex < sz) {
			PayInfoShow.enabled = true;
			PayInfoShow.text = infoArray[bs.slotSetting];
			RandAnsBtnShow.SetActive(false);
		} else {
			PayInfoShow.enabled = false;
			RandAnsBtnShow.SetActive(true);
		}
	}
	
	public void OnClickButton(byte index){
		byte newIndex = index;
		if (index == btnIndex) return;	// 選択場所が変更されてなければ再設定を行わない
		btnIndex = index;
		bool rand = index >= sz;
		if (rand) index = (byte)Random.Range(0, sz);
		bs.ChangeSlotSetting(index, rand);
	}
	
	public void CheckRandAnswer(){
		RandomAnswer.text = "Answer: " + (bs.slotSetting+1).ToString();
	}
}
