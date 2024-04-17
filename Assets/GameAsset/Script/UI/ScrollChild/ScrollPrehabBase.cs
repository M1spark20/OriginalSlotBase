using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollPrehabBase : MonoBehaviour
{
	private UISmartScroller scr;
	int lastID;

    // Start is called before the first frame update
    void Start()
    {
		scr = transform.parent.transform.parent.transform.parent.GetComponent<UISmartScroller>();
		lastID = int.MinValue;
    }

    // Update is called once per frame
    void Update()
    {
		int nowID = scr.GetContentID(int.Parse(name));
		if (nowID != lastID){
			RefreshData(nowID);
			lastID = nowID;
		}
    }
    
    virtual protected void RefreshData(int pID){ }
}
