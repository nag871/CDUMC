using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private int _nightTime;

    [SerializeField] private TMP_Text _clockText;

    [SerializeField] Canvas CamerasCanvas;
    [SerializeField] Canvas WinUI;
    [SerializeField] Canvas DeathUI;

    [SerializeField] GameObject _ambientPlayer;

    [HideInInspector] public bool playerHiding;
    [HideInInspector] public bool playerDead;
    [HideInInspector] public bool windowClosed;
    [HideInInspector] public bool lightsEnabled;

    [HideInInspector] public bool enemyAttack;
    [HideInInspector] public bool outsiderAttack;

    [HideInInspector] public bool gameOver;

    private bool _camerasCanvasEnable;

    private float _currentTime;

    private DateTime gameStartTime;

    private void Start()
    {
        _camerasCanvasEnable = false;
        gameOver = false;

        _currentTime = 0;

        gameStartTime = DateTime.Now;

        Invoke(nameof(Timeout), _nightTime);
    }

    private void Update()
    {
        SetClock();

        _currentTime += Time.deltaTime;

        if (playerDead)
            Invoke(nameof(DeadUIEnable), 1.66f);
    }

    private void DeadUIEnable()
    {
        _ambientPlayer.SetActive(false);
        DeathUI.gameObject.SetActive(true);
    }

    public void CamCanvasControl()
    {
        _camerasCanvasEnable = !_camerasCanvasEnable;

        CamerasCanvas.gameObject.SetActive(_camerasCanvasEnable);
    }

    private void Timeout()
    {
        if (!playerDead)
        {
            gameOver = true;
            CamerasCanvas.gameObject.SetActive(false);
            _ambientPlayer.SetActive(false);
            WinUI.gameObject.SetActive(true);
        }

    }

    private void SetClock()
    {

        int minutes = Mathf.FloorToInt(_currentTime / 60f);
        int seconds = Mathf.FloorToInt(_currentTime % 60f);

        string timerString = string.Format("{0:00}:{1:00}", minutes, seconds);
        _clockText.text = timerString;
    }


    public void ReloadLVL()
    {
        SceneManager.LoadScene(1);
    }

    public void ToMainMenu()
    {
        SceneManager.LoadScene(0);
    }

}
