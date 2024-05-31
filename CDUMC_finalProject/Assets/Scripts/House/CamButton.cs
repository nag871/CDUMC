using UnityEngine;

public class CamButton : MonoBehaviour
{
    public int NumberCam;

    [SerializeField] private AudioSource _pressSound;

    public void ChooseCamera()
    {
        _pressSound.Play();
        CamerasConrtol.instance.EnableCamera(NumberCam);
    }

}
