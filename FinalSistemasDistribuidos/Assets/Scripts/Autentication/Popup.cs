using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Popup : MonoBehaviour
{
    private void OnEnable()
    {
        Invoke("HidePopUp", 2f);
    }

    private void HidePopUp()
    {
        gameObject.SetActive(false);
    }
}
