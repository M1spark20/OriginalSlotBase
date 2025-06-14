using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 変数とタイマの状況に応じてSpriteRendererの表示/非表示および点滅制御を行うコンポーネント。
/// </summary>
public class SimpleRampController : MonoBehaviour
{
	/// <summary>
	/// SlotDataSingletonのインスタンス参照。
	/// </summary>
	private SlotEffectMaker2023.Singleton.SlotDataSingleton slotData;    // スロット基本情報

	/// <summary>
	/// 条件判定を行う変数名。指定なし（空文字）で変数判定をスキップ。
	/// </summary>
	[SerializeField] private string VariableName;    // 条件判定を行う変数名。指定なしで判定しない
	/// <summary>
	/// 範囲判定用の値A。
	/// </summary>
	[SerializeField] private int RangeA;          // 条件判定値A
	/// <summary>
	/// 範囲判定用の値B。
	/// </summary>
	[SerializeField] private int RangeB;          // 条件判定値B
	/// <summary>
	/// 範囲判定に等号を含める場合はtrue。
	/// </summary>
	[SerializeField] private bool EqualFlag;       // 判定条件に等号を含むか
	/// <summary>
	/// 変数条件を満たすときに表示を反転させる場合はtrue。
	/// </summary>
	[SerializeField] private bool VarInvert;       // 条件を満たすときに表示するか(true: 表示しない)

	/// <summary>
	/// 時間判定を行うタイマ名。指定なし（空文字）で時間判定をスキップ。
	/// </summary>
	[SerializeField] private string TimerName;       // 時間判定を行うタイマ名。指定なしで判定しない
	/// <summary>
	/// 時間下限値（秒）。
	/// </summary>
	[SerializeField] private float TimeBegin;       // 時間下限値
	/// <summary>
	/// 時間条件を満たすときに表示を反転させる場合はtrue。
	/// </summary>
	[SerializeField] private bool TimeInvert;      // 条件を満たすときに表示するか(true: 表示しない)

	/// <summary>
	/// 点滅可能回数。負数で無制限。
	/// </summary>
	[SerializeField] private int BlinkCount;      // 点滅回数
	/// <summary>
	/// 点滅周期（秒）。
	/// </summary>
	[SerializeField] private float BlinkCycle;      // 点滅周期

	/// <summary>
	/// Start は初期化時に一度だけ呼び出され、シングルトンインスタンスを取得します。
	/// </summary>
	private void Start()
	{
		slotData = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance();
	}

	/// <summary>
	/// Update は毎フレーム呼び出され、条件に応じて表示/非表示や点滅制御を行います。
	/// </summary>
	private void Update()
	{
		bool condVar = true;
		bool condTimer = true;

		// 変数条件の検証
		if (VariableName != string.Empty)
		{
			int min = Math.Min(RangeA, RangeB);
			int max = Math.Max(RangeA, RangeB);
			bool activated = slotData.valManager.GetVariable(VariableName)?.CheckRange(min, max, EqualFlag) == true;
			condVar = activated ^ VarInvert;
		}

		// タイマ条件の検証
		if (TimerName != string.Empty)
		{
			var elem = slotData.timerData.GetTimer(TimerName);
			if (elem == null) condTimer = false;
			else if (!elem.isActivate) condTimer = false;
			else condTimer = (elem.elapsedTime > TimeBegin) ^ TimeInvert;

			// タイマが無効なら表示を常に行う
			if (!condTimer)
			{
				GetComponent<SpriteRenderer>().enabled = true;
				return;
			}
		}

		// 既定は表示OFF
		var renderer = GetComponent<SpriteRenderer>();
		renderer.enabled = true;
		if (condVar && condTimer)
		{
			renderer.enabled = false;
			// 点滅処理
			if (BlinkCycle <= 0f || BlinkCount == 0) return;
			if (TimerName != string.Empty)
			{
				var elem = slotData.timerData.GetTimer(TimerName);
				if (elem == null || !elem.isActivate) return;
				var elapsed = elem.elapsedTime - TimeBegin;
				if (elapsed % BlinkCycle > BlinkCycle / 2f && (BlinkCount < 0 || (int)(elapsed / BlinkCycle) < BlinkCount))
					renderer.enabled = true;
			}
		}
	}
}