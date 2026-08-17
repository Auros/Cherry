using BeatSaberMarkupLanguage;
using HMUI;
using SiraUtil.Logging;
using Zenject;

namespace Cherry.UI
{
    internal class CherryFlowCoordinator : FlowCoordinator, IInitializable
    {
        public void Initialize() { }
        private SiraLog _siraLog = null!;
        private bool _openedSettings = false;

        private ButtonManager _buttonManager = null!;
        private OpenSettingsView _openSettingsView = null!;
        private CherryRequestView _cherryRequestView = null!;
        private CherrySettingsView _cherrySettingsView = null!;
        private FlowCoordinator _presentingFlowCoordinator = null!;
        private MainFlowCoordinator _mainFlowCoordinator = null!;

        private LevelSearchViewController _levelSearchViewController = null!;
        private SelectLevelCategoryViewController _selectLevelCategoryViewController = null!;
        private LevelFilteringNavigationController _levelFilteringNavigationController = null!;
        private LevelCollectionNavigationController _levelCollectionNavigationController = null!;

        [Inject]
        protected void Construct(SiraLog siraLog, ButtonManager buttonManager, OpenSettingsView openSettingsView, CherryRequestView cherryRequestView, CherrySettingsView cherrySettingsView, MainFlowCoordinator mainFlowCoordinator, LevelSearchViewController levelSearchViewController,
                                 SelectLevelCategoryViewController selectLevelCategoryViewController, LevelFilteringNavigationController levelFilteringNavigationController, LevelCollectionNavigationController levelCollectionNavigationController)
        {
            _siraLog = siraLog;
            _buttonManager = buttonManager;
            _openSettingsView = openSettingsView;
            _cherryRequestView = cherryRequestView;
            _cherrySettingsView = cherrySettingsView;
            _mainFlowCoordinator = mainFlowCoordinator;
            _levelSearchViewController = levelSearchViewController;
            _selectLevelCategoryViewController = selectLevelCategoryViewController;
            _levelFilteringNavigationController = levelFilteringNavigationController;
            _levelCollectionNavigationController = levelCollectionNavigationController;
        }

        public override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            if (firstActivation)
            {
                SetTitle("Cherry Request Manager");
                showBackButton = true;
            }
            if (addedToHierarchy)
            {
                ViewController view = _openSettingsView;
                if (_openedSettings) view = _cherrySettingsView;
                ProvideInitialViewControllers(_cherryRequestView, view);
            }
        }

        protected void Start()
        {
            _buttonManager.WasClicked += CherryWasClicked;
            _cherryRequestView.SelectLevelRequested += SelectLevel;
            _openSettingsView.OpenSettingsButtonClicked += OpenSettingsButtonWasClicked;
        }

        private void OpenSettingsButtonWasClicked()
        {
            _openedSettings = true;
            SetLeftScreenViewController(_cherrySettingsView, ViewController.AnimationType.In);
        }

        private void SelectLevel(BeatmapLevel level)
        {
            _presentingFlowCoordinator.DismissFlowCoordinator(this, () =>
            {
                var categories = _selectLevelCategoryViewController._levelCategoryInfos;
                for (int i = 0; i < categories.Length; i++)
                {
                    if (categories[i].levelCategory == SelectLevelCategoryViewController.LevelCategory.All)
                    {
                        var control = _selectLevelCategoryViewController._levelFilterCategoryIconSegmentedControl;
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
            _openSettingsView.OpenSettingsButtonClicked -= OpenSettingsButtonWasClicked;
        }

        private void CherryWasClicked()
        {
            _presentingFlowCoordinator = _mainFlowCoordinator.YoungestChildFlowCoordinatorOrSelf();
            _presentingFlowCoordinator.PresentFlowCoordinator(this, animationDirection: ViewController.AnimationDirection.Vertical);
        }

        public override void BackButtonWasPressed(ViewController topViewController)
        {
            _presentingFlowCoordinator.DismissFlowCoordinator(this);
        }
    }
}
