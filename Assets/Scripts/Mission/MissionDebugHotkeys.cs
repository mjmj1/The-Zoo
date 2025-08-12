using UnityEngine;
using Gameplay;
using Interactions;
// Gauge Test Code
public class MissionDebugHotkeys : MonoBehaviour
{
    [Header("Drag & Drop")]
    [SerializeField] private TeamProgressState state;
    [SerializeField] private InteractionController ic;
    [SerializeField] private bool useControllerForHider = true;

    [Header("Steps (0~1)")]
    [SerializeField] private float seekerStep = 0.05f;
    [SerializeField] private float hiderStep = 0.02f;

    private int _tmpCompleted;

    void Update()
    {
        //    if (Input.GetKeyDown(KeyCode.Alpha1))
        //    {
        //        if (useControllerForHider && ic != null)
        //        {
        //            ic.ReportTargetCompletedRpc();
        //        }
        //        else
        //        {
        //            _tmpCompleted = Mathf.Min(_tmpCompleted + 1, Mathf.Max(1, ic != null ? ic.TargetCount : 1));
        //            float norm = (ic != null && ic.TargetCount > 0)
        //                ? (float)_tmpCompleted / ic.TargetCount
        //                : (float)_tmpCompleted / 5f;
        //            state.SetProgressRpc(TeamRole.Hider, Mathf.Clamp01(norm));
        //        }
        //    }

        //    if (Input.GetKeyDown(KeyCode.Alpha2))
        //    {
        //        state.AddProgressRpc(TeamRole.Seeker, seekerStep);
        //    }

        //    if (Input.GetKeyDown(KeyCode.Alpha3))
        //    {
        //        state.AddProgressRpc(TeamRole.Hider, hiderStep);
        //    }

        //    if (Input.GetKeyDown(KeyCode.Alpha0))
        //    {
        //        _tmpCompleted = 0;
        //        state.SetProgressRpc(TeamRole.Seeker, 0f);
        //        state.SetProgressRpc(TeamRole.Hider, 0f);
        //    }
        //}
    }
}
