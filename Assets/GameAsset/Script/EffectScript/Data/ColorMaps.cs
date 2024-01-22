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
			if (card >= cardNum) return 0;
			if (y >= sizeH) return 0;
			if (x >= sizeW) return 0;

			// indexを計算して書き出し
			uint index = x;
			index += y * sizeW;
			index += card * sizeH * sizeW;
			return mapData[(int)index];
		}
		public byte GetMapDataElem(uint card, uint y, uint x, ColorMapElem color)
		{
			int map = GetMapData(card, y, x);
			return (byte)(map >> (8 * (int)color) & 0xFF);
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
		public string useTimerName { get; set; } // 制御に使用するタイマ名
		public int loopTime { get; set; }        // ループ時間[ms]
		public uint sizeW { get; private set; }  // カラーマップのWサイズ
		public uint sizeH { get; private set; }  // カラーマップのHサイズ

		public List<ColorMap> elemData { get; set; }	// カラーマップアニメーションまとめ

		public ColorMapList()
		{
			dataName = string.Empty;
			sizeW = 0;
			sizeH = 0;
			useTimerName = string.Empty;
			loopTime = -1;
		}

		public bool StoreData(ref BinaryWriter fs, int version)
		{
			fs.Write(dataName);
			fs.Write(useTimerName);
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
			useTimerName = fs.ReadString();
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
			if (type == EChangeNameType.Timer && useTimerName.Equals(src)) useTimerName = dst;
			foreach (var cm in elemData) cm.Rename(type, src, dst);
        }
	}
}