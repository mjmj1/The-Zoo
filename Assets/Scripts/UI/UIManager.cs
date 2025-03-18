using UnityEngine;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] GameObject TitleMenu;
        [SerializeField] GameObject LobbyMenu;
        
        static InformationPopup _informationPopup;

        void Awake()
        {
            _informationPopup = GetComponent<InformationPopup>();
            TitleMenu.SetActive(true);
            LobbyMenu.SetActive(false);
        }

        public static void OpenInformationPopup(string massage)
        {
            _informationPopup.GetInformationPopup(massage);
        }
    }
}
