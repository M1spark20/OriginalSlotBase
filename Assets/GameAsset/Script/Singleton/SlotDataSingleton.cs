using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class SlotDataSingleton
{
	// 変数(仮)
	public float[]	reelPos { get; set; }
	
    // Singletonインスタンス
    private static SlotDataSingleton ins = new SlotDataSingleton();
    
	/// <summary>
	/// インスタンスの初期化を行います。Singletonであるためprivateメンバです
	/// </summary>
	private SlotDataSingleton()
	{
		reelPos = new float[SlotMaker2022.LocalDataSet.REEL_MAX];
		for(int i=0; i<reelPos.Length; ++i) reelPos[i] = 11.0f + i;
	}
	
	/// <summary>
	/// インスタンスの取得を行います。
	/// </summary>
	public static SlotDataSingleton GetInstance() { return ins; }
}
