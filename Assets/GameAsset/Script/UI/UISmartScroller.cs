using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// スマートなスクロール可能リストを実現するUIコンポーネント。
/// コンテンツプレハブを再利用し、スクロールに応じて要素を動的に更新・配置します。
/// </summary>
public class UISmartScroller : MonoBehaviour
{
	/// <summary>
	/// コンテンツ要素として使用するプレハブ。
	/// </summary>
	[SerializeField] private GameObject ContentPrehab;
	/// <summary>
	/// レイキャスト設定の切替に使用するGraphicRaycaster参照。
	/// </summary>
	[SerializeField] private GraphicRaycaster RaycasterReference;

	/// <summary>
	/// オブジェクトの表示を行うデータのインスタンス配列。
	/// </summary>
	private GameObject[] ShowData;        // オブジェクトの表示を行うデータ
	/// <summary>
	/// 各インスタンスにアタッチされたスクリプトの配列。
	/// </summary>
	private ScrollPrehabBase[] ShowScr;   // オブジェクトのスクリプト
	/// <summary>
	/// 各インスタンスが表示するデータIDの配列。
	/// </summary>
	private int[] ShowContentID;          // オブジェクトが表示するデータのID

	/// <summary>
	/// ContentリストのTransform。
	/// </summary>
	private Transform ContentList;
	/// <summary>
	/// ContentリストのRectTransform。
	/// </summary>
	private RectTransform ContentTransform;
	/// <summary>
	/// 自身のRectTransform。
	/// </summary>
	private RectTransform MyTransform;
	/// <summary>
	/// ScrollRectコンポーネント。
	/// </summary>
	private ScrollRect Rect;
	/// <summary>
	/// プレハブ要素の高さ。
	/// </summary>
	private float ContentSize;
	/// <summary>
	/// 管理する実際の要素数（オーバードロー含む）。
	/// </summary>
	private int ShowContentNum;

	/// <summary>
	/// 総コンテンツ数。
	/// </summary>
	public int ContentCount { get; private set; }
	/// <summary>
	/// 表示開始オフセット。
	/// </summary>
	public int ShowOffset { get; private set; }
	/// <summary>
	/// 選択されているコンテンツID。
	/// </summary>
	public int SelectedIndex { get; private set; }

	/// <summary>
	/// オーバードローとして追加で保持する要素数。
	/// </summary>
	private const int overDraw = 2;

	/// <summary>
	/// Awake はインスタンス化時に呼び出され、ビューサイズ確認と要素インスタンスの初期化を行います。
	/// </summary>
	public void Awake()
	{
		ContentSize = ContentPrehab?.GetComponent<RectTransform>().sizeDelta.y ?? 1f;
		ContentList = transform.Find("Viewport/Content");
		ContentTransform = ContentList.GetComponent<RectTransform>();
		MyTransform = GetComponent<RectTransform>();
		Rect = GetComponent<ScrollRect>();
		ContentCount = 0;
		CheckViewSize();
		SelectedIndex = -1;

		ShowData = null;
		if (ContentPrehab == null) return;

		ShowData = new GameObject[ShowContentNum];
		ShowScr = new ScrollPrehabBase[ShowContentNum];
		ShowContentID = new int[ShowContentNum];
		for (int i = 0; i < ShowData.Length; ++i)
		{
			ShowData[i] = Instantiate(ContentPrehab, ContentList.transform);
			ShowData[i].name = i.ToString();
			ShowScr[i] = ShowData[i].GetComponent<ScrollPrehabBase>();
			ShowContentID[i] = int.MinValue;
			// 初期位置は[0]を一番上に置く(ただし一番上は枠外)
			ShowData[i].transform.localPosition = new Vector3(0, -ContentSize * ShowContentID[i], 0);
			ShowScr[i].SetVisible(i >= ShowOffset && i < ContentCount);
		}
		Rect.verticalNormalizedPosition = 1f;
	}

	/// <summary>
	/// Update は毎フレーム呼び出され、要素の更新とレイキャスター有効設定を行います。
	/// </summary>
	public void Update()
	{
		ElemUpdate(false);
		if (RaycasterReference != null)
		{
			bool flag = RaycasterReference.enabled;
			foreach (var item in ShowScr) item.SetRaycaster(flag);
		}
	}

	/// <summary>
	/// ビューサイズから一度に表示可能な要素数を計算します。
	/// </summary>
	public void CheckViewSize()
	{
		float ViewSize = MyTransform.sizeDelta.y;
		ShowContentNum = (int)Mathf.Ceil(ViewSize / ContentSize) + 2 * overDraw;  // はみ出し量も1としてカウント
	}

	/// <summary>
	/// 指定インデックスのコンテンツIDを取得します。
	/// </summary>
	/// <param name="pID">コンテンツ配列内のインデックス。</param>
	/// <returns>表示対象データのID。</returns>
	public int GetContentID(int pID) { return ShowContentID[pID]; }

