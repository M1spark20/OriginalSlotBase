using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MainMenuSettings : MainMenuElemBase
{
	[SerializeField] private ScrollRect Viewer;
	[SerializeField] private float moveSize;
	
	private float ViewerHeight;
	private float ScrollHeight;
	
    // Start is called before the first frame update
    protected override void Awake()
    {
    	base.Awake();
    	ViewerHeight = Viewer.transform.Find("Viewport/Content").GetComponent<RectTransform>().sizeDelta.y;
    	ScrollHeight = ViewerHeight - GetComponent<RectTransform>().sizeDelta.y;
    }
    
    public override void RefreshData(){ }
    
    public override void OnGetKeyDown(EMenuButtonID eKeyID){
    	if (ScrollHeight < 0f) return;
    	float moveSizeStd = moveSize / ScrollHeight;
    	float pos = 1f - Viewer.verticalNormalizedPosition;
    	
    	if (eKeyID == EMenuButtonID.eScrUp) {
			pos -= moveSizeStd;
    	} else if (eKeyID == EMenuButtonID.eScrDn) {
			pos += moveSizeStd;
		}
		
		// 調整
		if (pos < 0f) pos = 0f;
		if (pos > 1f) pos = 1f;
		Viewer.verticalNormalizedPosition = 1f - pos;
    }
}
