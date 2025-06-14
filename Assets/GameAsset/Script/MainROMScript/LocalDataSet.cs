using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SlotMaker2022
{
    /// <summary>
    /// スロットマシンに使用する定数およびデータ定義クラスをまとめたクラスです。
    /// </summary>
    public class LocalDataSet
    {
        public const int REEL_MAX = 3;
        public const int COMA_MAX = 21;
        public const int SLIP_MAX = 5; // スベリ最大数+1を定義
        public const int SLIP_CT = 2;   // CT中最大スベリ。スベリ最大数+1を定義
        public const int SHOW_MAX = 3; // リール表示コマ数(3コマ)
        public const int PAYLINE_MAX = 6;
        public const int GAMEMODE_MAX = 4;
        public const int BET_MAX = 3;
        public const int SYMBOL_MAX = 10;   // リールに定義できるシンボル数の最大数
        public const int BONUSFLAG_MAX = 8;
        public const int RTMODE_MAX = 7;
        public const int SETTING_MAX = 6;
        public const int GAME_COUNTER_MAX = 3;

        /// <summary>
        /// ソフトウェア情報を保持し、バイナリストリームへの読み書きを行うクラスです。
        /// </summary>
        public class SoftwareInformation : ILocalDataInterface
        {
            public string ReelChipPath { get; set; }
            public int ReachPosRow { get; set; }    // ReachDataのROW位置
            public int ReachPosCol { get; set; }    // ReachDataのCol位置
            public int ReachBet { get; set; }       // ReachDataのBET数
            public int ReachGameMode { get; set; }  // ReachDataのGameMode
            public int ReachReelPos { get; set; }   // ReachDataのリール位置
            public int ReachComaPos { get; set; }   // ReachDataのコマ位置
            public int CtrlBet { get; set; }        // ctrlのBET位置
            public int CtrlGameMode { get; set; }   // ctrlのMode位置
            public int CtrlBonusFlag { get; set; }  // ctrlのBonus位置
            public int CtrlCastFlag { get; set; }   // ctrlのCast位置

            /// <summary>
            /// インスタンスを初期化し、フィールドをデフォルト値で設定します。
            /// </summary>
            public SoftwareInformation()
            {
                ReelChipPath = "";
                ReachPosRow = 0;
                ReachPosCol = 0;
                ReachBet = BET_MAX - 1;
                ReachGameMode = 0;
                ReachReelPos = 0;
                ReachComaPos = COMA_MAX - 1;
                CtrlBet = BET_MAX - 1;
                CtrlGameMode = 0;
                CtrlBonusFlag = 0;
                CtrlCastFlag = 0;
            }

            /// <summary>
            /// ソフトウェア情報をバイナリに書き出します。
            /// </summary>
            /// <param name="fs">書き込み先の BinaryWriter への参照</param>
            /// <param name="version">データバージョン番号</param>
            /// <returns>書き込みが成功した場合は true、それ以外は false</returns>
            public bool StoreData(ref BinaryWriter fs, int version)
            {
                fs.Write(ReelChipPath);
                fs.Write(ReachPosRow);
                fs.Write(ReachPosCol);
                fs.Write(ReachBet);
                fs.Write(ReachGameMode);
                fs.Write(ReachReelPos);
                fs.Write(ReachComaPos);
                fs.Write(CtrlBet);
                fs.Write(CtrlGameMode);
                fs.Write(CtrlBonusFlag);
                fs.Write(CtrlCastFlag);
                return true;
            }

            /// <summary>
            /// バイナリストリームからソフトウェア情報を読み込みます。
            /// </summary>
            /// <param name="fs">読み込み元の BinaryReader への参照</param>
            /// <param name="version">データバージョン番号</param>
            /// <returns>読み込みが成功した場合は true、それ以外は false</returns>
            public bool ReadData(ref BinaryReader fs, int version)
            {
                ReelChipPath = fs.ReadString();
                ReachPosRow = fs.ReadInt32();
                ReachPosCol = fs.ReadInt32();
                ReachBet = fs.ReadInt32();
                ReachGameMode = fs.ReadInt32();
                ReachReelPos = fs.ReadInt32();
                ReachComaPos = fs.ReadInt32();
                CtrlBet = fs.ReadInt32();
                CtrlGameMode = fs.ReadInt32();
                CtrlBonusFlag = fs.ReadInt32();
                CtrlCastFlag = fs.ReadInt32();
                return true;
            }
        }

        /// <summary>
        /// リール配列データを保持し、バイナリへの読み書きを行うクラスです。
        /// </summary>
        public class ReelArray : ILocalDataInterface
        {
            public int Pos { get; set; }
            public byte Coma { get; set; }
            public bool Ex13 { get; set; }
            public bool Ex12 { get; set; }
            public bool Ex11 { get; set; }
            public bool Ex10 { get; set; }

            /// <summary>
            /// インスタンスを初期化し、フィールドをデフォルト値で設定します。
            /// </summary>
            public ReelArray()
            {
                this.Pos = 0;
                this.Coma = 0;
                this.Ex13 = false;
                this.Ex12 = false;
                this.Ex11 = false;
                this.Ex10 = false;
            }

            /// <summary>
            /// リール配列データをバイナリに書き出します。
            /// </summary>
            /// <param name="fs">書き込み先の BinaryWriter への参照</param>
            /// <param name="version">データバージョン番号</param>
            /// <returns>書き込みが成功した場合は true、それ以外は false</returns>
            public bool StoreData(ref BinaryWriter fs, int version)
            {
                fs.Write(Coma);
                fs.Write(Ex13);
                fs.Write(Ex12);
                fs.Write(Ex11);
                fs.Write(Ex10);
                return true;
            }

            /// <summary>
            /// バイナリストリームからリール配列データを読み込みます。
            /// </summary>
            /// <param name="fs">読み込み元の BinaryReader への参照</param>
            /// <param name="version">データバージョン番号</param>
            /// <returns>読み込みが成功した場合は true、それ以外は false</returns>
            public bool ReadData(ref BinaryReader fs, int version)
            {
                Coma = fs.ReadByte();
                Ex13 = fs.ReadBoolean();
                Ex12 = fs.ReadBoolean();
                Ex11 = fs.ReadBoolean();
                Ex10 = fs.ReadBoolean();
                return true;
            }
        }

        /// <summary>
        /// 配当関係の共通データを管理し、バイナリ読み書きを行うクラスです。
        /// </summary>
        public class CastCommonData : ILocalDataInterface
        {
            // 定数
            public const int PAYOUT_DATA_MAX = 6;
            public const int BONUSPAY_DATA_MAX = 3;
            public const int GAMENUM_DATA_MAX = 3;
            public const int INTERVAL_UNIT_PAY = 25;
            public const int INTERVAL_UNIT_REP = 100;
            public const int INTERVAL_RANGE_MAX = 15;
            public const int PAYOUT_NUM_MAX = 15;

            // 有効BET数定義
            public UserBaseData CanUseBet { get; set; }
            // 最大払い出し枚数
            public UserBaseData MaxPayout { get; set; }
            // 有効ライン定義データ
            public UserBaseData PayLineData { get; set; }
            // 有効ライン選択データ
            public UserBaseData AvailableLineData { get; set; }
            // フラグ優先度(Trueでボーナス優先制御になる)
            public bool IsPriorBonus { get; set; }
            // 払い出し枚数定義
            public UserBaseData PayoutData { get; set; }
            // ボーナス上限定義
            public UserBaseData BonusPayData { get; set; }
            // ゲーム数上限定義(ボーナス・RT)
            public UserBaseData GameNumData { get; set; }
            // 払い出しインターバル
            public uint IntervalPay { get; set; }
            // リプレイインターバル
            public uint IntervalRep { get; set; }

            /// <summary>
            /// インスタンスを初期化し、各 UserBaseData を生成します。
            /// </summary>
            public CastCommonData()
            {
                CanUseBet = new UserBaseData(1, true, BET_MAX * GAMEMODE_MAX);
                MaxPayout = new UserBaseData(PAYOUT_NUM_MAX + 1, false, BET_MAX);
                PayLineData = new UserBaseData((uint)Math.Pow(REEL_MAX, SHOW_MAX) + 1, false, PAYLINE_MAX);
                AvailableLineData = new UserBaseData(1, true, PAYLINE_MAX * BET_MAX);
                IsPriorBonus = false;
                PayoutData = new UserBaseData(PAYOUT_NUM_MAX + 1, false, PAYOUT_DATA_MAX);
                BonusPayData = new UserBaseData(9, true, BONUSPAY_DATA_MAX);
                GameNumData = new UserBaseData(10, true, GAMENUM_DATA_MAX);

                IntervalPay = 0;
                IntervalRep = 0;
            }

            /// <summary>
            /// 配当共通データをバイナリに書き出します。
            /// </summary>
            /// <param name="fs">書き込み先の BinaryWriter への参照</param>
            /// <param name="version">データバージョン番号</param>
            /// <returns>書き込みが成功した場合は true、それ以外は false</returns>
            public bool StoreData(ref BinaryWriter fs, int version)
            {
                fs.Write(decimal.ToUInt32(CanUseBet.Export()));
                fs.Write(decimal.ToUInt32(MaxPayout.Export()));
                fs.Write(decimal.ToUInt32(PayLineData.Export()));
                fs.Write(decimal.ToUInt32(AvailableLineData.Export()));
                fs.Write(IsPriorBonus);
                fs.Write(decimal.ToUInt32(PayoutData.Export()));
                fs.Write(decimal.ToUInt32(BonusPayData.Export()));
                fs.Write(decimal.ToUInt32(GameNumData.Export()));

                fs.Write(IntervalPay);
                fs.Write(IntervalRep);
                return true;
            }

            /// <summary>
            /// バイナリストリームから配当共通データを読み込みます。
            /// </summary>
            /// <param name="fs">読み込み元の BinaryReader への参照</param>
            /// <param name="version">データバージョン番号</param>
            /// <returns>読み込みが成功した場合は true、それ以外は false</returns>
            public bool ReadData(ref BinaryReader fs, int version)
            {
                CanUseBet.Import(fs.ReadUInt32());
                MaxPayout.Import(fs.ReadUInt32());
                PayLineData.Import(fs.ReadUInt32());
                AvailableLineData.Import(fs.ReadUInt32());
                IsPriorBonus = fs.ReadBoolean();
                PayoutData.Import(fs.ReadUInt32());
                BonusPayData.Import(fs.ReadUInt32());
                GameNumData.Import(fs.ReadUInt32());

                IntervalPay = fs.ReadUInt32();
                IntervalRep = fs.ReadUInt32();
                return true;
            }
        }

        /// <summary>
        /// キャスト要素データを管理するクラスです。
        /// </summary>
        public class CastElemData : ILocalDataInterface
        {
            // 有効モード定義
            public UserBaseData AvailGameMode { get; set; }
            // 払い出しシンボル定義
            public UserBaseData PaySymbol { get; set; }
            // 払い出し枚数定義(0:設定なし、1:index=0～)
            public UserBaseData PayoutNumID { get; set; }
            // 状態移行フラグ
            public bool ChangeGameModeFlag { get; set; }
            public byte ChangeGameModeDest { get; set; }
            // シフト移行フラグ
            public bool IsShift { get; set; }
            // RT移行フラグ(移行有無・上書き・移行先)
            public bool ChangeRTFlag { get; set; }
            public bool CanOverwriteRT { get; set; }
            public byte ChangeRTDest { get; set; }
            // 払い出し上限定義
            public byte BonusPayoutMaxID { get; set; }
            // ゲーム数上限定義
            public byte BonusGameMaxID { get; set; }
            // ボーナスフラグ
            public byte ValidateBonusFlag { get; set; }
            // 払い出し待機時間(0:設定なし、1:index=0～)
            public byte PayoutIntervalID { get; set; }
            // 入賞時G数リセットID
            public byte ResetGame { get; set; }
            // フラグ名(ROMデータには反映しない)
            public string FlagName { get; set; }

            /// <summary>
            /// 新しいインスタンスを初期化します。
            /// 事前に要素数を読み込んでデータを作成します。
            /// </summary>
            public CastElemData()
            {
                // 事前に要素数を読み込んでデータを作成すること
                AvailGameMode = new UserBaseData(1, true, GAMEMODE_MAX);
                PaySymbol = new UserBaseData(1, true, SYMBOL_MAX * REEL_MAX);
                PayoutNumID = new UserBaseData(3, true, BET_MAX);
                ChangeGameModeFlag = false;
                ChangeGameModeDest = 0;
                IsShift = false;
                ChangeRTFlag = false;
                CanOverwriteRT = false;
                ChangeRTDest = 0;
                BonusPayoutMaxID = 0;
                BonusGameMaxID = 0;
                ValidateBonusFlag = 0;
                PayoutIntervalID = 0;
                ResetGame = 0;
                FlagName = "";
            }

            /// <summary>
            /// 指定したバイナリライターにデータを書き込みます。
            /// </summary>
            /// <param name="fs">データを書き込む BinaryWriter への参照</param>
            /// <param name="version">書き込み対象のデータバージョン</param>
            /// <returns>書き込みに成功した場合は true、失敗した場合は false</returns>
            public bool StoreData(ref BinaryWriter fs, int version)
            {
                fs.Write(decimal.ToByte(AvailGameMode.Export()));
                fs.Write(decimal.ToUInt32(PaySymbol.Export()));
                fs.Write(decimal.ToUInt32(PayoutNumID.Export()));
                fs.Write(ChangeGameModeFlag);
                fs.Write(ChangeGameModeDest);
                fs.Write(IsShift);
                fs.Write(ChangeRTFlag);
                fs.Write(CanOverwriteRT);
                fs.Write(ChangeRTDest);
                fs.Write(BonusPayoutMaxID);
                fs.Write(BonusGameMaxID);
                fs.Write(ValidateBonusFlag);
                fs.Write(PayoutIntervalID);
                fs.Write(ResetGame);
                fs.Write(FlagName);
                return true;
            }

            /// <summary>
            /// 指定したバイナリリーダーからデータを読み込みます。
            /// </summary>
            /// <param name="fs">データを読み込む BinaryReader への参照</param>
            /// <param name="version">読み込み対象のデータバージョン</param>
            /// <returns>読み込みに成功した場合は true、失敗した場合は false</returns>
            public bool ReadData(ref BinaryReader fs, int version)
            {
                AvailGameMode.Import(fs.ReadByte());
                PaySymbol.Import(fs.ReadUInt32());
                PayoutNumID.Import(fs.ReadUInt32());
                ChangeGameModeFlag = fs.ReadBoolean();
                ChangeGameModeDest = fs.ReadByte();
                IsShift = fs.ReadBoolean();
                ChangeRTFlag = fs.ReadBoolean();
                CanOverwriteRT = fs.ReadBoolean();
                ChangeRTDest = fs.ReadByte();
                BonusPayoutMaxID = fs.ReadByte();
                BonusGameMaxID = fs.ReadByte();
                ValidateBonusFlag = fs.ReadByte();
                PayoutIntervalID = fs.ReadByte();
                ResetGame = fs.ReadByte();
                FlagName = fs.ReadString();
                return true;
            }
        }
        /// <summary>
        /// フラグ関係データを管理するクラスです。
        /// </summary>
        public class FlagCommonData : ILocalDataInterface
        {
            /// <summary>
            /// 乱数最大値
            /// </summary>
            public const int RAND_MAX = 65536;

            // フラグ数
            public byte FlagNum { get; set; }
            // 抽選データ数
            public byte RandNum { get; set; }
            // 当該機種の最大設定数
            public byte SetNum { get; set; }

            /// <summary>
            /// 新しいインスタンスを初期化します。
            /// </summary>
            public FlagCommonData()
            {
                FlagNum = 0;
                RandNum = 0;
                SetNum = SETTING_MAX;
            }

            /// <summary>
            /// データをバイナリライターに書き込みます。
            /// </summary>
            /// <param name="fs">データを書き込む BinaryWriter への参照</param>
            /// <param name="version">データのバージョン</param>
            /// <returns>書き込みに成功した場合は true</returns>
            public bool StoreData(ref BinaryWriter fs, int version)
            {
                fs.Write(FlagNum);
                fs.Write(RandNum);
                fs.Write(SetNum);
                return true;
            }

            /// <summary>
            /// バイナリリーダーからデータを読み込みます。
            /// </summary>
            /// <param name="fs">データを読み込む BinaryReader への参照</param>
            /// <param name="version">データのバージョン</param>
            /// <returns>読み込みに成功した場合は true</returns>
            public bool ReadData(ref BinaryReader fs, int version)
            {
                FlagNum = fs.ReadByte();
                RandNum = fs.ReadByte();
                SetNum = fs.ReadByte();
                return true;
            }
        }

        /// <summary>
        /// フラグ要素データを管理するクラスです。
        /// </summary>
        public class FlagElemData : ILocalDataInterface
        {
            // 成立時CT制御指定(1コマすべり)
            public byte ControlCT { get; set; }
            // 小役フラグ(開始)
            public byte FlagBegin { get; set; }
            // 小役フラグ(終了：指定した番号を含む)
            public byte FlagEnd { get; set; }
            // リプレイフラグ
            public UserBaseData IsReplay { get; set; }
            // フラグ名指定(ユーザ入力)
            public string UserFlagName { get; set; }

            /// <summary>
            /// 新しいインスタンスを初期化します。
            /// </summary>
            public FlagElemData()
            {
                ControlCT = 0;
                FlagBegin = 0;
                FlagEnd = 0;
                IsReplay = new UserBaseData(1, true, BET_MAX);
                UserFlagName = string.Empty;
            }

            /// <summary>
            /// データをバイナリライターに書き込みます。
            /// </summary>
            /// <param name="fs">データを書き込む BinaryWriter への参照</param>
            /// <param name="version">データのバージョン</param>
            /// <returns>書き込みに成功した場合は true</returns>
            public bool StoreData(ref BinaryWriter fs, int version)
            {
                fs.Write(ControlCT);
                fs.Write(FlagBegin);
                fs.Write(FlagEnd);
                fs.Write(decimal.ToByte(IsReplay.Export()));
                fs.Write(UserFlagName);
                return true;
            }

            /// <summary>
            /// バイナリリーダーからデータを読み込みます。
            /// </summary>
            /// <param name="fs">データを読み込む BinaryReader への参照</param>
            /// <param name="version">データのバージョン</param>
            /// <returns>読み込みに成功した場合は true</returns>
            public bool ReadData(ref BinaryReader fs, int version)
            {
                ControlCT = fs.ReadByte();
                FlagBegin = fs.ReadByte();
                FlagEnd = fs.ReadByte();
                IsReplay.Import(fs.ReadByte());
                UserFlagName = fs.ReadString();
                return true;
            }
        }
        /// <summary>
        /// ボーナスフラグ抽選データを管理するクラスです。
        /// </summary>
        public class FlagRandData : ILocalDataInterface
        {
            // ボーナスフラグ定義
            public byte BonusFlag { get; set; }
            // 当選時成立フラグ
            public byte LaunchFlagID { get; set; }
            // 抽選対象ベット数
            public byte CondBet { get; set; }
            // 抽選対象状態
            public byte CondGameMode { get; set; }
            // 抽選対象RT状態
            public byte CondRTMode { get; set; }
            // 設定不問フラグ
            public bool CommonSet { get; set; }
            // 乱数データ(書き出し時はdecimalで行う。仮数部96bit = 16*6bitより格納可能)
            // CommonSet = trueの時は、index:0のデータを参考とすること
            public UserBaseData RandValue { get; set; }

            /// <summary>
            /// 新しいインスタンスを初期化します。
            /// </summary>
            public FlagRandData()
            {
                BonusFlag = 0;
                LaunchFlagID = 0;
                CondBet = 0;
                CondGameMode = 0;
                CondRTMode = 0;
                CommonSet = false;

                // 最大設定数に関わらず配列数はSETTING_MAX分確保しておく
                RandValue = new UserBaseData(16, true, SETTING_MAX);
            }

            /// <summary>
            /// データをバイナリライターに書き込みます。
            /// </summary>
            /// <param name="fs">データを書き込む BinaryWriter への参照</param>
            /// <param name="version">データのバージョン</param>
            /// <returns>書き込みに成功した場合は true</returns>
            public bool StoreData(ref BinaryWriter fs, int version)
            {
                fs.Write(BonusFlag);
                fs.Write(LaunchFlagID);
                fs.Write(CondBet);
                fs.Write(CondGameMode);
                fs.Write(CondRTMode);
                fs.Write(CommonSet);
                fs.Write(RandValue.Export());   // decimal型
                return true;
            }

            /// <summary>
            /// バイナリリーダーからデータを読み込みます。
            /// </summary>
            /// <param name="fs">データを読み込む BinaryReader への参照</param>
            /// <param name="version">データのバージョン</param>
            /// <returns>読み込みに成功した場合は true</returns>
            public bool ReadData(ref BinaryReader fs, int version)
            {
                BonusFlag = fs.ReadByte();
                LaunchFlagID = fs.ReadByte();
                CondBet = fs.ReadByte();
                CondGameMode = fs.ReadByte();
                CondRTMode = fs.ReadByte();
                CommonSet = fs.ReadBoolean();
                RandValue.Import(fs.ReadDecimal());
                return true;
            }
        }

        /// <summary>
        /// RT継続ゲーム数およびRT移行データを管理するクラスです。
        /// </summary>
        public class RTCommonData : ILocalDataInterface
        {
            // RT継続ゲーム数データ
            public UserBaseData ContGameNum { get; set; }
            // 状態移行時RT遷移データ数
            public byte MoveRTNum { get; set; }
            // ボーナス成立時RT移行先
            public UserBaseData MoveByBonus { get; set; }

            /// <summary>
            /// 新しいインスタンスを初期化します。
            /// </summary>
            public RTCommonData()
            {
                ContGameNum = new UserBaseData(CastCommonData.GAMENUM_DATA_MAX + 1, false, RTMODE_MAX);
                MoveRTNum = 0;
                MoveByBonus = new UserBaseData(RTMODE_MAX + 1, false, BONUSFLAG_MAX - 1);
            }

            /// <summary>
            /// データをバイナリライターに書き込みます。
            /// </summary>
            /// <param name="fs">データを書き込む BinaryWriter への参照</param>
            /// <param name="version">データのバージョン</param>
            /// <returns>書き込みに成功した場合は true</returns>
            public bool StoreData(ref BinaryWriter fs, int version)
            {
                fs.Write(decimal.ToUInt16(ContGameNum.Export()));
                fs.Write(MoveRTNum);
                fs.Write(decimal.ToUInt32(MoveByBonus.Export()));
                return true;
            }

            /// <summary>
            /// バイナリリーダーからデータを読み込みます。
            /// </summary>
            /// <param name="fs">データを読み込む BinaryReader への参照</param>
            /// <param name="version">データのバージョン</param>
            /// <returns>読み込みに成功した場合は true</returns>
            public bool ReadData(ref BinaryReader fs, int version)
            {
                ContGameNum.Import(fs.ReadUInt16());
                MoveRTNum = fs.ReadByte();
                MoveByBonus.Import(fs.ReadUInt32());
                return true;
            }
        }
        /// <summary>
        /// RT移行データを管理するクラスです。
        /// </summary>
        public class RTMoveData : ILocalDataInterface
        {
            // 状態移行元GameMode
            public byte ModeSrc { get; set; }
            // 状態移行先GameMode
            public byte ModeDst { get; set; }
            // RT上書き可否フラグ
            public bool CanOverride { get; set; }
            // RT移行先
            public byte Destination { get; set; }

            /// <summary>
            /// 新しいインスタンスを初期化します。
            /// </summary>
            public RTMoveData()
            {
                ModeSrc = 0;
                ModeDst = 0;
                CanOverride = false;
                Destination = 0;
            }

            /// <summary>
            /// データをバイナリライターに書き込みます。
            /// </summary>
            /// <param name="fs">データを書き込む BinaryWriter への参照</param>
            /// <param name="version">データのバージョン</param>
            /// <returns>書き込みに成功した場合は true</returns>
            public bool StoreData(ref BinaryWriter fs, int version)
            {
                fs.Write(ModeSrc);
                fs.Write(ModeDst);
                fs.Write(CanOverride);
                fs.Write(Destination);
                return true;
            }

            /// <summary>
            /// バイナリリーダーからデータを読み込みます。
            /// </summary>
            /// <param name="fs">データを読み込む BinaryReader への参照</param>
            /// <param name="version">データのバージョン</param>
            /// <returns>読み込みに成功した場合は true</returns>
            public bool ReadData(ref BinaryReader fs, int version)
            {
                ModeSrc = fs.ReadByte();
                ModeDst = fs.ReadByte();
                CanOverride = fs.ReadBoolean();
                Destination = fs.ReadByte();
                return true;
            }
        }

        /// <summary>
        /// フリーズ共通データを管理するクラスです。
        /// </summary>
        public class FreezeCommonData : ILocalDataInterface
        {
            // フリーズ制御データ数
            public byte ControlNum { get; set; }
            // フリーズ時間データ数
            public byte TimeNum { get; set; }

            /// <summary>
            /// 新しいインスタンスを初期化します。
            /// </summary>
            public FreezeCommonData()
            {
                ControlNum = 0;
                TimeNum = 0;
            }

            /// <summary>
            /// データをバイナリライターに書き込みます。
            /// </summary>
            /// <param name="fs">データを書き込む BinaryWriter への参照</param>
            /// <param name="version">データのバージョン</param>
            /// <returns>書き込みに成功した場合は true</returns>
            public bool StoreData(ref BinaryWriter fs, int version)
            {
                fs.Write(ControlNum);
                fs.Write(TimeNum);
                return true;
            }

            /// <summary>
            /// バイナリリーダーからデータを読み込みます。
            /// </summary>
            /// <param name="fs">データを読み込む BinaryReader への参照</param>
            /// <param name="version">データのバージョン</param>
            /// <returns>読み込みに成功した場合は true</returns>
            public bool ReadData(ref BinaryReader fs, int version)
            {
                ControlNum = fs.ReadByte();
                TimeNum = fs.ReadByte();
                return true;
            }
        }
        /// <summary>
        /// フリーズ制御データを管理するクラスです。
        /// フリーズの種類やタイミング、抽選条件などを保持し、シリアライズ/デシリアライズを提供します。
        /// </summary>
        public class FreezeControlData : ILocalDataInterface
        {
            // 定数定義
            /// <summary>
            /// フリーズ制御タイプを表す列挙型です。
            /// </summary>
            public enum FreezeControlType : byte
            {
                Flag, Cast, Mode, RT
            }

            /// <summary>
            /// フリーズ発生タイミングを表す列挙型です。
            /// </summary>
            public enum FreezeTiming : byte
            {
                BeforeWait, AfterWait, Stop1st, Stop2nd, BeforePay, AfterPay, AddGames, Reset
            };

            // 条件判定インターフェイス
            /// <summary>
            /// フリーズ抽選条件のインターフェイスです。
            /// 条件データをエクスポートする Export メソッドを提供します。
            /// </summary>
            public interface IFreezeCond
            {
                /// <summary>
                /// 条件データをバイナリ形式のバイトとして取得します。
                /// </summary>
                /// <returns>エクスポートされた条件データのバイト値</returns>
                byte Export();
            }

            // 条件判定：フラグ成立/小役入賞
            /// <summary>
            /// フリーズ条件として小役入賞やボーナス成立を判定するクラスです。
            /// NoBonusFlag と FlagID に基づいて条件をバイトデータにエクスポートします。
            /// </summary>
            public class FreezeCondFlag : IFreezeCond
            {
                // ボーナス非成立時or成立G限定か(true: 非成立時or成立G限定)
                public bool NoBonusFlag { get; set; }
                // 成立フラグID/入賞小役ID
                public byte FlagID { get; set; }

                /// <summary>
                /// 指定の条件データを解析して初期化します。
                /// </summary>
                /// <param name="pConditionData">フラグおよびボーナス条件を含むバイトデータ</param>
                public FreezeCondFlag(byte pConditionData)
                {
                    FlagID = (byte)(pConditionData & 0x7F);
                    NoBonusFlag = (pConditionData & 0x80) != 0;
                }

                /// <summary>
                /// デフォルト値でインスタンスを初期化します。
                /// </summary>
                public FreezeCondFlag()
                {
                    NoBonusFlag = true;
                    FlagID = 0;
                }

                /// <summary>
                /// 現在の条件をバイト形式でエクスポートします。
                /// </summary>
                /// <returns>エクスポートされた条件データのバイト値</returns>
                public byte Export()
                {
                    byte ans = FlagID;
                    if (NoBonusFlag) ans |= 0x80;
                    return ans;
                }
            }
            /// <summary>
            /// 条件判定：ボーナスフラグおよびモード変化を判定するクラスです。
            /// 指定のバイトデータからボーナスフラグ、元のゲームモードおよび移行先ゲームモードを解析します。
            /// </summary>
            public class FreezeCondMode : IFreezeCond
            {
                // ボーナスフラグ(成立Gのみ有効)
                public byte BonusFlag { get; set; }
                // ゲームモード移行元
                public byte ModeSrc { get; set; }
                // ゲームモード移行先
                public byte ModeDst { get; set; }
                // 抽選対象がボーナスフラグか
                public bool IsBonus { get; set; }

                /// <summary>
                /// 指定のバイトデータから条件を解析して初期化します。
                /// </summary>
                /// <param name="pConditionData">解析対象のバイトデータ</param>
                public FreezeCondMode(byte pConditionData)
                {
                    IsBonus = (pConditionData & 0x80) != 0;
                    BonusFlag = (byte)((pConditionData >> 4) & 0x7);
                    ModeSrc = (byte)((pConditionData >> 2) & 0x3);
                    ModeDst = (byte)((pConditionData >> 0) & 0x3);
                }

                /// <summary>
                /// デフォルトの条件で初期化します。全フラグおよびモードは0に設定されます。
                /// </summary>
                public FreezeCondMode()
                {
                    BonusFlag = 0;
                    ModeSrc = 0;
                    ModeDst = 0;
                    IsBonus = false;
                }

                /// <summary>
                /// 条件をバイト形式でエクスポートします。フラグおよびモード情報を1バイトにパックします。
                /// </summary>
                /// <returns>エクスポートされた条件データのバイト値</returns>
                public byte Export()
                {
                    byte ans = (byte)(IsBonus ? 0x80 : 0x00);
                    ans |= (byte)(ModeDst << 0);
                    ans |= (byte)(ModeSrc << 2);
                    ans |= (byte)(BonusFlag << 4);
                    return ans;
                }
            }

            /// <summary>
            /// 条件判定：RT（リプレイタイム）移行を判定するクラスです。
            /// 指定のバイトデータからRT移行元および移行先モードを解析します。
            /// </summary>
            public class FreezeCondRT : IFreezeCond
            {
                // RT移行元
                public byte ModeSrc { get; set; }
                // RT移行先
                public byte ModeDst { get; set; }

                /// <summary>
                /// 指定のバイトデータからRT移行条件を解析して初期化します。
                /// </summary>
                /// <param name="pConditionData">解析対象のバイトデータ</param>
                public FreezeCondRT(byte pConditionData)
                {
                    ModeSrc = (byte)((pConditionData >> 3) & 0x7);
                    ModeDst = (byte)((pConditionData >> 0) & 0x7);
                }

                /// <summary>
                /// デフォルトのRT条件で初期化します。移行元および移行先は0に設定されます。
                /// </summary>
                public FreezeCondRT()
                {
                    ModeSrc = 0;
                    ModeDst = 0;
                }

                /// <summary>
                /// RT移行条件をバイト形式でエクスポートします。移行元および移行先情報を1バイトにパックします。
                /// </summary>
                /// <returns>エクスポートされた条件データのバイト値</returns>
                public byte Export()
                {
                    byte ans = ModeDst;
                    ans |= (byte)(ModeSrc << 3);
                    return ans;
                }
            }
            /// <summary>
            /// フリーズ抽選タイプ
            /// </summary>
            public FreezeControlType ControlType { get; set; }
            /// <summary>
            /// フリーズ発生タイミング・操作
            /// </summary>
            public FreezeTiming Timing { get; set; }
            /// <summary>
            /// フリーズ待機時間ID
            /// </summary>
            public byte WaitID { get; set; }
            /// <summary>
            /// 抽選条件(データは IFreezeCond 実装で読み込み・判定される)
            /// </summary>
            public byte Condition { get; set; }
            /// <summary>
            /// 抽選分母
            /// </summary>
            public byte RandVal { get; set; }
            /// <summary>
            /// ゲーム数シフト
            /// </summary>
            public byte ShiftGameNum { get; set; }
            /// <summary>
            /// 抽選タイプ名
            /// </summary>
            public string ControlTypeName { get; set; }
            /// <summary>
            /// 発生タイミング名
            /// </summary>
            public string TimingName { get; set; }

            /// <summary>
            /// デフォルトの抽選設定でインスタンスを初期化します。
            /// </summary>
            public FreezeControlData()
            {
                ControlType = FreezeControlType.Flag;
                Timing = FreezeTiming.Reset;
                WaitID = 0;
                Condition = new FreezeCondFlag().Export();
                RandVal = 0;
                ShiftGameNum = 0;
                ControlTypeName = "";
                TimingName = "";
            }

            /// <summary>
            /// インスタンスのデータをバイナリストリームに書き込みます。
            /// </summary>
            /// <param name="fs">書き込み先の BinaryWriter 参照</param>
            /// <param name="version">データバージョン</param>
            /// <returns>書き込みが成功した場合に true を返します</returns>
            public bool StoreData(ref BinaryWriter fs, int version)
            {
                // enum->byte変換
                fs.Write((byte)ControlType);
                fs.Write((byte)Timing);
                fs.Write(WaitID);
                fs.Write(Condition);
                fs.Write(RandVal);
                fs.Write(ShiftGameNum);
                fs.Write(ControlTypeName);
                fs.Write(TimingName);
                return true;
            }

            /// <summary>
            /// バイナリストリームからデータを読み込み、インスタンスを初期化します。
            /// </summary>
            /// <param name="fs">読み込み元の BinaryReader 参照</param>
            /// <param name="version">データバージョン</param>
            /// <returns>読み込みが成功した場合に true を返します</returns>
            public bool ReadData(ref BinaryReader fs, int version)
            {
                // byte->enum変換
                ControlType = (FreezeControlType)Enum.ToObject(typeof(FreezeControlType), fs.ReadByte());
                Timing = (FreezeTiming)Enum.ToObject(typeof(FreezeTiming), fs.ReadByte());
                WaitID = fs.ReadByte();
                Condition = fs.ReadByte();
                RandVal = fs.ReadByte();
                ShiftGameNum = fs.ReadByte();
                ControlTypeName = fs.ReadString();
                TimingName = fs.ReadString();
                return true;
            }
        }
        /// <summary>
        /// フリーズ待機時間データを管理します。
        /// Exp が true の場合は TIME_LONG、false の場合は TIME_SHORT を単位として待機時間を計算します。
        /// </summary>
        public class FreezeTimeData : ILocalDataInterface
        {
            // 時素定義
            public const int TIME_SHORT = 100;
            public const int TIME_LONG = 1000;

            /// <summary>
            /// 単位設定(True:1000[ms], False:100[ms])
            /// </summary>
            public bool Exp { get; set; }
            /// <summary>
            /// フリーズ待機時間の要素数
            /// </summary>
            public byte Elem { get; set; }

            /// <summary>
            /// デフォルトのフリーズ時間データでインスタンスを初期化します。
            /// </summary>
            public FreezeTimeData()
            {
                Exp = false;
                Elem = 0;
            }

            /// <summary>
            /// インスタンスのデータをバイナリストリームに書き込みます。
            /// </summary>
            /// <param name="fs">書き込み先の BinaryWriter 参照</param>
            /// <param name="version">データバージョン</param>
            /// <returns>書き込みが成功した場合に true を返します</returns>
            public bool StoreData(ref BinaryWriter fs, int version)
            {
                fs.Write(Exp);
                fs.Write(Elem);
                return true;
            }

            /// <summary>
            /// バイナリストリームからデータを読み込み、インスタンスを初期化します。
            /// </summary>
            /// <param name="fs">読み込み元の BinaryReader 参照</param>
            /// <param name="version">データバージョン</param>
            /// <returns>読み込みが成功した場合に true を返します</returns>
            public bool ReadData(ref BinaryReader fs, int version)
            {
                Exp = fs.ReadBoolean();
                Elem = fs.ReadByte();
                return true;
            }

            /// <summary>
            /// 実際の待機時間を計算して返します。
            /// </summary>
            /// <returns>計算された待機時間（ミリ秒）</returns>
            public int CalcTime()
            {
                int res = Exp ? TIME_LONG : TIME_SHORT;
                res *= Elem;
                return res;
            }
        }

        // 制御関係データ
        public class ComaCombinationData : ILocalDataInterface // 停止位置テーブル定義
        {
            public const byte PresetCombNum = SYMBOL_MAX + 4;
            /// <summary>停止位置テーブルの組み合わせデータ</summary>
            public UserBaseData Combination { get; set; }
            /// <summary>ユーザコメント</summary>
            public string UserComment { get; set; }

            /// <summary>
            /// デフォルトの停止位置テーブル定義でインスタンスを初期化します。
            /// </summary>
            public ComaCombinationData()
            {
                Combination = new UserBaseData(1, true, PresetCombNum);
                UserComment = string.Empty;
            }

            /// <summary>
            /// インスタンスのデータをバイナリストリームに書き込みます。
            /// </summary>
            /// <param name="fs">書き込み先の BinaryWriter 参照</param>
            /// <param name="version">データバージョン</param>
            /// <returns>書き込みが成功した場合に true を返します</returns>
            public bool StoreData(ref BinaryWriter fs, int version)
            {
                fs.Write((ushort)Combination.Export());
                fs.Write(UserComment);
                return true;
            }

            /// <summary>
            /// バイナリストリームからデータを読み込み、インスタンスを初期化します。
            /// </summary>
            /// <param name="fs">読み込み元の BinaryReader 参照</param>
            /// <param name="version">データバージョン</param>
            /// <returns>読み込みが成功した場合に true を返します</returns>
            public bool ReadData(ref BinaryReader fs, int version)
            {
                Combination.Import(fs.ReadUInt16());
                UserComment = fs.ReadString();
                return true;
            }
        }

        public class SlipBaseData : ILocalDataInterface
        {   // すべりコマテーブル定義
            /// <summary>すべりコマテーブルデータ</summary>
            public UserBaseData Table { get; set; }
            /// <summary>コメント</summary>
            public string Comment { get; set; }

            /// <summary>
            /// デフォルトのすべりコマテーブル定義でインスタンスを初期化します。
            /// </summary>
            public SlipBaseData()
            {
                Table = new UserBaseData(SLIP_MAX, false, COMA_MAX);
                Comment = string.Empty;
            }

            /// <summary>
            /// インスタンスのデータをバイナリストリームに書き込みます。
            /// </summary>
            /// <param name="fs">書き込み先の BinaryWriter 参照</param>
            /// <param name="version">データバージョン</param>
            /// <returns>書き込みが成功した場合に true を返します</returns>
            public bool StoreData(ref BinaryWriter fs, int version)
            {
                fs.Write((ulong)Table.Export());
                fs.Write(Comment);
                return true;
            }

            /// <summary>
            /// バイナリストリームからデータを読み込み、インスタンスを初期化します。
            /// </summary>
            /// <param name="fs">読み込み元の BinaryReader 参照</param>
            /// <param name="version">データバージョン</param>
            /// <returns>読み込みが成功した場合に true を返します</returns>
            public bool ReadData(ref BinaryReader fs, int version)
            {
                Table.Import(fs.ReadUInt64());
                Comment = fs.ReadString();
                return true;
            }
        }
        /// <summary>
        /// リーチ目・停止位置定義共通のリール位置データを管理します。
        /// JudgeReel が REEL_MAX 以上の場合、該当リール引込制御を行います。
        /// </summary>
        public class ReelPosElemData : ILocalDataInterface // リーチ目・停止位置定義共通のリール位置データ
        {
            // 定義基準リール：REEL_MAX以上の場合当該リール引込制御を起こす
            public byte JudgeReel { get; set; }
            // 確認対象リール：データ数可変のためUserBaseDataを使う
            public UserBaseData JudgeComaPos { get; set; }
            // 停止位置反転フラグ：停止可否設定後に全データを反転させるかを定義
            public bool IsInvert { get; set; }
            // 停止位置テーブル番号
            public byte CombinationID { get; set; }

            /// <summary>
            /// デフォルトのリール位置要素データでインスタンスを初期化します。
            /// </summary>
            public ReelPosElemData()
            {
                JudgeReel = 0;
                JudgeComaPos = new UserBaseData(1u, true, SHOW_MAX);
                IsInvert = false;
                CombinationID = 0;
            }

            /// <summary>
            /// 指定されたインスタンスをディープコピーして新しいインスタンスを生成します。
            /// </summary>
            /// <param name="deepCopySrc">コピー元の ReelPosElemData インスタンス</param>
            public ReelPosElemData(ReelPosElemData deepCopySrc)
            {   // 引数データのディープコピー(値渡し)を生成する
                JudgeReel = deepCopySrc.JudgeReel;
                JudgeComaPos = new UserBaseData(deepCopySrc.JudgeComaPos);
                IsInvert = deepCopySrc.IsInvert;
                CombinationID = deepCopySrc.CombinationID;
            }

            /// <summary>
            /// インスタンスのデータをバイナリストリームに書き込みます。
            /// </summary>
            /// <param name="fs">書き込み先の BinaryWriter 参照</param>
            /// <param name="version">データバージョン</param>
            /// <returns>書き込みが成功した場合に true を返します</returns>
            public bool StoreData(ref BinaryWriter fs, int version)
            {
                fs.Write(JudgeReel);
                fs.Write((byte)JudgeComaPos.Export());
                fs.Write(IsInvert);
                fs.Write(CombinationID);
                return true;
            }

            /// <summary>
            /// バイナリストリームからデータを読み込み、インスタンスを初期化します。
            /// </summary>
            /// <param name="fs">読み込み元の BinaryReader 参照</param>
            /// <param name="version">データバージョン</param>
            /// <returns>読み込みが成功した場合に true を返します</returns>
            public bool ReadData(ref BinaryReader fs, int version)
            {
                JudgeReel = fs.ReadByte();
                JudgeComaPos.Import(fs.ReadByte());
                IsInvert = fs.ReadBoolean();
                CombinationID = fs.ReadByte();
                return true;
            }
        }

        /// <summary>
        /// 各リーチ目要素定義を管理します。
        /// DefReelPosNum 個の ReelPosElemData と コメント文字列を保持します。
        /// </summary>
        public class ReelPosData : ILocalDataInterface    // 各リーチ目要素定義
        {
            // ReelPosData定義数
            public const byte DefReelPosNum = REEL_MAX;

            // リール停止位置データ
            public ReelPosElemData[] PosData { get; set; }
            /// <summary>コメント</summary>
            public string Comment { get; set; }

            /// <summary>
            /// デフォルトの各リーチ目要素定義でインスタンスを初期化します。
            /// </summary>
            public ReelPosData()
            {
                PosData = new ReelPosElemData[DefReelPosNum];
                for (int i = 0; i < DefReelPosNum; ++i)
                    PosData[i] = new ReelPosElemData();
            }

            /// <summary>
            /// 指定されたインスタンスをディープコピーして新しいインスタンスを生成します。
            /// </summary>
            /// <param name="deepCopySrc">コピー元の ReelPosData インスタンス</param>
            public ReelPosData(ReelPosData deepCopySrc)
            {   // 引数データのディープコピー(値渡し)を生成する
                Comment = deepCopySrc.Comment;
                PosData = new ReelPosElemData[DefReelPosNum];
                for (int i = 0; i < DefReelPosNum; ++i)
                    PosData[i] = new ReelPosElemData(deepCopySrc.PosData[i]);
            }

            /// <summary>
            /// インスタンスのデータをバイナリストリームに書き込みます。
            /// </summary>
            /// <param name="fs">書き込み先の BinaryWriter 参照</param>
            /// <param name="version">データバージョン</param>
            /// <returns>書き込みが成功した場合に true を返します</returns>
            public bool StoreData(ref BinaryWriter fs, int version)
            {
                fs.Write(DefReelPosNum);
                for (int i = 0; i < DefReelPosNum; ++i)
                    if (!PosData[i].StoreData(ref fs, version)) return false;
                fs.Write(Comment);
                return true;
            }

            /// <summary>
            /// バイナリストリームからデータを読み込み、インスタンスを初期化します。
            /// </summary>
            /// <param name="fs">読み込み元の BinaryReader 参照</param>
            /// <param name="version">データバージョン</param>
            /// <returns>読み込みが成功した場合に true を返します</returns>
            public bool ReadData(ref BinaryReader fs, int version)
            {
                byte dataSize = fs.ReadByte();
                if (dataSize != DefReelPosNum) return false;
                for (int i = 0; i < DefReelPosNum; ++i)
                    if (!PosData[i].ReadData(ref fs, version)) return false;
                Comment = fs.ReadString();
                return true;
            }
        }
        /// <summary>
        /// リーチ目定義を管理します。
        /// 最大BET数、モード0のみ有効なリーチ目データを保持し、シリアライズ/デシリアライズを行います。
        /// </summary>
        public class ReachData : ILocalDataInterface // リーチ目定義(最大BET数、モード0のみ有効)
        {
            // リーチ目レベル最大数設定
            public const byte ReachLevelMax = 2;

            // 定義位置設定(0-20：左1st、21-41：中1st、42-62：右1st)
            public byte BaseReelPos { get; set; }
            // レベル別のリーチ目数
            public byte[] ReachPatNum { get; set; }
            // リーチ目定義
            public List<ReelPosData> PosData { get; set; }

            /// <summary>
            /// デフォルトコンストラクタ：リーチ目定義リストとレベル数を初期化します。
            /// </summary>
            public ReachData()
            {
                BaseReelPos = 0;
                ReachPatNum = new byte[ReachLevelMax];
                for (int i = 0; i < ReachLevelMax; ++i) ReachPatNum[i] = 0;
                PosData = new List<ReelPosData>();
            }

            /// <summary>
            /// データをバイナリライターに書き込みます。
            /// </summary>
            /// <param name="fs">書き込み先の BinaryWriter 参照</param>
            /// <param name="version">データバージョン</param>
            /// <returns>書き込みが成功した場合に true を返します</returns>
            public bool StoreData(ref BinaryWriter fs, int version)
            {
                fs.Write(BaseReelPos);
                fs.Write(ReachLevelMax);
                for (int i = 0; i < ReachLevelMax; ++i) fs.Write(ReachPatNum[i]);

                fs.Write(PosData.Count);
                for (int i = 0; i < PosData.Count; ++i)
                    if (!PosData[i].StoreData(ref fs, version)) return false;
                return true;
            }

            /// <summary>
            /// バイナリリーダーからデータを読み込みます。
            /// </summary>
            /// <param name="fs">読み込み元の BinaryReader 参照</param>
            /// <param name="version">データバージョン</param>
            /// <returns>読み込みが成功した場合に true を返します</returns>
            public bool ReadData(ref BinaryReader fs, int version)
            {
                BaseReelPos = fs.ReadByte();
                byte levelVal = fs.ReadByte();
                if (levelVal != ReachLevelMax) return false;

                int posNumFromPatNum = 0;
                for (int i = 0; i < ReachLevelMax; ++i)
                {
                    ReachPatNum[i] = fs.ReadByte();
                    posNumFromPatNum += ReachPatNum[i];
                }

                // 定義数とlistサイズが不一致した場合異常終了
                int posNumFromBinary = fs.ReadInt32();
                if (posNumFromPatNum != posNumFromBinary) return false;

                for (int i = 0; i < posNumFromBinary; ++i)
                {
                    var addData = new ReelPosData();
                    if (!addData.ReadData(ref fs, version)) return false;
                    PosData.Add(addData);
                }

                return true;
            }
        }

        /// <summary>
        /// 組み合わせ優先制御容認データを管理します。
        /// 各リールの組み合わせ優先制御データとコメントを保持し、シリアライズ/デシリアライズを行います。
        /// </summary>
        public class CombiPriorityData : ILocalDataInterface
        {
            // 組み合わせ優先制御容認データ：(0,1,2)=(1st, 2nd, 3rd)
            public UserBaseData[] PriData { get; set; }
            /// <summary>コメント</summary>
            public string Comment { get; set; }

            /// <summary>
            /// デフォルトコンストラクタ：PriData 配列とコメントを初期化します。
            /// </summary>
            public CombiPriorityData()
            {
                PriData = new UserBaseData[REEL_MAX];
                Comment = string.Empty;

                uint elemNum = 1;
                for (int i = 0; i < REEL_MAX; ++i)
                {
                    elemNum *= (uint)(REEL_MAX - i);   // 各押し順のデータ数決定
                    PriData[i] = new UserBaseData(1, true, elemNum);
                }
            }

            /// <summary>
            /// データをバイナリライターに書き込みます。
            /// </summary>
            /// <param name="fs">書き込み先の BinaryWriter 参照</param>
            /// <param name="version">データバージョン</param>
            /// <returns>書き込みが成功した場合に true を返します</returns>
            public bool StoreData(ref BinaryWriter fs, int version)
            {
                fs.Write(PriData.Length);
                for (int i = 0; i < PriData.Length; ++i)
                    fs.Write((byte)PriData[i].Export());
                fs.Write(Comment);
                return true;
            }

            /// <summary>
            /// バイナリリーダーからデータを読み込みます。
            /// </summary>
            /// <param name="fs">読み込み元の BinaryReader 参照</param>
            /// <param name="version">データバージョン</param>
            /// <returns>読み込みが成功した場合に true を返します</returns>
            public bool ReadData(ref BinaryReader fs, int version)
            {
                int dataSize = fs.ReadInt32();
                if (dataSize != REEL_MAX) return false;
                for (int i = 0; i < PriData.Length; ++i)
                    PriData[i].Import(fs.ReadByte());
                Comment = fs.ReadString();
                return true;
            }
        }

        /// <summary>
        /// すべりコマ数増分およびテーブルIDを管理する要素データです。
        /// ディープコピー、シリアライズ、デシリアライズをサポートします。
        /// </summary>
        public class ControlSlipElem : ILocalDataInterface
        {
            /// <summary>すべりコマ数増分(0～3)</summary>
            public byte SlipIncrement { get; set; }
            /// <summary>すべりコマテーブル番号</summary>
            public byte TableID { get; set; }

            /// <summary>
            /// デフォルトコンストラクタ：SlipIncrement を 1、TableID を 0 に初期化します。
            /// </summary>
            public ControlSlipElem()
            {
                SlipIncrement = 1;
                TableID = 0;
            }

            /// <summary>
            /// 指定されたインスタンスをディープコピーして新しいインスタンスを生成します。
            /// </summary>
            /// <param name="deepCopySrc">コピー元の ControlSlipElem インスタンス</param>
            public ControlSlipElem(ControlSlipElem deepCopySrc)
            {   // 引数データのディープコピー(値渡し)を生成する
                SlipIncrement = deepCopySrc.SlipIncrement;
                TableID = deepCopySrc.TableID;
            }

            /// <summary>
            /// データをバイナリライターに書き込みます。
            /// </summary>
            /// <param name="fs">書き込み先の BinaryWriter 参照</param>
            /// <param name="version">データバージョン</param>
            /// <returns>書き込みが成功した場合に true を返します</returns>
            public bool StoreData(ref BinaryWriter fs, int version)
            {
                fs.Write(SlipIncrement);
                fs.Write(TableID);
                return true;
            }

            /// <summary>
            /// バイナリリーダーからデータを読み込みます。
            /// </summary>
            /// <param name="fs">読み込み元の BinaryReader 参照</param>
            /// <param name="version">データバージョン</param>
            /// <returns>読み込みが成功した場合に true を返します</returns>
            public bool ReadData(ref BinaryReader fs, int version)
            {
                SlipIncrement = fs.ReadByte();
                TableID = fs.ReadByte();
                return true;
            }
        }
        /// <summary>
        /// リール制御定義の基本データを表します。
        /// 3リール用のすべり制御や回避位置テーブルを保持し、シリアライズ/デシリアライズをサポートします。
        /// </summary>
        public class ReelControlElem3Reels : ILocalDataInterface
        {   // リール制御定義：基本データ
            public const byte AvoidPosNum = 3;

            public byte DefinePos { get; set; }                 // 定義位置設定
            public UserBaseData Exist2ndSlip { get; set; }      // 2ndすべりコマテーブル定義(2nd/3rd定義のみ)
            public bool Exist3rdSlip { get; set; }              // 3rdすべりコマテーブル有効有無(2nd/3rd定義のみ)
            public byte[] AvoidPosDataCount { get; set; }       // 停止位置テーブル数定義数(回避箇所) - 2nd/3rd, 2ndのみ, 3rdのみ
            public List<ReelPosData> AvoidPos { get; set; }     // 停止位置テーブル数定義(回避箇所) 配列：定義種別/List：定義データ
            public List<ControlSlipElem> SlipElem { get; set; } // すべりコマ定義(定義過剰は無視、定義不足はデフォルト値使用)

            /// <summary>
            /// デフォルトコンストラクタ：各プロパティを初期値で初期化します。
            /// </summary>
            public ReelControlElem3Reels()
            {
                DefinePos = COMA_MAX * REEL_MAX;
                Exist2ndSlip = new UserBaseData(1, true, SLIP_MAX);
                Exist3rdSlip = false;
                AvoidPosDataCount = new byte[AvoidPosNum];
                AvoidPos = new List<ReelPosData>();
                SlipElem = new List<ControlSlipElem>();
            }

            /// <summary>
            /// ディープコピーコンストラクタ：指定されたインスタンスの値を複製して新しいインスタンスを生成します。
            /// </summary>
            /// <param name="deepCopySrc">コピー元の ReelControlElem3Reels インスタンス</param>
            public ReelControlElem3Reels(ReelControlElem3Reels deepCopySrc)
            {   // 引数データのディープコピー(値渡し)を生成する
                DefinePos = deepCopySrc.DefinePos;
                Exist2ndSlip = new UserBaseData(deepCopySrc.Exist2ndSlip);
                Exist3rdSlip = deepCopySrc.Exist3rdSlip;
                AvoidPosDataCount = new byte[AvoidPosNum];
                for (int i = 0; i < AvoidPosNum; ++i) AvoidPosDataCount[i] = deepCopySrc.AvoidPosDataCount[i];

                AvoidPos = new List<ReelPosData>();
                foreach (var item in deepCopySrc.AvoidPos) AvoidPos.Add(new ReelPosData(item));
                SlipElem = new List<ControlSlipElem>();
                foreach (var item in deepCopySrc.SlipElem) SlipElem.Add(new ControlSlipElem(item));
            }

            /// <summary>
            /// データをバイナリライターに書き込みます。
            /// </summary>
            /// <param name="fs">書き込み先の BinaryWriter 参照</param>
            /// <param name="version">データバージョン</param>
            /// <returns>書き込みが成功した場合に true を返します</returns>
            public bool StoreData(ref BinaryWriter fs, int version)
            {
                fs.Write(DefinePos);
                fs.Write((byte)Exist2ndSlip.Export());
                fs.Write(Exist3rdSlip);

                fs.Write(AvoidPosNum);
                for (int i = 0; i < AvoidPosNum; ++i) fs.Write(AvoidPosDataCount[i]);

                fs.Write(AvoidPos.Count);
                for (int j = 0; j < AvoidPos.Count; ++j) AvoidPos[j].StoreData(ref fs, version);

                fs.Write(SlipElem.Count);
                for (int i = 0; i < SlipElem.Count; ++i) SlipElem[i].StoreData(ref fs, version);
                return true;
            }

            /// <summary>
            /// バイナリリーダーからデータを読み込みます。
            /// </summary>
            /// <param name="fs">読み込み元の BinaryReader 参照</param>
            /// <param name="version">データバージョン</param>
            /// <returns>読み込みが成功した場合に true を返します</returns>
            public bool ReadData(ref BinaryReader fs, int version)
            {
                DefinePos = fs.ReadByte();
                Exist2ndSlip.Import(fs.ReadByte());
                Exist3rdSlip = fs.ReadBoolean();
                byte numCheck = fs.ReadByte();

                if (numCheck != AvoidPosNum) return false;
                for (int i = 0; i < AvoidPosNum; ++i) AvoidPosDataCount[i] = fs.ReadByte();

                int avoidSize = fs.ReadInt32();
                for (int j = 0; j < avoidSize; ++j)
                {
                    var newData = new ReelPosData();
                    if (!newData.ReadData(ref fs, version)) return false;
                    AvoidPos.Add(newData);
                }

                int slipNum = fs.ReadInt32();
                for (int i = 0; i < slipNum; ++i)
                {
                    ControlSlipElem newData = new ControlSlipElem();
                    if (!newData.ReadData(ref fs, version)) return false;
                    SlipElem.Add(newData);
                }
                return true;
            }
        }

        /// <summary>
        /// リール制御定義のヘッダデータを表します。
        /// BET数、ゲームモード、フラグ状態、および停止位置制御要素リストを管理し、シリアライズ/デシリアライズを行います。
        /// </summary>
        public class ReelControlData : ILocalDataInterface
        {   // リール制御定義：ヘッダデータ
            public byte BetNum { get; set; }                            // 制御対象BET数
            public byte GameMode { get; set; }                          // 制御対象ゲームモード
            public byte BonusFlag { get; set; }                         // 制御対象ボーナスフラグ
            public byte CastFlag { get; set; }                          // 制御対象小役フラグ
            public UserBaseData ReachAvail { get; set; }                // リーチ目停止可否
            public UserBaseData ReachPri { get; set; }                  // リーチ目優先Lv1フラグ
            public UserBaseData ReachSec { get; set; }                  // リーチ目優先Lv2フラグ
            public List<ReelControlElem3Reels> ElemData { get; set; }   // 各停止位置要素データ
            public byte CombiPriority { get; set; }                     // 組み合わせ優先制御容認データID(1stのみ)

            /// <summary>
            /// デフォルトコンストラクタ：各プロパティを初期値で初期化します。
            /// </summary>
            public ReelControlData()
            {
                BetNum = 0;
                GameMode = 0;
                BonusFlag = 0;
                CastFlag = 0;
                ReachAvail = new UserBaseData(1, true, ReachData.ReachLevelMax);
                ReachPri = new UserBaseData(1, true, ReachData.ReachLevelMax);
                ReachSec = new UserBaseData(1, true, ReachData.ReachLevelMax);
                ElemData = new List<ReelControlElem3Reels>();
                CombiPriority = 0;
            }

            /// <summary>
            /// データをバイナリライターに書き込みます。
            /// </summary>
            /// <param name="fs">書き込み先の BinaryWriter 参照</param>
            /// <param name="version">データバージョン</param>
            /// <returns>書き込みが成功した場合に true を返します</returns>
            public bool StoreData(ref BinaryWriter fs, int version)
            {
                fs.Write(BetNum);
                fs.Write(GameMode);
                fs.Write(BonusFlag);
                fs.Write(CastFlag);
                fs.Write((byte)ReachAvail.Export());
                fs.Write((byte)ReachPri.Export());
                fs.Write((byte)ReachSec.Export());
                fs.Write(ElemData.Count);
                for (int i = 0; i < ElemData.Count; ++i) ElemData[i].StoreData(ref fs, version);
                fs.Write(CombiPriority);
                return true;
            }

            /// <summary>
            /// バイナリリーダーからデータを読み込みます。
            /// </summary>
            /// <param name="fs">読み込み元の BinaryReader 参照</param>
            /// <param name="version">データバージョン</param>
            /// <returns>読み込みが成功した場合に true を返します</returns>
            public bool ReadData(ref BinaryReader fs, int version)
            {
                BetNum = fs.ReadByte();
                GameMode = fs.ReadByte();
                BonusFlag = fs.ReadByte();
                CastFlag = fs.ReadByte();
                ReachAvail.Import(fs.ReadByte());
                ReachPri.Import(fs.ReadByte());
                ReachSec.Import(fs.ReadByte());
                int elemDataSize = fs.ReadInt32();
                for (int i = 0; i < elemDataSize; ++i)
                {
                    var newData = new ReelControlElem3Reels();
                    if (!newData.ReadData(ref fs, version)) return false;
                    ElemData.Add(newData);
                }
                CombiPriority = fs.ReadByte();
                return true;
            }
        }
    }
}