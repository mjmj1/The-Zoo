// MissionDebugHotkeys.cs
using UnityEngine;
using Gameplay;           // TeamProgressState, TeamRole
using Interactions;      // InteractionController
using Unity.Netcode;

public class MissionDebugHotkeys : MonoBehaviour
{
    [Header("Drag & Drop")]
    [SerializeField] private TeamProgressState state;       // �� ������Ʈ �巡��
    [SerializeField] private InteractionController ic;      // �� ������Ʈ �巡�� (��� ����)
    [SerializeField] private bool useControllerForHider = true; // true�� ��Ʈ�ѷ� ��η� �Ϸ� ó��

    [Header("Steps (0~1)")]
    [SerializeField] private float seekerStep = 0.05f; // 5%
    [SerializeField] private float hiderStep = 0.02f; // 2%

    // ��Ʈ�ѷ��� �� �� �� �ӽ� ī����(�������� ���)
    private int _tmpCompleted;

    void Update()
    {
        // 1) ���̴� ��ǥ 1�� �Ϸ�
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (useControllerForHider && ic != null)
            {
                // ��Ʈ�ѷ��� Report RPC�� ������ٸ� �� ���(���� �ڿ�������)
                ic.ReportTargetCompletedRpc();
            }
            else
            {
                // ��Ʈ�ѷ� �� ���� ��: ���൵�� ���� �ø���
                _tmpCompleted = Mathf.Min(_tmpCompleted + 1, Mathf.Max(1, ic != null ? ic.TargetCount : 1));
                float norm = (ic != null && ic.TargetCount > 0)
                    ? (float)_tmpCompleted / ic.TargetCount
                    : (float)_tmpCompleted / 5f; // �ӽ� �и�(���ϸ� �ν����ͷ� ����)
                state.SetProgressRpc(TeamRole.Hider, Mathf.Clamp01(norm));
            }
        }

        // 2) ��Ŀ ���൵ +step
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            state.AddProgressRpc(TeamRole.Seeker, seekerStep);
        }

        // 3) ���̴� ���൵ +step (����)
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            state.AddProgressRpc(TeamRole.Hider, hiderStep);
        }

        // 4) �� �� �ʱ�ȭ
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            _tmpCompleted = 0;
            state.SetProgressRpc(TeamRole.Seeker, 0f);
            state.SetProgressRpc(TeamRole.Hider, 0f);
        }
    }
}
