using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 単一スプライトを用いたマップ表示を行うコンポーネント。
/// ArrayX×ArrayYの配置で同一スプライトを表示し、カラーマップに従って色変更します。
/// </summary>
public class UnityMapPlayerSingle : IMultiImageWriter
{
	/// <summary>
	/// マップの水平要素数。
	/// </summary>
	[SerializeField, Min(1)] protected int ArrayX;
	/// <summary>
	/// マップの垂直要素数。
	/// </summary>
	[SerializeField, Min(1)] protected int ArrayY;
	/// <summary>
	/// X方向の重複オフセット。
	/// </summary>
	[SerializeField] protected float LapX;
	/// <summary>
	/// Y方向の重複オフセット。
	/// </summary>
	[SerializeField] protected float LapY;
	/// <summary>
	/// リール用カラーマップシフタ名の配列。
	/// </summary>
	[SerializeField] private string[] MapShifterName;

	/// <summary>
	/// 画像分割と配置パラメータを初期化します。
	/// CutWayXはtrue、ShowX/YはArrayX/Y、OverlapX/YはLapX/LapYを使用します。
	/// </summary>
	protected override void InitDivision()
	{
		DivX = 1;
		DivY = 1;
		CutWayX = true;
		ShowX = ArrayX;
		ShowY = -ArrayY;
		OverlapX = LapX;
		OverlapY = LapY;
	}

	/// <summary>
	/// 初期化処理を行い、すべての要素に同一スプライトを設定します。
	/// </summary>
	protected override void Start()
	{
		base.Start();
		for (int i = 0; i < ArrayX * ArrayY; ++i)
			mComaInstance[i].GetComponent<SpriteRenderer>().sprite = mImageBuilder.Extract(0);
	}

	/// <summary>
	/// 毎フレーム呼び出され、カラーマップに従って各要素の色を更新します。
	/// </summary>
	protected override void Update()
	{
		int[][] matColor = GetColor();
		for (int i = 0; i < ArrayX * ArrayY; ++i)
		{
			int setColor = matColor[i % ArrayX][i / ArrayX];
			mComaInstance[i].GetComponent<SpriteRenderer>().color = new Color32(
				SlotEffectMaker2023.Data.ColorMapList.GetColorElem(setColor, SlotEffectMaker2023.Data.ColorMapElem.Red),
				SlotEffectMaker2023.Data.ColorMapList.GetColorElem(setColor, SlotEffectMaker2023.Data.ColorMapElem.Green),
				SlotEffectMaker2023.Data.ColorMapList.GetColorElem(setColor, SlotEffectMaker2023.Data.ColorMapElem.Blue),
				SlotEffectMaker2023.Data.ColorMapList.GetColorElem(setColor, SlotEffectMaker2023.Data.ColorMapElem.Alpha)
			);
		}
	}

	/// <summary>
	/// カラーマップに基づき頂点カラーを計算し、2次元配列で返します。
	/// </summary>
	/// <returns>
	/// [ArrayX][ArrayY]構造の頂点カラーインデックス配列。
	/// </returns>
	private int[][] GetColor()
	{
		// 戻り値データ型作成
		int[][] ans = new int[ArrayX][];
		for (int i = 0; i < ans.Length; ++i) ans[i] = new int[ArrayY];

		// 使用するカラーマップ定義の一覧を取得
		var useMap = new List<SlotEffectMaker2023.Data.ColorMapList>();
		var data = SlotEffectMaker2023.Singleton.EffectDataManagerSingleton.GetInstance();
		var act = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance();
		for (int i = 0; i < MapShifterName.Length; ++i)
		{
			if (MapShifterName[i] == null) continue;
			string nowMapName = act.colorMapData.ExportElemName(MapShifterName[i]);
			if (nowMapName == null) continue;
			var nowMap = data.ColorMap.GetMapList(nowMapName);
			var shifter = data.ColorMap.GetShifter(MapShifterName[i]);
			if (shifter == null) continue;
			var timer = act.timerData.GetTimer(shifter.UseTimerName);
			if (timer == null || !timer.isActivate) continue;
			nowMap.SetCard((float)timer.elapsedTime);
			useMap.Add(nowMap);
		}

		// 各要素の色を設定
		for (uint x = 0; x < ArrayX; ++x)
			for (uint y = 0; y < ArrayY; ++y)
			{
				int c = 0;
				foreach (var um in useMap)
				{
					c = SlotEffectMaker2023.Data.ColorMapList.ComboColor(um.GetColor(x, y), c);
				}
				ans[x][y] = c;
			}

		return ans;
	}
}