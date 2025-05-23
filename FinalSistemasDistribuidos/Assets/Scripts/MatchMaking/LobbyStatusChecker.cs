using UnityEngine;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;

public class LobbyStatusChecker : MonoBehaviour
{
    [SerializeField] private LobbyVisualizer lobbyVisualizer;

    public void CheckIfInLobby()
    {
        string uid = FirebaseAuth.DefaultInstance.CurrentUser?.UserId;
        if (string.IsNullOrEmpty(uid))
        {
            Debug.LogWarning("No authenticated user found.");
            return;
        }

        DatabaseReference userRef = FirebaseDatabase.DefaultInstance.GetReference("users").Child(uid);
        userRef.Child("currentLobby").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Error checking currentLobby: " + task.Exception);
                return;
            }

            if (!task.Result.Exists)
            {
                Debug.Log("User is not in a lobby.");
                return;
            }

            string lobbyId = task.Result.Value?.ToString();
            if (!string.IsNullOrEmpty(lobbyId))
            {
                Debug.Log("User is in lobby: " + lobbyId);
                lobbyVisualizer.VisualizeLobby(lobbyId);
            }
            else
            {
                Debug.Log("currentLobby field is empty.");
            }
        });
    }
}
