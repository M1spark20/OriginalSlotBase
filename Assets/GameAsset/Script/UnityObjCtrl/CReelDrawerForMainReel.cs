using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// アタッチしたGameObjectの位置をコマ(0,0)の場所に指定すること
/// <summary>
/// メインリールの描画を行うコンポーネント。
/// アタッチされたGameObjectの位置を基準にリールコマを生成し、
/// フレーム毎に描画位置や色、ブラーエフェクトを更新します。
/// </summary>
public class CReelDrawerForMainReel : MonoBehaviour
{
	/// <summary>リール横幅基準値。</summary>
	const float POS_WBASE = 3.46f;
	/// <summary>描画開始インデックス。</summary>
	const int START_INDEX = -2;
	/// <summary>描画対象コマ数。</summary>
	const int DRAW_NUM = 7;

	MultiImageBuilder mImageBuilder;    // Sprite生成用クラス(各コマを格納)
	GameObject[][] mComaInstance;    // Instantiateしたコマオブジェクト
	GameObject[] mCutLine;         // 切れ目用オブジェクト

	// ReelBlur用変数
	const int BLUR_BASEFPS = 90;          // ブラー計算のベースFPS
	float[] mLastPosDelta;              // 前フレーム位置差分
	Material[] mReelMat;                   // 各リール用マテリアル
	Material[] mCutMat;                    // 切れ目用マテリアル

	// 描画画像
	/// <summary>コマ表示用チッププレハブ。</summary>
	[SerializeField] private GameObject PrehabChip;
	/// <summary>切れ目表示用プレハブ。</summary>
	[SerializeField] private GameObject PrehabCutLine;
	/// <summary>リール用カラーマップシフタ名配列。</summary>
	[SerializeField] private string[] MapShifterName;

	/// <summary>
	/// Start は最初のフレーム更新前に一度だけ呼び出され、
	/// 各リールのコマオブジェクトの生成・初期化を行います。
	/// </summary>
	void Start()
	{
		// リール数を取得してImageBuilderとTextureのインスタンスを生成する
		const int reelNum = SlotMaker2022.LocalDataSet.REEL_MAX;
		mComaInstance = new GameObject[reelNum][];
		mCutLine = new GameObject[reelNum];
		mImageBuilder = ReelChipHolder.GetInstance().ReelChipData;

		// ReelBlur用変数初期化
		mReelMat = new Material[reelNum];
		mCutMat = new Material[reelNum];

		// Prehabと親Transformの定義
		Transform parent = this.transform;

		// 各リールごとにコマをInstantiateし、SpriteとMaterialを設定
		var mainROM = SlotMaker2022.MainROMDataManagerSingleton.GetInstance();
		const int comaNum = SlotMaker2022.LocalDataSet.COMA_MAX;
		for (int reelC = 0; reelC < reelNum; ++reelC)
		{
			mComaInstance[reelC] = new GameObject[comaNum];
			string test = "";

			// リールブラー用Materialをプレハブからコピーして作成
			mReelMat[reelC] = new Material(PrehabChip.GetComponent<SpriteRenderer>().sharedMaterial);

			// 切れ目オブジェクトの生成
			mCutLine[reelC] = Instantiate(PrehabCutLine, parent);

			for (int posC = 0; posC < comaNum; ++posC)
			{
				// データは逆順格納されているため、縦位置のみ後で調整
				int comaIndex = mainROM.ReelArray[reelC][posC].Coma;
				test += comaIndex.ToString();
				Vector3 initPos = new Vector3(POS_WBASE * reelC, 0.0f, 0.0f);
				mComaInstance[reelC][comaNum - posC - 1] = Instantiate(PrehabChip, parent);
				mComaInstance[reelC][comaNum - posC - 1].transform.localPosition = initPos;

				// SpriteRendererにSpriteとMaterialを設定
				SpriteRenderer sp = mComaInstance[reelC][comaNum - posC - 1].GetComponent<SpriteRenderer>();
				sp.sprite = mImageBuilder.Extract(comaIndex);
				sp.sharedMaterial = mReelMat[reelC];
			}
			Debug.Log(test);
		}
	}

