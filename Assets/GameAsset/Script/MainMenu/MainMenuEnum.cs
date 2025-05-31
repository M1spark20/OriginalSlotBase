using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// メインメニューのボタン操作を識別する列挙型です。
/// スクロールや各種入力に対応するIDを定義しています。
/// </summary>
public enum EMenuButtonID
{
	/// <summary>左方向へのスクロール入力</summary>
	eScrLeft,

	/// <summary>右方向へのスクロール入力</summary>
	eScrRight,

	/// <summary>上方向へのスクロール入力</summary>
	eScrUp,

	/// <summary>下方向へのスクロール入力</summary>
	eScrDn,

	/// <summary>ボタンの最大数（未使用）</summary>
	eButtonMax
}