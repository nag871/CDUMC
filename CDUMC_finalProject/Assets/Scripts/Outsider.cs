using UnityEngine;
using UnityEngine.AI;

public class Outsider : MonoBehaviour
{
    [SerializeField] private int _countOfRooms;
    [SerializeField] private int _countStepsWhenCanAttcak;
    [SerializeField] private int _cancheAttack;
    [SerializeField] private int _maxCancheAttack;

    [SerializeField] private Transform startPos;
    [SerializeField] private Transform playerPos;

    [SerializeField] private Transform[] Outside; // ������ ����� ��������
    [SerializeField] private Transform[] kitchen; // ������ ����� ��������
    [SerializeField] private Transform[] garage; // ������ ����� ��������

    [SerializeField] private float _timeToWait;
    [SerializeField] private float _timeToAttack;
    [SerializeField] private float _timeToStay;

    private int _currentWaypointIndex = 0; // ������ ������� ����� ��������
    private int _countOfSteps;
    private int _currentScenarioIndex;

    private Transform[] _currentScenario;

    private NavMeshAgent _agent; // ��������� ��� ����������� �� NavMesh
    private Animator _animator;
    private AudioSource _footSteps;

    private WindowControl _windowControl;

    private GameManager _gameManager;

    private float _waitTime; // ����� �������� �� ������� �����
    private float _timer; // ������ ��� ������������ ������� ��������
    private float timerDoor = 0;
    private float timer = 0;

    private bool _attackPlayer;
    private bool _gotFlash;
    private bool _tryingAtttack;
    private bool _playerDead;

    [Space]

#pragma warning disable CS0414 // ���� "Outsider.currentScenario" ��������� ��������, �� ��� �� ���� �� ������������.
    [SerializeField] private string currentScenario;
#pragma warning restore CS0414 // ���� "Outsider.currentScenario" ��������� ��������, �� ��� �� ���� �� ������������.
    [SerializeField] private int countOfSteps;

    // ��������� ������ ��� �������� ��������� ������ (������)
    private int[] _recentScenarios = new int[2];
    private int _recentScenarioIndex = 0;

    void Start()
    {
        _animator = GetComponentInChildren<Animator>();
        _footSteps = GetComponent<AudioSource>();
        _windowControl = GameObject.FindGameObjectWithTag("PlayerWindow").GetComponent<WindowControl>();
        _gameManager = GameObject.FindGameObjectWithTag("GameMenager").GetComponent<GameManager>();

        _agent = GetComponent<NavMeshAgent>();
        transform.position = startPos.position;

        // �������������� ������ ������
        for (int i = 0; i < _recentScenarios.Length; i++)
        {
            _recentScenarios[i] = -1;
        }

        // �������� ��������� �������
        ChooseNewScenario();

    }

    void Update()
    {
        if (_gameManager.gameOver)
        {
            _tryingAtttack = false;
            _attackPlayer = false;
            _agent.speed = 0;
        }

        countOfSteps = _countOfSteps;

        // �������� ��������
        switch (_currentScenarioIndex)
        {
            case 0:
                _currentScenario = Outside;
                currentScenario = "Outside";
                break;
            case 1:
                _currentScenario = kitchen;
                currentScenario = "kitchen";
                break;
            case 2:
                _currentScenario = garage;
                currentScenario = "garage";
                break;
        }
        // ���� ���� ������ ������� ����� ��������
        if (!_agent.pathPending && _agent.remainingDistance < 0.01f)
        {
            _timer += Time.deltaTime;

            EndPath();

            _footSteps.Stop();
            _animator.SetBool("isWalking", false);

            // ���� ����� �������� �������, ��������� � ��������� �����
            if (_timer >= _waitTime && !_attackPlayer && !_tryingAtttack)
            {
                GoToNextWaypoint();
                _footSteps.Play();
                _animator.SetBool("isWalking", true);
            }
        }

        if (_attackPlayer && !_playerDead && !_gameManager.enemyAttack)
            Attack();
        if (_tryingAtttack && !_playerDead)
            TryingAttack();

        if (_tryingAtttack)
            _animator.SetBool("isWalking", true);
    }