	/// <summary>
	/// Update は毎フレーム呼び出され、リールコマの可視化更新とブラーエフェクトの適用を行います。
	/// </summary>
	void Update()
	{
		// Spriteサイズ取得
		(float spW, float spH) = mImageBuilder.GetSpriteSize();
		const int comaNum = SlotMaker2022.LocalDataSet.COMA_MAX;
		const int showNum = SlotMaker2022.LocalDataSet.SHOW_MAX;
		int[][] matColor = GetColor();

		for (int reelC = 0; reelC < mComaInstance.Length; ++reelC)
		{
			// まず全GameObjectを非表示に
			mCutLine[reelC].GetComponent<SpriteRenderer>().enabled = false;
			for (int posC = 0; posC < mComaInstance[reelC].Length; ++posC)
				mComaInstance[reelC][posC].GetComponent<SpriteRenderer>().enabled = false;

			// リール位置データ取得
			var reelData = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance().reelData[reelC];
			float reelPos = reelData.reelPos;
			byte baseComaID = reelData.GetReelComaID();
			float margin = (float)baseComaID - reelPos;
			float posYOffset = spH * margin;

			// 表示対象コマを選択し、色と位置を設定
			for (int posC = START_INDEX; posC < START_INDEX + DRAW_NUM; ++posC)
			{
				int posCtrl = baseComaID + posC;
				while (posCtrl >= comaNum) posCtrl -= comaNum;
				while (posCtrl < 0) posCtrl += comaNum;

				var sr = mComaInstance[reelC][posCtrl].GetComponent<SpriteRenderer>();
				sr.enabled = true;
				int colorH = posC < 0 ? 0 : posC >= showNum ? showNum - 1 : posC;
				int setColor = matColor[reelC][colorH];
				sr.color = new Color32(
					SlotEffectMaker2023.Data.ColorMapList.GetColorElem(setColor, SlotEffectMaker2023.Data.ColorMapElem.Red),
					SlotEffectMaker2023.Data.ColorMapList.GetColorElem(setColor, SlotEffectMaker2023.Data.ColorMapElem.Green),
					SlotEffectMaker2023.Data.ColorMapList.GetColorElem(setColor, SlotEffectMaker2023.Data.ColorMapElem.Blue),
					SlotEffectMaker2023.Data.ColorMapList.GetColorElem(setColor, SlotEffectMaker2023.Data.ColorMapElem.Alpha)
				);

				float posY = spH * posC + posYOffset;
				var pos = mComaInstance[reelC][posCtrl].transform.localPosition;
				pos.y = posY;
				mComaInstance[reelC][posCtrl].transform.localPosition = pos;

				// 切れ目の表示
				if (posCtrl == 0)
				{
					pos.y -= 0.5f * spH;
					mCutLine[reelC].transform.localPosition = pos;
					mCutLine[reelC].GetComponent<SpriteRenderer>().enabled = true;
				}
			}

			// ブラー範囲計算と適用
			float reelSpeed = reelData.reelSpeed;
			float delta = (reelSpeed * comaNum / 60f / (float)BLUR_BASEFPS) * spH;
			mReelMat[reelC].SetFloat("_BlurRange", delta);
		}
	}

	/// <summary>
	/// OnDestroy はオブジェクト破棄時に呼び出され、生成したSpriteやMaterialを破棄します。
	/// </summary>
	void OnDestroy()
	{
		// TextureとMaterialの破棄
		mImageBuilder.DestroySprite();
		for (int i = 0; i < mReelMat.Length; ++i) Destroy(mReelMat[i]);
		for (int i = 0; i < mCutLine.Length; ++i) Destroy(mCutLine[i]);
		for (int i = 0; i < mCutMat.Length; ++i) Destroy(mCutMat[i]);
	}

	/// <summary>
	/// 現在有効なカラーマップシフタを用いて、
	/// 各リール・各コマの頂点カラーを計算して返します。
	/// </summary>
	/// <returns>リールごと、表示コマ数分の色番号を格納した二次元配列。</returns>
	private int[][] GetColor()
	{
		const int reelNum = SlotMaker2022.LocalDataSet.REEL_MAX;
		const int showNum = SlotMaker2022.LocalDataSet.SHOW_MAX;

		int[][] ans = new int[reelNum][];
		for (int i = 0; i < ans.Length; ++i) ans[i] = new int[showNum];

		// 有効シフタのマップ一覧取得
		var useMap = new List<SlotEffectMaker2023.Data.ColorMapList>();
		var data = SlotEffectMaker2023.Singleton.EffectDataManagerSingleton.GetInstance();
		var act = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance();
		for (int i = 0; i < MapShifterName.Length; ++i)
		{
			if (MapShifterName[i] == null) continue;
			string nowMapName = act.colorMapData.ExportElemName(MapShifterName[i]);
			if (nowMapName == null) continue;
			var nowMap = data.ColorMap.GetMapList(nowMapName);
			var shifter = data.ColorMap.GetShifter(MapShifterName[i]);
			if (shifter == null) continue;
			var timer = act.timerData.GetTimer(shifter.UseTimerName);
			if (timer == null || !timer.isActivate) continue;
			nowMap.SetCard((float)timer.elapsedTime);
			useMap.Add(nowMap);
		}

		// 各コマに色を適用
		for (uint x = 0; x < reelNum; ++x)
			for (uint y = 0; y < showNum; ++y)
			{
				int c = 0;
				for (int mapC = 0; mapC < useMap.Count; ++mapC)
				{
					int srcColor = useMap[mapC].GetColor(x, y);
					c = SlotEffectMaker2023.Data.ColorMapList.ComboColor(srcColor, c);
				}
				ans[(int)x][(int)(showNum - y - 1)] = c;
			}

		return ans;
	}
}