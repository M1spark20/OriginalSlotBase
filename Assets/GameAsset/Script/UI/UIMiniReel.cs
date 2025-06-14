using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ミニリールの表示とターゲット設定、クリックによるアシスト指定を行うUI制御クラス
/// </summary>
public class UIMiniReel : MonoBehaviour
{
	/// <summary>表示用コマのベースプレハブ</summary>
	[SerializeField] private GameObject ComaBase;
	/// <summary>コマ位置表示ベースプレハブ</summary>
	[SerializeField] private GameObject PosBase;
	/// <summary>ターゲット表示用プレハブ</summary>
	[SerializeField] private GameObject TargetBase;
	/// <summary>リール間のX方向距離</summary>
	[SerializeField] float DiffX;
	/// <summary>ポジション表示のオフセット</summary>
	[SerializeField] Vector2 PosCtrlOffset;

	private GameObject[,] Coma;
	private GameObject[,] Pos;
	private GameObject[] Target;
	private RectTransform[,] PosCtrl;
	private RectTransform[] TargetCtrl;
	private Image[] TargetImg;
	private Vector2 comaSize;

	private const int PosShowNum = 2;
	private const int reelNum = SlotMaker2022.LocalDataSet.REEL_MAX;
	private const int comaNum = SlotMaker2022.LocalDataSet.COMA_MAX;
	private const int reelNP = SlotEffectMaker2023.Action.ReelBasicData.REEL_NPOS;

	private SlotEffectMaker2023.Action.ReelBasicData[] ReelData;
	private SlotEffectMaker2023.Action.SlotTimerManager timer;

	// ターゲット設定用変数(20240602追加)
	private int targetReel;
	private int targetComa;
	private int lastComaPos;

	/// <summary>
	/// 初期化処理：コマやターゲットUIを生成し初期配置
	/// </summary>
	void Start()
	{
		Coma = new GameObject[reelNum, comaNum];
		Pos = new GameObject[reelNum, PosShowNum];
		Target = new GameObject[PosShowNum];
		PosCtrl = new RectTransform[reelNum, PosShowNum];
		TargetCtrl = new RectTransform[PosShowNum];
		TargetImg = new Image[PosShowNum];
		ReelData = new SlotEffectMaker2023.Action.ReelBasicData[reelNum];
		targetReel = -1;
		targetComa = -1;
		lastComaPos = -1;

		var ra = SlotMaker2022.MainROMDataManagerSingleton.GetInstance().ReelArray;
		var comaData = ReelChipHolder.GetInstance();
		comaSize = ComaBase.GetComponent<RectTransform>().sizeDelta;
		timer = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance().timerData;

		for (int reelC = 0; reelC < reelNum; ++reelC)
		{
			ReelData[reelC] = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance().reelData[reelC];

			for (int comaC = 0; comaC < comaNum; ++comaC)
			{
				Coma[reelC, comaC] = (reelC == 0 && comaC == 0) ? ComaBase : Instantiate(ComaBase, ComaBase.transform.parent.transform);
				Coma[reelC, comaC].GetComponent<Image>().sprite = comaData.ReelChipDataMini.Extract(ra[reelC][comaC].Coma);
				var pos = Coma[reelC, comaC].GetComponent<RectTransform>().anchoredPosition;
				pos += new Vector2(reelC * DiffX, -comaC * comaSize.y);
				Coma[reelC, comaC].GetComponent<RectTransform>().anchoredPosition = pos;
				var r = reelC;
				var c = comaC;
				Coma[reelC, comaC].GetComponent<Button>().onClick.AddListener(() => OnClickButton(r, c));
			}

			for (int posC = 0; posC < PosShowNum; ++posC)
			{
				Pos[reelC, posC] = (reelC == 0 && posC == 0) ? PosBase : Instantiate(PosBase, PosBase.transform.parent.transform);
				PosCtrl[reelC, posC] = Pos[reelC, posC].GetComponent<RectTransform>();
				var pos = PosCtrl[reelC, posC].anchoredPosition;
				pos += new Vector2(reelC * DiffX, -posC * comaNum * comaSize.y);
				PosCtrl[reelC, posC].anchoredPosition = pos;
			}
		}

		for (int posC = 0; posC < PosShowNum; ++posC)
		{
			Target[posC] = posC == 0 ? TargetBase : Instantiate(TargetBase, TargetBase.transform.parent.transform);
			TargetCtrl[posC] = Target[posC].GetComponent<RectTransform>();
			TargetImg[posC] = Target[posC].GetComponent<Image>();
		}

		RefreshPos();
	}

	/// <summary>
	/// 毎フレーム、コマの位置とアシスト音を処理
	/// </summary>
	void Update()
	{
		RefreshPos();

		if (targetReel == -1 || targetComa == -1) return;
		var comaData = ReelData[targetReel];
		if (!comaData.isRotate || !comaData.accEnd || comaData.pushPos != reelNP) return;

		int comaPos = comaData.GetReelComaIDFixed();
		if (comaPos != lastComaPos && comaPos == targetComa)
		{
			timer.GetTimer("AssistSound").Activate();
			timer.GetTimer("AssistSound").Reset();
		}
		lastComaPos = comaPos;
	}

	/// <summary>
	/// コマの位置とターゲットウィンドウの位置を更新する処理
	/// </summary>
	private void RefreshPos()
	{
		for (int reelC = 0; reelC < reelNum; ++reelC)
		{
			for (int posC = 0; posC < PosShowNum; ++posC)
			{
				var pos = PosCtrl[reelC, posC].anchoredPosition;
				pos.y = (ReelData[reelC].reelPos - posC * comaNum) * comaSize.y + PosCtrlOffset.y;
				PosCtrl[reelC, posC].anchoredPosition = pos;
			}
		}

		if (targetReel >= 0)
		{
			var comaData = ReelData[targetReel];
			if (!comaData.isRotate && comaData.stopPos == targetComa)
			{
				targetReel = -1;
				targetComa = -1;
			}
		}

		for (int posC = 0; posC < PosShowNum; ++posC)
		{
			TargetImg[posC].enabled = (targetReel >= 0 && targetComa >= 0);
			TargetCtrl[posC].anchoredPosition = new Vector2(targetReel * DiffX, (targetComa - posC * comaNum) * comaSize.y) + PosCtrlOffset;
		}
	}

	/// <summary>
	/// コマをクリックしたときの処理。指定位置が既に選択されていれば解除、それ以外ならアシスト対象として設定
	/// </summary>
	/// <param name="reelC">クリックされたリール番号</param>
	/// <param name="comaC">クリックされたコマ番号</param>
	public void OnClickButton(int reelC, int comaC)
	{
		comaC = comaNum - comaC - 1;

		if (reelC == targetReel && comaC == targetComa)
		{
			targetReel = -1;
			targetComa = -1;
		}
		else
		{
			targetReel = reelC;
			targetComa = comaC;
			lastComaPos = -1;
		}
	}
}
