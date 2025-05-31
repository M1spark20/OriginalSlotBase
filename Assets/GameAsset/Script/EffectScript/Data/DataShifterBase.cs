using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SlotEffectMaker2023.Data
{
	/// <summary>
	/// タイマーに紐づいて要素IDを時間経過で切り替える基底クラスです。
	/// </summary>
	public abstract class DataShifterBase : IEffectNameInterface
	{
		/// <summary>時間計算時の分母（ミリ秒→秒変換用）</summary>
		public const float TIME_DIV = 1000f;

		/// <summary>切り替え対象のシフター名。デフォルトタイマもこの名前で生成されます。</summary>
		public string ShifterName { get; set; }

		/// <summary>制御に使用するタイマー名</summary>
		public string UseTimerName { get; set; }

		/// <summary>鳴動開始時間（ミリ秒）</summary>
		public int BeginTime { get; set; }

		/// <summary>鳴動終了時間（ミリ秒）（※UseTimer基準）</summary>
		public int StopTime { get; set; }

		/// <summary>デフォルトの要素ID（外部から変更可能）</summary>
		public string DefaultElemID { get; set; }

		// 内部用：このクラスで扱う名前変更タイプ
		private readonly EChangeNameType MyType;

		/// <summary>
		/// サブクラスごとに名前変更時の対象タイプを返します。
		/// </summary>
		/// <returns>このシフターの EChangeNameType</returns>
		protected abstract EChangeNameType GetMyType();

		/// <summary>
		/// デフォルトコンストラクタ。各プロパティを初期化します。
		/// </summary>
		public DataShifterBase()
		{
			ShifterName = string.Empty;
			UseTimerName = string.Empty;
			BeginTime = 0;
			StopTime = -1;
			DefaultElemID = string.Empty;
			MyType = GetMyType();
		}

		/// <summary>
		/// このインスタンスの内容をバイナリに書き込みます。
		/// </summary>
		/// <param name="fs">書き込み先の BinaryWriter（ref）</param>
		/// <param name="version">データのバージョン</param>
		/// <returns>書き込み成功時に true を返します。</returns>
		public bool StoreData(ref BinaryWriter fs, int version)
		{
			fs.Write(ShifterName);
			fs.Write(UseTimerName);
			fs.Write(BeginTime);
			fs.Write(StopTime);
			fs.Write(DefaultElemID);
			return true;
		}

		/// <summary>
		/// バイナリからこのインスタンスの内容を読み込みます。
		/// </summary>
		/// <param name="fs">読み込み元の BinaryReader（ref）</param>
		/// <param name="version">データのバージョン</param>
		/// <returns>読み込み成功時に true を返します。</returns>
		public bool ReadData(ref BinaryReader fs, int version)
		{
			ShifterName = fs.ReadString();
			UseTimerName = fs.ReadString();
			BeginTime = fs.ReadInt32();
			StopTime = fs.ReadInt32();
			DefaultElemID = fs.ReadString();
			return true;
		}

		/// <summary>
		/// 名前変更時に、タイマー名または要素IDを更新します。
		/// </summary>
		/// <param name="type">変更の種類（タイマー or MyType）</param>
		/// <param name="src">元の名前</param>
		/// <param name="dst">新しい名前</param>
		public void Rename(EChangeNameType type, string src, string dst)
		{
			if (type == EChangeNameType.Timer && UseTimerName.Equals(src))
				UseTimerName = dst;

			if (type == MyType && DefaultElemID.Equals(src))
				DefaultElemID = dst;
		}
	}
}
