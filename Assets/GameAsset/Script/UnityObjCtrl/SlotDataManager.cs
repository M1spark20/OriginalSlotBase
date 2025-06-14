using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

/// <summary>
/// ゲーム全体のデータ管理と入力/UI制御を担うマネージャクラス。
/// シングルトンからデータ読み込み、UI初期化、メニュー表示、入力処理、データ保存までを統括します。
/// </summary>
public class SlotDataManager : MonoBehaviour
{
	// GameData定義(Singleton含む)
	private SlotMaker2022.MainROMDataManagerSingleton mainROM;       // mainROMツールデータ
	private SlotEffectMaker2023.Singleton.EffectDataManagerSingleton effectData; // エフェクト固定データ
	private SlotEffectMaker2023.Singleton.SlotDataSingleton slotData;          // スロット基本情報
	private SlotEffectMaker2023.Action.SlotTimerManager timer;                  // タイマー(slotDataから抜粋)
	private ISlotControllerBase controller;                                     // ゲーム制御用クラス
	private ReelChipHolder chipData;                                            // リール図柄格納データ

	private bool MenuShown;       // メインメニュー表示中フラグ
	private bool KeyAvailable;    // キー入力受付フラグ

	/// <summary>メインリール用チップテクスチャ</summary>
	[SerializeField] private Texture2D ReelChip;
	/// <summary>ミニリール用チップテクスチャ</summary>
	[SerializeField] private Texture2D ReelChipMini;
	[SerializeField] private TextAsset MainROM;     // MainROM JSONデータ
	[SerializeField] private TextAsset EffectData;  // EffectData JSONデータ

	[SerializeField] private GameObject MainMenuObj;      // メインメニューUIオブジェクト
	private Canvas MainMenuCanvas;
	private CanvasGroup MainMenuCanvasGroup;
	private GraphicRaycaster MainMenuTouch;
	private MainMenuManager MainMenuScr;

	private bool[] GetKeyDownJoin;
	private bool[] GetKeyJoin;
	[SerializeField] private GraphicRaycaster TouchPanel;  // ゲーム入力用タッチパネル
	[SerializeField] private SteamworksAPIManager SteamAPI; // 実績管理
	[SerializeField] private UILanguageChanger LangChanger; // 言語切替

	[SerializeField] private GameObject ResetCaution;  // リセット確認ダイアログ
	private Canvas CautionCanvas;
	private GraphicRaycaster CautionTouch;

	private const float GR_TIME = 0.1f; // メニューグラデーション時間
	private float grStartTime;

	private string SavePath;
	private string SaveSysPath;
	private string BackupPath;

