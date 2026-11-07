using UnityEngine.Events;

namespace TF.UI.Title
{
    /// <summary>タイトルの新規開始と再開の操作を定義する。</summary>
    public interface ITitleView
    {
        /// <summary>新しいバトルの開始が要求された通知。</summary>
        event UnityAction StartRequested;

        /// <summary>保存したバトルの再開が要求された通知。</summary>
        event UnityAction ResumeRequested;

        /// <summary>タイトル全体の操作受付を切り替える。</summary>
        void SetInputEnabled(bool enabled);

        /// <summary>再開できる保存データがあるかを表示へ反映する。</summary>
        void SetResumeAvailable(bool available);
    }
}