using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode; // NetworkVariable �б��

// ��=Seeker, ��=Hider ���� HUD
public class ProgressHUD : MonoBehaviour
{
    [Header("Drag & Drop")]
    [SerializeField] private Gameplay.TeamProgressState state; // ���� ��ġ�� �� ������Ʈ
    [SerializeField] private Image leftGauge;    // Seeker  (Image Type=Filled, Vertical, Origin=Bottom)
    [SerializeField] private Image rightGauge;   // Hider   (Image Type=Filled, Vertical, Origin=Bottom)
    [SerializeField] private TMP_Text leftText;  // "0~100%"
    [SerializeField] private TMP_Text rightText; // "0~100%"

    [SerializeField] private float lerpSpeed = 6f; // �ε巴�� ���� �ӵ�

    float _seekerTarget, _hiderTarget;

    void OnEnable()
    {
        if (!state)
        {
            Debug.LogError("[ProgressHUD] TeamProgressState ������ ��� �ֽ��ϴ�. �ν����Ϳ��� �巡�׷� �����ϼ���.");
            enabled = false;
            return;
        }

        // ��Ʈ��ũ �� ���濡 ����
        state.SeekerProgress.OnValueChanged += OnSeekerChanged;
        state.HiderProgress.OnValueChanged += OnHiderChanged;

        // �ʱ� ����ȭ
        OnSeekerChanged(0f, state.SeekerProgress.Value);
        OnHiderChanged(0f, state.HiderProgress.Value);

        // ����ĳ��Ʈ ���� �ʰ�
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
        // ��(Seeker)
        if (leftGauge)
        {
            leftGauge.fillAmount = Mathf.MoveTowards(leftGauge.fillAmount, _seekerTarget, Time.deltaTime * lerpSpeed);
            if (leftText) leftText.text = Mathf.RoundToInt(leftGauge.fillAmount * 100f) + "%";
        }
        // ��(Hider)
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
