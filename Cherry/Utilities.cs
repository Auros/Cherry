using HMUI;
using IPA.Utilities;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Cherry
{
    internal static class Utilities
    {
        internal static readonly FieldAccessor<ImageView, float>.Accessor ImageSkew = FieldAccessor<ImageView, float>.GetAccessor("_skew");
        internal static readonly FieldAccessor<ImageView, Color>.Accessor ImageViewColor0 = FieldAccessor<ImageView, Color>.GetAccessor("_color0");
        internal static readonly FieldAccessor<ImageView, Color>.Accessor ImageViewColor1 = FieldAccessor<ImageView, Color>.GetAccessor("_color1");
        
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
            ImageSkew(ref imageView) = skew;
            imageView.SetVerticesDirty();
        }

        public static void SetSkew(this Button button, float skew)
        {
            foreach (var imageView in button.GetComponentsInChildren<ImageView>(true))
            {
                var image = imageView;
                ImageSkew(ref image) = skew;
                image.SetVerticesDirty();
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