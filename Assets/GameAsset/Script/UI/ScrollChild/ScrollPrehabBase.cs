using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollPrehabBase : MonoBehaviour
{
	private UISmartScroller scr;
	private Canvas canvas;
	private GraphicRaycaster touch;
	private bool touchable;
	public bool Selected { get; private set; }

    // Start is called before the first frame update
    virtual protected void Awake(){
		scr = transform.parent.transform.parent.transform.parent.GetComponent<UISmartScroller>();
		canvas = GetComponent<Canvas>();
		touch = GetComponent<GraphicRaycaster>();
		Selected = false;
		touchable = false;
	}
	
	virtual protected void Update(){
		if (touch != null) touch.enabled = canvas.enabled && touchable;
	}
	
    virtual public void RefreshData(int pID, bool pIsSelected){ Selected = pIsSelected; }
    
    protected void SelectMe() { scr.SetSelected(int.Parse(name)); }
    
    public void SetVisible(bool visible) { canvas.enabled = visible; }
    
    public void SetRaycaster(bool pTouchable) { touchable = pTouchable; }
}
