using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// WaitCutオプションの切り替えUIを制御するコンポーネント。
/// </summary>
public class UIWaitCutSet : MonoBehaviour
{
	/// <summary>
	/// 切り替え用のボタンUI配列。
	/// </summary>
	[SerializeField] private Button[] ChangerUI;

	private Image[] im;
	private TextMeshProUGUI[] Text;
	private SlotEffectMaker2023.Action.SystemData sys;

	/// <summary>
	/// Start は最初のフレーム更新前に一度だけ呼び出され、UI初期化と表示状態の反映を行います。
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
		ChangeWait(sys.WaitCut);
	}

	/// <summary>
	/// WaitCut機能の有効/無効を切り替え、UIのカラーを更新します。
	/// </summary>
	/// <param name="enabled">WaitCutを有効にする場合はtrue、それ以外はfalse。</param>
	public void ChangeWait(bool enabled)
	{
		sys.WaitCut = enabled;
		for (int i = 0; i < ChangerUI.Length; ++i)
		{
			// 選択中は黄色、それ以外は白色で表示
			Color itemCol = sys.WaitCut ^ (i == 0) ? Color.yellow : Color.white;
			im[i].color = itemCol;
			Text[i].color = itemCol;
		}
	}
}
