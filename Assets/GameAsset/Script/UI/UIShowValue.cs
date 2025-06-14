using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// 変数の値をTextMeshProUGUIで表示し、条件に応じて点滅させるUIコンポーネント。
/// </summary>
public class UIShowValue : MonoBehaviour
{
	/// <summary>
	/// 表示するデータの名前ラベル。
	/// </summary>
	[SerializeField] protected string DataName;
	/// <summary>
	/// 取得する変数のキー名。
	/// </summary>
	[SerializeField] protected string DispVariable;
	/// <summary>
	/// 小数表示桁数（小数点以下n桁まで表示）。
	/// </summary>
	[SerializeField, Min(0)] protected int ShowDigitRound;    // showDigitからnケタ分を小数にする
	/// <summary>
	/// 正負の符号を表示するかどうか。
	/// </summary>
	[SerializeField] protected bool ShowPlusMinus;
	/// <summary>
	/// 値の末尾に付与するサフィックス文字列。
	/// </summary>
	[SerializeField] protected string Suffix;
	/// <summary>
	/// 点滅の判定に用いる変数のキー名。
	/// </summary>
	[SerializeField] protected string BlinkCondVar;
	/// <summary>
	/// 点滅判定の最小値。
	/// </summary>
	[SerializeField] protected int BlinkCondMin;
	/// <summary>
	/// 点滅判定の最大値。
	/// </summary>
	[SerializeField] protected int BlinkCondMax;
	/// <summary>
	/// 点滅サイクルの周期（秒）。
	/// </summary>
	[SerializeField] protected float BlinkCycle;

	private TextMeshProUGUI Title;
	private TextMeshProUGUI Value;
	private Color red;

	/// <summary>
	/// Start は初期化処理を行い、TitleおよびValueコンポーネントを取得してDataNameを設定します。
	/// </summary>
	private void Start()
	{
		Title = transform.Find("Title").GetComponent<TextMeshProUGUI>();
		Value = transform.Find("Value").GetComponent<TextMeshProUGUI>();
		Title.text = DataName;
		red = new Color(1f, 0.3f, 0.3f);
	}

	/// <summary>
	/// Update は毎フレーム呼び出され、指定変数の値を取得して表示テキストを更新し、
	/// BlinkCondVarの値に応じてテキストを点滅させます。
	/// </summary>
	private void Update()
	{
		Title.text = DataName;
		Value.text = string.Empty;
		Value.color = Color.white;

		var varData = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance().valManager;
		int? showVal = varData.GetVariable(DispVariable)?.val;
		if (!showVal.HasValue) return;

		if (ShowPlusMinus)
		{
			if ((int)showVal > 0) Value.text += "+";
			if ((int)showVal < 0) Value.color = red;
		}

		float sh = (float)showVal / Mathf.Pow(10f, ShowDigitRound);
		Value.text += sh.ToString("F" + ShowDigitRound.ToString()) + Suffix;

		// 点滅条件
		int? condVal = varData.GetVariable(BlinkCondVar)?.val;
		if (!condVal.HasValue) return;
		if ((int)condVal >= BlinkCondMin && (int)condVal <= BlinkCondMax)
		{
			var tm = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance().timerData.GetTimer("general");
			if (tm != null)
			{
				if (tm.elapsedTime % BlinkCycle > BlinkCycle / 2f) Value.text = string.Empty;
			}
		}
	}
}