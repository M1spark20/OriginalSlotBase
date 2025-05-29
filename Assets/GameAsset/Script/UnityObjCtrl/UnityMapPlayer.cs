using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// スプライトを用いたマップ表示を行うコンポーネント。
/// MapSizeX×MapSizeYのスプライトを並べ、カラーマップに従って色変更します。
/// </summary>
public class UnityMapPlayer : IMultiImageWriter
{
	/// <summary>
	/// マップの水平サイズ（列数）。
	/// </summary>
	[SerializeField, Min(1)] protected int MapSizeX;
	/// <summary>
	/// マップの垂直サイズ（行数）。
	/// </summary>
	[SerializeField, Min(1)] protected int MapSizeY;
	/// <summary>
	/// リール用カラーマップシフタ名の配列。
	/// </summary>
	[SerializeField] private string[] MapShifterName;

	/// <summary>
	/// 画像分割と配置パラメータを初期化します。
	/// </summary>
	protected override void InitDivision()
	{
		DivX = MapSizeX;
		DivY = MapSizeY;
		CutWayX = true;
		ShowX = MapSizeX;
		ShowY = -MapSizeY;
		OverlapX = 0f;
		OverlapY = 0f;
	}

	/// <summary>
	/// 初期化処理を行い、全スプライトを取得して表示に設定します。
	/// </summary>
	protected override void Start()
	{
		base.Start();
		for (int i = 0; i < MapSizeX * MapSizeY; ++i)
			mComaInstance[i].GetComponent<SpriteRenderer>().sprite = mImageBuilder.Extract(i);
	}

	/// <summary>
	/// 毎フレーム呼び出され、カラーマップに応じた色変更を行います。
	/// </summary>
	protected override void Update()
	{
		int[][] matColor = GetColor();
		for (int i = 0; i < MapSizeX * MapSizeY; ++i)
		{
			int setColor = matColor[i % MapSizeX][i / MapSizeX];
			mComaInstance[i].GetComponent<SpriteRenderer>().color = new Color32(
				SlotEffectMaker2023.Data.ColorMapList.GetColorElem(setColor, SlotEffectMaker2023.Data.ColorMapElem.Red),
				SlotEffectMaker2023.Data.ColorMapList.GetColorElem(setColor, SlotEffectMaker2023.Data.ColorMapElem.Green),
				SlotEffectMaker2023.Data.ColorMapList.GetColorElem(setColor, SlotEffectMaker2023.Data.ColorMapElem.Blue),
				SlotEffectMaker2023.Data.ColorMapList.GetColorElem(setColor, SlotEffectMaker2023.Data.ColorMapElem.Alpha)
			);
		}
	}

	/// <summary>
	/// カラーマップに基づき頂点カラーを計算して配列で返します。
	/// </summary>
	/// <returns>MapSizeX×MapSizeYの頂点カラーインデックス配列。</returns>
	private int[][] GetColor()
	{
		// 戻り値データ型作成
		int[][] ans = new int[MapSizeX][];
		for (int i = 0; i < ans.Length; ++i) ans[i] = new int[MapSizeY];

		// 使用するカラーマップ定義の一覧を取得する
		var useMap = new List<SlotEffectMaker2023.Data.ColorMapList>();
		var data = SlotEffectMaker2023.Singleton.EffectDataManagerSingleton.GetInstance();
		var act = SlotEffectMaker2023.Singleton.SlotDataSingleton.GetInstance();
		for (int i = 0; i < MapShifterName.Length; ++i)
		{
			if (MapShifterName[i] == null) continue;

			// 現在のMapデータを呼び出す
			string nowMapName = act.colorMapData.ExportElemName(MapShifterName[i]);
			if (nowMapName == null) continue;
			var nowMap = data.ColorMap.GetMapList(nowMapName);

			// タイマを参照して、有効ならデータをセットする
			var shifter = data.ColorMap.GetShifter(MapShifterName[i]);
			if (shifter == null) continue;
			var timer = act.timerData.GetTimer(shifter.UseTimerName);
			if (timer == null || !timer.isActivate) continue;
			nowMap.SetCard((float)timer.elapsedTime);
			useMap.Add(nowMap);
		}

		// 各コマの色を設定する
		for (uint x = 0; x < MapSizeX; ++x)
			for (uint y = 0; y < MapSizeY; ++y)
			{
				int c = 0;
				for (int mapC = 0; mapC < useMap.Count; ++mapC)
				{
					int srcColor = useMap[mapC].GetColor(x, y);
					c = SlotEffectMaker2023.Data.ColorMapList.ComboColor(srcColor, c);
				}
				ans[(int)x][(int)y] = c;
			}

		return ans;
	}
}
