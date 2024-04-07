using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReelChipHolder
{
	const int DIV_X = 1;
	const int DIV_Y = 10;
	
	public MultiImageBuilder ReelChipData { get; private set; }	// Sprite生成用クラス(各コマを格納)
	
	// Singletonインスタンス
	private static ReelChipHolder ins = new ReelChipHolder();
	
	private ReelChipHolder()
	{
		ReelChipData = new MultiImageBuilder();
	}
	
	public void Init(Texture2D ReelChip){
		ReelChipData.BuildSprite(ReelChip, "reelMain", DIV_X, DIV_Y, false);
	}
	
	// instance取得
	public static ReelChipHolder GetInstance() { return ins; }
}
