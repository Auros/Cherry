using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using System;
using Zenject;

namespace Cherry.UI
{
    [ViewDefinition("Cherry.Views.cherry-settings-view.bsml")]
    [HotReload(RelativePathToLayout = @"..\Views\cherry-settings-view.bsml")]
    internal class CherrySettingsView : BSMLAutomaticViewController
    {
        private Config _config = null!;

        [UIValue("map-rating-enabled")]
        protected bool MapRatingEnabled
        {
            get => _config.DoMapRating;
            set => _config.DoMapRating = value;
        }

        [UIValue("map-rating")]
        protected float MinimumRating
        {
            get => _config.MiniumMapRating;
            set => _config.MiniumMapRating = value;
        }

        [UIValue("automap")]
        protected bool AutoMap
        {
            get => _config.AllowAutoMappedSongs;
            set => _config.AllowAutoMappedSongs = value;
        }

        [UIValue("session-length")]
        protected float SessionLength
        {
            get => _config.SesssionLengthInHours;
            set => _config.SesssionLengthInHours = value;
        }

        [UIValue("mod-override")]
        protected bool ModOverride
        {
            get => _config.AllowL3FilterBypass;
            set => _config.AllowL3FilterBypass = value;
        }

        [UIValue("age-host")]
        protected Settings.AgeView _ageView = null!;

        [UIValue("min-njs-enabled")]
        protected bool MinNJSEnabled
        {
            get => _config.DoMinNJS;
            set => _config.DoMinNJS = value;
        }

        [UIValue("min-njs")]
        protected float MinNJS
        {
            get => _config.MinNJS;
            set => _config.MinNJS = value;
        }

        [UIValue("max-njs-enabled")]
        protected bool MaxNJSEnabled
        {
            get => _config.DoMaxNJS;
            set => _config.DoMaxNJS = value;
        }

        [UIValue("max-njs")]
        protected float MaxNJS
        {
            get => _config.MaxNJS;
            set => _config.MaxNJS = value;
        }

        [UIValue("msr-length-enabled")]
        protected bool MSRLengthEnabled
        {
            get => _config.DoMaxSongLength;
            set => _config.DoMaxSongLength = value;
        }

        [UIValue("msr-length")]
        protected float MSRLength
        {
            get => _config.MaxSongLengthInMinutes;
            set => _config.MaxSongLengthInMinutes = value;
        }

        [UIValue("unseen-request-flicker")]
        protected bool UnseenRequestFlicker
        {
            get => _config.BlinkCherryForUnseenRequests;
            set => _config.BlinkCherryForUnseenRequests = value;
        }

        [UIValue("twitch-host")]
        protected Settings.TwitchView _twitchView = null!;

        [UIValue("templating-host")]
        protected Settings.TemplatingView _templatingView = null!;

        [Inject]
        public void Construct(Config config, DiContainer container)
        {
            _config = config;
            _ageView = container.Instantiate<Settings.AgeView>();
            _twitchView = container.Instantiate<Settings.TwitchView>();
            _templatingView = container.Instantiate<Settings.TemplatingView>();
        }

        [UIAction("rating-formatter")]
        protected string FormatRating(float rating)
        {
            return string.Format("{0:0%}", rating);
    }

        [UIAction("session-formatter")]
        protected string FormatSession(float sessionLength)
        {
            return $"{sessionLength} hours";
        }

        [UIAction("minute-formatter")]
        protected string MinuteFormatter(float minutes)
        {
            float round = (float)Math.Round(minutes, 1);
            if (minutes == 1f)
            {
                return "1 minute";
            }
            return $"{round} minutes";
        }
    }
}