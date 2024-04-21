using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuKeyCtrlBase : MonoBehaviour
{
	private MainMenuManager scr;
	
	virtual protected void Start(){
		scr = transform.parent.GetComponent<MainMenuManager>();
		Debug.Log(transform.parent.name);
	}
	
	void Update(){
		for(int i=0; i<(int)EMenuButtonID.eButtonMax; ++i){
			var nowID = (EMenuButtonID)i;
			if (scr.GetKeyDownStatus(nowID)) OnGetKeyDown(nowID);
		}
	}
	
    public virtual void OnGetKeyDown(EMenuButtonID eKeyID){ }
}
