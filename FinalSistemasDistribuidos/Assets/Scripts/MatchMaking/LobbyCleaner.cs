using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;

public class LobbyCleaner : MonoBehaviour
{
    private void Start()
    {
        InvokeRepeating("CleanEmptyLobbies", 1f, 60f);
    }

    public void CleanEmptyLobbies()
    {
        DatabaseReference lobbiesRef = FirebaseDatabase.DefaultInstance.GetReference("lobbies");

        lobbiesRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || !task.Result.Exists)
            {
                Debug.LogWarning("Error retrieving lobbies or no lobbies found.");
                return;
            }

            foreach (var lobbySnapshot in task.Result.Children)
            {
                string lobbyId = lobbySnapshot.Key;
                var members = lobbySnapshot.Child("members");

                // Si no hay miembros, eliminar el lobby
                if (!members.HasChildren)
                {
                    Debug.Log($"Deleting empty lobby: {lobbyId}");
                    lobbiesRef.Child(lobbyId).RemoveValueAsync().ContinueWithOnMainThread(deleteTask =>
                    {
                        if (deleteTask.IsCompleted)
                        {
                            Debug.Log($"Lobby {lobbyId} deleted.");
                        }
                        else
                        {
                            Debug.LogWarning($"Failed to delete lobby {lobbyId}: {deleteTask.Exception}");
                        }
                    });
                }
            }
        });
    }
}
