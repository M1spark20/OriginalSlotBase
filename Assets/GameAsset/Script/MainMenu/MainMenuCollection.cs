using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// コレクション画面のメインメニュー要素を管理するクラスです。
/// 履歴表示と最新コレクション表示、達成数およびグラフ更新を行います。
/// </summary>
public class MainMenuCollection : MainMenuElemBase
{
	[SerializeField]
	private GameObject HistoryViewer;

	[SerializeField]
	private GameObject RecentViewer;

	[SerializeField]
	private TextMeshProUGUI AchieveCount;

	[SerializeField]
	private RectTransform AchieveGraph;

	private SlotEffectMaker2023.Data.CollectionData cd;
	private SlotEffectMaker2023.Action.CollectionLogger log;
	private UISmartScroller scroller;
	private UIShowPatternColleRecent recentScr;

	/// <summary>
	/// オブジェクト初期化時に呼ばれるメソッドです。
	/// シングルトンからデータとコンポーネントを取得します。
	/// </summary>
	protected override void Awake()
	{
		base.Awake();
		cd = SlotEffectMaker2023.Singleton.EffectDataManagerSingleton.GetInstance().Collection;
		log = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance().collectionManager;
		scroller = HistoryViewer?.GetComponent<UISmartScroller>();
		recentScr = RecentViewer.GetComponent<UIShowPatternColleRecent>();
	}

	/// <summary>
	/// 毎フレーム呼ばれる更新処理です。
	/// 現在の実装では未使用です。
	/// </summary>
	private void Update()
	{
		// 更新処理なし
	}

	/// <summary>
	/// コレクション画面の表示データを最新の状態にリフレッシュします。
	/// 履歴スクロールサイズ・要素更新、最新達成コレクション、達成数テキストおよびグラフを更新します。
	/// </summary>
	public override void RefreshData()
	{
		// サイズ取得
		int size = (cd.Collections.Count + 3) / 4;    // 4個ごとに行を分けるため、端数は切り上げ

		// スクロール領域設定と要素更新
		scroller.SetContentSize(size, 0);
		scroller.ElemUpdate(true);

		// 最近達成したデータ更新
		recentScr.RefreshData(0, false);

		// 達成数とグラフ更新
		int percent = log.GetAchievedCount() * 100 / cd.Collections.Count;
		AchieveCount.text = log.GetAchievedCount().ToString("D03") + " / " +
							cd.Collections.Count.ToString("D03") +
							" (" + percent.ToString("D03") + "%)";
		AchieveGraph.localScale = new Vector2(percent / 100f, 1f);
	}

	/// <summary>
	/// キー入力を受け付け、スクロールを上下に移動させます。
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
