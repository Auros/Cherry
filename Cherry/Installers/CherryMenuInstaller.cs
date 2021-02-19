using Zenject;
using SiraUtil;
using Cherry.UI;

namespace Cherry.Installers
{
    internal class CherryMenuInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<ButtonManager>().AsSingle();
            Container.Bind<CherryRequestView>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<IInitializable>().To<CherryFlowCoordinator>().FromNewComponentOnNewGameObject(nameof(CherryFlowCoordinator)).AsSingle();
        }
    }
}