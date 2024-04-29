using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIShowPatternColle : ScrollPrehabBase
{
	private SlotEffectMaker2023.Data.CollectionData cd;
	private GameObject[] refObj;
	private CollectionBuilder[] refScr;
	
	private int showNum = 4;
	
	protected override void Awake(){
		base.Awake();
    	cd = SlotEffectMaker2023.Singleton.EffectDataManagerSingleton.GetInstance().Collection;
        
        refObj = new GameObject[showNum];
        refScr = new CollectionBuilder[showNum];
        
        // scr取得(マジックナンバー要調整)
        for (int i=0; i<showNum; ++i){
        	refObj[i] = transform.Find("Elem+" + i.ToString()).gameObject;
        	refScr[i] = refObj[i].transform.GetComponent<CollectionBuilder>();
        }
    }

	public override void RefreshData(int pID, bool pIsSelected){
		base.RefreshData(pID, pIsSelected);
		for (int i=0; i<showNum; ++i){
			int refID = showNum * pID + i;
			if (refID < 0 || refID >= cd.Collections.Count) {
				refScr[i].SetData(null, 0);
			} else {
				refScr[i].SetData(cd.Collections[refID], refID+1);
			}
		}
		Debug.Log("test");
	}
}
