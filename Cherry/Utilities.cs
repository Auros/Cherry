using HMUI;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Cherry
{
    internal static class Utilities
    {
        private static readonly Gradient _colorGradient = new Gradient()
        {
            colorKeys = new GradientColorKey[]
            {
                new GradientColorKey(Color.black, 0),
                new GradientColorKey(new Color(1f, 0.188f, 0.188f), .33f),
                new GradientColorKey(Color.yellow, .66f),
                new GradientColorKey(new Color(0.388f, 1f, 0.388f), 0.95f),
                new GradientColorKey(Color.cyan, 1f)
            }
        };
        private static Material _roundEdge = null!;
        public static Material UINoGlowRoundEdge
        {
            get
            {
                if (_roundEdge == null)
                {
                    _roundEdge = Resources.FindObjectsOfTypeAll<Material>().First(m => m.name == "UINoGlowRoundEdge");
                }
                return _roundEdge;
            }
        }

        public static void SetSkew(this ImageView imageView, float skew)
        {
            imageView._skew = skew;
            imageView.SetVerticesDirty();
        }

        public static void SetSkew(this Button button, float skew)
        {
            foreach (var imageView in button.GetComponentsInChildren<ImageView>(true))
            {
                imageView._skew = skew;
                imageView.SetVerticesDirty();
            }
        }

        public static Color Evaluate(float value)
        {
            return _colorGradient.Evaluate(value);
        }

        public static bool HasDangerousMessageTemplateProperty(string template)
            => template.Contains(".name%") || template.Contains(".mention%");
    }
}
