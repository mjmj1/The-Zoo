using TMPro;
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

    public void OnSettingBtnClick()
    {
        roomNameIpf.text = GameManager.Instance.title;

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
