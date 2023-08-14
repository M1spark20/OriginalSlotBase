using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class SlotDataSingleton
{
	// 変数(仮)
	//public float[]	reelPos { get; set; }
	public List<ReelBasicData>	reelData { get; set; }
	
    // Singletonインスタンス
    private static SlotDataSingleton ins = new SlotDataSingleton();
    
	/// <summary>
	/// インスタンスの初期化を行います。Singletonであるためprivateメンバです
	/// </summary>
	private SlotDataSingleton()
	{
		reelData = new List<ReelBasicData>();
		
		// 読み込み処理
		
		if (reelData.Count == 0){
			for (int i=0; i<SlotMaker2022.LocalDataSet.REEL_MAX; ++i){
				reelData.Add(new ReelBasicData(12));
			}
		}
	}
	
	/// <summary>
	/// インスタンスの取得を行います。
	/// </summary>
	public static SlotDataSingleton GetInstance() { return ins; }
}
