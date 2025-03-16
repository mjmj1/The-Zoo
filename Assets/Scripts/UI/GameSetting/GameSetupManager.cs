using TMPro;
using UI.Sessions;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.UI;

public class GameSetupManager : MonoBehaviour
{
    [SerializeField] private GameObject gameSetupPopup;

    [SerializeField] private TMP_InputField _roomNameInputfield;
    [SerializeField] private TMP_Dropdown _headCountDropdown;
    [SerializeField] private TMP_Dropdown _levelDropdown;
    [SerializeField] private TMP_Dropdown _ratioDropdown;
    [SerializeField] private Button completeButton;
    [SerializeField] private Button cancelButton;

    GameObject[] players;
    private GameObject[] _gameObjects;

    private void Start()
    {
        _gameObjects = GameObject.FindGameObjectsWithTag(Strings.PLAYER);
    }

    public void OnSettingButtonClick()
    {
        Debug.Log("players.Length : " + players.Length);

        // roomNameIpf.text = GameManager.Instance.title;

        if (gameSetupPopup.activeSelf)
        {
            gameSetupPopup.SetActive(false);
        }
        else
        {
            gameSetupPopup.SetActive(true);
        }
    }

    public void OnCompleteButtonClick()
    {
        gameSetupPopup.SetActive(false);
    }

    public void OnCancelButtonClick()
    {
        gameSetupPopup.SetActive(false);
    }
}
