using Firebase.Auth;
using Firebase.Database;
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
        if(FirebaseAuth.DefaultInstance.CurrentUser != null)
        {
            var mDatabaseRef = FirebaseDatabase.DefaultInstance.RootReference;
            var userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
            
            // comentado con fines de test
            //mDatabaseRef.Child("users-online").Child(userId).SetValueAsync(null);
        }
    }
}