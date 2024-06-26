using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization;
using TMPro;

public class UIShowPatternScr : ScrollPrehabBase
{
	[SerializeField] private string LocalizeStringTable;
	[SerializeField] private string LocalizeStringID;

	private SlotEffectMaker2023.Data.HistoryConfig hc;
	private SlotEffectMaker2023.Action.HistoryManager hm;
	private ReelChipHolder comaData;
	private GameObject[] refObj;
	private Canvas[] refCanvas;
	private ReelPatternBuilder[] refScr;
	private TextMeshProUGUI[] info;
	
	private int showNum = 2;
	
	protected override void Awake(){
		base.Awake();
        hc = SlotEffectMaker2023.Singleton.EffectDataManagerSingleton.GetInstance().HistoryConf;
        hm = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance().historyManager;
        comaData = ReelChipHolder.GetInstance();
        
        refObj = new GameObject[showNum];
        refCanvas = new Canvas[showNum];
        refScr = new ReelPatternBuilder[showNum];
        info = new TextMeshProUGUI[showNum];
        
        // scr取得(マジックナンバー要調整)
        for (int i=0; i<showNum; ++i){
        	refObj[i] = transform.Find("Elem+" + i.ToString()).gameObject;
        	refCanvas[i] = refObj[i].transform.GetComponent<Canvas>();
        	refScr[i] = refObj[i].transform.GetComponent<ReelPatternBuilder>();
        	info[i] = refObj[i].transform.Find("Info").GetComponent<TextMeshProUGUI>();
        }
    }

	public override void RefreshData(int pID, bool pIsSelected){
		base.RefreshData(pID, pIsSelected);
		for (int i=0; i<showNum; ++i){
			int refID = showNum * pID + i;
			if (refID < 0 || refID >= hm.PatternHist.Count) {
				refCanvas[i].enabled = false;
			} else {
				refCanvas[i].enabled = true;
				// ローカライズ対応
				var localizedString = new LocalizedString(LocalizeStringTable, LocalizeStringID);
				localizedString.Arguments = new object[] { new Dictionary<string, int>() { { "0", refID } } };
				refScr[i].SetData(hm.PatternHist[refID], localizedString.GetLocalizedString());
			}
		}
		Debug.Log("test");
	}
}
