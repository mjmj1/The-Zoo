using System;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Interactions
{
    public abstract class Interactable : NetworkBehaviour
    {
        public enum InteractableType
        {
            LeftClick,
            RightClick,
            R,
            F
        }

        public static Interactable instance;

        [SerializeField] protected Image interactionUI;
        [SerializeField] protected RectTransform canvas;
        private Camera cam;
        public Vector3 offset;
        public NetworkVariable<bool> TargetMission;
        public NetworkVariable<int> maxSpawnCount;

        private void Reset()
        {
            TargetMission.Value = false;
            maxSpawnCount.Value = 4;
        }
        public override void OnNetworkSpawn()
        {
            TargetMission.OnValueChanged += OnTargetMissionChanged;
            maxSpawnCount.OnValueChanged += OnMaxSpawnCountChanged;

            OnTargetMissionChanged(false, TargetMission.Value);
            OnMaxSpawnCountChanged(0, 4);
        }
        public override void OnNetworkDespawn()
        {
            TargetMission.OnValueChanged -= OnTargetMissionChanged;
            maxSpawnCount.OnValueChanged -= OnMaxSpawnCountChanged;
        }
        private void OnTargetMissionChanged(bool previousValue, bool newValue)
        {
            if (!IsOwner) return;
            TargetMission.Value = newValue;
        }
        private void OnMaxSpawnCountChanged(int previousValue, int newValue)
        {
            if (!IsOwner) return;
            maxSpawnCount.Value = newValue;
        }
        private void Awake()
        {
            if(instance == null)
            {
                instance = this;
            }
        }

        protected virtual void Start()
        {
            cam = Camera.main;
            offset = new Vector3(1.0f, 1.0f, 0);
            
            if (interactionUI != null)
            {
                interactionUI.gameObject.SetActive(false);
            }
        }

        public void ShowInteractableUI()
        {
            if (!interactionUI) return;
            
            interactionUI.gameObject.SetActive(true);
            
            if (!cam.transform.hasChanged) return;

            interactionUI.transform.position = cam.WorldToScreenPoint(transform.position + offset);
        }

        public void HideInteractableUI()
        {
            interactionUI?.gameObject.SetActive(false);
        }

        public abstract void StartInteract();
        public abstract void StopInteract();

        public abstract InteractableType GetInteractableType();
    }
}