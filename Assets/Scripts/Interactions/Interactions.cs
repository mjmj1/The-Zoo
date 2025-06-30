using Static;
using Unity.VisualScripting;
using UnityEngine;
/// <summary>
/// ���Ŀ� ���ͷ��� ������Ʈ�� �����ϰ� ��ȣ�ۿ뿡 �䱸�Ǵ� ���׵��� ����
/// = This script is including parts with creating objects and interacting with them
/// �ش� ��ũ��Ʈ�� ������ ���� ������ �Ѿ �� ����Ǿ� ��
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
            InteractionObjects = GameObject.FindGameObjectsWithTag("Tree"); // �浹�� �浹 ������ ��ȣ�ۿ�
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
