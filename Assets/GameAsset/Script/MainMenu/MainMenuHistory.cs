using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Localization;
using System;

/// <summary>
/// コレクション履歴画面のメインメニュー要素を管理するクラスです。
/// ボーナス履歴一覧のスクロール制御、成立パターン表示、日付・契機・演出のローカライズ表示、グラフ更新を行います。
/// </summary>
public class MainMenuHistory : MainMenuElemBase
{
	[SerializeField]
	private GameObject PatternElem;

	[SerializeField]
	private GameObject HistoryViewer;

	[SerializeField]
	private GameObject Date;

	[SerializeField]
	private UIBalanceGraph GraphScript;

	[SerializeField]
	private string LocalizeTableID;

	[SerializeField]
	private string LocalizeBroughtID;

	[SerializeField]
	private string LocalizeEffectID;

	[SerializeField]
	private TextMeshProUGUI BroughtText;

	[SerializeField]
	private TextMeshProUGUI EffectText;

	private SlotEffectMaker2023.Action.HistoryManager hm;
	private ReelPatternBuilder builder;
	private UISmartScroller scroller;
	private TextMeshProUGUI dateShow;
	private int lastShow;
	private GetDynamicLocalText LocalizeGet;

	/// <summary>
	/// オブジェクト生成時に呼び出される初期化メソッドです。
	/// シングルトンから履歴データとUI要素を取得します。
	/// </summary>
	protected override void Awake()
	{
		base.Awake();
		hm = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance().historyManager;
		builder = PatternElem?.GetComponent<ReelPatternBuilder>();
		scroller = HistoryViewer?.GetComponent<UISmartScroller>();
		dateShow = Date?.GetComponent<TextMeshProUGUI>();
		lastShow = int.MinValue;
		LocalizeGet = GetComponent<GetDynamicLocalText>();
	}

	/// <summary>
	/// 毎フレーム呼び出される更新メソッドです。
	/// スクロール選択位置が変わったらパターン表示を更新します。
	/// </summary>
	private void Update()
	{
		if (hm.BonusHist.Count == 0 || scroller.ContentCount <= 0) return;
		int nowShow = scroller.SelectedIndex;
		if (nowShow != lastShow)
		{
			ShowPattern(nowShow);
			lastShow = nowShow;
		}
	}

	/// <summary>
	/// 要素データを最新状態にリフレッシュします。
	/// ボーナス履歴数に応じてスクロール領域を設定し、パターン・グラフを更新します。
	/// </summary>
	public override void RefreshData()
	{
		// サイズ取得
		int size = hm.BonusHist.Count;
		int offset = size > 0 && hm.BonusHist[0].IsActivate ? 0 : 1;

		// スクロール領域設定と更新
		scroller.SetContentSize(size - offset, offset);
		scroller.ElemUpdate(true);
		scroller.MoveSelectedCenter();

		// 成立パターン表示
		ShowPattern(scroller.SelectedIndex);

		// グラフ更新 (初回nullチェック回避)
		GraphScript?.GraphDraw();
	}

	/// <summary>
	/// 指定インデックスのボーナス履歴パターンを表示します。
	/// 日付テキスト、契機テキスト、演出テキストをローカライズしてセットします。
	/// </summary>
	/// <param name="nowShow">表示する履歴のインデックス</param>
	private void ShowPattern(int nowShow)
	{
		int size = hm.BonusHist.Count;
		int offset = size > 0 && hm.BonusHist[0].IsActivate ? 0 : 1;

		dateShow.text = string.Empty;
		if (size - offset == 0)
		{
			builder.SetData(null, "NO DATA"); // 初回当たり前はマスク
		}
		else if (nowShow < 0 || nowShow >= size)
		{
			builder.SetData(null, "Select Bonus Data");
		}
		else
		{
			var entry = hm.BonusHist[nowShow];
			// リールパターン表示
			string tableText = string.Format(
				LocalizeGet.GetText(LocalizeTableID),
				entry.LossGame.ToString());
			builder.SetData(entry.InPattern, tableText);

			// 日付表示
			dateShow.text = entry.InDate;

			// 契機表示
			string[] broughtArr = LocalizeGet.GetText(LocalizeBroughtID).Split(",");
			BroughtText.text = broughtArr[entry.InPattern.FlagID];

			// 演出表示
			string[] effectArr = LocalizeGet.GetText(LocalizeEffectID).Split("|");
			EffectText.text = effectArr[entry.InPattern.InEffect]
				.Replace(
					",",
					Environment.NewLine + Environment.NewLine);
		}
	}

	/// <summary>
	/// メニュー操作キー入力時の処理を実装します。
	/// 上下キーでスクロール選択を移動します。
	/// </summary>
	/// <param name="eKeyID">押下されたメニュー操作ボタンの識別子</param>
	public override void OnGetKeyDown(EMenuButtonID eKeyID)
	{
		if (eKeyID == EMenuButtonID.eScrUp)
		{
			scroller.SetSelectedByKey(-1);
			scroller.MoveSelectedCenter();
		}
		else if (eKeyID == EMenuButtonID.eScrDn)
		{
			scroller.SetSelectedByKey(1);
			scroller.MoveSelectedCenter();
		}
	}
}
