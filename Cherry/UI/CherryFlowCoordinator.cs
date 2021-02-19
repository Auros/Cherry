using BeatSaberMarkupLanguage;
using HMUI;
using Zenject;

namespace Cherry.UI
{
    internal class CherryFlowCoordinator : FlowCoordinator, IInitializable
    {
        public void Initialize() { }
        private ButtonManager _buttonManager = null!;
        private CherryRequestView _cherryRequestView = null!;
        private SoloFreePlayFlowCoordinator _soloFreePlayFlowCoordinator = null!;

        [Inject]
        protected void Construct(ButtonManager buttonManager, CherryRequestView cherryRequestView, SoloFreePlayFlowCoordinator soloFreePlayFlowCoordinator)
        {
            _buttonManager = buttonManager;
            _cherryRequestView = cherryRequestView;
            _soloFreePlayFlowCoordinator = soloFreePlayFlowCoordinator;
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
        }

        protected void OnDestroy()
        {
            _buttonManager.WasClicked -= CherryWasClicked;
        }

        private void CherryWasClicked()
        {
            if (_soloFreePlayFlowCoordinator.IsFlowCoordinatorInHierarchy(_soloFreePlayFlowCoordinator))
            {
                _soloFreePlayFlowCoordinator.PresentFlowCoordinator(this, animationDirection: ViewController.AnimationDirection.Vertical);
            }
        }

        protected override void BackButtonWasPressed(ViewController topViewController)
        {
            _soloFreePlayFlowCoordinator.DismissFlowCoordinator(this);
        }
    }
}