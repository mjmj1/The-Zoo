// MissionDebugHotkeys.cs
using UnityEngine;
using Gameplay;           // TeamProgressState, TeamRole
using Interactions;      // InteractionController
using Unity.Netcode;

public class MissionDebugHotkeys : MonoBehaviour
{
    [Header("Drag & Drop")]
    [SerializeField] private TeamProgressState state;       // 씬 오브젝트 드래그
    [SerializeField] private InteractionController ic;      // 씬 오브젝트 드래그 (없어도 동작)
    [SerializeField] private bool useControllerForHider = true; // true면 컨트롤러 경로로 완료 처리

    [Header("Steps (0~1)")]
    [SerializeField] private float seekerStep = 0.05f; // 5%
    [SerializeField] private float hiderStep = 0.02f; // 2%

    // 컨트롤러를 못 쓸 때 임시 카운터(씬에서만 사용)
    private int _tmpCompleted;

    void Update()
    {
        // 1) 하이더 목표 1개 완료
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (useControllerForHider && ic != null)
            {
                // 컨트롤러에 Report RPC를 만들었다면 이 경로(가장 자연스러움)
                ic.ReportTargetCompletedRpc();
            }
            else
            {
                // 컨트롤러 손 못댈 때: 진행도만 직접 올리기
                _tmpCompleted = Mathf.Min(_tmpCompleted + 1, Mathf.Max(1, ic != null ? ic.TargetCount : 1));
                float norm = (ic != null && ic.TargetCount > 0)
                    ? (float)_tmpCompleted / ic.TargetCount
                    : (float)_tmpCompleted / 5f; // 임시 분모(원하면 인스펙터로 노출)
                state.SetProgressRpc(TeamRole.Hider, Mathf.Clamp01(norm));
            }
        }

        // 2) 시커 진행도 +step
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            state.AddProgressRpc(TeamRole.Seeker, seekerStep);
        }

        // 3) 하이더 진행도 +step (직접)
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            state.AddProgressRpc(TeamRole.Hider, hiderStep);
        }

        // 4) 둘 다 초기화
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            _tmpCompleted = 0;
            state.SetProgressRpc(TeamRole.Seeker, 0f);
            state.SetProgressRpc(TeamRole.Hider, 0f);
        }
    }
}
