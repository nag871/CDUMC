using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [SerializeField] private int _countOfRooms;
    [SerializeField] private int _countStepsWhenCanAttcak;
    [SerializeField] private int _cancheAttack;
    [SerializeField] private int _maxCancheAttack;

    [SerializeField] private Transform startPos;
    [SerializeField] private Transform playerPos;

    [SerializeField] private Transform[] hall; // массив точек маршрута
    [SerializeField] private Transform[] bedroom; // массив точек маршрута
    [SerializeField] private Transform[] kitchen; // массив точек маршрута

    [SerializeField] private float _timeToWaitToEnter;
    [SerializeField] private float _timeToWaitInOffice;

    private int _currentWaypointIndex = 0; // индекс текущей точки маршрута
    private int _countOfSteps;
    private int _currentScenarioIndex;

    private Transform[] _currentScenario;

    private NavMeshAgent _agent; // компонент дл€ перемещени€ по NavMesh
    private Animator _animator;
    private AudioSource _footSteps;
    private Animator _doorAnimator;

    private GameManager _gameManager;

    private float _waitTime; // врем€ ожидани€ на текущей точке
    private float _timer; // таймер дл€ отслеживани€ времени ожидани€
    private float timerDoor = 0;
    private float timer = 0;

    private bool _attackPlayer;
    private bool _tryingAtttack;
    private bool _playerDead;

    [Space]

#pragma warning disable CS0414 // ѕолю "EnemyAI.currentScenario" присвоено значение, но оно ни разу не использовано.
    [SerializeField] private string currentScenario;
#pragma warning restore CS0414 // ѕолю "EnemyAI.currentScenario" присвоено значение, но оно ни разу не использовано.
    [SerializeField] private int countOfSteps;

    // ƒобавл€ем список дл€ хранени€ последних комнат (пам€ть)
    private int[] _recentScenarios = new int[2];
    private int _recentScenarioIndex = 0;

    void Start()
    {
        _animator = GetComponentInChildren<Animator>();
        _footSteps = GetComponent<AudioSource>();
        _gameManager = GameObject.FindGameObjectWithTag("GameMenager").GetComponent<GameManager>();
        _doorAnimator = GameObject.FindGameObjectWithTag("PlayerDoor").GetComponent<Animator>();

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
                _currentScenario = hall;
                currentScenario = "hall";
                break;
            case 1:
                _currentScenario = bedroom;
                currentScenario = "bedroom";
                break;
            case 2:
                _currentScenario = kitchen;
                currentScenario = "kitchen";
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
            if (_timer >= _waitTime && !_attackPlayer)
            {
                GoToNextWaypoint();
                _footSteps.Play();
                _animator.SetBool("isWalking", true);
            }
        }

        if (_attackPlayer && !_playerDead && !_gameManager.outsiderAttack)
            Attack();
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
        if (_currentWaypointIndex == _currentScenario.Length)
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
            {
            _tryingAtttack = true;
            _agent.SetDestination(playerPos.position);
            _agent.speed = 1f;
            if (!_agent.pathPending && _agent.remainingDistance < 0.01f)
            {
                Invoke(nameof(StartAttack), _timeToWaitToEnter);
            }
        }
        else if (_currentWaypointIndex == _currentScenario.Length)
        {
            ResetAttack();
        }
    }

    private void StartAttack()
    {
        _attackPlayer = true;
    }

    private void Attack()
    {
        _animator.SetBool("isAttacking", true);
        _animator.SetBool("playerHiding", true);

        _doorAnimator.SetBool("isOpened", true);

        _gameManager.enemyAttack = true;

        transform.eulerAngles = new Vector3(0, -90, 0);

        if (timerDoor <= 2)
        {
            timerDoor += Time.deltaTime;
        }
        if (timerDoor >= 2)
        {
            bool playerHiding = _gameManager.playerHiding;
            bool windowClosed = _gameManager.windowClosed;

            transform.eulerAngles = new Vector3(0, -90, 0);

            if (!playerHiding)
            {
                _animator.SetBool("playerHiding", false);
                GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().Dead(-1, -1);
                _playerDead = true;

            }
            else if (playerHiding)
            {
                bool playerCaught = false;

                if (timer <= _timeToWaitInOffice)
                {
                    timer += Time.deltaTime;

                    if (!playerHiding || !windowClosed)
                    {
                        playerCaught = true;
                        _animator.SetBool("playerHiding", false);
                        GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().Dead(-1, -1);
                        _playerDead = true;
                    }
                }
                if (!playerCaught && timer >= _timeToWaitInOffice)
                {
                    timer = 0;
                    timerDoor = 0;
                    ResetAttack();
                }
            }
        }

    }


    private void ResetAttack()
    {
        if (_attackPlayer)
            _doorAnimator.SetBool("isOpened", false);

        _gameManager.enemyAttack = false;

        _attackPlayer = false;
        _tryingAtttack = false;
        _currentWaypointIndex = 0;
        _countOfSteps = 0;

        _animator.SetBool("isAttacking", false);

        // ¬ыбираем новую комнату
        ChooseNewScenario();
    }
}
