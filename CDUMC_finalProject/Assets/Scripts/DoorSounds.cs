using UnityEngine;

public class DoorSounds : MonoBehaviour
{
    [SerializeField] private AudioSource _doorCloseAuido;
    [SerializeField] private AudioSource _doorOpenAuido;

    public void OpenSound()
    {
        _doorOpenAuido.Play();
    }

    public void CloseSound()
    {
        _doorCloseAuido.Play();
    }

}
