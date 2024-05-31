using UnityEngine;

public class WindowControl : MonoBehaviour
{
    [SerializeField] private Canvas _canvas;

    [SerializeField] private AudioSource _openAudio;
    [SerializeField] private AudioSource _closeAudio;

    [HideInInspector] public bool isClosed;
    [HideInInspector] public bool localIsClosed;

    private Animator _animator;



    private void Start()
    {
        _animator = GetComponent<Animator>();
    }

    public void ChangeWindowsStage()
    {
        localIsClosed = !localIsClosed;

        if (localIsClosed)
            _openAudio.Play();
        if (!localIsClosed)
            _closeAudio.Play();

        _animator.SetBool("Closed", localIsClosed);
    }

    public void ChangeBool()
    {
        isClosed = localIsClosed;
    }


    public void SetActiveCanavas(bool active)
    {
        _canvas.gameObject.SetActive(active);
    }

}
