using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// ボーナス履歴をスクロール表示するプレハブ用クラスです。
/// ScrollPrehabBase を継承し、履歴データをUI要素に反映します。
/// </summary>
public class UIShowBonusHistScr : ScrollPrehabBase
{
	[SerializeField] private Button Select;

	private SlotEffectMaker2023.Data.HistoryConfig hc;
	private SlotEffectMaker2023.Action.HistoryManager hm;
	private ReelChipHolder comaData;

	/// <summary>
	/// オブジェクト生成時の初期化処理です。
	/// ScrollPrehabBase の Awake を呼び、履歴設定・マネージャ・リールチップホルダを取得し、
	/// セレクトボタンのクリックリスナーを登録します。
	/// </summary>
	protected override void Awake()
	{
		base.Awake();
		hc = SlotEffectMaker2023.Singleton.EffectDataManagerSingleton.GetInstance().HistoryConf;
		hm = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance().historyManager;
		comaData = ReelChipHolder.GetInstance();

		Select?.onClick.AddListener(OnClick);
	}

	/// <summary>
	/// 履歴アイテムの表示情報を更新します。
	/// ScrollPrehabBase.RefreshData を呼び出した後、IDに対応するボーナス履歴データを
	/// UIテキストやイメージに反映します。
	/// </summary>
	/// <param name="pID">更新対象の履歴インデックス</param>
	/// <param name="pIsSelected">現在選択中かどうか</param>
	public override void RefreshData(int pID, bool pIsSelected)
	{
		if (pID < 0 || pID >= hm.BonusHist.Count) return;
		base.RefreshData(pID, pIsSelected);

		var refData = hm.BonusHist[pID];
		var txtColor = pIsSelected ? Color.yellow : Color.white;

		transform.Find("Number").GetComponent<TextMeshProUGUI>().text = (hm.BonusHist.Count - pID).ToString();
		transform.Find("Number").GetComponent<TextMeshProUGUI>().color = txtColor;
		transform.Find("Game").GetComponent<TextMeshProUGUI>().text = refData.InGame.ToString();
		transform.Find("Game").GetComponent<TextMeshProUGUI>().color = txtColor;
		transform.Find("Coma").GetComponent<Image>().sprite =
			comaData.ReelChipDataMini.Extract(hc.GetConfig(refData.BonusFlag).ComaID);
		transform.Find("Get").GetComponent<TextMeshProUGUI>().text =
			refData.IsFinished ? (refData.MedalAfter - refData.MedalBefore).ToString() : string.Empty;
		transform.Find("Get").GetComponent<TextMeshProUGUI>().color = txtColor;
	}

	/// <summary>
	/// ボタンがクリックされたとき、自身を選択状態に設定します。
	/// </summary>
	public void OnClick()
	{
		SelectMe();
	}
}
