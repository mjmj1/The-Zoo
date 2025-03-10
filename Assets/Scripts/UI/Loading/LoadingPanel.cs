using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Loading
{
    public class LoadingPanel : MonoBehaviour
    {
        [SerializeField] TMP_Text title;
        void Start()
        {
            gameObject.SetActive(false);
        }

        void OnEnable()
        {
            
        }
    }
}
