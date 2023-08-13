using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlotDataManager : MonoBehaviour
{
	// GameData(Singleton)定義
	SlotMaker2022.MainROMDataManagerSingleton	mainROM;	// mainROMツールデータ
	SlotDataSingleton							slotData;	// スロット基本情報
	
	// Start is called before the first frame update
	void Start()
	{
		mainROM = SlotMaker2022.MainROMDataManagerSingleton.GetInstance();
		slotData = SlotDataSingleton.GetInstance();
		
		// ファイルからデータを読み込む
		if (!mainROM.ReadData()) Debug.Log("mainROM Read: Error"); else Debug.Log("mainROM Read: Done");
	}

	// Update is called once per frame
	void Update()
	{
		for(int i=0; i<slotData.reelPos.Length; ++i){
			//slotData.reelPos[i] += 0.01f;
			
			// リール位置補正
			int comaMax = SlotMaker2022.LocalDataSet.COMA_MAX;
			while (slotData.reelPos[i] > comaMax) slotData.reelPos[i] -= comaMax;
			while (slotData.reelPos[i] < 0) slotData.reelPos[i] += comaMax;
		}
	}
}
