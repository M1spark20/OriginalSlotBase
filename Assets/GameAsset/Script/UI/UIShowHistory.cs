using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIShowHistory : MonoBehaviour
{
	[SerializeField, Min(1)] protected int ShowNum;
	[SerializeField] protected float DiffY;
	[SerializeField] protected GameObject ParentObjectSetter;
	
	protected GameObject Parent;
	protected GameObject[] Value;
	protected GameObject[] Symbol;
	protected SlotEffectMaker2023.Data.HistoryConfig hc;
	protected SlotEffectMaker2023.Action.HistoryManager hm;
	protected ReelChipHolder comaData;

    // Start is called before the first frame update
    virtual protected void Start()
    {
    	Parent = ParentObjectSetter;
    	if (Parent == null) Parent = this.gameObject;
    	
        Value = new GameObject[ShowNum];
        Symbol = new GameObject[ShowNum];
        
        Value[0] = Parent.transform.Find("Game").gameObject;
        Symbol[0] = Parent.transform.Find("Image").gameObject;
        
        for (int i=1; i<ShowNum; ++i){
        	Value[i] = Instantiate(Value[0], Parent.transform);
        	Value[i].transform.localPosition += new Vector3(0, DiffY * i, 0);
        	Symbol[i] = Instantiate(Symbol[0], Parent.transform);
        	Symbol[i].transform.localPosition += new Vector3(0, DiffY * i, 0);
        }
        
        hc = SlotEffectMaker2023.Singleton.EffectDataManagerSingleton.GetInstance().HistoryConf;
        hm = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance().historyManager;
        comaData = ReelChipHolder.GetInstance();
    }

    // Update is called once per frame
    virtual protected void Update()
    {
    	int refHist = 0;
    	for(int i = 0; i < ShowNum; ++i){
    		// 履歴データの抽出
    		while(refHist < hm.BonusHist.Count){
    			if (!hm.BonusHist[refHist].IsActivate) ++refHist;
    			else break;
    		}
    		UpdateData(i, refHist);
			++refHist;
    	}
    }
    
    virtual protected void UpdateData(int setPos, int refHist){
		if (refHist >= hm.BonusHist.Count){
    		// 履歴データが尽きた場合
			Value[setPos].SetActive(false);
			Symbol[setPos].SetActive(false);
		} else {
			// 履歴データ表示
			var refData = hm.BonusHist[refHist];
			Value[setPos].SetActive(true);
			Value[setPos].GetComponent<TextMeshProUGUI>().text = refData.InGame.ToString();
			Symbol[setPos].SetActive(true);
			Symbol[setPos].GetComponent<Image>().sprite = comaData.ReelChipData.Extract(hc.GetConfig(refData.BonusFlag).ComaID);
		}
    }
}
