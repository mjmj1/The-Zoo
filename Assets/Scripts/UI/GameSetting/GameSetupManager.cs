using Static;
using TMPro;
using UI.Sessions;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.UI;
using Networks;

public class GameSetupManager : MonoBehaviour
{
    [SerializeField] private GameObject _gameSetupPopup;

    [SerializeField] private TMP_InputField _roomNameInputfield;
    [SerializeField] private TMP_Dropdown _headCountDropdown;
    [SerializeField] private TMP_Dropdown _levelDropdown;
    [SerializeField] private TMP_Dropdown _ratioDropdown;
    [SerializeField] private Button _completeButton;
    [SerializeField] private Button _cancelButton;
    [SerializeField] private GameObject _warningPopup;

    private ConnectionManager connectionManager;
    // map
    public Transform _world;
    public GameObject _respawnRange;
    [SerializeField] private SphereCollider _rangeCollider;
    // interactions
    [SerializeField] private GameObject _interactionObjectsPrefab;

    public void OnSettingButtonClick()
    {
        if (_gameSetupPopup.activeSelf)
        {
            _gameSetupPopup.SetActive(false);
        }
        else
        {
            _gameSetupPopup.SetActive(true);
        }
    }

    public void OnCompleteButtonClick()
    {
        // complete을 누르면 인원변동 사항이 적용 됨
        connectionManager = GameObject.Find("NetworkManager").GetComponent<ConnectionManager>();
        Debug.Log("limit player : " + _headCountDropdown.options[_headCountDropdown.value].text);
        int maxPlayers = int.Parse(_headCountDropdown.options[_headCountDropdown.value].text);

        connectionManager.UpdateSessionAsync("test", maxPlayers);

        _gameSetupPopup.SetActive(false);
    }

    public void OnCancelButtonClick()
    {
        _gameSetupPopup.SetActive(false);
    }

    public void OnCheckButtonClick()
    {
        _warningPopup.SetActive(false);
    }

    public void OnStartButtonClick()
    {
        for(int i = 0; i < 3; i++)
        {
            var item = Instantiate(_interactionObjectsPrefab, Return_RansdomPosition(), Quaternion.identity);
        }
    }

    private Vector3 Return_RansdomPosition()
    {
        Vector3 originPosition = _respawnRange.transform.position;
        
        float range_X = _rangeCollider.bounds.size.x;
        float range_Z = _rangeCollider.bounds.size.z;

        range_X = Random.Range((range_X / 2) * -1, range_X / 2);
        range_Z = Random.Range((range_Z / 2) * -1, range_Z / 2);
        Vector3 RandomPosition = new Vector3(range_X, 0f, range_Z);

        Vector3 respawnPosition = originPosition + RandomPosition;
        return respawnPosition;
    }
}
