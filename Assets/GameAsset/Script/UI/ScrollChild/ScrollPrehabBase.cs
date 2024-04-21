using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollPrehabBase : MonoBehaviour
{
	private UISmartScroller scr;
	private int lastID;
	private bool lastSelected;
	private bool initFlag;
	private bool forceUpdate;

    // Start is called before the first frame update
    virtual protected void Start()
    {
		scr = transform.parent.transform.parent.transform.parent.GetComponent<UISmartScroller>();
		lastID = int.MinValue;
		lastSelected = false;
    }

    // Update is called once per frame
    void Update()
    {
    	int myID = int.Parse(name);
		int nowID = scr.GetContentID(myID);
		bool isSelected = scr.GetIsSelected(myID);
		if (NeedRefresh(nowID, isSelected) || scr.GetNeedUpdate(myID)){
			RefreshData(nowID, isSelected);
			lastID = nowID;
			lastSelected = isSelected;
			if (initFlag) { ShowUI(true); initFlag = false; }
		}
    }
    
    void OnEnable(){
    	ShowUI(false);
    	initFlag = true;
	}
    
    virtual protected bool NeedRefresh(int pID, bool pIsSelected){
    	return pID != lastID || lastSelected ^ pIsSelected;
    }
    
    virtual protected void RefreshData(int pID, bool pIsSelected){ }
    
    virtual protected void ShowUI(bool pVisible){ }
    
    protected void SelectMe() { scr.SetSelected(int.Parse(name)); }
}
