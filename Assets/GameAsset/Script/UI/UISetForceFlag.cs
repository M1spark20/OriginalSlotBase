using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UISetForceFlag : MonoBehaviour
{
	[SerializeField] private Button EnableButton;
	[SerializeField] private GameObject BaseObject;
	[SerializeField] private Vector2 Interval;
	// #*でリール図柄を、その他で文字を表示。","で分割
	[SerializeField] private string BonusShow;
	[SerializeField] private string MinorShow;
	// 改行位置の指定
	[SerializeField] private int ShowReturnPos;
	
	private GameObject[] selectorB;
	private GameObject[] selectorM;
	private Image   imEn;
	private Image[] imB;
	private Image[] imM;
	private TextMeshProUGUI   txEn;
	private TextMeshProUGUI[] txB;
	private TextMeshProUGUI[] txM;
	private SlotEffectMaker2023.Action.SystemData sys;
	
	private void Start(){
		// 表示内容をデコードする
		string[] bsBDec = BonusShow.Split(",");
		string[] bsMDec = MinorShow.Split(",");
		int BonusCount = bsBDec.Length;
		int MinorCount = bsMDec.Length;
		
		// 初期化
		selectorB = new GameObject[BonusCount];
		imB = new Image[BonusCount];
		txB = new TextMeshProUGUI[BonusCount];
		selectorM = new GameObject[MinorCount];
		imM = new Image[MinorCount];
		txM = new TextMeshProUGUI[MinorCount];
		imEn = EnableButton.GetComponent<Image>();
		txEn = EnableButton.transform.Find("IDText").GetComponent<TextMeshProUGUI>();
		// Singleton初期化
		sys = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance().sysData;
		var comaData = ReelChipHolder.GetInstance();	// Singleton
		
		// Instantiateする(Bonus)
		for(int i=0; i<BonusCount; ++i){
			selectorB[i] = i == 0 ? BaseObject : Instantiate(BaseObject, this.transform);
			selectorB[i].GetComponent<RectTransform>().anchoredPosition += new Vector2(Interval.x * i, 0);
			
			// 画像を載せて文字を更新する
			imB[i] = selectorB[i].transform.Find("Image") .GetComponent<Image>();
			txB[i] = selectorB[i].transform.Find("Button").transform.Find("IDText").GetComponent<TextMeshProUGUI>();
			if (bsBDec[i][0] == '#'){
				imB[i].enabled = true;
				imB[i].sprite = comaData.ReelChipDataMini.Extract((int)(bsBDec[i][1] - '0'));
				txB[i].text = bsBDec[i].Length > 2 ? bsBDec[i].Substring(2, bsBDec[i].Length - 2) : "";
			} else {
				imB[i].enabled = false;
				txB[i].text = bsBDec[i];
			}
			// フォーカスをボタンに戻す
			imB[i] = selectorB[i].transform.Find("Button").GetComponent<Image>();
			
			// ボタン押下時のスクリプト登録
        	var prm = i-1;	// 変数に入れないとデルタがうまく動かないらしい…
        	selectorB[i].transform.Find("Button").GetComponent<Button>().onClick.AddListener(() => OnClickButtonB(prm));
		}
		
		// Instantiateする(Minor)
		for(int i=0; i<MinorCount; ++i){
        	Debug.Log(bsMDec[i]);
			selectorM[i] = Instantiate(BaseObject, this.transform);
			selectorM[i].GetComponent<RectTransform>().anchoredPosition += new Vector2(Interval.x * (i%ShowReturnPos), Interval.y * (1+(int)(i/ShowReturnPos)));
			
			// 画像を載せて文字を更新する
			imM[i] = selectorM[i].transform.Find("Image") .GetComponent<Image>();
			txM[i] = selectorM[i].transform.Find("Button").transform.Find("IDText").GetComponent<TextMeshProUGUI>();
			if (bsMDec[i][0] == '#'){
				imM[i].enabled = true;
				imM[i].sprite = comaData.ReelChipDataMini.Extract((int)(bsMDec[i][1] - '0'));
				txM[i].text = bsMDec[i].Length > 2 ? bsMDec[i].Substring(2, bsMDec[i].Length - 2) : "";
			} else {
				imM[i].enabled = false;
				txM[i].text = bsMDec[i];
			}
			// フォーカスをボタンに戻す
			imM[i] = selectorM[i].transform.Find("Button").GetComponent<Image>();
			
			// ボタン押下時のスクリプト登録
        	var prm = i-1;	// 変数に入れないとデルタがうまく動かないらしい…
        	selectorM[i].transform.Find("Button").GetComponent<Button>().onClick.AddListener(() => OnClickButtonM(prm));
		}
	}
	
	private void Update(){
		Color cl = sys.ForceFlagEnable ? Color.yellow : Color.white;
		imEn.color = cl;
		txEn.color = cl;
		
		for (int i=0; i<selectorB.Length; ++i){
			selectorB[i].SetActive(sys.ForceFlagEnable);
			cl = (sys.ForceFlagEnable && sys.ForceFlagBonus == i-1) ? Color.yellow : Color.white;
			imB[i].color = cl;
			txB[i].color = cl;
		}
		for (int i=0; i<selectorM.Length; ++i){
			selectorM[i].SetActive(sys.ForceFlagEnable);
			cl = (sys.ForceFlagEnable && sys.ForceFlagMinor == i-1) ? Color.yellow : Color.white;
			imM[i].color = cl;
			txM[i].color = cl;
		}
	}
	
	// 強制フラグ有効化
	public void OnClickButtonEn(){
		sys.ForceFlagEnable = true;
	}
	// ボーナスフラグ設定
	public void OnClickButtonB(int index){
		sys.ForceFlagBonus = index;
	}
	// 小役フラグ設定
	public void OnClickButtonM(int index){
		sys.ForceFlagMinor = index;
	}
}
