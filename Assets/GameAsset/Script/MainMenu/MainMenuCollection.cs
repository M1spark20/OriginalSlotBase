using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MainMenuCollection : MainMenuElemBase
{
	[SerializeField] private GameObject HistoryViewer;
	
	private SlotEffectMaker2023.Data.CollectionData cd;
	private UISmartScroller scroller;
	
    // Start is called before the first frame update
    protected override void Awake()
    {
    	base.Awake();
    	cd = SlotEffectMaker2023.Singleton.EffectDataManagerSingleton.GetInstance().Collection;
    	scroller = HistoryViewer?.GetComponent<UISmartScroller>() ?? null;
    }
    
    // Update is called once per frame
    private void Update() { }
    
    protected override void RefreshData(){
    	// サイズ取得
    	int size = (cd.Collections.Count + 3) / 4;	// 4コ毎に更新するためsizeを4で割る、端数切り上げ
    	// サイズ指定とIndex更新
    	scroller.SetContentSize(size, 0);
    	// データ全更新
    	scroller.ElemUpdate(true);
    }
    
    public override void OnGetKeyDown(EMenuButtonID eKeyID){
    	if (eKeyID == EMenuButtonID.eScrUp) {
			scroller.MovePosition(-1/3f);
    	} else if (eKeyID == EMenuButtonID.eScrDn) {
			scroller.MovePosition( 1/3f);
		}
    }
}
