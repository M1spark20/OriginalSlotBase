using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using TMPro;

/// <summary>
/// UI上の言語切替ボタンを管理し、選択中の言語に応じて色を変えるクラス
/// </summary>
public class UILanguageChanger : MonoBehaviour
{
	/// <summary>
	/// 言語切替用のボタン配列
	/// </summary>
	[SerializeField] private Button[] ChangerUI;

	/// <summary>
	/// ボタンに対応するImageコンポーネント配列
	/// </summary>
	private Image[] im;

	/// <summary>
	/// ボタンに表示されるTextMeshProUGUI配列
	/// </summary>
	private TextMeshProUGUI[] Text;

	// quoted from: https://anogame.net/unitypackage_localization/

	/// <summary>
	/// 初期化処理：ボタンからImageおよびTextを取得
	/// </summary>
	private void Awake()
	{
		im = new Image[ChangerUI.Length];
		Text = new TextMeshProUGUI[ChangerUI.Length];
		for (int i = 0; i < ChangerUI.Length; ++i)
		{
			im[i] = ChangerUI[i].GetComponent<Image>();
			Text[i] = ChangerUI[i].transform.Find("IDText").GetComponent<TextMeshProUGUI>();
		}
	}

	/// <summary>
	/// 毎フレームの更新処理：選択中のロケールと言語ボタン名を比較し、色を変更する
	/// </summary>
	private void Update()
	{
		string nowLocale = LocalizationSettings.SelectedLocale.Identifier.Code;
		for (int i = 0; i < ChangerUI.Length; ++i)
		{
			Color itemCol = ChangerUI[i].name == nowLocale ? Color.yellow : Color.white;
			im[i].color = itemCol;
			Text[i].color = itemCol;
		}
	}
}
