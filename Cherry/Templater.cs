using System.Collections.Generic;
using System.Text;

namespace Cherry
{
    /// <summary>
    /// Allows formatting of messages with keyed template values. Builds the string all at once to prevent value injection.
    /// </summary>
    internal class Templater
    {
        private readonly string _template;
        private readonly Dictionary<string, string> _replacers = new Dictionary<string, string>();

        public Templater(string template)
        {
            _template = template;
        }

        public void AddReplacer(string key, string value)
        {
            _replacers[key] = value;
        }

        public string Build()
        {
            StringBuilder sb = new StringBuilder();

            int? enter = null;
            int? exit = null;

            for (int i = 0; i < _template.Length; i++)
            {
                var currentChar = _template[i];
                if (currentChar == '%')
                {
                    if (i != 0 && _template[i - 1] == '\\')
                    {
                        sb[sb.Length - 1] = currentChar;
                        continue;
                    }

                    if (!enter.HasValue)
                        enter = i;
                    else
                        exit = i;

                    if (!enter.HasValue || !exit.HasValue)
                        continue;

                    var key = _template.Substring(enter.Value + 1, exit.Value - enter.Value - 1);
                    if (_replacers.TryGetValue(key, out var value))
                        sb.Append(value);

                    i = exit.Value;
                    enter = null;
                    exit = null;
                }
                else if (!enter.HasValue)
                {
                    sb.Append(currentChar);
                }
            }
            return sb.ToString();
        }
    }
}