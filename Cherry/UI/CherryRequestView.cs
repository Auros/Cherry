using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using Cherry.Interfaces;
using Cherry.Managers;
using Cherry.Models;
using HMUI;
using IPA.Utilities;
using SiraUtil.Tools;
using System;
using System.Collections.Generic;
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

        #region Accessors

        internal static readonly FieldAccessor<ImageView, float>.Accessor ImageSkew = FieldAccessor<ImageView, float>.GetAccessor("_skew");
        internal static readonly FieldAccessor<ImageView, Color>.Accessor ImageColor0 = FieldAccessor<ImageView, Color>.GetAccessor("_color0");
        internal static readonly FieldAccessor<ImageView, Color>.Accessor ImageColor1 = FieldAccessor<ImageView, Color>.GetAccessor("_color1");
        internal static readonly FieldAccessor<TableView, ScrollView>.Accessor Scroller = FieldAccessor<TableView, ScrollView>.GetAccessor("_scrollView");
        internal static readonly FieldAccessor<LevelListTableCell, Image>.Accessor CellCoverImage = FieldAccessor<LevelListTableCell, Image>.GetAccessor("_coverImage");
        internal static readonly FieldAccessor<LevelListTableCell, TextMeshProUGUI>.Accessor SongNameText = FieldAccessor<LevelListTableCell, TextMeshProUGUI>.GetAccessor("_songNameText");
        internal static readonly FieldAccessor<LevelListTableCell, TextMeshProUGUI>.Accessor SongAuthorText = FieldAccessor<LevelListTableCell, TextMeshProUGUI>.GetAccessor("_songAuthorText");
        internal static readonly FieldAccessor<ScrollView, Button>.Accessor PageUpButton = FieldAccessor<ScrollView, Button>.GetAccessor("_pageUpButton");
        internal static readonly FieldAccessor<ScrollView, Button>.Accessor PageDownButton = FieldAccessor<ScrollView, Button>.GetAccessor("_pageDownButton");
        internal static readonly FieldAccessor<LevelListTableCell, Image>.Accessor CellBackground = FieldAccessor<LevelListTableCell, Image>.GetAccessor("_backgroundImage");
        internal static readonly FieldAccessor<CustomListTableData, LevelListTableCell>.Accessor CellInstance = FieldAccessor<CustomListTableData, LevelListTableCell>.GetAccessor("songListTableCellInstance");

        #endregion

        private bool _isDirty;
        private bool _isInHistory;
        private bool _isProcessing;
        private Config _config = null!;
        private IDenier _denier = null!;
        private SiraLog _siraLog = null!;
        private MapStore _mapStore = null!;
        private IRequestHistory _requestHistory = null!;
        private IRequestManager _requestManager = null!;
        private TweeningManager _tweeningManager = null!;
        private CherryLevelManager _cherryLevelManager = null!;
        private WebImageAsyncLoader _webImageAsyncLoader = null!;

        private RequestCellInfo? _lastSelectedCellInfo;
        private CancellationTokenSource? _downloadCancelSource;
        private List<RequestEventArgs> _skipLoadingQueue = null!;
        private Queue<RequestEventArgs> _requestLoadingQueue = null!;
        private Queue<RequestEventArgs> _historyLoadingQueue = null!;
        private readonly List<RequestCellInfo> _activeRequests = new List<RequestCellInfo>();
        public event Action<IPreviewBeatmapLevel>? SelectLevelRequested;

        private readonly Color _downloadButtonColor = new Color(0.7f, 0.47f, 0f);
        private readonly Color _openColor0 = new Color(0.217f, 0.782f, 0f);
        private readonly Color _openColor1 = new Color(0.065f, 0.239f, 0f);
        private readonly Color _closedColor0 = new Color(0.804f, 0.217f, 0.152f);
        private readonly Color _closedColor1 = new Color(0.652f, 0f, 0f);

        [Inject]
        protected async Task Construct(Config config, IDenier denier, SiraLog siraLog, MapStore mapStore, DiContainer container, IRequestHistory requestHistory, IRequestManager requestManager, TweeningManager tweeningManager, CherryLevelManager cherryLevelManager, WebImageAsyncLoader webImageAsyncLoader)
        {
            _config = config;
            _denier = denier;
            _siraLog = siraLog;
            _mapStore = mapStore;
            _requestHistory = requestHistory;
            _requestManager = requestManager;
            _tweeningManager = tweeningManager;
            _cherryLevelManager = cherryLevelManager;
            _webImageAsyncLoader = webImageAsyncLoader;

            _skipLoadingQueue = new List<RequestEventArgs>();
            _requestLoadingQueue = new Queue<RequestEventArgs>();
            _historyLoadingQueue = new Queue<RequestEventArgs>();
            _requestDetailView = container.Instantiate<RequestDetailView>();
            _requestPanelView = container.InstantiateComponent<RequestPanelView>(gameObject);

            foreach (var request in (await _requestHistory.History()).Where(r => !r.WasPlayed).OrderBy(h => h.Args.RequestTime))
                _requestLoadingQueue.Enqueue(request.Args);

            _requestManager.SongRequested += SongRequested;
            _requestManager.SongSkipped += RequestManager_SongSkipped;

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
            if (_isDirty)
            {
                requestList.tableView.ReloadData();
                ResetSubPanels();
                _isDirty = false;
            }
        }

        [UIAction("#post-parse")]
        protected void Parsed()
        {
            var scroller = Scroller(ref requestList.tableView);
            //HideScrollButtons(ref scroller) = false;

            PageUpButton(ref scroller) = upButton;
            PageDownButton(ref scroller) = downButton;

            var list = requestList;
            var inst = CellInstance(ref list);
            if (inst == null)
                inst = CellInstance(ref list) = list.GetTableCell();
            var bg = CellBackground(ref inst);
            var cbg = CellCoverImage(ref inst);
            var cellBackground = (bg as ImageView)!;
            var cellCoverImage = (cbg as ImageView)!;

            ImageSkew(ref cellBackground) = 0f;
            ImageSkew(ref cellCoverImage) = 0f;
            cellCoverImage.SetVerticesDirty();
            cellBackground.SetVerticesDirty();
            upButton.SetSkew(0f);
            downButton.SetSkew(0f);

            var songNameText = SongNameText(ref inst);
            var authorNameText = SongAuthorText(ref inst);
            songNameText.fontSizeMin = 1f;
            songNameText.fontSizeMax = 4f;
            songNameText.enableAutoSizing = true;
            songNameText.transform.localPosition = new Vector2(authorNameText.transform.position.x, songNameText.transform.localPosition.y);
            songNameText.fontStyle = FontStyles.Normal;
            authorNameText.fontStyle = FontStyles.Normal;

            ImageView topBackground = (topPanelBackground.background as ImageView)!;
            ImageSkew(ref topBackground) = 0f;
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

            RequestCellInfo request = (requestList.data[index] as RequestCellInfo)!;

            int durationInSeconds = request.map.MapMetadata.Duration;
            if (durationInSeconds == 0)
            {
                durationInSeconds = request.map.MapMetadata.MapCharacteristics.Select(f => f.Difficulties.AnyLength).FirstOrDefault();
            }
            _lastSelectedCellInfo = request;
            _requestDetailView.SetData(
                request.map.Name,
                request.map.Uploader.Name,
                request.request,
                request.icon,
                (float)request.map.MapStats.Rating,
                durationInSeconds
            );
            _requestPanelView.SetSkipButtonInteractability(!_isInHistory);
            bool levelInstalled = _cherryLevelManager.LevelIsInstalled(request.map.Hash);
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

        private void RequestManager_SongSkipped(object sender, RequestEventArgs e)
        {
            var request = _requestLoadingQueue.FirstOrDefault(c => c.RequestTime == e.RequestTime && c.Requester.ID == e.Requester.ID);
            if (request != null)
            {
                _skipLoadingQueue.Add(e);
                return;
            }

            if (requestList == null)
                return;

            var req = requestList.data.Cast<RequestCellInfo>().FirstOrDefault(c => c.request.RequestTime == e.RequestTime && c.request.Requester.ID == e.Requester.ID);
            if (req != null)
            {
                requestList.data.Remove(req);
                if (isActiveAndEnabled)
                {
                    requestList.tableView.ReloadData();
                    ResetSubPanels();
                }
                else
                {
                    _isDirty = true;
                }
            }
        }

        private void PlayButtonClicked()
        {
            if (_lastSelectedCellInfo != null)
            {
                var map = _lastSelectedCellInfo.map;
                var request = _lastSelectedCellInfo.request;
                IPreviewBeatmapLevel? level = _cherryLevelManager.TryGetLevel(map.Hash);
                if (level != null)
                {
                    RemoveAndReloadLatest();
                    SelectLevelRequested?.Invoke(level);
                    _requestManager.MarkAsRead(request);
                    if (_isInHistory) HistoryButtonClicked();
                }
                else
                {
                    if (_downloadCancelSource != null)
                    {
                        _downloadCancelSource.Cancel();
                        _downloadCancelSource = null;
                    }
                    else
                    {
                        _ = DownloadRequest(_lastSelectedCellInfo);
                    }
                }
            }
        }

        private async Task DownloadRequest(RequestCellInfo cell)
        {
            _downloadCancelSource = new CancellationTokenSource();
            Progress<double> progress = new Progress<double>();
            progress.ProgressChanged += Progress_ProgressChanged;
            IPreviewBeatmapLevel? level = null;
            try
            {
                level = await _cherryLevelManager.DownloadLevel($"{cell.map.Key} ({cell.map.MapMetadata.SongName} - {cell.map.Uploader.Name})", cell.map.Hash, $"https://beatsaver.com{cell.map.DownloadURL}", _downloadCancelSource.Token, progress);
            }
            catch
            {

            }
            _downloadCancelSource = null;
            progress.ProgressChanged -= Progress_ProgressChanged;
            if (level != null)
            {
                _requestPanelView.SetPlayButtonText("Play");
                _requestPanelView.SetPlayButtonColor(null);
                RemoveAndReloadLatest();
                SelectLevelRequested?.Invoke(level);
                _requestManager.MarkAsRead(cell.request);
            }
            else
            {
                _requestPanelView.SetPlayButtonText("Download");
                _requestPanelView.SetPlayButtonColor(_downloadButtonColor);
            }
        }

        private void RemoveAndReloadLatest()
        {
            requestList.data.Remove(_lastSelectedCellInfo);
            requestList.tableView.ReloadData();
            ResetSubPanels();
        }

        private void Progress_ProgressChanged(object sender, double value)
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
            var lid = requester.ID.ToLower();
            var requesters = requestList.data.Cast<RequestCellInfo>().Where(r => r.request.Requester.ID.ToLower() == lid).ToArray();
            for (int i = 0; i < requesters.Length; i++)
            {
                _requestManager.Remove(requesters[i].request);
                requestList.data.Remove(requesters[i]);
            }
            requestList.tableView.ReloadData();
        }

        private void HistoryButtonClicked()
        {
            _ = HistoryButtonClickedAsync();
        }

        private async Task HistoryButtonClickedAsync()
        {
            _requestPanelView.SetHistoryButtonText(_isInHistory ? "History" : "Queue");
            requestList.tableView.SelectCellWithIdx(-1);
            ResetSubPanels();
            if (_isInHistory)
            {
                _isInHistory = false;
                requestList.data.Clear();
                requestList.data.AddRange(_activeRequests);
                requestQueueText.text = "Request Queue";
                requestList.tableView.ReloadData();
                _activeRequests.Clear();
            }
            else
            {
                _isInHistory = true;
                _activeRequests.Clear();
                _activeRequests.AddRange(requestList.data.Cast<RequestCellInfo>());
                requestList.data.Clear();
                requestList.tableView.ReloadData();
                requestQueueText.text = "History";

                foreach (var request in (await _requestHistory.History()).Where(r => r.WasPlayed).Take(10))
                    _historyLoadingQueue.Enqueue(request.Args);
            }
        }

        private void SkipButtonClicked()
        {
            if (_lastSelectedCellInfo != null)
            {
                _requestManager.Remove(_lastSelectedCellInfo.request);
                requestList.data.Remove(_lastSelectedCellInfo);
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
            requestList.tableView.ReloadData();
            requestList.tableView.SelectCellWithIdx(-1);
            _lastSelectedCellInfo = null;
        }

        private void QueueButtonClicked()
        {
            SetQueueStatus(!_config.QueueOpened);
        }

        private void SongRequested(object sender, RequestEventArgs e)
        {
            _requestLoadingQueue.Enqueue(e);
        }

        private async Task SongRequestedAsync(RequestEventArgs e, bool forHistory)
        {
            _isProcessing = true;

            if (_skipLoadingQueue.Contains(e))
            {
                _skipLoadingQueue.Remove(e);
                _isProcessing = false;
                return;
            }

            try
            {
                Map? mapq = await _mapStore.GetMapAsync(e.Key);
                if (mapq == null)
                    return;
                
                Map map = mapq.Value;
                Sprite coverSprite = await _webImageAsyncLoader.LoadSpriteAsync($"https://beatsaver.com{map.CoverURL}", CancellationToken.None);
                RequestCellInfo cell = new RequestCellInfo(e, map, coverSprite);

                if (!(forHistory && !_isInHistory))
                {
                    requestList.data.Add(cell);
                    requestList.tableView.ReloadData();
                }
            }
            catch (Exception ex)
            {
                _siraLog.Error("An error has occured.");
                _siraLog.Logger.Error(ex);
            }
            _isProcessing = false;
        }

        private void SetQueueStatus(bool value)
        {
            _config.QueueOpened = value;
            UpdateQueueColors();
        }

        private void UpdateQueueColors()
        {
            ImageView topBackground = (topPanelBackground.background as ImageView)!;
            _requestPanelView.SetQueueButtonColor(_config.QueueOpened ? Color.green : Color.red);
            _requestPanelView.SetQueueButtonText(_config.QueueOpened ? "Close Queue" : "Open Queue");

            var startColor0 = ImageColor0(ref topBackground);
            var startColor1 = ImageColor1(ref topBackground);
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

        protected void Update()
        {
            if (isInViewControllerHierarchy && !_isProcessing)
            {
                if (_requestLoadingQueue.Count > 0 && !_isInHistory)
                    _ = SongRequestedAsync(_requestLoadingQueue.Dequeue(), false);
                else if (_historyLoadingQueue.Count > 0)
                    _ = SongRequestedAsync(_historyLoadingQueue.Dequeue(), true);
            }
        }

        public void Dispose()
        {
            _requestPanelView.HistoryButtonClicked -= HistoryButtonClicked;
            _requestPanelView.QueueButtonClicked -= QueueButtonClicked;
            _requestPanelView.SkipButtonClicked -= SkipButtonClicked;
            _requestPanelView.PlayButtonClicked -= PlayButtonClicked;

            _requestDetailView.BanForeverButtonClicked -= BanUserForever;
            _requestDetailView.BanSessionButtonClicked -= BanUserSession;
            _requestDetailView.BanSongButtonClicked -= BanSong;

            _requestManager.SongSkipped -= RequestManager_SongSkipped;
            _requestManager.SongRequested -= SongRequested;
        }
    }
}