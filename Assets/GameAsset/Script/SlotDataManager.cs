using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlotDataManager : MonoBehaviour
{
	// GameData定義(Singleton含む)
	SlotMaker2022.MainROMDataManagerSingleton	mainROM;	// mainROMツールデータ
	SlotDataSingleton							slotData;	// スロット基本情報
	SlotTimerManagerSingleton					timer;		// タイマー
	ISlotControllerBase							controller;	// ゲーム制御用クラス
	
	// Start is called before the first frame update
	void Start()
	{
		mainROM  = SlotMaker2022.MainROMDataManagerSingleton.GetInstance();
		slotData = SlotDataSingleton.GetInstance();
		timer    = SlotTimerManagerSingleton.GetInstance();
		
		// ファイルからデータを読み込む
		if (!mainROM .ReadData()) Debug.Log("mainROM Read: Error");  else Debug.Log("mainROM Read: Done");
		if (!slotData.ReadData()) Debug.Log("slotData Read: Error"); else Debug.Log("slotData Read: Done");
		if (!timer   .ReadData()) Debug.Log("timer Read: Error");    else Debug.Log("timer Read: Done");
		
		// コントローラー初期インスタンス生成
		controller = new SCWaitBet();
	}

	// Update is called once per frame
	void Update()
	{
		// 各SingletonのProcess
		timer.Process(Time.deltaTime);
		
		// リール停止テスト
		if (Input.GetKeyDown(KeyCode.LeftArrow )) slotData.reelData[0].SetStopPos(0);
		if (Input.GetKeyDown(KeyCode.DownArrow )) slotData.reelData[1].SetStopPos(0);
		if (Input.GetKeyDown(KeyCode.RightArrow)) slotData.reelData[2].SetStopPos(0);
		
		// KeyDown設定
		if(Input.GetKeyDown(KeyCode.UpArrow    )) controller.OnGetKeyDown(EGameButtonID.eMaxBetAndStart);
		if(Input.GetKey    (KeyCode.UpArrow    )) controller.OnGetKey    (EGameButtonID.eMaxBetAndStart);
		if(Input.GetKeyDown("1")) controller.OnGetKeyDown(EGameButtonID.e1Bet);
		if(Input.GetKey    ("1")) controller.OnGetKey    (EGameButtonID.e1Bet);
		if(Input.GetKeyDown("3")) controller.OnGetKeyDown(EGameButtonID.eMaxBet);
		if(Input.GetKey    ("3")) controller.OnGetKey    (EGameButtonID.eMaxBet);
		
		// キー入力後プロセス
		controller = controller.ProcessAfterInput();
		for(int i=0; i<slotData.reelData.Count; ++i){ slotData.reelData[i].Process(); }
	}
}
