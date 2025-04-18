using Firebase.Auth;
using System;
using UnityEngine;

public class AuthStateHandler : MonoBehaviour
{
    [SerializeField] GameObject panelAuth;
    [SerializeField] GameObject panelScore;

    private void Reset()
    {
        panelAuth = GameObject.Find("PanelAuth");    
        panelScore = GameObject.Find("PanelScore");  
    }

    void Start()
    {
        FirebaseAuth.DefaultInstance.StateChanged += HandleStateChange;

        panelScore.SetActive(false);
    }

    private void HandleStateChange(object sender, EventArgs e)
    {
        if(FirebaseAuth.DefaultInstance.CurrentUser != null)
        {
            Invoke("SetAuth", 2f);
        }
        else
        {
            panelAuth.SetActive(true);
            panelScore.SetActive(false);
        }
    }

    private void SetAuth()
    {
        panelAuth.SetActive(false);
        panelScore.SetActive(true);
        Debug.Log($"{FirebaseAuth.DefaultInstance.CurrentUser.Email}");
    }
}
