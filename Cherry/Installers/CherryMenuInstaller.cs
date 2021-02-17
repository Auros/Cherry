using Cherry.UI;
using Zenject;

namespace Cherry.Installers
{
    internal class CherryMenuInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<ButtonManager>().AsSingle();
        }
    }
}