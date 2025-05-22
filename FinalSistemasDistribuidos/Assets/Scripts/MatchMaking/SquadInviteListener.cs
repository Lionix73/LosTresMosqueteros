using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Auth;
using Firebase.Extensions;
using TMPro;
using UnityEngine.UI;

public class SquadInviteListener : MonoBehaviour
{
    public GameObject squadInviteCardPrefab;
    public Transform invitePanel;

    private DatabaseReference dbRef;
    private FirebaseUser currentUser;
    private DatabaseReference invitesRef;
    private LobbyVisualizer lobbyVisualizer;

    void Start()
    {
        currentUser = FirebaseAuth.DefaultInstance.CurrentUser;
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;
        lobbyVisualizer = FindFirstObjectByType<LobbyVisualizer>();
    }

    private void OnEnable()
    {
        Invoke("ListenForSquadInvites", 1f);
    }

    void ListenForSquadInvites()
    {
        invitesRef = dbRef.Child("users").Child(currentUser.UserId).Child("invitations");
        invitesRef.ChildAdded += HandleNewSquadInvite;
    }

    void HandleNewSquadInvite(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError("Error al recibir invitación: " + args.DatabaseError.Message);
            return;
        }

        string senderId = args.Snapshot.Key;

        if (!args.Snapshot.HasChild("lobbyId") || !args.Snapshot.HasChild("username"))
        {
            Debug.LogWarning($"Invitación incompleta de {senderId}");
            return;
        }

        string lobbyId = args.Snapshot.Child("lobbyId").Value.ToString();
        string username = args.Snapshot.Child("username").Value.ToString();

        GameObject card = Instantiate(squadInviteCardPrefab, invitePanel);
        card.GetComponentInChildren<TextMeshProUGUI>().text = $"{username} te invitó a su escuadrón";

        Button acceptBtn = card.transform.Find("AcceptButton").GetComponent<Button>();
        Button rejectBtn = card.transform.Find("RejectButton").GetComponent<Button>();

        acceptBtn.onClick.AddListener(() =>
        {
            AcceptInvite(lobbyId, senderId, card);
        });

        rejectBtn.onClick.AddListener(() =>
        {
            RejectInvite(senderId, card);
        });
    }

    void AcceptInvite(string lobbyId, string senderId, GameObject card)
    {
        string userId = currentUser.UserId;

        dbRef.Child("lobbies").Child(lobbyId)
            .Child("members").Child(userId)
            .SetValueAsync(FirebaseAuth.DefaultInstance.CurrentUser.DisplayName)
            .ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Te uniste al lobby: " + lobbyId);
                RemoveInvite(senderId, card);
                lobbyVisualizer.VisualizeLobby(lobbyId);
            }
            else
            {
                Debug.LogError("Error al unirse al lobby: " + task.Exception);
            }
        });
    }

    void RejectInvite(string senderId, GameObject card)
    {
        RemoveInvite(senderId, card);
    }

    void RemoveInvite(string senderId, GameObject card)
    {
        invitesRef.Child(senderId).RemoveValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Destroy(card);
            }
        });
    }

    void OnDestroy()
    {
        if (invitesRef != null)
        {
            invitesRef.ChildAdded -= HandleNewSquadInvite;
        }
    }
}
