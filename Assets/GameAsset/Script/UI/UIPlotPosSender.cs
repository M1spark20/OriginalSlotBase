using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI 上でプロット位置を選択し、システムデータに送信するクラス
/// </summary>
public class UIPlotPosSender : MonoBehaviour
{
	/// <summary>ボタンのベースプレハブ</summary>
	[SerializeField] private Button BaseButton;

	/// <summary>ボタン間の配置間隔</summary>
	[SerializeField] private Vector2 Interval;

	/// <summary>ボタンの列数と行数（X:列, Y:行）</summary>
	[SerializeField] private Vector2Int NumCount;

	private Button[] selector;
	private Image[] im;
	private TextMeshProUGUI[] tx;
	private SlotEffectMaker2023.Action.SystemData sys;

	/// <summary>
	/// 初期化処理：ボタンを配置し、クリックイベントを登録します
	/// </summary>
	private void Start()
	{
		int sz = NumCount.x * NumCount.y;
		selector = new Button[sz];
		im = new Image[sz];
		tx = new TextMeshProUGUI[sz];
		sys = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance().sysData;

		// Instantiateする
		for (int i = 0; i < sz; ++i)
		{
			selector[i] = i == 0 ? BaseButton : Instantiate(BaseButton, this.transform);
			im[i] = selector[i].GetComponent<Image>();
			tx[i] = selector[i].transform.Find("IDText").GetComponent<TextMeshProUGUI>();

			selector[i].GetComponent<RectTransform>().anchoredPosition +=
				new Vector2(Interval.x * (i % NumCount.x), Interval.y * (i / NumCount.x));
			tx[i].text = (i + 1).ToString();

			// ボタン押下時のスクリプト登録
			var prm = i; // 変数に入れないとラムダ式内で正しく渡らないため
			selector[i].onClick.AddListener(() => OnClickButton(prm));
		}
	}

	/// <summary>
	/// 毎フレームの更新処理：現在選択中の位置に応じて色を変更します
	/// </summary>
	private void Update()
	{
		for (int i = 0; i < selector.Length; ++i)
		{
			Color cl = sys.InfoPos == i ? Color.yellow : Color.white;
			im[i].color = cl;
			tx[i].color = cl;
		}
	}

	/// <summary>
	/// ボタンがクリックされたときにプロット位置をシステムデータへ送信します
	/// </summary>
	/// <param name="index">クリックされたボタンのインデックス</param>
	public void OnClickButton(int index)
	{
		sys.InfoPos = index;
	}
}
