using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonManager : MonoBehaviour
{
    public void btnMultiplayer_OnClick()
    {
        SaveController.newGame = true;
        SceneManager.LoadScene("NetworkLobby");
    }

    public void Load_OnClick()
    {
        SaveController.newGame = false;
        SceneManager.LoadScene("LoadingScene");
    }

    public void btnExitGameQuery_OnClick()
    {
        GameObject.Find("MenuMain").transform.GetChild(2).gameObject.SetActive(false);
        GameObject.Find("MenuMain").transform.GetChild(3).gameObject.SetActive(true);
    }

    public void CancelQuery_OnClick()
    {
        GameObject.Find("MenuMain").transform.GetChild(2).gameObject.SetActive(true);
        GameObject.Find("MenuMain").transform.GetChild(3).gameObject.SetActive(false);
    }

    public void btnOpenOptionsMenu() 
    {
		GameObject.Find("Canvas").transform.GetChild(0).gameObject.SetActive(false);
		GameObject.Find("Canvas").transform.GetChild(1).gameObject.SetActive(true);
	}

	public void btnCloseOptionsMenu()
	{
		GameObject.Find("Canvas").transform.GetChild(0).gameObject.SetActive(true);
		GameObject.Find("Canvas").transform.GetChild(1).gameObject.SetActive(false);
	}

    public void btnExitGame_OnClick()
    {
        Application.Quit();
    }
}
