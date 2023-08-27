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
		
		// タイマ作成用データ生成
		TimerList tList = new TimerList();
		
		// ファイルからデータを読み込む
		if (!mainROM .ReadData())      Debug.Log("mainROM Read: Error");  else Debug.Log("mainROM Read: Done");
		if (!slotData.ReadData())      Debug.Log("slotData Read: Error"); else Debug.Log("slotData Read: Done");
		if (!timer   .ReadData(tList)) Debug.Log("timer Read: Error");    else Debug.Log("timer Read: Done");
		
		// コントローラー初期インスタンス生成
		controller = new SCWaitBet();
	}

	// Update is called once per frame
	void Update()
	{
		// 各SingletonのProcess
		timer.Process(Time.deltaTime);
		
		// KeyDown設定
		if(Input.GetKeyDown("1")) controller.OnGetKeyDown(EGameButtonID.e1Bet);
		if(Input.GetKey    ("1")) controller.OnGetKey    (EGameButtonID.e1Bet);
		if(Input.GetKeyDown("3")) controller.OnGetKeyDown(EGameButtonID.eMaxBet);
		if(Input.GetKey    ("3")) controller.OnGetKey    (EGameButtonID.eMaxBet);
		if(Input.GetKeyDown(KeyCode.UpArrow   )) controller.OnGetKeyDown(EGameButtonID.eMaxBetAndStart);
		if(Input.GetKey    (KeyCode.UpArrow   )) controller.OnGetKey    (EGameButtonID.eMaxBetAndStart);
		if(Input.GetKeyDown(KeyCode.LeftArrow )) controller.OnGetKeyDown(EGameButtonID.e1Reel);
		if(Input.GetKey    (KeyCode.LeftArrow )) controller.OnGetKey    (EGameButtonID.e1Reel);
		if(Input.GetKeyDown(KeyCode.DownArrow )) controller.OnGetKeyDown(EGameButtonID.e2Reel);
		if(Input.GetKey    (KeyCode.DownArrow )) controller.OnGetKey    (EGameButtonID.e2Reel);
		if(Input.GetKeyDown(KeyCode.RightArrow)) controller.OnGetKeyDown(EGameButtonID.e3Reel);
		if(Input.GetKey    (KeyCode.RightArrow)) controller.OnGetKey    (EGameButtonID.e3Reel);
		
		// キー入力後プロセス
		controller = controller.ProcessAfterInput();
		// システム変数更新
		slotData.Process();
	}
}
