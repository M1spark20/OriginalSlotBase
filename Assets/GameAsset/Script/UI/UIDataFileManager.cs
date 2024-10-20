using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIDataFileManager : MonoBehaviour
{
	[SerializeField] private Button FileButtonBase;
	[SerializeField] private Vector2 Interval;
	[SerializeField] private TextMeshProUGUI FileChangeInfo;
	[SerializeField] private Button[] ResetBtnUI;
	[SerializeField] private GraphicRaycaster RefRaycaster;
	[SerializeField] private TextMeshProUGUI Explanation;
	
	const int SAVEFILE_NUM = 10;
	private Button[] BtnsFile;
	private Image[] ImFile;
	private TextMeshProUGUI[] TextFile;
	
	private Image[] ImReset;
	private TextMeshProUGUI[] TextReset;
	private SlotEffectMaker2023.Action.SystemData sys;
	private byte defaultSaveID;
	
	// Start is called before the first frame update
    void Start()
    {
    	BtnsFile = new Button[SAVEFILE_NUM];
    	ImFile = new Image[SAVEFILE_NUM];
    	TextFile = new TextMeshProUGUI[SAVEFILE_NUM];
		// Instantiateする
		for(int i=0; i<SAVEFILE_NUM; ++i){
			BtnsFile[i] = i == 0 ? FileButtonBase : Instantiate(FileButtonBase, this.transform);
			ImFile[i] = BtnsFile[i].GetComponent<Image>();
			TextFile[i] = BtnsFile[i].transform.Find("IDText").GetComponent<TextMeshProUGUI>();
			
			BtnsFile[i].GetComponent<RectTransform>().anchoredPosition += new Vector2(Interval.x * i, 0);
			TextFile[i].text = (i+1).ToString();
			
			// ボタン押下時のスクリプト登録
        	byte prm = (byte)i;	// 変数に入れないとデルタがうまく動かないらしい…
        	BtnsFile[i].onClick.AddListener(() => ChangeFileID(prm));
		}    	
    	ImReset = new Image[ResetBtnUI.Length];
    	TextReset = new TextMeshProUGUI[ResetBtnUI.Length];
    	for(int i=0; i<ResetBtnUI.Length; ++i){
    		ImReset[i] = ResetBtnUI[i].GetComponent<Image>();
    		TextReset[i] = ResetBtnUI[i].transform.Find("IDText").GetComponent<TextMeshProUGUI>();
    	}
    	
    	sys = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance().sysData;
    	ChangeShow(sys.ResetFlag);
    	defaultSaveID = sys.UseSaveDataID;
    }
    
    void Update(){
    	// 画面が有効かを常時監視する
    	if (RefRaycaster != null) sys.ResetFlag &= RefRaycaster.enabled;
    	if (!RefRaycaster.enabled) sys.UseSaveDataID = defaultSaveID;
    	ChangeShow(sys.ResetFlag);
    	
    	// File描画(Reset有効時はボタンをクリックさせない)
    	if (sys.ResetFlag) ChangeFileID(defaultSaveID);
		for (int i=0; i<BtnsFile.Length; ++i){
			BtnsFile[i].interactable = !sys.ResetFlag;
			Color cl = sys.UseSaveDataID == i ? Color.yellow : Color.white;
			ImFile[i].color = cl;
			TextFile[i].color = cl;
		}
		FileChangeInfo.enabled = (sys.UseSaveDataID != defaultSaveID);
    }
    
    public void ChangeFileID(byte changeFor){
    	sys.UseSaveDataID = changeFor;
    }
    
	public void ChangeShow(bool enabled){
		sys.ResetFlag = enabled;
		for(int i=0; i<ResetBtnUI.Length; ++i){
    		Color itemCol = Color.white;
    		if (i == 0 && !sys.ResetFlag) itemCol = Color.yellow;
    		if (i == 1 && sys.ResetFlag) itemCol = Color.red;
    		ImReset[i].color = itemCol;
    		TextReset[i].color = itemCol;
		}
		Explanation.enabled = sys.ResetFlag;
	}
}
