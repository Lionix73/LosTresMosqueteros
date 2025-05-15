using Firebase.Auth;
using Firebase.Database;
using System.Collections;
using UnityEngine;

public class ChangeCharacterModel : MonoBehaviour
{
    private AuthStateHandler authHandler;
    private string userId;

    private void OnEnable()
    {
        authHandler = GameObject.Find("AuthStateHandler").GetComponent<AuthStateHandler>();
    }

    public void SelectCharacter(int index)
    {
        userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        StartCoroutine(SetCharacter(index, userId));
    }

    public IEnumerator SetCharacter(int index, string id)
    {
        switch (index)
        {
            case 0:
                FirebaseDatabase.DefaultInstance.RootReference.Child("users").Child(id).Child("characterId").SetValueAsync("nyx");
                break;
            case 1:
                FirebaseDatabase.DefaultInstance.RootReference.Child("users").Child(id).Child("characterId").SetValueAsync("umbra");
                break;
            case 2:
                FirebaseDatabase.DefaultInstance.RootReference.Child("users").Child(id).Child("characterId").SetValueAsync("npc");
                break;
            default:
                FirebaseDatabase.DefaultInstance.RootReference.Child("users").Child(id).Child("characterId").SetValueAsync("npc");
                break;
        }

        ChangeModel();
        yield return null;
    }

    private void ChangeModel()
    {
        GameObject model = GameObject.FindGameObjectWithTag("Player");
        Destroy(model);

        authHandler.GetCharacterModel(userId);
    }
}
