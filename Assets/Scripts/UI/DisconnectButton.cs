using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class DisconnectButton : Button
    {
        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);
            
            GameManager.Instance.connectionManager.DisconnectSessionAsync();
        }
    }
}
