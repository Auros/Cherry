﻿using Cherry.Filters;
using Zenject;

namespace Cherry.Installers
{
    internal class CherryFilterInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<AlwaysTrueFilter>().AsSingle();
            Container.BindInterfacesTo<AlwaysFalseFilter>().AsSingle();
            Container.BindInterfacesTo<MapAgeFilter>().AsSingle();
        }
    }
}