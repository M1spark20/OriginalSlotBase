using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// アタッチしたGameObjectの位置をコマ(0,0)の場所に指定すること
public class CReelDrawerForMainReel : MonoBehaviour
{
	// 定数
	const int DIV_X = 2;
	const int DIV_Y = 5;
	const float POS_WBASE = 3.46f;
	const int START_INDEX = -2;
	const int DRAW_NUM    =  7;
	
	MultiImageBuilder	mImageBuilder;	// Sprite生成用クラス(各コマを格納)
	GameObject[][]		mComaInstance;	// このクラスでInstantiateしたGameObject 実装元:Prehab_MainComa
	
	// Start is called before the first frame update
	void Start()
	{
		// リール数を取得してImageBuilderとTextureのインスタンスを生成する
		const int reelNum = SlotMaker2022.LocalDataSet.REEL_MAX;
		mImageBuilder = new MultiImageBuilder();
		mComaInstance = new GameObject[reelNum][];
		Texture2D tex = Resources.Load<Texture2D>("coma330x150");
		mImageBuilder.BuildSprite(tex, "reelMain", DIV_X, DIV_Y, false);
		
		// GameObjectの生成元となるPrehabと親objectを定義する
		GameObject prehab = Resources.Load<GameObject>("Prehab_MainComa");
		Transform parent = this.transform;
		
		// リール配列を取得、各リールのSpriteを変更しながらPrehabをInstantiateする
		var mainROM = SlotMaker2022.MainROMDataManagerSingleton.GetInstance();
		const int comaNum = SlotMaker2022.LocalDataSet.COMA_MAX;
		for(int reelC=0; reelC<reelNum; ++reelC){
			mComaInstance[reelC] = new GameObject[comaNum];
			string test = "";
			for(int posC=0; posC<comaNum; ++posC){
				// データは逆順に格納されていることに注意する。横方向のみ予め移動させておく
				int comaIndex = mainROM.ReelArray[reelC][posC].Coma;
				test += comaIndex.ToString();
				Vector3 initPos = new Vector3(POS_WBASE * reelC, 0.0f, 0.0f);
				mComaInstance[reelC][comaNum - posC - 1] = Instantiate(prehab, parent);
				mComaInstance[reelC][comaNum - posC - 1].transform.localPosition = initPos;
				// SpriteRendererを呼び出してSpriteを変更する
				SpriteRenderer sp = mComaInstance[reelC][comaNum - posC - 1].GetComponent<SpriteRenderer>();
				sp.sprite = mImageBuilder.Extract(comaIndex);
			}
			Debug.Log(test);
		}
	}

	// Update is called once per frame
	void Update()
	{
		// 使用する変数の事前計算
		(float spW, float spH) = mImageBuilder.GetSpriteSize();
		const int comaNum = SlotMaker2022.LocalDataSet.COMA_MAX;
		
		// 各リールに対して処理を行う
		for(int reelC=0; reelC<mComaInstance.Length; ++reelC){
			// 処理前に一度全GameObjectを無効化する
			for(int posC=0; posC<mComaInstance[reelC].Length; ++posC){
				mComaInstance[reelC][posC].SetActive(false);
			}
			
			// リールの座標を取得し、描画の基準となるコマをCeilingで取得する。1未満の数値をfloorで取得する
			float reelPos = SlotDataSingleton.GetInstance().reelPos[reelC];
			int baseComaID = (int)Math.Ceiling(reelPos);
			float margin = reelPos - (float)Math.Floor(reelPos);
			float posYOffset = spH * margin;	// Y座標のミクロ増分値
			
			// 描画すべきオブジェクトを選んでY座標を指定、Activeにする
			for(int posC=START_INDEX; posC<START_INDEX + DRAW_NUM; ++posC){
				// 設定するオブジェクトを選択する
				int posCtrl = baseComaID + posC;
				while (posCtrl >= comaNum) posCtrl -= comaNum;
				while (posCtrl < 0) posCtrl += comaNum;
				// GameObjectをActiveにする
				mComaInstance[reelC][posCtrl].SetActive(true);
				// Y座標を入力する
				float posY = spH * posC - posYOffset;
				Vector3 pos = mComaInstance[reelC][posCtrl].transform.localPosition;
				pos.y = posY;
				mComaInstance[reelC][posCtrl].transform.localPosition = pos;
			}
		}
	}
	
	// オブジェクト終了時の処理
	void OnDestroy()
	{
		mImageBuilder.DestroySprite();
		Debug.Log("Destroy");
	}
}
