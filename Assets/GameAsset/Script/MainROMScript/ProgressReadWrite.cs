using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SlotMaker2022
{
    class ProgressRead
    {
        BinaryReader fs;
        public int FileVersion { get; private set; }

        public ProgressRead()
        {
            fs = null;
            FileVersion = 0;
        }
        // ファイルからデータを開く
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
        // byte[]からデータを開く(Unity読込用)
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

        // ILocalInterfaceを実装したクラスのデータを読み込む
        public bool ReadData(ILocalDataInterface data)
        {
            bool result = true;
            try
            {
                data.ReadData(ref fs, FileVersion);
            }
            catch (Exception){
                result = false;
            }
            return result;
        }
        // Listをまとめて読み込むオーバーライド
        public bool ReadData<Type>(List<Type> list) where Type : ILocalDataInterface, new()
        {
            bool result = true;
            try
            {
                int count = fs.ReadInt32();
                for(int i=0; i<count; ++i)
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
        public void Close()
        {
            fs.Close();
        }

        // checkSum計算
        bool VerifyCheckSum()
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

    class ProgressWrite
    {
        FileStream   fs;
        MemoryStream ms;
        BinaryWriter bw;
        int fileVersion;

        public ProgressWrite()
        {
            fs = null;
            bw = null;
            ms = new MemoryStream();
            fileVersion = 0;
        }
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

        public bool WriteData(ILocalDataInterface data)
        {
            bool result = true;
            try
            {
                data.StoreData(ref bw, fileVersion);
            }
            catch (Exception){
                result = false;
            }
            return result;
        }

        // Listをまとめて書き込むオーバーライド
        public bool WriteData<Type>(List<Type> list) where Type : ILocalDataInterface
        {
            bool result = true;
            try
            {
                bw.Write(list.Count);
                for(int i=0; i<list.Count; ++i)
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
        public void Flush()
        {
            // 各データからbwに入れたデータをmsへ流す。stream位置を先頭に戻す
            bw.Flush();
            ms.Seek(0, SeekOrigin.Begin);
            // ハッシュ値を計算しつつ、msからfsへデータを転送する
            byte hash = 0x0;
            BinaryWriter swFile = new BinaryWriter(fs);
            BinaryReader msRead = new BinaryReader(ms);
            while(msRead.BaseStream.Position != msRead.BaseStream.Length)
            {
                byte readData = msRead.ReadByte();
                hash ^= readData;
                swFile.Write(readData);
            }

            // hashを加えてファイルへ書き出す
            swFile.Write(hash);
            fs.Flush();
        }
        public void Close()
        {
            fs.Close();
            bw.Close();
            ms.Close();
        }
    }
}
