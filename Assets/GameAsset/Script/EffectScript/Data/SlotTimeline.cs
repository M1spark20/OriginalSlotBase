using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SlotEffectMaker2023.Data
{
	/// <summary>
	/// ゲーム内タイムライン上の全アクションを管理するクラスです。
	/// </summary>
	public class SlotTimeline : IEffectNameInterface
	{
		/// <summary>サウンド切り替えアクションのリスト</summary>
		public List<EfActChangeSound> changeSound;

		/// <summary>変数条件アクションのリスト</summary>
		public List<EfActValCond> condData;

		/// <summary>タイマ条件アクションのリスト</summary>
		public List<EfActTimerCond> timerData;

		/// <summary>変数演算アクションのリスト</summary>
		public List<EfActCtrlVal> valOpData;

		/// <summary>タイマ制御アクションのリスト</summary>
		public List<EfActCtrlTimer> ctrlTimer;

		/// <summary>乱数抽選アクションのリスト</summary>
		public List<EfActRandVal> randData;

		/// <summary>複数変数設定アクションのリスト</summary>
		public List<EfActMultiVarSet> multiSetData;

		/// <summary>マップ切り替えアクションのリスト</summary>
		public List<EfActChangeMap> changeMap;

		/// <summary>
		/// デフォルトコンストラクタ。各アクションリストを初期化します。
		/// </summary>
		public SlotTimeline()
		{
			changeSound = new List<EfActChangeSound>();
			condData = new List<EfActValCond>();
			timerData = new List<EfActTimerCond>();
			valOpData = new List<EfActCtrlVal>();
			ctrlTimer = new List<EfActCtrlTimer>();
			randData = new List<EfActRandVal>();
			multiSetData = new List<EfActMultiVarSet>();
			changeMap = new List<EfActChangeMap>();
		}

		/// <summary>
		/// 全アクションデータをバイナリに書き込みます。
		/// </summary>
		/// <param name="fs">書き込み先の <see cref="BinaryWriter"/></param>
		/// <param name="version">データのバージョン</param>
		/// <returns>すべてのアクション書き込みに成功した場合 true を返します。</returns>
		public bool StoreData(ref BinaryWriter fs, int version)
		{
			fs.Write(changeSound.Count);
			foreach (var item in changeSound)
				if (!item.StoreData(ref fs, version)) return false;

			fs.Write(condData.Count);
			foreach (var item in condData)
				if (!item.StoreData(ref fs, version)) return false;

			fs.Write(timerData.Count);
			foreach (var item in timerData)
				if (!item.StoreData(ref fs, version)) return false;

			fs.Write(valOpData.Count);
			foreach (var item in valOpData)
				if (!item.StoreData(ref fs, version)) return false;

			fs.Write(ctrlTimer.Count);
			foreach (var item in ctrlTimer)
				if (!item.StoreData(ref fs, version)) return false;

			fs.Write(randData.Count);
			foreach (var item in randData)
				if (!item.StoreData(ref fs, version)) return false;

			fs.Write(multiSetData.Count);
			foreach (var item in multiSetData)
				if (!item.StoreData(ref fs, version)) return false;

			fs.Write(changeMap.Count);
			foreach (var item in changeMap)
				if (!item.StoreData(ref fs, version)) return false;

			return true;
		}

		/// <summary>
		/// バイナリから全アクションデータを読み込みます。
		/// </summary>
		/// <param name="fs">読み込み元の <see cref="BinaryReader"/></param>
		/// <param name="version">データのバージョン</param>
		/// <returns>すべてのアクション読み込みに成功した場合 true を返します。</returns>
		public bool ReadData(ref BinaryReader fs, int version)
		{
			int dataCount;

			dataCount = fs.ReadInt32();
			for (int i = 0; i < dataCount; ++i)
			{
				var cs = new EfActChangeSound();
				if (!cs.ReadData(ref fs, version)) return false;
				changeSound.Add(cs);
			}

			dataCount = fs.ReadInt32();
			for (int i = 0; i < dataCount; ++i)
			{
				var vc = new EfActValCond();
				if (!vc.ReadData(ref fs, version)) return false;
				condData.Add(vc);
			}

			dataCount = fs.ReadInt32();
			for (int i = 0; i < dataCount; ++i)
			{
				var tc = new EfActTimerCond();
				if (!tc.ReadData(ref fs, version)) return false;
				timerData.Add(tc);
			}

			dataCount = fs.ReadInt32();
			for (int i = 0; i < dataCount; ++i)
			{
				var cv = new EfActCtrlVal();
				if (!cv.ReadData(ref fs, version)) return false;
				valOpData.Add(cv);
			}

			dataCount = fs.ReadInt32();
			for (int i = 0; i < dataCount; ++i)
			{
				var ct = new EfActCtrlTimer();
				if (!ct.ReadData(ref fs, version)) return false;
				ctrlTimer.Add(ct);
			}

			dataCount = fs.ReadInt32();
			for (int i = 0; i < dataCount; ++i)
			{
				var rv = new EfActRandVal();
				if (!rv.ReadData(ref fs, version)) return false;
				randData.Add(rv);
			}

			dataCount = fs.ReadInt32();
			for (int i = 0; i < dataCount; ++i)
			{
				var mv = new EfActMultiVarSet();
				if (!mv.ReadData(ref fs, version)) return false;
				multiSetData.Add(mv);
			}

			dataCount = fs.ReadInt32();
			for (int i = 0; i < dataCount; ++i)
			{
				var cm = new EfActChangeMap();
				if (!cm.ReadData(ref fs, version)) return false;
				changeMap.Add(cm);
			}

			return true;
		}

		/// <summary>
		/// 名前変更時に全アクションリストの <see cref="Rename(EChangeNameType, string, string)"/> を呼び出します。
		/// </summary>
		/// <param name="type">変更の種類</param>
		/// <param name="src">元の名前</param>
		/// <param name="dst">新しい名前</param>
		public void Rename(EChangeNameType type, string src, string dst)
		{
			foreach (var item in changeSound) item.Rename(type, src, dst);
			foreach (var item in condData) item.Rename(type, src, dst);
			foreach (var item in timerData) item.Rename(type, src, dst);
			foreach (var item in valOpData) item.Rename(type, src, dst);
			foreach (var item in ctrlTimer) item.Rename(type, src, dst);
			foreach (var item in randData) item.Rename(type, src, dst);
			foreach (var item in multiSetData) item.Rename(type, src, dst);
			foreach (var item in changeMap) item.Rename(type, src, dst);
		}

		/// <summary>
		/// 全アクションの <c>dataName</c> を配列で取得します。
		/// </summary>
		/// <returns>全アクション名の配列</returns>
		public string[] GetAllActName()
		{
			var vs = new List<string>();
			foreach (var item in changeSound) vs.Add(item.dataName);
			foreach (var item in condData) vs.Add(item.dataName);
			foreach (var item in valOpData) vs.Add(item.dataName);
			foreach (var item in ctrlTimer) vs.Add(item.dataName);
			foreach (var item in randData) vs.Add(item.dataName);
			foreach (var item in multiSetData) vs.Add(item.dataName);
			foreach (var item in changeMap) vs.Add(item.dataName);
			return vs.ToArray();
		}

		/// <summary>
		/// 指定した名前のアクションを取得します。
		/// </summary>
		/// <param name="name">検索するアクションの <c>dataName</c></param>
		/// <returns>
		/// 見つかった場合は対応する <see cref="IEfAct"/>、存在しない場合は <c>null</c> を返します。
		/// </returns>
		public IEfAct GetActionFromName(string name)
		{
			foreach (var item in changeSound) if (item.dataName.Equals(name)) return item;
			foreach (var item in condData) if (item.dataName.Equals(name)) return item;
			foreach (var item in timerData) if (item.dataName.Equals(name)) return item;
			foreach (var item in valOpData) if (item.dataName.Equals(name)) return item;
			foreach (var item in ctrlTimer) if (item.dataName.Equals(name)) return item;
			foreach (var item in randData) if (item.dataName.Equals(name)) return item;
			foreach (var item in multiSetData) if (item.dataName.Equals(name)) return item;
			foreach (var item in changeMap) if (item.dataName.Equals(name)) return item;
			return null;
		}

		/// <summary>
		/// 指定したアクション名が存在するかどうかを確認します。
		/// </summary>
		/// <param name="name">確認するアクションの <c>dataName</c></param>
		/// <returns>存在する場合は <c>true</c>、存在しない場合は <c>false</c></returns>
		public bool IsActNameExist(string name)
		{
			return GetActionFromName(name) != null;
		}

		/// <summary>
		/// 条件アクション (condData) のすべての <c>dataName</c> を取得します。
		/// </summary>
		/// <returns>全条件アクション名の配列</returns>
		public string[] GetAllCondName()
		{
			var vs = new List<string>();
			foreach (var item in condData) vs.Add(item.dataName);
			return vs.ToArray();
		}

		/// <summary>
		/// 指定した名前の条件アクションを取得します。
		/// </summary>
		/// <param name="name">検索する条件アクションの <c>dataName</c></param>
		/// <returns>見つかった場合は <see cref="EfActValCond"/>、存在しない場合は <c>null</c> を返します。</returns>
		public EfActValCond GetCondFromName(string name)
		{
			foreach (var item in condData)
				if (item.dataName.Equals(name))
					return item;
			return null;
		}
	}
}
