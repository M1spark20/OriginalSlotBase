using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UISmartScroller : MonoBehaviour
{
	[SerializeField] private GameObject ContentPrehab;
	
	private GameObject[] ShowData;		// オブジェクトの表示を行うデータ
	private int[] ShowContentID;	// オブジェクトが表示するデータのID
	
	private Transform ContentList;
	private RectTransform ContentTransform;
	private RectTransform MyTransform;
	private ScrollRect Rect;
	private float ContentSize;
	private int ShowContentNum;
	
	public int ContentCount { get; private set; }
	
	private const int overDraw = 2;
	
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
        SetContentSize(1000);
        
        ShowData = null;
        if (ContentPrehab == null) return;
        
        ShowData = new GameObject[ShowContentNum];
        ShowContentID = new int[ShowContentNum];
        for (int i=0; i<ShowData.Length; ++i){
        	ShowData[i] = Instantiate(ContentPrehab, ContentList.transform);
        	ShowData[i].name = i.ToString();
        	ShowContentID[i] = i-overDraw;
        	// 初期位置は[0]を一番上に置く(ただし一番上は枠外)
        	ShowData[i].transform.localPosition = new Vector3(0, -ContentSize * ShowContentID[i], 0);
    		ShowData[i].SetActive(ShowContentID[i] >= 0 && ShowContentID[i] < ContentCount);
        }
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
        Debug.Log(nmPos.ToString("F6"));
        
        float showBegin = nmPos * moveSize;
        int beginItem = (int)(showBegin / ContentSize);
        int ctrlBegin = beginItem % ShowContentNum;
        
        // コンテンツの位置を調整する
        for (int i=0; i<ShowData.Length; ++i){
        	int ctrl = (ctrlBegin + i) % ShowContentNum;	// 制御データ
        	int currentID = beginItem + i - overDraw;
        	if (ShowContentID[ctrl] != currentID){
        		// データの更新を行う
        		ShowContentID[ctrl] = currentID;
        		ShowData[ctrl].SetActive(currentID >= 0 && currentID < ContentCount);
        	}
        	ShowData[ctrl].transform.localPosition = new Vector3(0, -(ContentSize * ShowContentID[ctrl]), 0);
        }
    }
    
    void CheckViewSize(){
        float ViewSize = MyTransform.sizeDelta.y;	
    	ShowContentNum = (int)Mathf.Ceil(ViewSize / ContentSize) + 2*overDraw;	// はみだし量も1としてカウント
    }
    
    public int GetContentID(int pID){ return ShowContentID[pID]; }
    
    public void SetContentSize(int size) {
    	// 最大スクロール量を調整する
    	ContentCount = size;
    	var contSize = ContentTransform.sizeDelta;
    	contSize.y = ContentSize * ContentCount;
    	ContentTransform.sizeDelta = contSize;
    }
}
