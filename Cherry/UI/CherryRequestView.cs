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
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Cherry.UI
{
    [ViewDefinition("Cherry.Views.request-view.bsml")]
    [HotReload(RelativePathToLayout = @"..\Views\request-view.bsml")]
    internal class CherryRequestView : BSMLAutomaticViewController, IDisposable
    {
        [UIComponent("up-button")]
        protected readonly Button upButton = null!;

        [UIComponent("down-button")]
        protected readonly Button downButton = null!;

        [UIComponent("request-list")]
        protected readonly CustomListTableData requestList = null!;

        [UIComponent("top-panel")]
        protected readonly Backgroundable topPanelBackground = null!;

        internal static readonly FieldAccessor<ImageView, float>.Accessor ImageSkew = FieldAccessor<ImageView, float>.GetAccessor("_skew");
        internal static readonly FieldAccessor<TableView, Button>.Accessor PageUpButton = FieldAccessor<TableView, Button>.GetAccessor("_pageUpButton");
        internal static readonly FieldAccessor<TableView, Button>.Accessor PageDownButton = FieldAccessor<TableView, Button>.GetAccessor("_pageDownButton");
        internal static readonly FieldAccessor<LevelListTableCell, Image>.Accessor CellCoverImage = FieldAccessor<LevelListTableCell, Image>.GetAccessor("_coverImage");
        internal static readonly FieldAccessor<LevelListTableCell, Image>.Accessor CellBackground = FieldAccessor<LevelListTableCell, Image>.GetAccessor("_backgroundImage");
        internal static readonly FieldAccessor<CustomListTableData, LevelListTableCell>.Accessor CellInstance = FieldAccessor<CustomListTableData, LevelListTableCell>.GetAccessor("songListTableCellInstance");

        private Config _config = null!;
        private SiraLog _siraLog = null!;
        private MapStore _mapStore = null!;
        private IRequestHistory _requestHistory = null!;
        private IRequestManager _requestManager = null!;
        private CherryLevelManager _cherryLevelManager = null!;
        private WebImageAsyncLoader _webImageAsyncLoader = null!;

        private bool _isProcessing;
        private RequestCellInfo? _lastSelectedCellInfo;
        private Queue<RequestEventArgs> _requestLoadingQueue = null!;

        [UIValue("detail-view")]
        private RequestDetailView _requestDetailView = null!;

        [UIValue("panel-view")]
        private RequestPanelView _requestPanelView = null!;

        [Inject]
        protected async Task Construct(Config config, SiraLog siraLog, MapStore mapStore, DiContainer container, IRequestHistory requestHistory, IRequestManager requestManager, CherryLevelManager cherryLevelManager, WebImageAsyncLoader webImageAsyncLoader)
        {
            _config = config;
            _siraLog = siraLog;
            _mapStore = mapStore;
            _requestHistory = requestHistory;
            _requestManager = requestManager;
            _cherryLevelManager = cherryLevelManager;
            _webImageAsyncLoader = webImageAsyncLoader;

            _requestLoadingQueue = new Queue<RequestEventArgs>();
            _requestDetailView = container.Instantiate<RequestDetailView>();
            _requestPanelView = container.InstantiateComponent<RequestPanelView>(gameObject);

            foreach (var request in await _requestHistory.History())
            {
                _siraLog.Null(request);
                if (!request.WasPlayed)
                    _requestLoadingQueue.Enqueue(request.Args);
            }

            _requestManager.SongRequested += SongRequested;
            _requestPanelView.SkipButtonClicked += SkipButtonClicked;
            _requestPanelView.QueueButtonClicked += QueueButtonClicked;
        }

        private void SkipButtonClicked()
        {
            if (_lastSelectedCellInfo != null)
            {
                _requestManager.Remove(_lastSelectedCellInfo.request);

                _requestDetailView.SetLoading();
                _requestPanelView.SetPlayButtonColor(null);
                _requestPanelView.SetPlayButtonText("Play");
                _requestPanelView.SetPlayButtonInteractability(false);
                _requestPanelView.SetSkipButtonInteractability(false);
                requestList.data.Remove(_lastSelectedCellInfo);
                requestList.tableView.ReloadData();
                requestList.tableView.SelectCellWithIdx(-1);
                _lastSelectedCellInfo = null;
            }
        }

        private void QueueButtonClicked()
        {
            SetQueueStatus(!_config.QueueOpened);
        }

        [UIAction("selected-request")]
        protected void SelectedCell(TableView _, int index)
        {
            RequestCellInfo request = (requestList.data[index] as RequestCellInfo)!;
            _lastSelectedCellInfo = request;
            _requestDetailView.SetData(
                request.map.Name,
                request.map.Uploader.Name,
                request.request.Requester.Username,
                request.icon,
                (float)request.map.MapStats.Rating,
                request.request.RequestTime
            );
            _requestPanelView.SetSkipButtonInteractability(true);
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
                _requestPanelView.SetPlayButtonColor(Color.red);
                _requestPanelView.SetPlayButtonInteractability(true);
            }
        }

        private void SongRequested(object sender, RequestEventArgs e)
        {
            _requestLoadingQueue.Enqueue(e);
        }

        private async Task SongRequestedAsync(RequestEventArgs e)
        {
            _isProcessing = true;
            try
            {
                Map? mapq = await _mapStore.GetMapAsync(e.Key);
                if (mapq == null)
                    return;
                
                Map map = mapq.Value;
                Sprite coverSprite = await _webImageAsyncLoader.LoadSpriteAsync($"https://beatsaver.com{map.CoverURL}", CancellationToken.None);
                RequestCellInfo cell = new RequestCellInfo(e, map, coverSprite);
                requestList.data.Add(cell);

                requestList.tableView.ReloadData();
            }
            catch (Exception ex)
            {
                _siraLog.Error("An error has occured.");
                _siraLog.Logger.Error(ex);
            }
            _isProcessing = false;
        }

        protected void Update()
        {
            if (isInViewControllerHierarchy && !_isProcessing && _requestLoadingQueue.Count > 0)
            {
                _ = SongRequestedAsync(_requestLoadingQueue.Dequeue());
            }
        }

        [UIAction("#post-parse")]
        protected void Parsed()
        {
            PageUpButton(ref requestList.tableView) = upButton;
            PageDownButton(ref requestList.tableView) = downButton;
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
            ImageView topBackground = (topPanelBackground.background as ImageView)!;
            ImageSkew(ref topBackground) = 0f;
            topBackground.color = Color.white;
            topBackground.color0 = new Color(0.217f, 0.782f, 0f);
            topBackground.color1 = new Color(0.065f, 0.239f, 0f);
            topBackground.SetVerticesDirty();

            _requestPanelView.SetPlayButtonText("Play");
            _requestPanelView.SetPlayButtonInteractability(false);
            _requestPanelView.SetSkipButtonInteractability(false);
            _requestPanelView.SetPlayButtonColor(null);

            _requestPanelView.SetQueueButtonText("Open Queue");
            _requestDetailView.SetLoading();

            _requestPanelView.SetQueueButtonColor(_config.QueueOpened ? Color.green : Color.red);
            _requestPanelView.SetQueueButtonText(_config.QueueOpened ? "Close Queue" : "Open Queue");

            //_requestDetailView.SetData("Cherry Song", "Cherry Uploader", "Auros", BeatSaberMarkupLanguage.Utilities.ImageResources.BlankSprite, 0.91f, DateTime.Now);
        }

        private void SetQueueStatus(bool value)
        {
            _config.QueueOpened = value;
            _requestPanelView.SetQueueButtonColor(_config.QueueOpened ? Color.green : Color.red);
            _requestPanelView.SetQueueButtonText(_config.QueueOpened ? "Close Queue" : "Open Queue");
        }

        public void Dispose()
        {
            _requestManager.SongRequested -= SongRequested;
            _requestPanelView.SkipButtonClicked -= SkipButtonClicked;
            _requestPanelView.QueueButtonClicked -= QueueButtonClicked;
        }
    }
}