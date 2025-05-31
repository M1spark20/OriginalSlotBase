using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// SlipCount表示の切り替えUIを制御するコンポーネント。
/// </summary>
public class UIShowSlipCountSet : MonoBehaviour
{
	/// <summary>
	/// 切り替え用のボタンUI配列。
	/// </summary>
	[SerializeField] private Button[] ChangerUI;

	private Image[] im;
	private TextMeshProUGUI[] Text;
	private SlotEffectMaker2023.Action.SystemData sys;

	/// <summary>
	/// Start は最初のフレーム更新前に一度呼び出され、UIの初期化を行います。
	/// </summary>
	private void Start()
	{
		im = new Image[ChangerUI.Length];
		Text = new TextMeshProUGUI[ChangerUI.Length];
		for (int i = 0; i < ChangerUI.Length; ++i)
		{
			im[i] = ChangerUI[i].GetComponent<Image>();
			Text[i] = ChangerUI[i].transform.Find("IDText").GetComponent<TextMeshProUGUI>();
		}

		sys = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance().sysData;
		ChangeShow(sys.ShowSlipCount);
	}

	/// <summary>
	/// SlipCountの表示状態を変更し、UIのカラーを更新します。
	/// </summary>
	/// <param name="enabled">SlipCount表示を有効にするかどうか</param>
	public void ChangeShow(bool enabled)
	{
		sys.ShowSlipCount = enabled;
		for (int i = 0; i < ChangerUI.Length; ++i)
		{
			// 選択中のアイテムは黄色、それ以外は白色に設定
			Color itemCol = sys.ShowSlipCount ^ (i == 0) ? Color.yellow : Color.white;
			im[i].color = itemCol;
			Text[i].color = itemCol;
		}
	}
}