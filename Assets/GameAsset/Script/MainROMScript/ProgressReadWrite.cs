using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Compression;

namespace SlotMaker2022
{
    /// <summary>
    /// バイナリファイルの読み込みを段階的に行うユーティリティクラスです。
    /// ファイルオープン時にチェックサムを検証し、バージョン情報を読み取ります。
    /// </summary>
    class ProgressRead
    {
        private BinaryReader fs;
        /// <summary>読み込んだファイルのバージョン</summary>
        public int FileVersion { get; private set; }

        /// <summary>コンストラクタ。内部ストリームを初期化します。</summary>
        public ProgressRead()
        {
            fs = null;
            FileVersion = 0;
        }

        /// <summary>
        /// ファイルパスからデータを開きます。
        /// </summary>
        /// <param name="filePath">読み込むファイルのパス</param>
        /// <returns>オープンとチェックサム検証に成功したら true、それ以外は false</returns>
        public bool OpenFile(string filePath)
        {
            try
            {
                fs = new BinaryReader(new FileStream(filePath, FileMode.Open, FileAccess.Read));
            }
            catch (Exception)
            {
                fs = null;
                return false;
            }

            // checksumの検証を行う
            if (!VerifyCheckSum()) return false;
            // ファイルバージョンを取得する
            FileVersion = fs.ReadInt32();
            return true;
        }

        /// <summary>
        /// バイト配列からデータを開きます（Unity 用）。
        /// </summary>
        /// <param name="data">読み込むバイナリデータ</param>
        /// <returns>オープンとチェックサム検証に成功したら true、それ以外は false</returns>
        public bool OpenFile(byte[] data)
        {
            try
            {
                fs = new BinaryReader(new MemoryStream(data));
            }
            catch (Exception)
            {
                fs = null;
                return false;
            }

            // checksumの検証を行う
            if (!VerifyCheckSum()) return false;
            // ファイルバージョンを取得する
            FileVersion = fs.ReadInt32();
            return true;
        }

        /// <summary>
        /// 圧縮ファイルからデータを開きます（Deflate 圧縮）。
        /// </summary>
        /// <param name="filePath">圧縮ファイルのパス</param>
        /// <returns>オープンとチェックサム検証に成功したら true、それ以外は false</returns>
        public bool OpenCompressedFile(string filePath)
        {
            try
            {
                using (var fsBefComp = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    var dec = new DeflateStream(fsBefComp, CompressionMode.Decompress, true);
                    var mem = new MemoryStream();
                    dec.CopyTo(mem);
                    fs = new BinaryReader(mem);
                }
            }
            catch (Exception)
            {
                fs = null;
                return false;
            }

            // checksumの検証を行う
            if (!VerifyCheckSum()) return false;
            // ファイルバージョンを取得する
            FileVersion = fs.ReadInt32();
            return true;
        }

        /// <summary>
        /// 圧縮データ（バイト配列）から読み込みます（Deflate 圧縮、Unity 用）。
        /// </summary>
        /// <param name="data">読み込む圧縮バイナリデータ</param>
        /// <returns>オープンとチェックサム検証に成功したら true、それ以外は false</returns>
        public bool OpenCompressedFile(byte[] data)
        {
            try
            {
                using (var fsBefComp = new MemoryStream(data))
                {
                    var dec = new DeflateStream(fsBefComp, CompressionMode.Decompress);
                    var mem = new MemoryStream();
                    dec.CopyTo(mem);
                    fs = new BinaryReader(mem);
                }
            }
            catch (Exception)
            {
                fs = null;
                return false;
            }

            // checksumの検証を行う
            if (!VerifyCheckSum()) return false;
            // ファイルバージョンを取得する
            FileVersion = fs.ReadInt32();
            return true;
        }

        /// <summary>
        /// ILocalDataInterface を実装したオブジェクトからデータを読み込みます。
        /// </summary>
        /// <param name="data">読み込むデータオブジェクト</param>
        /// <returns>読み込みに成功したら true、それ以外は false</returns>
        public bool ReadData(ILocalDataInterface data)
        {
            bool result = true;
            try
            {
                data.ReadData(ref fs, FileVersion);
            }
            catch (Exception)
            {
                result = false;
            }
            return result;
        }

        /// <summary>
        /// List に対して要素数と各 ILocalDataInterface 要素をまとめて読み込みます。
        /// </summary>
        /// <typeparam name="Type">ILocalDataInterface を実装した型</typeparam>
        /// <param name="list">読み込んだ要素を追加するリスト</param>
        /// <returns>読み込みに成功したら true、それ以外は false</returns>
        public bool ReadData<Type>(List<Type> list) where Type : ILocalDataInterface, new()
        {
            bool result = true;
            try
            {
                int count = fs.ReadInt32();
                for (int i = 0; i < count; ++i)
                {
                    Type ad = new Type();
                    ad.ReadData(ref fs, FileVersion);
                    list.Add(ad);
                }
            }
            catch (Exception)
            {
                result = false;
            }
            return result;
        }

        /// <summary>
        /// ストリームを閉じます。
        /// </summary>
        public void Close()
        {
            fs.Close();
        }

