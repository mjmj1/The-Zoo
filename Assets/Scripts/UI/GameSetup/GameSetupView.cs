using System;
using System.Collections.Generic;
using DG.Tweening;
using Networks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils;

namespace UI.GameSetup
{
    public class GameSetupView : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private List<RectTransform> contents = new();

        [Header("IsPrivate")]
        [SerializeField] private Toggle privateToggle;

        [Header("Code")]
        [SerializeField] private Button codeCopyButton;
        [SerializeField] private TMP_Text codeCopyText;

        [Header("Session Name")] 
        [SerializeField] private TMP_InputField sessionNameInput;
        [SerializeField] private TMP_Text sessionNamePlaceholder;

        [Header("Password")] 
        [SerializeField] private Sprite visibleOn;
        [SerializeField] private Sprite visibleOff;
        [SerializeField] private Image targetImage;
        [SerializeField] private Toggle passwordVisible;
        [SerializeField] private TMP_InputField passwordInput;

        [Header("Dropdowns")] 
        [SerializeField] private TMP_Dropdown playerSlotDropdown;
        [SerializeField] private TMP_Dropdown aiLevelDropdown;
        [SerializeField] private TMP_Dropdown npcPopulationDropdown;

        [Header("Buttons")] [SerializeField] private Button applyButton;
        [SerializeField] private Button cancelButton;

        private const float Duration = 0.3f;

        private GameSetupController _controller;

        private Sequence _closeSequence;
        private Sequence _openSequence;

        private Vector2 _closeSize;
        private Vector2 _openSize;
        private RectTransform _rectTransform;

        private bool _isOpen;
        private float _stepDuration;

        private void Start()
        {
            _controller = GetComponent<GameSetupController>();
            _rectTransform = GetComponent<RectTransform>();

            _openSize = _rectTransform.sizeDelta;
            _closeSize = new Vector2(_openSize.x, 15f);

            _rectTransform.sizeDelta = _closeSize;

            foreach (var child in contents)
            {
                child.localScale = Vector3.zero;
                child.gameObject.SetActive(false);

                var cg = child.GetComponent<CanvasGroup>();
                if (!cg) child.gameObject.AddComponent<CanvasGroup>().alpha = 0;
                else cg.alpha = 0;
            }

            _stepDuration = Duration / contents.Count;

            _openSequence = DOTween.Sequence().Pause().SetAutoKill(false).SetRecyclable(true);
            _closeSequence = DOTween.Sequence().Pause().SetAutoKill(false).SetRecyclable(true);

            SetupOpenSequence();
            SetupCloseSequence();

            Register();
            
            _controller.Initialize();
            
            Initialize();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.N))
            {
                _controller.Print();    
            }
        }

        private void Initialize()
        {
            codeCopyText.text = _controller.JoinCode;
            
            sessionNameInput.text = string.Empty;
            sessionNamePlaceholder.text = _controller.SessionName.Original;

            privateToggle.isOn = _controller.IsPrivate.Original;

            playerSlotDropdown.value = _controller.PlayerSlot.Original - 4;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;

            if (_isOpen || _openSequence.IsPlaying()) return;

            Clear();

            _openSequence.Restart();
        }

        private void Register()
        {
            privateToggle.onValueChanged.AddListener(OnPrivateToggled);
            sessionNameInput.onValueChanged.AddListener(OnSessionNameChanged);
            passwordInput.onValueChanged.AddListener(OnPasswordChanged);
            playerSlotDropdown.onValueChanged.AddListener(OnPlayerSlotChanged);
            aiLevelDropdown.onValueChanged.AddListener(OnAILevelChanged);
            npcPopulationDropdown.onValueChanged.AddListener(OnNPCPopulationChanged);
            
            codeCopyButton.onClick.AddListener(OnCopyCodeButtonClick);
            passwordVisible.onValueChanged.AddListener(OnPasswordVisibleChanged);

            applyButton.onClick.AddListener(OnApplyButtonClick);
            cancelButton.onClick.AddListener(OnCancelButtonClick);
        }

        private void TrackChange<T>(T value, GameOptionField<T> optionField)
        {
            optionField.Current = value;
            
            applyButton.interactable = optionField.IsDirty;
        }
        
        private void OnPrivateToggled(bool arg0)
        {
            TrackChange(arg0, _controller.IsPrivate);
        }
        
        private void OnSessionNameChanged(string arg0)
        {
            TrackChange(arg0, _controller.SessionName);
        }
        
        private void OnPasswordChanged(string arg0)
        {
            TrackChange(arg0, _controller.Password);
        }

        private void OnPlayerSlotChanged(int arg0)
        {
            TrackChange(arg0 + 4, _controller.PlayerSlot);
        }
        
        private void OnAILevelChanged(int arg0)
        {
            TrackChange(arg0, null);
        }

        private void OnNPCPopulationChanged(int arg0)
        {
            TrackChange(arg0, null);
        }

        private void Clear()
        {
            applyButton.interactable = false;
            
            _controller.Reset();
            
            Initialize();
        }

        private void OnCopyCodeButtonClick()
        {
            GUIUtility.systemCopyBuffer = codeCopyButton.GetComponentInChildren<TMP_Text>().text;
        }

        private void OnPasswordVisibleChanged(bool value)
        {
            targetImage.sprite = !value ? visibleOn : visibleOff;
            passwordInput.inputType = !value ? TMP_InputField.InputType.Password : TMP_InputField.InputType.Standard;
            passwordInput.ForceLabelUpdate();
        }

        private void OnApplyButtonClick()
        {
            _controller.Save();

            if (!_isOpen || (_closeSequence?.IsPlaying() ?? false)) return;

            _closeSequence.Restart();
        }

        private void OnCancelButtonClick()
        {
            if (!_isOpen || (_closeSequence?.IsPlaying() ?? false)) return;

            _closeSequence.Restart();
        }

        private void SetupOpenSequence()
        {
            _openSequence.Insert(0f, _rectTransform.DOSizeDelta(_openSize, Duration));

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
            _closeSequence.Insert(0.15f, _rectTransform.DOSizeDelta(_closeSize, Duration));

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