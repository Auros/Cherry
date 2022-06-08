using BeatSaberMarkupLanguage.Attributes;
using System.ComponentModel;

namespace Cherry.UI.Settings
{
    internal class TemplatingView : INotifyPropertyChanged
    {
        private Templater _templater;
        private readonly Config _config;

        public event PropertyChangedEventHandler? PropertyChanged;

        public TemplatingView(Config config)
        {
            _config = config;
            _templater = BuildExampleTemplater(_config.ActiveRequestMessageTemplate);
        }

        [UIValue("allow-dangerous-template-properties")]
        protected bool AllowDangerousTemplateProperties
        {
            get => _config.AllowDangerousTemplateProperties;
            set => _config.AllowDangerousTemplateProperties = value;
        }

        [UIValue("selected-template")]
        protected int SelectedTemplate
        {
            get =>
                _config.RequestMessageTemplates.Contains(_config.ActiveRequestMessageTemplate) ?
                _config.RequestMessageTemplates.IndexOf(_config.ActiveRequestMessageTemplate) : 0;
            set
            {
                _config.ActiveRequestMessageTemplate = _config.RequestMessageTemplates[value];
                _templater = BuildExampleTemplater(_config.ActiveRequestMessageTemplate);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FormattedTemplate)));
            }
        }

        [UIValue("template-limit")]
        protected int TemplateLimit => _config.RequestMessageTemplates.Count - 1;

        [UIValue("formatted-template")]
        protected string FormattedTemplate => $"  " + _templater.Build();

        [UIAction("template-formatter")]
        protected string FormatTemplate(int index) => $"<color=#{BuildFormatColor(index)}>Option {index + 1}</color>";

        private string BuildFormatColor(int index) => Utilities.HasDangerousMessageTemplateProperty(_config.RequestMessageTemplates[index]) ? "e05e26" : "ffffff";

        private static Templater BuildExampleTemplater(string template)
        {
            Templater templater = new Templater(template);

            templater.AddReplacer("key", "<color=#57ffee>1d026</color>");
            templater.AddReplacer("map.name", "<color=#57ffee>seeyousoon - Faster Please</color>");
            templater.AddReplacer("map.uploader.name", "<color=#57ffee>CoolingCloset</color>");
            templater.AddReplacer("requester.mention", "<color=#57ffee>@abcbadq</color>");

            return templater;
        }
    }
}