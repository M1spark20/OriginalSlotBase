using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// コレクションのリールパターン一覧をスクロール表示するプレハブ要素です。
/// ScrollPrehabBase を継承し、CollectionData のアイテムを CollectionBuilder に設定して表示します。
/// </summary>
public class UIShowPatternColle : ScrollPrehabBase
{
	private SlotEffectMaker2023.Data.CollectionData cd;
	private SlotEffectMaker2023.Action.CollectionLogger logger;
	private GameObject[] refObj;
	private Canvas[] refCanvas;
	private CollectionBuilder[] refScr;

	private int showNum = 4;

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

		// scr取得 (Elem+0～Elem+3)
		for (int i = 0; i < showNum; ++i)
		{
			refObj[i] = transform.Find("Elem+" + i.ToString()).gameObject;
			refCanvas[i] = refObj[i].GetComponent<Canvas>();
			refScr[i] = refObj[i].GetComponent<CollectionBuilder>();
		}
	}

	/// <summary>
	/// 要素の表示データを更新します。
	/// 指定されたページインデックスに応じて4つの要素を有効/無効化し、CollectionBuilder にデータと進捗を設定します。
	/// </summary>
	/// <param name="pID">ページインデックス (0 から開始)</param>
	/// <param name="pIsSelected">この要素が現在選択中かどうか</param>
	public override void RefreshData(int pID, bool pIsSelected)
	{
		base.RefreshData(pID, pIsSelected);
		for (int i = 0; i < showNum; ++i)
		{
			int refID = showNum * pID + i;
			if (refID < 0 || refID >= cd.Collections.Count)
			{
				refCanvas[i].enabled = false;
			}
			else
			{
				refCanvas[i].enabled = true;
				refScr[i].SetData(cd.Collections[refID], logger.Achievements[refID], refID + 1);
			}
		}
		Debug.Log("test");
	}
}
