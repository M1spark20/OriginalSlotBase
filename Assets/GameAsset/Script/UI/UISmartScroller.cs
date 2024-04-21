using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UISmartScroller : MonoBehaviour
{
	[SerializeField] private GameObject ContentPrehab;
	
	private GameObject[] ShowData;		// オブジェクトの表示を行うデータ
	private int[] ShowContentID;		// オブジェクトが表示するデータのID
	private bool[] NeedUpdate;			// オブジェクトの表示更新が必要かどうか
	
	private Transform ContentList;
	private RectTransform ContentTransform;
	private RectTransform MyTransform;
	private ScrollRect Rect;
	private float ContentSize;
	private int ShowContentNum;
	private bool ForceUpdateFlag;
	private bool ReadyUpdate;
	
	public int ContentCount { get; private set; }
	public int ShowOffset { get; private set; }
	public int SelectedIndex { get; private set; }
	
	private const int overDraw = 2;
	
	void Awake(){
		// Start完了前まで描画を行わない設定をする
		ReadyUpdate = false;
	}
	
    // Start is called before the first frame update
    void Start()
    {
        ContentSize = ContentPrehab?.GetComponent<RectTransform>().sizeDelta.y ?? 1f;
        ContentList = transform.Find("Viewport/Content");
        ContentTransform = ContentList.GetComponent<RectTransform>();
        MyTransform = GetComponent<RectTransform>();
        Rect = GetComponent<ScrollRect>();
        ContentCount = 0;
        CheckViewSize();
        ForceUpdateFlag = false;
        SelectedIndex = -1;
        
        ShowData = null;
        if (ContentPrehab == null) return;
        
        ShowData = new GameObject[ShowContentNum];
        ShowContentID = new int[ShowContentNum];
        NeedUpdate = new bool[ShowContentNum];
        for (int i=0; i<ShowData.Length; ++i){
        	ShowData[i] = Instantiate(ContentPrehab, ContentList.transform);
        	ShowData[i].name = i.ToString();
        	ShowContentID[i] = int.MinValue;
        	// 初期位置は[0]を一番上に置く(ただし一番上は枠外)
        	ShowData[i].transform.localPosition = new Vector3(0, -ContentSize * ShowContentID[i], 0);
        	NeedUpdate[i] = true;
    		ShowData[i].SetActive(i >= 0 && i < ContentCount);
        }
        
        // 描画開始フラグを立てる
        ReadyUpdate = true;
    }

    // Update is called once per frame
    void Update()
    {
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
        	int currentID = beginItem + i - overDraw;
        	
        	// データが非表示の場合は要更新フラグをリセットしない
        	NeedUpdate[ctrl] &= (currentID >= 0 && currentID < ContentCount);
        	
        	if (ShowContentID[ctrl] != currentID || ForceUpdateFlag){
        		// データの更新を行う
        		ShowContentID[ctrl] = currentID + ShowOffset;
        		ShowData[ctrl].SetActive(currentID >= 0 && currentID < ContentCount);
        		NeedUpdate[ctrl] = true;
        	}
        	// 初期位置は[0]を一番上に置く(ただし一番上は枠外)
        	ShowData[ctrl].transform.localPosition = new Vector3(0, -(ContentSize * currentID), 0);
        }
        
        // 強制更新フラグリセット
        ForceUpdateFlag = false;
    }
    
    void OnEnable() {
    	if (ReadyUpdate) Update();
    }
    
    void CheckViewSize(){
        float ViewSize = MyTransform.sizeDelta.y;	
    	ShowContentNum = (int)Mathf.Ceil(ViewSize / ContentSize) + 2*overDraw;	// はみだし量も1としてカウント
    }
    
    public int GetContentID(int pID){ return ShowContentID[pID]; }
    public bool GetIsSelected(int pID){ return ShowContentID[pID] == SelectedIndex; }
    public void SetSelected(int pID){ SelectedIndex = ShowContentID[pID]; }
    public bool GetNeedUpdate(int pID){ return NeedUpdate[pID]; }
    public void SetSelectedByKey(int diff){
    	SelectedIndex += diff;
    	// 範囲を表示範囲にトリム、offsetが表示されないことに注意する
    	if (SelectedIndex < ShowOffset) SelectedIndex = ShowOffset;
    	if (SelectedIndex >= ContentCount) SelectedIndex = ContentCount - 1;
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
    	// 最大スクロール量を調整する
    	bool sizeChanged = size != ContentCount;
    	int lastOffset = ShowOffset;
    	ContentCount = size;
    	ShowOffset = offset;
    	var contSize = ContentTransform.sizeDelta;
    	contSize.y = ContentSize * ContentCount;
    	ContentTransform.sizeDelta = contSize;
    	
    	// 全データを強制的に更新する
    	ForceUpdateFlag = true;
    	// 表示データ総数が変わる場合、選択データをリセットする。変わらない場合はoffset増分だけ増やす
    	if (sizeChanged) SelectedIndex = -1;
    	else SelectedIndex += (offset - lastOffset);
    }
}
