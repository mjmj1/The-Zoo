using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode; // NetworkVariable 읽기용

// 좌=Seeker, 우=Hider 고정 HUD
public class ProgressHUD : MonoBehaviour
{
    [Header("Drag & Drop")]
    [SerializeField] private Gameplay.TeamProgressState state; // 씬에 배치된 그 컴포넌트
    [SerializeField] private Image leftGauge;    // Seeker  (Image Type=Filled, Vertical, Origin=Bottom)
    [SerializeField] private Image rightGauge;   // Hider   (Image Type=Filled, Vertical, Origin=Bottom)
    [SerializeField] private TMP_Text leftText;  // "0~100%"
    [SerializeField] private TMP_Text rightText; // "0~100%"

    [SerializeField] private float lerpSpeed = 6f; // 부드럽게 보간 속도

    float _seekerTarget, _hiderTarget;

    void OnEnable()
    {
        if (!state)
        {
            Debug.LogError("[ProgressHUD] TeamProgressState 참조가 비어 있습니다. 인스펙터에서 드래그로 연결하세요.");
            enabled = false;
            return;
        }

        // 네트워크 값 변경에 반응
        state.SeekerProgress.OnValueChanged += OnSeekerChanged;
        state.HiderProgress.OnValueChanged += OnHiderChanged;

        // 초기 동기화
        OnSeekerChanged(0f, state.SeekerProgress.Value);
        OnHiderChanged(0f, state.HiderProgress.Value);

        // 레이캐스트 막지 않게
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
        // 좌(Seeker)
        if (leftGauge)
        {
            leftGauge.fillAmount = Mathf.MoveTowards(leftGauge.fillAmount, _seekerTarget, Time.deltaTime * lerpSpeed);
            if (leftText) leftText.text = Mathf.RoundToInt(leftGauge.fillAmount * 100f) + "%";
        }
        // 우(Hider)
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
