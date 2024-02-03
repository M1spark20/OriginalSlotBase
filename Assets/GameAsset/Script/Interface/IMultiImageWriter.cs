using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IMultiImageWriter : MonoBehaviour
{
	protected MultiImageBuilder mImageBuilder;	// Sprite生成用クラス(各コマを格納)
	protected GameObject[]		mComaInstance;	// このクラスでInstantiateしたGameObject 実装元:Prehab_Com
	
	// 継承先で定義すべきフィールド
	protected int				DivX;
	protected int				DivY;
	protected bool				CutWayX;
	protected int				ShowX;
	protected int				ShowY;
	protected float				OverlapX;
	protected float				OverlapY;
	
	// inspectorから取得
	[SerializeField] protected GameObject PrehabParent;	
	[SerializeField] protected Texture2D TexResource;
	[SerializeField] protected string InstanceName;
	
	// 定義すべき関数: テクスチャの使用番号を取得する
	abstract protected void InitDivision();								// DivX, DivY, CutWayXを初期化する
	
    // Start is called before the first frame update
	// GameObjectの生成とテクスチャの切り出しを行う
    protected virtual void Start()
    {
    	InitDivision();
    	
    	int ShowDigit = Mathf.Abs(ShowX * ShowY);
    	
		mImageBuilder = new MultiImageBuilder();
		mComaInstance = new GameObject[ShowDigit];
		mImageBuilder.BuildSprite(TexResource, InstanceName, DivX, DivY, CutWayX);
		
		// GameObjectの生成元となるPrehabと親objectを定義する
		Transform parent = this.transform;
		// GameObjectを生成し、場所を配置する(右揃えのみ)
		(float spW, float spH) = mImageBuilder.GetSpriteSize();
		for (uint i=0; i<ShowDigit; ++i){
			mComaInstance[i] = Instantiate(PrehabParent, parent);
			Vector3 initPos = new Vector3(Mathf.Sign(ShowX) * (spW - OverlapX) * (float)(i % Mathf.Abs(ShowX)), Mathf.Sign(ShowY) * (spH - OverlapY) * (float)(i / Mathf.Abs(ShowX)), 0.0f);
			mComaInstance[i].transform.localPosition = initPos;
		}
    }
    
    // Update is called once per frame(個別定義)
    protected abstract void Update();
    
	protected void OnDestroy()
	{
		// Textureの破棄
		mImageBuilder.DestroySprite();
	}
}
