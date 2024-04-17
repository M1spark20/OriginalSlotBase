using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIShowBonusHistScr : ScrollPrehabBase
{
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
        
		LastActivate = false;
		LastFinished = false;
	}
	
	protected override bool NeedRefresh(int pID){
		if (base.NeedRefresh(pID)) return true;
		if (pID < 0 || pID >= hm.BonusHist.Count) return false;
		var refData = hm.BonusHist[pID];
		if (refData.IsActivate ^ LastActivate) return true;
		if (refData.IsFinished ^ LastFinished) return true;
		return false;
	}
	
	protected override void RefreshData(int pID){
		if (pID < 0 || pID >= hm.BonusHist.Count) return;
		var refData = hm.BonusHist[pID];
		transform.Find("Number").GetComponent<TextMeshProUGUI>().text = (hm.BonusHist.Count - pID).ToString();
		transform.Find("Game").GetComponent<TextMeshProUGUI>().text = refData.InGame.ToString();
		transform.Find("Coma").GetComponent<Image>().sprite = comaData.ReelChipData.Extract(hc.GetConfig(refData.BonusFlag).ComaID);
		transform.Find("Get").gameObject.SetActive(refData.IsFinished);
		transform.Find("Get").GetComponent<TextMeshProUGUI>().text = (refData.MedalAfter - refData.MedalBefore).ToString();
		
		LastActivate = refData.IsActivate;
		LastFinished = refData.IsFinished;
	}
}
