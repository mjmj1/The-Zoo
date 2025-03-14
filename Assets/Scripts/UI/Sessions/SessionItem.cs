using TMPro;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class SessionItem : MonoBehaviour, ISelectHandler
{
    [SerializeField]
    TMP_Text sessionNameText;
        
    [SerializeField]
    TMP_Text sessionPlayersText;
    
    public UnityEvent<ISessionInfo> onSessionSelected;
        
    ISessionInfo _sessionInfo;

    public void SetSession(ISessionInfo sessionInfo)
    {
        this._sessionInfo = sessionInfo;
        UpdateSessionName(this._sessionInfo.Name);
        var currentPlayers = this._sessionInfo.MaxPlayers - this._sessionInfo.AvailableSlots;
        UpdatePlayersCount(currentPlayers, this._sessionInfo.MaxPlayers);
    }

    public void OnSelect(BaseEventData eventData)
    {
        print("OnSelect");
        onSessionSelected?.Invoke(_sessionInfo);
    }
    
    private void UpdateSessionName(string sessionName)
    {
        sessionNameText.text = sessionName;
    }

    private void UpdatePlayersCount(int currentPlayers, int maxPlayers)
    {
        sessionPlayersText.text = $"{currentPlayers}/{maxPlayers}";
    }
}
