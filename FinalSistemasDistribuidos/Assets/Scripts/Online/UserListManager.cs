using Firebase.Auth;
using Firebase.Database;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine;
using System.Collections;
using Firebase.Extensions;
using TMPro;

public class UserListManager : MonoBehaviour
{
    [SerializeField] private bool showFriends = false;

    public GameObject userItemPrefab;
    public GameObject friendItemPrefab;
    public Transform contentPanel;

    private string currentUserId;
    private HashSet<string> friendIds = new HashSet<string>();
    private DatabaseReference dbRef;

    void OnEnable()
    {
        currentUserId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        Invoke("LoadUserListFromFirebase", 1f);
    }

    public void LoadUserListFromFirebase()
    {
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject); // Limpiar el panel
        }

        // Paso 1: Obtener lista de amigos
        FirebaseDatabase.DefaultInstance.GetReference("users").Child(currentUserId).Child("friends")
            .GetValueAsync().ContinueWithOnMainThread(friendsTask =>
            {
                if (friendsTask.IsCompleted && friendsTask.Result.Exists)
                {
                    foreach (var friend in friendsTask.Result.Children)
                    {
                        friendIds.Add(friend.Key);

                        if(showFriends)
                        {
                            DataSnapshot snapshot = friendsTask.Result;

                            foreach (var child in snapshot.Children)
                        {
                            string userId = child.Key;

                            if (userId == currentUserId) continue; // Saltar a uno mismo

                            // Paso 3: Cargar datos del usuario
                            FirebaseDatabase.DefaultInstance.GetReference("users").Child(userId)
                                .GetValueAsync().ContinueWithOnMainThread(userTask =>
                                {
                                    if (userTask.IsFaulted || userTask.IsCanceled)
                                    {
                                        Debug.LogWarning("Error cargando usuario " + userId + ": " + userTask.Exception);
                                        return;
                                    }

                                    DataSnapshot userSnap = userTask.Result;

                                    if (userSnap.Exists)
                                    {
                                        string username = userSnap.Child("username").Value?.ToString() ?? "Desconocido";
                                        int level = userSnap.HasChild("level") ? int.Parse(userSnap.Child("level").Value.ToString()) : 1;
                                        string photoUrl = userSnap.HasChild("photoUrl") ? userSnap.Child("photoUrl").Value.ToString() : "";

                                        GameObject userItem = Instantiate(friendItemPrefab, contentPanel);
                                        userItem.transform.Find("Username").GetComponent<TextMeshProUGUI>().text = username;
                                        userItem.transform.Find("Level").GetComponent<TextMeshProUGUI>().text = "Level " + level;

                                        if (!string.IsNullOrEmpty(photoUrl))
                                            StartCoroutine(LoadImage(photoUrl, userItem.transform.Find("ProfileImg").GetComponent<RawImage>()));
                                    }
                                });
                        }
                        }
                    }
                }

                if(showFriends) return;

                // Paso 2: Obtener usuarios online
                FirebaseDatabase.DefaultInstance.GetReference("users-online")
                    .GetValueAsync().ContinueWithOnMainThread(task =>
                    {
                        if (task.IsFaulted || task.IsCanceled)
                        {
                            Debug.LogError("Error cargando usuarios online: " + task.Exception);
                            return;
                        }

                        DataSnapshot snapshot = task.Result;

                        foreach (var child in snapshot.Children)
                        {
                            string userId = child.Key;

                            if (userId == currentUserId) continue; // Saltar a uno mismo
                            if (friendIds.Contains(userId)) continue; // Saltar si es amigo

                            // Paso 3: Cargar datos del usuario
                            FirebaseDatabase.DefaultInstance.GetReference("users").Child(userId)
                                .GetValueAsync().ContinueWithOnMainThread(userTask =>
                                {
                                    if (userTask.IsFaulted || userTask.IsCanceled)
                                    {
                                        Debug.LogWarning("Error cargando usuario " + userId + ": " + userTask.Exception);
                                        return;
                                    }

                                    DataSnapshot userSnap = userTask.Result;

                                    if (userSnap.Exists)
                                    {
                                        string username = userSnap.Child("username").Value?.ToString() ?? "Desconocido";
                                        int level = userSnap.HasChild("level") ? int.Parse(userSnap.Child("level").Value.ToString()) : 1;
                                        string photoUrl = userSnap.HasChild("photoUrl") ? userSnap.Child("photoUrl").Value.ToString() : "";

                                        GameObject userItem = Instantiate(userItemPrefab, contentPanel);
                                        userItem.transform.Find("Username").GetComponent<TextMeshProUGUI>().text = username;
                                        userItem.transform.Find("Level").GetComponent<TextMeshProUGUI>().text = "Level " + level;

                                        if (!string.IsNullOrEmpty(photoUrl))
                                            StartCoroutine(LoadImage(photoUrl, userItem.transform.Find("ProfileImg").GetComponent<RawImage>()));
                                    }
                                });
                        }
                    });
            });
    }

    IEnumerator LoadImage(string url, RawImage img)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            img.texture = DownloadHandlerTexture.GetContent(request);
        }
        else
        {
            Debug.LogWarning("No se pudo cargar la imagen: " + url);
        }
    }
}
