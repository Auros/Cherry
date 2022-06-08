using BeatSaberMarkupLanguage.Components;
using Cherry.Interfaces;
using Cherry.Models;
using IPA.Loader;
using SiraUtil.Logging;
using SiraUtil.Zenject;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VRUIControls;
using Zenject;

namespace Cherry.UI
{
    internal class ButtonManager : IInitializable, IDisposable, ITickable
    {
        private bool _lastQueueValue;
        private ClickableImage? _image;
        public event Action? WasClicked;
        private readonly Config _config;
        private readonly Assembly _assembly;
        private readonly DiContainer _container;
        private readonly IRequestHistory _requestHistory;
        private readonly IRequestManager _requestManager;
        private readonly TweeningManager _tweeningManager;
        private readonly LevelSelectionNavigationController _levelSelectionNavigationController;
        private static readonly Color _emptyColor = new Color(0.15f, 0f, 0f, 1f);

        private bool _flickerState;
        private float _flickerCycleTime;
        private const float _flickerCycleSpeed = 0.45f;

        public bool DisableAnimation => _config.BlinkCherryForUnseenRequests && _requestManager.HasNewRequests;

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

                    if (DisableAnimation)
                    {
                        _image.color = value.Value;
                    }
                    else
                    {
                        Color oldColor = _image.color;
                        _tweeningManager.KillAllTweens(_image);
                        var tween = new FloatTween(0f, 1f, val => _image!.color = Color.Lerp(oldColor, value.Value, val), 1f, EaseType.InOutSine);
                        _tweeningManager.AddTween(tween, _image);
                        tween.onCompleted = delegate () { _image!.DefaultColor = value.Value; };
                    }
                }
            }
        }

        public ButtonManager(Config config, DiContainer container, UBinder<Plugin, PluginMetadata> metadataBinder, IRequestHistory requestHistory, IRequestManager requestManager, TimeTweeningManager tweeningManager, LevelSelectionNavigationController levelSelectionNavigationController)
        {
            _config = config;
            _container = container;
            _requestHistory = requestHistory;
            _requestManager = requestManager;
            _tweeningManager = tweeningManager;
            _lastQueueValue = _config.QueueOpened;
            _assembly = metadataBinder.Value.Assembly;
            _levelSelectionNavigationController = levelSelectionNavigationController;
        }

        public void Initialize()
        {
            _ = InitializeAsync();
            _config.Updated += Config_Updated;
            _requestManager.SongSkipped += RequestManager_SongStateChange;
            _requestManager.SongAccepted += RequestManager_SongStateChange;
            _requestManager.SongRequested += RequestManager_SongStateChange;
        }

        private void Config_Updated(Config config)
        {
            try
            {
                if (_lastQueueValue == config.QueueOpened)
                    return;
                _lastQueueValue = config.QueueOpened;
                RequestManager_SongStateChange(null!, null!);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private async void RequestManager_SongStateChange(object _, RequestEventArgs __)
        {
            try
            {
                if (!_config.QueueOpened)
                {
                    DefaultColor = _emptyColor;
                    return;
                }
                var history = await _requestHistory.History();
                DefaultColor = history.Any(r => !r.WasPlayed && r.RequestTime >= DateTime.Now.AddHours(-_config.SesssionLengthInHours)) ? Color.white : _emptyColor;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
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
            RequestManager_SongStateChange(null!, null!);


            var history = await _requestHistory.History();
            _requestManager.HasNewRequests = history.Any(r => !r.WasPlayed && r.RequestTime >= DateTime.Now.AddHours(-_config.SesssionLengthInHours));
        }

        public void Dispose()
        {
            _requestManager.SongRequested -= RequestManager_SongStateChange;
            _requestManager.SongAccepted -= RequestManager_SongStateChange;
            _requestManager.SongSkipped -= RequestManager_SongStateChange;
            _config.Updated -= Config_Updated;
            if (_image != null)
            {
                _image.OnClickEvent -= Clicked;
                UnityEngine.Object.Destroy(_image.sprite);
                UnityEngine.Object.Destroy(_image.sprite.texture);
            }
        }

        private void Clicked(PointerEventData _)
        {
            WasClicked?.Invoke();
        }

        private ClickableImage CreateImage()
        {
            GameObject gameObject = new GameObject("Cherry Icon");
            ClickableImage image = gameObject.AddComponent<ClickableImage>();
            image.material = BeatSaberMarkupLanguage.Utilities.ImageResources.NoGlowMat;

            image.rectTransform.SetParent(_levelSelectionNavigationController.transform);
            image.rectTransform.localPosition = new Vector3(75f, 25f, 0f);
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

        public void Tick()
        {
            if (!_config.BlinkCherryForUnseenRequests || !_requestManager.HasNewRequests)
                return;

            _flickerCycleTime += Time.deltaTime;
            if (_flickerCycleSpeed > _flickerCycleTime)
                return;

            _flickerCycleTime = 0;
            DefaultColor = _flickerState ? Color.white : _emptyColor;
            _flickerState = !_flickerState;
        }
    }
}