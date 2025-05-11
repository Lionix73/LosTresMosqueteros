using Firebase.Auth;
using Firebase.Database;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ButtonRegister : MonoBehaviour
{
    FirebaseAuth auth;

    [SerializeField] private Button registerButton;
    private Coroutine _registerCoroutine;

    private void Reset()
    {
        registerButton = GetComponent<Button>();
    }

    public void HandleRegistrationButtonClick()
    {
        string email = GameObject.Find("InputFieldMail").GetComponent<TMP_InputField>().text;
        string password = GameObject.Find("InputFieldPassword").GetComponent<TMP_InputField>().text;

        StartCoroutine(RegisterUser(email, password));
    }

    IEnumerator RegisterUser(string email, string password)
    {
        string username = GameObject.Find("InputFieldUsername").GetComponent<TMP_InputField>().text;
        var auth = FirebaseAuth.DefaultInstance;
        var registerTask = auth.CreateUserWithEmailAndPasswordAsync(email, password);

        yield return new WaitUntil(() => registerTask.IsCompleted);

        if (registerTask.IsCanceled || registerTask.IsFaulted) 
        {
            Debug.Log("algo salio mal");
        }
        else
        {
            AuthResult result = registerTask.Result;
            FirebaseDatabase.DefaultInstance.RootReference.Child("users").Child(result.User.UserId).Child("username").SetValueAsync(username);

            Debug.Log($"usuario creado: {result.User.DisplayName}, {result.User.UserId}");
        }
    }

    private void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        registerButton.onClick.AddListener(HandleRegistrationButtonClick);
    }
}
