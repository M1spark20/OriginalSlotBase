using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SlotMaker2022
{
    class ProgressRWConstant
    {
        public const string FILE_NAME = "Assets/Resources/mainROM.bin";
        public const int FILE_VERSION = 0;
    }
    class ProgressRead
    {
        BinaryReader fs;

        public ProgressRead()
        {
            fs = null;
        }
        public bool OpenFile()
        {
            try
            {
                fs = new BinaryReader(new FileStream(ProgressRWConstant.FILE_NAME, FileMode.Open, FileAccess.Read));
            }
            catch (Exception)
            {
                //MessageBox.Show("保存データが見つかりませんでした。読み込みをスキップします。", "情報", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                fs = null;
                return false;
            }
            return true;
        }
        public bool ReadData(ILocalDataInterface data)
        {
            bool result = true;
            try
            {
                data.ReadData(ref fs, ProgressRWConstant.FILE_VERSION);
            }
            catch (Exception){
                //MessageBox.Show("データ取得処理に失敗しました。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    ad.ReadData(ref fs, ProgressRWConstant.FILE_VERSION);
                    list.Add(ad);
                }
            }
            catch (Exception)
            {
                //MessageBox.Show("データ取得処理に失敗しました。", "エラー(List)", MessageBoxButtons.OK, MessageBoxIcon.Error);
                result = false;
            }
            return result;
        }
        public void Close()
        {
            fs.Close();
        }
    }

    class ProgressWrite
    {
        BinaryWriter fs;
        public ProgressWrite()
        {
            fs = null;
        }
        public bool OpenFile()
        {
            try
            {
                fs = new BinaryWriter(new FileStream(ProgressRWConstant.FILE_NAME, FileMode.Create, FileAccess.Write));
            }
            catch (FileNotFoundException)
            {
                //MessageBox.Show("保存ファイルのオープンに失敗しました。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                fs = null;
                return false;
            }
            return true;
        }
        // バックアップ生成用
        public bool OpenFile(string fileName)
        {
            try
            {
                fs = new BinaryWriter(new FileStream(fileName, FileMode.Create, FileAccess.Write));
            }
            catch (FileNotFoundException)
            {
                //MessageBox.Show("保存ファイルのオープンに失敗しました。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                data.StoreData(ref fs, ProgressRWConstant.FILE_VERSION);
            }
            catch (Exception){
               // MessageBox.Show("保存ファイルへの書き込みに失敗しました。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                fs.Write(list.Count);
                for(int i=0; i<list.Count; ++i)
                {
                    list[i].StoreData(ref fs, ProgressRWConstant.FILE_VERSION);
                }
            }
            catch (Exception)
            {
                //MessageBox.Show("保存ファイルへの書き込みに失敗しました。", "エラー(List)", MessageBoxButtons.OK, MessageBoxIcon.Error);
                result = false;
            }
            return result;
        }
        public void Flush()
        {
            fs.Flush();
        }
        public void Close()
        {
            fs.Close();
        }
    }
}
