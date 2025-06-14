using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// コレクション要素を構築・描画するクラスです。
/// 成立時のリールパターンや達成情報を表示します。
/// </summary>
public class CollectionBuilder : MonoBehaviour
{
	// Start is called before the first frame update
	// 成立時出目データ
	[Header("成立時出目データ")]
	[SerializeField] protected float ComaDX;
	[SerializeField] protected float LvIconDX;

	[SerializeField] protected string LocalizeRotateID;
	[SerializeField] protected string LocalizeHazureID;
	[SerializeField] protected string LocalizeAimingID;

	private GameObject Labels;
	private GameObject ReelInfo;
	private GameObject NCMask;
	private GameObject[] LevelIcon;
	private GameObject[] BonusInBG;
	private GameObject[] BonusInStopInfo;
	private GameObject[][] BonusInComaImg;
	private GameObject[][] BonusInComaID;
	private GameObject[] BonusInCutLine;
	private SlotMaker2022.LocalDataSet.ReelArray[][] ra;
	private GetDynamicLocalText lc;

	private SlotEffectMaker2023.Action.HistoryManager hm;
	private ReelChipHolder comaData;
	private float defaultLineHeight;
	private int ComaShowMax = 4;
	private int LevelIconNum = 5;

	/// <summary>
	/// コンポーネントの初期化処理を行います。
	/// 成立時出目のUI要素を生成し、必要な参照を取得します。
	/// </summary>
	private void Awake()
	{
		// 成立時出目データ初期化
		const int reelNum = SlotMaker2022.LocalDataSet.REEL_MAX;
		const int showComaNum = SlotMaker2022.LocalDataSet.SHOW_MAX;
		Labels = transform.Find("LabelSet").gameObject;
		ReelInfo = transform.Find("ReelSet").gameObject;
		NCMask = transform.Find("NCMask").gameObject;
		LevelIcon = new GameObject[LevelIconNum];
		BonusInBG = new GameObject[reelNum];
		BonusInStopInfo = new GameObject[reelNum];
		BonusInCutLine = new GameObject[reelNum];
		BonusInComaImg = new GameObject[reelNum][];
		BonusInComaID = new GameObject[reelNum][];
		ra = SlotMaker2022.MainROMDataManagerSingleton.GetInstance().ReelArray;
		for (int i = 0; i < reelNum; ++i)
		{
			BonusInComaImg[i] = new GameObject[showComaNum];
			BonusInComaID[i] = new GameObject[showComaNum];
		}

		LevelIcon[0] = Labels.transform.Find("Level_icon").gameObject;
		BonusInBG[0] = Labels.transform.Find("BG").gameObject;
		BonusInStopInfo[0] = ReelInfo.transform.Find("Info").gameObject;
		BonusInCutLine[0] = ReelInfo.transform.Find("Line").gameObject;
		BonusInComaImg[0][0] = ReelInfo.transform.Find("ComaSet").gameObject;
		BonusInComaID[0][0] = ReelInfo.transform.Find("ComaID").gameObject;
		defaultLineHeight = BonusInCutLine[0].transform.localPosition.y;

		for (int i = 0; i < reelNum; ++i)
		{
			if (i > 0)
			{
				BonusInBG[i] = Instantiate(BonusInBG[0], Labels.transform);
				BonusInBG[i].transform.localPosition += new Vector3(ComaDX * i, 0, 0);
				BonusInStopInfo[i] = Instantiate(BonusInStopInfo[0], ReelInfo.transform);
				BonusInStopInfo[i].transform.localPosition += new Vector3(ComaDX * i, 0, 0);
				BonusInCutLine[i] = Instantiate(BonusInCutLine[0], ReelInfo.transform);
				BonusInCutLine[i].transform.localPosition += new Vector3(ComaDX * i, 0, 0);
			}
			for (int j = (i == 0 ? 1 : 0); j < showComaNum; ++j)
			{
				BonusInComaImg[i][j] = Instantiate(BonusInComaImg[0][0], ReelInfo.transform);
				BonusInComaImg[i][j].transform.localPosition += new Vector3(ComaDX * i, BonusInComaImg[0][0].GetComponent<RectTransform>().sizeDelta.y * j, 0);
				BonusInComaID[i][j] = Instantiate(BonusInComaID[0][0], ReelInfo.transform);
				BonusInComaID[i][j].transform.localPosition += new Vector3(ComaDX * i, BonusInComaImg[0][0].GetComponent<RectTransform>().sizeDelta.y * j, 0);
			}
		}
		for (int i = 1; i < LevelIconNum; ++i)
		{
			LevelIcon[i] = Instantiate(LevelIcon[0], Labels.transform);
			LevelIcon[i].transform.localPosition += new Vector3(LvIconDX * i, 0, 0);
		}

		hm = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance().historyManager;
		lc = GetComponent<GetDynamicLocalText>();
		comaData = ReelChipHolder.GetInstance();
		Reset();
	}

