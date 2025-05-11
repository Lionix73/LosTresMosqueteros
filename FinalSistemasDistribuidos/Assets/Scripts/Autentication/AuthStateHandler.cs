using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using System;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class AuthStateHandler : MonoBehaviour
{
    [SerializeField] GameObject panelAuth;
    [SerializeField] GameObject panelScore;

    private void Reset()
    {
        panelAuth = GameObject.Find("PanelAuth");    
        panelScore = GameObject.Find("PanelScore");  
    }

    void Start()
    {
        FirebaseAuth.DefaultInstance.StateChanged += HandleStateChange;

        panelScore.SetActive(false);
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
            panelAuth.SetActive(true);
            panelScore.SetActive(false);
        }
    }

    private void SetAuth()
    {
        panelAuth.SetActive(false);
        panelScore.SetActive(true);
        Debug.Log($"{FirebaseAuth.DefaultInstance.CurrentUser.Email}");
    }

    private void SetUserOnline()
    {
        var mDatabaseRef = FirebaseDatabase.DefaultInstance.RootReference;
        var userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;

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
                PlayerPrefs.SetString("username", username);
                mDatabaseRef.Child("users-online").Child(userId).SetValueAsync(username);
                }
          });

    }
}
