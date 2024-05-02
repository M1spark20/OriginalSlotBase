using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MainMenuCollection : MainMenuElemBase
{
	[SerializeField] private GameObject HistoryViewer;
	[SerializeField] private GameObject RecentViewer;
	[SerializeField] private TextMeshProUGUI AchieveCount;
	[SerializeField] private RectTransform AchieveGraph;
	
	private SlotEffectMaker2023.Data.CollectionData cd;
	private SlotEffectMaker2023.Action.CollectionLogger log;
	private UISmartScroller scroller;
	private UIShowPatternColleRecent recentScr;
	
    // Start is called before the first frame update
    protected override void Awake()
    {
    	base.Awake();
    	cd = SlotEffectMaker2023.Singleton.EffectDataManagerSingleton.GetInstance().Collection;
    	log = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance().collectionManager;
    	scroller = HistoryViewer?.GetComponent<UISmartScroller>() ?? null;
    	recentScr = RecentViewer.GetComponent<UIShowPatternColleRecent>();
    }
    
    // Update is called once per frame
    private void Update() { }
    
    public override void RefreshData(){
    	// サイズ取得
    	int size = (cd.Collections.Count + 3) / 4;	// 4コ毎に更新するためsizeを4で割る、端数切り上げ
    	// サイズ指定とIndex更新
    	scroller.SetContentSize(size, 0);
    	// データ全更新
    	scroller.ElemUpdate(true);
    	
    	// 最近達成したデータ更新
    	recentScr.RefreshData(0, false);
    	// 達成数とグラフ更新
    	int percent = log.GetAchievedCount() * 100 / cd.Collections.Count;
    	AchieveCount.text = log.GetAchievedCount().ToString("D03") + " / " + cd.Collections.Count.ToString("D03") + " (" + percent.ToString("D03") + "%)";
    	AchieveGraph.localScale = new Vector2(percent / 100f, 1f);
    }
    
    public override void OnGetKeyDown(EMenuButtonID eKeyID){
    	if (eKeyID == EMenuButtonID.eScrUp) {
			scroller.MovePosition(-1/2f);
    	} else if (eKeyID == EMenuButtonID.eScrDn) {
			scroller.MovePosition( 1/2f);
		}
    }
}
