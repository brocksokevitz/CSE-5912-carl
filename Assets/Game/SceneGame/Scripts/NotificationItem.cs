using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NotificationItem : MonoBehaviour {
    [SerializeField]
    Text notificationMsg;

    public void Setup(string msg)
    {
        notificationMsg.text = msg;
    }
}
