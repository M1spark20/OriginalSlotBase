using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class SlotDataSingleton
{
	public List<ReelBasicData>	reelData  { get; set; }
	public SlotBasicData		basicData { get; set; }
	
	// エフェクト用変数
	public SlotValManager		valManager { get; set; }
	
	// 音源データ
	public SoundDataList		soundData { get; set; }
	
    // Singletonインスタンス
    private static SlotDataSingleton ins = new SlotDataSingleton();
    
	/// <summary>
	/// インスタンスの初期化を行います。Singletonであるためprivateメンバです
	/// </summary>
	private SlotDataSingleton()
	{
		reelData = new List<ReelBasicData>();
		basicData = new SlotBasicData();
		valManager = new SlotValManager();
		soundData = new SoundDataList();
	}
	
	/// <summary>
	/// インスタンスの取得を行います。
	/// </summary>
	public static SlotDataSingleton GetInstance() { return ins; }
	
	public bool ReadData(ref TimerList timerList){
		// 読み込み処理
		// reelData
		// basicData
		valManager.ReadData();
		
		// データが読み込めなかった場合にリール情報を新規生成する
		if (reelData.Count == 0){
			for (int i=0; i<SlotMaker2022.LocalDataSet.REEL_MAX; ++i){
				reelData.Add(new ReelBasicData(12));
			}
		}
		
		// soundData
		soundData.DataStab(ref timerList);
		
		return true;
	}
	
	/// <summary>
	/// システム変数を更新します。
	/// </summary>
	public void Process(){
		valManager.GetVariable("_betCount")		.val = basicData.betCount;
		valManager.GetVariable("_creditCount")	.val = basicData.creditShow;
		valManager.GetVariable("_payoutCount")	.val = basicData.payoutShow;
		valManager.GetVariable("_isReplay")		.SetBool(basicData.isReplay);
	}
}
