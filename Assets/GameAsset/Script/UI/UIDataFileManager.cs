using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// セーブデータのスロット切り替えとリセットUIを管理するクラス
/// </summary>
public class UIDataFileManager : MonoBehaviour
{
	/// <summary>セーブデータスロットのベースボタン</summary>
	[SerializeField] private Button FileButtonBase;

	/// <summary>スロットボタンの間隔</summary>
	[SerializeField] private Vector2 Interval;

	/// <summary>セーブデータ切替があったときの表示テキスト</summary>
	[SerializeField] private TextMeshProUGUI FileChangeInfo;

	/// <summary>リセットボタンのUI配列</summary>
	[SerializeField] private Button[] ResetBtnUI;

	/// <summary>Raycasterの参照（UI有効判定に使用）</summary>
	[SerializeField] private GraphicRaycaster RefRaycaster;

	/// <summary>リセット説明表示</summary>
	[SerializeField] private TextMeshProUGUI Explanation;

	private const int SAVEFILE_NUM = 10;
	private Button[] BtnsFile;
	private Image[] ImFile;
	private TextMeshProUGUI[] TextFile;

	private Image[] ImReset;
	private TextMeshProUGUI[] TextReset;
	private SlotEffectMaker2023.Action.SystemData sys;
	private byte defaultSaveID;

	/// <summary>
	/// 初期化処理。セーブスロットボタンとリセットボタンを生成し、初期状態を設定します。
	/// </summary>
	void Start()
	{
		BtnsFile = new Button[SAVEFILE_NUM];
		ImFile = new Image[SAVEFILE_NUM];
		TextFile = new TextMeshProUGUI[SAVEFILE_NUM];

		// Instantiateする
		for (int i = 0; i < SAVEFILE_NUM; ++i)
		{
			BtnsFile[i] = i == 0 ? FileButtonBase : Instantiate(FileButtonBase, this.transform);
			ImFile[i] = BtnsFile[i].GetComponent<Image>();
			TextFile[i] = BtnsFile[i].transform.Find("IDText").GetComponent<TextMeshProUGUI>();

			BtnsFile[i].GetComponent<RectTransform>().anchoredPosition += new Vector2(Interval.x * i, 0);
			TextFile[i].text = (i + 1).ToString();

			// ボタン押下時のスクリプト登録
			byte prm = (byte)i; // 変数に入れないとラムダ式内で正しく渡らないため
			BtnsFile[i].onClick.AddListener(() => ChangeFileID(prm));
		}

		ImReset = new Image[ResetBtnUI.Length];
		TextReset = new TextMeshProUGUI[ResetBtnUI.Length];

		for (int i = 0; i < ResetBtnUI.Length; ++i)
		{
			ImReset[i] = ResetBtnUI[i].GetComponent<Image>();
			TextReset[i] = ResetBtnUI[i].transform.Find("IDText").GetComponent<TextMeshProUGUI>();
		}

		sys = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance().sysData;
		ChangeShow(sys.ResetFlag);
		defaultSaveID = sys.UseSaveDataID;
	}

	/// <summary>
	/// 毎フレーム処理。UIの有効状態やスロットボタンのインタラクション状態を監視・更新します。
	/// </summary>
	void Update()
	{
		// 画面が有効かを常時監視する
		if (RefRaycaster != null)
			sys.ResetFlag &= RefRaycaster.enabled;

		if (!RefRaycaster.enabled)
			sys.UseSaveDataID = defaultSaveID;

		ChangeShow(sys.ResetFlag);

		// File描画(Reset有効時はボタンをクリックさせない)
		if (sys.ResetFlag)
			ChangeFileID(defaultSaveID);

		for (int i = 0; i < BtnsFile.Length; ++i)
		{
			BtnsFile[i].interactable = !sys.ResetFlag;
			Color cl = sys.UseSaveDataID == i ? Color.yellow : Color.white;
			ImFile[i].color = cl;
			TextFile[i].color = cl;
		}

		FileChangeInfo.enabled = (sys.UseSaveDataID != defaultSaveID);
	}

	/// <summary>
	/// 現在使用中のセーブデータIDを変更します
	/// </summary>
	/// <param name="changeFor">変更先のセーブID（0〜9）</param>
	public void ChangeFileID(byte changeFor)
	{
		sys.UseSaveDataID = changeFor;
	}

	/// <summary>
	/// リセットボタンの状態と説明の表示を切り替えます
	/// </summary>
	/// <param name="enabled">true でリセットモード、false で通常モード</param>
	public void ChangeShow(bool enabled)
	{
		sys.ResetFlag = enabled;

		for (int i = 0; i < ResetBtnUI.Length; ++i)
		{
			Color itemCol = Color.white;
			if (i == 0 && !sys.ResetFlag) itemCol = Color.yellow;
			if (i == 1 && sys.ResetFlag) itemCol = Color.red;

			ImReset[i].color = itemCol;
			TextReset[i].color = itemCol;
		}

		Explanation.enabled = sys.ResetFlag;
	}
}
