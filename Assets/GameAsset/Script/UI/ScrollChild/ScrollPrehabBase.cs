using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollPrehabBase : MonoBehaviour
{
	private UISmartScroller scr;
	private int lastID;

    // Start is called before the first frame update
    virtual protected void Start()
    {
		scr = transform.parent.transform.parent.transform.parent.GetComponent<UISmartScroller>();
		lastID = int.MinValue;
    }

    // Update is called once per frame
    void Update()
    {
		int nowID = scr.GetContentID(int.Parse(name));
		if (NeedRefresh(nowID)){
			RefreshData(nowID);
			lastID = nowID;
		}
    }
    
    virtual protected bool NeedRefresh(int pID){
    	return pID != lastID;
    }
    
    virtual protected void RefreshData(int pID){ }
}
