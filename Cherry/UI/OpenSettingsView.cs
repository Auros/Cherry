using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using System;

namespace Cherry.UI
{
    [ViewDefinition("Cherry.Views.open-settings-view.bsml")]
    [HotReload(RelativePathToLayout = @"..\Views\open-settings-view.bsml")]
    internal class OpenSettingsView : BSMLAutomaticViewController
    {
        public event Action? OpenSettingsButtonClicked;

        [UIAction("open-settings-clicked")] protected void OSC() => OpenSettingsButtonClicked?.Invoke();
    }
}