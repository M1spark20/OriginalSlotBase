using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SlotEffectMaker2023.Data
{
    public class SlotTimeline : IEffectNameInterface
    {
        public List<EfActChangeSound> changeSound;
		public List<EfActValCond> condData;
		public List<EfActTimerCond> timerData;
		public List<EfActCtrlVal> valOpData;
		public List<EfActCtrlTimer> ctrlTimer;
		public List<EfActRandVal> randData;
		public List<EfActMultiVarSet> multiSetData;
        public List<EfActChangeMap> changeMap;

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
		public bool ReadData(ref BinaryReader fs, int version)
		{
			int dataCount = fs.ReadInt32();
			for (int i = 0; i < dataCount; ++i)
			{
				EfActChangeSound cs = new EfActChangeSound();
				if (!cs.ReadData(ref fs, version)) return false;
				changeSound.Add(cs);
			}
			dataCount = fs.ReadInt32();
			for (int i = 0; i < dataCount; ++i)
			{
				EfActValCond vc = new EfActValCond();
				if (!vc.ReadData(ref fs, version)) return false;
				condData.Add(vc);
			}
			dataCount = fs.ReadInt32();
			for (int i = 0; i < dataCount; ++i)
			{
				EfActTimerCond tc = new EfActTimerCond();
				if (!tc.ReadData(ref fs, version)) return false;
				timerData.Add(tc);
			}
			dataCount = fs.ReadInt32();
			for (int i = 0; i < dataCount; ++i)
			{
				EfActCtrlVal cv = new EfActCtrlVal();
				if (!cv.ReadData(ref fs, version)) return false;
				valOpData.Add(cv);
			}
			dataCount = fs.ReadInt32();
			for (int i = 0; i < dataCount; ++i)
			{
				EfActCtrlTimer ct = new EfActCtrlTimer();
				if (!ct.ReadData(ref fs, version)) return false;
				ctrlTimer.Add(ct);
			}
			dataCount = fs.ReadInt32();
			for (int i = 0; i < dataCount; ++i)
			{
				EfActRandVal rv = new EfActRandVal();
				if (!rv.ReadData(ref fs, version)) return false;
				randData.Add(rv);
			}
			dataCount = fs.ReadInt32();
			for (int i = 0; i < dataCount; ++i)
			{
				EfActMultiVarSet mv = new EfActMultiVarSet();
				if (!mv.ReadData(ref fs, version)) return false;
				multiSetData.Add(mv);
			}
			dataCount = fs.ReadInt32();
			for (int i = 0; i < dataCount; ++i)
			{
				EfActChangeMap cm = new EfActChangeMap();
				if (!cm.ReadData(ref fs, version)) return false;
				changeMap.Add(cm);
			}
			return true;
		}
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
		// 全Actの名前を得る
		public string[] GetAllActName()
        {
			List<string> vs = new List<string>();
			foreach (var item in changeSound) vs.Add(item.dataName);
			foreach (var item in condData) vs.Add(item.dataName);
			foreach (var item in valOpData) vs.Add(item.dataName);
			foreach (var item in ctrlTimer) vs.Add(item.dataName);
			foreach (var item in randData) vs.Add(item.dataName);
			foreach (var item in multiSetData) vs.Add(item.dataName);
			foreach (var item in changeMap) vs.Add(item.dataName);
			return vs.ToArray();
        }
		// Actのデータを得る
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
		// 指定した名前のActが存在するか確認する
		public bool IsActNameExist(string name)
        {
			return GetActionFromName(name) != null;
        }

		// (20240810)外部から条件だけ取れるように関数追加
		public string[] GetAllCondName()
        {
			List<string> vs = new List<string>();
			foreach (var item in condData) vs.Add(item.dataName);
			return vs.ToArray();
        }
		public EfActValCond GetCondFromName(string name)
        {
			foreach (var item in condData) if (item.dataName.Equals(name)) return item;
			return null;
		}
	}
}
