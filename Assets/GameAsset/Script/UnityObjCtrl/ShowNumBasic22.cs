using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ShowNumBaseを継承し、符号付き数値を11x2の22分割スプライトで表示する実装クラス。
/// 負数の場合は特定のインデックス(21または10)を返し、正数は0～9のインデックスを返します。
/// </summary>
public class ShowNumBasic22 : ShowNumBase
{
	// 実装すべき関数の未実装
	/// <summary>
	/// 各桁のスプライト分割や配置に必要なパラメータを初期化します。
	/// DivX=11, DivY=2, 切り出し方向はX方向とし、表示開始位置や重複量を設定します。
	/// </summary>
	protected override void InitDivision()
	{
		DivX = 11;
		DivY = 2;
		CutWayX = true;
		ShowX = -ShowDigit;
		ShowY = 1;
		OverlapX = 0f;
		OverlapY = 0f;
	}

	/// <summary>
	/// 指定された数値と桁位置から使用するスプライトインデックスを算出します。
	/// 負数の場合はマイナス記号用インデックスを返し、該当桁に表示すべき値がない場合はnullを返します。
	/// </summary>
	/// <param name="val">対象の数値。</param>
	/// <param name="getDigit">取得する桁位置（0が最下位桁）。</param>
	/// <returns>
	/// スプライトインデックス(0～9: 数字, 10: プラス符号, 21: マイナス符号)。
	/// 表示しない場合はnull。
	/// </returns>
	protected override int? GetTextureIndex(int val, uint getDigit)
	{
		int digit = Mathf.Abs(val);
		int lastVal = digit;
		int baseDig = val < 0 ? 11 : 0;

		for (int i = 0; i < getDigit; ++i)
		{
			lastVal = digit;
			digit /= 10;
		}
		// 最上位ケタは数値がマイナスならマイナス記号を表示
		if (((digit == 0 && getDigit > 0) || getDigit + 1 == ShowDigit) && lastVal > 0)
			return val < 0 ? 21 : 10;
		// 表示しないケタはnullを返す
		if (digit == 0 && getDigit > 0 && lastVal == 0)
			return null;

		return (digit % 10) + baseDig;
	}
}
