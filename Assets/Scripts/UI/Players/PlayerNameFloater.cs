using TMPro;
using Unity.Services.Authentication;
using UnityEngine;

namespace UI.Players
{
    public class PlayerNameFloater : MonoBehaviour
    {
        [SerializeField] private TMP_Text playerNameText;

        private void Start()
        {
            playerNameText.text = AuthenticationService.Instance.PlayerId;
        }
    }
}