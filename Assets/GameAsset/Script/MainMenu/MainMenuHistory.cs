using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MainMenuHistory : UIShowHistory
{
	private int ShowBegin;
	public int SelectedIndex { get; private set; }

	// ボーナス履歴追加データ
	protected GameObject[] Number;
	protected GameObject[] Get;
	
	// 成立時出目データ
	[Header("成立時出目データ")]
	[SerializeField] protected float ComaDX;
	[SerializeField] private GameObject InGameObject;
	private GameObject[] BonusInBG;
	private GameObject[] BonusInStopInfo;
	private GameObject[][] BonusInComaImg;
	private GameObject[][] BonusInComaID;
	private GameObject[] BonusInCutLine;
	private SlotMaker2022.LocalDataSet.ReelArray[][] ra;
	
    // Start is called before the first frame update
    protected override void Start()
    {
    	base.Start();
        Number = new GameObject[ShowNum];
        Get = new GameObject[ShowNum];
        
        Number[0] = Parent.transform.Find("Number").gameObject;
        Get[0] = Parent.transform.Find("Get").gameObject;
        
        for (int i=1; i<ShowNum; ++i){
        	Number[i] = Instantiate(Number[0], Parent.transform);
        	Number[i].transform.localPosition += new Vector3(0, DiffY * i, 0);
        	Get[i] = Instantiate(Get[0], Parent.transform);
        	Get[i].transform.localPosition += new Vector3(0, DiffY * i, 0);
        }
        
        // 参照中データ初期化
        ShowBegin = 0;
        SelectedIndex = 0;
        
        // 成立時出目データ初期化
        const int reelNum = SlotMaker2022.LocalDataSet.REEL_MAX;
        const int showComaNum = SlotMaker2022.LocalDataSet.SHOW_MAX;
		BonusInBG = new GameObject[reelNum];
		BonusInStopInfo = new GameObject[reelNum];
		BonusInCutLine = new GameObject[reelNum];
		BonusInComaImg = new GameObject[reelNum][];
		BonusInComaID = new GameObject[reelNum][];
		ra = SlotMaker2022.MainROMDataManagerSingleton.GetInstance().ReelArray;
		for (int i=0; i<reelNum; ++i){
			BonusInComaImg[i] = new GameObject[showComaNum];
			BonusInComaID[i] = new GameObject[showComaNum];
		}
		
		BonusInBG[0] = InGameObject.transform.Find("BG").gameObject;
		BonusInStopInfo[0] = InGameObject.transform.Find("Order").gameObject;
		BonusInCutLine[0] = InGameObject.transform.Find("Line").gameObject;
		BonusInComaImg[0][0] = InGameObject.transform.Find("ComaImg").gameObject;
		BonusInComaID[0][0] = InGameObject.transform.Find("ComaID").gameObject;
		
		for (int i=0; i<reelNum; ++i){
			if (i > 0){
				BonusInBG[i] = Instantiate(BonusInBG[0], InGameObject.transform);
	        	BonusInBG[i].transform.localPosition += new Vector3(ComaDX * i, 0, 0);
				BonusInStopInfo[i] = Instantiate(BonusInStopInfo[0], InGameObject.transform);
	        	BonusInStopInfo[i].transform.localPosition += new Vector3(ComaDX * i, 0, 0);
				BonusInCutLine[i] = Instantiate(BonusInCutLine[0], InGameObject.transform);
	        	BonusInCutLine[i].transform.localPosition += new Vector3(ComaDX * i, 0, 0);
	        }
			for (int j = i==0 ? 1 : 0; j<showComaNum; ++j){
				BonusInComaImg[i][j] = Instantiate(BonusInComaImg[0][0], InGameObject.transform);
				BonusInComaImg[i][j].transform.localPosition += new Vector3(ComaDX * i, BonusInComaImg[0][0].GetComponent<RectTransform>().sizeDelta.y * j, 0);
				BonusInComaID[i][j] = Instantiate(BonusInComaID[0][0], InGameObject.transform);
				BonusInComaID[i][j].transform.localPosition += new Vector3(ComaDX * i, BonusInComaImg[0][0].GetComponent<RectTransform>().sizeDelta.y * j, 0);
			}
		}
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        
        // 成立時出目表示
        if (SelectedIndex >= 0 && SelectedIndex < hm.BonusHist.Count && hm.BonusHist[SelectedIndex].IsActivate){
        	string[] orderPtn = { "", "1st", "2nd", "3rd" };
        	var refData = hm.BonusHist[SelectedIndex];
        	var nowPtn = refData.InPattern;
        	InGameObject.transform.Find("ReelBase").gameObject.SetActive(true);
        	InGameObject.transform.Find("BetCount").GetComponent<TextMeshProUGUI>().text = nowPtn.BetNum.ToString() + "BET";
        	InGameObject.transform.Find("LossGame").GetComponent<TextMeshProUGUI>().text = "Loss game: " + refData.LossGame.ToString();
        	for(int i=0; i<BonusInBG.Length; ++i){
	        	BonusInBG[i].SetActive(true);
	        	BonusInStopInfo[i].SetActive(true);
	        	BonusInCutLine[i].SetActive(false);
        		BonusInStopInfo[i].GetComponent<TextMeshProUGUI>().text = orderPtn[nowPtn.StopOrder[i]] + " - [" + nowPtn.SlipCount[i].ToString() + "]";
        		for(int j=0; j<BonusInComaImg[i].Length; ++j){
        			int showComa = (j + nowPtn.ReelPos[i]) % SlotMaker2022.LocalDataSet.COMA_MAX;
        			BonusInComaImg[i][j].SetActive(true);
        			BonusInComaID[i][j].SetActive(true);
        			// データは逆順に格納されていることに注意する。
        			BonusInComaImg[i][j].GetComponent<Image>().sprite = comaData.ReelChipData.Extract(ra[i][SlotMaker2022.LocalDataSet.COMA_MAX - showComa - 1].Coma);
        			BonusInComaID[i][j].transform.Find("Text").GetComponent<TextMeshProUGUI>().text = (showComa + 1).ToString();
        			if (showComa == 0){
        				BonusInCutLine[i].SetActive(true);
        				Vector3 pos = BonusInCutLine[i].transform.localPosition;
        				pos.y = BonusInBG[i].transform.localPosition.y + BonusInComaImg[0][0].GetComponent<RectTransform>().sizeDelta.y * j;
        				BonusInCutLine[i].transform.localPosition = pos;
        			}
        		}
        	}
        } else {
        	// 表示するデータがない場合
        	InGameObject.transform.Find("ReelBase").gameObject.SetActive(false);
        	InGameObject.transform.Find("BetCount").GetComponent<TextMeshProUGUI>().text = string.Empty;
        	InGameObject.transform.Find("LossGame").GetComponent<TextMeshProUGUI>().text = "NO DATA";
        	for(int i=0; i<BonusInBG.Length; ++i){
	        	BonusInBG[i].SetActive(false);
	        	BonusInStopInfo[i].SetActive(false);
	        	BonusInCutLine[i].SetActive(false);
	        	for (int j = 0; j<BonusInComaImg[i].Length; ++j){
	        		BonusInComaImg[i][j].SetActive(false);
	        		BonusInComaID[i][j].SetActive(false);
	        	}
        	}
        }
    }
    
    protected override void UpdateData(int setPos, int refHist){
		if (refHist >= hm.BonusHist.Count){
    		// 履歴データが尽きた場合
			Value[setPos].SetActive(false);
			Symbol[setPos].SetActive(false);
			Number[setPos].SetActive(false);
			Get[setPos].SetActive(false);
		} else {
			// 履歴データ表示
			var refData = hm.BonusHist[refHist];
			Value[setPos].SetActive(true);
			Value[setPos].GetComponent<TextMeshProUGUI>().text = refData.InGame.ToString();
			Symbol[setPos].SetActive(true);
			Symbol[setPos].GetComponent<Image>().sprite = comaData.ReelChipData.Extract(hc.GetConfig(refData.BonusFlag).ComaID);
			Number[setPos].SetActive(true);
			Number[setPos].GetComponent<TextMeshProUGUI>().text = (hm.BonusHist.Count - SelectedIndex).ToString();
			Get[setPos].SetActive(refData.IsFinished);
			Get[setPos].GetComponent<TextMeshProUGUI>().text = (refData.MedalAfter - refData.MedalBefore).ToString();
		}
    }    
    public void ShowBeginUpdate(float pBarPosY){
    	
    }
}
