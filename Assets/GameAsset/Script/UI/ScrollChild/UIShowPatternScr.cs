using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIShowPatternScr : ScrollPrehabBase
{
	private SlotEffectMaker2023.Data.HistoryConfig hc;
	private SlotEffectMaker2023.Action.HistoryManager hm;
	private ReelChipHolder comaData;
	private GameObject[] refObj;
	private ReelPatternBuilder[] refScr;
	private TextMeshProUGUI[] info;
	
	private int showNum = 2;
	
	protected override void Awake(){
		base.Awake();
        hc = SlotEffectMaker2023.Singleton.EffectDataManagerSingleton.GetInstance().HistoryConf;
        hm = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance().historyManager;
        comaData = ReelChipHolder.GetInstance();
        
        refObj = new GameObject[showNum];
        refScr = new ReelPatternBuilder[showNum];
        info = new TextMeshProUGUI[showNum];
        
        // scr取得(マジックナンバー要調整)
        for (int i=0; i<showNum; ++i){
        	refObj[i] = transform.Find("Elem+" + i.ToString()).gameObject;
        	refScr[i] = refObj[i].transform.GetComponent<ReelPatternBuilder>();
        	info[i] = refObj[i].transform.Find("Info").GetComponent<TextMeshProUGUI>();
        }
    }

	public override void RefreshData(int pID, bool pIsSelected){
		base.RefreshData(pID, pIsSelected);
		for (int i=0; i<showNum; ++i){
			int refID = showNum * pID + i;
			if (refID < 0 || refID >= hm.PatternHist.Count) {
				refScr[i].SetData(null, "NO DATA");
			} else {
				refScr[i].SetData(hm.PatternHist[refID], refID.ToString() + " Game(s) before");
			}
		}
		Debug.Log("test");
	}
}
