using UnityEngine;
using Firebase.Database;
using Firebase.Auth;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;

public class FriendsDisplay : MonoBehaviour
{
    public Transform friendsContainer;
    public GameObject friendCardPrefab;

    private DatabaseReference dbRef;
    private string currentUserId;

    void Start()
    {
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;
        currentUserId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
    }

    public void ShowFriends()
    {
        // Limpiar contenedor
        foreach (Transform child in friendsContainer)
            Destroy(child.gameObject);

        // Leer la lista de amigos del usuario actual
        dbRef.Child("users").Child(currentUserId).Child("friends").GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                Dictionary<string, object> friendsDict = (Dictionary<string, object>)task.Result.Value;
                List<string> onlineIds = new List<string>();

                // Leer la lista de users-online
                dbRef.Child("users-online").GetValueAsync().ContinueWith(onlineTask =>
                {
                    if (onlineTask.IsCompleted)
                    {
                        DataSnapshot onlineSnapshot = onlineTask.Result;
                        foreach (var child in onlineSnapshot.Children)
                            onlineIds.Add(child.Key);

                        foreach (var friend in friendsDict)
                        {
                            string friendId = friend.Key;

                            // Traer información del amigo
                            dbRef.Child("users").Child(friendId).GetValueAsync().ContinueWith(userTask =>
                            {
                                if (userTask.IsFaulted || userTask.IsCanceled)
                                {
                                    Debug.LogWarning("Error cargando usuario " + friendId + ": " + userTask.Exception);
                                    return;
                                }

                                DataSnapshot userSnap = userTask.Result;

                                if (userSnap.Exists)
                                {
                                    string username = userSnap.Child("username").Value?.ToString() ?? "Desconocido";
                                    int level = userSnap.HasChild("level") ? int.Parse(userSnap.Child("level").Value.ToString()) : 1;
                                    string photoUrl = userSnap.HasChild("photoUrl") ? userSnap.Child("photoUrl").Value.ToString() : "";

                                    GameObject userItem = Instantiate(friendCardPrefab, friendsContainer);
                                    //userItem.transform.localScale = Vector3.one;

                                    userItem.transform.Find("Username").GetComponent<TextMeshProUGUI>().text = username;
                                    userItem.transform.Find("Level").GetComponent<TextMeshProUGUI>().text = "Level " + level;

                                    if (!string.IsNullOrEmpty(photoUrl))
                                        StartCoroutine(LoadImage(photoUrl, userItem.transform.Find("ProfileImg").GetComponent<RawImage>()));
                                }
                            });
                        }
                    }
                });
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
