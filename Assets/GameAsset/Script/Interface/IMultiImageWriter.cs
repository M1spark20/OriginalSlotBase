using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IMultiImageWriter : MonoBehaviour
{
	protected MultiImageBuilder mImageBuilder;	// Sprite生成用クラス(各コマを格納)
	protected GameObject[]		mComaInstance;	// このクラスでInstantiateしたGameObject 実装元:Prehab_Com
	protected int				DivX;
	protected int				DivY;
	protected bool				CutWayX;
	
	// inspectorから取得
	[SerializeField] protected string MatResourceName;
	[SerializeField] protected string DispVariable;
	[SerializeField, Min(1)] protected uint ShowDigit;
	
	[SerializeField] protected string TexResourceName;
	[SerializeField] protected string InstanceName;
	
	[SerializeField] string TimerName;		// 時間判定を行うタイマ名。指定なしで判定しない
	[SerializeField] float  TimeBegin;		// 時間下限値
	[SerializeField] bool   TimeInvert;		// 条件を満たすときに表示するか(true: 表示しない)
	
	// 定義すべき関数: テクスチャの使用番号を取得する
	abstract protected void InitDivision();								// DivX, DivY, CutWayXを初期化する
	abstract protected int? GetTextureIndex(int val, uint getDigit);	// valに対しdigitのindexを決める。表示しない場合null
	
    // Start is called before the first frame update
	// GameObjectの生成とテクスチャの切り出しを行う
    void Start()
    {
    	InitDivision();
    	
		mImageBuilder = new MultiImageBuilder();
		mComaInstance = new GameObject[ShowDigit];
		Texture2D tex = Resources.Load<Texture2D>(TexResourceName);
		mImageBuilder.BuildSprite(tex, InstanceName, DivX, DivY, CutWayX);
		
		// GameObjectの生成元となるPrehabと親objectを定義する
		GameObject prehab = Resources.Load<GameObject>(MatResourceName);
		Transform parent = this.transform;
		// GameObjectを生成し、場所を配置する(右揃えのみ)
		(float spW, float spH) = mImageBuilder.GetSpriteSize();
		for (uint i=0; i<ShowDigit; ++i){
			mComaInstance[i] = Instantiate(prehab, parent);
			Vector3 initPos = new Vector3(-spW * (float)i, 0.0f, 0.0f);
			mComaInstance[i].transform.localPosition = initPos;
		}
    }
    
    // Update is called once per frame
    void Update()
    {
    	// 表示させる値を取得する
		var varData = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance().valManager;
		int? showVal = varData.GetVariable(DispVariable)?.val;
		if (!showVal.HasValue) return;
		
		for (uint i=0; i<ShowDigit; ++i){
			int? spID = GetTextureIndex((int)showVal, i);
			SpriteRenderer sp = mComaInstance[i].GetComponent<SpriteRenderer>();
			sp.enabled = spID.HasValue && CheckTimer();
			if (spID.HasValue) sp.sprite = mImageBuilder.Extract((int)spID);
		}
    }
    
	void OnDestroy()
	{
		// Textureの破棄
		mImageBuilder.DestroySprite();
	}
	
	bool CheckTimer(){
		// 時間点灯条件判定
        if (TimerName == string.Empty) return true;
        
		var slotData = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance();
        bool activated = true;
    	var elem = slotData.timerData.GetTimer(TimerName);
    	if (elem == null) activated = false;
    	else {
    		if (!elem.isActivate) activated = false;		// タイマが無効な場合無効判定
    		else activated &= elem.elapsedTime > TimeBegin;	// 指定時間を超過しているか
    	}
    	return activated ^ TimeInvert;
	}
}
