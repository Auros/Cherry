using Zenject;
using SiraUtil;
using Cherry.UI;
using System;

namespace Cherry.Installers
{
    internal class CherryMenuInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<ButtonManager>().AsSingle();
            Container.Bind<OpenSettingsView>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<CherrySettingsView>().FromNewComponentAsViewController().AsSingle();
            Container.Bind(typeof(IDisposable), typeof(CherryRequestView)).To<CherryRequestView>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<IInitializable>().To<CherryFlowCoordinator>().FromNewComponentOnNewGameObject(nameof(CherryFlowCoordinator)).AsSingle();
        }
    }
}