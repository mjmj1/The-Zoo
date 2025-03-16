using Static;
using TMPro;
using UI.Sessions;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.UI;

public class GameSetupManager : MonoBehaviour
{
    [SerializeField] private GameObject gameSetupPopup;

    [SerializeField] private TMP_InputField roomNameIpf;
    [SerializeField] private TMP_Dropdown headCountDrd;
    [SerializeField] private TMP_Dropdown levelDrd;
    [SerializeField] private TMP_Dropdown ratioDrd;
    [SerializeField] private Button completeBtn;
    [SerializeField] private Button cancelBtn;

    GameObject[] players;
    private GameObject[] _gameObjects;

    private void Start()
    {
        _gameObjects = GameObject.FindGameObjectsWithTag(Strings.PLAYER);
    }

    public void OnSettingBtnClick()
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

    public void OnCompleteBtnClick()
    {
        gameSetupPopup.SetActive(false);
    }

    public void OnCancelBtnClick()
    {
        gameSetupPopup.SetActive(false);
    }
}
