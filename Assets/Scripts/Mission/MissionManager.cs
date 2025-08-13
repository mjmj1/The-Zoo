using System.Collections.Generic;
using GamePlay;
using Interactions;
using Players;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Mission
{
    public class MissionManager : MonoBehaviour
    {
        public static MissionManager instance;

        [SerializeField] private TMP_Text appleCountText;
        [SerializeField] private TMP_Text hiderCountText;

        private int hiderCount = 0;

        private void Awake()
        {
            if (!instance) instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            PlayManager.Instance.ObserverManager.observerIds.OnListChanged += OnListChanged;

            var uniqueIds = new HashSet<ulong>();

            foreach (var hider in PlayManager.Instance.RoleManager.HiderIds)
            {
                if (!uniqueIds.Add(hider.ClientId)) continue;
                hiderCount++;
            }

            hiderCountText.text = $": {hiderCount}";
            appleCountText.text = $": {GetComponent<InteractionController>().TargetCount}";
        }

        private void OnListChanged(NetworkListEvent<ulong> changeEvent)
        {
            hiderCountText.text = $": {--hiderCount}";
        }
    }
}