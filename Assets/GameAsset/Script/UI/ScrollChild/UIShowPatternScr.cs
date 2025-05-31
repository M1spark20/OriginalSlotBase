using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization;
using TMPro;

/// <summary>
/// リーチ目パターンをスクロール表示するプレハブ要素です。
/// ScrollPrehabBase を継承し、HistoryManager のパターン履歴を ReelPatternBuilder に設定して表示します。
/// </summary>
public class UIShowPatternScr : ScrollPrehabBase
{
	[SerializeField] private string LocalizeStringTable;
	[SerializeField] private string LocalizeStringID;

	private SlotEffectMaker2023.Data.HistoryConfig hc;
	private SlotEffectMaker2023.Action.HistoryManager hm;
	private ReelChipHolder comaData;
	private GameObject[] refObj;
	private Canvas[] refCanvas;
	private ReelPatternBuilder[] refScr;
	private TextMeshProUGUI[] info;

	private int showNum = 2;

	/// <summary>
	/// オブジェクト生成時の初期化処理です。
	/// シングルトンから HistoryConfig と HistoryManager を取得し、表示用子要素 (Elem+0～1) の参照をキャッシュします。
	/// </summary>
	protected override void Awake()
	{
		base.Awake();
		hc = SlotEffectMaker2023.Singleton.EffectDataManagerSingleton.GetInstance().HistoryConf;
		hm = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance().historyManager;
		comaData = ReelChipHolder.GetInstance();

		refObj = new GameObject[showNum];
		refCanvas = new Canvas[showNum];
		refScr = new ReelPatternBuilder[showNum];
		info = new TextMeshProUGUI[showNum];

		// scr取得(マジックナンバー要調整)
		for (int i = 0; i < showNum; ++i)
		{
			refObj[i] = transform.Find("Elem+" + i.ToString()).gameObject;
			refCanvas[i] = refObj[i].GetComponent<Canvas>();
			refScr[i] = refObj[i].GetComponent<ReelPatternBuilder>();
			info[i] = refObj[i].transform.Find("Info").GetComponent<TextMeshProUGUI>();
		}
	}

	/// <summary>
	/// 要素の表示データを最新状態に更新します。
	/// PatternHist の参照に基づき、要素ごとに ReelPatternBuilder にパターンデータとローカライズ済み文字列を設定します。
	/// </summary>
	/// <param name="pID">ページインデックス (0 から開始)</param>
	/// <param name="pIsSelected">この要素が現在選択中かどうか</param>
	public override void RefreshData(int pID, bool pIsSelected)
	{
		base.RefreshData(pID, pIsSelected);
		for (int i = 0; i < showNum; ++i)
		{
			int refID = showNum * pID + i;
			if (refID < 0 || refID >= hm.PatternHist.Count)
			{
				refCanvas[i].enabled = false;
			}
			else
			{
				refCanvas[i].enabled = true;
				// ローカライズ対応
				var localizedString = new LocalizedString(LocalizeStringTable, LocalizeStringID);
				localizedString.Arguments = new object[] { new Dictionary<string, int>() { { "0", refID } } };
				refScr[i].SetData(hm.PatternHist[refID], localizedString.GetLocalizedString());
			}
		}
		Debug.Log("test");
	}
}
