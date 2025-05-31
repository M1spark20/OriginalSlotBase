using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// メインメニュー全体を管理するクラスです。
/// 複数のパネル切り替え、選択 UI の表示、入力処理の振り分けを行います。
/// </summary>
public class MainMenuManager : MonoBehaviour
{
	[SerializeField]
	private GameObject[] Panels;

	[SerializeField]
	private Button SelectorBase;

	private int SelectedID;
	private Button[] Selector;
	private Image[] SelImage;
	private TextMeshProUGUI[] SelName;

	// 各パネルからキー入力情報を取得するための記録機能
	private MainMenuElemBase[] PanelScr;
	private CanvasGroup[] PanelsCanvasGroup;
	private GraphicRaycaster[] PanelsTouch;

	/// <summary>
	/// オブジェクト生成時に呼び出される初期化メソッドです。
	/// セレクターボタンとパネルの各種コンポーネントを取得し、初期選択状態を設定します。
	/// </summary>
	private void Awake()
	{
		SelectedID = -1;
		PanelScr = new MainMenuElemBase[Panels.Length];
		PanelsCanvasGroup = new CanvasGroup[Panels.Length];
		PanelsTouch = new GraphicRaycaster[Panels.Length];

		Selector = new Button[Panels.Length];
		SelImage = new Image[Panels.Length];
		SelName = new TextMeshProUGUI[Panels.Length];
		Selector[0] = SelectorBase;
		float sizeX = SelectorBase.GetComponent<RectTransform>().sizeDelta.x;

		for (int i = 0; i < Panels.Length; ++i)
		{
			// 選択用ボタン配置
			PanelsCanvasGroup[i] = Panels[i].GetComponent<CanvasGroup>();
			PanelsTouch[i] = Panels[i].GetComponent<GraphicRaycaster>();
			Panels[i].GetComponent<Canvas>().enabled = true;
			PanelsCanvasGroup[i].alpha = 0f;
			PanelsTouch[i].enabled = false;
			if (i > 0)
			{
				Selector[i] = Instantiate(SelectorBase, this.transform);
				Selector[i].transform.localPosition += new Vector3(sizeX * i, 0, 0);
			}
			SelImage[i] = Selector[i].GetComponent<Image>();
			SelName[i] = Selector[i].transform.Find("name").GetComponent<TextMeshProUGUI>();
			var prm = i;    // 変数に入れないとラムダ式でキャプチャ時に問題がある
			Selector[i].onClick.AddListener(() => OnClickButton(prm));

			// スクリプト登録とラベル設定
			PanelScr[i] = Panels[i].GetComponent<MainMenuElemBase>();
			SelName[i].text = PanelScr[i].GetElemName();
		}

		// 表示初期化 (メニュー非表示状態から)
		RefreshActivate(0);
		PanelsTouch[0].enabled = false;
	}

	/// <summary>
	/// 毎フレーム呼び出される更新処理です。
	/// セレクターの色と名称を現在の選択状態に応じて更新します。
	/// </summary>
	private void Update()
	{
		for (int i = 0; i < Selector.Length; ++i)
		{
			SelImage[i].color = SelectedID == i ? Color.yellow : new Color(0.88f, 0.88f, 0.88f);
			SelName[i].text = PanelScr[i].GetElemName();
			SelName[i].color = SelectedID == i ? Color.yellow : Color.white;
		}
	}

	/// <summary>
	/// メニュー操作キー入力時の処理を実装します。
	/// 左右キーでパネル切り替え、その他はアクティブパネルへ振り分けます。
	/// </summary>
	/// <param name="eKeyID">押下されたメニュー操作ボタンの識別子</param>
	public void OnGetKeyDown(EMenuButtonID eKeyID)
	{
		if (eKeyID == EMenuButtonID.eScrLeft)
		{
			RefreshActivate((SelectedID + Panels.Length - 1) % Panels.Length);
		}
		else if (eKeyID == EMenuButtonID.eScrRight)
		{
			RefreshActivate((SelectedID + 1) % Panels.Length);
		}
		else
		{
			// 各パネルにキー入力を転送
			PanelScr[SelectedID].OnGetKeyDown(eKeyID);
		}
	}

	/// <summary>
	/// 指定されたパネルをアクティブにし、他を非表示に設定します。
	/// </summary>
	/// <param name="activeID">アクティブにするパネルのインデックス</param>
	private void RefreshActivate(int activeID)
	{
		if (activeID != SelectedID)
		{
			if (SelectedID >= 0)
			{
				PanelsCanvasGroup[SelectedID].alpha = 0f;
				PanelsTouch[SelectedID].enabled = false;
			}
			if (activeID >= 0)
			{
				PanelScr[activeID].RefreshData();
				PanelsCanvasGroup[activeID].alpha = 1f;
				PanelsTouch[activeID].enabled = true;
			}
			SelectedID = activeID;
		}
	}

	/// <summary>
	/// メニューの表示状態が変わった際に呼ばれます。
	/// 再表示時にアクティブパネルの入力を有効化し、必要に応じてデータを更新します。
	/// </summary>
	/// <param name="visible">メニューが表示されているかどうか</param>
	public void OnMenuShownChange(bool visible)
	{
		if (SelectedID < 0)
			return;

		PanelsTouch[SelectedID].enabled = visible;
		if (visible)
			PanelScr[SelectedID].RefreshData();
	}

	/// <summary>
	/// セレクターボタンを直接クリックした際に呼ばれます。
	/// 該当インデックスのパネルをアクティブ化します。
	/// </summary>
	/// <param name="index">クリックされたボタンのインデックス</param>
	public void OnClickButton(int index)
	{
		RefreshActivate(index);
	}
}
