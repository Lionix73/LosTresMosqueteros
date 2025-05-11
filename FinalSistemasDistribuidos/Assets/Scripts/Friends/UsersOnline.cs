using Firebase.Auth;
using Firebase.Database;
using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.GPUSort;

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
        Debug.Log(snapshot.Value + " se ha conectado");

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
}