using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 複数の画像スプライトを用いて数値を表示するための基底クラス。
/// 変数の値を取得し、各桁ごとにテクスチャインデックスを算出してスプライトを更新します。
/// </summary>
public abstract class ShowNumBase : IMultiImageWriter
{
	/// <summary>
	/// 表示する変数のキー名。
	/// </summary>
	[SerializeField] protected string DispVariable;
	/// <summary>
	/// 表示する桁数。
	/// </summary>
	[SerializeField, Min(1)] protected int ShowDigit;
	/// <summary>
	/// 時間判定を行うタイマ名。指定なしで判定しない。
	/// </summary>
	[SerializeField] string TimerName;        // 時間判定を行うタイマ名。指定なしで判定しない
	/// <summary>
	/// 時間の下限値（秒）。
	/// </summary>
	[SerializeField] float TimeBegin;        // 時間下限値
	/// <summary>
	/// 条件を満たすときに表示しない場合はtrue。
	/// </summary>
	[SerializeField] bool TimeInvert;       // 条件を満たすときに表示するか(true: 表示しない)

	/// <summary>
	/// 指定の数値と桁位置から使用するテクスチャインデックスを算出します。
	/// </summary>
	/// <param name="val">対象の数値。</param>
	/// <param name="getDigit">取得する桁位置（0ベース）。</param>
	/// <returns>スプライトインデックス。表示しない場合はnull。</returns>
	abstract protected int? GetTextureIndex(int val, uint getDigit);

	/// <summary>
	/// 毎フレーム呼び出され、各桁のスプライトを更新します。
	/// </summary>
	protected override void Update()
	{
		// 表示させる値を取得する
		var varData = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance().valManager;
		int? showVal = varData.GetVariable(DispVariable)?.val;
		if (!showVal.HasValue) return;

		for (uint i = 0; i < ShowDigit; ++i)
		{
			int? spID = GetTextureIndex((int)showVal, i);
			SpriteRenderer sp = mComaInstance[i].GetComponent<SpriteRenderer>();
			sp.enabled = spID.HasValue && CheckTimer();
			if (spID.HasValue) sp.sprite = mImageBuilder.Extract((int)spID);
		}
	}

	/// <summary>
	/// タイマの状態に応じて表示可否を判定します。
	/// </summary>
	/// <returns>表示可能であればtrue。ただしTimeInvertフラグにより反転する。</returns>
	bool CheckTimer()
	{
		// 時間点灯条件判定
		if (TimerName == string.Empty) return true;

		var slotData = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance();
		bool activated = true;
		var elem = slotData.timerData.GetTimer(TimerName);
		if (elem == null) activated = false;
		else
		{
			if (!elem.isActivate) activated = false;        // タイマが無効な場合無効判定
			else activated &= elem.elapsedTime > TimeBegin; // 指定時間を超過しているか
		}
		return activated ^ TimeInvert;
	}
}
