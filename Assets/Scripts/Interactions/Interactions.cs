using Static;
using Unity.VisualScripting;
using UnityEngine;
/// <summary>
/// 추후에 인터렉션 오브젝트를 생성하고 상호작용에 요구되는 사항들을 포함
/// = This script is including parts with creating objects and interacting with them
/// 해당 스크립트의 내용은 게임 씬으로 넘어갈 때 실행되야 함
/// = This script needs to run when the game scene is loading
/// </summary>
public class Interactions : MonoBehaviour
{
    public GameObject[] InteractionObjects;
    private InteractionObject[] _interactionObjects;

    void Update()
    {
        if (InteractionObjects.Length == 0)
        {
            InteractionObjects = GameObject.FindGameObjectsWithTag("Tree"); // 충돌시 충돌 대상과의 상호작용
            _interactionObjects = new InteractionObject[InteractionObjects.Length];
            for (int i = 0; i < InteractionObjects.Length; i++)
            {
                _interactionObjects[i] = InteractionObjects[i].gameObject.GetComponentInChildren<InteractionObject>();
            }
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            TryStartInteraction();
        }

        if (Input.GetKeyUp(KeyCode.F))
        {
            CancelInteraction();
        }
    }

    void TryStartInteraction()
    {
        for (int i = 0; i < _interactionObjects.Length; i++)
        {
            _interactionObjects[i].StartInteraction();
        }

    }

    void CancelInteraction()
    {
        for (int i = 0; i < _interactionObjects.Length; i++)
        {
            _interactionObjects[i].CancelInteraction();
        }
    }
}
