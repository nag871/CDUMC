using UnityEngine;

public class Lightswitch : MonoBehaviour
{
    [SerializeField] private GameObject _light;

    private bool _enabled;

    private AudioSource _lightswitchSound;

    private GameManager _gameManager;

    void Start()
    {
        _gameManager = GameObject.FindGameObjectWithTag("GameMenager").GetComponent<GameManager>();

        _lightswitchSound = gameObject.GetComponent<AudioSource>();

        Switch();
    }

    public void Switch()
    {
        _enabled = !_enabled;
        _light.SetActive(_enabled);

        _lightswitchSound.Play();

        _gameManager.lightsEnabled = _enabled;
    }
}
