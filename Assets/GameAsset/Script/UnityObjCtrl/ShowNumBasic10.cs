using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ShowNumBaseを継承し、十進数（ベース10）での数値表示を行う実装クラス。
/// </summary>
public class ShowNumBasic10 : ShowNumBase
{
	// 実装すべき関数の未実装
	/// <summary>
	/// 各桁のスプライト分割や配置に必要なパラメータを初期化します。
	/// </summary>
	protected override void InitDivision()
	{
		DivX = 10;
		DivY = 1;
		CutWayX = true;
		ShowX = -ShowDigit;
		ShowY = 1;
		OverlapX = 0f;
		OverlapY = 0f;
	}

	/// <summary>
	/// 指定された数値の桁位置に対応するスプライトインデックスを算出します。
	/// </summary>
	/// <param name="val">対象の数値。</param>
	/// <param name="getDigit">取得する桁（0が最下位桁）。</param>
	/// <returns>0～9のインデックス。表示しない場合はnull。</returns>
	protected override int? GetTextureIndex(int val, uint getDigit)
	{
		int digit = val;
		if (val < 0) return null;
		for (int i = 0; i < getDigit; ++i) digit /= 10;
		if (digit == 0 && getDigit > 0) return null;
		return digit % 10;
	}
}