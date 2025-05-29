using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace SlotEffectMaker2023.Data
{
	/// <summary>
	/// カラーマップ要素のインデックスを表す列挙型。
	/// </summary>
	public enum ColorMapElem
	{
		Blue,
		Green,
		Red,
		Alpha,
		IdxMax
	}

	/// <summary>
	/// アニメーション変動方法を表す列挙型。
	/// </summary>
	public enum ColorMapAccelation
	{   // アニメーションの変動方法(通常, 加速, 減速, なし)
		Steady,
		Acc,
		Dec,
		None
	}

	/// <summary>
	/// カラーマップアニメーションデータを管理するクラス。
	/// 複数のカラー画像フレームを保持し、再生制御を行います。
	/// </summary>
	public class ColorMap : IEffectNameInterface
	{
		// カラーマップアニメーションデータ(Sys,1データに複数画像入力可)
		public uint sizeW { get; private set; }   // カラーマップのWサイズ
		public uint sizeH { get; private set; }   // カラーマップのHサイズ
		public uint cardNum { get; private set; } // カラーマップの描画枚数
		public bool fadeFlag { get; set; }        // フェードアニメーション有無
		public uint loopCount { get; set; }       // 繰り返し回数
		public int beginTime { get; set; }       // 再生開始時間[ms]
		public float scaleFactor { get; set; }    // 加速度制御値a [acc: t^a, dec: 1-(1-t)^a]
		public ColorMapAccelation speed { get; set; }  // アニメーション速度種類

		public List<int> mapData { get; private set; } // マップデータ本体(x + y*sizeW + card*sizeW*sizeH)

		/// <summary>
		/// コンストラクタ。デフォルト値で初期化し、データリストを作成します。
		/// </summary>
		public ColorMap()
		{
			sizeW = uint.MaxValue;
			sizeH = uint.MaxValue;
			cardNum = 0;
			fadeFlag = false;
			loopCount = 1;
			beginTime = 0;
			scaleFactor = 2f;
			speed = ColorMapAccelation.Steady;
			mapData = new List<int>();
		}

		/// <summary>
		/// カラーマップデータをバイナリ形式で保存します。
		/// </summary>
		/// <param name="fs">BinaryWriter の参照</param>
		/// <param name="version">保存バージョン</param>
		/// <returns>保存処理が成功したか（常に true）</returns>
		public bool StoreData(ref BinaryWriter fs, int version)
		{
			fs.Write(sizeW);
			fs.Write(sizeH);
			fs.Write(cardNum);
			fs.Write(fadeFlag);
			fs.Write(loopCount);
			fs.Write(beginTime);
			fs.Write(scaleFactor);
			fs.Write((byte)speed);

			int mapSize = mapData.Count;
			fs.Write(mapSize);
			for (int i = 0; i < mapSize; ++i) fs.Write(mapData[i]);
			return true;
		}

		/// <summary>
		/// バイナリ形式からカラーマップデータを読み込みます。
		/// </summary>
		/// <param name="fs">BinaryReader の参照</param>
		/// <param name="version">保存バージョン</param>
		/// <returns>読み込み処理が成功したか（常に true）</returns>
		public bool ReadData(ref BinaryReader fs, int version)
		{
			sizeW = fs.ReadUInt32();
			sizeH = fs.ReadUInt32();
			cardNum = fs.ReadUInt32();
			fadeFlag = fs.ReadBoolean();
			loopCount = fs.ReadUInt32();
			beginTime = fs.ReadInt32();
			scaleFactor = fs.ReadSingle();
			speed = (ColorMapAccelation)fs.ReadByte();

			int mapSize = fs.ReadInt32();
			for (int i = 0; i < mapSize; ++i) mapData.Add(fs.ReadInt32());
			return true;
		}

		/// <summary>
		/// 効果名変更イベントに応じて名前を更新します（未実装）。
		/// </summary>
		/// <param name="type">変更タイプ</param>
		/// <param name="src">元の名前</param>
		/// <param name="dst">新しい名前</param>
		public void Rename(EChangeNameType type, string src, string dst) { }

		/// <summary>
		/// カラーマップのサイズを設定します。既存データがある場合は変更しません。
		/// </summary>
		/// <param name="w">幅</param>
		/// <param name="h">高さ</param>
		public void SetSize(uint w, uint h)
		{
			if (mapData.Count > 0) return;
			sizeW = w; sizeH = h;
		}

		/// <summary>
		/// マップデータを追加し、カード数を更新します。
		/// </summary>
		/// <param name="pAddMap">追加するマップデータリスト</param>
		public void AddMapData(List<int> pAddMap)
		{
			if ((uint)pAddMap.Count % (sizeW * sizeH) != 0) return;
			mapData.AddRange(pAddMap);
			cardNum = (uint)mapData.Count / (sizeW * sizeH);
		}

		/// <summary>
		/// 特定のカード・位置の色データを取得します。
		/// </summary>
		/// <param name="card">カードインデックス</param>
		/// <param name="y">Y座標</param>
		/// <param name="x">X座標</param>
		/// <returns>カラー値、範囲外は 0</returns>
		public int GetMapData(uint card, uint y, uint x)
		{
			if (card >= cardNum * loopCount) return 0;
			if (y >= sizeH) return 0;
			if (x >= sizeW) return 0;

			// indexを計算して書き出し
			uint index = x;
			index += y * sizeW;
			index += (card % cardNum) * sizeH * sizeW;
			return mapData[(int)index];
		}

		/// <summary>
		/// 正規化進行率からカード位置を計算します。
		/// </summary>
		/// <param name="timeEqualized">正規化した進捗 [0,1]</param>
		/// <returns>計算されたカード位置(浮動小数点)</returns>
		public float GetCardF(float timeEqualized)
		{
			// 正規化した進捗を引数に取る
			// 数値チェック
			if (timeEqualized < 0f || timeEqualized > 1f) return 0f;

			uint cardTotal = cardNum * loopCount;
			// 定速
			if (speed == ColorMapAccelation.Steady) return cardTotal * timeEqualized;
			// 加速
			if (speed == ColorMapAccelation.Acc) return cardTotal * UnityEngine.Mathf.Pow(timeEqualized, scaleFactor);
			// 減速
			if (speed == ColorMapAccelation.Dec) return cardTotal * (1f - UnityEngine.Mathf.Pow(1f - timeEqualized, scaleFactor));

			return 0f;
		}

		/// <summary>
		/// マップデータをクリアし、カード数をリセットします。
		/// </summary>
		public void ClearMapData()
		{
			mapData.Clear();
			cardNum = 0u;
		}
	}

	/// <summary>
	/// カラーマップリストの再生制御を管理するクラス。
	/// タイムライン再生、色取得などの機能を提供します。
	/// </summary>
	public class ColorMapList : IEffectNameInterface
	{   // カラーマップタイムラインデータ(Sys)
		// 変数
		public string dataName { get; set; }     // カラーマップの名前
		public int loopTime { get; set; }       // ループ時間[ms]
		public uint sizeW { get; set; }      // カラーマップのWサイズ
		public uint sizeH { get; set; }       // カラーマップのHサイズ

		// データ出力用関数(一時変数のため保存しない)
		private int mRefDataIdx;    // 次の参照データ(参照なし時-1)
		private float mProgress;    // 現在のデータの進捗(0<=val<=1) val>0で次データあり確定
		private float mCardIDFloat; // 現在表示するカードID
		private const float divMS = 1000f;

		public List<ColorMap> elemData { get; set; }    // カラーマップアニメーションまとめ

		/// <summary>
		/// コンストラクタ。初期値を設定し、内部リストを初期化します。
		/// </summary>
		public ColorMapList()
		{
			dataName = string.Empty;
			sizeW = 1;
			sizeH = 1;
			loopTime = -1;
			elemData = new List<ColorMap>();
			// プライベートメンバ
			mRefDataIdx = -1;
			mProgress = 0;
			mCardIDFloat = 0;
		}

		/// <summary>
		/// カラーマップリストをバイナリ形式で保存します。
		/// </summary>
		/// <param name="fs">BinaryWriter の参照</param>
		/// <param name="version">保存バージョン</param>
		/// <returns>保存に成功したか（常に true）</returns>
		public bool StoreData(ref BinaryWriter fs, int version)
		{
			fs.Write(dataName);
			fs.Write(loopTime);
			fs.Write(sizeW);
			fs.Write(sizeH);

			int elemSize = elemData.Count;
			fs.Write(elemSize);
			for (int i = 0; i < elemSize; ++i) elemData[i].StoreData(ref fs, version);
			return true;
		}

		/// <summary>
		/// バイナリ形式からカラーマップリストを読み込みます。
		/// </summary>
		/// <param name="fs">BinaryReader の参照</param>
		/// <param name="version">保存バージョン</param>
		/// <returns>読み込みに成功したか（常に true）</returns>
		public bool ReadData(ref BinaryReader fs, int version)
		{
			dataName = fs.ReadString();
			loopTime = fs.ReadInt32();
			sizeW = fs.ReadUInt32();
			sizeH = fs.ReadUInt32();

			int mapSize = fs.ReadInt32();
			for (int i = 0; i < mapSize; ++i)
			{
				ColorMap cm = new ColorMap();
				cm.ReadData(ref fs, version);
				if (cm.sizeW != sizeW || cm.sizeH != sizeH) return false;
				elemData.Add(cm);
			}
			return true;
		}

		/// <summary>
		/// 効果名変更イベントに応じて内部要素の名前を更新します。
		/// </summary>
		/// <param name="type">変更タイプ</param>
		/// <param name="src">元の名前</param>
		/// <param name="dst">新しい名前</param>
		public void Rename(EChangeNameType type, string src, string dst)
		{
			foreach (var cm in elemData) cm.Rename(type, src, dst);
		}

		/// <summary>
		/// 現在時刻に応じたカード位置を計算し、内部状態を更新します。
		/// </summary>
		/// <param name="currentTime">現在時刻[ms]</param>
		public void SetCard(float currentTime)
		{
			// 参照時間により取り出すカードの選択を行う
			// データ初期化
			mRefDataIdx = -1;
			mProgress = 0f;
			mCardIDFloat = 0f;
			if (elemData.Count == 0) return;

			// 計算時間算出
			float calcTime = currentTime;
			float loopTimeF = loopTime / divMS;
			int lastDataTime = elemData[elemData.Count - 1].beginTime;
			if (currentTime > lastDataTime / divMS)
			{
				if (loopTime < 0) return;
				else if (loopTime >= lastDataTime) calcTime = loopTimeF;
				else
				{
					float loopSize = (lastDataTime - loopTime) / divMS;
					calcTime = ((currentTime - loopTimeF) % loopSize) + loopTimeF;
				}
			}

			// 使用データ算出(mRefDataIdx - 0から走査)
			if (calcTime < 0f || calcTime < elemData[0].beginTime / divMS) return;
			for (mRefDataIdx = 0; mRefDataIdx < elemData.Count; ++mRefDataIdx)
			{
				if (mRefDataIdx + 1 >= elemData.Count) break;
				if (calcTime < elemData[mRefDataIdx + 1].beginTime / divMS) break;
			}

			// 使用カード算出
			if (mRefDataIdx + 1 < elemData.Count)
			{
				float begTimeF = elemData[mRefDataIdx].beginTime / divMS;
				float nextTimeF = elemData[mRefDataIdx + 1].beginTime / divMS;
				mProgress = (calcTime - begTimeF) / (nextTimeF - begTimeF);
				mCardIDFloat = elemData[mRefDataIdx].GetCardF(mProgress);
			}
		}

		/// <summary>
		/// 現在参照中のカラーを取得します。
		/// </summary>
		/// <param name="getX">取得X座標</param>
		/// <param name="getY">取得Y座標</param>
		/// <returns>AARRGGBB形式のカラー</returns>
		public int GetColor(uint getX, uint getY)
		{   // 使用する定義の取り出しを行う
			// データが空 or 初期定義未達で0を返す
			if (elemData.Count == 0 || mRefDataIdx < 0) return 0;

			// 色情報の抽出を行う | 初期データ -> 最終データ時処理: 1枚目のデータのみを表示する
			int ans = elemData[mRefDataIdx].GetMapData(0, getY, getX);
			if (mProgress > 0f)
			{   // 初期・中間データの処理: 時間によって参照位置を変える
				uint card = (uint)mCardIDFloat;
				ans = card < elemData[mRefDataIdx].cardNum * elemData[mRefDataIdx].loopCount ?
					elemData[mRefDataIdx].GetMapData(card, getY, getX) : elemData[mRefDataIdx + 1].GetMapData(0, getY, getX);

				if (elemData[mRefDataIdx].fadeFlag)
				{
					// フェードありの処理を行う
					int nextC = elemData[mRefDataIdx + 1].GetMapData(0, getY, getX);
					if (card + 1 < elemData[mRefDataIdx].cardNum * elemData[mRefDataIdx].loopCount)
						nextC = elemData[mRefDataIdx].GetMapData(card + 1, getY, getX);
					ans = GetMediumColor(ans, nextC, mCardIDFloat % 1f);
				}
			}

			return ans;
		}

		/// <summary>
		/// progress比率に応じた中間色を計算します。
		/// </summary>
		/// <param name="nowColor">現在のカラー</param>
		/// <param name="nextColor">次のカラー</param>
		/// <param name="progress">進行率(0-1)</param>
		/// <returns>合成後のカラー</returns>
		private int GetMediumColor(int nowColor, int nextColor, float progress)
		{
			float progInv = 1f - progress;
			int ans = 0;
			for (int c = 0; c < (int)ColorMapElem.IdxMax; ++c)
			{
				float cf = GetColorElem(nextColor, (ColorMapElem)c) * progress
						 + GetColorElem(nowColor, (ColorMapElem)c) * progInv;
				ans |= (int)cf << (8 * c);
			}
			return ans;
		}

		/// <summary>
		/// カラーコードから特定要素を抽出します。
		/// </summary>
		/// <param name="color">AARRGGBBカラー</param>
		/// <param name="type">抽出要素</param>
		/// <returns>要素値(0-255)</returns>
		public static byte GetColorElem(int color, ColorMapElem type)
		{
			return (byte)(color >> (8 * (int)type) & 0xFF);
		}

		/// <summary>
		/// 2つのカラーをアルファブレンド合成します。
		/// </summary>
		/// <param name="src">上書き色（ソース）</param>
		/// <param name="dst">被覆色（デスティネーション）</param>
		/// <returns>合成後のカラー</returns>
		public static int ComboColor(int src, int dst)
		{
			float srcA = GetColorElem(src, ColorMapElem.Alpha) / (float)byte.MaxValue;
			float dstA = GetColorElem(dst, ColorMapElem.Alpha) / (float)byte.MaxValue;

			// 色の計算
			float outA = srcA + dstA * (1f - srcA);
			int ans = 0;
			for (int c = 0; c < (int)ColorMapElem.IdxMax; ++c)
			{
				byte val = (byte)((GetColorElem(src, (ColorMapElem)c) * srcA
								 + GetColorElem(dst, (ColorMapElem)c) * dstA * (1f - srcA)) / outA);
				if ((ColorMapElem)c == ColorMapElem.Alpha) val = (byte)(outA * byte.MaxValue);
				ans |= val << (8 * c);
			}
			return ans;
		}
	}

	/// <summary>
	/// ColorMap データの名前シフタを定義するクラス。
	/// </summary>
	public class ColorMapShifter : DataShifterBase
	{
		protected override EChangeNameType GetMyType() { return EChangeNameType.ColorMap; }
	}

	/// <summary>
	/// ColorMapList と ColorMapShifter をまとめて管理するデータクラス。
	/// </summary>
	public class ColorMapDataManager : IEffectNameInterface
	{
		// 変数
		public List<ColorMapList> mapList { get; set; }
		public List<ColorMapShifter> shifter { get; set; }

		/// <summary>
		/// コンストラクタ。内部リストを初期化します。
		/// </summary>
		public ColorMapDataManager()
		{
			mapList = new List<ColorMapList>();
			shifter = new List<ColorMapShifter>();
		}

		/// <summary>
		/// データをバイナリ形式で保存します。
		/// </summary>
		/// <param name="fs">BinaryWriter の参照</param>
		/// <param name="version">保存バージョン</param>
		/// <returns>保存に成功したか（常に true）</returns>
		public bool StoreData(ref BinaryWriter fs, int version)
		{
			fs.Write(mapList.Count);
			for (int i = 0; i < mapList.Count; ++i) mapList[i].StoreData(ref fs, version);
			fs.Write(shifter.Count);
			for (int i = 0; i < shifter.Count; ++i) shifter[i].StoreData(ref fs, version);
			return true;
		}

		/// <summary>
		/// バイナリ形式からデータを読み込みます。
		/// </summary>
		/// <param name="fs">BinaryReader の参照</param>
		/// <param name="version">保存バージョン</param>
		/// <returns>読み込みが成功したか（常に true）</returns>
		public bool ReadData(ref BinaryReader fs, int version)
		{
			int sz = fs.ReadInt32();
			for (int i = 0; i < sz; ++i)
			{
				ColorMapList cm = new ColorMapList();
				cm.ReadData(ref fs, version);
				mapList.Add(cm);
			}
			sz = fs.ReadInt32();
			for (int i = 0; i < sz; ++i)
			{
				ColorMapShifter sf = new ColorMapShifter();
				sf.ReadData(ref fs, version);
				shifter.Add(sf);
			}
			return true;
		}

		/// <summary>
		/// 効果名変更イベントに応じて内部データの名前を更新します。
		/// </summary>
		/// <param name="type">変更タイプ</param>
		/// <param name="src">元の名前</param>
		/// <param name="dst">新しい名前</param>
		public void Rename(EChangeNameType type, string src, string dst)
		{
			for (int i = 0; i < mapList.Count; ++i) mapList[i].Rename(type, src, dst);
			for (int i = 0; i < shifter.Count; ++i) shifter[i].Rename(type, src, dst);
		}

		/// <summary>
		/// 全てのマップリスト名を配列で取得します。
		/// </summary>
		/// <returns>マップリスト名の配列</returns>
		public string[] GetMapListName()
		{
			List<string> ans = new List<string>();
			foreach (var item in mapList) ans.Add(item.dataName);
			return ans.ToArray();
		}

		/// <summary>
		/// 全てのシフタ名を配列で取得します。
		/// </summary>
		/// <returns>シフタ名の配列</returns>
		public string[] GetShifterName()
		{
			List<string> ans = new List<string>();
			foreach (var item in shifter) ans.Add(item.ShifterName);
			return ans.ToArray();
		}

		/// <summary>
		/// 指定した名前のマップリストを取得します。
		/// </summary>
		/// <param name="name">マップリスト名</param>
		/// <returns>該当する ColorMapList、存在しない場合は null</returns>
		public ColorMapList GetMapList(string name)
		{
			foreach (var item in mapList)
				if (item.dataName.Equals(name)) return item;
			return null;
		}

		/// <summary>
		/// 指定した名前のシフタを取得します。
		/// </summary>
		/// <param name="name">シフタ名</param>
		/// <returns>該当する ColorMapShifter、存在しない場合は null</returns>
		public ColorMapShifter GetShifter(string name)
		{
			foreach (var item in shifter)
				if (item.ShifterName.Equals(name)) return item;
			return null;
		}
	}
}
