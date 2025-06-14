using System.Collections.Generic;
using System.IO;

namespace SlotEffectMaker2023.Data
{
	/// <summary>
	/// 小役成立回数のカウントに使用する条件データを保持するクラス。
	/// </summary>
	public class FlagCounterCond : IEffectNameInterface
	{
		/// <summary>カウント結果を格納する変数名</summary>
		public string OutVar { get; set; }

		/// <summary>カウントフラグの最小値</summary>
		public byte FlagMin { get; set; }

		/// <summary>カウントフラグの最大値</summary>
		public byte FlagMax { get; set; }

		/// <summary>デフォルトコンストラクタ。プロパティを初期化します。</summary>
		public FlagCounterCond()
		{
			OutVar = string.Empty;
			FlagMin = 0;
			FlagMax = 0;
		}

		/// <summary>このインスタンスをバイナリに書き込みます。</summary>
		/// <param name="fs">書き込み先の BinaryWriter</param>
		/// <param name="version">データのバージョン</param>
		/// <returns>書き込み成功時に true を返します。</returns>
		public bool StoreData(ref BinaryWriter fs, int version)
		{
			fs.Write(OutVar);
			fs.Write(FlagMin);
			fs.Write(FlagMax);
			return true;
		}

		/// <summary>バイナリからこのインスタンスを読み込みます。</summary>
		/// <param name="fs">読み込み元の BinaryReader</param>
		/// <param name="version">データのバージョン</param>
		/// <returns>読み込み成功時に true を返します。</returns>
		public bool ReadData(ref BinaryReader fs, int version)
		{
			OutVar = fs.ReadString();
			FlagMin = fs.ReadByte();
			FlagMax = fs.ReadByte();
			return true;
		}

		/// <summary>名前変更時に、対象の変数名を更新します。</summary>
		/// <param name="type">変更の種類</param>
		/// <param name="src">元の名前</param>
		/// <param name="dst">新しい名前</param>
		public void Rename(EChangeNameType type, string src, string dst)
		{
			if (type == EChangeNameType.Var && OutVar.Equals(src))
				OutVar = dst;
		}
	}

	/// <summary>
	/// 小役成立回数のセット条件をまとめるクラス。
	/// </summary>
	public class FlagCounterSet : IEffectNameInterface
	{
		/// <summary>個別のカウント条件リスト</summary>
		public List<FlagCounterCond> elemData { get; set; }

		/// <summary>カウント対象とするアクション（ActVC）の名前</summary>
		public string CountCond { get; set; }

		/// <summary>デフォルトコンストラクタ。リストと文字列を初期化します。</summary>
		public FlagCounterSet()
		{
			elemData = new List<FlagCounterCond>();
			CountCond = string.Empty;
		}

		/// <summary>このインスタンスをバイナリに書き込みます。</summary>
		/// <param name="fs">書き込み先の BinaryWriter</param>
		/// <param name="version">データのバージョン</param>
		/// <returns>書き込み成功時に true を返します。</returns>
		public bool StoreData(ref BinaryWriter fs, int version)
		{
			fs.Write(CountCond);
			fs.Write(elemData.Count);
			foreach (var item in elemData)
				item.StoreData(ref fs, version);
			return true;
		}

		/// <summary>バイナリからこのインスタンスを読み込みます。</summary>
		/// <param name="fs">読み込み元の BinaryReader</param>
		/// <param name="version">データのバージョン</param>
		/// <returns>読み込み成功時に true を返します。</returns>
		public bool ReadData(ref BinaryReader fs, int version)
		{
			CountCond = fs.ReadString();
			int dataCount = fs.ReadInt32();
			for (int i = 0; i < dataCount; ++i)
			{
				var fc = new FlagCounterCond();
				fc.ReadData(ref fs, version);
				elemData.Add(fc);
			}
			return true;
		}

		/// <summary>名前変更時に、内部リストおよび条件名を更新します。</summary>
		/// <param name="type">変更の種類</param>
		/// <param name="src">元の名前</param>
		/// <param name="dst">新しい名前</param>
		public void Rename(EChangeNameType type, string src, string dst)
		{
			foreach (var item in elemData)
				item.Rename(type, src, dst);

			if (type == EChangeNameType.Timeline && CountCond.Equals(src))
				CountCond = dst;
		}
	}
}
