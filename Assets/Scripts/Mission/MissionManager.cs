using Gameplay;
using GamePlay;
using Interactions;
using Players;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Mission
{
    public class MissionManager : MonoBehaviour
    {
        public static MissionManager instance;

        [SerializeField] private TMP_Text treeCountText;
        [SerializeField] private TMP_Text hiderCountText;

        [Header("Refs (Drag & Drop)")]
        [SerializeField] private TeamProgressState state;
        [SerializeField] private InteractionController interaction;

        private int hiderCount = 0;
        private int hiderCountInitial = 0;
        private int targetTotal = 0;
        private int targetDone = 0;

        private void Awake()
        {
            if (!instance) instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            if (!interaction) interaction = GetComponent<InteractionController>();
            if (!state)
            {
                Debug.LogError("[MissionManager] TeamProgressState가 비었습니다. 인스펙터에 드래그로 연결하세요.");
                return;
            }

            PlayManager.Instance.ObserverManager.observerIds.OnListChanged += OnListChanged;

            var uniqueIds = new HashSet<ulong>();

            foreach (var hider in PlayManager.Instance.RoleManager.hiderIds)
            {
                if (!uniqueIds.Add(hider.ClientId)) continue;
                hiderCount++;
            }
            hiderCountInitial = Mathf.Max(1, hiderCount);

            targetTotal = interaction ? interaction.TargetCount : 0;
            targetTotal = Mathf.Max(1, targetTotal);

            hiderCountText.text = $": {hiderCount}";
            treeCountText.text = $": {targetTotal}";

            PushSeekerProgress();
            PushHiderProgress();

            if (interaction != null) interaction.OnTargetCompleted += OnHiderTargetCompleted;

            var ic = GetComponent<Interactions.InteractionController>();
            ic.OnTargetCompleted += OnHiderTargetCompleted;

            //hiderCountText.text = $": {hiderCount}";
            //treeCountText.text = $": {GetComponent<InteractionController>().TargetCount}";
        }

        private void OnDestroy()
        {
            if (PlayManager.Instance && PlayManager.Instance.ObserverManager != null)
                PlayManager.Instance.ObserverManager.observerIds.OnListChanged -= OnListChanged;

            if (interaction != null)
                interaction.OnTargetCompleted -= OnHiderTargetCompleted;
        }

        // 하이더가 '관전자(Observer)'로 이동 → 시커가 한 명 잡은 것
        private void OnListChanged(NetworkListEvent<ulong> changeEvent)
        {
            hiderCountText.text = $": {--hiderCount}";
            PushSeekerProgress();
        }

        // 하이더 측 상호작용(트리 등) 1개 완료
        private void OnHiderTargetCompleted()
        {
            var ic = GetComponent<Interactions.InteractionController>();
            treeCountText.text = $": {ic.RemainingTargetCount}";

            // 진행도 0~1 (네 TeamProgressState 그대로 사용)
            float norm = ic.TargetCount > 0 ? (float)ic.CompletedTargetCount / ic.TargetCount : 0f;
            state.SetProgressRpc(Gameplay.TeamRole.Hider, norm);
        }

        // === 진행도 계산 & 서버 반영 ===

        // Seeker: (잡힌 하이더 수) / (초기 하이더 수)
        private void PushSeekerProgress()
        {
            float caught = hiderCountInitial - hiderCount;
            float norm = Mathf.Clamp01(caught / (float)hiderCountInitial);

            // 서버에 설정 요청 (모든 클라 HUD 자동 업데이트)
            state.SetProgressRpc(TeamRole.Seeker, norm);
        }

        // Hider: (완료한 타겟 수) / (총 타겟 수)
        private void PushHiderProgress()
        {
            float norm = Mathf.Clamp01(targetDone / (float)targetTotal);
            state.SetProgressRpc(TeamRole.Hider, norm);
        }
    }
}