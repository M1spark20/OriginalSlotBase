using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// スロットが描画されない幅が広い場合に、スロットの端にUIデータを合わせて表示するクラス
/// </summary>
public class UIPlotter : MonoBehaviour
{
	/// <summary>ゲームの描画領域の幅</summary>
	[SerializeField, Min(0)] private float GameWidth;

	/// <summary>UI基準の横幅</summary>
	[SerializeField, Min(0)] private float TargetWidth;

	/// <summary>UI基準の縦幅</summary>
	[SerializeField, Min(0)] private float TargetHeight;

	/// <summary>左側に表示するかどうかのフラグ</summary>
	[SerializeField, Min(0)] private bool IsLeftPanel;

	private RectTransform rect;
	private Canvas myCanvas;
	private SlotEffectMaker2023.Action.SystemData sys;

	/// <summary>各表示位置に対応した可視状態フラグ</summary>
	[SerializeField] bool[] visible;

	/// <summary>各表示位置に対応したY方向アンカー値</summary>
	[SerializeField] float[] anchorY;

	/// <summary>現在のポジションID（表示位置）</summary>
	public int posID;

	/// <summary>
	/// 初期化処理：RectTransform、Canvas、およびシステムデータの参照を取得
	/// </summary>
	void Start()
	{
		rect = GetComponent<RectTransform>();
		myCanvas = GetComponent<Canvas>();
		sys = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance().sysData;
	}

	/// <summary>
	/// 毎フレームUIの位置と表示状態を更新する処理
	/// </summary>
	void Update()
	{
		int posID = sys.InfoPos;
		if (posID >= anchorY.Length || posID >= visible.Length) return;

		myCanvas.enabled = visible[posID];

		// 拡大後の自身の幅を計算
		float UIext = Screen.width / TargetWidth;
		float myWidth = rect.sizeDelta.x * UIext;

		// 拡大後のゲーム描画幅を計算
		float Gameext = Screen.height / TargetHeight;
		float bgWidth = (Screen.width - GameWidth * Gameext) / 2f;

		// 移動幅を計算、移動量はUIに関係するためUIextで割る
		float ansX = Mathf.Max(0f, (bgWidth - myWidth) / UIext);
		if (!IsLeftPanel) ansX *= -1f;

		// 表示位置調整
		rect.anchorMin = new Vector2(rect.anchorMin.x, anchorY[posID]);
		rect.anchorMax = new Vector2(rect.anchorMax.x, anchorY[posID]);
		rect.pivot = new Vector2(rect.pivot.x, anchorY[posID]);
		rect.anchoredPosition = new Vector2(ansX, 0);
	}
}
