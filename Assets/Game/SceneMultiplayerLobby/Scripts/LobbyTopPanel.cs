using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Prototype.NetworkLobby
{
    public class LobbyTopPanel : MonoBehaviour
    {
        public bool isInGame = false;

        protected bool isDisplayed = true;
        protected Image panelImage;

        void Start()
        {
            panelImage = GetComponent<Image>();
        }


        void Update()
        {
            if (!isInGame)
                return;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ToggleVisibility(!isDisplayed);
            }

        }

        public void ToggleVisibility(bool visible)
        {
            isDisplayed = visible;
            foreach (Transform t in transform)
            {
                t.gameObject.SetActive(isDisplayed);
            }
            if (visible)
            {
                Debug.Log("Un-Locking the cursor...");
                // Lock the cursor to the window
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Debug.Log("Locking the cursor...");
                // Lock the cursor to the window
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            if (panelImage != null)
            {
                panelImage.enabled = isDisplayed;
            }
        }
    }
}