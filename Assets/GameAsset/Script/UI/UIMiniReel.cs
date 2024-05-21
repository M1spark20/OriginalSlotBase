using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMiniReel : MonoBehaviour
{
	[SerializeField] private GameObject ComaBase;
	[SerializeField] private GameObject PosBase;
	
	private GameObject[,] Coma;
	private GameObject[,] Pos;
	private RectTransform[,] PosCtrl;
	private Vector2 comaSize;
	
	private const int PosShowNum = 2;
	private const int reelNum = SlotMaker2022.LocalDataSet.REEL_MAX;
	private const int comaNum = SlotMaker2022.LocalDataSet.COMA_MAX;
	
	private SlotEffectMaker2023.Action.ReelBasicData[] ReelData;
	
    // Start is called before the first frame update
    void Start()
    {
    	
        Coma = new GameObject[reelNum, comaNum];
        Pos  = new GameObject[reelNum, PosShowNum];
        PosCtrl = new RectTransform[reelNum, PosShowNum];
        ReelData = new SlotEffectMaker2023.Action.ReelBasicData[reelNum];
        
        var ra = SlotMaker2022.MainROMDataManagerSingleton.GetInstance().ReelArray;
        var comaData = ReelChipHolder.GetInstance();
        comaSize = ComaBase.GetComponent<RectTransform>().sizeDelta;
        
        for(int reelC = 0; reelC < reelNum; ++reelC){
        	ReelData[reelC] = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance().reelData[reelC];
        	
        	for (int comaC = 0; comaC < comaNum; ++comaC){
        		Coma[reelC, comaC] = (reelC == 0 && comaC == 0) ? ComaBase : Instantiate(ComaBase, ComaBase.transform.parent.transform);
        		Coma[reelC, comaC].GetComponent<Image>().sprite = comaData.ReelChipDataMini.Extract(ra[reelC][comaC].Coma);
        		var pos = Coma[reelC, comaC].GetComponent<RectTransform>().anchoredPosition;
        		pos += new Vector2(reelC * comaSize.x, -comaC * comaSize.y);
        		Coma[reelC, comaC].GetComponent<RectTransform>().anchoredPosition = pos;
        	}
        	for (int posC = 0; posC < PosShowNum; ++posC){
        		Pos[reelC, posC] = (reelC == 0 && posC == 0) ? PosBase : Instantiate(PosBase, PosBase.transform.parent.transform);
        		PosCtrl[reelC, posC] = Pos[reelC, posC].GetComponent<RectTransform>();
        		var pos = PosCtrl[reelC, posC].anchoredPosition;
        		pos += new Vector2(reelC * comaSize.x, -posC * comaNum * comaSize.y);
        		PosCtrl[reelC, posC].anchoredPosition = pos;
        	}
        }
        
        RefreshPos();
    }

    // Update is called once per frame
    void Update()
    {
        RefreshPos();
    }
    
    private void RefreshPos(){
        for(int reelC = 0; reelC < reelNum; ++reelC){
        	for (int posC = 0; posC < PosShowNum; ++posC){
        		var pos = PosCtrl[reelC, posC].anchoredPosition;
        		pos.y = (ReelData[reelC].reelPos - posC * comaNum) * comaSize.y;
        		PosCtrl[reelC, posC].anchoredPosition = pos;
        	}
        }
    	
    }
}
