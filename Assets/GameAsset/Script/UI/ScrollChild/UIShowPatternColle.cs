using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIShowPatternColle : ScrollPrehabBase
{
	private SlotEffectMaker2023.Data.CollectionData cd;
	private SlotEffectMaker2023.Action.CollectionLogger logger;
	private GameObject[] refObj;
	private Canvas[] refCanvas;
	private CollectionBuilder[] refScr;
	
	private int showNum = 4;
	
	protected override void Awake(){
		base.Awake();
    	cd = SlotEffectMaker2023.Singleton.EffectDataManagerSingleton.GetInstance().Collection;
    	logger = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance().collectionManager;
        
        refObj = new GameObject[showNum];
        refCanvas = new Canvas[showNum];
        refScr = new CollectionBuilder[showNum];
        
        // scr取得(マジックナンバー要調整)
        for (int i=0; i<showNum; ++i){
        	refObj[i] = transform.Find("Elem+" + i.ToString()).gameObject;
        	refCanvas[i] = refObj[i].transform.GetComponent<Canvas>();
        	refScr[i] = refObj[i].transform.GetComponent<CollectionBuilder>();
        }
    }

	public override void RefreshData(int pID, bool pIsSelected){
		base.RefreshData(pID, pIsSelected);
		for (int i=0; i<showNum; ++i){
			int refID = showNum * pID + i;
			if (refID < 0 || refID >= cd.Collections.Count) {
				refCanvas[i].enabled = false;
			} else {
				refCanvas[i].enabled = true;
				refScr[i].SetData(cd.Collections[refID], logger.Achievements[refID], refID+1);
			}
		}
		Debug.Log("test");
	}
}
