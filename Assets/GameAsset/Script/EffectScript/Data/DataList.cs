using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SlotEffectMaker2023.Data
{
    // ゲームを動かすためのインプットデータを入れる
    public class EfValCond : SlotMaker2022.ILocalDataInterface
    {   // 変数に関する条件を記載する(単体)
        public string valName { get; set; }
        public int min { get; set; }
        public int max { get; set; }
        public bool invFlag { get; set; }

        public EfValCond()
        {
            valName = string.Empty;
            min = int.MinValue;
            max = int.MaxValue;
            invFlag = false;
        }
        public bool StoreData(ref BinaryWriter fs, int version)
        {
            fs.Write(valName);
            fs.Write(min);
            fs.Write(max);
            fs.Write(invFlag);
            return true;
        }
        public bool ReadData(ref BinaryReader fs, int version)
        {
            valName = fs.ReadString();
            min = fs.ReadInt32();
            max = fs.ReadInt32();
            invFlag = fs.ReadBoolean();
            return true;
        }
    }
    public class EfTimeCond : SlotMaker2022.ILocalDataInterface
    {   // 時間に関する条件を記載する
        public string timerName { get; set; }
        public int elapsed { get; set; }
        public bool trigHold { get; set; }      // トリガを立てっぱなしにするか(true:立てっぱなし)

        public EfTimeCond()
        {
            timerName = string.Empty;
            elapsed = 0;
            trigHold = false;
        }
        public bool StoreData(ref BinaryWriter fs, int version)
        {
            fs.Write(timerName);
            fs.Write(elapsed);
            fs.Write(trigHold);
            return true;
        }
        public bool ReadData(ref BinaryReader fs, int version)
        {
            timerName = fs.ReadString();
            elapsed = fs.ReadInt32();
            trigHold = fs.ReadBoolean();
            return true;
        }
    }
    public class EfActionSwitch : SlotMaker2022.ILocalDataInterface
    {
        public int condVal { get; set; }
        public string actName { get; set; }
        public EfActionSwitch()
        {
            actName = string.Empty;
            condVal = 0;
        }
        public bool StoreData(ref BinaryWriter fs, int version)
        {
            fs.Write(condVal);
            fs.Write(actName);
            return true;
        }
        public bool ReadData(ref BinaryReader fs, int version)
        {
            condVal = fs.ReadInt32();
            actName = fs.ReadString();
            return true;
        }
    }
    public class EfCondTrig : SlotMaker2022.ILocalDataInterface
    {
        public string actName { get; set; } // 実行データ名
        public bool cdEnable { get; set; }  // 条件成立or不成立で実行性格付け
        public EfCondTrig()
        {
            actName = string.Empty;
            cdEnable = true;
        }
        public bool StoreData(ref BinaryWriter fs, int version)
        {
            fs.Write(actName);
            fs.Write(cdEnable);
            return true;
        }
        public bool ReadData(ref BinaryReader fs, int version)
        {
            actName = fs.ReadString();
            cdEnable = fs.ReadBoolean();
            return true;
        }
    }

    //// タイムライン用IF ////
    public abstract class IEfAct : SlotMaker2022.ILocalDataInterface
    {
        public string dataName { get; set; }
        public string usage { get; set; }
        public abstract void Action();
        public virtual bool StoreData(ref BinaryWriter fs, int version)
        {
            fs.Write(dataName);
            fs.Write(usage);
            return true;
        }
        public virtual bool ReadData(ref BinaryReader fs, int version)
        {
            dataName = fs.ReadString();
            usage = fs.ReadString();
            return true;
        }
    }
    public class EfActValCond : IEfAct
    {   // 変数による条件分岐を行う
        public List<List<EfValCond>> conds { get; set; }
        public List<EfCondTrig> actionList { get; set; }
        public EfActValCond()
        {
            conds = new List<List<EfValCond>>();
            actionList = new List<EfCondTrig>();
        }
        public override void Action()
        {
            var varList = Singleton.SlotDataSingleton.GetInstance().valManager;
            bool actionFlag = true;
            foreach(var itemAnd in conds)
            {
                bool actionOR = false;
                foreach (var itemOr in itemAnd)
                {
                    SlotVariable data = varList.GetVariable(itemOr.valName);
                    if (data == null) continue;
                    actionOR = data.CheckRange(itemOr.min, itemOr.max, true) ^ itemOr.invFlag;
                    if (actionOR) break;
                }
                if (!actionOR) { actionFlag = false; break; }
            }
            // 処理の実行
            foreach (var item in actionList)
            {
                // Singletonからデータを呼び出して実行
                if (actionFlag != item.cdEnable) continue;
                var actItem = Singleton.EffectDataManagerSingleton.GetInstance().Timeline.GetActionFromName(item.actName);
                if (actItem == null) continue;
                actItem.Action();
            }
        }
        public override bool StoreData(ref BinaryWriter fs, int version)
        {
            if (!base.StoreData(ref fs, version)) return false;
            fs.Write(conds.Count);
            for (int a = 0; a < conds.Count; ++a)
            {
                fs.Write(conds[a].Count);
                for (int r = 0; r < conds[a].Count; ++r) conds[a][r].StoreData(ref fs, version);
            }
            fs.Write(actionList.Count);
            for (int i = 0; i < actionList.Count; ++i) actionList[i].StoreData(ref fs, version);
            return true;
        }
        public override bool ReadData(ref BinaryReader fs, int version)
        {
            if (!base.ReadData(ref fs, version)) return false;
            int andCount = fs.ReadInt32();
            for (int a = 0; a < andCount; ++a)
            {
                int orCount = fs.ReadInt32();
                conds.Add(new List<EfValCond>());
                for (int r = 0; r < orCount; ++r)
                {
                    var vc = new EfValCond();
                    vc.ReadData(ref fs, version);
                    conds[a].Add(vc);
                }
            }
            int actNum = fs.ReadInt32();
            for (int i = 0; i < actNum; ++i)
            {
                var ad = new EfCondTrig();
                ad.ReadData(ref fs, version);
                actionList.Add(ad);
            }
            return true;
        }
    }
    public class EfActTimerCond : IEfAct
    {   // タイマによる条件分岐を行う
        public EfTimeCond cond { get; set; }
        public List<EfCondTrig> action { get; set; }
        public EfActTimerCond()
        {
            cond = new EfTimeCond();
            action = new List<EfCondTrig>();
        }
        public override void Action()
        {
            // タイマ値を取得して判定を行う
            var timerList = Singleton.SlotDataSingleton.GetInstance().timerData;
            var checkT = timerList.GetTimer(cond.timerName);
            if (checkT == null) return;
            if (!checkT.GetActionFlag(cond.elapsed, cond.trigHold)) return;
            // 処理の実行
            foreach (var item in action)
            {
                // Singletonからデータを呼び出して実行
                var actItem = Singleton.EffectDataManagerSingleton.GetInstance().Timeline.GetActionFromName(item.actName);
                if (actItem == null) continue;
                actItem.Action();
            }
        }
        public override bool StoreData(ref BinaryWriter fs, int version)
        {
            if (!base.StoreData(ref fs, version)) return false;
            cond.StoreData(ref fs, version);
            fs.Write(action.Count);
            for (int i = 0; i < action.Count; ++i) action[i].StoreData(ref fs, version);
            return true;
        }
        public override bool ReadData(ref BinaryReader fs, int version)
        {
            if (!base.ReadData(ref fs, version)) return false;
            cond.ReadData(ref fs, version);
            int actSize = fs.ReadInt32();
            for (int i = 0; i < actSize; ++i)
            {
                EfCondTrig ef = new EfCondTrig();
                ef.ReadData(ref fs, version);
                action.Add(ef);
            }
            return true;
        }
    }
    public class EfActCtrlTimer : IEfAct
    {
        public string defName { get; set; }
        public bool setActivate { get; set; }
        public bool forceReset { get; set; }
        public EfActCtrlTimer()
        {
            defName = string.Empty;
            setActivate = true;
            forceReset = false;
        }
        public override void Action()
        {   // タイマの有効化/無効化を行う: defName[arrValName::val]
            string ctrlTimerName = defName;
            // タイマデータを呼び出す
            var tdata = Singleton.SlotDataSingleton.GetInstance().timerData;
            Action.SlotTimer timer = tdata.GetTimer(ctrlTimerName);
            if (timer == null) return;
            // タイマを制御する
            if (setActivate) 
            {
                timer.Activate();
                if (forceReset) timer.Reset();
            }
            else 
            {
                timer.SetDisabled();
            }
        }
        public override bool StoreData(ref BinaryWriter fs, int version)
        {
            if (!base.StoreData(ref fs, version)) return false;
            fs.Write(defName);
            fs.Write(setActivate);
            fs.Write(forceReset);
            return true;
        }
        public override bool ReadData(ref BinaryReader fs, int version)
        {
            if(!base.ReadData(ref fs, version)) return false;
            defName = fs.ReadString();
            setActivate = fs.ReadBoolean();
            forceReset = fs.ReadBoolean();
            return true;
        }
    }
    public class EfActCtrlVal : IEfAct
    {
        public class OP : SlotMaker2022.ILocalDataInterface
        {
            public string varName;  // emptyで数字を参照
            public int fixVal;
            public ECalcOperand op;
            public OP()
            {
                varName = string.Empty;
                fixVal = 0;
                op = ECalcOperand.eAdd;
            }
            public bool StoreData(ref BinaryWriter fs, int version)
            {
                fs.Write(varName);
                fs.Write(fixVal);
                fs.Write((byte)op);
                return true;
            }
            public bool ReadData(ref BinaryReader fs, int version)
            {
                varName = fs.ReadString();
                fixVal = fs.ReadInt32();
                op = (ECalcOperand)fs.ReadByte();
                return true;
            }
        }
        public string valInputFor { get; set; }
        public List<OP> operands { get; set; }
        public EfActCtrlVal()
        {
            valInputFor = string.Empty;
            operands = new List<OP>();
        }
        public override bool StoreData(ref BinaryWriter fs, int version)
        {
            if(!base.StoreData(ref fs, version)) return false;
            fs.Write(valInputFor);
            fs.Write(operands.Count);
            for (int i = 0; i < operands.Count; ++i) operands[i].StoreData(ref fs, version);
            return true;
        }
        public override bool ReadData(ref BinaryReader fs, int version)
        {
            if(!base.ReadData(ref fs, version)) return false;
            valInputFor = fs.ReadString();
            int size = fs.ReadInt32();
            for(int i=0; i<size; ++i)
            {
                OP nData = new OP();
                if(!nData.ReadData(ref fs, version)) return false;
                operands.Add(nData);
            }
            return true;
        }
        public override void Action()
        {
            // 変数データの取得
            var vList = Singleton.SlotDataSingleton.GetInstance().valManager;
            var inData = vList.GetVariable(valInputFor);
            // 変数が見つからない場合は何もしない
            if (inData == null) return;

            // operandによる値の計算(上から順に計算する、加減乗除の優先度は非考慮)
            int ans = 0;
            foreach (var op in operands) ans = Operate(op, ans);
            inData.val = ans;
        }
        private int Operate(OP operand, int opLeft)
        {   // 単体の計算処理
            // 変数データの取得
            int val = operand.fixVal;
            if (operand.varName != string.Empty)
            {
                var vList = Singleton.SlotDataSingleton.GetInstance().valManager;
                var vs = vList.GetVariable(operand.varName);
                if (vs == null) return opLeft;
                val = vs.val;
            }

            if (operand.op == ECalcOperand.eAdd) return opLeft + val;
            if (operand.op == ECalcOperand.eSub) return opLeft - val;
            if (operand.op == ECalcOperand.eMul) return opLeft * val;
            // オペレータ未指定/初期値の場合 or 除数が0の場合
            if (operand.op == ECalcOperand.eNone || val == 0) return opLeft;
            // 除数系の計算
            if (operand.op == ECalcOperand.eDiv) return opLeft / val;
            if (operand.op == ECalcOperand.eMod) return opLeft % val;
            // 何も引っかからなかった時
            return opLeft;
        }
    }
    public class EfActChangeSound : IEfAct
    {
        public string playDataName { get; set; }
        public string variableRef { get; set; }
        public List<EfActionSwitch> switcher { get; set; }
        public EfActChangeSound()
        {
            playDataName = string.Empty;
            variableRef = string.Empty;
            switcher = new List<EfActionSwitch>();
        }
        public override void Action()
        {
            var player = Singleton.EffectDataManagerSingleton.GetInstance().GetSoundPlayer(playDataName);
            if (player == null) return;
            string soundID = player.DefaultSoundID;

            // soundIDを変更する
            var actData = Singleton.SlotDataSingleton.GetInstance();
            var valData = actData.valManager.GetVariable(variableRef);
            if (valData != null)
            {
                foreach (var item in switcher)
                {
                    if (valData.val == item.condVal) { soundID = item.actName; break; }
                }
            }

            var soundData = actData.soundData;
            soundData.ChangeSoundID(playDataName, soundID);
        }
        public override bool StoreData(ref BinaryWriter fs, int version)
        {
            if(!base.StoreData(ref fs, version)) return false;
            fs.Write(playDataName);
            fs.Write(variableRef);
            fs.Write(switcher.Count);
            for (int i = 0; i < switcher.Count; ++i) switcher[i].StoreData(ref fs, version);
            return true;
        }
        public override bool ReadData(ref BinaryReader fs, int version)
        {
            if(!base.ReadData(ref fs, version)) return false;
            playDataName = fs.ReadString();
            variableRef = fs.ReadString();
            int size = fs.ReadInt32();
            for (int i = 0; i < size; ++i)
            {
                EfActionSwitch sw = new EfActionSwitch();
                if(!sw.ReadData(ref fs, version)) return false;
                switcher.Add(sw);
            }
            return true;
        }
    }
}
