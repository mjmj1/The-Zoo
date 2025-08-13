using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProgressHUD : MonoBehaviour
{
    [Header("Drag & Drop")]
    [SerializeField] private ProgressState.TeamProgressState state;
    [SerializeField] private Image leftGauge;    // Seeker  
    [SerializeField] private Image rightGauge;   // Hider   
    [SerializeField] private TMP_Text leftText;  // "0~100%"
    [SerializeField] private TMP_Text rightText; // "0~100%"

    [SerializeField] private float lerpSpeed = 6f; 

    float _seekerTarget, _hiderTarget;

    void OnEnable()
    {
        if (!state)
        {
            Debug.LogError("[ProgressHUD] TeamProgressState 참조가 비어 있습니다. 인스펙터에서 드래그로 연결하세요.");
            enabled = false;
            return;
        }

        state.SeekerProgress.OnValueChanged += OnSeekerChanged;
        state.HiderProgress.OnValueChanged += OnHiderChanged;

        OnSeekerChanged(0f, state.SeekerProgress.Value);
        OnHiderChanged(0f, state.HiderProgress.Value);

        if (leftGauge) leftGauge.raycastTarget = false;
        if (rightGauge) rightGauge.raycastTarget = false;
    }

    void OnDisable()
    {
        if (!state) return;
        state.SeekerProgress.OnValueChanged -= OnSeekerChanged;
        state.HiderProgress.OnValueChanged -= OnHiderChanged;
    }

    void Update()
    {
        // (Seeker)
        if (leftGauge)
        {
            leftGauge.fillAmount = Mathf.MoveTowards(leftGauge.fillAmount, _seekerTarget, Time.deltaTime * lerpSpeed);
            if (leftText) leftText.text = Mathf.RoundToInt(leftGauge.fillAmount * 100f) + "%";
        }
        // (Hider)
        if (rightGauge)
        {
            rightGauge.fillAmount = Mathf.MoveTowards(rightGauge.fillAmount, _hiderTarget, Time.deltaTime * lerpSpeed);
            if (rightText) rightText.text = Mathf.RoundToInt(rightGauge.fillAmount * 100f) + "%";
        }
    }

    void OnSeekerChanged(float _, float now)
    {
        _seekerTarget = Mathf.Clamp01(now); // 0~1
    }

    void OnHiderChanged(float _, float now)
    {
        _hiderTarget = Mathf.Clamp01(now);  // 0~1
    }
}
