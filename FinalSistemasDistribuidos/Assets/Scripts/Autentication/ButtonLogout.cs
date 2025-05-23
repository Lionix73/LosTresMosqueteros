using Firebase.Auth;
using Firebase.Database;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonLogout : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        var mDatabaseRef = FirebaseDatabase.DefaultInstance.RootReference;
        var userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        //mDatabaseRef.Child("users-online").Child(userId).SetValueAsync(null);
        //mDatabaseRef.Child("users").Child(userId).Child("currentLobby").SetValueAsync(null);

        FirebaseAuth.DefaultInstance.SignOut();

        // Borrar modelos de personajes de la escena
        GameObject[] models = GameObject.FindGameObjectsWithTag("Player");

        foreach (var model in models)
        {
            Destroy(model);
        }
    }
}
