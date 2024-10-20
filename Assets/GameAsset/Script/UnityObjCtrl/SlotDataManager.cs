using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;

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
	bool KeyAvailable;	// キー入力を受け付けるか設定
	
	// リールチップ画像を指定
	[SerializeField] private Texture2D ReelChip;
	[SerializeField] private Texture2D ReelChipMini;
	[SerializeField] private TextAsset MainROM;
	[SerializeField] private TextAsset EffectData;
	
	[SerializeField] private GameObject MainMenuObj;
	private Canvas MainMenuCanvas;
	private CanvasGroup MainMenuCanvasGroup;
	private GraphicRaycaster MainMenuTouch;
	private MainMenuManager MainMenuScr;
	
	private bool[] GetKeyDownJoin;
	private bool[] GetKeyJoin;
	[SerializeField] private GraphicRaycaster TouchPanel;
	[SerializeField] private SteamworksAPIManager SteamAPI;
	[SerializeField] private UILanguageChanger LangChanger;
	
	[SerializeField] private GameObject ResetCaution;
	private Canvas CautionCanvas;
	private GraphicRaycaster CautionTouch;
	
	// メニューグラデーション
	private const float GR_TIME = 0.1f;
	private float grStartTime;
	
	// セーブデータ元パス
	private string SavePath;
	private string SaveSysPath;
	private string BackupPath;
	
	void Awake()
	{
		// Android環境でのカクつき防止のため、現在時刻を冒頭に1回取得しておく(初回取得が非常に重い様子)
		var dummyT = DateTime.Now;
		
		// データの初期化を開始する
		mainROM    = SlotMaker2022.MainROMDataManagerSingleton.GetInstance();
		effectData = SlotEffectMaker2023.Singleton.EffectDataManagerSingleton.GetInstance();
		slotData   = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance();
		chipData   = ReelChipHolder.GetInstance();
		chipData.Init(ReelChip, ReelChipMini);
		SaveSysPath= Application.persistentDataPath + "/System.bytes";
		
		// システムデータを読み込む
		bool sysReadFlag = slotData  .ReadSysData(SaveSysPath);
		if (!sysReadFlag) Debug.Log("sysData Read: Error");   else Debug.Log("sysData Read: Done");
		
		string saveDataSW = slotData.sysData.UseSaveDataID > 0 ? slotData.sysData.UseSaveDataID.ToString() : "";
		SavePath   = Application.persistentDataPath + "/SaveData" + saveDataSW +".bytes";
		BackupPath = Application.persistentDataPath + "/Backup" + saveDataSW + ".bak";
		//Debug.Log(SavePath);
		
		// タイマ作成用データ生成
		var tList = new SlotEffectMaker2023.Data.TimerList();
		
		// ファイルからデータを読み込む
		if (!mainROM   .ReadData(MainROM))    Debug.Log("mainROM Read: Error");    else Debug.Log("mainROM Read: Done");
		if (!effectData.ReadData(EffectData)) Debug.Log("effectData Read: Error"); else Debug.Log("effectData Read: Done");
		if (!slotData  .ReadData(SavePath))   Debug.Log("slotData Read: Error");   else Debug.Log("slotData Read: Done");
		
		// Cautionウィンドウ表示
		CautionCanvas = ResetCaution.GetComponent<Canvas>();
		CautionTouch  = ResetCaution.GetComponent<GraphicRaycaster>();
		CautionCanvas.enabled = slotData.sysData.ResetFlag;
		CautionTouch .enabled = slotData.sysData.ResetFlag;
		KeyAvailable = !slotData.sysData.ResetFlag;
		
		// データリセット発火判定(20241014追加)
		if (slotData.sysData.ResetFlag) {
			slotData.ResetData(BackupPath);
		}
		
		// Singleton初期化
		slotData.Init(effectData.SoundPlayList, effectData.TimerList, effectData.VarList, effectData.ColorMap.shifter, effectData.Collection);
		timer      = slotData.timerData;	// インスタンス確定後に参照を追加することでリセットを可能にする
		
		// コントローラー初期インスタンス生成
		controller = new SCWaitBet();
		// システム変数更新(初期化)
		slotData.Process();
		
		MainMenuScr = MainMenuObj.GetComponent<MainMenuManager>();
		MainMenuCanvas = MainMenuObj.GetComponent<Canvas>();
		MainMenuCanvasGroup = MainMenuObj.GetComponent<CanvasGroup>();
		MainMenuTouch = MainMenuObj.GetComponent<GraphicRaycaster>();
		
		// SystemDataの読み込みに失敗した場合、または読み込んだデータのバージョンが最新でない場合に初期起動としてメニューを自動表示する
		const int LatestSysVersion = SlotEffectMaker2023.Singleton.SlotDataSingleton.FILE_VERSION_SYS;
		MenuShown = !sysReadFlag || slotData.sysData.ReadVersion != LatestSysVersion;
		MainMenuCanvas.enabled = true;
		MainMenuCanvasGroup.alpha = 0f;
		SetMenuShown();
		grStartTime = -GR_TIME;
		
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
		if (KeyAvailable) {
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
		}
		
		// Menuグラデーション
		float dt = CalcDT();
		if (dt > GR_TIME) MainMenuCanvasGroup.alpha = MenuShown ? 1f : 0f;
		else MainMenuCanvasGroup.alpha = MenuShown ? dt / GR_TIME : 1f - (dt / GR_TIME);
		
		ResetTouchStatus();
		// キー入力後プロセス
		controller = controller.ProcessAfterInput(DataSaveAct, CheckAchievementAct);
		// システム変数更新
		slotData.Process();
	}
	
	// グラデーション時間取得
	private float CalcDT() { return Time.time - grStartTime; }
	
	private void SetMenuShown(){
		grStartTime = Time.time;
		MainMenuTouch.enabled = MenuShown;
		TouchPanel.enabled = !MenuShown;
		MainMenuScr.OnMenuShownChange(MenuShown);
	}
	
	public void MenuShowToggle(){
		if (CalcDT() < GR_TIME) return;
		MenuShown ^= true;
		SetMenuShown();
	}
	
	public void MenuHide(){
		if (!MenuShown || CalcDT() < GR_TIME) return;
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
		slotData.SaveSysData(SaveSysPath);
	}
	// Object無効化時にデータを保存する (20241020Add)データリセット時はバックアップを保存する
	private void OnDisable(){
		if (slotData.sysData.ResetFlag) File.Copy(SavePath, BackupPath, true);
		slotData.SaveSysData(SaveSysPath);
	}
	
	/// Cautionウィンドウ ///
	// 閉じる場合はQuitスクリプトを呼び出す
	// 続行する場合
	public void CautionStart(){
		CautionCanvas.enabled = false;
		CautionTouch .enabled = false;
		KeyAvailable = true;
	}
}
