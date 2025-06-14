using System;
using System.Collections.Generic;
using System.IO;

namespace SlotEffectMaker2023.Data
{
	/// <summary>
	/// ボーナス設定データを保持するクラスです。
	/// </summary>
	public class BonusConfig : IEffectNameInterface
	{
		/// <summary>ボーナス識別ID</summary>
		public byte BonusID { get; set; }

		/// <summary>小役識別ID（Coma）</summary>
		public byte ComaID { get; set; }

		/// <summary>ボーナスタイプ</summary>
		public byte BonusType { get; set; }

		/// <summary>デフォルトコンストラクタ。プロパティを初期化します。</summary>
		public BonusConfig()
		{
			BonusID = 0;
			ComaID = 0;
			BonusType = 0;
		}

		/// <summary>このインスタンスの内容をバイナリに書き込みます。</summary>
		/// <param name="fs">書き込み先の BinaryWriter（ref）</param>
		/// <param name="version">データのバージョン</param>
		/// <returns>書き込み成功時に true を返します。</returns>
		public bool StoreData(ref BinaryWriter fs, int version)
		{
			fs.Write(BonusID);
			fs.Write(ComaID);
			fs.Write(BonusType);
			return true;
		}

		/// <summary>バイナリからこのインスタンスの内容を読み込みます。</summary>
		/// <param name="fs">読み込み元の BinaryReader（ref）</param>
		/// <param name="version">データのバージョン</param>
		/// <returns>読み込み成功時に true を返します。</returns>
		public bool ReadData(ref BinaryReader fs, int version)
		{
			BonusID = fs.ReadByte();
			ComaID = fs.ReadByte();
			BonusType = fs.ReadByte();
			return true;
		}

		/// <summary>名前変更時に呼び出されますが、本クラスでは何も行いません。</summary>
		/// <param name="type">変更の種類</param>
		/// <param name="src">元の名前</param>
		/// <param name="dst">新しい名前</param>
		public void Rename(EChangeNameType type, string src, string dst)
		{
			// 本クラスには名前変更対象のフィールドがありません
		}
	}

	/// <summary>
	/// ボーナス履歴設定を管理するクラスです。
	/// </summary>
	public class HistoryConfig : IEffectNameInterface
	{
		// 定数
		/// <summary>ボーナスタイプの最大数（1～記載数まで、0は総計で使用）</summary>
		public const int BONUS_TYPE_MAX = 3;

		/// <summary>各ボーナスの設定リスト</summary>
		public List<BonusConfig> BonusConfig { get; set; }

		/// <summary>成立時演出取得用変数名</summary>
		public string LaunchEffect { get; set; }

		/// <summary>入賞G数取得用変数名</summary>
		public string InGameHolder { get; set; }

		/// <summary>ボーナス入賞回数設定先変数名リスト</summary>
		public List<string> BonusCountHolder { get; set; }

		/// <summary>デフォルトコンストラクタ。リストとプロパティを初期化します。</summary>
		public HistoryConfig()
		{
			BonusConfig = new List<BonusConfig>();
			LaunchEffect = string.Empty;
			InGameHolder = string.Empty;
			BonusCountHolder = new List<string>();
			for (int i = 0; i <= BONUS_TYPE_MAX; ++i)
				BonusCountHolder.Add(string.Empty);
		}

		/// <summary>このインスタンスの内容をバイナリに書き込みます。</summary>
		/// <param name="fs">書き込み先の BinaryWriter（ref）</param>
		/// <param name="version">データのバージョン</param>
		/// <returns>書き込み成功時に true を返します。</returns>
		public bool StoreData(ref BinaryWriter fs, int version)
		{
			fs.Write(BonusConfig.Count);
			foreach (var item in BonusConfig)
				item.StoreData(ref fs, version);

			fs.Write(LaunchEffect);
			fs.Write(InGameHolder);

			fs.Write(BonusCountHolder.Count);
			foreach (var item in BonusCountHolder)
				fs.Write(item);

			return true;
		}

		/// <summary>バイナリからこのインスタンスの内容を読み込みます。</summary>
		/// <param name="fs">読み込み元の BinaryReader（ref）</param>
		/// <param name="version">データのバージョン</param>
		/// <returns>読み込み成功時に true を返します。</returns>
		public bool ReadData(ref BinaryReader fs, int version)
		{
			int dataCount = fs.ReadInt32();
			for (int i = 0; i < dataCount; ++i)
			{
				var bc = new BonusConfig();
				bc.ReadData(ref fs, version);
				BonusConfig.Add(bc);
			}

			LaunchEffect = fs.ReadString();
			InGameHolder = fs.ReadString();

			dataCount = fs.ReadInt32();
			for (int i = 0; i < dataCount && i <= BONUS_TYPE_MAX; ++i)
				BonusCountHolder[i] = fs.ReadString();

			return true;
		}

		/// <summary>
		/// 名前変更時に、内部の BonusConfig と変数名を更新します。
		/// </summary>
		/// <param name="type">変更の種類（Var: 変数名）</param>
		/// <param name="src">元の名前</param>
		/// <param name="dst">新しい名前</param>
		public void Rename(EChangeNameType type, string src, string dst)
		{
			foreach (var item in BonusConfig)
				item.Rename(type, src, dst);

			if (type == EChangeNameType.Var && InGameHolder.Equals(src))
				InGameHolder = dst;

			if (type == EChangeNameType.Var && LaunchEffect.Equals(src))
				LaunchEffect = dst;

			for (int i = 0; i < BONUS_TYPE_MAX; ++i)
			{
				if (type == EChangeNameType.Var && BonusCountHolder[i].Equals(src))
					BonusCountHolder[i] = dst;
			}
		}

		/// <summary>
		/// 指定したボーナスIDに対応する BonusConfig を取得します。
		/// </summary>
		/// <param name="pBonusID">検索するボーナスID</param>
		/// <returns>該当する設定があればその BonusConfig、なければ null を返します。</returns>
		public BonusConfig GetConfig(int pBonusID)
		{
			foreach (var item in BonusConfig)
				if (item.BonusID == pBonusID)
					return item;
			return null;
		}
	}
}
