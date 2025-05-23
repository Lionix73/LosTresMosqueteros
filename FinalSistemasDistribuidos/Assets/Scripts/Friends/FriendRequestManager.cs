using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase.Database;
using Firebase.Auth;
using System.Collections.Generic;

public class FriendRequestManager : MonoBehaviour
{
    public GameObject friendRequestCardPrefab;
    public Transform contentParent;

    private string currentUserId;

    private void Start()
    {
        InvokeRepeating("LoadFriendRequests", 1f, 2f);
    }

    private void LoadFriendRequests()
    {
        currentUserId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        FirebaseDatabase.DefaultInstance.GetReference("users")
            .Child(currentUserId)
            .Child("friendRequest")
            .ValueChanged += (sender, args) =>
            {
                // Limpia las tarjetas existentes
                foreach (Transform child in contentParent)
                {
                    Destroy(child.gameObject);
                }

                if (args.DatabaseError != null || args.Snapshot == null || !args.Snapshot.Exists) return;

                foreach (var child in args.Snapshot.Children)
                {
                    string requesterId = child.Key;
                    string requesterName = child.Value.ToString();

                    GameObject card = Instantiate(friendRequestCardPrefab, contentParent);
                    FriendRequestCard cardScript = card.GetComponent<FriendRequestCard>();
                    cardScript.Setup(requesterId, requesterName);
                }
            };
    }
}
