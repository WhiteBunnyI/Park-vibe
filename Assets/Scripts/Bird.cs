using System.Collections;
using UnityEngine;

public class Bird : MonoBehaviour, IHelper
{
    [SerializeField] private string _helpText;
    [SerializeField] private float _cooldown;
    [Header("Only for view")]
    [SerializeField] private bool _isActive;
    [SerializeField] private bool _isPlayerInZone;
    [SerializeField] private Renderer _model;

    Animation _anim;
    Coroutine _flyAwayCoroutine;

    private void Start()
    {
        _isActive = true;
        _isPlayerInZone = false;
        _model = GetComponentInChildren<Renderer>();
        _anim = GetComponent<Animation>();
        _anim.wrapMode = WrapMode.Loop;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _isPlayerInZone = true;
            _flyAwayCoroutine ??= StartCoroutine(FlyAway());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _isPlayerInZone = false;
        }
    }

    IEnumerator FlyAway()
    {
        _isActive = false;
        _model.enabled = false;

        while(_isPlayerInZone)
            yield return new WaitForSeconds(_cooldown);

        _model.enabled = true;
        _flyAwayCoroutine = null;
        _isActive = true;
    }

    public bool IsActive => _isActive;
    public Renderer Renderer => _model;

    public string Help => IsActive ? _helpText : string.Empty;
}
