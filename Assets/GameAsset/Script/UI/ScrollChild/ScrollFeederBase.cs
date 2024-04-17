using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollFeederBase : MonoBehaviour
{
	private UISmartScroller scr;
	private int lastSize;
	
    // Start is called before the first frame update
    virtual protected void Start()
    {
        scr = transform.GetComponent<UISmartScroller>();
        lastSize = -1;
    }

    // Update is called once per frame
    void Update()
    {
    	int nowSize = FeedContentSize();
        if (nowSize >= 0 && nowSize != lastSize){
        	scr.SetContentSize(nowSize);
        	lastSize = nowSize;
        }
    }
    
    virtual protected int FeedContentSize() { return 0; }
}
