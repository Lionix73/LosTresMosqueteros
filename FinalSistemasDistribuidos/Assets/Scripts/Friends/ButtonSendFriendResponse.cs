using Firebase.Auth;
using Firebase.Database;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum FriendResponse
{
    Accept = 1,
    Reject = 2
}
public class ButtonSendFriendResponse : MonoBehaviour
{

    [SerializeField]
    private Button _sendResponseButton;
    [SerializeField]
    private TMP_InputField _friendUserIdInputField;

    private void Reset()

    {
        _sendResponseButton = GetComponent<Button>();
        _friendUserIdInputField = GameObject.Find("InputFieldFriendUserId").GetComponent<TMP_InputField>();

    }
    void Start()
    {
        _sendResponseButton.onClick.AddListener(HandleSendRsponseButtonClicked);

    }

    async private void HandleSendRsponseButtonClicked()
    {
        string friendUserId = _friendUserIdInputField.text;

        var mDatabaseRef = FirebaseDatabase.DefaultInstance.RootReference;
        var userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        string friendUsermane = (await FirebaseDatabase.DefaultInstance
                                        .GetReference("users/" + friendUserId + "/username")
                                        .GetValueAsync()).Value?.ToString();


        //Respuesta estatica que acepta la solicitud de amistad (estado 1)
        await mDatabaseRef.Child("users")
                    .Child(friendUserId)
                    .Child("friendResponse")
                    .Child(userId)
                    .SetValueAsync(1);

        // TODO: asegurarse de que el proceso de arriba(linea 46) si se completo
        await mDatabaseRef.Child("users").Child(userId).Child("friends").Child(friendUserId).SetValueAsync(friendUsermane);
    }
}