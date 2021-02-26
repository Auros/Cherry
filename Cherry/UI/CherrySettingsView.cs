using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
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

        [UIValue("age-host")]
        protected Settings.AgeView _ageView = null!;

        [UIValue("twitch-host")]
        protected Settings.TwitchView _twitchView = null!;

        [Inject]
        public void Construct(Config config, DiContainer container)
        {
            _config = config;
            _ageView = container.Instantiate<Settings.AgeView>();
            _twitchView = container.Instantiate<Settings.TwitchView>();
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
    }
}