        /// <summary>
        /// ストリーム先頭からチェックサムを計算・検証します。
        /// </summary>
        /// <returns>チェックサムが一致すれば true、それ以外は false</returns>
        private bool VerifyCheckSum()
        {
            // ストリームの先頭から検証を行う
            fs.BaseStream.Seek(0, SeekOrigin.Begin);

            // checkSum検証: 最後の1byteを除いてcheckSumを検証する
            byte hash = 0x0;
            byte lastReadData = 0x0;
            while (fs.BaseStream.Position != fs.BaseStream.Length)
            {
                hash ^= lastReadData;
                lastReadData = fs.ReadByte();
            }

            // ストリームを先頭に戻す
            fs.BaseStream.Seek(0, SeekOrigin.Begin);
            // 最後に読んだデータがhash値なのでこれと計算結果が一致するか返す
            return lastReadData == hash;
        }
    }

    /// <summary>
    /// バイナリデータの書き出しを段階的に行うユーティリティクラスです。
    /// メモリストリームに溜めたデータを最終的にファイルに書き出し、チェックサムを付加します。
    /// </summary>
    class ProgressWrite
    {
        private FileStream fs;
        private MemoryStream ms;
        private BinaryWriter bw;
        private int fileVersion;

        /// <summary>コンストラクタ。内部ストリームを初期化します。</summary>
        public ProgressWrite()
        {
            fs = null;
            bw = null;
            ms = new MemoryStream();
            fileVersion = 0;
        }

        /// <summary>
        /// ファイルを書き出す準備をします。
        /// </summary>
        /// <param name="filePath">出力先ファイルパス</param>
        /// <param name="version">ファイルバージョン</param>
        /// <returns>準備に成功したら true、それ以外は false</returns>
        public bool OpenFile(string filePath, int version)
        {
            try
            {
                // 最終書き出しに向けたファイルストリームを生成する
                fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                // データ書き出し先のメモリストリームを生成する
                bw = new BinaryWriter(ms);
                // ファイルの冒頭にバージョン情報を付ける
                fileVersion = version;
                bw.Write(fileVersion);
            }
            catch (FileNotFoundException)
            {
                fs = null;
                return false;
            }
            return true;
        }

        /// <summary>
        /// ILocalDataInterface を実装したオブジェクトのデータを書き出します。
        /// </summary>
        /// <param name="data">書き出すデータオブジェクト</param>
        /// <returns>書き出しに成功したら true、それ以外は false</returns>
        public bool WriteData(ILocalDataInterface data)
        {
            bool result = true;
            try
            {
                data.StoreData(ref bw, fileVersion);
            }
            catch (Exception)
            {
                result = false;
            }
            return result;
        }

        /// <summary>
        /// List に対して要素数と各 ILocalDataInterface 要素をまとめて書き出します。
        /// </summary>
        /// <typeparam name="Type">ILocalDataInterface を実装した型</typeparam>
        /// <param name="list">書き出す要素を持つリスト</param>
        /// <returns>書き出しに成功したら true、それ以外は false</returns>
        public bool WriteData<Type>(List<Type> list) where Type : ILocalDataInterface
        {
            bool result = true;
            try
            {
                bw.Write(list.Count);
                for (int i = 0; i < list.Count; ++i)
                {
                    list[i].StoreData(ref bw, fileVersion);
                }
            }
            catch (Exception)
            {
                result = false;
            }
            return result;
        }

        /// <summary>
        /// メモリストリームの内容をファイルに書き出し、チェックサムを付加します。
        /// </summary>
        public void Flush()
        {
            // 各データからbwに入れたデータをmsへ流す。stream位置を先頭に戻す
            bw.Flush();
            ms.Seek(0, SeekOrigin.Begin);
            // ハッシュ値を計算しつつ、msからfsへデータを転送する
            byte hash = 0x0;
            BinaryWriter swFile = new BinaryWriter(fs);
            BinaryReader msRead = new BinaryReader(ms);
            while (msRead.BaseStream.Position != msRead.BaseStream.Length)
            {
                byte readData = msRead.ReadByte();
                hash ^= readData;
                swFile.Write(readData);
            }

            // hashを加えてファイルへ書き出す
            swFile.Write(hash);
            fs.Flush();
        }

        /// <summary>
        /// 圧縮（Deflate）をかけてメモリストリームの内容をファイルに書き出し、チェックサムを付加します。
        /// </summary>
        public void FlushCompressed()
        {
            // 各データからbwに入れたデータをmsへ流す。stream位置を先頭に戻す
            bw.Flush();
            ms.Seek(0, SeekOrigin.Begin);
            // ハッシュ値を計算しつつ、msから圧縮用ストリームcmpへデータを転送する
            byte hash = 0x0;
            MemoryStream cmp = new MemoryStream();
            BinaryWriter swFile = new BinaryWriter(cmp);
            BinaryReader msRead = new BinaryReader(ms);
            while (msRead.BaseStream.Position != msRead.BaseStream.Length)
            {
                byte readData = msRead.ReadByte();
                hash ^= readData;
                swFile.Write(readData);
            }
            // hashを加える
            swFile.Write(hash);
            swFile.Flush();
            cmp.Seek(0, SeekOrigin.Begin);

            // 圧縮をかける
            var cmpStream = new DeflateStream(fs, CompressionMode.Compress, true);
            var cmpData = cmp.ToArray();
            cmpStream.Write(cmpData, 0, cmpData.Length);
            cmpStream.Close();
            // ファイルへ書き出す
            fs.Flush();
        }

        /// <summary>
        /// すべてのストリームを閉じます。
        /// </summary>
        public void Close()
        {
            fs.Close();
            bw.Close();
            ms.Close();
        }
    }
}
