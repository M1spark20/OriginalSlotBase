using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UISmartScroller : MonoBehaviour
{
	[SerializeField] private GameObject ContentPrehab;
	
	private GameObject[] ShowData;		// オブジェクトの表示を行うデータ
	private ScrollPrehabBase[] ShowScr;	// オブジェクトのスクリプト
	private int[] ShowContentID;		// オブジェクトが表示するデータのID
	
	private Transform ContentList;
	private RectTransform ContentTransform;
	private RectTransform MyTransform;
	private ScrollRect Rect;
	private float ContentSize;
	private int ShowContentNum;
	
	public int ContentCount { get; private set; }
	public int ShowOffset { get; private set; }
	public int SelectedIndex { get; private set; }
	
	private const int overDraw = 2;
	
    // Start is called before the first frame update
    void Awake()
    {
        ContentSize = ContentPrehab?.GetComponent<RectTransform>().sizeDelta.y ?? 1f;
        ContentList = transform.Find("Viewport/Content");
        ContentTransform = ContentList.GetComponent<RectTransform>();
        MyTransform = GetComponent<RectTransform>();
        Rect = GetComponent<ScrollRect>();
        ContentCount = 0;
        CheckViewSize();
        SelectedIndex = -1;
        
        ShowData = null;
        if (ContentPrehab == null) return;
        
        ShowData = new GameObject[ShowContentNum];
        ShowScr = new ScrollPrehabBase[ShowContentNum];
        ShowContentID = new int[ShowContentNum];
        for (int i=0; i<ShowData.Length; ++i){
        	ShowData[i] = Instantiate(ContentPrehab, ContentList.transform);
        	ShowData[i].name = i.ToString();
        	ShowScr[i] = ShowData[i].GetComponent<ScrollPrehabBase>();
        	ShowContentID[i] = int.MinValue;
        	// 初期位置は[0]を一番上に置く(ただし一番上は枠外)
        	ShowData[i].transform.localPosition = new Vector3(0, -ContentSize * ShowContentID[i], 0);
    		ShowData[i].SetActive(i >= 0 && i < ContentCount);
        }
    }

    // Update is called once per frame
    void Update()
    {
    	ElemUpdate(false);
    }
    
    void CheckViewSize(){
        float ViewSize = MyTransform.sizeDelta.y;
    	ShowContentNum = (int)Mathf.Ceil(ViewSize / ContentSize) + 2*overDraw;	// はみだし量も1としてカウント
    }
    
    public int GetContentID(int pID){ return ShowContentID[pID]; }
    public bool GetIsSelected(int pID){ return ShowContentID[pID] == SelectedIndex; }
    public void SetSelected(int pID){ SelectedIndex = ShowContentID[pID]; }
    
    public void SetSelectedByKey(int diff){
    	SelectedIndex += diff;
    	// 範囲を表示範囲にトリム、offsetが表示されないことに注意する
    	if (SelectedIndex < ShowOffset) SelectedIndex = ShowOffset;
    	if (SelectedIndex >= ContentCount + ShowOffset) SelectedIndex = ContentCount + ShowOffset - 1;
    	Debug.Log(SelectedIndex.ToString());
    	
    	// 選択データを中央に持ってくる
    	float MarkPos = (MyTransform.sizeDelta.y - ContentSize) / 2f;
    	float ScrPos = SelectedIndex * ContentSize - MarkPos;
    	float moveSize = ContentTransform.sizeDelta.y - MyTransform.sizeDelta.y;
    	if (moveSize <= 0f) Rect.verticalNormalizedPosition = 1f;
    	else {
    		float nmPos = ScrPos / moveSize;
	        // スクロール位置をトリムする
	        if (nmPos < 0f) nmPos = 0f;
	        if (nmPos > 1f) nmPos = 1f;
	        // Verticalは反転させる
	        Rect.verticalNormalizedPosition = 1f - nmPos;
    	}
    }
    
    public void SetContentSize(int size, int offset) {
    	// 最大スクロール量を調整する、sizeはoffset(0からいくつ分のデータを表示しない)を抜いた値を入れる
    	int lastSize = ContentCount;
    	int lastOffset = ShowOffset;
    	ContentCount = size;
    	ShowOffset = offset;
    	// データサイズ更新
    	var contSize = ContentTransform.sizeDelta;
    	contSize.y = ContentSize * (ContentCount);
    	ContentTransform.sizeDelta = contSize;
    	
    	// 選択データを更新する
    	SelectedIndex += (size - lastSize) + (offset - lastOffset);
    	if (size == 0) SelectedIndex = -1;
    }
    
    // データ更新・スクロール処理
    public void ElemUpdate(bool pForceUpdate){
        float nmPos = 1f - Rect.verticalNormalizedPosition;	// 一番上を0にする
        float moveSize = ContentTransform.sizeDelta.y - MyTransform.sizeDelta.y;	// スクロールする幅はContentの高さ-Viewの高さ
        // 引っ張り対策で値がオーバーする場合はトリムする
        if (nmPos < 0f) nmPos = 0f;
        if (nmPos > 1f) nmPos = 1f;
        // スクロール量が存在しない場合はトリムする
        if (moveSize < 0f) nmPos = 0f;
        // Debug.Log(nmPos.ToString("F6"));
        
        float showBegin = nmPos * moveSize;
        int beginItem = (int)(showBegin / ContentSize);
        int ctrlBegin = beginItem % ShowContentNum;
        
        // コンテンツの位置を調整する
        for (int i=0; i<ShowData.Length; ++i){
        	int ctrl = (ctrlBegin + i) % ShowContentNum;	// 制御データ
        	int currentID = beginItem + i - overDraw + ShowOffset;
       	
        	if (ShowContentID[ctrl] != currentID || ShowScr[ctrl].Selected ^ (currentID == SelectedIndex) || pForceUpdate){
        		// データの更新を行う
        		ShowContentID[ctrl] = currentID;
        		ShowData[ctrl].SetActive(currentID >= 0 && currentID < ContentCount + ShowOffset);
        		ShowScr[ctrl].RefreshData(currentID, currentID == SelectedIndex);
        	}
        	// 初期位置は[0]を一番上に置く(ただし一番上は枠外)
        	ShowData[ctrl].transform.localPosition = new Vector3(0, -(ContentSize * (currentID - ShowOffset)), 0);
        }
    }
}
