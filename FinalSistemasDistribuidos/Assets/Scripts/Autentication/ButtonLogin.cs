using Firebase.Auth;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ButtonLogin : MonoBehaviour
{
    [SerializeField] private GameObject popUpFail;
    [SerializeField] private GameObject popUpSuccess;
    [SerializeField] private Button loginButton;
    [SerializeField] private TMP_InputField emailInput;
    [SerializeField] private TMP_InputField passwordInput;

    string popUpMsg = "Error";
    bool fail = true;

    private void Reset()
    {
        loginButton = GetComponent<Button>();
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
                popUpMsg = "SignIn was canceled.";
                fail = true;
                return;
            }
            if (task.IsFaulted)
            {
                string[] errorType = task.Exception.Message.Split("(");
                errorType[1] = errorType[1].Replace(")", "");
                popUpMsg = errorType[1];
                fail = true;

                Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: " + popUpMsg);
                return;
            }

            fail = false;
            popUpMsg = "¡HURRA! Te has loggeado exitosamente";

            AuthResult result = task.Result;
        });
    }

    public void ShowPopUp()
    {
        Invoke("ActivatePopUps", 0.5f);
    }

    private void ActivatePopUps()
    {
        if (fail)
        {
            popUpFail.SetActive(true);
            popUpFail.GetComponentInChildren<TextMeshProUGUI>().text = popUpMsg;
        }
        else
        {
            popUpSuccess.SetActive(true);
            popUpSuccess.GetComponentInChildren<TextMeshProUGUI>().text = popUpMsg;

            passwordInput.text = "";
            emailInput.text = "";
        }
    }
}
