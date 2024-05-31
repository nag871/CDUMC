using UnityEngine;
using UnityEngine.UI;

public class CamerasConrtol : MonoBehaviour
{
    [SerializeField] RenderTexture[] CamTexture;
    [SerializeField] RawImage Screen;
    [SerializeField] private GameObject[] _Cameras;

    public static CamerasConrtol instance;

    private void Awake()
    {
        instance = this;
        EnableCamera(1);
    }

    public void EnableCamera(int NumberCam)
    {
        for (int i = 0; i < _Cameras.Length; i++)
        {
            if (i == NumberCam)
                _Cameras[i].GetComponent<Camera>().enabled = true;
            else
                _Cameras[i].GetComponent<Camera>().enabled = false;
        }
        Screen.texture = CamTexture[NumberCam];
    }
}
