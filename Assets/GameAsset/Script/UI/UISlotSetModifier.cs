using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// スロット設定ボタンを動的に生成し、設定変更および表示を制御するUIコンポーネント。
/// </summary>
public class UISlotSetModifier : MonoBehaviour
{
	/// <summary>
	/// 基本となるボタンプレハブ。
	/// </summary>
	[SerializeField] private Button BaseButton;
	/// <summary>
	/// ボタンの配置間隔（X方向オフセット）。
	/// </summary>
	[SerializeField] private Vector2 Interval;
	/// <summary>
	/// 各設定に対応する支払情報をカンマ区切りで指定。
	/// </summary>
	[SerializeField, Multiline] private string PayInfo;
	/// <summary>
	/// 支払情報を表示するTextMeshProUGUIコンポーネント。
	/// </summary>
	[SerializeField] private TextMeshProUGUI PayInfoShow;

	/// <summary>
	/// ランダム設定時に表示するボタンの親オブジェクト。
	/// </summary>
	[SerializeField] private GameObject RandAnsBtnShow;
	/// <summary>
	/// ランダム設定の回答を表示するTextMeshProUGUIコンポーネント。
	/// </summary>
	[SerializeField] private TextMeshProUGUI RandomAnswer;

	private Button[] selector;
	private Image[] im;
	private TextMeshProUGUI[] tx;
	private Canvas ansBtnC;
	private GraphicRaycaster ansBtnR;

	private SlotEffectMaker2023.Action.SlotBasicData bs;
	private string[] infoArray;

	private byte btnIndex;
	private const byte sz = (byte)SlotMaker2022.LocalDataSet.SETTING_MAX;

	/// <summary>
	/// Start は最初のフレーム更新前に一度だけ呼び出され、
	/// ボタンの生成・初期配置およびデータ初期化を行います。
	/// </summary>
	private void Start()
	{
		selector = new Button[sz + 1];
		im = new Image[sz + 1];
		tx = new TextMeshProUGUI[sz + 1];

		bs = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance().basicData;
		infoArray = PayInfo.Split(",");

		// Instantiateする
		for (int i = 0; i < sz + 1; ++i)
		{
			selector[i] = i == 0 ? BaseButton : Instantiate(BaseButton, this.transform);
			im[i] = selector[i].GetComponent<Image>();
			tx[i] = selector[i].transform.Find("IDText").GetComponent<TextMeshProUGUI>();

			selector[i].GetComponent<RectTransform>()
				.anchoredPosition += new Vector2(Interval.x * i, 0);
			tx[i].text = i == sz ? "?" : (i + 1).ToString();

			// ボタン押下時のスクリプト登録
			byte prm = (byte)i;    // 変数に入れないとラムダがうまく動かない
			selector[i].onClick.AddListener(() => OnClickButton(prm));
		}

		// ボタン位置初期設定
		btnIndex = bs.setRandom ? (byte)sz : bs.slotSetting;
	}

	/// <summary>
	/// Update は毎フレーム呼び出され、選択中ボタンのハイライト表示と
	/// 支払情報またはランダム回答ボタンの表示切り替えを行います。
	/// </summary>
	private void Update()
	{
		for (int i = 0; i < selector.Length; ++i)
		{
			Color cl = btnIndex == i ? Color.yellow : Color.white;
			im[i].color = cl;
			tx[i].color = cl;
		}
		if (btnIndex < sz)
		{
			PayInfoShow.enabled = true;
			PayInfoShow.text = infoArray[bs.slotSetting];
			RandAnsBtnShow.SetActive(false);
		}
		else
		{
			PayInfoShow.enabled = false;
			RandAnsBtnShow.SetActive(true);
		}
	}

	/// <summary>
	/// ボタンがクリックされた際に呼び出され、設定を変更します。
	/// </summary>
	/// <param name="index">クリックされたボタンのインデックス（0～SETTING_MAX まで）。</param>
	public void OnClickButton(byte index)
	{
		if (index == btnIndex) return;  // 変更なしなら何もしない
		btnIndex = index;
		bool rand = index >= sz;
		if (rand) index = (byte)Random.Range(0, sz);
		bs.ChangeSlotSetting(index, rand);
	}

	/// <summary>
	/// ランダム設定時の回答表示を更新します。
	/// </summary>
	public void CheckRandAnswer()
	{
		RandomAnswer.text = "Answer: " + (bs.slotSetting + 1).ToString();
	}
}