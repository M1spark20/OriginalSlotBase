using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
	[SerializeField] private GameObject[] Panels;
	[SerializeField] private Button SelectorBase;
	
	private int SelectedID;
	private Button[] Selector;
	private Image[] SelImage;
	private TextMeshProUGUI[] SelName;
	
	// 各パネルからキー入力情報を取得するための記録機能
	private MainMenuElemBase[] PanelScr;
	private CanvasGroup[] PanelsCanvasGroup;
	private GraphicRaycaster[] PanelsTouch;
	
    // Start is called before the first frame update
    void Start()
    {
        SelectedID = -1;
        PanelScr = new MainMenuElemBase[Panels.Length];
        PanelsCanvasGroup = new CanvasGroup[Panels.Length];
        PanelsTouch = new GraphicRaycaster[Panels.Length];
        
        Selector = new Button[Panels.Length];
        SelImage = new Image[Panels.Length];
        SelName = new TextMeshProUGUI[Panels.Length];
        Selector[0] = SelectorBase;
        float sizeX = SelectorBase.GetComponent<RectTransform>().sizeDelta.x;
        
        for(int i=0; i<Panels.Length; ++i) {
	        // 選択用ボタン配置
        	PanelsCanvasGroup[i] = Panels[i].GetComponent<CanvasGroup>();
        	PanelsTouch[i] = Panels[i].GetComponent<GraphicRaycaster>();
        	Panels[i].GetComponent<Canvas>().enabled = true;
        	PanelsCanvasGroup[i].alpha = 0f;
        	PanelsTouch[i].enabled = false;
        	if (i > 0) {
        		Selector[i] = Instantiate(SelectorBase, this.transform);
        		Selector[i].transform.localPosition += new Vector3(sizeX * i, 0, 0);
        	}
        	SelImage[i] = Selector[i].GetComponent<Image>();
        	SelName[i] = Selector[i].transform.Find("name").GetComponent<TextMeshProUGUI>();
        	var prm = i;	// 変数に入れないとデルタがうまく動かないらしい…
        	Selector[i].onClick.AddListener(() => OnClickButton(prm));
        	// スクリプト登録
        	PanelScr[i] = Panels[i].GetComponent<MainMenuElemBase>();
        	SelName[i].text = PanelScr[i].GetElemName();
        }
        // 表示初期化(Menuが表示されていない前提)
        RefreshActivate(0);
        PanelsTouch[0].enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        // タブの描画(後から追加)
        for (int i=0; i<Selector.Length; ++i) {
        	SelImage[i].color = SelectedID == i ? Color.yellow : new Color(0.88f, 0.88f, 0.88f);
        	SelName[i].text = PanelScr[i].GetElemName();
        	SelName[i].color = SelectedID == i ? Color.yellow : Color.white;
        }
    }
    
    public void OnGetKeyDown(EMenuButtonID eKeyID){
    	if (eKeyID == EMenuButtonID.eScrLeft) {
    		RefreshActivate( (SelectedID + Panels.Length - 1) % Panels.Length );
    	} else if (eKeyID == EMenuButtonID.eScrRight) {
    		RefreshActivate( (SelectedID + 1) % Panels.Length );
    	} else {
    		// 各コンポーネントへアクセスする
    		PanelScr[SelectedID].OnGetKeyDown(eKeyID);
    	}
    }

    // アクティブにするパネルを決める
    private void RefreshActivate(int activeID){
    	if (activeID != SelectedID){
    		if (SelectedID >= 0){
    			PanelsCanvasGroup[SelectedID].alpha = 0f;
    			PanelsTouch[SelectedID].enabled = false;
    		}
    		if (activeID >= 0){
    			PanelScr[activeID].RefreshData();
    			PanelsCanvasGroup[activeID].alpha = 1f;
    			PanelsTouch[activeID].enabled = true;
    		}
    		SelectedID = activeID;
    	} 
    }
    
    // メニュー再表示時の描画をかける
    public void OnMenuShownChange(bool visible){
    	if (SelectedID < 0) return;
    	PanelsTouch[SelectedID].enabled = visible;
    	if(visible) PanelScr[SelectedID].RefreshData();
    }
    
    public void OnClickButton(int index){
    	// ボタンを押したデータへ遷移する。
    	RefreshActivate(index);
    }
}
