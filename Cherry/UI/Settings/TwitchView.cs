using BeatSaberMarkupLanguage.Attributes;

namespace Cherry.UI.Settings
{
    internal class TwitchView
    {
        private readonly Config _config;

        public TwitchView(Config config)
        {
            _config = config;
        }

        [UIValue("tts-prefix")]
        protected bool TTSPrefix
        {
            get => _config.AddTwitchTTSPrefix;
            set => _config.AddTwitchTTSPrefix = value;
        }

        [UIValue("normal-user")]
        protected bool NormalUser
        {
            get => _config.GlobalConcurrent.Enabled;
            set => _config.GlobalConcurrent.Enabled = value;
        }

        [UIValue("normal-requests")]
        protected int NormalRequests
        {
            get => _config.GlobalConcurrent.MaxConcurrentRequests;
            set => _config.GlobalConcurrent.MaxConcurrentRequests = value;
        }

        [UIValue("sub")]
        protected bool Sub
        {
            get => _config.Level1Concurrent.Enabled;
            set => _config.Level1Concurrent.Enabled = value;
        }

        [UIValue("sub-requests")]
        protected int SubRequests
        {
            get => _config.Level1Concurrent.MaxConcurrentRequests;
            set => _config.Level1Concurrent.MaxConcurrentRequests = value;
        }

        [UIValue("vip")]
        protected bool VIP
        {
            get => _config.Level2Concurrent.Enabled;
            set => _config.Level2Concurrent.Enabled = value;
        }

        [UIValue("vip-requests")]
        protected int VIPRequests
        {
            get => _config.Level2Concurrent.MaxConcurrentRequests;
            set => _config.Level2Concurrent.MaxConcurrentRequests = value;
        }

        [UIValue("mod")]
        protected bool Mod
        {
            get => _config.Level3Concurrent.Enabled;
            set => _config.Level3Concurrent.Enabled = value;
        }

        [UIValue("mod-requests")]
        protected int ModRequests
        {
            get => _config.Level3Concurrent.MaxConcurrentRequests;
            set => _config.Level3Concurrent.MaxConcurrentRequests = value;
        }
    }
}