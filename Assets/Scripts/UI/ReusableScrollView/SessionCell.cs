using TMPro;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.UI;

namespace UI.ReusableScrollView
{
    public class SessionCell : MonoBehaviour, IReusableCell<ISessionInfo>
    {
        [SerializeField] private Image lockIcon;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text playerCountText;

        private ISessionInfo _sessionInfo;
        
        public void Setup(ISessionInfo session, int index)
        {
            lockIcon.enabled = session.HasPassword;
            
            nameText.text = session.Name;
            
            var currentPlayer = session.MaxPlayers - session.AvailableSlots;
            playerCountText.text = $"{currentPlayer}/{session.MaxPlayers}";
        }
    }
}