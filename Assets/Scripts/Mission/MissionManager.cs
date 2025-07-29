using Interactions;
using Players.Roles;
using TMPro;
using UI.PlayerList;
using UnityEngine;

public class MissionManager : MonoBehaviour
{
    public static MissionManager instance;

    public int HiderCount = 0;

    [SerializeField] private TMP_Text treeCountText;
    [SerializeField] private TMP_Text HiderCountText;

    private void Awake()
    {
        if(instance == null)
            instance = this;
    }

    private void Start()
    {
        var players = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < players.Length; i++)
        {
            var player = players[i];
            if(player.GetComponent<HiderRole>().gameObject.activeSelf)
            {
                HiderCount++;
            }
        }

        treeCountText.text = ": " + this.GetComponent<InteractionController>().TargetCount.ToString();
        HiderCountText.text = ": " + HiderCount;
    }
}
