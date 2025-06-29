using UnityEngine.SceneManagement;

namespace Utils
{
    static class GameplayEventHandler
    {
        internal static void LoadLobbyScene()
        {
            SceneManager.LoadScene("Lobby");
        }
    }
}