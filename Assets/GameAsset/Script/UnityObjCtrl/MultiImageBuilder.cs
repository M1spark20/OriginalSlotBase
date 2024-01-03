using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiImageBuilder
{
	float	mEachSizeX;	// 生成Spriteサイズ(横)
	float	mEachSizeY;	// 生成Spriteサイズ(縦)
	int		mSpriteNum;	// Sprite分割数
	float	mPPI;
	
	protected Sprite[]	mDrawSprite;	// 分割後生成Sprite
	
	/// <summary>
	/// 画像データをもとにSprite基本データを作成します
	/// </summary>
	protected void InitSpriteInfo(Texture2D srcImage, int divX, int divY){
		mEachSizeX = srcImage.width / divX;
		mEachSizeY = srcImage.height / divY;
		mSpriteNum = divX * divY;
		mPPI = 100.0f; // 今のところ定数
	}
	
	/// <summary>
	/// 画像データから特定範囲を切り出しクラス内部向けに1枚のSpriteを生成します
	/// 出展：https://takap-tech.com/entry/2021/07/24/160227
	/// </summary>
	protected Sprite CreateInternalSprite(Texture2D srcImage, float posX, float posY, float sizeW, float sizeH, string spName){
		Vector2 center = new Vector2(0.5f, 0.5f);
		
		Rect extractRect = new Rect(posX, posY, sizeW, sizeH);
		Sprite sp = Sprite.Create(srcImage, extractRect, center, mPPI, 0, SpriteMeshType.FullRect);
		sp.name = spName;
		return sp;
	}
	
	/// <summary>
	/// indexから画像の位置データを算出します。wayX=trueでx方向, falseでy方向にindexは進んでいきます
	/// </summary>
	protected (int x, int y) GetIndexXY(int index, int divX, int divY, bool wayX){
		int px = wayX ? (index % divX) : (index / divY);
		int py = wayX ? (index / divX) : (index % divY);
		return (px, py);
	}
	
	/// <summary>
	/// 画像データからSprite群を生成します
	/// </summary>
	public void BuildSprite(Texture2D srcImage, string defName, int divX, int divY, bool wayX){
		// divの妥当性確認
		if(divX <= 0 || divY <= 0) return;
		
		// 生成Spriteのサイズを取得
		InitSpriteInfo(srcImage, divX, divY);
		
		// Sprite配列のインスタンス生成
		mDrawSprite = new Sprite[mSpriteNum];
		
		// Sprite生成
		for (int spC=0; spC<mSpriteNum; ++spC){
			// 切り出し位置取得(座標基準が右下であることに注意)
			(int idX, int idY) = GetIndexXY(spC, divX, divY, wayX);
			float posX = mEachSizeX * idX;
			float posY = mEachSizeY * (divY - idY - 1);
			mDrawSprite[spC] = CreateInternalSprite(srcImage, posX, posY, mEachSizeX, mEachSizeY, defName + "_" + spC.ToString());
		}
	}
	
	/// <summary>
	/// 画像データからSprite群を生成します。生成順をitems配列により指定します
	/// </summary>
	public void BuildSprite(Texture2D srcImage, string defName, int divX, int divY, bool wayX, int[] items){
		// div, itemsの妥当性確認
		if (divX <= 0 || divY <= 0) return;
		if (items.Length <= 0) return;
		
		// 生成Spriteのサイズを取得
		InitSpriteInfo(srcImage, divX, divY);
		
		// items要素がすべて分割数内に収まるか確認)
		foreach (var id in items){
			if (id < 0 || id >= mSpriteNum) { Debug.Log("invalid item id for MultiImageBuilder"); return; }
		}
		
		// Sprite配列のインスタンス生成
		mDrawSprite = new Sprite[items.Length];
		
		// Sprite生成
		for (int spC=0; spC<items.Length; ++spC){
			// 切り出し位置取得(座標基準が右下であることに注意)
			(int idX, int idY) = GetIndexXY(items[spC], divX, divY, wayX);
			float posX = mEachSizeX * idX;
			float posY = mEachSizeY * (divY - idY - 1);
			mDrawSprite[spC] = CreateInternalSprite(srcImage, posX, posY, mEachSizeX, mEachSizeY, defName + "_" + spC.ToString());
		}
	}
	
	/// <summary>
	/// 生成したSpriteを読み込みます
	/// </summary>
	public Sprite Extract(int index){
		if (index < 0 || index >= mSpriteNum) { Debug.Log("invalid index id for MultiImageBuilder"); return null; }
		return mDrawSprite[index];
	}
	
	/// <summary>
	/// Spriteのサイズを取得します
	/// </summary>
	public (float w, float h) GetSpriteSize(){
		return (mEachSizeX / mPPI, mEachSizeY / mPPI);
	}
	
	/// <summary>
	/// 生成したSpriteの後片付けを行います(onDestroyで呼び出し必須)
	/// </summary>
	public void DestroySprite(){
		for (int spC=0; spC<mDrawSprite.Length; ++spC){
			if (mDrawSprite[spC]){
				UnityEngine.Object.Destroy(this.mDrawSprite[spC]);
			}
		}
		Debug.Log("Destroy called.");
	}
}
