using Cherry.Installers;
using IPA;
using IPA.Config.Stores;
using SiraUtil;
using SiraUtil.Attributes;
using SiraUtil.Zenject;
using Conf = IPA.Config.Config;
using IPALogger = IPA.Logging.Logger;

namespace Cherry
{
    [Plugin(RuntimeOptions.DynamicInit), Slog]
    public class Plugin
    {
        [Init]
        public Plugin(Conf conf, IPALogger logger, Zenjector zenjector)
        {
            Config config = conf.Generated<Config>();
            zenjector.On<PCAppInit>().Pseudo(Container =>
            {
                Container.BindLoggerAsSiraLogger(logger);
                Container.BindInstance(config).AsSingle();
            });
            zenjector.OnApp<CherryCoreInstaller>();
        }

        [OnEnable]
        public void OnEnable()
        {

        }

        [OnDisable]
        public void OnDisable()
        {

        }
    }
}