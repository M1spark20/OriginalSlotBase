using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIShowBonusHistScr : ScrollPrehabBase
{
	[SerializeField] private Button Select;

	private SlotEffectMaker2023.Data.HistoryConfig hc;
	private SlotEffectMaker2023.Action.HistoryManager hm;
	private ReelChipHolder comaData;
	
	protected override void Awake(){
		base.Awake();
        hc = SlotEffectMaker2023.Singleton.EffectDataManagerSingleton.GetInstance().HistoryConf;
        hm = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance().historyManager;
        comaData = ReelChipHolder.GetInstance();
        
        Select?.onClick.AddListener(OnClick);
	}
	
	public override void RefreshData(int pID, bool pIsSelected){
		if (pID < 0 || pID >= hm.BonusHist.Count) return;
		base.RefreshData(pID, pIsSelected);
		var refData = hm.BonusHist[pID];
		var txtColor = pIsSelected ? Color.yellow : Color.white;
		
		transform.Find("Number").GetComponent<TextMeshProUGUI>().text = (hm.BonusHist.Count - pID).ToString();
		transform.Find("Number").GetComponent<TextMeshProUGUI>().color = txtColor;
		transform.Find("Game").GetComponent<TextMeshProUGUI>().text = refData.InGame.ToString();
		transform.Find("Game").GetComponent<TextMeshProUGUI>().color = txtColor;
		transform.Find("Coma").GetComponent<Image>().sprite = comaData.ReelChipData.Extract(hc.GetConfig(refData.BonusFlag).ComaID);
		transform.Find("Get").GetComponent<TextMeshProUGUI>().text = refData.IsFinished ? (refData.MedalAfter - refData.MedalBefore).ToString() : string.Empty;
		transform.Find("Get").GetComponent<TextMeshProUGUI>().color = txtColor;
	}
	
	public void OnClick(){ SelectMe(); }
}
