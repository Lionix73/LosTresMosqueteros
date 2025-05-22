using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using TMPro;
using UnityEngine;

public class FriendLabel : MonoBehaviour
{
    [SerializeField] private TMP_Text _label;

    private LobbyVisualizer lobbyInfo;

    private void Awake()
    {
        lobbyInfo = FindFirstObjectByType<LobbyVisualizer>();
    }

    private void Reset()
    {
        _label = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        Invoke("SetUsername", 1f);
    }

    private void SetUsername()
    {
        var userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        string id;
        
        if (userId == lobbyInfo.ids[0])
        {
            id = lobbyInfo.ids[1];
        }
        else
            id = lobbyInfo.ids[0];

        FirebaseDatabase.DefaultInstance
          .GetReference("users/" + id + "/username")
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
                  _label.text = username;

                  lobbyInfo.ids.Remove(username);
              }
          });
    }
}
