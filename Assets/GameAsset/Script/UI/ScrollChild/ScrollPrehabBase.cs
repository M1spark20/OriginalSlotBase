using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// スクロール用プレハブのベースクラスです。
/// UISmartScrollerとの連携や表示制御を行います。
/// </summary>
public class ScrollPrehabBase : MonoBehaviour
{
	private UISmartScroller scr;
	private Canvas canvas;
	private GraphicRaycaster touch;
	private bool touchable;

	/// <summary>
	/// プレハブが選択されているかどうかを示します。
	/// </summary>
	public bool Selected { get; private set; }

	// Start is called before the first frame update
	// プレハブ起動時に一度だけ呼び出される初期化処理
	/// <summary>
	/// 初期化時にUISmartScroller、Canvas、GraphicRaycasterを取得し、選択状態とタッチ可能フラグをリセットします。
	/// </summary>
	virtual protected void Awake()
	{
		scr = transform.parent.transform.parent.transform.parent.GetComponent<UISmartScroller>();
		canvas = GetComponent<Canvas>();
		touch = GetComponent<GraphicRaycaster>();
		Selected = false;
		touchable = false;
	}

	// Update is called once per frame
	// 毎フレーム呼び出される処理
	/// <summary>
	/// フレームごとにタッチ可能状態に応じてGraphicRaycasterを有効/無効にします。
	/// </summary>
	virtual protected void Update()
	{
		if (touch != null) touch.enabled = canvas.enabled && touchable;
	}

	/// <summary>
	/// プレハブの表示内容を更新します。
	/// </summary>
	/// <param name="pID">アイテムの識別子</param>
	/// <param name="pIsSelected">現在選択中かどうか</param>
	virtual public void RefreshData(int pID, bool pIsSelected)
	{
		Selected = pIsSelected;
	}

	/// <summary>
	/// 自分自身をUISmartScrollerの選択対象として登録します。
	/// </summary>
	protected void SelectMe()
	{
		scr.SetSelected(int.Parse(name));
	}

	/// <summary>
	/// Canvasの表示/非表示を切り替えます。
	/// </summary>
	/// <param name="visible">表示する場合は true、非表示にする場合は false</param>
	public void SetVisible(bool visible)
	{
		canvas.enabled = visible;
	}

	/// <summary>
	/// タッチ入力の受け付け可否を設定します。
	/// </summary>
	/// <param name="pTouchable">タッチ可能にする場合は true、不可にする場合は false</param>
	public void SetRaycaster(bool pTouchable)
	{
		touchable = pTouchable;
	}
}
