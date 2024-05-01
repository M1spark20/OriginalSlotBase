using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MainMenuPattern : MainMenuElemBase
{
	[SerializeField] private GameObject HistoryViewer;
	
	private SlotEffectMaker2023.Action.HistoryManager hm;
	private UISmartScroller scroller;
	
    // Start is called before the first frame update
    protected override void Awake()
    {
    	base.Awake();
    	hm = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance().historyManager;
    	scroller = HistoryViewer?.GetComponent<UISmartScroller>() ?? null;
    }
    
    // Update is called once per frame
    private void Update() { }
    
    public override void RefreshData(){
    	// サイズ取得
    	int size = (hm.PatternHist.Count + 1) / 2;	// 2コ毎に更新するためsizeを2で割る、端数切り上げ
    	// サイズ指定とIndex更新
    	scroller.SetContentSize(size, 0);
    	// データ全更新
    	scroller.ElemUpdate(true);
    }
    
    public override void OnGetKeyDown(EMenuButtonID eKeyID){
    	if (eKeyID == EMenuButtonID.eScrUp) {
			scroller.MovePosition(-0.5f);
    	} else if (eKeyID == EMenuButtonID.eScrDn) {
			scroller.MovePosition(0.5f);
		}
    }
}
