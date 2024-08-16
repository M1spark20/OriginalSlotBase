using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class SlotDataManager : MonoBehaviour
{
	// GameData定義(Singleton含む)
	SlotMaker2022.MainROMDataManagerSingleton					mainROM;	// mainROMツールデータ
	SlotEffectMaker2023.Singleton.EffectDataManagerSingleton	effectData;	// エフェクト固定データ
	SlotEffectMaker2023.Singleton.SlotDataSingleton				slotData;	// スロット基本情報
	SlotEffectMaker2023.Action.SlotTimerManager 				timer;		// タイマー(slotDataから抜粋)
	ISlotControllerBase											controller;	// ゲーム制御用クラス
	ReelChipHolder												chipData;	// リール図柄格納データ
	
	bool MenuShown;	// メインメニュー表示中かのフラグ
	
	// リールチップ画像を指定
	[SerializeField] private Texture2D ReelChip;
	[SerializeField] private Texture2D ReelChipMini;
	[SerializeField] private TextAsset MainROM;
	[SerializeField] private TextAsset EffectData;
	
	[SerializeField] private GameObject MainMenuObj;
	private Canvas MainMenuCanvas;
	private GraphicRaycaster MainMenuTouch;
	private MainMenuManager MainMenuScr;
	
	private bool[] GetKeyDownJoin;
	private bool[] GetKeyJoin;
	[SerializeField] private GraphicRaycaster TouchPanel;
	[SerializeField] private SteamworksAPIManager SteamAPI;
	
	// セーブデータ元パス
	private string SavePath;
	
	void Awake()
	{
		mainROM    = SlotMaker2022.MainROMDataManagerSingleton.GetInstance();
		effectData = SlotEffectMaker2023.Singleton.EffectDataManagerSingleton.GetInstance();
		slotData   = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance();
		timer      = slotData.timerData;
		chipData   = ReelChipHolder.GetInstance();
		chipData.Init(ReelChip, ReelChipMini);
		SavePath   = Application.persistentDataPath + "/SaveData.bytes";
		Debug.Log(SavePath);
		
		// タイマ作成用データ生成
		var tList = new SlotEffectMaker2023.Data.TimerList();
		
		// ファイルからデータを読み込む
		if (!mainROM   .ReadData(MainROM))    Debug.Log("mainROM Read: Error");    else Debug.Log("mainROM Read: Done");
		if (!effectData.ReadData(EffectData)) Debug.Log("effectData Read: Error"); else Debug.Log("effectData Read: Done");
		if (!slotData  .ReadData(SavePath))   Debug.Log("slotData Read: Error");   else Debug.Log("slotData Read: Done");
		
		// Singleton初期化
		slotData.Init(effectData.SoundPlayList, effectData.TimerList, effectData.VarList, effectData.ColorMap.shifter, effectData.Collection);
		
		// コントローラー初期インスタンス生成
		controller = new SCWaitBet();
		// システム変数更新(初期化)
		slotData.Process();
		
		// メニュー非表示からスタート
		MainMenuScr = MainMenuObj.GetComponent<MainMenuManager>();
		MainMenuCanvas = MainMenuObj.GetComponent<Canvas>();
		MainMenuTouch = MainMenuObj.GetComponent<GraphicRaycaster>();
		MenuShown = false;
		MainMenuCanvas.enabled = MenuShown;
		MainMenuTouch.enabled = MenuShown;
		
		// タッチ入力関連初期化
		GetKeyDownJoin = new bool[(int)EGameButtonID.eButtonMax];
		GetKeyJoin     = new bool[(int)EGameButtonID.eButtonMax];
		ResetTouchStatus();
		
		// 各端末で60fpsにする
		Application.targetFrameRate = 60;
	}

	// Update is called once per frame
	void Update()
	{
		// タイマーのみ先に更新
		timer.Process(Time.deltaTime);
		
		// Menu表示ボタン(描画は暫定)
		if (Input.GetKeyDown("m")) MenuShowToggle();
		
		// KeyDown設定(Menu表示状態により制御を変える)
		if (MenuShown){
			// メニュー画面の制御
			if(Input.GetKeyDown(KeyCode.UpArrow   )) MainMenuScr.OnGetKeyDown(EMenuButtonID.eScrUp);
			if(Input.GetKeyDown(KeyCode.LeftArrow )) MainMenuScr.OnGetKeyDown(EMenuButtonID.eScrLeft);
			if(Input.GetKeyDown(KeyCode.DownArrow )) MainMenuScr.OnGetKeyDown(EMenuButtonID.eScrDn);
			if(Input.GetKeyDown(KeyCode.RightArrow)) MainMenuScr.OnGetKeyDown(EMenuButtonID.eScrRight);
		} else {
			// ゲーム本体の制御
			if(Input.GetKeyDown("1")) OnScreenTouch(EGameButtonID.e1Bet);
			if(Input.GetKey    ("1")) OnScreenHover(EGameButtonID.e1Bet);
			if(Input.GetKeyDown("3")) OnScreenTouch(EGameButtonID.eMaxBet);
			if(Input.GetKey    ("3")) OnScreenHover(EGameButtonID.eMaxBet);
			if(Input.GetKeyDown(KeyCode.UpArrow   )) OnScreenTouch(EGameButtonID.eMaxBetAndStart);
			if(Input.GetKey    (KeyCode.UpArrow   )) OnScreenHover(EGameButtonID.eMaxBetAndStart);
			if(Input.GetKeyDown(KeyCode.LeftArrow )) OnScreenTouch(EGameButtonID.e1Reel);
			if(Input.GetKey    (KeyCode.LeftArrow )) OnScreenHover(EGameButtonID.e1Reel);
			if(Input.GetKeyDown(KeyCode.DownArrow )) OnScreenTouch(EGameButtonID.e2Reel);
			if(Input.GetKey    (KeyCode.DownArrow )) OnScreenHover(EGameButtonID.e2Reel);
			if(Input.GetKeyDown(KeyCode.RightArrow)) OnScreenTouch(EGameButtonID.e3Reel);
			if(Input.GetKey    (KeyCode.RightArrow)) OnScreenHover(EGameButtonID.e3Reel);
			// キー制御
			for(int i=0; i<(int)EGameButtonID.eButtonMax; ++i){
				if (GetKeyDownJoin[i]) controller.OnGetKeyDown((EGameButtonID)i);
				if (GetKeyJoin    [i]) controller.OnGetKey    ((EGameButtonID)i);
			}
		}
		
		ResetTouchStatus();
		// キー入力後プロセス
		controller = controller.ProcessAfterInput(DataSaveAct, CheckAchievementAct);
		// システム変数更新
		slotData.Process();
	}
	
	private void SetMenuShown(){
		MainMenuCanvas.enabled = MenuShown;
		MainMenuTouch.enabled = MenuShown;
		TouchPanel.enabled = !MenuShown;
		MainMenuScr.OnMenuShownChange(MenuShown);
	}
	
	public void MenuShowToggle(){
		MenuShown ^= true;
		SetMenuShown();
	}
	
	public void MenuHide(){
		MenuShown = false;
		SetMenuShown();
	}
	
	public void OnScreenTouch(EGameButtonID pID){
		GetKeyDownJoin[(int)pID] = true;
	}
	
	public void OnScreenHover(EGameButtonID pID){
		GetKeyJoin    [(int)pID] = true;
	}
	
	// タッチ初期化
	private void ResetTouchStatus(){
		for(int i=0; i<(int)EGameButtonID.eButtonMax; ++i){
			GetKeyDownJoin[i] = false;
			GetKeyJoin    [i] = false;
		}
	}
	
	// Steam実績確認時コールバック
	public void CheckAchievementAct(){
		// システム変数のみ更新してから評価する
		slotData.UpdateSysVar();
		SteamAPI.OnGameStateChange();
	}
	
	// セーブ時コールバック
	public void DataSaveAct(){
		slotData.SaveData(SavePath);
	}
}
