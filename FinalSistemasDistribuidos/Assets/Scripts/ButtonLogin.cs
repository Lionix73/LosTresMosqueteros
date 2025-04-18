using Firebase.Auth;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ButtonLogin : MonoBehaviour
{
    FirebaseAuth auth;
    [SerializeField] private Button loginButton;
    [SerializeField] private TMP_InputField emailInput;
    [SerializeField] private TMP_InputField passwordInput;

    private void Reset()
    {
        loginButton = GetComponent<Button>();

        emailInput = GameObject.Find("InputFieldMail").GetComponent<TMP_InputField>();
        passwordInput = GameObject.Find("InputFieldPassword").GetComponent<TMP_InputField>();
    }

    private void Start()
    {
        loginButton.onClick.AddListener(LoginUser);
    }

    private void LoginUser()
    {
        var auth = FirebaseAuth.DefaultInstance;
        string email = emailInput.text;
        string password = passwordInput.text;

        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task => {
            if (task.IsCanceled)
            {
                Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                return;
            }

            Firebase.Auth.AuthResult result = task.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})",
                result.User.DisplayName, result.User.UserId);
        });
    }
}
