using System.Collections.Generic;
using System.IO;

namespace SlotEffectMaker2023.Data
{
	// サウンドID情報。IDにより管理する(Sys)
	public class SoundID : IEffectNameInterface
	{
		public string DataName { get; set; }    // サウンド定義名
		public string ShotResName { get; set; } // 単音 or イントロ音源のリソース名
		public string LoopResName { get; set; } // ループ音源のリソース名
		public int LoopBegin { get; set; }  // ループ音源の開始遅延時間[ms]

		public SoundID()
		{
			DataName = string.Empty;
			ShotResName = string.Empty;
			LoopResName = string.Empty;
			LoopBegin = -1;
		}

		public bool StoreData(ref BinaryWriter fs, int version)
		{
			fs.Write(DataName);
			fs.Write(ShotResName);
			fs.Write(LoopResName);
			fs.Write(LoopBegin);
			return true;
		}
		public bool ReadData(ref BinaryReader fs, int version)
		{
			DataName = fs.ReadString();
			ShotResName = fs.ReadString();
			LoopResName = fs.ReadString();
			LoopBegin = fs.ReadInt32();
			return true;
		}
		public void Rename(EChangeNameType type, string src, string dst) { }
	}

	// 音を鳴らす単体データ(Sys)
	public class SoundPlayData : DataShifterBase
	{
		// 定数
		const string SHOT_HEADER = "#SS_";
		const string LOOP_HEADER = "#SL_";

        // 実装要素
        protected override EChangeNameType GetMyType() { return EChangeNameType.SoundID; }

        // タイマ名を取得する
        public string GetShotTimerName() { return SHOT_HEADER + ShifterName; }
		public string GetLoopTimerName() { return LOOP_HEADER + ShifterName; }
	}
}
