using BeatSaberMarkupLanguage;
using HMUI;
using IPA.Utilities;
using SiraUtil.Tools;
using Zenject;

namespace Cherry.UI
{
    internal class CherryFlowCoordinator : FlowCoordinator, IInitializable
    {
        public void Initialize() { }
        private SiraLog _siraLog = null!;
        private ButtonManager _buttonManager = null!;
        private CherryRequestView _cherryRequestView = null!;
        private FlowCoordinator _parentFlowCoordinator = null!;
        private MainFlowCoordinator _mainFlowCoordinator = null!;
        private SelectLevelCategoryViewController _selectLevelCategoryViewController = null!;
        private LevelFilteringNavigationController _levelFilteringNavigationController = null!;
        private LevelCollectionNavigationController _levelCollectionNavigationController = null!;
        private static readonly FieldAccessor<SelectLevelCategoryViewController, IconSegmentedControl>.Accessor SegmentedControl = FieldAccessor<SelectLevelCategoryViewController, IconSegmentedControl>.GetAccessor("_levelFilterCategoryIconSegmentedControl");
        private static readonly FieldAccessor<SelectLevelCategoryViewController, SelectLevelCategoryViewController.LevelCategoryInfo[]>.Accessor Categories = FieldAccessor<SelectLevelCategoryViewController, SelectLevelCategoryViewController.LevelCategoryInfo[]>.GetAccessor("_levelCategoryInfos");

        [Inject]
        protected void Construct(SiraLog siraLog, ButtonManager buttonManager, CherryRequestView cherryRequestView, MainFlowCoordinator mainFlowCoordinator, SelectLevelCategoryViewController selectLevelCategoryViewController,
                                 LevelFilteringNavigationController levelFilteringNavigationController, LevelCollectionNavigationController levelCollectionNavigationController)
        {
            _siraLog = siraLog;
            _buttonManager = buttonManager;
            _cherryRequestView = cherryRequestView;
            _mainFlowCoordinator = mainFlowCoordinator;
            _selectLevelCategoryViewController = selectLevelCategoryViewController;
            _levelFilteringNavigationController = levelFilteringNavigationController;
            _levelCollectionNavigationController = levelCollectionNavigationController;
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            if (firstActivation)
            {
                SetTitle("Cherry Request Manager");
                showBackButton = true;
            }
            if (addedToHierarchy)
            {
                ProvideInitialViewControllers(_cherryRequestView);
            }
        }

        protected void Start()
        {
            _buttonManager.WasClicked += CherryWasClicked;
            _cherryRequestView.SelectLevelRequested += SelectLevel;
        }

        private void SelectLevel(IPreviewBeatmapLevel level)
        {
            _parentFlowCoordinator.DismissFlowCoordinator(this, () =>
            {
                var categories = Categories(ref _selectLevelCategoryViewController);
                for (int i = 0; i < categories.Length; i++)
                {
                    if (categories[i].levelCategory == SelectLevelCategoryViewController.LevelCategory.All)
                    {

                        var control = SegmentedControl(ref _selectLevelCategoryViewController);
                        control.SelectCellWithNumber(i);

                        _levelFilteringNavigationController.UpdateSecondChildControllerContent(SelectLevelCategoryViewController.LevelCategory.All);
                        _levelCollectionNavigationController.SelectLevel(level);
                        break;
                    }
                }
            });
        }

        protected void OnDestroy()
        {
            _buttonManager.WasClicked -= CherryWasClicked;
            _cherryRequestView.SelectLevelRequested -= SelectLevel;
        }

        private void CherryWasClicked()
        {
            _parentFlowCoordinator = _mainFlowCoordinator.YoungestChildFlowCoordinatorOrSelf();
            _parentFlowCoordinator.PresentFlowCoordinator(this, animationDirection: ViewController.AnimationDirection.Vertical);
        }

        protected override void BackButtonWasPressed(ViewController topViewController)
        {
            _parentFlowCoordinator.DismissFlowCoordinator(this);
        }
    }
}