using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using IPA.Utilities;
using SiraUtil.Tools;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Cherry.UI
{
    [ViewDefinition("Cherry.Views.request-view.bsml")]
    [HotReload(RelativePathToLayout = @"..\Views\request-view.bsml")]
    internal class CherryRequestView : BSMLAutomaticViewController
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
        internal static readonly FieldAccessor<LevelListTableCell, Image>.Accessor CellBackground = FieldAccessor<LevelListTableCell, Image>.GetAccessor("_backgroundImage");
        internal static readonly FieldAccessor<CustomListTableData, LevelListTableCell>.Accessor CellInstance = FieldAccessor<CustomListTableData, LevelListTableCell>.GetAccessor("songListTableCellInstance");

        private SiraLog _siraLog = null!;

        [UIValue("detail-view")]
        private RequestDetailView _requestDetailView = null!;

        [UIValue("panel-view")]
        private RequestPanelView _requestPanelView = null!;

        [Inject]
        protected void Construct(SiraLog siraLog, DiContainer container)
        {
            _siraLog = siraLog;
            _requestDetailView = container.Instantiate<RequestDetailView>();
            _requestPanelView = container.InstantiateComponent<RequestPanelView>(gameObject);
        }

        [UIAction("#post-parse")]
        protected async Task Parsed()
        {
            PageUpButton(ref requestList.tableView) = upButton;
            PageDownButton(ref requestList.tableView) = downButton;

            var list = requestList;
            var inst = CellInstance(ref list);
            if (inst == null)
                inst = CellInstance(ref list) = list.GetTableCell();
            var bg = CellBackground(ref inst);
            var cellBackground = (bg as ImageView)!;
            ImageSkew(ref cellBackground) = 0f;
            cellBackground.SetVerticesDirty();

            upButton.SetSkew(0f);
            downButton.SetSkew(0f);

            ImageView topBackground = (topPanelBackground.background as ImageView)!;
            ImageSkew(ref topBackground) = 0f;
            topBackground.color = Color.white;
            topBackground.color0 = new Color(0.217f, 0.782f, 0f);
            topBackground.color1 = new Color(0.065f, 0.239f, 0f);
            topBackground.SetVerticesDirty();

            await Task.CompletedTask;

            var uwu = BeatSaberMarkupLanguage.Utilities.FindSpriteInAssembly("Cherry.Resources.cherry.png");
            uwu.texture.wrapMode = TextureWrapMode.Clamp;

            requestList.data.Clear();
            for (int i = 0; i < 6; i++)
                requestList.data.Add(new CustomListTableData.CustomCellInfo("uwu", "owo", uwu));
            requestList.tableView.ReloadData();

            _requestPanelView.SetPlayButtonColor(null);
            _requestPanelView.SetPlayButtonText("Play");
            _requestPanelView.SetQueueButtonColor(Color.red);
            _requestPanelView.SetQueueButtonText("Open Queue");
            _requestDetailView.SetData("Cherry Song", "Cherry Uploader", "Auros", uwu, 0.91f, System.DateTime.Now);
        }
    }
}