using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class AuthStateHandler : MonoBehaviour
{
    [SerializeField] GameObject panelLogin;
    [SerializeField] GameObject panelLobby;
    [SerializeField] Transform spawnPoint;
    [SerializeField] private Transform charactersInGame;
    [SerializeField] private GameObject PopUp;
    [SerializeField] private LobbyStatusChecker lobbyStatus;
    private CharacterLibrary characterLibrary;

    private void Reset()
    {
        panelLogin = GameObject.Find("LogIn");    
        panelLobby = GameObject.Find("Lobby");  
    }

    void Start()
    {
        characterLibrary = GetComponent<CharacterLibrary>();

        FirebaseAuth.DefaultInstance.StateChanged += HandleStateChange;

        panelLobby.SetActive(false);
    }

    private void HandleStateChange(object sender, EventArgs e)
    {
        if(FirebaseAuth.DefaultInstance.CurrentUser != null)
        {
            StartCoroutine(SetAuth());
            SetUserOnline();
        }
        else
        {
            panelLogin.SetActive(true);
            panelLobby.SetActive(false);
        }
    }

    private IEnumerator SetAuth()
    {
        PopUp.SetActive(true);
        PopUp.GetComponentInChildren<TextMeshProUGUI>().text = "¡HURRA! Te has loggeado";
        yield return new WaitForSeconds(2f);
        panelLogin.SetActive(false);
        panelLobby.SetActive(true);
        lobbyStatus.CheckIfInLobby();
    }

    private void SetUserOnline()
    {
        var mDatabaseRef = FirebaseDatabase.DefaultInstance.RootReference;
        string userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;

        FirebaseDatabase.DefaultInstance
          .GetReference("users/" + userId + "/username")
          .GetValueAsync().ContinueWithOnMainThread(task =>
          {
                if (task.IsFaulted)
                {
                    // Handle the error...
                }
                else if (task.IsCompleted)
                {
                    DataSnapshot snapshot = task.Result;
                    string username = snapshot.Value.ToString();
                    
                    mDatabaseRef.Child("users-online").Child(userId).SetValueAsync(username);
                    GetCharacterModel(userId);

                    FirebaseUser user = FirebaseAuth.DefaultInstance.CurrentUser;

                    if (user != null)
                    {
                        UserProfile profile = new UserProfile
                        {
                            DisplayName = username
                        };

                        user.UpdateUserProfileAsync(profile).ContinueWith(task =>
                        {
                            if (task.IsCanceled)
                            {
                                Debug.LogError("UpdateUserProfileAsync fue cancelado.");
                                return;
                            }
                            if (task.IsFaulted)
                            {
                                Debug.LogError("Error al actualizar el perfil: " + task.Exception);
                                return;
                            }

                            Debug.Log("Nombre actualizado exitosamente a: " + FirebaseAuth.DefaultInstance.CurrentUser.DisplayName);
                        });
                    }
                }
          });
    }

    public void GetCharacterModel(string userID)
    {
        FirebaseDatabase.DefaultInstance.RootReference
        .Child("users")
        .Child(userID)
        .GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                string characterId = snapshot.Child("characterId").Value.ToString();
                GameObject characterPrefab = characterLibrary.GetCharacterPrefab(characterId);
                Instantiate(characterPrefab, spawnPoint.position, new Quaternion(0, -0.39f, 0, 0.91f), charactersInGame);
            }
        });
    }
}
