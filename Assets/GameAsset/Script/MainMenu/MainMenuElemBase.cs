using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MainMenuElemBase : MonoBehaviour
{
	virtual protected void Awake() {
	}
	
    virtual protected void Start()
    {
    	// 特段データを更新するだけでいい場合はオーバーライドしない
    	RefreshData();
    }
    
    abstract public void RefreshData();
    
    abstract public void OnGetKeyDown(EMenuButtonID eKeyID);
}
