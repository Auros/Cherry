using ChatCore;
using Cherry.Managers;
using SiraUtil.Zenject;
using Zenject;

namespace Cherry.Installers
{
    internal class CherryCoreInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInstance(new UBinder<Plugin, ChatCoreInstance>(ChatCoreInstance.Create())).AsSingle();
            Container.BindInterfacesTo<CherryRequestManager>().AsSingle();
            Container.BindInterfacesTo<TwitchRequestSource>().AsSingle();
            Container.Bind<MapStore>().AsSingle();
        }
    }
}