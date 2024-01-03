using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace SlotEffectMaker2023.Data
{
	public enum ColorMapElem
	{
		Red, Green, Blue, Alpha
	}

	public class ColorMap : SlotMaker2022.ILocalDataInterface
	{	// カラーマップアニメーションデータ(Sys,1データに複数画像入力可)
		public uint sizeW { get; private set; }   // カラーマップのWサイズ
		public uint sizeH { get; private set; }   // カラーマップのHサイズ
		public uint cardNum { get; private set; } // カラーマップの描画枚数
		public bool fadeFlag { get; set; }        // フェードアニメーション有無
		public uint loopCount { get; set; }       // 繰り返し回数
		public int  beginTime { get; set; }       // 再生開始時間[ms]

		List<uint> mapData; // マップデータ本体

		public ColorMap(uint w, uint h)
		{
			sizeW = w;
			sizeH = h;
			cardNum = 0;
			fadeFlag = false;
			loopCount = 0;
			beginTime = 0;
			mapData = new List<uint>();
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

			int mapSize = fs.ReadInt32();
			for (int i = 0; i < mapSize; ++i) mapData.Add(fs.ReadUInt32());
			return true;
		}

		// 関数
		public void AddMapData(List<uint> pAddMap)
		{
			if ((uint)pAddMap.Count % (sizeW * sizeH) != 0) return;
			mapData.AddRange(pAddMap);
			cardNum = (uint)mapData.Count / (sizeW * sizeH);
		}
		public uint GetMapData(uint card, uint y, uint x)
		{
			if (card >= cardNum) return 0u;
			if (y >= sizeH) return 0u;
			if (x >= sizeH) return 0u;

			// indexを計算して書き出し
			uint index = x;
			index += y * sizeW;
			index += card * sizeH * sizeW;
			return mapData[(int)index];
		}
		public byte GetMapDataElem(uint card, uint y, uint x, ColorMapElem color)
		{
			uint map = GetMapData(card, y, x);
			return (byte)(map >> (8 * (int)color) & 0xFF);
		}
		public void ClearMapData()
		{
			mapData.Clear();
			cardNum = 0u;
		}
	}

	public class ColorMapList : SlotMaker2022.ILocalDataInterface
	{	// カラーマップタイムラインデータ(Sys)
		// 変数
		public string useTimerName { get; set; } // 制御に使用するタイマ名
		public int loopTime { get; set; }        // ループ時間[ms]
		public uint sizeW { get; private set; }  // カラーマップのWサイズ
		public uint sizeH { get; private set; }  // カラーマップのHサイズ

		public List<ColorMap> elemData { get; set; }	// カラーマップアニメーションまとめ

		public ColorMapList(uint w, uint h)
		{
			sizeW = w;
			sizeH = h;
			useTimerName = string.Empty;
			loopTime = -1;
		}

		public bool StoreData(ref BinaryWriter fs, int version)
		{
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
			useTimerName = fs.ReadString();
			loopTime = fs.ReadInt32();
			sizeW = fs.ReadUInt32();
			sizeH = fs.ReadUInt32();

			int mapSize = fs.ReadInt32();
			for (int i = 0; i < mapSize; ++i)
			{
				ColorMap cm = new ColorMap(sizeW, sizeH);
				cm.ReadData(ref fs, version);
				elemData.Add(cm);
			}
			return true;
		}
	}
}