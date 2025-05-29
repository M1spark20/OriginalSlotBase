using System;
using System.IO;

namespace SlotMaker2022
{
    /// <summary>
    /// ローカルデータの読み書きを定義するインターフェースです。
    /// </summary>
    public interface ILocalDataInterface
    {
        /// <summary>
        /// バイナリライターにデータを書き込みます。
        /// </summary>
        /// <param name="fs">データを書き込む BinaryWriter への参照。</param>
        /// <param name="version">データ形式のバージョン。</param>
        /// <returns>書き込み処理が成功した場合に true を返します。</returns>
        bool StoreData(ref BinaryWriter fs, int version);

        /// <summary>
        /// バイナリリーダーからデータを読み込みます。
        /// </summary>
        /// <param name="fs">データを読み込む BinaryReader への参照。</param>
        /// <param name="version">データ形式のバージョン。</param>
        /// <returns>読み込み処理が成功した場合に true を返します。</returns>
        bool ReadData(ref BinaryReader fs, int version);
    }

    /// <summary>
    /// 任意基数でのデータを保持し、数値への変換や読み書きを提供するクラスです。
    /// </summary>
    public class UserBaseData
    {
        readonly uint[] elemData;

        /// <summary>
        /// 要素数を取得します。
        /// </summary>
        public uint ElemSize { get; }

        /// <summary>
        /// 基数（BaseNum）を取得します。
        /// </summary>
        public uint BaseNum { get; }

        /// <summary>
        /// 新しい UserBaseData インスタンスを初期化します。
        /// </summary>
        /// <param name="baseVal">基数を決定する値。</param>
        /// <param name="fixedBase">true の場合、BaseNum を 2^baseVal に設定します。</param>
        /// <param name="dataSize">データ要素の数。</param>
        public UserBaseData(uint baseVal, bool fixedBase, uint dataSize)
        {
            // fixedBase:trueでBaseNumが2^baseVal(=桁数)に、falseでそのままになる
            BaseNum = fixedBase ? (uint)Math.Pow(2, baseVal) : baseVal;
            ElemSize = dataSize;
            elemData = new uint[dataSize];
            for (int count = 0; count < ElemSize; ++count) elemData[count] = 0;
        }

        /// <summary>
        /// 指定したソースからディープコピーを生成します。
        /// </summary>
        /// <param name="deepCopySrc">コピー元の UserBaseData インスタンス。</param>
        public UserBaseData(UserBaseData deepCopySrc)
        {
            // 引数データのディープコピー(値渡し)を生成する
            BaseNum = deepCopySrc.BaseNum;
            ElemSize = deepCopySrc.ElemSize;
            elemData = new uint[deepCopySrc.elemData.Length];
            for (int count = 0; count < elemData.Length; ++count)
                elemData[count] = deepCopySrc.elemData[count];
        }

        /// <summary>
        /// 指定位置にデータを設定します。
        /// </summary>
        /// <param name="pos">設定位置のインデックス。</param>
        /// <param name="inputData">格納するデータ（0 以上 BaseNum 未満）。</param>
        /// <returns>設定が成功した場合に true を返します。位置が範囲外またはデータが基数以上の場合は false を返します。</returns>
        public bool SetData(uint pos, uint inputData)
        {
            if (pos >= ElemSize) return false;
            if (inputData >= BaseNum) return false;
            elemData[pos] = inputData;
            return true;
        }

        /// <summary>
        /// 指定位置のデータを取得します。
        /// </summary>
        /// <param name="pos">取得位置のインデックス。</param>
        /// <returns>データを返します。位置が範囲外の場合は uint.MaxValue を返します。</returns>
        public uint GetData(uint pos)
        {
            if (pos >= ElemSize) return uint.MaxValue;
            return elemData[pos];
        }

        /// <summary>
        /// 内部データを累乗計算して 10 進数として出力します。
        /// </summary>
        /// <returns>累乗計算された 10 進数の値を返します。</returns>
        public decimal Export()
        {
            decimal ans = 0;
            decimal expNum = 1;
            for (int count = 0; count < ElemSize; ++count)
            {
                if (count > 0) expNum *= BaseNum;
                ans += elemData[count] * expNum;
            }
            return ans;
        }

        /// <summary>
        /// 10 進数の値を内部の基数表現に変換して格納します。
        /// </summary>
        /// <param name="importData">変換元の 10 進数値。</param>
        public void Import(decimal importData)
        {
            for (int count = 0; count < ElemSize; ++count)
            {
                elemData[count] = decimal.ToUInt32(importData % BaseNum);
                importData /= BaseNum;
            }
        }
    }
}

