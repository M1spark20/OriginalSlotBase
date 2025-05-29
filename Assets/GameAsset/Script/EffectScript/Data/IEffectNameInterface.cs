using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlotEffectMaker2023.Data
{
    /// <summary>
    /// 名前変更処理の対象タイプを表します。
    /// </summary>
    public enum EChangeNameType
    {
        /// <summary>変数名</summary>
        Var,

        /// <summary>タイマー名</summary>
        Timer,

        /// <summary>サウンドID</summary>
        SoundID,

        /// <summary>サウンドプレイヤー名</summary>
        SoundPlayer,

        /// <summary>タイムライン名</summary>
        Timeline,

        /// <summary>カラーマップID</summary>
        ColorMap,

        /// <summary>マッププレイヤー名</summary>
        MapPlayer,

        /// <summary>コレクション名</summary>
        Collection,

        /// <summary>情報用キー名</summary>
        Info,

        /// <summary>ゲーム実績名</summary>
        GameAchievement,

        /// <summary>名前変更を行わない</summary>
        None
    }

    /// <summary>
    /// 名前変更機能を持つインターフェースです。
    /// </summary>
    interface IEffectNameInterface : SlotMaker2022.ILocalDataInterface
    {
        /// <summary>
        /// 名前変更時に呼び出されます。
        /// </summary>
        /// <param name="type">変更の種類（EChangeNameType）</param>
        /// <param name="src">元の名前</param>
        /// <param name="dst">新しい名前</param>
        void Rename(EChangeNameType type, string src, string dst);
    }
}