    public void GetFlash()
    {
        if (_attackPlayer)
            _gotFlash = true;
    }

    void GoToNextWaypoint()
    {
        TryAttackPlayer(_cancheAttack);
        if (!_tryingAtttack)
        {
            // �������� ��������� ����� �������� � ������������� ����� ���� ��� �����
            Transform currentWaypoint = _currentScenario[_currentWaypointIndex];
            _agent.SetDestination(currentWaypoint.position);
            _waitTime = currentWaypoint.GetComponent<Waypoint>().timerToWait;
            _agent.speed = currentWaypoint.GetComponent<Waypoint>().walkingSpeed;
            _timer = 0f;

            _animator.speed = _agent.speed / 0.75f;

            _countOfSteps++;
            _currentWaypointIndex++;
        }
    }

    private void EndPath()
    {
        if (_currentWaypointIndex == _currentScenario.Length && !_tryingAtttack)
        {
            TryAttackPlayer(_cancheAttack);
            _cancheAttack++;
        }
    }

    private void ChooseNewScenario()
    {
        // �������� ��������� �������, �������� �� ��������� 
        int newScenario = Random.Range(0, _countOfRooms);
        while (_recentScenarios.Contains(newScenario))
        {
            newScenario = Random.Range(0, _countOfRooms);
        }

        // ��������� ����� ������� � ������ ���������
        _recentScenarios[_recentScenarioIndex] = newScenario;
        _recentScenarioIndex = (_recentScenarioIndex + 1) % _recentScenarios.Length;

        _currentScenarioIndex = newScenario;
    }

    private void TryAttackPlayer(int chance)
    {
        if (Random.Range(chance, _maxCancheAttack) <= _maxCancheAttack && _countOfSteps > _countStepsWhenCanAttcak)
            _tryingAtttack = true;
        else if (_currentWaypointIndex == _currentScenario.Length && !_tryingAtttack)
            ResetAttack();
    }

    private void TryingAttack()
    {
        _agent.speed = 1f;
        _agent.SetDestination(playerPos.position);

        if (!_agent.pathPending && _agent.remainingDistance < 0.1f)
        {
            Invoke(nameof(StartAttack), _timeToWait);
            _tryingAtttack = false;
            _animator.SetBool("isWalking", false);
        }
    }

    private void StartAttack()
    {
        _attackPlayer = true;
    }

    private void Attack()
    {
        bool playerHiding = _gameManager.playerHiding;
        bool windowClosed = _gameManager.windowClosed;
        bool playerCaught = false;

        _gameManager.outsiderAttack = true;

        if (playerHiding && !_playerDead && !_gotFlash)
        {
            _animator.SetTrigger("playerHiding");
            GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().Dead(1, 2);
            _playerDead = true;

        }

        _animator.SetBool("isAttacking", true);

        transform.eulerAngles = new Vector3(0, 180, 0);

        if (timerDoor <= _timeToAttack)
        {
            timerDoor += Time.deltaTime;
        }
        if (timerDoor >= _timeToAttack)
        {
            if (windowClosed && _gotFlash)
                _gotFlash = false;

            if (!windowClosed && !playerCaught && _gotFlash)
            {
                _animator.SetTrigger("GetFlash");
                ResetAttack();
            }    

            transform.eulerAngles = new Vector3(0, 180, 0);

            if (!playerHiding)
            {
                if (windowClosed)
                    _animator.SetBool("WindowClosed", true);
                else
                    _animator.SetBool("WindowClosed", false);

                if (timer <= _timeToStay)
                    timer += Time.deltaTime;

                if (!playerCaught && timer >= _timeToStay)
                {
                    playerCaught = true;
                    _animator.SetBool("canAttack", true);
                }
            }
        }
    }


    private void ResetAttack()
    {
        _gameManager.outsiderAttack = false;

        _gotFlash = false;
        _attackPlayer = false;
        _currentWaypointIndex = 0;
        _countOfSteps = 0;

        _animator.SetBool("isAttacking", false);

        // �������� ����� �������
        ChooseNewScenario();
    }
}
