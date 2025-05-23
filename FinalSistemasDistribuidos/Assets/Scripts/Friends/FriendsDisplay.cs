using Firebase.Auth;
using Firebase.Database;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine;
using System.Collections;
using Firebase.Extensions;
using TMPro;

public class FriendsDisplay : MonoBehaviour
{
    public GameObject friendItemPrefab;
    public Transform contentPanel;

    private DatabaseReference dbRef;
    private string currentUserId;

    void Start()
    {
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;
    }

    void OnEnable()
    {
        Invoke("ShowFriends", 1f);
    }

    public void ShowFriends()
    {
        currentUserId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;

        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }

        // Paso 1: Obtener lista de amigos
        FirebaseDatabase.DefaultInstance.GetReference("users").Child(currentUserId).Child("friends")
            .GetValueAsync().ContinueWithOnMainThread(friendsTask =>
            {
                if (friendsTask.IsCompleted && friendsTask.Result.Exists)
                {
                    foreach (var friend in friendsTask.Result.Children)
                    {
                        DataSnapshot snapshot = friendsTask.Result;

                        foreach (var child in snapshot.Children)
                        {
                            string userId = child.Key;

                            if (userId == currentUserId) continue; // Saltar a uno mismo

                            // Paso 3: Cargar datos del usuario
                            FirebaseDatabase.DefaultInstance
                                .GetReference("users").Child(userId)
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
                                        string photoUrl = userSnap.HasChild("photoUrl") ? userSnap.Child("photoUrl").Value.ToString() : "";

                                        GameObject userItem = Instantiate(friendItemPrefab, contentPanel);
                                        userItem.transform.Find("Username").GetComponent<TextMeshProUGUI>().text = username;

                                        if (!string.IsNullOrEmpty(photoUrl))
                                            StartCoroutine(LoadImage(photoUrl, userItem.transform.Find("ProfileImg").GetComponent<RawImage>()));
                                    }
                                });
                        }
                    }
                }
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
