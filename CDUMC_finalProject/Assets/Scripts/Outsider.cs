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

    [SerializeField] private Transform[] Outside; // массив точек маршрута
    [SerializeField] private Transform[] kitchen; // массив точек маршрута
    [SerializeField] private Transform[] garage; // массив точек маршрута

    [SerializeField] private float _timeToWait;
    [SerializeField] private float _timeToAttack;
    [SerializeField] private float _timeToStay;

    private int _currentWaypointIndex = 0; // индекс текущей точки маршрута
    private int _countOfSteps;
    private int _currentScenarioIndex;

    private Transform[] _currentScenario;

    private NavMeshAgent _agent; // компонент дл€ перемещени€ по NavMesh
    private Animator _animator;
    private AudioSource _footSteps;

    private WindowControl _windowControl;

    private GameManager _gameManager;

    private float _waitTime; // врем€ ожидани€ на текущей точке
    private float _timer; // таймер дл€ отслеживани€ времени ожидани€
    private float timerDoor = 0;
    private float timer = 0;

    private bool _attackPlayer;
    private bool _gotFlash;
    private bool _tryingAtttack;
    private bool _playerDead;

    [Space]

#pragma warning disable CS0414 // ѕолю "Outsider.currentScenario" присвоено значение, но оно ни разу не использовано.
    [SerializeField] private string currentScenario;
#pragma warning restore CS0414 // ѕолю "Outsider.currentScenario" присвоено значение, но оно ни разу не использовано.
    [SerializeField] private int countOfSteps;

    // ƒобавл€ем список дл€ хранени€ последних комнат (пам€ть)
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

        // »нициализируем список комнат
        for (int i = 0; i < _recentScenarios.Length; i++)
        {
            _recentScenarios[i] = -1;
        }

        // ¬ыбираем случайную комнату
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

        // ¬ыбираем сценарий
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
        // если враг достиг текущей точки маршрута
        if (!_agent.pathPending && _agent.remainingDistance < 0.01f)
        {
            _timer += Time.deltaTime;

            EndPath();

            _footSteps.Stop();
            _animator.SetBool("isWalking", false);

            // если врем€ ожидани€ истекло, переходим к следующей точке
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
            // получаем компонент точки маршрута и устанавливаем новую цель дл€ врага
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
        // ¬ыбираем случайную комнату, отличную от последних 
        int newScenario = Random.Range(0, _countOfRooms);
        while (_recentScenarios.Contains(newScenario))
        {
            newScenario = Random.Range(0, _countOfRooms);
        }

        // ƒобавл€ем новую комнату в список последних
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

        // ¬ыбираем новую комнату
        ChooseNewScenario();
    }
}