	/// <summary>
	/// 指定インデックスが選択中か判定します。
	/// </summary>
	/// <param name="pID">コンテンツ配列内のインデックス。</param>
	/// <returns>選択中であればtrue。</returns>
	public bool GetIsSelected(int pID) { return ShowContentID[pID] == SelectedIndex; }

	/// <summary>
	/// 指定インデックスを選択状態に設定します。
	/// </summary>
	/// <param name="pID">コンテンツ配列内のインデックス。</param>
	public void SetSelected(int pID) { SelectedIndex = ShowContentID[pID]; }

	/// <summary>
	/// キー操作に応じて選択インデックスを移動します。
	/// </summary>
	/// <param name="diff">移動量（正負）。</param>
	public void SetSelectedByKey(int diff)
	{
		SelectedIndex += diff;
		if (SelectedIndex < ShowOffset) SelectedIndex = ShowOffset;
		if (SelectedIndex >= ContentCount + ShowOffset) SelectedIndex = ContentCount + ShowOffset - 1;
	}

	/// <summary>
	/// 選択中要素を画面中央にスクロールさせます。
	/// </summary>
	public void MoveSelectedCenter()
	{
		float MarkPos = (MyTransform.sizeDelta.y + ContentSize) / 2f;
		float ScrPos = SelectedIndex * ContentSize - MarkPos;
		float moveSize = ContentTransform.sizeDelta.y - MyTransform.sizeDelta.y;
		if (moveSize <= 0f) Rect.verticalNormalizedPosition = 1f;
		else
		{
			float nmPos = ScrPos / moveSize;
			if (nmPos < 0f) nmPos = 0f;
			if (nmPos > 1f) nmPos = 1f;
			Rect.verticalNormalizedPosition = 1f - nmPos;
		}
	}

	/// <summary>
	/// 指定高さ分だけスクロール位置を移動させます。
	/// </summary>
	/// <param name="MoveSizeByOriginalHeight">ビュー高さを1としたときの移動量。</param>
	public void MovePosition(float MoveSizeByOriginalHeight)
	{
		float moveSize = ContentTransform.sizeDelta.y - MyTransform.sizeDelta.y;
		float nmPos = (1f - Rect.verticalNormalizedPosition) + (MoveSizeByOriginalHeight * MyTransform.sizeDelta.y) / moveSize;
		if (nmPos < 0f) nmPos = 0f;
		if (nmPos > 1f) nmPos = 1f;
		Rect.verticalNormalizedPosition = 1f - nmPos;
	}

	/// <summary>
	/// コンテンツ全体のサイズとオフセットを設定し、スクロール範囲を更新します。
	/// </summary>
	/// <param name="size">表示可能な要素数。</param>
	/// <param name="offset">表示開始オフセット。</param>
	public void SetContentSize(int size, int offset)
	{
		int lastSize = ContentCount;
		int lastOffset = ShowOffset;
		ContentCount = size;
		ShowOffset = offset;
		var contSize = ContentTransform.sizeDelta;
		contSize.y = ContentSize * (ContentCount);
		ContentTransform.sizeDelta = contSize;

		if (size != lastSize) SelectedIndex = offset;
		else SelectedIndex += (offset - lastOffset);
		if (size == 0) SelectedIndex = -1;
	}

	/// <summary>
	/// スクロール位置に応じて表示要素を更新し、必要に応じて再配置やデータリフレッシュを行います。
	/// </summary>
	/// <param name="pForceUpdate">強制更新フラグ。</param>
	public void ElemUpdate(bool pForceUpdate)
	{
		float nmPos = 1f - Rect.verticalNormalizedPosition;
		float moveSize = ContentTransform.sizeDelta.y - MyTransform.sizeDelta.y;
		if (nmPos < 0f) nmPos = 0f;
		if (nmPos > 1f) nmPos = 1f;
		if (moveSize < 0f) nmPos = 0f;

		float showBegin = nmPos * moveSize;
		int beginItem = (int)(showBegin / ContentSize);
		int ctrlBegin = beginItem % ShowContentNum;

		for (int i = 0; i < ShowData.Length; ++i)
		{
			int ctrl = (ctrlBegin + i) % ShowContentNum;
			int currentID = beginItem + i - overDraw + ShowOffset;

			if (ShowContentID[ctrl] != currentID || ShowScr[ctrl].Selected ^ (currentID == SelectedIndex) || pForceUpdate)
			{
				ShowContentID[ctrl] = currentID;
				ShowScr[ctrl].SetVisible(currentID >= ShowOffset && currentID < ContentCount + ShowOffset);
				ShowScr[ctrl].RefreshData(currentID, currentID == SelectedIndex);
			}
			ShowData[ctrl].transform.localPosition = new Vector3(0, -(ContentSize * (currentID - ShowOffset)), 0);
		}
	}
}