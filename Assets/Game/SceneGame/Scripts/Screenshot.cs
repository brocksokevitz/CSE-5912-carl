using UnityEngine;
using System.Collections;
using System;
using System.IO;

public class Screenshot : MonoBehaviour
{
    int resWidth = Screen.width;
    int resHeight = Screen.height;
    public static string filepath = System.Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    public static string screenshots = filepath + "\\Pong\\Screenshots";

    private bool takeScreenshot = false;

    public static string ScreenShotName(int width, int height)
    {
        return screenshots + "\\screen_"+ width+"x" + height + "_" + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".png";
    }

    void LateUpdate()
    {

        if (Input.GetKeyDown(KeyCode.T))
        {
            RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
            GetComponent<Camera>().targetTexture = rt;
            Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
            GetComponent<Camera>().Render();
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
            GetComponent<Camera>().targetTexture = null;
            RenderTexture.active = null;
            Destroy(rt);
            byte[] bytes = screenShot.EncodeToPNG();
            string filename = ScreenShotName(resWidth, resHeight);
            FileStream file = File.Open(filename, FileMode.OpenOrCreate);
            file.Write(bytes, 0, bytes.Length);
            file.Close();
        }
    }
}