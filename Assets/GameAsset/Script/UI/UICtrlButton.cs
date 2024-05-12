using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICtrlButton : MonoBehaviour
{
	[SerializeField] private EGameButtonID   Function;
	[SerializeField] private SlotDataManager DataManager;
	[SerializeField] private bool Slidable;
	
	bool buttonDownFlag;
	
    private void Awake()
    {
        buttonDownFlag = false;
    }

    // Update is called once per frame
    private void Update()
    {
    	if (Slidable) buttonDownFlag &= Input.GetMouseButton(0);
        if (buttonDownFlag) DataManager?.OnScreenHover(Function);
    }
    
    // ボタンのDown/Upで押下状態を見る。Down状態のドラッグはPointerExitで見る
    public void OnButtonUp(){
    	buttonDownFlag = false;
    }
    public void OnButtonDown(){
		if (!buttonDownFlag) DataManager?.OnScreenTouch(Function);	// 初回のみ
    	buttonDownFlag = true;
    }
    public void OnPointerExit(){
    	buttonDownFlag = false;
    }
    
    // スライドストップ機能: タップorクリック中にEnterされた場合にbuttonDownをtrueにする。Slidable=trueでのみ機能
    public void OnPointerEnter(){
    	if (!Slidable) return;
    	if (!Input.GetMouseButton(0)) return;
		if (buttonDownFlag) return;
		DataManager?.OnScreenTouch(Function);	// 初回のみ
    	buttonDownFlag = true;
    }
}
