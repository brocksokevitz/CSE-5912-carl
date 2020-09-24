using UnityEngine;
using System.Collections;

public class GameSettings : MonoBehaviour
{
	public static int ResolutionIndex { get; set; }
	public static int AntiAliasing { get; set; }
	public static int Quality { get; set; }

    private static bool _fullscreen = true;
    public static bool Fullscreen
    {
        get
        {
            return _fullscreen;
        }
        set
        {
            _fullscreen = value;
        }
    }

    private static float _musicVolume = -1;
    public static float MusicVolume
	{
		get
		{
			return _musicVolume;
		}

		set
		{
			_musicVolume = value;
		}
	}

    private static float _sfxVolume = -1;
    public static float SfxVolume
    {
        get
        {
            return _sfxVolume;
        }

        set
        {
            _sfxVolume = value;
        }
    }
}
