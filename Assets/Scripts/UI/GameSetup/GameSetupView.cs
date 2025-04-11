using System;
using System.Collections.Generic;
using DG.Tweening;
using Static;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Static.Strings;

namespace UI.GameSetup
{
    public class GameSetupView : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private List<RectTransform> contents = new();

        [Header("Input Fields")]
        [SerializeField] private Button copyCodeButton;
        [SerializeField] private Toggle privateToggle;
        [SerializeField] private TMP_InputField sessionNameInput;

        [Header("Password")]
        [SerializeField] private Sprite visibleOn;
        [SerializeField] private Sprite visibleOff;
        [SerializeField] private Image targetImage;
        [SerializeField] private TMP_InputField passwordInput;
        [SerializeField] private Toggle passwordVisible;

        [Header("Dropdowns")]
        [SerializeField] private TMP_Dropdown maxPlayersDropdown;
        [SerializeField] private TMP_Dropdown aiLevelDropdown;
        [SerializeField] private TMP_Dropdown npcPopulationDropdown;

        [Header("Buttons")]
        [SerializeField] private Button applyButton;
        [SerializeField] private Button cancelButton;

        private readonly float _duration = 0.3f;
        private Sequence _closeSequence;

        private Vector2 _closeSize;

        private GameSetupController _gameSetupController;

        private bool _isOpen;

        private Sequence _openSequence;
        private Vector2 _openSize;
        private RectTransform _rectTransform;

        private float _stepDuration;

        private void Start()
        {
            _gameSetupController = GetComponent<GameSetupController>();
            _rectTransform = GetComponent<RectTransform>();

            _openSize = _rectTransform.sizeDelta;
            _closeSize = new Vector2(200f, 25f);

            _rectTransform.sizeDelta = _closeSize;

            foreach (var child in contents)
            {
                child.localScale = Vector3.zero;
                child.gameObject.SetActive(false);

                var cg = child.GetComponent<CanvasGroup>();
                if (cg == null) child.gameObject.AddComponent<CanvasGroup>().alpha = 0;
                else cg.alpha = 0;
            }

            _stepDuration = _duration / contents.Count;

            _openSequence = DOTween.Sequence().Pause().SetAutoKill(false).SetRecyclable(true);
            _closeSequence = DOTween.Sequence().Pause().SetAutoKill(false).SetRecyclable(true);

            SetupOpenSequence();
            SetupCloseSequence();

            copyCodeButton.onClick.AddListener(OnCopyCodeButtonClick);

            sessionNameInput.onValueChanged.AddListener(OnSessionNameInputChanged);
            passwordInput.onValueChanged.AddListener(OnPasswordInputChanged);

            privateToggle.onValueChanged.AddListener(OnPrivateToggleChanged);
            passwordVisible.onValueChanged.AddListener(OnPasswordVisibilityChanged);
            maxPlayersDropdown.onValueChanged.AddListener(OnMaxPlayersDropdownChanged);

            applyButton.onClick.AddListener(OnApplyButtonClick);
            cancelButton.onClick.AddListener(OnCancelButtonClick);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;

            if (_isOpen || _openSequence.IsPlaying()) return;
            
            Clear();

            _openSequence.Restart();
        }

        private void Clear()
        {
            privateToggle.isOn = _gameSetupController.IsPrivate.Origin;

            sessionNameInput.text = null;
            sessionNameInput.placeholder.GetComponent<TMP_Text>().text = _gameSetupController.SessionName.Origin;
            
            copyCodeButton.GetComponentInChildren<TMP_Text>().text = _gameSetupController.Code;
            
            passwordInput.text = _gameSetupController.Password.Origin;
            
            maxPlayersDropdown.value = _gameSetupController.PlayerSlot.Origin;
            
            applyButton.interactable = false;
            
            _gameSetupController.Clear();
        }

        private void OnCopyCodeButtonClick()
        {
            GUIUtility.systemCopyBuffer = copyCodeButton.GetComponentInChildren<TMP_Text>().text;
        }

        private void OnPasswordVisibilityChanged(bool value)
        {
            targetImage.sprite = !value ? visibleOn : visibleOff;
            passwordInput.inputType = !value ? TMP_InputField.InputType.Password : TMP_InputField.InputType.Standard;
            passwordInput.ForceLabelUpdate();
        }

        private void TrackChange<T>(T value, SetupData<T> data)
        {
            applyButton.interactable = _gameSetupController.HasDirty;
            data.Set(value);
        }

        private void OnPrivateToggleChanged(bool value)
        {
            TrackChange(value, _gameSetupController.IsPrivate);
        }

        private void OnSessionNameInputChanged(string value)
        {
            TrackChange(value, _gameSetupController.SessionName);
        }

        private void OnPasswordInputChanged(string value)
        {
            TrackChange(value, _gameSetupController.Password);
        }

        private void OnMaxPlayersDropdownChanged(int value)
        {
            TrackChange(value, _gameSetupController.PlayerSlot);
        }

        private void OnApplyButtonClick()
        {
            _gameSetupController.Save();

            if (!_isOpen || (_closeSequence?.IsPlaying() ?? false)) return;

            _closeSequence.Restart();
        }

        private void OnCancelButtonClick()
        {
            _gameSetupController.Clear();

            if (!_isOpen || (_closeSequence?.IsPlaying() ?? false)) return;

            _closeSequence.Restart();
        }

        private void SetupOpenSequence()
        {
            _openSequence.Insert(0f, _rectTransform.DOSizeDelta(_openSize, _duration));

            for (var i = 0; i < contents.Count; i++)
            {
                var child = contents[i];
                var delay = i * _stepDuration;

                var cg = child.GetComponent<CanvasGroup>();

                _openSequence.Insert(delay, DOTween.Sequence()
                    .AppendCallback(() => child.gameObject.SetActive(true))
                    .Join(cg.DOFade(1, 0.2f))
                    .Join(child.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack))
                );
            }

            _openSequence.OnComplete(() => _isOpen = true);
        }

        private void SetupCloseSequence()
        {
            _closeSequence.Insert(0.15f, _rectTransform.DOSizeDelta(_closeSize, _duration));

            for (var i = contents.Count - 1; i >= 0; i--)
            {
                var child = contents[i];
                var cg = child.GetComponent<CanvasGroup>();

                var delay = (contents.Count - 1 - i) * _stepDuration;

                _closeSequence.Insert(delay, DOTween.Sequence()
                    .Join(cg.DOFade(0f, 0.15f))
                    .Join(child.DOScale(Vector3.zero, 0.15f).SetEase(Ease.InBack))
                    .AppendCallback(() => child.gameObject.SetActive(false))
                );
            }

            _closeSequence.OnComplete(() => _isOpen = false);
        }
    }
}