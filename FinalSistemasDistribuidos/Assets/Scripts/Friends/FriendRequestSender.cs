using UnityEngine;
using TMPro;
using Firebase.Database;
using Firebase.Auth;
using System.Collections.Generic;

public class FriendRequestSender : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI usernameText;
    private DatabaseReference dbRef;
    private FirebaseAuth auth;
    //private GameObject popUpInvitation;

    private void Start()
    {
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;
        auth = FirebaseAuth.DefaultInstance;
        //popUpInvitation = GameObject.Find("PopUpInvitation");
    }

    public void SendFriendRequest()
    {
        string targetUsername = usernameText.text;

        // 1. Buscar en "users-online" para encontrar el userId que tenga ese username
        FirebaseDatabase.DefaultInstance
            .GetReference("users-online")
            .GetValueAsync().ContinueWith(task => {
                if (task.IsFaulted || !task.IsCompleted) return;

                DataSnapshot snapshot = task.Result;

                string targetUserId = null;

                foreach (var child in snapshot.Children)
                {
                    string onlineUserId = child.Key;
                    string onlineUsername = child.Value.ToString();

                    if (onlineUsername == targetUsername)
                    {
                        targetUserId = onlineUserId;
                        break;
                    }
                }

                if (string.IsNullOrEmpty(targetUserId))
                {
                    Debug.LogWarning("No se encontró al usuario objetivo en línea.");
                    return;
                }

                // 2. Obtener info del usuario actual (quien envía la solicitud)
                string myUserId = auth.CurrentUser.UserId;

                FirebaseDatabase.DefaultInstance
                    .GetReference("users")
                    .Child(myUserId)
                    .Child("username")
                    .GetValueAsync().ContinueWith(usernameTask => {
                        if (usernameTask.IsFaulted || !usernameTask.IsCompleted) return;

                        string myUsername = usernameTask.Result.Value.ToString();
                        Debug.Log(targetUserId);
                        // 3. Escribir la solicitud en el nodo del usuario objetivo
                        dbRef.Child("users")
                             .Child(targetUserId)
                             .Child("friendRequest")
                             .Child(myUserId)
                             .SetValueAsync(myUsername)
                             .ContinueWith(setTask => {
                                 if (setTask.IsCompleted)
                                 {
                                     Debug.Log("Solicitud de amistad enviada a " + targetUsername);
                                     //popUpInvitation.SetActive(true);
                                     //popUpInvitation.GetComponentInChildren<TextMeshProUGUI>().text = "Solicitud de amistad enviada a " + targetUsername;
                                 }
                                 else
                                 {
                                     Debug.LogError("Error al enviar solicitud: " + setTask.Exception);
                                 }
                             });
                    });
            });
    }
}
