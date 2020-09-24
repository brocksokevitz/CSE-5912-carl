using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NotificationManager : NetworkBehaviour {

    [SerializeField]
    private GameObject notificationItemPrefab;

    private float notificationTime = 3f; // The amount of time (in seconds) to show the notification before making it disappear
    private float messageTime = 120f; // The amount of time (in seconds) to show the messages before making it disappear
    private GameObject notificationItem;
    private GameObject notificationPanel;

    private void Start()
    {
        notificationPanel = gameObject;
        notificationItem = notificationItemPrefab;
    }

    

    public void NewNotification(string message, bool isMessage)
    {
        if (notificationItem != null && notificationPanel != null)
        {
            if (notificationPanel.transform.childCount > 9)
            {
                Destroy(notificationPanel.transform.GetChild(0).gameObject);
            }
            GameObject newObj = Instantiate(notificationItem, notificationPanel.transform);
            newObj.GetComponent<NotificationItem>().Setup(message);
            Destroy(newObj, isMessage ? messageTime : notificationTime);
        }
        else
        {
            if (notificationItem == null)
                Debug.LogError("Can't create notification: static notificationItem GameObject is NULL");
        }
    }

    public static string GetRedString(string msg)
    {
        return "<color=#FF0000>" + msg + "</color>";
    }

    public static string GetBlueString(string msg)
    {
        return "<color=#0099FF>" + msg + "</color>";
    }

    public static string GetColoredString(Color clr, string msg)
    {
        return "<color=#" + ColorUtility.ToHtmlStringRGB(clr)  + ">" + msg + "</color>";
    }
}
