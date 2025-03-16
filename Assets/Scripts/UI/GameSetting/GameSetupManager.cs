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
    [SerializeField] private GameObject _sessionPlayerList;

    GameObject[] players;
    private GameObject[] _gameObjects;

    private ConnectionManager connectionManager;

    private void Start()
    {
        _gameObjects = GameObject.FindGameObjectsWithTag(Strings.PLAYER);
    }

    public void OnPlayerListButtonClick()
    {
        if(_sessionPlayerList.activeSelf)
        {
            _sessionPlayerList.SetActive(false);
        }
        else
        {
            _sessionPlayerList.SetActive(true);
        }
        
    }

    public void OnSettingButtonClick()
    {
        // roomNameIpf.text = GameManager.Instance.title;

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
        // complete�� ������ �ο����� ������ ���� �Ǿ�� ��
        connectionManager = GameObject.Find("NetworkManager").GetComponent<ConnectionManager>();
        Debug.Log("limit player : " + _headCountDropdown.options[_headCountDropdown.value].text);
        int maxPlayers = int.Parse(_headCountDropdown.options[_headCountDropdown.value].text);

        connectionManager.UpdateSession("test", maxPlayers);

        _gameSetupPopup.SetActive(false);
    }

    public void OnCancelButtonClick()
    {
        _gameSetupPopup.SetActive(false);
    }
}
