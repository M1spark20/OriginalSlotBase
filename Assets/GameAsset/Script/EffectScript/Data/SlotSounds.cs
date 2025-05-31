using System.Collections.Generic;
using System.IO;

namespace SlotEffectMaker2023.Data
{
	/// <summary>
	/// サウンドID情報を保持するクラス。IDによりサウンド定義を管理します。（Sys）
	/// </summary>
	public class SoundID : IEffectNameInterface
	{
		/// <summary>サウンド定義名</summary>
		public string DataName { get; set; }

		/// <summary>単音またはイントロ音源のリソース名</summary>
		public string ShotResName { get; set; }

		/// <summary>ループ音源のリソース名</summary>
		public string LoopResName { get; set; }

		/// <summary>ループ音源の開始遅延時間（ミリ秒）</summary>
		public int LoopBegin { get; set; }

		/// <summary>デフォルトコンストラクタ。プロパティを初期化します。</summary>
		public SoundID()
		{
			DataName = string.Empty;
			ShotResName = string.Empty;
			LoopResName = string.Empty;
			LoopBegin = -1;
		}

		/// <summary>このインスタンスの内容をバイナリに書き込みます。</summary>
		/// <param name="fs">書き込み先の BinaryWriter（ref）</param>
		/// <param name="version">データのバージョン</param>
		/// <returns>書き込み成功時に true を返します。</returns>
		public bool StoreData(ref BinaryWriter fs, int version)
		{
			fs.Write(DataName);
			fs.Write(ShotResName);
			fs.Write(LoopResName);
			fs.Write(LoopBegin);
			return true;
		}

		/// <summary>バイナリからこのインスタンスの内容を読み込みます。</summary>
		/// <param name="fs">読み込み元の BinaryReader（ref）</param>
		/// <param name="version">データのバージョン</param>
		/// <returns>読み込み成功時に true を返します。</returns>
		public bool ReadData(ref BinaryReader fs, int version)
		{
			DataName = fs.ReadString();
			ShotResName = fs.ReadString();
			LoopResName = fs.ReadString();
			LoopBegin = fs.ReadInt32();
			return true;
		}

		/// <summary>
		/// 名前変更時に呼び出されますが、本クラスでは特に処理を行いません。
		/// </summary>
		/// <param name="type">変更の種類</param>
		/// <param name="src">元の名前</param>
		/// <param name="dst">新しい名前</param>
		public void Rename(EChangeNameType type, string src, string dst)
		{
			// 処理なし
		}
	}

	/// <summary>
	/// サウンドを鳴らす単体データを表すクラス。（Sys）
	/// </summary>
	public class SoundPlayData : DataShifterBase
	{
		// 定数
		private const string SHOT_HEADER = "#SS_";
		private const string LOOP_HEADER = "#SL_";

		/// <summary>使用する名前変更タイプを返します（SoundID）。</summary>
		protected override EChangeNameType GetMyType() => EChangeNameType.SoundID;

		/// <summary>
		/// ショット再生用タイマ名を取得します。
		/// </summary>
		/// <returns>ショットタイマ名</returns>
		public string GetShotTimerName() => SHOT_HEADER + ShifterName;

		/// <summary>
		/// ループ再生用タイマ名を取得します。
		/// </summary>
		/// <returns>ループタイマ名</returns>
		public string GetLoopTimerName() => LOOP_HEADER + ShifterName;
	}
}
