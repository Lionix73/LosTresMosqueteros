using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using JetBrains.Annotations;
using System;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class AuthStateHandler : MonoBehaviour
{
    [SerializeField] GameObject panelLogin;
    [SerializeField] GameObject panelLobby;
    [SerializeField] Transform spawnPoint;
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
            Invoke("SetAuth", 2f);
            SetUserOnline();
        }
        else
        {
            panelLogin.SetActive(true);
            panelLobby.SetActive(false);
        }
    }

    private void SetAuth()
    {
        panelLogin.SetActive(false);
        panelLobby.SetActive(true);
        Debug.Log($"{FirebaseAuth.DefaultInstance.CurrentUser.Email}");
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
                Instantiate(characterPrefab, spawnPoint.position, new Quaternion(0, -0.39f, 0, 0.91f));
            }
        });
    }
}
