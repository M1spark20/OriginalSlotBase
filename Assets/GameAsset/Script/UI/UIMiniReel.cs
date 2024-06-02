using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMiniReel : MonoBehaviour
{
	[SerializeField] private GameObject ComaBase;
	[SerializeField] private GameObject PosBase;
	[SerializeField] private GameObject TargetBase;
	[SerializeField] float DiffX;
	[SerializeField] Vector2 PosCtrlOffset;
	
	private GameObject[,] Coma;
	private GameObject[,] Pos;
	private GameObject[] Target;
	private RectTransform[,] PosCtrl;
	private RectTransform[] TargetCtrl;
	private Image[] TargetImg;
	private Vector2 comaSize;
	
	private const int PosShowNum = 2;
	private const int reelNum = SlotMaker2022.LocalDataSet.REEL_MAX;
	private const int comaNum = SlotMaker2022.LocalDataSet.COMA_MAX;
	private const int reelNP = SlotEffectMaker2023.Action.ReelBasicData.REEL_NPOS;
	
	private SlotEffectMaker2023.Action.ReelBasicData[] ReelData;
	private SlotEffectMaker2023.Action.SlotTimerManager timer;
	
	// ターゲット設定用変数(20240602追加)
	private int targetReel;
	private int targetComa;
	private int lastComaPos;
	
    // Start is called before the first frame update
    void Start()
    {
    	
        Coma = new GameObject[reelNum, comaNum];
        Pos  = new GameObject[reelNum, PosShowNum];
        Target = new GameObject[PosShowNum];
        PosCtrl = new RectTransform[reelNum, PosShowNum];
        TargetCtrl = new RectTransform[PosShowNum];
        TargetImg = new Image[PosShowNum];
        ReelData = new SlotEffectMaker2023.Action.ReelBasicData[reelNum];
        targetReel = -1;
        targetComa = -1;
        lastComaPos = -1;
        
        var ra = SlotMaker2022.MainROMDataManagerSingleton.GetInstance().ReelArray;
        var comaData = ReelChipHolder.GetInstance();
        comaSize = ComaBase.GetComponent<RectTransform>().sizeDelta;
        timer = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance().timerData;
        
        for(int reelC = 0; reelC < reelNum; ++reelC){
        	ReelData[reelC] = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance().reelData[reelC];
        	
        	for (int comaC = 0; comaC < comaNum; ++comaC){
        		Coma[reelC, comaC] = (reelC == 0 && comaC == 0) ? ComaBase : Instantiate(ComaBase, ComaBase.transform.parent.transform);
        		Coma[reelC, comaC].GetComponent<Image>().sprite = comaData.ReelChipDataMini.Extract(ra[reelC][comaC].Coma);
        		var pos = Coma[reelC, comaC].GetComponent<RectTransform>().anchoredPosition;
        		pos += new Vector2(reelC * DiffX, -comaC * comaSize.y);
        		Coma[reelC, comaC].GetComponent<RectTransform>().anchoredPosition = pos;
        		// ボタンの登録(変数に入れないとデルタがうまく動かないらしい…)
	        	var r = reelC;
	        	var c = comaC;
	        	Coma[reelC, comaC].GetComponent<Button>().onClick.AddListener(() => OnClickButton(r, c));
        	}
        	for (int posC = 0; posC < PosShowNum; ++posC){
        		Pos[reelC, posC] = (reelC == 0 && posC == 0) ? PosBase : Instantiate(PosBase, PosBase.transform.parent.transform);
        		PosCtrl[reelC, posC] = Pos[reelC, posC].GetComponent<RectTransform>();
        		var pos = PosCtrl[reelC, posC].anchoredPosition;
        		pos += new Vector2(reelC * DiffX, -posC * comaNum * comaSize.y);
        		PosCtrl[reelC, posC].anchoredPosition = pos;
        	}
        }
        
        // ターゲット生成
    	for (int posC = 0; posC < PosShowNum; ++posC){
    		Target[posC] = posC == 0 ? TargetBase : Instantiate(TargetBase, TargetBase.transform.parent.transform);
	        TargetCtrl[posC] = Target[posC].GetComponent<RectTransform>();
	        TargetImg[posC] = Target[posC].GetComponent<Image>();
        }
        
        RefreshPos();
    }

    // Update is called once per frame
    void Update()
    {
        RefreshPos();
        // アシスト音を鳴らす
        if (targetReel == -1 || targetComa == -1) return;
        var comaData = ReelData[targetReel];
        if (!comaData.isRotate || !comaData.accEnd || comaData.pushPos != reelNP) return;	// ボタン無効時は音を鳴らさない
        // コマがTargetに到達した瞬間にだけ音を鳴らす
        int comaPos = comaData.GetReelComaIDFixed();
        if (comaPos != lastComaPos && comaPos == targetComa) {
        	// AssistSoundタイマをリセットすることによる
			timer.GetTimer("AssistSound").Activate();
			timer.GetTimer("AssistSound").Reset();
		}
        lastComaPos = comaPos;
    }
    
    private void RefreshPos(){
        for(int reelC = 0; reelC < reelNum; ++reelC){
        	for (int posC = 0; posC < PosShowNum; ++posC){
        		var pos = PosCtrl[reelC, posC].anchoredPosition;
        		pos.y = (ReelData[reelC].reelPos - posC * comaNum) * comaSize.y + PosCtrlOffset.y;
        		PosCtrl[reelC, posC].anchoredPosition = pos;
        	}
        }
        // アシスト窓位置変更
        for (int posC = 0; posC < PosShowNum; ++posC){
	        TargetImg[posC].enabled = (targetReel >= 0 && targetComa >= 0);
	        TargetCtrl[posC].anchoredPosition = new Vector2(targetReel * DiffX, (targetComa - posC * comaNum) * comaSize.y) + PosCtrlOffset;
        }
    }
    
    public void OnClickButton(int reelC, int comaC){
    	// コマ位置の上下反転
    	comaC = comaNum - comaC - 1;
    	// 重複なら指定解除・それ以外ならアシスト指定
    	if (reelC == targetReel && comaC == targetComa) {
	        targetReel = -1;
	        targetComa = -1;
    	} else {
	        targetReel = reelC;
	        targetComa = comaC;
	        lastComaPos = -1;
    	}
    }
}
