using Cherry.Installers;
using IPA;
using IPA.Config.Stores;
using IPA.Loader;
using SiraUtil.Attributes;
using SiraUtil.Zenject;
using Conf = IPA.Config.Config;
using IPALogger = IPA.Logging.Logger;

namespace Cherry
{
    [Plugin(RuntimeOptions.DynamicInit), Slog, NoEnableDisable]
    public class Plugin
    {
        [Init]
        public Plugin(Conf conf, IPALogger logger, Zenjector zenjector, PluginMetadata metadata)
        {
            zenjector.UseHttpService();
            zenjector.UseLogger(logger);
            Config config = conf.Generated<Config>();

            config.Version = metadata.HVersion;

            zenjector.Install(Location.App, Container =>
            {
                Container.BindInstance(config).AsSingle();
                Container.BindInstance(new UBinder<Plugin, PluginMetadata>(metadata));
            });
            zenjector.Install<CherryCoreInstaller>(Location.App);
            zenjector.Install<CherryMenuInstaller>(Location.Menu);
            zenjector.Install<CherryFilterInstaller>(Location.App);
        }
    }
}