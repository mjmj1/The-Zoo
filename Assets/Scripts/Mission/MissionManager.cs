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
                Debug.LogError("[MissionManager] TeamProgressState�� ������ϴ�. �ν����Ϳ� �巡�׷� �����ϼ���.");
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

        // ���̴��� '������(Observer)'�� �̵� �� ��Ŀ�� �� �� ���� ��
        private void OnListChanged(NetworkListEvent<ulong> changeEvent)
        {
            hiderCountText.text = $": {--hiderCount}";
            PushSeekerProgress();
        }

        // ���̴� �� ��ȣ�ۿ�(Ʈ�� ��) 1�� �Ϸ�
        private void OnHiderTargetCompleted()
        {
            var ic = GetComponent<Interactions.InteractionController>();
            treeCountText.text = $": {ic.RemainingTargetCount}";

            // ���൵ 0~1 (�� TeamProgressState �״�� ���)
            float norm = ic.TargetCount > 0 ? (float)ic.CompletedTargetCount / ic.TargetCount : 0f;
            state.SetProgressRpc(Gameplay.TeamRole.Hider, norm);
        }

        // === ���൵ ��� & ���� �ݿ� ===

        // Seeker: (���� ���̴� ��) / (�ʱ� ���̴� ��)
        private void PushSeekerProgress()
        {
            float caught = hiderCountInitial - hiderCount;
            float norm = Mathf.Clamp01(caught / (float)hiderCountInitial);

            // ������ ���� ��û (��� Ŭ�� HUD �ڵ� ������Ʈ)
            state.SetProgressRpc(TeamRole.Seeker, norm);
        }

        // Hider: (�Ϸ��� Ÿ�� ��) / (�� Ÿ�� ��)
        private void PushHiderProgress()
        {
            float norm = Mathf.Clamp01(targetDone / (float)targetTotal);
            state.SetProgressRpc(TeamRole.Hider, norm);
        }
    }
}