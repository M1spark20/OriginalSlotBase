using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Localization;

public class MainMenuHistory : MainMenuElemBase
{
	[SerializeField] private GameObject PatternElem;
	[SerializeField] private GameObject HistoryViewer;
	[SerializeField] private GameObject Date;
	[SerializeField] private UIBalanceGraph GraphScript;
	[SerializeField] private UnityEngine.Localization.LocalizedStringTable LocalizeTable;
	[SerializeField] private string LocalizeID;
	
	private SlotEffectMaker2023.Action.HistoryManager hm;
	private ReelPatternBuilder builder;
	private UISmartScroller scroller;
	private TextMeshProUGUI dateShow;
	private UnityEngine.Localization.Tables.StringTable lc;
	
	private int lastShow;
	
    // Start is called before the first frame update
    protected override void Awake()
    {
    	base.Awake();
    	hm = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance().historyManager;
    	builder = PatternElem?.GetComponent<ReelPatternBuilder>() ?? null;
    	scroller = HistoryViewer?.GetComponent<UISmartScroller>() ?? null;
    	dateShow = Date?.GetComponent<TextMeshProUGUI>() ?? null;
    	lastShow = int.MinValue;
    	lc = null;
    	if (LocalizeTable.IsEmpty) Debug.Log("a");
    }
    
    // Update is called once per frame
    private void Update()
    {
    	if (hm.BonusHist.Count == 0 || scroller.ContentCount <= 0) return;
    	int nowShow = scroller.SelectedIndex;
    	if (nowShow != lastShow){
    		ShowPattern(scroller.SelectedIndex);
    		lastShow = nowShow;
    	}
    }
    
    public override void RefreshData(){
    	// サイズ取得
    	int size = hm.BonusHist.Count;
    	int offset = size > 0 ? (hm.BonusHist[0].IsActivate ? 0 : 1) : 0;
    	// サイズ指定とIndex更新
    	scroller.SetContentSize(size - offset, offset);
    	// データ全更新、選択データを表示させる
    	scroller.ElemUpdate(true);
    	scroller.MoveSelectedCenter();
    	// 成立時出目更新
    	ShowPattern(scroller.SelectedIndex);
    	// グラフ更新(なぜか初回だけnullになる…)
    	GraphScript?.GraphDraw();
    }
    
    private void ShowPattern(int nowShow) {
    	// サイズ取得
    	int size = hm.BonusHist.Count;
    	int offset = size > 0 ? (hm.BonusHist[0].IsActivate ? 0 : 1) : 0;
    	dateShow.text = string.Empty;
    	if (size - offset == 0) builder.SetData(null, "NO DATA"); // 1回目の当たりは入賞までここでマスクされる
    	else if (nowShow < 0 || nowShow >= hm.BonusHist.Count) builder.SetData(null, "Select Bonus Data");
    	else {
    		if (lc == null) lc = LocalizeTable.GetTable();
    		builder.SetData(hm.BonusHist[nowShow].InPattern, string.Format(lc[LocalizeID].Value, hm.BonusHist[nowShow].LossGame.ToString()));
    		dateShow.text = hm.BonusHist[nowShow].InDate;
    	}
    }
    
    public override void OnGetKeyDown(EMenuButtonID eKeyID){
    	if (eKeyID == EMenuButtonID.eScrUp) {
			scroller.SetSelectedByKey(-1);
			scroller.MoveSelectedCenter();
    	} else if (eKeyID == EMenuButtonID.eScrDn) {
			scroller.SetSelectedByKey(1);
			scroller.MoveSelectedCenter();
		}
    }
}
