using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Cherry.UI
{
    internal class RequestPanelView : MonoBehaviour
    {
        [UIComponent("play-button")]
        protected readonly NoTransitionsButton _playButton = null!;

        [UIComponent("queue-button")]
        protected readonly Button _queueButton = null!;

        [UIComponent("history-button")]
        protected readonly Button _historyButton = null!;

        [UIComponent("skip-button")]
        protected readonly Button _skipButton = null!;

        private ImageView _queueButtonUnderline = null!;
        private CurvedTextMeshPro _queueButtonText = null!;

        private Color? _activeColor;
        private ImageView _playButtonBorder = null!;
        private ImageView _playButtonOutline = null!;
        private ImageView _playButtonBackground = null!;
        private CurvedTextMeshPro _playButtonText = null!;
        private CurvedTextMeshPro _historyButtonText = null!;
        private Color _defaultPlayButtonBorderColor = Color.blue;
        private Color _defaultPlayButtonOutlineColor = Color.blue;
        private Color _defaultPlayButtonBGColorLeftTop = Color.blue;
        private Color _defaultPlayButtonBGColorLeftBottom = Color.blue;

        public event Action? PlayButtonClicked;
        public event Action? SkipButtonClicked;
        public event Action? QueueButtonClicked;
        public event Action? HistoryButtonClicked;

        [UIAction("#post-parse")]
        protected void Parsed()
        {
            _playButton.SetSkew(0f);
            _queueButton.SetSkew(0f);
            _historyButton.SetSkew(0f);
            _skipButton.SetSkew(0f);

            _queueButtonText = _queueButton.GetComponentInChildren<CurvedTextMeshPro>();
            _queueButtonUnderline = _queueButton.transform.Find("Underline").GetComponent<ImageView>();

            _playButtonText = _playButton.GetComponentInChildren<CurvedTextMeshPro>();
            _historyButtonText = _historyButton.GetComponentInChildren<CurvedTextMeshPro>();
            _playButtonBorder = _playButton.transform.Find("Border").GetComponent<ImageView>();
            _playButtonBackground = _playButton.transform.Find("BG").GetComponent<ImageView>();
            _playButtonOutline = _playButton.transform.Find("OutlineWrapper/Outline").GetComponent<ImageView>();

            _defaultPlayButtonBorderColor = _playButtonBorder.color;
            _defaultPlayButtonOutlineColor = _playButtonOutline.color;
            _defaultPlayButtonBGColorLeftTop = Utilities.ImageViewColor0(ref _playButtonBackground);
            _defaultPlayButtonBGColorLeftBottom = Utilities.ImageViewColor1(ref _playButtonBackground);

            _playButton.selectionStateDidChangeEvent += SelectionChanged;
        }

        private void SelectionChanged(NoTransitionsButton.SelectionState selection)
        {
            if (_activeColor != null)
            {
                if (selection == NoTransitionsButton.SelectionState.Normal)
                {
                    SetPlayButtonInternal(_activeColor.Value);
                }
                else if (selection == NoTransitionsButton.SelectionState.Highlighted)
                {
                    var color = _activeColor.Value;
                    SetPlayButtonInternal(_activeColor.Value.ColorWithAlpha(color.a * 0.65f));
                }
            }
        }

        protected void OnDestroy()
        {
            if (_playButton != null)
                _playButton.selectionStateDidChangeEvent -= SelectionChanged;
        }

        [UIAction("play-button-clicked")] protected void PBC() => PlayButtonClicked?.Invoke();
        [UIAction("skip-button-clicked")] protected void SBC() => SkipButtonClicked?.Invoke();
        [UIAction("queue-button-clicked")] protected void QBC() => QueueButtonClicked?.Invoke();
        [UIAction("history-button-clicked")] protected void HBC() => HistoryButtonClicked?.Invoke();

        public void SetPlayButtonText(string text)
        {
            _playButtonText.text = text;
        }

        public void SetPlayButtonInteractability(bool interactability)
        {
            _playButton.interactable = interactability;
        }

        public void SetPlayButtonColor(Color? color)
        {
            if (!color.HasValue)
            {
                _activeColor = null;
                _playButtonBorder.color = _defaultPlayButtonBorderColor;
                _playButtonOutline.color = _defaultPlayButtonOutlineColor;
                _playButtonBackground.color0 = _defaultPlayButtonBGColorLeftTop;
                _playButtonBackground.color1 = _defaultPlayButtonBGColorLeftBottom;
                _playButton.GetComponent<ButtonStaticAnimations>().enabled = true;
            }
            else
            {
                SetPlayButtonInteractability(true);
                _playButton.GetComponent<ButtonStaticAnimations>().enabled = false;
                _activeColor = color.Value;
                var activeColor = color.Value;
                StartCoroutine(SetPlayButtonInternal(activeColor));
            }
            _playButtonBackground.SetVerticesDirty();
            _playButtonOutline.SetVerticesDirty();
            _playButtonBorder.SetVerticesDirty();
        }

        private IEnumerator SetPlayButtonInternal(Color activeColor)
        {
            yield return new WaitForEndOfFrame();
            _playButtonBackground.color0 = activeColor;
            _playButtonBorder.color = activeColor.ColorWithAlpha(0.65f * activeColor.a);
            _playButtonOutline.color = activeColor.ColorWithAlpha(0.25f * activeColor.a);
            _playButtonBackground.color1 = new Color(activeColor.r * 0.5f, activeColor.g * 0.5f, activeColor.b * 0.5f, activeColor.a);
        }

        public void SetQueueButtonText(string text)
        {
            _queueButtonText.text = text;
        }

        public void SetQueueButtonColor(Color color)
        {
            _queueButtonUnderline.color = color;
            _queueButtonUnderline.SetVerticesDirty();
        }

        public void SetSkipButtonInteractability(bool interactability)
        {
            _skipButton.interactable = interactability;
        }

        public void SetHistoryButtonText(string text)
        {
            _historyButtonText.text = text;
        }
    }
}