using TMPro;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class SessionItem : MonoBehaviour, ISelectHandler
{
    [SerializeField]
    TMP_Text m_SessionNameText;
        
    [SerializeField]
    TMP_Text m_SessionPlayersText;
    
    public UnityEvent<ISessionInfo> OnSessionSelected;
        
    ISessionInfo m_SessionInfo;

    public void SetSession(ISessionInfo sessionInfo)
    {
        m_SessionInfo = sessionInfo;
        SetSessionName(m_SessionInfo.Id);
        var currentPlayers = m_SessionInfo.MaxPlayers - m_SessionInfo.AvailableSlots;
        SetPlayers(currentPlayers, m_SessionInfo.MaxPlayers);
    }
        
    void SetSessionName(string sessionName)
    {
        m_SessionNameText.text = sessionName;
    }
        
    void SetPlayers(int currentPlayers, int maxPlayers)
    {
        m_SessionPlayersText.text = $"{currentPlayers}/{maxPlayers}";
    }

    public void OnSelect(BaseEventData eventData)
    {
        OnSessionSelected?.Invoke(m_SessionInfo);
    }
}