	/// <summary>
	/// Awake は最初に呼び出され、シングルトンインスタンス取得、データロード、UI初期化、タイマー・フレームレート設定を行います。
	/// </summary>
	private void Awake()
	{
		// Android初回DateTime取得遅延回避
		var dummyT = DateTime.Now;

		mainROM = SlotMaker2022.MainROMDataManagerSingleton.GetInstance();
		effectData = SlotEffectMaker2023.Singleton.EffectDataManagerSingleton.GetInstance();
		slotData = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance();
		chipData = ReelChipHolder.GetInstance();
		chipData.Init(ReelChip, ReelChipMini);

		SaveSysPath = Application.persistentDataPath + "/System.bytes";
		bool sysReadFlag = slotData.ReadSysData(SaveSysPath);
		Debug.Log(sysReadFlag ? "sysData Read: Done" : "sysData Read: Error");

		string saveDataSW = slotData.sysData.UseSaveDataID > 0 ? slotData.sysData.UseSaveDataID.ToString() : string.Empty;
		SavePath = Application.persistentDataPath + "/SaveData" + saveDataSW + ".bytes";
		BackupPath = Application.persistentDataPath + "/Backup" + saveDataSW + ".bak";

		timer = slotData.timerData;

		if (!mainROM.ReadData(MainROM)) Debug.Log("mainROM Read: Error"); else Debug.Log("mainROM Read: Done");
		if (!effectData.ReadData(EffectData)) Debug.Log("effectData Read: Error"); else Debug.Log("effectData Read: Done");
		if (!slotData.ReadData(SavePath)) Debug.Log("slotData Read: Error"); else Debug.Log("slotData Read: Done");

		CautionCanvas = ResetCaution.GetComponent<Canvas>();
		CautionTouch = ResetCaution.GetComponent<GraphicRaycaster>();
		CautionCanvas.enabled = slotData.sysData.ResetFlag;
		CautionTouch.enabled = slotData.sysData.ResetFlag;
		KeyAvailable = !slotData.sysData.ResetFlag;

		if (slotData.sysData.ResetFlag)
			slotData.ResetData(BackupPath);

		slotData.Init(effectData.SoundPlayList, effectData.TimerList, effectData.VarList, effectData.ColorMap.shifter, effectData.Collection);
		controller = new SCWaitBet();
		slotData.Process();

		MainMenuScr = MainMenuObj.GetComponent<MainMenuManager>();
		MainMenuCanvas = MainMenuObj.GetComponent<Canvas>();
		MainMenuCanvasGroup = MainMenuObj.GetComponent<CanvasGroup>();
		MainMenuTouch = MainMenuObj.GetComponent<GraphicRaycaster>();

		const int LatestSysVersion = SlotEffectMaker2023.Singleton.SlotDataSingleton.FILE_VERSION_SYS;
		MenuShown = !sysReadFlag || slotData.sysData.ReadVersion != LatestSysVersion;
		MainMenuCanvas.enabled = true;
		MainMenuCanvasGroup.alpha = 0f;
		SetMenuShown();
		grStartTime = -GR_TIME;

		GetKeyDownJoin = new bool[(int)EGameButtonID.eButtonMax];
		GetKeyJoin = new bool[(int)EGameButtonID.eButtonMax];
		ResetTouchStatus();

		Application.targetFrameRate = 60;
	}

	/// <summary>
	/// Update は毎フレーム呼び出され、タイマー処理、入力処理、メニュー制御、ゲーム制御プロセス、データ更新を行います。
	/// </summary>
	private void Update()
	{
		timer.Process(Time.deltaTime);

		if (Input.GetKeyDown("m")) MenuShowToggle();

		if (KeyAvailable)
		{
			if (MenuShown)
			{
				if (Input.GetKeyDown(KeyCode.UpArrow)) MainMenuScr.OnGetKeyDown(EMenuButtonID.eScrUp);
				if (Input.GetKeyDown(KeyCode.LeftArrow)) MainMenuScr.OnGetKeyDown(EMenuButtonID.eScrLeft);
				if (Input.GetKeyDown(KeyCode.DownArrow)) MainMenuScr.OnGetKeyDown(EMenuButtonID.eScrDn);
				if (Input.GetKeyDown(KeyCode.RightArrow)) MainMenuScr.OnGetKeyDown(EMenuButtonID.eScrRight);
			}
			else
			{
				if (Input.GetKeyDown("1")) OnScreenTouch(EGameButtonID.e1Bet);
				if (Input.GetKey("1")) OnScreenHover(EGameButtonID.e1Bet);
				if (Input.GetKeyDown("3")) OnScreenTouch(EGameButtonID.eMaxBet);
				if (Input.GetKey("3")) OnScreenHover(EGameButtonID.eMaxBet);
				if (Input.GetKeyDown(KeyCode.UpArrow)) OnScreenTouch(EGameButtonID.eMaxBetAndStart);
				if (Input.GetKey(KeyCode.UpArrow)) OnScreenHover(EGameButtonID.eMaxBetAndStart);
				if (Input.GetKeyDown(KeyCode.LeftArrow)) OnScreenTouch(EGameButtonID.e1Reel);
				if (Input.GetKey(KeyCode.LeftArrow)) OnScreenHover(EGameButtonID.e1Reel);
				if (Input.GetKeyDown(KeyCode.DownArrow)) OnScreenTouch(EGameButtonID.e2Reel);
				if (Input.GetKey(KeyCode.DownArrow)) OnScreenHover(EGameButtonID.e2Reel);
				if (Input.GetKeyDown(KeyCode.RightArrow)) OnScreenTouch(EGameButtonID.e3Reel);
				if (Input.GetKey(KeyCode.RightArrow)) OnScreenHover(EGameButtonID.e3Reel);

				for (int i = 0; i < (int)EGameButtonID.eButtonMax; ++i)
				{
					if (GetKeyDownJoin[i]) controller.OnGetKeyDown((EGameButtonID)i);
					if (GetKeyJoin[i]) controller.OnGetKey((EGameButtonID)i);
				}
			}
		}

		float dt = CalcDT();
		MainMenuCanvasGroup.alpha = dt > GR_TIME ? (MenuShown ? 1f : 0f)
			: (MenuShown ? dt / GR_TIME : 1f - dt / GR_TIME);

		ResetTouchStatus();
		controller = controller.ProcessAfterInput(DataSaveAct, CheckAchievementAct);
		slotData.Process();
	}

