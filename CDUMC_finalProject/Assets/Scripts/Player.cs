using UnityEngine;
using UnityEngine.Events;

public class Player : MonoBehaviour
{
    [SerializeField] private int _cooldownFlash;

    [SerializeField] private UnityEvent activeFlash;

    [SerializeField] private Animator _animator;
    [SerializeField] private Animator _flashAnimator;
    [SerializeField] private WindowControl _windowControl;

    [SerializeField] private AudioSource _getUpfromChairAudio;
    [SerializeField] private AudioSource _satdownToChairAudio;
    [SerializeField] private AudioSource _turnAudio;
    [SerializeField] private AudioSource _turn2Audio;
    [SerializeField] private AudioSource _turn3Audio;
    [SerializeField] private AudioSource _deathAudio;
    [SerializeField] private AudioSource _flashAudio;
    [SerializeField] private AudioSource _flashReloadAudio;

    [HideInInspector] public bool isDead;
    private int _countPos;

    private bool _canFalsh;
    private bool _lockedMove;
    private bool _isHiding;
    private bool _lookAtDoor;
    private bool _lookAtWindow;
    private bool _lookAtScreen;

    private GameManager _gameManager;

    void Start()
    {
        _gameManager = GameObject.FindGameObjectWithTag("GameMenager").GetComponent<GameManager>();
        isDead = false;
        _canFalsh = true;
    }

    void Update()
    {
        ChangeState();
        SetAnimator();
        Flash();
    }

    public void Flash()
    {
        if (_canFalsh && !_isHiding && !isDead && !_lockedMove && Input.GetKeyDown(KeyCode.Mouse1))
        {
            _flashAnimator.SetTrigger("flash");
            _flashAudio.Play();
            _canFalsh = false;
            activeFlash.Invoke();
            Invoke(nameof(CanFlash), _cooldownFlash);
            _flashReloadAudio.PlayDelayed(1);
        }
    }

    private void CanFlash()
    {
        _canFalsh = true;
    }

    public void ChangeState()
    {
        //change poss
        if (Input.GetKeyDown(KeyCode.D) && !_lookAtWindow && !_isHiding && !_lookAtScreen && !_lockedMove)
            _countPos++;
        if (Input.GetKeyDown(KeyCode.A) && !_lookAtDoor && !_isHiding && !_lookAtScreen && !_lockedMove)
            _countPos--;

        //Turn to screen
        if (Input.GetKeyDown(KeyCode.W) && _countPos == 0 && !_lookAtScreen && !_lockedMove)
        {
            _lookAtScreen = true;

            _turn2Audio.Play();

            Invoke(nameof(ControlScreen), 1f);

            _lockedMove = true;
            Invoke(nameof(UnLockMove), 0.5f);
        }
        if (Input.GetKeyDown(KeyCode.S) && _countPos == 0 && _lookAtScreen && !_lockedMove)
        {
            _lookAtScreen = false;

            _turn3Audio.Play();

            ControlScreen();

            _lockedMove = true;
            Invoke(nameof(UnLockMove), 0.5f);
        }
        //Hide
        if (Input.GetKeyDown(KeyCode.W) && _countPos == 1 && !_isHiding && !_lockedMove && !_windowControl.localIsClosed)
        {
            _isHiding = true;
            _lockedMove = true;
            _getUpfromChairAudio.Play();
            _windowControl.SetActiveCanavas(false);
            Invoke(nameof(SetButonWindow), 1.6f);
            Invoke(nameof(UnLockMove), 2.5f);
        }
        else if (Input.GetKeyDown(KeyCode.W) && _countPos == 1 && _isHiding && !_lockedMove && !_windowControl.localIsClosed)
        {
            _isHiding = false;
            _countPos = 0;
            _lockedMove = true;
            _satdownToChairAudio.Play();
            _windowControl.SetActiveCanavas(false);
            Invoke(nameof(SetButonWindow), 1.6f);
            Invoke(nameof(UnLockMove), 3.3f);
        }

        //move player
        if (_countPos == 0)
        {
            _turnAudio.Play();
            _lookAtWindow = false;
            _lookAtDoor = false;
        }
        else if (_countPos == 1)
        {
            _turn2Audio.Play();
            _lookAtWindow = true;
            _lookAtDoor = false;
        }
        else if (_countPos == -1)
        {
            _turn3Audio.Play();
            _lookAtWindow = false;
            _lookAtDoor = true;
        }

        _gameManager.playerHiding = _isHiding;
        _gameManager.windowClosed = _windowControl.isClosed;
    }

    public void Dead(int pos, int typeDeath)
    {
        if (_lookAtScreen)
        {
            ControlScreen();
            _lookAtScreen = false;
        }

        _lockedMove = false;
        if(typeDeath != 2)
            _countPos = pos;
        ChangeState();
        _gameManager.playerDead = true;
        _lockedMove = true;
        _deathAudio.Play();
        if (typeDeath == -1)
            _animator.SetTrigger("isDeadType1");
        else if (typeDeath == 1)
            _animator.SetTrigger("isDeadType2");
        else if (typeDeath == 2)
            _animator.SetTrigger("isDeadType3");
    }

    private void UnLockMove()
    {
        _lockedMove = false;
    }

    private void SetButonWindow()
    {
        _windowControl.SetActiveCanavas(true);
    }

    private void SetAnimator()
    {
        _animator.SetBool("LookAtDoor", _lookAtDoor);
        _animator.SetBool("LookAtWindow", _lookAtWindow);
        _animator.SetBool("HideInWindow", _isHiding);
        _animator.SetBool("LookAtScreen", _lookAtScreen);
    }

    private void ControlScreen()
    {
        _gameManager.CamCanvasControl();
    }

}
