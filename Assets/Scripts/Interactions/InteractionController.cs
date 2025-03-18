using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class InteractionController : MonoBehaviour
{
    [SerializeField] private Button gameStartButton;
    [SerializeField] private GameObject worldObject;
    [SerializeField] private GameObject interactionObjects;
    public SphereCollider SphereCollider;

    private void Start()
    {
        SphereCollider = worldObject.GetComponentInChildren<SphereCollider>();

        gameStartButton.onClick.AddListener(OnGameStartButtonClick);
    }
    
    public void OnGameStartButtonClick()
    {
        GameObject objects = Instantiate(interactionObjects, RandomPosition(), Quaternion.identity);
        StartCoroutine(PauseObjectsRoutine(objects));
    }

    public IEnumerator PauseObjectsRoutine(GameObject objects)
    {
        yield return new WaitForSeconds(5);
        objects.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
    }


    Vector3 RandomPosition()
    {
        Vector3 originPosition = interactionObjects.transform.position;

        float range_X = SphereCollider.bounds.size.x;
        float range_Y = SphereCollider.bounds.size.y;
        float range_Z = SphereCollider.bounds.size.z;

        range_X = Random.Range((range_X / 2) * -1, range_X / 2);
        range_Y = Random.Range((range_Y / 2) * -1, range_Y / 2);
        range_Z = Random.Range((range_Z / 2) * -1, range_Z / 2);

        Vector3 RandomPostion = new Vector3(range_X, range_Y, range_Z);

        Vector3 respawnPosition = originPosition + RandomPostion;

        return respawnPosition;
    }

}