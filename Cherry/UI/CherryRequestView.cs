using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using Cherry.Interfaces;
using Cherry.Managers;
using Cherry.Models;
using HMUI;
using SiraUtil.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using Tweening;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Cherry.UI
{
    [ViewDefinition("Cherry.Views.request-view.bsml")]
    [HotReload(RelativePathToLayout = @"..\Views\request-view.bsml")]
    internal class CherryRequestView : BSMLAutomaticViewController, IDisposable
    {
        #region BSML Bindings

        [UIValue("detail-view")]
        private RequestDetailView _requestDetailView = null!;

        [UIValue("panel-view")]
        private RequestPanelView _requestPanelView = null!;

        [UIComponent("up-button")]
        protected readonly Button upButton = null!;

        [UIComponent("down-button")]
        protected readonly Button downButton = null!;

        [UIComponent("request-list")]
        protected readonly CustomListTableData requestList = null!;

        [UIComponent("top-panel")]
        protected readonly Backgroundable topPanelBackground = null!;

        [UIComponent("request-queue-text")]
        protected readonly CurvedTextMeshPro requestQueueText = null!;

        #endregion

        private bool _isDirty;
        private bool _isInHistory;
        private Config _config = null!;
        private IDenier _denier = null!;
        private SiraLog _siraLog = null!;
        private IRequestHistory _requestHistory = null!;
        private IRequestManager _requestManager = null!;
        private RequestQueueService _requestQueueService = null!;
        private TweeningManager _tweeningManager = null!;
        private CherryLevelManager _cherryLevelManager = null!;

        private RequestCellInfo? _lastSelectedCellInfo;
        private CancellationTokenSource? _downloadCancelSource;
        private CancellationTokenSource? _historyCancelSource;
        public event Action<BeatmapLevel>? SelectLevelRequested;

        private readonly Color _downloadButtonColor = new Color(0.5f, 0.5f, 0.5f);
        private readonly Color _openColor0 = new Color(0.217f, 0.782f, 0f);
        private readonly Color _openColor1 = new Color(0.065f, 0.239f, 0f);
        private readonly Color _closedColor0 = new Color(0.804f, 0.217f, 0.152f);
        private readonly Color _closedColor1 = new Color(0.652f, 0f, 0f);

        [Inject]
        protected void Construct(Config config, IDenier denier, SiraLog siraLog, DiContainer container, IRequestHistory requestHistory, IRequestManager requestManager, RequestQueueService requestQueueService, TimeTweeningManager tweeningManager, CherryLevelManager cherryLevelManager)
        {
            _config = config;
            _denier = denier;
            _siraLog = siraLog;
            _requestHistory = requestHistory;
            _requestManager = requestManager;
            _requestQueueService = requestQueueService;
            _tweeningManager = tweeningManager;
            _cherryLevelManager = cherryLevelManager;

            _requestDetailView = container.Instantiate<RequestDetailView>();
            _requestPanelView = container.InstantiateComponent<RequestPanelView>(gameObject);

            _requestQueueService.Changed += RequestsChanged;

            _requestPanelView.PlayButtonClicked += PlayButtonClicked;
            _requestPanelView.SkipButtonClicked += SkipButtonClicked;
            _requestPanelView.QueueButtonClicked += QueueButtonClicked;
            _requestPanelView.HistoryButtonClicked += HistoryButtonClicked;

            _requestDetailView.BanSongButtonClicked += BanSong;
            _requestDetailView.BanSessionButtonClicked += BanUserSession;
            _requestDetailView.BanForeverButtonClicked += BanUserForever;
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            if (_isDirty && !_isInHistory)
                ShowActiveRequests();
            _requestManager.HasNewRequests = false;
        }

        [UIAction("#post-parse")]
        protected void Parsed()
        {
            var tableView = requestList.GetTableView();
            var scroller = tableView._scrollView;

            scroller._pageUpButton = upButton;
            scroller._pageDownButton = downButton;

            var inst = requestList.GetSongListTableCellPrefab();
            if (inst == null)
            {
                inst = requestList.GetTableCell();
                requestList.SetSongListTableCellPrefab(inst);
            }
            var cellBackground = (ImageView)inst._backgroundImage;
            var cellCoverImage = (ImageView)inst._coverImage;

            cellBackground._skew = 0f;
            cellCoverImage._skew = 0f;
            cellCoverImage.SetVerticesDirty();
            cellBackground.SetVerticesDirty();
            upButton.SetSkew(0f);
            downButton.SetSkew(0f);

            var songNameText = inst._songNameText;
            var authorNameText = inst._songAuthorText;
            songNameText.fontSizeMin = 1f;
            songNameText.fontSizeMax = 4f;
            songNameText.enableAutoSizing = true;
            songNameText.transform.localPosition = new Vector2(authorNameText.transform.position.x, songNameText.transform.localPosition.y);
            songNameText.fontStyle = FontStyles.Normal;
            authorNameText.fontStyle = FontStyles.Normal;

            var topBackground = (ImageView)topPanelBackground.GetBackground();
            topBackground._skew = 0f;
            topBackground.color = Color.white;
            topBackground.color0 = _config.QueueOpened ? _openColor0 : _closedColor0;
            topBackground.color1 = _config.QueueOpened ? _openColor1 : _closedColor1;
            topBackground.SetVerticesDirty();

            _requestPanelView.SetPlayButtonText("Play");
            _requestPanelView.SetPlayButtonInteractability(false);
            _requestPanelView.SetSkipButtonInteractability(false);
            _requestPanelView.SetPlayButtonColor(null);

            _requestPanelView.SetQueueButtonText("Open Queue");
            _requestDetailView.SetLoading();

            UpdateQueueColors();
            ShowActiveRequests();
        }

        [UIAction("selected-request")]
        protected void SelectedCell(TableView _, int index)
        {
            if (_downloadCancelSource != null)
            {
                _downloadCancelSource.Cancel();
                _requestPanelView.SetPlayButtonText("Play");
                _requestPanelView.SetPlayButtonColor(null);
            }

            var request = (RequestCellInfo)requestList.GetData()[index];

            float durationInSeconds = request.map.LatestVersion.Difficulties.Select(f => f.Seconds).FirstOrDefault();
            _lastSelectedCellInfo = request;
            _requestDetailView.SetData(
                request.map.Name,
                request.map.Uploader.Name,
                request.request,
                request.GetIcon(),
                (float)request.map.MapStats.Rating,
                durationInSeconds
            );
            _requestPanelView.SetSkipButtonInteractability(!_isInHistory);
            var levelInstalled = _cherryLevelManager.TryGetLevel(request.map.LatestVersion.Hash, request.map.LatestVersion.State) != null;
            if (levelInstalled)
            {
                _requestPanelView.SetPlayButtonColor(null);
                _requestPanelView.SetPlayButtonText("Play");
                _requestPanelView.SetPlayButtonInteractability(true);
            }
            else
            {
                _requestPanelView.SetPlayButtonText("Download");
                _requestPanelView.SetPlayButtonColor(_downloadButtonColor);
                _requestPanelView.SetPlayButtonInteractability(true);
            }
        }

        private void PlayButtonClicked()
        {
            if (_lastSelectedCellInfo == null)
                return;

            var map = _lastSelectedCellInfo.map;
            var request = _lastSelectedCellInfo.request;
            BeatmapLevel? level = _cherryLevelManager.TryGetLevel(map.LatestVersion.Hash, map.LatestVersion.State);
            if (level != null)
            {
                _requestManager.MarkAsRead(request);
                ResetSubPanels();
                SelectLevelRequested?.Invoke(level);
                if (_isInHistory) HistoryButtonClicked();
                return;
            }

            if (_downloadCancelSource != null)
                _downloadCancelSource.Cancel();
            else
                _ = DownloadRequest(_lastSelectedCellInfo);
        }

        private async Task DownloadRequest(RequestCellInfo cell)
        {
            using var cancellationTokenSource = new CancellationTokenSource();
            _downloadCancelSource = cancellationTokenSource;
            var progress = new Progress<float>();
            progress.ProgressChanged += Progress_ProgressChanged;
            BeatmapLevel? level = null;
            _requestPanelView.SetPlayButtonText("Fetching...");
            try
            {
                level = await _cherryLevelManager.DownloadLevel($"{cell.map.Key} ({cell.map.MapMetadata.SongName} - {cell.map.Uploader.Name})", cell.map.LatestVersion.Hash, cell.map.LatestVersion.DownloadURL, cell.map.LatestVersion.State, cancellationTokenSource.Token, progress);
            }
            catch (OperationCanceledException) when (cancellationTokenSource.IsCancellationRequested)
            {
            }
            catch (Exception e)
            {
                _siraLog.Error(e);
            }
            if (ReferenceEquals(_downloadCancelSource, cancellationTokenSource))
                _downloadCancelSource = null;
            progress.ProgressChanged -= Progress_ProgressChanged;
            if (level != null)
            {
                _requestPanelView.SetPlayButtonText("Play");
                _requestPanelView.SetPlayButtonColor(null);
                _requestManager.MarkAsRead(cell.request);
                ResetSubPanels();
                SelectLevelRequested?.Invoke(level);
                if (_isInHistory) HistoryButtonClicked();
            }
            else
            {
                _requestPanelView.SetPlayButtonText("Download");
                _requestPanelView.SetPlayButtonColor(_downloadButtonColor);
            }
        }

        private void Progress_ProgressChanged(object sender, float value)
        {
            _requestPanelView.SetPlayButtonText($"Downloading...\n{string.Format("{0:0%}", value)}");
        }

        private void BanSong(RequestEventArgs request)
        {
            _denier.DenySong(request.Key);
            SkipButtonClicked();
        }

        private void BanUserSession(IRequester requester)
        {
            _denier.DenyUser(requester.ID, DateTime.Now.AddHours(_config.SesssionLengthInHours));
            RemoveUserFromQueue(requester);
            ResetSubPanels();
        }

        private void BanUserForever(IRequester requester)
        {
            _denier.DenyUser(requester.ID);
            RemoveUserFromQueue(requester);
            ResetSubPanels();
        }

        private void RemoveUserFromQueue(IRequester requester)
        {
            var requesters = _requestQueueService.Requests
                .Where(request => string.Equals(request.request.Requester.ID, requester.ID, StringComparison.OrdinalIgnoreCase))
                .ToArray();
            for (int i = 0; i < requesters.Length; i++)
                _requestManager.Remove(requesters[i].request);
            ResetSubPanels();
        }

        private void HistoryButtonClicked()
        {
            _ = HistoryButtonClickedAsync();
        }

        private async Task HistoryButtonClickedAsync()
        {
            _historyCancelSource?.Cancel();
            _historyCancelSource?.Dispose();
            _historyCancelSource = null;

            _requestPanelView.SetHistoryButtonText(_isInHistory ? "History" : "Queue");
            requestList.GetTableView().SelectCellWithIdx(-1);
            ResetSubPanels();
            if (_isInHistory)
            {
                _isInHistory = false;
                requestQueueText.text = "Request Queue";
                ShowActiveRequests();
            }
            else
            {
                _isInHistory = true;
                _historyCancelSource = new CancellationTokenSource();
                CancellationToken cancellationToken = _historyCancelSource.Token;
                requestList.GetData().Clear();
                requestList.GetTableView().ReloadData();
                requestQueueText.text = "History";

                foreach (var request in (await _requestHistory.History()).Where(r => r.WasPlayed).Take(10))
                {
                    try
                    {
                        var cell = await _requestQueueService.ResolveAsync(request.Args, cancellationToken);
                        cancellationToken.ThrowIfCancellationRequested();
                        if (cell != null && _isInHistory)
                        {
                            requestList.GetData().Add(cell);
                            requestList.GetTableView().ReloadData();
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        return;
                    }
                    catch (Exception exception)
                    {
                        _siraLog.Error($"Failed to load historical request {request.Key}.");
                        _siraLog.Error(exception);
                    }
                }
            }
        }

        private void SkipButtonClicked()
        {
            if (_lastSelectedCellInfo != null)
            {
                _requestManager.Remove(_lastSelectedCellInfo.request);
                ResetSubPanels();
            }
        }

        private void ResetSubPanels()
        {
            _requestDetailView.SetLoading();
            _requestPanelView.SetPlayButtonColor(null);
            _requestPanelView.SetPlayButtonText("Play");
            _requestPanelView.SetPlayButtonInteractability(false);
            _requestPanelView.SetSkipButtonInteractability(false);
            requestList.GetTableView().ReloadData();
            requestList.GetTableView().SelectCellWithIdx(-1);
            _lastSelectedCellInfo = null;
        }

        private void RequestsChanged()
        {
            if (_isActivated)
                _requestManager.HasNewRequests = false;

            if (_isInHistory || requestList == null)
            {
                _isDirty = true;
                return;
            }

            ShowActiveRequests();
        }

        private void ShowActiveRequests()
        {
            if (_isInHistory || requestList == null)
                return;

            var requests = _requestQueueService.Requests;
            var selectedRequestWasRemoved = _lastSelectedCellInfo != null
                && !requests.Any(item => ReferenceEquals(item.request, _lastSelectedCellInfo.request));

            var data = requestList.GetData();
            data.Clear();
            foreach (var request in requests)
                data.Add(request);
            requestList.GetTableView().ReloadData();
            _isDirty = false;

            if (selectedRequestWasRemoved)
                ResetSubPanels();
        }

        private void QueueButtonClicked()
        {
            _config.QueueOpened = !_config.QueueOpened;
            UpdateQueueColors();
        }

        private void UpdateQueueColors()
        {
            var topBackground = (ImageView)topPanelBackground.GetBackground();
            _requestPanelView.SetQueueButtonColor(_config.QueueOpened ? Color.green : Color.red);
            _requestPanelView.SetQueueButtonText(_config.QueueOpened ? "Close Queue" : "Open Queue");

            var startColor0 = topBackground._color0;
            var startColor1 = topBackground._color1;
            var endColor0 = _config.QueueOpened ? _openColor0 : _closedColor0;
            var endColor1 = _config.QueueOpened ? _openColor1 : _closedColor1;

            _tweeningManager.KillAllTweens(topPanelBackground);
            _tweeningManager.AddTween(new FloatTween(0f, 1f, val =>
            {
                topBackground.color0 = Color.Lerp(startColor0, endColor0, val);
                topBackground.color1 = Color.Lerp(startColor1, endColor1, val);
                topBackground.SetAllDirty();
            }, 0.5f, EaseType.InOutQuad), topPanelBackground);
        }

        public void Dispose()
        {
            _historyCancelSource?.Cancel();
            _historyCancelSource?.Dispose();
            _downloadCancelSource?.Cancel();
            _downloadCancelSource?.Dispose();

            _requestQueueService.Changed -= RequestsChanged;
            _requestPanelView.HistoryButtonClicked -= HistoryButtonClicked;
            _requestPanelView.QueueButtonClicked -= QueueButtonClicked;
            _requestPanelView.SkipButtonClicked -= SkipButtonClicked;
            _requestPanelView.PlayButtonClicked -= PlayButtonClicked;

            _requestDetailView.BanForeverButtonClicked -= BanUserForever;
            _requestDetailView.BanSessionButtonClicked -= BanUserSession;
            _requestDetailView.BanSongButtonClicked -= BanSong;

        }
    }
}
