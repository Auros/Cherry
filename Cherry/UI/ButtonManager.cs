using BeatSaberMarkupLanguage.Components;
using IPA.Loader;
using SiraUtil.Zenject;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VRUIControls;
using Zenject;

namespace Cherry.UI
{
    internal class ButtonManager : IInitializable, IDisposable
    {
        private ClickableImage? _image;
        public event Action? WasClicked;
        private readonly Assembly _assembly;
        private readonly DiContainer _container;
        private readonly LevelSelectionNavigationController _levelSelectionNavigationController;

        public Color? DefaultColor
        {
            get
            {
                if (_image != null)
                {
                    return _image.DefaultColor;
                }
                return null;
            }
            set
            {
                if (_image != null && value.HasValue)
                {
                    _image.DefaultColor = value.Value;
                }
            }
        }

        public ButtonManager(DiContainer container, UBinder<Plugin, PluginMetadata> metadataBinder, LevelSelectionNavigationController levelSelectionNavigationController)
        {
            _container = container;
            _assembly = metadataBinder.Value.Assembly;
            _levelSelectionNavigationController = levelSelectionNavigationController;
        }

        public void Initialize()
        {
            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            _image = CreateImage();
            using Stream mrs = _assembly.GetManifestResourceStream("Cherry.Resources.cherry.png");
            using MemoryStream ms = new MemoryStream();
            await mrs.CopyToAsync(ms);

            _image.OnClickEvent += Clicked;
            _image.sprite = BeatSaberMarkupLanguage.Utilities.LoadSpriteRaw(ms.ToArray());
            _image.sprite.texture.wrapMode = TextureWrapMode.Clamp;
        }

        public void Dispose()
        {
            if (_image != null)
                _image.OnClickEvent -= Clicked;
        }

        private void Clicked(PointerEventData _)
        {
            WasClicked?.Invoke();
        }

        private ClickableImage CreateImage()
        {
            GameObject gameObject = new GameObject("ClickyCherryIcon");
            ClickableImage image = gameObject.AddComponent<ClickableImage>();
            image.material = BeatSaberMarkupLanguage.Utilities.ImageResources.NoGlowMat;

            image.rectTransform.SetParent(_levelSelectionNavigationController.transform);
            image.rectTransform.localPosition = new Vector3(79f, 20f, 0f);
            image.rectTransform.localScale = new Vector3(.3f, .3f, .3f);
            image.rectTransform.sizeDelta = new Vector2(20f, 20f);
            gameObject.AddComponent<LayoutElement>();

            var canvas = gameObject.AddComponent<Canvas>();
            canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord1;
            canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord2;
            canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.Tangent;
            canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.Normal;
            _container.InstantiateComponent<VRGraphicRaycaster>(gameObject);

            return image;
        }
    }
}