using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TF.UI.Title
{
    /// <summary>新規開始・再開の操作通知とボタン表示を担当する。</summary>
    [DisallowMultipleComponent]
    public sealed class TitleView : MonoBehaviour, ITitleView
    {
        /// <summary>新しいバトルを始めるボタン。</summary>
        [SerializeField] private Button _startButton;

        /// <summary>保存したバトルを再開するボタン。</summary>
        [SerializeField] private Button _resumeButton;

        /// <summary>Presenterから指定された操作受付状態。</summary>
        private bool _isInputEnabled;

        /// <summary>再開できる保存データがあるか。</summary>
        private bool _isResumeAvailable;

        /// <summary>新しいバトルの開始を通知する。</summary>
        public event UnityAction StartRequested;

        /// <summary>保存したバトルの再開を通知する。</summary>
        public event UnityAction ResumeRequested;

        /// <summary>ボタン操作の通知を登録する。</summary>
        private void OnEnable()
        {
            if (_startButton != null)
            {
                _startButton.onClick.AddListener(HandleStartClicked);
            }

            if (_resumeButton != null)
            {
                _resumeButton.onClick.AddListener(HandleResumeClicked);
            }

            if (_startButton == null || _resumeButton == null)
            {
                Debug.LogError("タイトルの新規開始・再開ボタンを設定してください。", this);
            }

            RefreshButtons();
        }

        /// <summary>非表示時に操作の通知を解除する。</summary>
        private void OnDisable()
        {
            if (_startButton != null)
            {
                _startButton.onClick.RemoveListener(HandleStartClicked);
            }

            if (_resumeButton != null)
            {
                _resumeButton.onClick.RemoveListener(HandleResumeClicked);
            }
        }

        /// <summary>タイトル全体の操作受付を設定する。</summary>
        public void SetInputEnabled(bool enabled)
        {
            _isInputEnabled = enabled;
            RefreshButtons();
        }

        /// <summary>再開データの有無を反映する。</summary>
        public void SetResumeAvailable(bool available)
        {
            _isResumeAvailable = available;
            RefreshButtons();
        }

        /// <summary>画面の受付状態と保存データの有無をボタンへ反映する。</summary>
        private void RefreshButtons()
        {
            if (_startButton != null)
            {
                _startButton.interactable = _isInputEnabled;
            }

            if (_resumeButton != null)
            {
                _resumeButton.interactable = _isInputEnabled && _isResumeAvailable;
            }
        }

        /// <summary>操作できる場合だけ新規開始を通知する。</summary>
        private void HandleStartClicked()
        {
            if (isActiveAndEnabled && _isInputEnabled && _startButton != null && _startButton.IsInteractable())
            {
                StartRequested?.Invoke();
            }
        }

        /// <summary>再開データがあり操作できる場合だけ再開を通知する。</summary>
        private void HandleResumeClicked()
        {
            if (isActiveAndEnabled && _isInputEnabled && _isResumeAvailable
                && _resumeButton != null && _resumeButton.IsInteractable())
            {
                ResumeRequested?.Invoke();
            }
        }
    }
}