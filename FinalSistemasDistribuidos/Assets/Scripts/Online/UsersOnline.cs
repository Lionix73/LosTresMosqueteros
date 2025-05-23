using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;

public class UsersOnline : MonoBehaviour
{
    void Start()
    {
        var reference = FirebaseDatabase.DefaultInstance.GetReference("users-online");
        reference.ChildAdded += HandleChildAdded;
        reference.ChildRemoved += HandleChildRemoved;

    }
    private void HandleChildAdded(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            //Debug.LogError(args.DatabaseError.Message);
            Debug.Log("Error");
            return;
        }

        DataSnapshot snapshot = args.Snapshot;
        //Debug.Log(snapshot.Value + " se ha conectado");
    }

    private void HandleChildRemoved(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            //Debug.LogError(args.DatabaseError.Message);
            Debug.Log("Error");
            return;
        }

        DataSnapshot snapshot = args.Snapshot;

        Debug.Log(snapshot.Value + " se ha desconectado");
    }

    private void OnApplicationQuit()
    {
        if (FirebaseAuth.DefaultInstance.CurrentUser != null)
        {
            var mDatabaseRef = FirebaseDatabase.DefaultInstance.RootReference;
            var userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;

            // Comentado con fines de test
            //mDatabaseRef.Child("users-online").Child(userId).SetValueAsync(null);
            //RemoveUserFromLobbyOnLogout();
        }
    }

    public void RemoveUserFromLobbyOnLogout()
    {
        string uid = FirebaseAuth.DefaultInstance.CurrentUser?.UserId;
        if (string.IsNullOrEmpty(uid))
        {
            Debug.LogWarning("No user is logged in.");
            return;
        }

        DatabaseReference userRef = FirebaseDatabase.DefaultInstance.GetReference("users").Child(uid);
        userRef.Child("currentLobby").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || !task.Result.Exists)
            {
                Debug.Log("User was not in any lobby.");
                return;
            }

            string lobbyId = task.Result.Value.ToString();
            if (string.IsNullOrEmpty(lobbyId))
            {
                Debug.Log("currentLobby is empty.");
                return;
            }

            // Remove user from members list
            DatabaseReference lobbyRef = FirebaseDatabase.DefaultInstance.GetReference("lobbies").Child(lobbyId).Child("members").Child(uid);
            lobbyRef.RemoveValueAsync().ContinueWithOnMainThread(removeTask =>
            {
                if (removeTask.IsCompleted)
                {
                    Debug.Log("User removed from lobby members.");
                }
                else
                {
                    Debug.LogWarning("Failed to remove user from lobby members: " + removeTask.Exception);
                }
            });

            // Optionally clear currentLobby in user's node
            userRef.Child("currentLobby").RemoveValueAsync();
        });
    }
}