	/// <summary>
	/// グラデーション時間の経過を取得します。
	/// </summary>
	/// <returns>前回グラデーション開始からの経過秒数。</returns>
	private float CalcDT() { return Time.time - grStartTime; }

	/// <summary>
	/// メニュー表示状態に応じてCanvasやタッチパネルの有効状態を切り替え、コールバック通知します。
	/// </summary>
	private void SetMenuShown()
	{
		grStartTime = Time.time;
		MainMenuTouch.enabled = MenuShown;
		TouchPanel.enabled = !MenuShown;
		MainMenuScr.OnMenuShownChange(MenuShown);
	}

	/// <summary>
	/// メニューの表示/非表示をトグルします。
	/// グラデーション時間経過中は切り替えを行いません。
	/// </summary>
	public void MenuShowToggle()
	{
		if (CalcDT() < GR_TIME) return;
		MenuShown ^= true;
		SetMenuShown();
	}

	/// <summary>
	/// メニューを強制的に非表示にします。
	/// </summary>
	public void MenuHide()
	{
		if (!MenuShown || CalcDT() < GR_TIME) return;
		MenuShown = false;
		SetMenuShown();
	}

	/// <summary>
	/// 画面タッチ操作を受け付け、ボタンIDを記録します。
	/// </summary>
	/// <param name="pID">タッチされたボタンのID。</param>
	public void OnScreenTouch(EGameButtonID pID) { GetKeyDownJoin[(int)pID] = true; }

	/// <summary>
	/// 画面ホバー操作を受け付け、ボタンIDを記録します。
	/// </summary>
	/// <param name="pID">ホバーされたボタンのID。</param>
	public void OnScreenHover(EGameButtonID pID) { GetKeyJoin[(int)pID] = true; }

	/// <summary>
	/// タッチ入力フラグをリセットします。
	/// </summary>
	private void ResetTouchStatus()
	{
		for (int i = 0; i < (int)EGameButtonID.eButtonMax; ++i)
		{
			GetKeyDownJoin[i] = false;
			GetKeyJoin[i] = false;
		}
	}

	/// <summary>
	/// Steam実績確認時のコールバック。
	/// システム変数を更新後、実績表示を行います。
	/// </summary>
	public void CheckAchievementAct()
	{
		slotData.UpdateSysVar();
		SteamAPI.OnGameStateChange();
	}

	/// <summary>
	/// セーブデータ保存時のコールバック。
	/// 通常・システム両データをファイルに書き出します。
	/// </summary>
	public void DataSaveAct()
	{
		slotData.SaveData(SavePath);
		slotData.SaveSysData(SaveSysPath);
	}

	/// <summary>
	/// MonoBehaviour無効化時に呼び出され、
	/// リセット時バックアップコピーおよびシステムデータ保存を行います。
	/// </summary>
	private void OnDisable()
	{
		if (slotData.sysData.ResetFlag)
			File.Copy(SavePath, BackupPath, true);
		slotData.SaveSysData(SaveSysPath);
	}

	/// <summary>
	/// リセット確認ダイアログで「続行」を選択した際に呼び出されます。
	/// ダイアログを閉じ、キー入力受付を有効にします。
	/// </summary>
	public void CautionStart()
	{
		CautionCanvas.enabled = false;
		CautionTouch.enabled = false;
		KeyAvailable = true;
	}
}
