using System.Collections.Generic;
using DG.Tweening;
using Static;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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

            privateToggle.onValueChanged.AddListener(OnPrivateToggleChanged);
            sessionNameInput.onValueChanged.AddListener(OnSessionNameInputChanged);
            passwordInput.onValueChanged.AddListener(OnPasswordInputChanged);
            passwordVisible.onValueChanged.AddListener(OnPasswordVisibilityChanged);
            maxPlayersDropdown.onValueChanged.AddListener(OnMaxPlayersDropdownChanged);
            applyButton.onClick.AddListener(OnApplyButtonClick);
            cancelButton.onClick.AddListener(OnCancelButtonClick);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;

            if (_isOpen || _openSequence.IsPlaying()) return;

            _openSequence.Restart();

            Clear();
        }

        private void Clear()
        {
            var session = Manage.Session();

            copyCodeButton.GetComponentInChildren<TMP_Text>().text = session.Code;
            sessionNameInput.placeholder.GetComponent<TMP_Text>().text = session.Name;

            applyButton.interactable = false;
        }

        private void OnCopyCodeButtonClick()
        {
            GUIUtility.systemCopyBuffer = copyCodeButton.GetComponentInChildren<TMP_Text>().text;
        }
        
        private void OnPrivateToggleChanged(bool arg0)
        {
            applyButton.interactable = true;
            _gameSetupController.IsPrivate = arg0;
        }
        
        private void OnSessionNameInputChanged(string arg0)
        {
            applyButton.interactable = true;
            _gameSetupController.SessionName = arg0;
        }
        
        private void OnPasswordInputChanged(string arg0)
        {
            applyButton.interactable = true;
            _gameSetupController.Password = arg0;
        }

        private void OnPasswordVisibilityChanged(bool arg0)
        {
            targetImage.sprite = !arg0 ? visibleOn : visibleOff;
            passwordInput.inputType = !arg0 ? TMP_InputField.InputType.Password : TMP_InputField.InputType.Standard;
            passwordInput.ForceLabelUpdate();
        }
        
        private void OnMaxPlayersDropdownChanged(int arg0)
        {
            applyButton.interactable = true;
            _gameSetupController.MaxPlayers = arg0;
        }

        private void OnApplyButtonClick()
        {
            _gameSetupController.Save();
            
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