	/// <summary>
	/// 指定されたパターンと実績データをUIに適用して表示を更新します。
	/// </summary>
	/// <param name="nowPtn">表示するコレクションパターンデータ</param>
	/// <param name="achievement">該当要素の達成情報</param>
	/// <param name="pID">要素番号（1始まり）</param>
	public void SetData(SlotEffectMaker2023.Data.CollectionDataElem nowPtn, SlotEffectMaker2023.Action.CollectionAchieveElem achievement, int pID)
	{
		if (nowPtn == null) return;

		// 番号・達成状況描画
		Reset();
		NCMask.GetComponent<Image>().enabled = !(achievement.CompTimes > 0);
		Labels.transform.Find("No").GetComponent<TextMeshProUGUI>().text = "No." + pID.ToString("D03");
		Labels.transform.Find("CompCount").GetComponent<TextMeshProUGUI>().text = achievement.CompTimes.ToString();
		Labels.transform.Find("DateF").GetComponent<TextMeshProUGUI>().text = achievement.FirstComp;
		Labels.transform.Find("DateR").GetComponent<TextMeshProUGUI>().text = achievement.RecentComp;

		for (int i = 0; i < BonusInBG.Length; ++i)
		{
			// 共通定義
			var ePtn = nowPtn.CollectionElem[i].Pattern;
			BonusInCutLine[i].GetComponent<Image>().enabled = false;
			string[] infoStr = { "", "", "ANY", lc.GetText(LocalizeRotateID), lc.GetText(LocalizeHazureID), lc.GetText(LocalizeAimingID) };
			BonusInStopInfo[i].GetComponent<TextMeshProUGUI>().text = infoStr[(int)ePtn];
			for (int j = 0; j < LevelIconNum; ++j) LevelIcon[j].GetComponent<Image>().color = j < nowPtn.Level ? Color.yellow : Color.gray * 0.5f;

			// ReelPos定義
			if (ePtn == SlotEffectMaker2023.Data.CollectionReelPattern.eReelPos)
			{
				for (int j = 0; j < BonusInComaImg[i].Length; ++j)
				{
					int showComa = (j + nowPtn.CollectionElem[i].ReelPos) % SlotMaker2022.LocalDataSet.COMA_MAX;
					GameObject modComa = BonusInComaImg[i][j].transform.Find("ComaImg+1").gameObject;
					modComa.GetComponent<Canvas>().enabled = true;
					BonusInComaID[i][j].GetComponent<Image>().enabled = true;
					// データは逆順に格納されていることに注意する。
					modComa.transform.Find("0").GetComponent<Image>().sprite =
						comaData.ReelChipDataMini.Extract(ra[i][SlotMaker2022.LocalDataSet.COMA_MAX - showComa - 1].Coma);
					BonusInComaID[i][j].transform.Find("Text").GetComponent<TextMeshProUGUI>().text = (showComa + 1).ToString();
					if (showComa == 0)
					{
						BonusInCutLine[i].GetComponent<Image>().enabled = true;
						Vector3 pos = BonusInCutLine[i].transform.localPosition;
						pos.y = defaultLineHeight + BonusInComaImg[0][0].GetComponent<RectTransform>().sizeDelta.y * j;
						BonusInCutLine[i].transform.localPosition = pos;
					}
				}
			}

			// ComaID定義
			if (ePtn == SlotEffectMaker2023.Data.CollectionReelPattern.eComaItem)
			{
				for (byte comaC = 0; comaC < BonusInComaImg[i].Length; ++comaC)
				{
					// データ一覧取得、要素数から使用するComaImgを呼び出し
					List<byte> comaSet = nowPtn.CollectionElem[i].GetItemList(comaC);
					GameObject modComa = BonusInComaImg[i][comaC]
						.transform.Find("ComaImg+" + comaSet.Count.ToString())?.gameObject ?? null;
					if (modComa == null) continue;
					modComa.GetComponent<Canvas>().enabled = true;
					// コマ配置
					for (byte symC = 0; symC < comaSet.Count; ++symC)
						modComa.transform.Find(symC.ToString()).GetComponent<Image>().sprite =
							comaData.ReelChipDataMini.Extract(comaSet[symC]);
					// Invert描画
					BonusInComaImg[i][comaC].transform.Find("Invert").GetComponent<Image>().enabled =
						nowPtn.CollectionElem[i].ComaItem[comaC] < 0;
				}
			}
		}
	}

	/// <summary>
	/// UI 表示を初期状態にリセットします。
	/// </summary>
	public void Reset()
	{
		NCMask.GetComponent<Image>().enabled = false;
		for (int i = 0; i < BonusInBG.Length; ++i)
		{
			BonusInStopInfo[i].GetComponent<TextMeshProUGUI>().text = string.Empty;
			BonusInCutLine[i].GetComponent<Image>().enabled = false;
			for (int j = 0; j < BonusInComaImg[i].Length; ++j)
			{
				BonusInComaID[i][j].GetComponent<Image>().enabled = false;
				BonusInComaID[i][j].transform.Find("Text").GetComponent<TextMeshProUGUI>().text = string.Empty;
				BonusInComaImg[i][j].transform.Find("Invert").GetComponent<Image>().enabled = false;
				for (int k = 1; k <= ComaShowMax; ++k)
					BonusInComaImg[i][j].transform.Find("ComaImg+" + k.ToString()).GetComponent<Canvas>().enabled = false;
			}
		}
	}
}
