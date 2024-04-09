using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIShowHistory : MonoBehaviour
{
	[SerializeField, Min(1)] private int ShowNum;
	[SerializeField] private float DiffY;
	
	private GameObject[] Value;
	private GameObject[] Symbol;
	private SlotEffectMaker2023.Data.HistoryConfig hc;
	private SlotEffectMaker2023.Action.HistoryManager hm;
	private ReelChipHolder comaData;

    // Start is called before the first frame update
    void Start()
    {
        Value = new GameObject[ShowNum];
        Symbol = new GameObject[ShowNum];
        
        Value[0] = transform.Find("Game").gameObject;
        Symbol[0] = transform.Find("Image").gameObject;
        
        for (int i=1; i<ShowNum; ++i){
        	Value[i] = Instantiate(Value[0], this.transform);
        	Value[i].transform.localPosition += new Vector3(0, DiffY * i, 0);
        	Symbol[i] = Instantiate(Symbol[0], this.transform);
        	Symbol[i].transform.localPosition += new Vector3(0, DiffY * i, 0);
        	
	        hc = SlotEffectMaker2023.Singleton.EffectDataManagerSingleton.GetInstance().HistoryConf;
	        hm = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance().historyManager;
	        comaData = ReelChipHolder.GetInstance();
        }
    }

    // Update is called once per frame
    void Update()
    {
    	int refHist = 0;
    	for(int i = 0; i < ShowNum; ++i){
    		// 履歴データの抽出
    		while(refHist < hm.BonusHist.Count){
    			if (!hm.BonusHist[refHist].IsActivate) ++refHist;
    			else break;
    		}
    		if (refHist >= hm.BonusHist.Count){
	    		// 履歴データが尽きた場合
    			Value[i].SetActive(false);
    			Symbol[i].SetActive(false);
    		} else {
    			// 履歴データ表示
    			var refData = hm.BonusHist[refHist];
    			Value[i].SetActive(true);
    			Value[i].GetComponent<TextMeshProUGUI>().text = refData.InGame.ToString();
    			Symbol[i].SetActive(true);
    			Symbol[i].GetComponent<Image>().sprite = comaData.ReelChipData.Extract(hc.GetConfig(refData.BonusFlag).ComaID);
    		}
    		++refHist;
    	}
    }
}
