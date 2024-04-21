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
	
	private bool LastActivate;
	private bool LastFinished;
	
	protected override void Start(){
		base.Start();
        hc = SlotEffectMaker2023.Singleton.EffectDataManagerSingleton.GetInstance().HistoryConf;
        hm = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance().historyManager;
        comaData = ReelChipHolder.GetInstance();
        
        Select?.onClick.AddListener(OnClick);
        
		LastActivate = false;
		LastFinished = false;
	}
	
	protected override bool NeedRefresh(int pID, bool pIsSelected){
		if (base.NeedRefresh(pID, pIsSelected)) return true;
		if (pID < 0 || pID >= hm.BonusHist.Count) return false;
		var refData = hm.BonusHist[pID];
		if (refData.IsActivate ^ LastActivate) return true;
		if (refData.IsFinished ^ LastFinished) return true;
		return false;
	}
	
	protected override void RefreshData(int pID, bool pIsSelected){
		if (pID < 0 || pID >= hm.BonusHist.Count) return;
		var refData = hm.BonusHist[pID];
		var txtColor = pIsSelected ? Color.yellow : Color.white;
		
		transform.Find("Number").GetComponent<TextMeshProUGUI>().text = (hm.BonusHist.Count - pID).ToString();
		transform.Find("Number").GetComponent<TextMeshProUGUI>().color = txtColor;
		transform.Find("Game").GetComponent<TextMeshProUGUI>().text = refData.InGame.ToString();
		transform.Find("Game").GetComponent<TextMeshProUGUI>().color = txtColor;
		transform.Find("Coma").GetComponent<Image>().sprite = comaData.ReelChipData.Extract(hc.GetConfig(refData.BonusFlag).ComaID);
		transform.Find("Get").GetComponent<TextMeshProUGUI>().text = refData.IsFinished ? (refData.MedalAfter - refData.MedalBefore).ToString() : string.Empty;
		transform.Find("Get").GetComponent<TextMeshProUGUI>().color = txtColor;
		
		LastActivate = refData.IsActivate;
		LastFinished = refData.IsFinished;
	}
	
	protected override void ShowUI(bool pVisible){
		transform.Find("Number").GetComponent<TextMeshProUGUI>().enabled = pVisible;
		transform.Find("Game").GetComponent<TextMeshProUGUI>().enabled = pVisible;
		transform.Find("Coma").GetComponent<Image>().enabled = pVisible;
		transform.Find("Get").GetComponent<TextMeshProUGUI>().enabled = pVisible;
	}
	
	public void OnClick(){ SelectMe(); }
}
