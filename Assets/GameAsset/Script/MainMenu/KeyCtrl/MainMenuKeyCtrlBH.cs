using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuKeyCtrlBH : MainMenuKeyCtrlBase
{
	UISmartScroller Hist;
	
    // Start is called before the first frame update
    protected override void Start()
    {
    	base.Start();
        Hist = transform.Find("HistoryPanel/ScrollView").GetComponent<UISmartScroller>();
    }

    public override void OnGetKeyDown(EMenuButtonID eKeyID){
    	base.OnGetKeyDown(eKeyID);
    	if (eKeyID == EMenuButtonID.eScrUp) {
			Hist.SetSelectedByKey(-1);
    	} else if (eKeyID == EMenuButtonID.eScrDn) {
			Hist.SetSelectedByKey(1);
		}
    }
}
