using System.Collections.Generic;
using System.IO;

namespace SlotEffectMaker2023.Data
{
	// サウンドID情報。IDにより管理する(Sys)
	public class SoundID : SlotMaker2022.ILocalDataInterface
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
	}

	// 音を鳴らす単体データ(Sys)
	public class SoundPlayData : SlotMaker2022.ILocalDataInterface
	{
		// 定数
		public const float TIME_DIV = 1000f;
		const string SHOT_HEADER = "#SS_";
		const string LOOP_HEADER = "#SL_";

		// 変数
		public string PlayerName { get; set; }  // プレイヤー名。デフォルトタイマ名がこの名前で生成される

		public string UseTimerName { get; set; }    // 制御に使用するタイマー名
		public int BeginTime { get; set; }  // 鳴動開始時間[ms]
		public int StopTime { get; set; }   // 鳴動終了時間[ms] (※UseTimer基準)
		public string DefaultSoundID { get; set; } // デフォルトで鳴らすサウンドのID: 外部から変更可能

		public SoundPlayData()
		{
			PlayerName = string.Empty;
			UseTimerName = string.Empty;
			BeginTime = 0;
			StopTime = -1;
			DefaultSoundID = string.Empty;
		}

		public bool StoreData(ref BinaryWriter fs, int version)
		{
			fs.Write(PlayerName);
			fs.Write(UseTimerName);
			fs.Write(BeginTime);
			fs.Write(StopTime);
			fs.Write(DefaultSoundID);
			return true;
		}
		public bool ReadData(ref BinaryReader fs, int version)
		{
			PlayerName = fs.ReadString();
			UseTimerName = fs.ReadString();
			BeginTime = fs.ReadInt32();
			StopTime = fs.ReadInt32();
			DefaultSoundID = fs.ReadString();
			return true;
		}

		// タイマ名を取得する
		public string GetShotTimerName() { return SHOT_HEADER + PlayerName; }
		public string GetLoopTimerName() { return LOOP_HEADER + PlayerName; }
	}
}
