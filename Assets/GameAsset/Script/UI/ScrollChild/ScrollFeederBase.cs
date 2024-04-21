using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollFeederBase : MonoBehaviour
{
	private UISmartScroller scr;
	private int lastSize;
	private int lastOffset;
	private bool ReadyUpdate;
	
	void Awake(){
		// Start完了前まで描画を行わない設定をする
		ReadyUpdate = false;
	}

	
    // Start is called before the first frame update
    virtual protected void Start()
    {
        scr = transform.GetComponent<UISmartScroller>();
        lastSize = -1;
        lastOffset = 0;
        ReadyUpdate = true;
    }

    // Update is called once per frame
    private void Update()
    {
    	int nowSize = FeedContentSize();
    	int offset = FeedOffsetSize();
        if ((nowSize - offset >= 0 && nowSize != lastSize) || offset != lastOffset){
        	scr.SetContentSize(nowSize - offset, offset);
        	lastSize = nowSize;
        	lastOffset = offset;
        }
    }
    
    private void OnEnable(){
    	if (ReadyUpdate) Update();
    }
    
    virtual protected int FeedContentSize() { return 0; }
    virtual protected int FeedOffsetSize() { return 0; }
}
