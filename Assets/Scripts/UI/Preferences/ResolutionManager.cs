using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI.Preferences
{
    public class ResolutionManager : MonoBehaviour
    {
        private struct ResolutionData
        {
            public Resolution Resolution;
            public readonly (int width, int height) AspectRatio;

            public ResolutionData(Resolution resolution, (int width, int height) aspectRatio)
            {
                this.Resolution = resolution;
                AspectRatio = aspectRatio;
            }

            public override string ToString()
            {
                return $"{Resolution.width} x {Resolution.height}({AspectRatio.width}:{AspectRatio.height})";
            }
        }
        
        [SerializeField] private TMP_Dropdown resolutionDropdown;
        [SerializeField] private Toggle fullScreenToggle;
        [SerializeField] private Button resolutionButton;

        private readonly List<ResolutionData> _resolutionList = new ();
        private int _currentResolutionIndex;
        
        void InitResolutionList()
        {
            foreach (var t in Screen.resolutions)
            {
                if (t.width < 1280) continue;
                
                if (!Mathf.Approximately((float)t.refreshRateRatio.value, 60)) continue;
                
                if (t.width * 9 == t.height * 16)
                {
                    ResolutionData data = new(t, (16, 9));
                    _resolutionList.Add(data);
                }
                else if (t.width * 10 == t.height * 16)
                {
                    ResolutionData data = new(t, (16, 10));
                    _resolutionList.Add(data);
                }
            }
        }

        void SetUpResolutionDropdown()
        {
            resolutionDropdown.ClearOptions();

            var optionNum = 0;
            
            foreach (var item in _resolutionList)
            {
                var option = new TMP_Dropdown.OptionData
                {
                    text = item.ToString()
                };

                resolutionDropdown.options.Add(option);
                
                if (item.Resolution.width == Screen.width && item.Resolution.height == Screen.height)
                {
                    resolutionDropdown.value = optionNum;
                }
                
                optionNum++;
            }
            
            resolutionDropdown.RefreshShownValue();
        }
        
        private void DropboxOptionChanged(int x)
        {
            _currentResolutionIndex = x;
        }
        
        private void ResolutionBtnClick()
        {
            Screen.SetResolution(_resolutionList[_currentResolutionIndex].Resolution.width, _resolutionList[_currentResolutionIndex].Resolution.height, fullScreenToggle.isOn);
        }
        
        void Start()
        {
            InitResolutionList();
            SetUpResolutionDropdown();
            
            resolutionButton.onClick.AddListener(ResolutionBtnClick);
            resolutionDropdown.onValueChanged.AddListener(DropboxOptionChanged);
        }
    }
}
