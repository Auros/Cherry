using CatCore;
using Cherry.Interfaces;
using Cherry.Managers;
using SiraUtil.Zenject;
using System;
using Zenject;

namespace Cherry.Installers
{
    internal class CherryCoreInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.Bind(typeof(IDisposable), typeof(WebImageAsyncLoader)).To<WebImageAsyncLoader>().AsSingle();
            Container.BindInstance(new UBinder<Plugin, CatCoreInstance>(CatCoreInstance.Create())).AsSingle();
            Container.BindInterfacesTo<RequestHistoryDatabase>().AsSingle();
            Container.BindInterfacesTo<CherryRequestManager>().AsSingle();
            Container.BindInterfacesTo<TwitchRequestSource>().AsSingle();
            Container.Bind<IDenier>().To<CherryDenylist>().AsSingle();
            Container.Bind<CherryLevelManager>().AsSingle();
            Container.Bind<MapStore>().AsSingle();
        }
    }
}