using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MainMenuElemBase : MonoBehaviour
{
	private bool ready;
	
	virtual protected void Awake() {
    	ready = false;
	}
	
    virtual protected void Start()
    {
    	// 特段データを更新するだけでいい場合はオーバーライドしない
    	ready = true;
    	RefreshData();
    }
    
    virtual protected void OnEnable(){
    	// 特段データを更新するだけでいい場合はオーバーライドしない
    	if (ready) RefreshData();
    }
    
    abstract protected void RefreshData();
    
    abstract public void OnGetKeyDown(EMenuButtonID eKeyID);
}
