using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase.Database;
using Firebase.Auth;
using System.Collections.Generic;

public class FriendRequestCard : MonoBehaviour
{
    public TextMeshProUGUI requestText;
    public Button acceptButton;
    public Button rejectButton;

    private string requesterId;
    private string requesterName;
    private string currentUserId;

    public void Setup(string _requesterId, string _requesterName)
    {
        requesterId = _requesterId;
        requesterName = _requesterName;
        currentUserId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;

        requestText.text = $"{requesterName} quiere ser tu amigo";
        acceptButton.onClick.AddListener(AcceptRequest);
        rejectButton.onClick.AddListener(RejectRequest);
    }

    void AcceptRequest()
    {
        DatabaseReference db = FirebaseDatabase.DefaultInstance.RootReference;
        string friendRequestPath = $"users/{currentUserId}/friendRequest/{requesterId}";

        // Primero, leemos el nombre del usuario que envió la solicitud
        FirebaseDatabase.DefaultInstance.GetReference(friendRequestPath)
            .GetValueAsync().ContinueWith(task =>
            {
                if (task.IsCompleted && task.Result.Exists)
                {
                    string requesterName = task.Result.Value.ToString();
                    string currentUserName = FirebaseAuth.DefaultInstance.CurrentUser.DisplayName ?? "Desconocido";

                    Dictionary<string, object> updates = new Dictionary<string, object>
                    {
                        // Añadir a ambos como amigos (con nombre)
                        [$"users/{currentUserId}/friends/{requesterId}"] = requesterName,
                        [$"users/{requesterId}/friends/{currentUserId}"] = currentUserName,
                        // Eliminar la solicitud
                        [$"users/{currentUserId}/friendRequest/{requesterId}"] = null
                    };

                    db.UpdateChildrenAsync(updates).ContinueWith(updateTask =>
                    {
                        if (updateTask.IsCompleted)
                            Destroy(gameObject);
                    });
                }
            });
    }


    void RejectRequest()
    {
        FirebaseDatabase.DefaultInstance.GetReference("users")
            .Child(currentUserId)
            .Child("friendRequest")
            .Child(requesterId)
            .RemoveValueAsync().ContinueWith(task =>
            {
                if (task.IsCompleted)
                    Destroy(gameObject);
            });
    }
}
