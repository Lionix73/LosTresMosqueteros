using UnityEngine;
using Firebase.Database;
using Firebase.Auth;
using System;
using TMPro;
using System.Collections;
using Firebase.Extensions;

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
        DatabaseReference userLobbyRef = FirebaseDatabase.DefaultInstance
            .GetReference("users")
            .Child(currentUserId)
            .Child("currentLobby");

        // Verificamos si el usuario ya tiene un lobby activo
        userLobbyRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Error al obtener currentLobby del usuario.");
                return;
            }

            string existingLobbyId = task.Result.Exists ? task.Result.Value.ToString() : null;
            string lobbyIdToUse = existingLobbyId;

            if (string.IsNullOrEmpty(existingLobbyId))
            {
                // Crear nuevo lobby
                lobbyIdToUse = Guid.NewGuid().ToString();
                DatabaseReference newLobbyRef = FirebaseDatabase.DefaultInstance.GetReference("lobbies").Child(lobbyIdToUse);

                newLobbyRef.Child("host").SetValueAsync(currentUserId);
                newLobbyRef.Child("members").Child(currentUserId).SetValueAsync(FirebaseAuth.DefaultInstance.CurrentUser.DisplayName);

                // Actualizar currentLobby del usuario
                userLobbyRef.SetValueAsync(lobbyIdToUse);
            }

            // Guardar invitación para el amigo
            DatabaseReference inviteRef = FirebaseDatabase.DefaultInstance
                .GetReference("users")
                .Child(friendUserId)
                .Child("invitations")
                .Child(currentUserId);

            inviteRef.Child("lobbyId").SetValueAsync(lobbyIdToUse);
            inviteRef.Child("username").SetValueAsync(FirebaseAuth.DefaultInstance.CurrentUser.DisplayName);
            inviteRef.Child("timestamp").SetValueAsync(ServerValue.Timestamp);

            Debug.Log("Invitación enviada al lobby: " + lobbyIdToUse);
        });
    }

}