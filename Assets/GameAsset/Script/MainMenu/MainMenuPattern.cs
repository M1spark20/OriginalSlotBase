using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// ボーナス履歴のリールパターン一覧を表示するメインメニュー要素です。
/// UISmartScroller を使用して履歴データを2コマ毎にスクロール表示します。
/// </summary>
public class MainMenuPattern : MainMenuElemBase
{
	[SerializeField]
	private GameObject HistoryViewer;

	private SlotEffectMaker2023.Action.HistoryManager hm;
	private UISmartScroller scroller;

	/// <summary>
	/// オブジェクト生成時の初期化処理です。
	/// シングルトンから履歴マネージャを取得し、UISmartScroller コンポーネントをセットアップします。
	/// </summary>
	protected override void Awake()
	{
		base.Awake();
		hm = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance().historyManager;
		scroller = HistoryViewer?.GetComponent<UISmartScroller>();
	}

	/// <summary>
	/// 毎フレーム呼び出されます。
	/// 特に処理を行わない場合は空実装で問題ありません。
	/// </summary>
	private void Update()
	{
	}

	/// <summary>
	/// 要素の表示データを最新状態に更新します。
	/// 履歴データのパターン数に応じてスクロール領域を設定し、要素全更新を実行します。
	/// </summary>
	public override void RefreshData()
	{
		// サイズ取得
		int size = (hm.PatternHist.Count + 1) / 2;  // 2コ毎に更新するためsizeを2で割る、端数切り上げ
													// サイズ指定とIndex更新
		scroller.SetContentSize(size, 0);
		// データ全更新
		scroller.ElemUpdate(true);
	}

	/// <summary>
	/// メニュー操作キー入力時の処理を実装します。
	/// 上下キーでスクロール位置を移動します。
	/// </summary>
	/// <param name="eKeyID">押下されたメニュー操作ボタンの識別子</param>
	public override void OnGetKeyDown(EMenuButtonID eKeyID)
	{
		if (eKeyID == EMenuButtonID.eScrUp)
		{
			scroller.MovePosition(-0.5f);
		}
		else if (eKeyID == EMenuButtonID.eScrDn)
		{
			scroller.MovePosition(0.5f);
		}
	}
}
