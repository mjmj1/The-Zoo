using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Preferences
{
    public class ResolutionPreference : MonoBehaviour
    {
        private struct ResolutionData
        {
            public Resolution Resolution;
            private readonly (int width, int height) _aspectRatio;

            public ResolutionData(Resolution resolution, (int width, int height) aspectRatio)
            {
                this.Resolution = resolution;
                _aspectRatio = aspectRatio;
            }

            public override string ToString()
            {
                return $"{Resolution.width} x {Resolution.height}({_aspectRatio.width}:{_aspectRatio.height})";
            }
        }
        
        [SerializeField] private TMP_Dropdown resolutionDropdown;
        [SerializeField] private Toggle fullScreenToggle;
        [SerializeField] private Button resolutionSaveButton;

        private readonly List<ResolutionData> resolutions = new ();
        private int _currentResolutionIndex;
        
        void Start()
        {
            InitializeResolutionList();
            UpdateResolutionDropdown();
            
            resolutionSaveButton.onClick.AddListener(OnResolutionSaveButtonClick);
            resolutionDropdown.onValueChanged.AddListener(OnDropboxOptionChanged);
        }
        
        void InitializeResolutionList()
        {
            foreach (var t in Screen.resolutions)
            {
                if (t.width < 1280) continue;
                
                if (!Mathf.Approximately((float)t.refreshRateRatio.value, 60)) continue;
                
                if (t.width * 9 == t.height * 16)
                {
                    ResolutionData data = new(t, (16, 9));
                    resolutions.Add(data);
                }
                else if (t.width * 10 == t.height * 16)
                {
                    ResolutionData data = new(t, (16, 10));
                    resolutions.Add(data);
                }
            }
        }

        void UpdateResolutionDropdown()
        {
            resolutionDropdown.ClearOptions();

            var currentIndex = 0;
            
            foreach (var item in resolutions)
            {
                var option = new TMP_Dropdown.OptionData
                {
                    text = item.ToString()
                };

                resolutionDropdown.options.Add(option);
                
                if (item.Resolution.width == Screen.width && item.Resolution.height == Screen.height)
                {
                    resolutionDropdown.value = currentIndex;
                }
                
                currentIndex++;
            }
            
            resolutionDropdown.RefreshShownValue();
        }
        
        void OnDropboxOptionChanged(int x)
        {
            _currentResolutionIndex = x;
        }
        
        void OnResolutionSaveButtonClick()
        {
            Screen.SetResolution(resolutions[_currentResolutionIndex].Resolution.width, resolutions[_currentResolutionIndex].Resolution.height, fullScreenToggle.isOn);
            TitleUIManager.OpenInformationPopup("해상도 변경이 완료되었습니다.");
        }
        
    }
}
