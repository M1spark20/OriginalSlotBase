using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// 設定メニュー画面でスクロール操作を提供する要素クラスです。
/// ScrollRect コンポーネントを利用し、上下キー入力で表示領域を移動します。
/// </summary>
public class MainMenuSettings : MainMenuElemBase
{
	[SerializeField]
	private ScrollRect Viewer;

	[SerializeField]
	private float moveSize;

	private float ViewerHeight;
	private float ScrollHeight;

	// Start is called before the first frame update
	/// <summary>
	/// オブジェクト生成時の初期化処理を行います。
	/// ScrollRect のコンテンツ高さと表示領域高さからスクロール可能範囲を計算します。
	/// </summary>
	protected override void Awake()
	{
		base.Awake();
		ViewerHeight = Viewer.transform.Find("Viewport/Content").GetComponent<RectTransform>().sizeDelta.y;
		ScrollHeight = ViewerHeight - GetComponent<RectTransform>().sizeDelta.y;
	}

	/// <summary>
	/// メニュー要素の表示データを更新します。
	/// 設定メニューには特別な更新処理はなく、空実装とします。
	/// </summary>
	public override void RefreshData()
	{
	}

	/// <summary>
	/// メニュー操作キー入力時の処理を行います。
	/// ScrollHeight が正の場合のみ、上下キーでスクロール位置を移動します。
	/// </summary>
	/// <param name="eKeyID">押下されたメニュー操作ボタンの識別子</param>
	public override void OnGetKeyDown(EMenuButtonID eKeyID)
	{
		if (ScrollHeight < 0f) return;
		float moveSizeStd = moveSize / ScrollHeight;
		float pos = 1f - Viewer.verticalNormalizedPosition;

		if (eKeyID == EMenuButtonID.eScrUp)
		{
			pos -= moveSizeStd;
		}
		else if (eKeyID == EMenuButtonID.eScrDn)
		{
			pos += moveSizeStd;
		}

		// 調整
		if (pos < 0f) pos = 0f;
		if (pos > 1f) pos = 1f;
		Viewer.verticalNormalizedPosition = 1f - pos;
	}
}