using Firebase.Auth;
using Firebase.Database;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ButtonRegister : MonoBehaviour
{
    [SerializeField] private GameObject registerPanel;
    [SerializeField] private GameObject popUpFail;
    [SerializeField] private GameObject popUpSuccess;
    [SerializeField] private Button registerButton;
    [SerializeField] private TMP_InputField emailInput;
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TMP_InputField passwordInput;

    private string photoUrl = "https://pbs.twimg.com/profile_images/1864040171760427008/oP7sr5jA_400x400.jpg";
    private string popUpMsg = "Error";
    private bool fail = true;

    private void Reset()
    {
        registerButton = GetComponent<Button>();
    }

    public void HandleRegistrationButtonClick()
    {
        string email = emailInput.text;
        string password = passwordInput.text;

        StartCoroutine(RegisterUser(email, password));
    }

    IEnumerator RegisterUser(string email, string password)
    {
        string username = usernameInput.text;
        var auth = FirebaseAuth.DefaultInstance;
        var registerTask = auth.CreateUserWithEmailAndPasswordAsync(email, password);

        yield return new WaitUntil(() => registerTask.IsCompleted);

        if (registerTask.IsCanceled)
        {
            Debug.LogError("Register was canceled.");
            popUpMsg = "Register was canceled.";
            fail = true;
            yield return null;
        }
        else if(registerTask.IsFaulted) 
        {
            string[] errorType = registerTask.Exception.Message.Split("(");
            errorType[1] = errorType[1].Replace(")", "");
            popUpMsg = errorType[1];
            fail = true;

            Debug.LogError("Register encountered an error: " + popUpMsg);
            yield return null;
        }
        else
        {
            AuthResult result = registerTask.Result;
            FirebaseDatabase.DefaultInstance.RootReference.Child("users").Child(result.User.UserId).Child("username").SetValueAsync(username);
            FirebaseDatabase.DefaultInstance.RootReference.Child("users").Child(result.User.UserId).Child("characterId").SetValueAsync("npc");
            FirebaseDatabase.DefaultInstance.RootReference.Child("users").Child(result.User.UserId).Child("level").SetValueAsync(1);
            FirebaseDatabase.DefaultInstance.RootReference.Child("users").Child(result.User.UserId).Child("photoUrl").SetValueAsync(photoUrl);

            Debug.Log($"usuario creado: {result.User.DisplayName}, {result.User.UserId}");
            fail = false;
            popUpMsg = "¡HURRA! Te has registrado exitosamente";
            registerPanel.SetActive(false);
        }
    }

    private void Start()
    {
        registerButton.onClick.AddListener(HandleRegistrationButtonClick);
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
