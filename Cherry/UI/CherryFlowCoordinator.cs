using BeatSaberMarkupLanguage;
using HMUI;
using SiraUtil.Logging;
using System.Linq;
using UnityEngine;
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

        [Inject]
        protected void Construct(SiraLog siraLog, ButtonManager buttonManager, OpenSettingsView openSettingsView, CherryRequestView cherryRequestView, CherrySettingsView cherrySettingsView, MainFlowCoordinator mainFlowCoordinator)
        {
            _siraLog = siraLog;
            _buttonManager = buttonManager;
            _openSettingsView = openSettingsView;
            _cherryRequestView = cherryRequestView;
            _cherrySettingsView = cherrySettingsView;
            _mainFlowCoordinator = mainFlowCoordinator;
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
                bool selected = false;
                foreach (var controller in Active<LevelCollectionNavigationController>())
                {
                    controller.SelectLevel(level);
                    selected = true;
                }

                foreach (var controller in Active<LevelCollectionViewController>())
                {
                    controller.SelectLevel(level);
                    selected = true;
                }

                if (!selected)
                    _siraLog.Warn("Could not find an active level collection for the requested map.");
            });
        }

        private static T[] Active<T>() where T : Component
        {
            return Resources.FindObjectsOfTypeAll<T>()
                .Where(item => item != null && item.gameObject != null && item.gameObject.activeInHierarchy)
                .ToArray();
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
