using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using IPA.Utilities;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Cherry.UI
{
    internal class RequestDetailView
    {
        internal static readonly FieldAccessor<ImageView, float>.Accessor ImageSkew = FieldAccessor<ImageView, float>.GetAccessor("_skew");

        [UIComponent("cover-image")]
        protected readonly Image _coverImage = null!;

        [UIComponent("requester-button")]
        protected readonly Button _requesterButton = null!;

        [UIComponent("suggestions-button")]
        protected readonly Button _suggestionsButton = null!;

        [UIComponent("title-text")]
        protected readonly CurvedTextMeshPro _titleText = null!;

        [UIComponent("uploader-text")]
        protected readonly CurvedTextMeshPro _uploaderText = null!;

        [UIComponent("requester-text")]
        protected readonly CurvedTextMeshPro _requesterText = null!;

        [UIComponent("rating-text")]
        protected readonly CurvedTextMeshPro _ratingText = null!;

        [UIComponent("time-text")]
        protected readonly CurvedTextMeshPro _timeText = null!;

        [UIComponent("content-root")]
        protected readonly RectTransform _contentRoot = null!;

        [UIComponent("loading-root")]
        protected readonly RectTransform _loadingRoot = null!;

        public event Action? RequesterButtonClicked;

        [UIAction("requester-button-clicked")] protected void RBC() => RequesterButtonClicked?.Invoke();

        [UIAction("#post-parse")]
        protected void Parsed()
        {
            _coverImage.material = Utilities.UINoGlowRoundEdge;
            _requesterButton.SetSkew(0f);
            _suggestionsButton.SetSkew(0f);

            _titleText.fontSizeMin = 3f;
            _titleText.fontSizeMax = 4.5f;

            _uploaderText.fontSizeMin = 1.5f;
            _uploaderText.fontSizeMax = 3f;

            _requesterText.fontSizeMin = 1.5f;
            _requesterText.fontSizeMax = 3f;

            _titleText.enableAutoSizing = true;
            _uploaderText.enableAutoSizing = true;
            _requesterText.enableAutoSizing = true;
        }

        public void SetData(string songName, string uploaderName, string requesterName, Sprite imageCover, float rating, DateTime time, string? suggestions = null)
        {
            _contentRoot.gameObject.SetActive(true);
            _loadingRoot.gameObject.SetActive(false);

            _titleText.text = songName;
            _uploaderText.text = uploaderName;
            _requesterText.text = $"requested by <color=#919191>{requesterName}</color>";
            _coverImage.sprite = imageCover;

            _ratingText.text = string.Format("{0:0%}", rating);
            _ratingText.color = Utilities.Evaluate(rating);
            _timeText.text = time.ToString("h:mm tt");

            _suggestionsButton.interactable = suggestions != null;
        }

        public void SetLoading()
        {
            _contentRoot.gameObject.SetActive(false);
            _loadingRoot.gameObject.SetActive(true);
        }
    }
}