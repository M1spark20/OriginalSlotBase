using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// リールチップ用のMultiImageBuilderインスタンスを管理するシングルトンコンポーネント。
/// メインリールとミニリール用のスプライト生成クラスを保持します。
/// </summary>
public class ReelChipHolder
{
	/// <summary>
	/// スプライト分割数のX方向。
	/// </summary>
	const int DIV_X = 1;
	/// <summary>
	/// スプライト分割数のY方向。
	/// </summary>
	const int DIV_Y = 10;

	/// <summary>
	/// メインリール用のスプライト生成クラス。
	/// </summary>
	public MultiImageBuilder ReelChipData { get; private set; }        // Sprite生成用クラス(各コマを格納)
	/// <summary>
	/// ミニリール用（サイズ半分）のスプライト生成クラス。
	/// </summary>
	public MultiImageBuilder ReelChipDataMini { get; private set; }    // サイズ半分のクラス(各コマを格納)

	// Singletonインスタンス
	private static ReelChipHolder ins = new ReelChipHolder();

	/// <summary>
	/// コンストラクタ。MultiImageBuilderインスタンスを初期化します。
	/// </summary>
	private ReelChipHolder()
	{
		ReelChipData = new MultiImageBuilder();
		ReelChipDataMini = new MultiImageBuilder();
	}

	/// <summary>
	/// テクスチャからリール用スプライトを生成します。
	/// </summary>
	/// <param name="ReelChip">メインリール用テクスチャ。</param>
	/// <param name="ReelChipMini">ミニリール用テクスチャ。</param>
	public void Init(Texture2D ReelChip, Texture2D ReelChipMini)
	{
		ReelChipData.BuildSprite(ReelChip, "reelMain", DIV_X, DIV_Y, false);
		ReelChipDataMini.BuildSprite(ReelChipMini, "reelMini", DIV_X, DIV_Y, false);
	}

	// instance取得
	/// <summary>
	/// ReelChipHolderのシングルトンインスタンスを取得します。
	/// </summary>
	/// <returns>ReelChipHolderインスタンス。</returns>
	public static ReelChipHolder GetInstance() { return ins; }
}