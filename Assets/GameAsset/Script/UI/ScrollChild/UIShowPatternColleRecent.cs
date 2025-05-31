using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// コレクションの最新達成パターンをスクロール表示するプレハブ要素です。
/// ScrollPrehabBase を継承し、CollectionData の最新取得 ID に基づいて CollectionBuilder にデータを設定して表示します。
/// </summary>
public class UIShowPatternColleRecent : ScrollPrehabBase
{
	private SlotEffectMaker2023.Data.CollectionData cd;
	private SlotEffectMaker2023.Action.CollectionLogger logger;
	private GameObject[] refObj;
	private Canvas[] refCanvas;
	private CollectionBuilder[] refScr;

	/// <summary>表示する要素数 (固定: 4)</summary>
	public const int showNum = 4;

	/// <summary>
	/// オブジェクト生成時の初期化処理です。
	/// シングルトンから CollectionData と CollectionLogger を取得し、子要素 (Elem+0～3) の参照をキャッシュします。
	/// </summary>
	protected override void Awake()
	{
		base.Awake();
		cd = SlotEffectMaker2023.Singleton.EffectDataManagerSingleton.GetInstance().Collection;
		logger = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance().collectionManager;

		refObj = new GameObject[showNum];
		refCanvas = new Canvas[showNum];
		refScr = new CollectionBuilder[showNum];

		// scr取得(マジックナンバー要調整)
		for (int i = 0; i < showNum; ++i)
		{
			refObj[i] = transform.Find("Elem+" + i.ToString()).gameObject;
			refCanvas[i] = refObj[i].GetComponent<Canvas>();
			refScr[i] = refObj[i].GetComponent<CollectionBuilder>();
		}
	}

	/// <summary>
	/// 要素の表示データを最新状態に更新します。
	/// ログの NewGetID リストに基づき、表示可能な要素を有効/無効化し、CollectionBuilder に該当データと進捗を設定します。
	/// </summary>
	/// <param name="pID">ページインデックス (0 から開始)</param>
	/// <param name="pIsSelected">この要素が現在選択中かどうか</param>
	public override void RefreshData(int pID, bool pIsSelected)
	{
		base.RefreshData(pID, pIsSelected);
		for (int i = 0; i < showNum; ++i)
		{
			int refID = showNum * pID + i;
			if (refID < 0 || refID >= logger.NewGetID.Count)
			{
				refCanvas[i].enabled = false;
			}
			else
			{
				refCanvas[i].enabled = true;
				int content = logger.NewGetID[refID];
				refScr[i].SetData(cd.Collections[content], logger.Achievements[content], content + 1);
			}
		}
		Debug.Log("test");
	}
}
