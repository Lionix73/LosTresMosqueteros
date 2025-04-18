using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using System;
using TMPro;
using UnityEngine;

public class LabelUsername : MonoBehaviour
{
    [SerializeField] private TMP_Text _label;

    private void Reset()
    {
        _label = GetComponent<TMP_Text>();
    }
    
    void Start()
    {
        FirebaseAuth.DefaultInstance.StateChanged += HandleStateChange;
    }

    private void HandleStateChange(object sender, EventArgs e)
    {
        if (FirebaseAuth.DefaultInstance.CurrentUser != null)
        {
            SetUsername();
        }
    }

    private void SetUsername()
    {
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
                  _label.text = snapshot.Value.ToString();
                  // Do something with snapshot...
              }
          });
    }
}
