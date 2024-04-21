using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MainMenuHistory : MonoBehaviour
{
	[SerializeField] private GameObject PatternElem;
	[SerializeField] private GameObject HistoryViewer;
	
	private SlotEffectMaker2023.Action.HistoryManager hm;
	private ReelPatternBuilder builder;
	private UISmartScroller scroller;
	
	private int lastShow;
	
    // Start is called before the first frame update
    private void Start()
    {
    	hm = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance().historyManager;
    	builder = PatternElem?.GetComponent<ReelPatternBuilder>() ?? null;
    	scroller = HistoryViewer?.GetComponent<UISmartScroller>() ?? null;
    	lastShow = int.MinValue;
    }

    // Update is called once per frame
    private void Update()
    {
    	if (hm.BonusHist.Count == 0 || scroller.ContentCount <= 0) return;
    	int nowShow = scroller.SelectedIndex;
    	if (nowShow != lastShow){
    		if (nowShow < 0 || nowShow >= hm.BonusHist.Count) {
    			builder.Reset();
    			transform.Find("Date").GetComponent<TextMeshProUGUI>().text = string.Empty;
    			PatternElem.transform.Find("Info").GetComponent<TextMeshProUGUI>().text = "Select Bonus Data";
    		} else {
				builder.SetData(hm.BonusHist[nowShow].InPattern);
    			transform.Find("Date").GetComponent<TextMeshProUGUI>().text = hm.BonusHist[nowShow].InDate;
				PatternElem.transform.Find("Info").GetComponent<TextMeshProUGUI>().text = "Loss game: " + hm.BonusHist[nowShow].LossGame.ToString();
    		}
    		lastShow = nowShow;
    	}
    }
}
