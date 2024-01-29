using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// アタッチしたGameObjectの位置をコマ(0,0)の場所に指定すること
public class CReelDrawerForMainReel : MonoBehaviour
{
	// 定数
	const int DIV_X = 1;
	const int DIV_Y = 10;
	const float POS_WBASE = 3.46f;
	const int START_INDEX = -2;
	const int DRAW_NUM    =  7;
	
	MultiImageBuilder	mImageBuilder;	// Sprite生成用クラス(各コマを格納)
	GameObject[][]		mComaInstance;	// このクラスでInstantiateしたGameObject 実装元:Prehab_MainComa
	GameObject[]		mCutLine;		// 切れ目用オブジェクト 実装元:Prehab_MainComa
	
	// ReelBlur用変数
	const int  BLUR_BASEFPS = 90;	// ブラー計算のベースとなるfps値
	float[]    mLastPosDelta;		// 各リールの前回位置からの差分[reelNum]
	Material[] mReelMat;			// 各リールのマテリアル[reelNum]
	Material[] mCutMat;				// 各リール切れ目のマテリアル[reelNum]
	
	// リール用ColorMap
	[SerializeField] private string[] MapShifterName;
	
	// Start is called before the first frame update
	void Start()
	{
		// リール数を取得してImageBuilderとTextureのインスタンスを生成する
		const int reelNum = SlotMaker2022.LocalDataSet.REEL_MAX;
		mImageBuilder = new MultiImageBuilder();
		mComaInstance = new GameObject[reelNum][];
		mCutLine      = new GameObject[reelNum];
		Texture2D tex = Resources.Load<Texture2D>("coma330x150");
		mImageBuilder.BuildSprite(tex, "reelMain", DIV_X, DIV_Y, false);
		
		// ReelBlur用変数初期化
		mReelMat      = new Material[reelNum];
		mCutMat       = new Material[reelNum];
		
		// GameObjectの生成元となるPrehabと親objectを定義する
		GameObject prehab    = Resources.Load<GameObject>("Prehab_MainComa");
		GameObject prehabCut = Resources.Load<GameObject>("Prehab_CutLine");
		Transform parent = this.transform;
		
		// リール配列を取得、各リールのSpriteを変更しながらPrehabをInstantiateする
		var mainROM = SlotMaker2022.MainROMDataManagerSingleton.GetInstance();
		const int comaNum = SlotMaker2022.LocalDataSet.COMA_MAX;
		for(int reelC=0; reelC<reelNum; ++reelC){
			mComaInstance[reelC] = new GameObject[comaNum];
			string test = "";
			
			// リールブラー用Material新規生成(Prehabからコピー)
			mReelMat[reelC] = new Material(prehab.GetComponent<SpriteRenderer>().sharedMaterial);

			// 切れ目関係のobjectを生成する
			mCutLine[reelC] = Instantiate(prehabCut, parent);
			mCutMat [reelC] = new Material(prehabCut.GetComponent<SpriteRenderer>().sharedMaterial);
			mCutMat [reelC].SetInt("_Weight", 48);
			mCutLine[reelC].GetComponent<SpriteRenderer>().sharedMaterial = mCutMat[reelC];
			
			// 各コマにSpriteとMaterialを割り当てる
			for(int posC=0; posC<comaNum; ++posC){
				// データは逆順に格納されていることに注意する。横方向のみ予め移動させておく
				int comaIndex = mainROM.ReelArray[reelC][posC].Coma;
				test += comaIndex.ToString();
				Vector3 initPos = new Vector3(POS_WBASE * reelC, 0.0f, 0.0f);
				mComaInstance[reelC][comaNum - posC - 1] = Instantiate(prehab, parent);
				mComaInstance[reelC][comaNum - posC - 1].transform.localPosition = initPos;
				
				// SpriteRendererを呼び出してSpriteを変更し、sharedMaterialを割り当てる
				SpriteRenderer sp = mComaInstance[reelC][comaNum - posC - 1].GetComponent<SpriteRenderer>();
				sp.sprite = mImageBuilder.Extract(comaIndex);
				sp.sharedMaterial = mReelMat[reelC];
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
		const int showNum = SlotMaker2022.LocalDataSet.SHOW_MAX;
		int[][] matColor = GetColor();
		
		// 各リールに対して処理を行う
		for(int reelC=0; reelC<mComaInstance.Length; ++reelC){
			// 処理前に一度全GameObjectを無効化する
			mCutLine[reelC].GetComponent<SpriteRenderer>().enabled = false;
			for(int posC=0; posC<mComaInstance[reelC].Length; ++posC){
				mComaInstance[reelC][posC].GetComponent<SpriteRenderer>().enabled = false;
			}
			
			// リールの座標を取得し、描画の基準となるコマをCeilingで取得する。1未満の数値をfloorで取得する
			var reelData = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance().reelData[reelC];
			float reelPos = reelData.reelPos;
			byte baseComaID = reelData.GetReelComaID();
			float margin = (float)baseComaID - reelPos;
			float posYOffset = spH * margin;	// Y座標のミクロ未達値
			
			// 描画すべきオブジェクトを選んでY座標を指定、Activeにする
			for(int posC=START_INDEX; posC<START_INDEX + DRAW_NUM; ++posC){
				// 設定するオブジェクトを選択する
				int posCtrl = baseComaID + posC;
				while (posCtrl >= comaNum) posCtrl -= comaNum;
				while (posCtrl < 0) posCtrl += comaNum;
				// GameObjectをActiveにしてカラーを設定する
				mComaInstance[reelC][posCtrl].GetComponent<SpriteRenderer>().enabled = true;
				int colorH = posC;
				if (posC < 0) colorH = 0;
				if (posC >= showNum) colorH = showNum - 1;
				int setColor = matColor[reelC][colorH];
				mComaInstance[reelC][posCtrl].GetComponent<SpriteRenderer>().color = new Color32(
					SlotEffectMaker2023.Data.ColorMapList.GetColorElem(setColor, SlotEffectMaker2023.Data.ColorMapElem.Red),
					SlotEffectMaker2023.Data.ColorMapList.GetColorElem(setColor, SlotEffectMaker2023.Data.ColorMapElem.Green),
					SlotEffectMaker2023.Data.ColorMapList.GetColorElem(setColor, SlotEffectMaker2023.Data.ColorMapElem.Blue),
					SlotEffectMaker2023.Data.ColorMapList.GetColorElem(setColor, SlotEffectMaker2023.Data.ColorMapElem.Alpha)
				);
				// Y座標を入力する
				float posY = spH * posC + posYOffset;	// 未達分を足す
				Vector3 pos = mComaInstance[reelC][posCtrl].transform.localPosition;
				pos.y = posY;
				mComaInstance[reelC][posCtrl].transform.localPosition = pos;
				// 切れ目を描画する
				if (posCtrl == 0){
					pos.y -= 0.5f * spH;
					mCutLine[reelC].transform.localPosition = pos;
					mCutLine[reelC].GetComponent<SpriteRenderer>().enabled = true;
				}
			}
			
			// リール速度に応じたブラー範囲を取得して、material経由でshaderに差分値を設定する
			float reelSpeed = reelData.reelSpeed; // [rpm]
			float delta = (reelSpeed * comaNum / 60f / (float)BLUR_BASEFPS) * spH;
			mReelMat[reelC].SetFloat("_BlurRange", delta);
		}
	}
	
	// オブジェクト終了時の処理
	void OnDestroy()
	{
		// TextureとMaterialの破棄
		mImageBuilder.DestroySprite();
		for(int i=0; i<mReelMat.Length; ++i) Destroy(mReelMat[i]);
		for(int i=0; i<mCutLine.Length; ++i) Destroy(mCutLine[i]);
		for(int i=0; i<mCutMat.Length; ++i) Destroy(mCutMat[i]);
	}
	
	// materialを変化させる
	private int[][] GetColor(){
		const int reelNum = SlotMaker2022.LocalDataSet.REEL_MAX;
		const int showNum = SlotMaker2022.LocalDataSet.SHOW_MAX;
		
		// 戻り値データ型作成
		int[][] ans = new int[reelNum][];
		for(int i=0; i<ans.Length; ++i) ans[i] = new int[showNum];
		
		// 使用するカラーマップ定義の一覧を取得する
		var useMap = new List<SlotEffectMaker2023.Data.ColorMapList>();
		var data = SlotEffectMaker2023.Singleton.EffectDataManagerSingleton.GetInstance();
		var act = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance();
		for (int i=0; i<MapShifterName.Length; ++i){
			if (MapShifterName[i] == null) continue;
			
			// 現在のMapデータを呼び出す
			string nowMapName = act.colorMapData.ExportElemName(MapShifterName[i]);
			if (nowMapName == null) continue;
			var nowMap = data.ColorMap.GetMapList(nowMapName);
			
			// タイマを参照して、有効ならデータをセットする
			var shifter = data.ColorMap.GetShifter(MapShifterName[i]);
			if (shifter == null) continue;
			var timer = act.timerData.GetTimer(shifter.UseTimerName);
			if (timer == null) continue;
			if (!timer.isActivate) continue;
			nowMap.SetCard((float)timer.elapsedTime);
			useMap.Add(nowMap);
		}
		
		// 各コマの色を設定する
		for (uint x=0; x<reelNum; ++x)
		for (uint y=0; y<showNum; ++y){
			int c = 0;
			for (int mapC = 0; mapC < useMap.Count; ++mapC){
				int srcColor = useMap[mapC].GetColor(x, y);
				//Debug.Log(srcColor);
				c = SlotEffectMaker2023.Data.ColorMapList.ComboColor(srcColor, c);
			}
			// yのインデックスが逆なことに注意する
			ans[(int)x][(int)(showNum - y - 1)] = c;
		}
		
		return ans;
	}
}
