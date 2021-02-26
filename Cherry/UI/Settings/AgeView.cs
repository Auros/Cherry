using BeatSaberMarkupLanguage.Attributes;
using System;
using System.Collections.Generic;

namespace Cherry.UI.Settings
{
    internal class AgeView
    {
        private readonly Config _config;

        public AgeView(Config config)
        {
            _config = config;

            for (int i = 1; i <= 12; i++)
                months.Add(i);
            for (int i = 2018; i <= DateTime.Now.Year; i++)
                years.Add(i);
        }

        [UIValue("age-filter-enabled")]
        protected bool AgeFilterEnabled
        {
            get => _config.DoMapAge;
            set => _config.DoMapAge = value;
        }

        [UIValue("month-options")]
        protected List<int> months = new List<int>();

        [UIValue("year-options")]
        protected List<int> years = new List<int>();

        [UIValue("month")]
        protected int MapMonth
        {
            get => _config.MinimumAge.Month;
            set => _config.MinimumAge = new DateTime(_config.MinimumAge.Year, value, _config.MinimumAge.Day);
        }

        [UIValue("year")]
        protected int Year
        {
            get => _config.MinimumAge.Year;
            set => _config.MinimumAge = new DateTime(value, _config.MinimumAge.Month, _config.MinimumAge.Day);
        }

        [UIAction("format-month")]
        public string FormatMonth(int month)
        {
            return ((Month)(month - 1)).ToString();
        }

        public enum Month
        {
            January,
            February,
            March,
            April,
            May,
            June,
            July,
            August,
            September,
            October,
            November,
            December
        }
    }
}