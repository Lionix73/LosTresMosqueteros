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
    [SerializeField] private Button registerButton;
    [SerializeField] private TMP_InputField emailInput;
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TMP_InputField passwordInput;

    private string photoUrl = "https://pbs.twimg.com/profile_images/1864040171760427008/oP7sr5jA_400x400.jpg";

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

        if (registerTask.IsCanceled || registerTask.IsFaulted) 
        {
            Debug.Log("algo salio mal");
        }
        else
        {
            AuthResult result = registerTask.Result;
            FirebaseDatabase.DefaultInstance.RootReference.Child("users").Child(result.User.UserId).Child("username").SetValueAsync(username);
            FirebaseDatabase.DefaultInstance.RootReference.Child("users").Child(result.User.UserId).Child("characterId").SetValueAsync("npc");
            FirebaseDatabase.DefaultInstance.RootReference.Child("users").Child(result.User.UserId).Child("level").SetValueAsync(1);
            FirebaseDatabase.DefaultInstance.RootReference.Child("users").Child(result.User.UserId).Child("photoUrl").SetValueAsync(photoUrl);

            Debug.Log($"usuario creado: {result.User.DisplayName}, {result.User.UserId}");
            registerPanel.SetActive(false);
        }
    }

    private void Start()
    {
        registerButton.onClick.AddListener(HandleRegistrationButtonClick);
    }
}
