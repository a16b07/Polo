using UnityEngine;

public class GameResolution : MonoBehaviour
{
    public int width  = 480;
    public int height = 260;
    public FullScreenMode mode = FullScreenMode.Windowed;

    void Awake()
    {
        Screen.SetResolution(width, height, mode);
    }
}
