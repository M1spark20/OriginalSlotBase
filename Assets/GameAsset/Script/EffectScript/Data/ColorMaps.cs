using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace SlotEffectMaker2023.Data
{
	public enum ColorMapElem
	{
		Blue, Green, Red, Alpha, IdxMax
	}
	public enum ColorMapAccelation
    {	// アニメーションの変動方法(通常, 加速, 減速, なし)
		Steady, Acc, Dec, None
    }

	public class ColorMap : IEffectNameInterface
	{	// カラーマップアニメーションデータ(Sys,1データに複数画像入力可)
		public uint sizeW { get; private set; }   // カラーマップのWサイズ
		public uint sizeH { get; private set; }   // カラーマップのHサイズ
		public uint cardNum { get; private set; } // カラーマップの描画枚数
		public bool fadeFlag { get; set; }        // フェードアニメーション有無
		public uint loopCount { get; set; }       // 繰り返し回数
		public int  beginTime { get; set; }       // 再生開始時間[ms]
		public float scaleFactor { get; set; }    // 加速度制御値a [acc: t^a, dec: 1-(1-t)^a]
		public ColorMapAccelation speed { get; set; }	// アニメーション速度種類

		public List<int> mapData { get; private set; } // マップデータ本体(x + y*sizeW + card*sizeW*sizeH)

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

		// 保存関数
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
		public void Rename(EChangeNameType type, string src, string dst) { }

		// 関数
		public void SetSize(uint w, uint h)
        {
			if (mapData.Count > 0) return;
			sizeW = w; sizeH = h;
        }
		public void AddMapData(List<int> pAddMap)
		{
			if ((uint)pAddMap.Count % (sizeW * sizeH) != 0) return;
			mapData.AddRange(pAddMap);
			cardNum = (uint)mapData.Count / (sizeW * sizeH);
		}
		public int GetMapData(uint card, uint y, uint x)
		{
			if (card >= cardNum * loopCount) return 0;
			if (y >= sizeH) return 0;
			if (x >= sizeW) return 0;

			// indexを計算して書き出し
			uint index = x;
			index += y * sizeW;
			index += (card % loopCount) * sizeH * sizeW;
			return mapData[(int)index];
		}
		public float GetCardF(float timeEqualized)
        {	// 正規化した進捗を引数に取る
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
		public void ClearMapData()
		{
			mapData.Clear();
			cardNum = 0u;
		}
	}

	public class ColorMapList : IEffectNameInterface
	{   // カラーマップタイムラインデータ(Sys)
		// 変数
		public string dataName { get; set; }	 // カラーマップの名前
		public int loopTime { get; set; }        // ループ時間[ms]
		public uint sizeW { get; set; }			 // カラーマップのWサイズ
		public uint sizeH { get; set; }          // カラーマップのHサイズ

		// データ出力用関数(一時変数のため保存しない)
		private int   mRefDataIdx;	// 次の参照データ(参照なし時-1)
		private float mProgress;	// 現在のデータの進捗(0<=val<=1) val>0で次データあり確定
		private float mCardIDFloat;	// 現在表示するカードID
		private const float divMS = 1000f;

		public List<ColorMap> elemData { get; set; }	// カラーマップアニメーションまとめ

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
		public void Rename(EChangeNameType type, string src, string dst)
        {
			foreach (var cm in elemData) cm.Rename(type, src, dst);
        }

		// データ取得に必要な時間の計算を行って内部に保存する。(色取り出し前に呼び出すことで制御)
		public void SetCard(float currentTime)
        {	// 参照時間により取り出すカードの選択を行う
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
				if (calcTime >= elemData[mRefDataIdx].beginTime / divMS) break;

			// 使用カード算出(次のカードがない場合はmProgress, mCardIDFloatとも0)
			if (mRefDataIdx + 1 < elemData.Count)
            {
				float begTimeF = elemData[mRefDataIdx].beginTime / divMS;
				float nextTimeF = elemData[mRefDataIdx + 1].beginTime / divMS;
				mProgress = (calcTime - begTimeF) / (nextTimeF - begTimeF);    // テーブル進捗[0,1]
				mCardIDFloat = elemData[mRefDataIdx].GetCardF(mProgress);
			}
        }
		// 時間を引数に当該データの色を得る(AARRGGBB)
		public int GetColor(uint getX, uint getY)
        {   // 使用する定義の取り出しを行う
			// データが空 or 初期定義未達で0を返す
			if (elemData.Count == 0 || mRefDataIdx < 0) return 0;

			// 色情報の抽出を行う | 初期データ -> 最終データ時処理: 1枚目のデータのみを表示する
			int ans = elemData[mRefDataIdx].GetMapData(0, getY, getX);
			if (mProgress > 0f)
            {   // 初期・中間データの処理: 時間によって参照位置を変える
				// 選択カードの色を取り出す(fadeなしの場合)
				uint card = (uint)mCardIDFloat;
				ans = elemData[mRefDataIdx].GetMapData(card, getY, getX);

				if (elemData[mRefDataIdx].fadeFlag)
                {	// フェードありの処理を行う
					int nextC = elemData[mRefDataIdx + 1].GetMapData(0, getY, getX);  // 初期データ: 次データの頭
					// 現在データに次データがある場合は当該要素を読み込む
					if (card + 1 < elemData[mRefDataIdx].cardNum * elemData[mRefDataIdx].loopCount)
						nextC = elemData[mRefDataIdx + 1].GetMapData(card + 1, getY, getX);
					ans = GetMediumColor(ans, nextC, mCardIDFloat % 1f);
				}
			}

			return ans;
        }

		// 色の比率から中間色を生成する
		private int GetMediumColor(int nowColor, int nextColor, float progress)
        {
			float progInv = 1f - progress;
			int ans = 0;
			for (int c = 0; c < (int)ColorMapElem.IdxMax; ++c)
            {
				float cf = GetColorElem(nextColor, (ColorMapElem)c) * progress + GetColorElem(nowColor, (ColorMapElem)c) * progInv;
				ans |= (byte)cf >> (8 * c);
            }
			return 0;
        }
		// int型カラーから特定の色を抽出する(クラス名で呼出し可)
		public static byte GetColorElem(int color, ColorMapElem type)
        {
			return (byte)(color >> (8 * (int)type) & 0xFF);
		}
	}

	public class ColorMapShifter : DataShifterBase
    {
        protected override EChangeNameType GetMyType() { return EChangeNameType.ColorMap; }
    }

	public class ColorMapDataManager : IEffectNameInterface 
	{
		// 変数
		public List<ColorMapList> mapList { get; set; }
		public List<ColorMapShifter> shifter { get; set; }

        public ColorMapDataManager()
        {
			mapList = new List<ColorMapList>();
			shifter = new List<ColorMapShifter>();
        }
		public bool StoreData(ref BinaryWriter fs, int version)
        {
			fs.Write(mapList.Count);
			for (int i = 0; i < mapList.Count; ++i) mapList[i].StoreData(ref fs, version);
			fs.Write(shifter.Count);
			for (int i = 0; i < shifter.Count; ++i) shifter[i].StoreData(ref fs, version);
			return true;
        }
		public bool ReadData(ref BinaryReader fs, int version)
        {
			int sz = fs.ReadInt32();
			for(int i=0; i<sz; ++i)
            {
				ColorMapList cm = new ColorMapList();
				cm.ReadData(ref fs, version);
				mapList.Add(cm);
            }
			sz = fs.ReadInt32();
			for(int i=0; i<sz; ++i)
            {
				ColorMapShifter sf = new ColorMapShifter();
				sf.ReadData(ref fs, version);
				shifter.Add(sf);
            }
			return true;
        }
		public void Rename(EChangeNameType type, string src, string dst)
        {
			for (int i = 0; i < mapList.Count; ++i) mapList[i].Rename(type, src, dst);
			for (int i = 0; i < shifter.Count; ++i) shifter[i].Rename(type, src, dst);
        }

		// 一覧取得
		public string[] GetMapListName()
        {
			List<string> ans = new List<string>();
			foreach (var item in mapList) ans.Add(item.dataName);
			return ans.ToArray();
        }
		public string[] GetShifterName()
        {
			List<string> ans = new List<string>();
			foreach (var item in shifter) ans.Add(item.ShifterName);
			return ans.ToArray();
        }

		// 名前からデータ取得
		public ColorMapList GetMapList(string name)
        {
			foreach (var item in mapList)
				if (item.dataName.Equals(name)) return item;
			return null;
        }
		public ColorMapShifter GetShifter(string name)
        {
			foreach (var item in shifter)
				if (item.ShifterName.Equals(name)) return item;
			return null;
        }
	}
}