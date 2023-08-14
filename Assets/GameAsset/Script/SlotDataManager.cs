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
		for(int i=0; i<slotData.reelData.Count; ++i){
			slotData.reelData[i].Process();
		}
		
		// リール始動テスト
		if (Input.GetKey(KeyCode.UpArrow)){
			for(int i=0; i<slotData.reelData.Count; ++i){
				slotData.reelData[i].Start();
			}
		}
		
		// リール停止テスト
		if (Input.GetKeyDown(KeyCode.LeftArrow )) slotData.reelData[0].SetStopPos(0);
		if (Input.GetKeyDown(KeyCode.DownArrow )) slotData.reelData[1].SetStopPos(0);
		if (Input.GetKeyDown(KeyCode.RightArrow)) slotData.reelData[2].SetStopPos(0);
	}
}
