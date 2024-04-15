using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIShowHistory : MonoBehaviour
{
	[SerializeField, Min(1)] private int ShowNum;
	[SerializeField] private float DiffY;
	[SerializeField] private GameObject ParentObjectSetter;
	
	private GameObject Parent;
	private GameObject[] Value;
	private GameObject[] Symbol;
	private GameObject[] Number;
	private GameObject[] Get;
	private SlotEffectMaker2023.Data.HistoryConfig hc;
	private SlotEffectMaker2023.Action.HistoryManager hm;
	private ReelChipHolder comaData;
	
	public int ShowBeginID { get; private set; }
	public int SelectedIndex { get; private set; }
	
	// 更新確認用変数
	private int LastHistCount;
	private bool TopActivate;
	private bool TopFinished;

    // Start is called before the first frame update
    private void Start()
    {
    	Parent = ParentObjectSetter;
    	if (Parent == null) Parent = this.gameObject;
    	
        Value = new GameObject[ShowNum];
        Symbol = new GameObject[ShowNum];
        Number = new GameObject[ShowNum];
        Get = new GameObject[ShowNum];

        Value[0] = null;
        Value[0] = Parent.transform.Find("Game")?.gameObject;
        Symbol[0] = null;
        Symbol[0] = Parent.transform.Find("Image")?.gameObject;
        Number[0] = null;
        Number[0] = Parent.transform.Find("Number")?.gameObject;
        Get[0] = null;
        Get[0] = Parent.transform.Find("Get")?.gameObject;

        for (int i=1; i<ShowNum; ++i){
        	if (Value[0] != null) {
	        	Value[i] = Instantiate(Value[0], Parent.transform);
	        	Value[i].transform.localPosition += new Vector3(0, DiffY * i, 0);
	        } else { Value[i] = null; }
	        if (Symbol[0] != null) {
	        	Symbol[i] = Instantiate(Symbol[0], Parent.transform);
	        	Symbol[i].transform.localPosition += new Vector3(0, DiffY * i, 0);
	        } else { Symbol[i] = null; }
	        if (Number[0] != null) {
	        	Number[i] = Instantiate(Number[0], Parent.transform);
	        	Number[i].transform.localPosition += new Vector3(0, DiffY * i, 0);
	        } else { Number[i] = null; }
	        if (Get[0] != null) {
	        	Get[i] = Instantiate(Get[0], Parent.transform);
	        	Get[i].transform.localPosition += new Vector3(0, DiffY * i, 0);
	        } else { Get[i] = null; }
        }
        
        hc = SlotEffectMaker2023.Singleton.EffectDataManagerSingleton.GetInstance().HistoryConf;
        hm = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance().historyManager;
        comaData = ReelChipHolder.GetInstance();
        
        ShowBeginID = 0;
        SelectedIndex = 0;
        
        LastHistCount = -1; // 初回更新を行うために0未満の値を入れる
        TopActivate = LastHistCount > 0 ? hm.BonusHist[0].IsActivate : true;
        TopFinished = LastHistCount > 0 ? hm.BonusHist[0].IsFinished : true;
    }

    // Update is called once per frame
    private void Update()
    {
    	bool refresh = LastHistCount != hm.BonusHist.Count;
    	if (hm.BonusHist.Count > 0) {
    		refresh |= TopActivate ^ hm.BonusHist[0].IsActivate;
    		refresh |= TopFinished ^ hm.BonusHist[0].IsFinished;
    	}
    	if (refresh) OnRefresh();
    	
        LastHistCount = hm.BonusHist.Count;
        TopActivate = LastHistCount > 0 ? hm.BonusHist[0].IsActivate : true;
        TopFinished = LastHistCount > 0 ? hm.BonusHist[0].IsFinished : true;
    }
    
    private void OnRefresh(){
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
    
    private void UpdateData(int setPos, int refHist){
		if (refHist >= hm.BonusHist.Count){
    		// 履歴データが尽きた場合
			Value[setPos]?.SetActive(false);
			Symbol[setPos]?.SetActive(false);
			Number[setPos]?.SetActive(false);
			Get[setPos]?.SetActive(false);
		} else {
			// 履歴データ表示
			var refData = hm.BonusHist[refHist];
			if (Value[setPos] != null) {
				Value[setPos].SetActive(true);
				Value[setPos].GetComponent<TextMeshProUGUI>().text = refData.InGame.ToString();
			}
			if (Symbol[setPos] != null) {
				Symbol[setPos].SetActive(true);
				Symbol[setPos].GetComponent<Image>().sprite = comaData.ReelChipData.Extract(hc.GetConfig(refData.BonusFlag).ComaID);
			}
			if (Number[setPos] != null) {
				Number[setPos].SetActive(false);
				Number[setPos].GetComponent<TextMeshProUGUI>().text = (hm.BonusHist.Count - SelectedIndex).ToString();
			}
			if (Get[setPos] != null) {
				Get[setPos].SetActive(refData.IsFinished);
				Get[setPos].GetComponent<TextMeshProUGUI>().text = (refData.MedalAfter - refData.MedalBefore).ToString();
			}
		}
    }
}
