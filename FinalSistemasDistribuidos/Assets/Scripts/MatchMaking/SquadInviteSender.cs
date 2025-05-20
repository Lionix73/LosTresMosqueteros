using UnityEngine;
using Firebase.Database;
using Firebase.Auth;
using System;
using TMPro;

public class SquadInviteSender : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI usernameText;

    public void SearchFriendID()
    {
        string targetFriendName = usernameText.text;

        // 1. Buscar en "users-online" para encontrar el userId que tenga ese username
        FirebaseDatabase.DefaultInstance
            .GetReference("users-online")
            .GetValueAsync().ContinueWith(task =>
            {
                if (task.IsFaulted || !task.IsCompleted) return;

                DataSnapshot snapshot = task.Result;

                string targetUserId = null;

                foreach (var child in snapshot.Children)
                {
                    string onlineUserId = child.Key;
                    string onlineUsername = child.Value.ToString();

                    if (onlineUsername == targetFriendName)
                    {
                        targetUserId = onlineUserId;
                        SendSquadInvite(targetUserId);
                        break;
                    }
                }

                if (string.IsNullOrEmpty(targetUserId))
                {
                    Debug.LogWarning("No se encontró al usuario objetivo en línea.");
                    return;
                }
            });
    }

    public void SendSquadInvite(string friendUserId)
    {
        string currentUserId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        string lobbyId = Guid.NewGuid().ToString(); // Generamos ID de lobby

        // Creamos el lobby con el host
        DatabaseReference lobbyRef = FirebaseDatabase.DefaultInstance.GetReference("lobbies").Child(lobbyId);
        lobbyRef.Child("host").SetValueAsync(currentUserId);
        lobbyRef.Child("members").Child(currentUserId).SetValueAsync(FirebaseAuth.DefaultInstance.CurrentUser.DisplayName);

        // Guardamos la invitación en el nodo del usuario receptor
        DatabaseReference inviteRef = FirebaseDatabase.DefaultInstance
            .GetReference("users")
            .Child(friendUserId)
            .Child("invitations")
            .Child(currentUserId);

        inviteRef.Child("lobbyId").SetValueAsync(lobbyId);
        inviteRef.Child("username").SetValueAsync(FirebaseAuth.DefaultInstance.CurrentUser.DisplayName);
        inviteRef.Child("timestamp").SetValueAsync(ServerValue.Timestamp);

        // Guardamos nuestro estado de lobby también
        FirebaseDatabase.DefaultInstance.GetReference("users").Child(currentUserId).Child("currentLobby").SetValueAsync(lobbyId);

        Debug.Log("Invitación enviada a lobby: " + lobbyId);
    }
}