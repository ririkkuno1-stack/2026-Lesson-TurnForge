using System;
using Cysharp.Threading.Tasks;
using TF.GameFlow;

namespace TF.UI.Title
{
    /// <summary>
    /// タイトルの開始操作と、戦闘開始処理を接続
    /// </summary>
    public sealed class TitlePresenter : IDisposable
    {
        /// <summary>
        /// タイトル画面の操作と入力受付
        /// </summary>
        private readonly ITitleView _view;

        /// <summary>
        /// ゲーム全体の進行状態
        /// </summary>
        private readonly GameFlowController _gameFlow;

        /// <summary>
        /// 外部から渡された非同期の戦闘開始処理
        /// </summary>
        private readonly Func<UniTask<bool>> _startBattleAsync;

        /// <summary>保存したバトルを再開する処理。</summary>
        private readonly Func<UniTask<bool>> _resumeBattleAsync;

        /// <summary>再開できる保存データの有無を取得する。</summary>
        private readonly Func<bool> _canResume;

        /// <summary>
        /// イベントを購読しているか
        /// </summary>
        private bool _isInitialized = false;

        /// <summary>
        /// このPresenterからの開始要求を処理しているか
        /// </summary>
        private bool _isStarting = false;

        /// <summary>
        /// このPresenterが破棄されたか
        /// </summary>
        private bool _isDisposed = false;

        /// <summary>
        /// タイトル表示・進行状態・戦闘開始処理を受け取る
        /// </summary>
        public TitlePresenter(ITitleView view, GameFlowController gameFlow, Func<UniTask<bool>> startBattleAsync,
            Func<UniTask<bool>> resumeBattleAsync, Func<bool> canResume)
        {
            _view = view;
            _gameFlow = gameFlow;
            _startBattleAsync = startBattleAsync;
            _resumeBattleAsync = resumeBattleAsync;
            _canResume = canResume;
        }

        /// <summary>
        /// 操作と状態変更の購読を開始
        /// </summary>
        public bool TryInitialize()
        {
            if (_isDisposed || _gameFlow == null || _view == null || _startBattleAsync == null
                || _resumeBattleAsync == null || _canResume == null)
            {
                return false;
            }

            if (_isInitialized)
            {
                return true;
            }

            _view.StartRequested += HandleStartRequested;
            _view.ResumeRequested += HandleResumeRequested;
            _gameFlow.StateChanged += HandleStateChanged;
            _isInitialized = true;

            RefreshInput();
            return true;
        }

        /// <summary>
        /// タイトルでの開始要求を受け付ける
        /// </summary>
        private void HandleStartRequested()
        {
            HandleBattleRequested(false);
        }

        /// <summary>タイトルから再開を要求する。</summary>
        private void HandleResumeRequested()
        {
            HandleBattleRequested(true);
        }

        /// <summary>連打と再開データを確認し、選んだ開始処理へ渡す。</summary>
        private void HandleBattleRequested(bool resumeSavedBattle)
        {
            if (_isDisposed || !_isInitialized || _isStarting || _gameFlow.CurrentState != GameState.Title
                || (resumeSavedBattle && !_canResume()))
            {
                return;
            }

            _isStarting = true;
            RefreshInput();
            StartBattleAsync(resumeSavedBattle).Forget();
        }

        /// <summary>
        /// 戦闘開始処理の完了後、現在の状態に合わせて入力を更新
        /// </summary>
        private async UniTaskVoid StartBattleAsync(bool resumeSavedBattle)
        {
            // 成功・失敗の画面遷移は、開始処理側が担当
            await (resumeSavedBattle ? _resumeBattleAsync() : _startBattleAsync());

            _isStarting = false;

            // 待機中に破棄された場合は、Viewへアクセスしない
            if (_isDisposed)
            {
                return;
            }

            RefreshInput();
        }

        /// <summary>
        /// ゲーム状態の変更に合わせて入力受付を更新
        /// </summary>
        private void HandleStateChanged(GameState state)
        {
            if (_isDisposed)
            {
                return;
            }

            RefreshInput();
        }

        /// <summary>
        /// タイトル表示中かつ開始処理中でない場合だけ操作を許可
        /// </summary>
        private void RefreshInput()
        {
            bool enabled = !_isStarting && _gameFlow.CurrentState == GameState.Title;
            _view.SetResumeAvailable(_canResume());
            _view.SetInputEnabled(enabled);
        }

        /// <summary>
        /// 操作と状態変更の購読を解除
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;

            if (_isInitialized)
            {
                _gameFlow.StateChanged -= HandleStateChanged;
                _view.StartRequested -= HandleStartRequested;
                _view.ResumeRequested -= HandleResumeRequested;
                _isInitialized = false;
            }
        }
    }
}