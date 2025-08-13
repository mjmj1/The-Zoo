using System;
using System.Collections.Generic;
using System.Linq;
using Players;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Preferences
{
    public class Preferences : MonoBehaviour
    {
        [Header("Sound")]
        [SerializeField] private Slider bgmSlider; // 0~1
        [SerializeField] private Slider sfxSlider; // 0~1

        [Header("Mouse")]
        [SerializeField] private Slider mouseSensSlider; // Min 0.02, Max 5.0
        [SerializeField] private TMP_Text mouseSensValueText;

        [Header("Display")]
        [SerializeField] private TMP_Dropdown resolutionDropdown;
        [SerializeField] private TMP_Dropdown screenModeDropdown;

        [Header("Actions")]
        [SerializeField] private Button saveButton;
        [SerializeField] private Button cancelButton;

        private List<(int w, int h, RefreshRate rr)> resList;

        private readonly string[] screenModeLabels = { "전용 전체화면", "보더리스 전체화면", "최대화", "창 모드" };
        private readonly FullScreenMode[] screenModes = {
            FullScreenMode.ExclusiveFullScreen,
            FullScreenMode.FullScreenWindow,
            FullScreenMode.MaximizedWindow,
            FullScreenMode.Windowed
        };

        [Serializable]
        private struct State
        {
            public float bgm;
            public float sfx;
            public float mouseSens;
            public int resolutionIndex;
            public int screenModeIndex;
        }

        private State original;

        private void Awake()
        {
            bgmSlider.onValueChanged.AddListener(SetBgm);
            sfxSlider.onValueChanged.AddListener(SetSfx);
            mouseSensSlider.onValueChanged.AddListener(OnMouseSensChanged);

            saveButton.onClick.AddListener(OnSave);
            cancelButton.onClick.AddListener(OnCancel);
        }

        private void Start()
        {
            gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            BuildResolutionList();
            BuildScreenModeList();
            LoadOrCaptureCurrent();
            ApplyUIFromState(original);
        }

        public void OnPreferencesPressed()
        {
            gameObject.SetActive(true);
        }

        #region Build Lists
        private void BuildResolutionList()
        {
            var map = new Dictionary<(int w, int h), RefreshRate>();
            foreach (var r in Screen.resolutions)
            {
                var key = (r.width, r.height);
                if (!map.TryGetValue(key, out var best) || r.refreshRateRatio.value > best.value)
                    map[key] = r.refreshRateRatio;
            }

            resList = map
                .Select(kv => (kv.Key.w, kv.Key.h, kv.Value))
                .OrderBy(r => r.w * r.h)
                .ThenBy(r => r.Value.value)
                .ToList();

            resolutionDropdown.ClearOptions();
            var options = resList.Select(r => $"{r.w} x {r.h} ({Math.Round(r.rr.value)}Hz)").ToList();
            resolutionDropdown.AddOptions(options);
        }

        private void BuildScreenModeList()
        {
            screenModeDropdown.ClearOptions();
            screenModeDropdown.AddOptions(screenModeLabels.ToList());
        }
        #endregion

        #region Load & Save
        private void LoadOrCaptureCurrent()
        {
            var s = new State
            {
                bgm = PlayerPrefs.GetFloat("opt_bgm", 0.75f),
                sfx = PlayerPrefs.GetFloat("opt_sfx", 0.75f),
                mouseSens = PlayerPrefs.GetFloat("opt_mouse_sens", 0.1f),
                resolutionIndex = PlayerPrefs.HasKey("opt_res_idx") ? PlayerPrefs.GetInt("opt_res_idx") : IndexOfCurrentResolution(),
                screenModeIndex = PlayerPrefs.HasKey("opt_scr_idx") ? PlayerPrefs.GetInt("opt_scr_idx") : IndexOfCurrentScreenMode()
            };

            original = s;
        }

        private int IndexOfCurrentResolution()
        {
            var cur = Screen.currentResolution;
            int idx = resList.FindIndex(r => r.w == cur.width && r.h == cur.height);
            return ClampResIndex(idx < 0 ? resList.Count - 1 : idx);
        }

        private int ClampResIndex(int idx) => Mathf.Clamp(idx, 0, Mathf.Max(0, resList.Count - 1));

        private int IndexOfCurrentScreenMode()
        {
            var mode = Screen.fullScreenMode;
            var idx = Array.IndexOf(screenModes, mode);
            return Mathf.Clamp(idx, 0, screenModes.Length - 1);
        }

        private void SaveToPrefs(State s)
        {
            PlayerPrefs.SetFloat("opt_bgm", s.bgm);
            PlayerPrefs.SetFloat("opt_sfx", s.sfx);
            PlayerPrefs.SetFloat("opt_mouse_sens", s.mouseSens);
            PlayerPrefs.SetInt("opt_res_idx", s.resolutionIndex);
            PlayerPrefs.SetInt("opt_scr_idx", s.screenModeIndex);
            PlayerPrefs.Save();
        }
        #endregion

        #region Apply
        private void ApplyUIFromState(State s)
        {
            bgmSlider.SetValueWithoutNotify(s.bgm);
            sfxSlider.SetValueWithoutNotify(s.sfx);
            SetBgm(s.bgm);
            SetSfx(s.sfx);

            mouseSensSlider.SetValueWithoutNotify(Mathf.Clamp(s.mouseSens, 0.02f, 5f));
            UpdateMouseSensLabel(s.mouseSens);
            ApplyMouseSensitivityToLocalPlayer(s.mouseSens);

            resolutionDropdown.SetValueWithoutNotify(ClampResIndex(s.resolutionIndex));
            screenModeDropdown.SetValueWithoutNotify(Mathf.Clamp(s.screenModeIndex, 0, screenModes.Length - 1));
        }

        private void ApplyDisplay(int resIndex, int screenModeIndex)
        {
            var r = resList[ClampResIndex(resIndex)];
            var mode = screenModes[Mathf.Clamp(screenModeIndex, 0, screenModes.Length - 1)];
            Screen.SetResolution(r.w, r.h, mode, r.rr);
        }

        private void SetBgm(float v)
        {
            AudioManager.Instance.SetBGMVolume(bgmSlider.value);
        }

        private void SetSfx(float v)
        {
            AudioManager.Instance.SetSfxVolume(sfxSlider.value);
        }

        private void OnMouseSensChanged(float v)
        {
            UpdateMouseSensLabel(v);
            ApplyMouseSensitivityToLocalPlayer(v);
        }

        private void UpdateMouseSensLabel(float v)
        {
            if (mouseSensValueText != null)
                mouseSensValueText.text = $"{v:0.00}";
        }

        private void ApplyMouseSensitivityToLocalPlayer(float v)
        {
            if (NetworkManager.Singleton == null || NetworkManager.Singleton.LocalClient == null) return;

            var playerObj = NetworkManager.Singleton.LocalClient.PlayerObject;
            if (playerObj == null) return;

            var controller = playerObj.GetComponent<PlayerController>();
            if (controller != null)
                controller.ApplyMouseSensitivity(v);
        }
        #endregion

        #region Buttons
        private void OnSave()
        {
            var s = new State
            {
                bgm = bgmSlider.value,
                sfx = sfxSlider.value,
                mouseSens = mouseSensSlider.value,
                resolutionIndex = resolutionDropdown.value,
                screenModeIndex = screenModeDropdown.value
            };

            ApplyDisplay(s.resolutionIndex, s.screenModeIndex);
            SetBgm(s.bgm);
            SetSfx(s.sfx);
            ApplyMouseSensitivityToLocalPlayer(s.mouseSens);

            SaveToPrefs(s);
            original = s;

            gameObject.SetActive(false);
        }

        private void OnCancel()
        {
            ApplyUIFromState(original);
            ApplyDisplay(original.resolutionIndex, original.screenModeIndex);
            gameObject.SetActive(false);
        }
        #endregion
    }
}
