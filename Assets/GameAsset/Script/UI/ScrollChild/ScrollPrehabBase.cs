using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollPrehabBase : MonoBehaviour
{
	private UISmartScroller scr;
	public bool Selected { get; private set; }

    // Start is called before the first frame update
    virtual protected void Awake(){
		scr = transform.parent.transform.parent.transform.parent.GetComponent<UISmartScroller>();
		Selected = false;
	}
	
    virtual public void RefreshData(int pID, bool pIsSelected){ Selected = pIsSelected; }
    
    protected void SelectMe() { scr.SetSelected(int.Parse(name)); }
}
