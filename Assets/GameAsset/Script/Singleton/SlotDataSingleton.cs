using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class SlotDataSingleton
{
	public List<ReelBasicData>	reelData  { get; set; }
	public SlotBasicData		basicData { get; set; }
	
    // Singletonインスタンス
    private static SlotDataSingleton ins = new SlotDataSingleton();
    
	/// <summary>
	/// インスタンスの初期化を行います。Singletonであるためprivateメンバです
	/// </summary>
	private SlotDataSingleton()
	{
		reelData = new List<ReelBasicData>();
		basicData = new SlotBasicData();
	}
	
	/// <summary>
	/// インスタンスの取得を行います。
	/// </summary>
	public static SlotDataSingleton GetInstance() { return ins; }
	
	public bool ReadData(){
		// 読み込み処理
		// reelData
		// basicData
		
		// データが読み込めなかった場合にリール情報を新規生成する
		if (reelData.Count == 0){
			for (int i=0; i<SlotMaker2022.LocalDataSet.REEL_MAX; ++i){
				reelData.Add(new ReelBasicData(12));
			}
		}
		return true;
	}
}
