using TMPro;
using UnityEngine;

public class ShowPendingMessages : MonoBehaviour
{
    [SerializeField] private Transform messageContainer;
    [SerializeField] private GameObject notificacion;

    private TextMeshProUGUI textNoti;

    private void Start()
    {
        textNoti = notificacion.GetComponentInChildren<TextMeshProUGUI>();    
    }

    private void Update()
    {
        if (messageContainer != null)
        {
            int amountNotis = messageContainer.childCount;

            textNoti.text = amountNotis.ToString();

            if(amountNotis > 0)
            {
                notificacion.SetActive(true);
            }
            else { notificacion.SetActive(false); }
        }
    }
}
