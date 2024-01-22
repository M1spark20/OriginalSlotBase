using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SlotEffectMaker2023.Data
{
    public abstract class DataShifterBase : IEffectNameInterface
    {
        public const float TIME_DIV = 1000f;

		// 変数
		public string ShifterName { get; set; }  // シフター名。デフォルトタイマ名がこの名前で生成される

		public string UseTimerName { get; set; }    // 制御に使用するタイマー名
		public int BeginTime { get; set; }  // 鳴動開始時間[ms]
		public int StopTime { get; set; }   // 鳴動終了時間[ms] (※UseTimer基準)
		public string DefaultElemID { get; set; } // デフォルトの要素ID: 外部から変更可能

		// 実装要素
		private readonly EChangeNameType MyType;
		protected abstract EChangeNameType GetMyType();

        public DataShifterBase()
        {
			ShifterName = string.Empty;
			UseTimerName = string.Empty;
			BeginTime = 0;
			StopTime = -1;
			DefaultElemID = string.Empty;
			MyType = GetMyType();
		}
		public bool StoreData(ref BinaryWriter fs, int version)
		{
			fs.Write(ShifterName);
			fs.Write(UseTimerName);
			fs.Write(BeginTime);
			fs.Write(StopTime);
			fs.Write(DefaultElemID);
			return true;
		}
		public bool ReadData(ref BinaryReader fs, int version)
		{
			ShifterName = fs.ReadString();
			UseTimerName = fs.ReadString();
			BeginTime = fs.ReadInt32();
			StopTime = fs.ReadInt32();
			DefaultElemID = fs.ReadString();
			return true;
		}
		public void Rename(EChangeNameType type, string src, string dst)
		{
			if (type == EChangeNameType.Timer && UseTimerName.Equals(src)) UseTimerName = dst;
			if (type == MyType && DefaultElemID.Equals(src)) DefaultElemID = dst;
		}
	}
}
