using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine.Networking;
using Firebase.Auth;

public class UserListManager : MonoBehaviour
{
    public GameObject userItemPrefab;
    public Transform contentPanel;

    void OnEnable()
    {
        Invoke("LoadUserListFromFirebase", 1f);
    }

    public void LoadUserListFromFirebase()
    {
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject); // Limpia el panel primero
        }

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

                    if (FirebaseAuth.DefaultInstance.CurrentUser.UserId == userId) continue;

                    // Obtener los datos del usuario
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
                                //userItem.transform.localScale = Vector3.one;

                                userItem.transform.Find("Username").GetComponent<TextMeshProUGUI>().text = username;
                                userItem.transform.Find("Level").GetComponent<TextMeshProUGUI>().text = "Level " + level;

                                if (!string.IsNullOrEmpty(photoUrl))
                                    StartCoroutine(LoadImage(photoUrl, userItem.transform.Find("ProfileImg").GetComponent<RawImage>()));
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
