using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
	[SerializeField] private GameObject[] Panels;
	
	private int SelectedID;
	
	// 各パネルからキー入力情報を取得するための記録機能
	private bool[] GetKeyDown;
	
    // Start is called before the first frame update
    void Start()
    {
        SelectedID = -1;
        GetKeyDown = new bool[(int)EMenuButtonID.eButtonMax];
        for(int i=0; i<GetKeyDown.Length; ++i) GetKeyDown[i] = false;
        
        for(int i=0; i<Panels.Length; ++i) Panels[i].SetActive(false);
        RefreshActivate(0);
    }

    // Update is called once per frame
    void Update()
    {
        if (Panels.Length == 0) return;
        // タブの描画(後から追加)
    }
    
    public void OnGetKeyDown(EMenuButtonID eKeyID){
    	if (eKeyID == EMenuButtonID.eScrLeft) {
    		RefreshActivate( (SelectedID + Panels.Length - 1) % Panels.Length );
    	} else if (eKeyID == EMenuButtonID.eScrRight) {
    		RefreshActivate( (SelectedID + 1) % Panels.Length );
    	} else {
    		// 各コンポーネントへのアクセスは左右キーをマスクする
    		GetKeyDown[(int)eKeyID] = true;
    	}
    }
    public bool GetKeyDownStatus(EMenuButtonID eKeyID){
    	if (eKeyID == EMenuButtonID.eButtonMax) return false;
    	bool ans = GetKeyDown[(int)eKeyID];
    	GetKeyDown[(int)eKeyID] = false;
    	return ans;
    }
    
    // アクティブにするパネルを決める
    private void RefreshActivate(int activeID){
    	if (activeID != SelectedID){
    		if (SelectedID >= 0) Panels[SelectedID].SetActive(false);
    		if (activeID >= 0) Panels[activeID].SetActive(true);
    		SelectedID = activeID;
    	} 
    }
}
