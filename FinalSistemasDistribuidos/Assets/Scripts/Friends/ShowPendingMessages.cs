using TMPro;
using UnityEngine;

public class ShowPendingMessages : MonoBehaviour
{
    [SerializeField] private Transform messageContainer;
    [SerializeField] private GameObject notification;
    [SerializeField] private GameObject emptyMailbox;

    private TextMeshProUGUI textNoti;

    private void Start()
    {
        textNoti = notification.GetComponentInChildren<TextMeshProUGUI>();    
    }

    private void Update()
    {
        if (messageContainer != null)
        {
            int amountNotis = messageContainer.childCount;

            textNoti.text = amountNotis.ToString();

            if(amountNotis > 0)
            {
                emptyMailbox.SetActive(false);
                notification.SetActive(true);
            }
            else 
            {
                notification.SetActive(false);
                emptyMailbox.SetActive(true);
            }
        }
    }
}
