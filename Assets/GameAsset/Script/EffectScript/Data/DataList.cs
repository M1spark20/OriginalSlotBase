using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SlotEffectMaker2023.Data
{
    /// <summary>
    /// ゲームを動かすためのインプットデータを入れる
    /// </summary>
    public class EfValCond : IEffectNameInterface
    {
        // 変数に関する条件を記載する(単体)
        public string valName { get; set; }
        public int min { get; set; }
        public int max { get; set; }
        public bool invFlag { get; set; }

        /// <summary>
        /// EfValCond の新しいインスタンスを初期化します。
        /// valName を空文字、min を int.MinValue、max を int.MaxValue、invFlag を false に設定します。
        /// </summary>
        public EfValCond()
        {
            valName = string.Empty;
            min = int.MinValue;
            max = int.MaxValue;
            invFlag = false;
        }

        /// <summary>
        /// 指定したバイナリライターにデータを書き込みます。
        /// </summary>
        /// <param name="fs">データを書き込む BinaryWriter への参照。</param>
        /// <param name="version">データ形式のバージョン。</param>
        /// <returns>書き込み処理が成功した場合に true を返します。</returns>
        public bool StoreData(ref BinaryWriter fs, int version)
        {
            fs.Write(valName);
            fs.Write(min);
            fs.Write(max);
            fs.Write(invFlag);
            return true;
        }

        /// <summary>
        /// 指定したバイナリリーダーからデータを読み込みます。
        /// </summary>
        /// <param name="fs">データを読み込む BinaryReader への参照。</param>
        /// <param name="version">データ形式のバージョン。</param>
        /// <returns>読み込み処理が成功した場合に true を返します。</returns>
        public bool ReadData(ref BinaryReader fs, int version)
        {
            valName = fs.ReadString();
            min = fs.ReadInt32();
            max = fs.ReadInt32();
            invFlag = fs.ReadBoolean();
            return true;
        }

        /// <summary>
        /// 名前変更タイプが Var の場合、valName が指定された src と一致すれば dst に変更します。
        /// </summary>
        /// <param name="type">変更を適用する名前の種類。</param>
        /// <param name="src">変更前の名前。</param>
        /// <param name="dst">変更後の名前。</param>
        public void Rename(EChangeNameType type, string src, string dst)
        {
            if (type == EChangeNameType.Var && valName.Equals(src)) valName = dst;
        }
    }
    /// <summary>
    /// 時間に関する条件を記載する
    /// </summary>
    public class EfTimeCond : IEffectNameInterface
    {
        // 時間に関する条件を記載する
        public string timerName { get; set; }
        public int elapsed { get; set; }
        public bool trigHold { get; set; }      // トリガを立てっぱなしにするか(true:立てっぱなし)

        /// <summary>
        /// EfTimeCond の新しいインスタンスを初期化します。
        /// timerName を空文字、elapsed を 0、trigHold を false に設定します。
        /// </summary>
        public EfTimeCond()
        {
            timerName = string.Empty;
            elapsed = 0;
            trigHold = false;
        }

        /// <summary>
        /// 指定したバイナリライターにデータを書き込みます。
        /// </summary>
        /// <param name="fs">データを書き込む BinaryWriter への参照。</param>
        /// <param name="version">データ形式のバージョン。</param>
        /// <returns>書き込み処理が成功した場合に true を返します。</returns>
        public bool StoreData(ref BinaryWriter fs, int version)
        {
            fs.Write(timerName);
            fs.Write(elapsed);
            fs.Write(trigHold);
            return true;
        }

        /// <summary>
        /// 指定したバイナリリーダーからデータを読み込みます。
        /// </summary>
        /// <param name="fs">データを読み込む BinaryReader への参照。</param>
        /// <param name="version">データ形式のバージョン。</param>
        /// <returns>読み込み処理が成功した場合に true を返します。</returns>
        public bool ReadData(ref BinaryReader fs, int version)
        {
            timerName = fs.ReadString();
            elapsed = fs.ReadInt32();
            trigHold = fs.ReadBoolean();
            return true;
        }

        /// <summary>
        /// 名前変更タイプが Timer の場合、timerName が指定された src と一致すれば dst に変更します。
        /// </summary>
        /// <param name="type">変更を適用する名前の種類。</param>
        /// <param name="src">変更前の名前。</param>
        /// <param name="dst">変更後の名前。</param>
        public void Rename(EChangeNameType type, string src, string dst)
        {
            if (type == EChangeNameType.Timer && timerName.Equals(src)) timerName = dst;
        }
    }
    /// <summary>
    /// アクション名の切り替え条件を管理する入力データを表します。
    /// </summary>
    public class EfActionSwitch : IEffectNameInterface
    {
        private EChangeNameType useType;

        public int condVal { get; set; }
        public string actName { get; set; }

        /// <summary>
        /// EfActionSwitch の新しいインスタンスを初期化します。
        /// actName を空文字、condVal を 0、useType を None に設定します。
        /// </summary>
        public EfActionSwitch()
        {
            actName = string.Empty;
            condVal = 0;
            useType = EChangeNameType.None;
        }

        /// <summary>
        /// 指定したバイナリライターに condVal と actName を書き込みます。
        /// </summary>
        /// <param name="fs">データを書き込む BinaryWriter への参照。</param>
        /// <param name="version">データ形式のバージョン。</param>
        /// <returns>書き込み処理が成功した場合に true を返します。</returns>
        public bool StoreData(ref BinaryWriter fs, int version)
        {
            fs.Write(condVal);
            fs.Write(actName);
            return true;
        }

        /// <summary>
        /// 指定したバイナリリーダーから condVal と actName を読み込みます。
        /// </summary>
        /// <param name="fs">データを読み込む BinaryReader への参照。</param>
        /// <param name="version">データ形式のバージョン。</param>
        /// <returns>読み込み処理が成功した場合に true を返します。</returns>
        public bool ReadData(ref BinaryReader fs, int version)
        {
            condVal = fs.ReadInt32();
            actName = fs.ReadString();
            return true;
        }

        /// <summary>
        /// useType と一致するタイプの場合、actName が指定された src と一致すれば dst に変更します。
        /// </summary>
        /// <param name="type">変更を適用する名前の種類。</param>
        /// <param name="src">変更前の名前。</param>
        /// <param name="dst">変更後の名前。</param>
        public void Rename(EChangeNameType type, string src, string dst)
        {
            if (type == useType && actName.Equals(src)) actName = dst;
        }

        // 独自関数
        /// <summary>
        /// Rename メソッドで使用する名前変更タイプを設定します。
        /// </summary>
        /// <param name="type">このインスタンスで使用する EChangeNameType。</param>
        public void SetRenameType(EChangeNameType type) { useType = type; }
    }

    /// <summary>
    /// 条件成立時にトリガを発生させるための入力データを表します。
    /// </summary>
    public class EfCondTrig : IEffectNameInterface
    {
        /// <summary>
        /// 実行データの名前。
        /// </summary>
        public string actName { get; set; } // 実行データ名

        /// <summary>
        /// 条件成立時または不成立時に実行可否を指定します(true: 実行可能)。
        /// </summary>
        public bool cdEnable { get; set; }  // 条件成立or不成立で実行性格付け

        /// <summary>
        /// EfCondTrig の新しいインスタンスを初期化します。
        /// actName を空文字、cdEnable を true に設定します。
        /// </summary>
        public EfCondTrig()
        {
            actName = string.Empty;
            cdEnable = true;
        }

        /// <summary>
        /// 指定したバイナリライターに actName と cdEnable を書き込みます。
        /// </summary>
        /// <param name="fs">データを書き込む BinaryWriter への参照。</param>
        /// <param name="version">データ形式のバージョン。</param>
        /// <returns>書き込み処理が成功した場合に true を返します。</returns>
        public bool StoreData(ref BinaryWriter fs, int version)
        {
            fs.Write(actName);
            fs.Write(cdEnable);
            return true;
        }

        /// <summary>
        /// 指定したバイナリリーダーから actName と cdEnable を読み込みます。
        /// </summary>
        /// <param name="fs">データを読み込む BinaryReader への参照。</param>
        /// <param name="version">データ形式のバージョン。</param>
        /// <returns>読み込み処理が成功した場合に true を返します。</returns>
        public bool ReadData(ref BinaryReader fs, int version)
        {
            actName = fs.ReadString();
            cdEnable = fs.ReadBoolean();
            return true;
        }

        /// <summary>
        /// 名前変更タイプが Timeline の場合、actName が指定された src と一致すれば dst に変更します。
        /// </summary>
        /// <param name="type">変更を適用する名前の種類。</param>
        /// <param name="src">変更前の名前。</param>
        /// <param name="dst">変更後の名前。</param>
        public void Rename(EChangeNameType type, string src, string dst)
        {
            if (type == EChangeNameType.Timeline && actName.Equals(src)) actName = dst;
        }
    }

    /// <summary>
    /// 実行データを表す抽象基底クラスです。
    /// dataName と usage を保持し、各種アクションの共通処理を定義します。
    /// </summary>
    public abstract class IEfAct : IEffectNameInterface
    {
        /// <summary>
        /// 実行データの名前。
        /// </summary>
        public string dataName { get; set; }

        /// <summary>
        /// 実行データの用途（メモなど）。
        /// </summary>
        public string usage { get; set; }

        /// <summary>
        /// IEfAct の新しいインスタンスを初期化します。
        /// dataName と usage を空文字に設定します。
        /// </summary>
        public IEfAct()
        {
            dataName = string.Empty;
            usage = string.Empty;
        }

        /// <summary>
        /// このインスタンスが表すアクションを実行します。
        /// 派生クラスで具体的な処理を実装してください。
        /// </summary>
        public abstract void Action();

        /// <summary>
        /// ベースクラスで定義されたデータ名と用途をバイナリライターに書き込みます。
        /// </summary>
        /// <param name="fs">データを書き込む BinaryWriter への参照。</param>
        /// <param name="version">データ形式のバージョン。</param>
        /// <returns>書き込み処理が成功した場合に true を返します。</returns>
        public virtual bool StoreData(ref BinaryWriter fs, int version)
        {
            fs.Write(dataName);
            fs.Write(usage);
            return true;
        }

        /// <summary>
        /// ベースクラスで定義されたデータ名と用途をバイナリリーダーから読み込みます。
        /// </summary>
        /// <param name="fs">データを読み込む BinaryReader への参照。</param>
        /// <param name="version">データ形式のバージョン。</param>
        /// <returns>読み込み処理が成功した場合に true を返します。</returns>
        public virtual bool ReadData(ref BinaryReader fs, int version)
        {
            dataName = fs.ReadString();
            usage = fs.ReadString();
            return true;
        }

        /// <summary>
        /// 名前変更タイプに応じて dataName またはその他の名前を変更します。
        /// 派生クラスで具体的な挙動を実装してください。
        /// </summary>
        /// <param name="type">変更を適用する名前の種類。</param>
        /// <param name="src">変更前の名前。</param>
        /// <param name="dst">変更後の名前。</param>
        public abstract void Rename(EChangeNameType type, string src, string dst);
    }
    /// <summary>
    /// 変数の値に応じてアクションを切り替える基底クラスです。
    /// IEfAct の機能を拡張し、switcher リストによる条件分岐を提供します。
    /// </summary>
    public abstract class IEfChangeBase : IEfAct
    {
        /// <summary>
        /// スイッチャー（条件分岐）を識別する名前。
        /// </summary>
        public string switcherName { get; set; }

        /// <summary>
        /// 参照する変数名。
        /// </summary>
        public string variableRef { get; set; }

        /// <summary>
        /// 条件値とアクションの組み合わせリスト。
        /// </summary>
        public List<EfActionSwitch> switcher { get; set; }

        /// <summary>
        /// IEfChangeBase の新しいインスタンスを初期化します。
        /// switcherName と variableRef を空文字、switcher を空のリストに設定します。
        /// </summary>
        public IEfChangeBase()
        {
            switcherName = string.Empty;
            variableRef = string.Empty;
            switcher = new List<EfActionSwitch>();
        }

        /// <summary>
        /// base.StoreData を呼び出した後、switcherName, variableRef, switcher の要素数と各要素のデータを書き込みます。
        /// </summary>
        /// <param name="fs">データを書き込む BinaryWriter への参照。</param>
        /// <param name="version">データ形式のバージョン。</param>
        /// <returns>書き込み処理が成功した場合に true を返します。base.StoreData が false を返した場合は false を返します。</returns>
        public override bool StoreData(ref BinaryWriter fs, int version)
        {
            if (!base.StoreData(ref fs, version)) return false;
            fs.Write(switcherName);
            fs.Write(variableRef);
            fs.Write(switcher.Count);
            for (int i = 0; i < switcher.Count; ++i)
                switcher[i].StoreData(ref fs, version);
            return true;
        }

        /// <summary>
        /// base.ReadData を呼び出した後、switcherName, variableRef, switcher の要素数と各要素のデータを読み込みます。
        /// </summary>
        /// <param name="fs">データを読み込む BinaryReader への参照。</param>
        /// <param name="version">データ形式のバージョン。</param>
        /// <returns>読み込み処理が成功した場合に true を返します。base.ReadData が false を返した場合は false を返します。</returns>
        public override bool ReadData(ref BinaryReader fs, int version)
        {
            if (!base.ReadData(ref fs, version)) return false;
            switcherName = fs.ReadString();
            variableRef = fs.ReadString();
            int size = fs.ReadInt32();
            for (int i = 0; i < size; ++i)
            {
                EfActionSwitch sw = new EfActionSwitch();
                sw.SetRenameType(GetElemType());
                if (!sw.ReadData(ref fs, version)) return false;
                switcher.Add(sw);
            }
            return true;
        }

        /// <summary>
        /// switcher 要素の Rename で使用する名前変更タイプを取得します。
        /// 派生クラスで具体的な EChangeNameType を返してください。
        /// </summary>
        /// <returns>使用する EChangeNameType。</returns>
        protected abstract EChangeNameType GetElemType();
    }
    /// <summary>
    /// 変数による条件分岐を行うアクションを表します。
    /// conds による AND-OR 条件評価後、actionList 内のアクションを実行します。
    /// </summary>
    public class EfActValCond : IEfAct
    {
        // 変数による条件分岐を行う
        /// <summary>
        /// AND-OR 構造で定義された変数条件のリスト。
        /// </summary>
        public List<List<EfValCond>> conds { get; set; }

        /// <summary>
        /// 条件成立時に実行されるアクションリスト。
        /// </summary>
        public List<EfCondTrig> actionList { get; set; }

        /// <summary>
        /// EfActValCond の新しいインスタンスを初期化します。
        /// conds と actionList を空のリストに設定し、usage を "[変数条件]" に設定します。
        /// </summary>
        public EfActValCond()
        {
            conds = new List<List<EfValCond>>();
            actionList = new List<EfCondTrig>();
            usage = "[変数条件]";
        }

        /// <summary>
        /// 条件評価を行い、actionList 内の各アクションを実行します。
        /// </summary>
        public override void Action()
        {
            bool actionFlag = Evaluate();
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

        /// <summary>
        /// base.StoreData を呼び出した後、conds と actionList のデータを書き込みます。
        /// </summary>
        /// <param name="fs">データを書き込む BinaryWriter への参照。</param>
        /// <param name="version">データ形式のバージョン。</param>
        /// <returns>書き込み処理が成功した場合に true を返します。</returns>
        public override bool StoreData(ref BinaryWriter fs, int version)
        {
            if (!base.StoreData(ref fs, version)) return false;
            fs.Write(conds.Count);
            for (int a = 0; a < conds.Count; ++a)
            {
                fs.Write(conds[a].Count);
                for (int r = 0; r < conds[a].Count; ++r)
                    conds[a][r].StoreData(ref fs, version);
            }
            fs.Write(actionList.Count);
            for (int i = 0; i < actionList.Count; ++i)
                actionList[i].StoreData(ref fs, version);
            return true;
        }

        /// <summary>
        /// base.ReadData を呼び出した後、conds と actionList のデータを読み込みます。
        /// </summary>
        /// <param name="fs">データを読み込む BinaryReader への参照。</param>
        /// <param name="version">データ形式のバージョン。</param>
        /// <returns>読み込み処理が成功した場合に true を返します。</returns>
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

        /// <summary>
        /// conds と actionList 内の各要素に対して Rename を実行します。
        /// </summary>
        /// <param name="type">変更を適用する名前の種類。</param>
        /// <param name="src">変更前の名前。</param>
        /// <param name="dst">変更後の名前。</param>
        public override void Rename(EChangeNameType type, string src, string dst)
        {
            foreach (var condAnd in conds)
                foreach (var condOr in condAnd)
                    condOr.Rename(type, src, dst);
            foreach (var act in actionList)
                act.Rename(type, src, dst);
        }

        // キャスト時 or クラス型直接指定時のみ外部からも使用可能
        /// <summary>
        /// 登録された conds の条件を評価し、すべての AND グループが成立するかを判定します。
        /// </summary>
        /// <returns>すべての条件が成立すれば true、そうでなければ false を返します。</returns>
        public bool Evaluate()
        {
            var varList = Singleton.SlotDataSingleton.GetInstance().valManager;
            foreach (var itemAnd in conds)
            {
                bool actionOR = false;
                foreach (var itemOr in itemAnd)
                {
                    SlotVariable data = varList.GetVariable(itemOr.valName);
                    if (data == null) continue;
                    actionOR = data.CheckRange(itemOr.min, itemOr.max, true) ^ itemOr.invFlag;
                    if (actionOR) break;
                }
                if (!actionOR) { return false; }
            }
            return true;
        }

        // 20241018追加：offsetつき判定 > キャスト時 or クラス型直接指定時のみ外部からも使用可能
        // Tuple = (valName, offset)
        /// <summary>
        /// offset を考慮して条件を評価します。
        /// </summary>
        /// <param name="ov">変数名とオフセット値のタプルリスト。</param>
        /// <returns>すべての条件が成立すれば true、そうでなければ false を返します。</returns>
        public bool Evaluate(List<Tuple<string, int>> ov)
        {
            var varList = Singleton.SlotDataSingleton.GetInstance().valManager;
            foreach (var itemAnd in conds)
            {
                bool actionOR = false;
                foreach (var itemOr in itemAnd)
                {
                    SlotVariable data = varList.GetVariable(itemOr.valName);
                    if (data == null) continue;
                    int offset = 0;
                    foreach (var ovItem in ov)
                        if (ovItem.Item1 == itemOr.valName) { offset = ovItem.Item2; break; }
                    actionOR = data.CheckRange(itemOr.min, itemOr.max, true, offset) ^ itemOr.invFlag;
                    if (actionOR) break;
                }
                if (!actionOR) { return false; }
            }
            return true;
        }
    }
    /// <summary>
    /// タイマー条件に基づいてアクションを実行するクラスです。
    /// cond で定義された時間条件が成立すると、action リスト内のアクションを順次実行します。
    /// </summary>
    public class EfActTimerCond : IEfAct
    {
        /// <summary>
        /// 時間条件を保持する EfTimeCond インスタンス。
        /// </summary>
        public EfTimeCond cond { get; set; }

        /// <summary>
        /// 条件成立時に実行されるアクションリスト。
        /// </summary>
        public List<EfCondTrig> action { get; set; }

        /// <summary>
        /// EfActTimerCond の新しいインスタンスを初期化します。
        /// cond を新規インスタンス化し、action を空のリストに設定し、usage を "[トリガ]" に設定します。
        /// </summary>
        public EfActTimerCond()
        {
            cond = new EfTimeCond();
            action = new List<EfCondTrig>();
            usage = "[トリガ]";
        }

        /// <summary>
        /// タイマー値を取得し、cond の条件を満たしていれば action リスト内の各アクションを実行します。
        /// </summary>
        public override void Action()
        {
            // タイマ値を取得して判定を行う
            var timerList = Singleton.SlotDataSingleton.GetInstance().timerData;
            var checkT = timerList.GetTimer(cond.timerName);
            if (checkT == null) return;
            if (!checkT.GetActionFlag(cond.elapsed / 1000f, cond.trigHold)) return;
            // 処理の実行
            foreach (var item in action)
            {
                // Singletonからデータを呼び出して実行
                var actItem = Singleton.EffectDataManagerSingleton.GetInstance().Timeline.GetActionFromName(item.actName);
                if (actItem == null) continue;
                actItem.Action();
            }
        }

        /// <summary>
        /// base.StoreData を呼び出した後、cond と action のデータを書き込みます。
        /// </summary>
        /// <param name="fs">データを書き込む BinaryWriter への参照。</param>
        /// <param name="version">データ形式のバージョン。</param>
        /// <returns>書き込み処理が成功した場合に true を返します。</returns>
        public override bool StoreData(ref BinaryWriter fs, int version)
        {
            if (!base.StoreData(ref fs, version)) return false;
            cond.StoreData(ref fs, version);
            fs.Write(action.Count);
            for (int i = 0; i < action.Count; ++i)
                action[i].StoreData(ref fs, version);
            return true;
        }

        /// <summary>
        /// base.ReadData を呼び出した後、cond と action のデータを読み込みます。
        /// </summary>
        /// <param name="fs">データを読み込む BinaryReader への参照。</param>
        /// <param name="version">データ形式のバージョン。</param>
        /// <returns>読み込み処理が成功した場合に true を返します。</returns>
        public override bool ReadData(ref BinaryReader fs, int version)
        {
            if (!base.ReadData(ref fs, version)) return false;
            cond.ReadData(ref fs, version);
            int actSize = fs.ReadInt32();
            for (int i = 0; i < actSize; ++i)
            {
                var ef = new EfCondTrig();
                ef.ReadData(ref fs, version);
                action.Add(ef);
            }
            return true;
        }

        /// <summary>
        /// cond と action 内の各要素に対して Rename を実行します。
        /// </summary>
        /// <param name="type">変更を適用する名前の種類。</param>
        /// <param name="src">変更前の名前。</param>
        /// <param name="dst">変更後の名前。</param>
        public override void Rename(EChangeNameType type, string src, string dst)
        {
            cond.Rename(type, src, dst);
            foreach (var act in action)
                act.Rename(type, src, dst);
        }
    }
    /// <summary>
    /// タイマー制御アクションを表します。
    /// defName で指定したタイマーの有効化/無効化およびリセットを行います。
    /// </summary>
    public class EfActCtrlTimer : IEfAct
    {
        /// <summary>
        /// 制御対象のタイマー名。
        /// </summary>
        public string defName { get; set; }

        /// <summary>
        /// タイマーを有効化するかどうか(true: 有効化、false: 無効化)。
        /// </summary>
        public bool setActivate { get; set; }

        /// <summary>
        /// 有効化時に強制的にリセットするかどうか。
        /// </summary>
        public bool forceReset { get; set; }

        /// <summary>
        /// EfActCtrlTimer の新しいインスタンスを初期化します。
        /// defName を空文字、setActivate を true、forceReset を false、usage を "[タイマ制御]" に設定します。
        /// </summary>
        public EfActCtrlTimer()
        {
            defName = string.Empty;
            setActivate = true;
            forceReset = false;
            usage = "[タイマ制御]";
        }

        /// <summary>
        /// 指定したタイマーの有効化/無効化を行います。
        /// </summary>
        public override void Action()
        {
            // タイマの有効化/無効化を行う: defName[arrValName::val]
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

        /// <summary>
        /// base.StoreData を呼び出した後、defName, setActivate, forceReset のデータを書き込みます。
        /// </summary>
        /// <param name="fs">データを書き込む BinaryWriter への参照。</param>
        /// <param name="version">データ形式のバージョン。</param>
        /// <returns>書き込み処理が成功した場合に true を返します。</returns>
        public override bool StoreData(ref BinaryWriter fs, int version)
        {
            if (!base.StoreData(ref fs, version)) return false;
            fs.Write(defName);
            fs.Write(setActivate);
            fs.Write(forceReset);
            return true;
        }

        /// <summary>
        /// base.ReadData を呼び出した後、defName, setActivate, forceReset のデータを読み込みます。
        /// </summary>
        /// <param name="fs">データを読み込む BinaryReader への参照。</param>
        /// <param name="version">データ形式のバージョン。</param>
        /// <returns>読み込み処理が成功した場合に true を返します。</returns>
        public override bool ReadData(ref BinaryReader fs, int version)
        {
            if (!base.ReadData(ref fs, version)) return false;
            defName = fs.ReadString();
            setActivate = fs.ReadBoolean();
            forceReset = fs.ReadBoolean();
            return true;
        }

        /// <summary>
        /// 名前変更タイプが Timer の場合、defName が指定された src と一致すれば dst に変更します。
        /// </summary>
        /// <param name="type">変更を適用する名前の種類。</param>
        /// <param name="src">変更前の名前。</param>
        /// <param name="dst">変更後の名前。</param>
        public override void Rename(EChangeNameType type, string src, string dst)
        {
            if (type == EChangeNameType.Timer && defName.Equals(src)) defName = dst;
        }
    }
    /// <summary>
    /// 変数への演算操作を行うアクションを表します。
    /// valInputFor で指定した変数を取得し、operands の OP を順に適用し、結果を変数に設定します。
    /// </summary>
    public class EfActCtrlVal : IEfAct
    {
        /// <summary>
        /// 単一の演算を表すクラスです。
        /// varName が空文字の場合は fixVal、そうでなければ指定変数の値を用います。
        /// </summary>
        public class OP : IEffectNameInterface
        {
            /// <summary>参照する変数名。emptyでfixValを使用。</summary>
            public string varName;  // emptyで数字を参照
            /// <summary>固定値。</summary>
            public int fixVal;
            /// <summary>演算子。</summary>
            public ECalcOperand op;

            /// <summary>
            /// OP の新しいインスタンスを初期化します。
            /// varName を空文字、fixVal を 0、op を eAdd に設定します。
            /// </summary>
            public OP()
            {
                varName = string.Empty;
                fixVal = 0;
                op = ECalcOperand.eAdd;
            }

            /// <summary>
            /// BinaryWriter に varName, fixVal, op を書き込みます。
            /// </summary>
            /// <param name="fs">BinaryWriter への参照。</param>
            /// <param name="version">データ形式のバージョン。</param>
            /// <returns>書き込み成功時に true を返します。</returns>
            public bool StoreData(ref BinaryWriter fs, int version)
            {
                fs.Write(varName);
                fs.Write(fixVal);
                fs.Write((byte)op);
                return true;
            }

            /// <summary>
            /// BinaryReader から varName, fixVal, op を読み込みます。
            /// </summary>
            /// <param name="fs">BinaryReader への参照。</param>
            /// <param name="version">データ形式のバージョン。</param>
            /// <returns>読み込み成功時に true を返します。</returns>
            public bool ReadData(ref BinaryReader fs, int version)
            {
                varName = fs.ReadString();
                fixVal = fs.ReadInt32();
                op = (ECalcOperand)fs.ReadByte();
                return true;
            }

            /// <summary>
            /// 名前変更タイプが Var の場合、varName が一致すれば dst に変更します。
            /// </summary>
            /// <param name="type">変更を適用する名前の種類。</param>
            /// <param name="src">変更前の名前。</param>
            /// <param name="dst">変更後の名前。</param>
            public void Rename(EChangeNameType type, string src, string dst)
            {
                if (type == EChangeNameType.Var && varName.Equals(src)) varName = dst;
            }
        }

        /// <summary>演算対象の変数名。</summary>
        public string valInputFor { get; set; }
        /// <summary>適用する演算リスト。</summary>
        public List<OP> operands { get; set; }

        /// <summary>
        /// EfActCtrlVal の新しいインスタンスを初期化します。
        /// valInputFor を空文字、operands を空のリスト、usage を "[変数演算]" に設定します。
        /// </summary>
        public EfActCtrlVal()
        {
            valInputFor = string.Empty;
            operands = new List<OP>();
            usage = "[変数演算]";
        }

        /// <summary>
        /// base.StoreData を呼び出した後、valInputFor, operands のデータを書き込みます。
        /// </summary>
        /// <param name="fs">BinaryWriter への参照。</param>
        /// <param name="version">データ形式のバージョン。</param>
        /// <returns>書き込み成功時に true を返します。</returns>
        public override bool StoreData(ref BinaryWriter fs, int version)
        {
            if (!base.StoreData(ref fs, version)) return false;
            fs.Write(valInputFor);
            fs.Write(operands.Count);
            for (int i = 0; i < operands.Count; ++i) operands[i].StoreData(ref fs, version);
            return true;
        }

        /// <summary>
        /// base.ReadData を呼び出した後、valInputFor, operands のデータを読み込みます。
        /// </summary>
        /// <param name="fs">BinaryReader への参照。</param>
        /// <param name="version">データ形式のバージョン。</param>
        /// <returns>読み込み成功時に true を返します。</returns>
        public override bool ReadData(ref BinaryReader fs, int version)
        {
            if (!base.ReadData(ref fs, version)) return false;
            valInputFor = fs.ReadString();
            int size = fs.ReadInt32();
            for (int i = 0; i < size; ++i)
            {
                OP nData = new OP();
                if (!nData.ReadData(ref fs, version)) return false;
                operands.Add(nData);
            }
            return true;
        }

        /// <summary>
        /// 変数を取得し、operands の演算結果を順に適用して変数に設定します。
        /// </summary>
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

        /// <summary>
        /// 名前変更タイプが Var の場合、valInputFor と operands 内の varName を更新します。
        /// </summary>
        /// <param name="type">変更を適用する名前の種類。</param>
        /// <param name="src">変更前の名前。</param>
        /// <param name="dst">変更後の名前。</param>
        public override void Rename(EChangeNameType type, string src, string dst)
        {
            if (type == EChangeNameType.Var && valInputFor.Equals(src)) valInputFor = dst;
            foreach (var item in operands) item.Rename(type, src, dst);
        }

        /// <summary>
        /// 単一の演算を実行します。
        /// </summary>
        /// <param name="operand">適用する OP インスタンス。</param>
        /// <param name="opLeft">左オペランドの現在値。</param>
        /// <returns>演算結果を返します。</returns>
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
    /// <summary>
    /// 音声の切り替えを行うアクションです。
    /// 変数 variableRef の値に応じて、指定した SoundPlayer(switcherName) のサウンドIDを switcher リストで設定します。
    /// </summary>
    public class EfActChangeSound : IEfChangeBase
    {
        /// <summary>
        /// EfActChangeSound の新しいインスタンスを初期化します。
        /// base クラスの初期化に加え、usage を "[鳴り分け]" に設定します。
        /// </summary>
        public EfActChangeSound() : base()
        {
            usage = "[鳴り分け]";
        }

        /// <summary>
        /// switcher 要素の Rename で使用する名前変更タイプを取得します。
        /// SoundID を返します。
        /// </summary>
        /// <returns>EChangeNameType.SoundID を返します。</returns>
        protected override EChangeNameType GetElemType() { return EChangeNameType.SoundID; }

        /// <summary>
        /// SoundPlayer(switcherName) を取得し、variableRef の変数値に応じてサウンドIDを切り替えます。
        /// </summary>
        public override void Action()
        {
            var player = Singleton.EffectDataManagerSingleton.GetInstance().GetSoundPlayer(switcherName);
            if (player == null) return;
            string soundID = player.DefaultElemID;

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
            soundData.ChangeElem(switcherName, soundID);
        }

        /// <summary>
        /// 名前変更タイプに応じて switcherName, variableRef, switcher の要素名を更新します。
        /// </summary>
        /// <param name="type">変更を適用する名前の種類。</param>
        /// <param name="src">変更前の名前。</param>
        /// <param name="dst">変更後の名前。</param>
        public override void Rename(EChangeNameType type, string src, string dst)
        {
            if (type == EChangeNameType.SoundPlayer && switcherName.Equals(src)) switcherName = dst;
            if (type == EChangeNameType.Var && variableRef.Equals(src)) variableRef = dst;
            foreach (var sw in switcher) sw.Rename(type, src, dst);
        }
    }
    /// <summary>
    /// カラーマップの切り替えを行うアクションです。
    /// 変数 variableRef の値に応じて、指定した ColorMap Shifter(switcherName) のマップIDを switcher リストで設定します。
    /// </summary>
    public class EfActChangeMap : IEfChangeBase
    {
        /// <summary>
        /// EfActChangeMap の新しいインスタンスを初期化します。
        /// base クラスの初期化に加え、usage を "[MAP切替]" に設定します。
        /// </summary>
        public EfActChangeMap() : base()
        {
            usage = "[MAP切替]";
        }

        /// <summary>
        /// switcher 要素の Rename で使用する名前変更タイプを取得します。
        /// ColorMap を返します。
        /// </summary>
        /// <returns>EChangeNameType.ColorMap を返します。</returns>
        protected override EChangeNameType GetElemType() { return EChangeNameType.ColorMap; }

        /// <summary>
        /// ColorMap Shifter(switcherName) を取得し、variableRef の変数値に応じてマップIDを切り替えます。
        /// </summary>
        public override void Action()
        {
            var player = Singleton.EffectDataManagerSingleton.GetInstance().ColorMap.GetShifter(switcherName);
            if (player == null) return;
            string mapID = player.DefaultElemID;

            // mapDataIDを変更する
            var actData = Singleton.SlotDataSingleton.GetInstance();
            var valData = actData.valManager.GetVariable(variableRef);
            if (valData != null)
            {
                foreach (var item in switcher)
                {
                    if (valData.val == item.condVal) { mapID = item.actName; break; }
                }
            }

            var mapData = actData.colorMapData;
            mapData.ChangeElem(switcherName, mapID);
        }

        /// <summary>
        /// 名前変更タイプに応じて switcherName, variableRef, switcher の要素名を更新します。
        /// </summary>
        /// <param name="type">変更を適用する名前の種類。</param>
        /// <param name="src">変更前の名前。</param>
        /// <param name="dst">変更後の名前。</param>
        public override void Rename(EChangeNameType type, string src, string dst)
        {
            if (type == EChangeNameType.MapPlayer && switcherName.Equals(src)) switcherName = dst;
            if (type == EChangeNameType.Var && variableRef.Equals(src)) variableRef = dst;
            foreach (var sw in switcher) sw.Rename(type, src, dst);
        }
    }
    /// <summary>
    /// 乱数を用いた抽選を行い、抽選結果に応じたアクションを実行します。
    /// randMax を上限としてランダム値を生成し、randData の condVal を減算しながら結果を決定します。
    /// </summary>
    public class EfActRandVal : IEfAct
    {
        /// <summary>
        /// 乱数抽選最大値。
        /// </summary>
        public int randMax { get; set; }                    // 乱数抽選最大値

        /// <summary>
        /// 抽選データ（condVal と actName の組み合わせリスト）。
        /// </summary>
        public List<EfActionSwitch> randData { get; set; }  // 抽選データ

        /// <summary>
        /// EfActRandVal の新しいインスタンスを初期化します。
        /// randMax を 256、randData を空のリスト、usage を "[乱数抽選]" に設定します。
        /// </summary>
        public EfActRandVal()
        {
            randMax = 256;
            randData = new List<EfActionSwitch>();
            usage = "[乱数抽選]";
        }

        /// <summary>
        /// base.StoreData を呼び出した後、randMax と randData のデータを書き込みます。
        /// </summary>
        /// <param name="fs">データを書き込む BinaryWriter への参照。</param>
        /// <param name="version">データ形式のバージョン。</param>
        /// <returns>書き込み処理が成功した場合に true を返します。</returns>
        public override bool StoreData(ref BinaryWriter fs, int version)
        {
            if (!base.StoreData(ref fs, version)) return false;
            fs.Write(randMax);
            fs.Write(randData.Count);
            for (int i = 0; i < randData.Count; ++i)
                randData[i].StoreData(ref fs, version);
            return true;
        }

        /// <summary>
        /// base.ReadData を呼び出した後、randMax と randData のデータを読み込みます。
        /// </summary>
        /// <param name="fs">データを読み込む BinaryReader への参照。</param>
        /// <param name="version">データ形式のバージョン。</param>
        /// <returns>読み込み処理が成功した場合に true を返します。</returns>
        public override bool ReadData(ref BinaryReader fs, int version)
        {
            if (!base.ReadData(ref fs, version)) return false;
            randMax = fs.ReadInt32();
            int sz = fs.ReadInt32();
            for (int i = 0; i < sz; ++i)
            {
                EfActionSwitch ad = new EfActionSwitch();
                ad.ReadData(ref fs, version);
                ad.SetRenameType(EChangeNameType.Timeline);
                randData.Add(ad);
            }
            return true;
        }

        /// <summary>
        /// 名前変更タイプが Var の場合のみ、randData 内の varName を更新します。
        /// </summary>
        /// <param name="type">変更を適用する名前の種類。</param>
        /// <param name="src">変更前の名前。</param>
        /// <param name="dst">変更後の名前。</param>
        public override void Rename(EChangeNameType type, string src, string dst)
        {
            if (type != EChangeNameType.Var) return;
            for (int i = 0; i < randData.Count; ++i)
                randData[i].Rename(type, src, dst);
        }

        /// <summary>
        /// 乱数を生成し、randData の condVal を順に減算しながら抽選を行い、
        /// 条件を満たした最初のアクションを実行します。
        /// </summary>
        public override void Action()
        {
            // 抽選実施
            int randVal = UnityEngine.Random.Range(0, randMax);
            for (int dec = 0; dec < randData.Count; ++dec)
            {
                randVal -= randData[dec].condVal;
                if (randVal < 0)
                {
                    // データ確定
                    var tl = Singleton.EffectDataManagerSingleton.GetInstance().Timeline;
                    tl.GetActionFromName(randData[dec].actName).Action();
                    return;
                }
            }
        }
    }
    /// <summary>
    /// 複数の変数に一括で値を設定するアクションを表します。
    /// </summary>
    public class EfActMultiVarSet : IEfAct
    {
        /// <summary>
        /// 変数名と設定値の組み合わせリスト。
        /// </summary>
        public List<EfActionSwitch> setData { get; set; }

        /// <summary>
        /// EfActMultiVarSet の新しいインスタンスを初期化します。
        /// setData を空のリストに、usage を "[複数変数]" に設定します。
        /// </summary>
        public EfActMultiVarSet()
        {
            setData = new List<EfActionSwitch>();
            usage = "[複数変数]";
        }

        /// <summary>
        /// base.StoreData を呼び出した後、setData のデータを書き込みます。
        /// </summary>
        /// <param name="fs">データを書き込む BinaryWriter への参照。</param>
        /// <param name="version">データ形式のバージョン。</param>
        /// <returns>書き込み処理が成功した場合に true を返します。</returns>
        public override bool StoreData(ref BinaryWriter fs, int version)
        {
            if (!base.StoreData(ref fs, version)) return false;
            fs.Write(setData.Count);
            for (int i = 0; i < setData.Count; ++i)
                setData[i].StoreData(ref fs, version);
            return true;
        }

        /// <summary>
        /// base.ReadData を呼び出した後、setData のデータを読み込みます。
        /// </summary>
        /// <param name="fs">データを読み込む BinaryReader への参照。</param>
        /// <param name="version">データ形式のバージョン。</param>
        /// <returns>読み込み処理が成功した場合に true を返します。</returns>
        public override bool ReadData(ref BinaryReader fs, int version)
        {
            if (!base.ReadData(ref fs, version)) return false;
            int sz = fs.ReadInt32();
            for (int i = 0; i < sz; ++i)
            {
                EfActionSwitch ef = new EfActionSwitch();
                ef.SetRenameType(EChangeNameType.Var);
                if (!ef.ReadData(ref fs, version)) return false;
                setData.Add(ef);
            }
            return true;
        }

        /// <summary>
        /// 名前変更タイプが Var の場合、setData 内の変数名を更新します。
        /// </summary>
        /// <param name="type">変更を適用する名前の種類。</param>
        /// <param name="src">変更前の名前。</param>
        /// <param name="dst">変更後の名前。</param>
        public override void Rename(EChangeNameType type, string src, string dst)
        {
            if (type != EChangeNameType.Var) return;
            for (int i = 0; i < setData.Count; ++i)
                setData[i].Rename(type, src, dst);
        }

        /// <summary>
        /// setData の各要素に従い、変数に condVal の値を設定します。
        /// </summary>
        public override void Action()
        {
            var ins = Singleton.SlotDataSingleton.GetInstance();
            foreach (var item in setData)
            {
                var variable = ins.valManager.GetVariable(item.actName);
                if (variable != null)
                    variable.val = item.condVal;
            }
        }
    }
}
