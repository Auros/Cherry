using Cherry.Filters;
using Zenject;

namespace Cherry.Installers
{
    internal class CherryFilterInstaller : Installer
    {
        public override void InstallBindings()
        {
            //Container.BindInterfacesTo<AlwaysFalseFilter>().AsSingle();
            Container.BindInterfacesTo<AlwaysTrueFilter>().AsSingle();

            Container.BindInterfacesTo<QueueStatusFilter>().AsSingle();
            Container.BindInterfacesTo<AlreadyRequested>().AsSingle();

            Container.BindInterfacesTo<ConcurrencyFilter>().AsSingle();
            Container.BindInterfacesTo<BannedUserFilter>().AsSingle();
            Container.BindInterfacesTo<BannedMapFilter>().AsSingle();

            Container.BindInterfacesTo<AutomappedFilter>().AsSingle();
            Container.BindInterfacesTo<LengthFilter>().AsSingle();
            Container.BindInterfacesTo<MapAgeFilter>().AsSingle();
            Container.BindInterfacesTo<RatingFilter>().AsSingle();
        }
    }
}