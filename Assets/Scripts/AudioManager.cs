using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioClip _ambience;
    [SerializeField] private AudioClip _windBlowing;
    [SerializeField] private AudioClip _step;

    AudioSource _sourceAmbience;

    static AudioManager _instance;

    private void Start()
    {
        if (_instance != null)
        {
            Destroy(this);
            return;
        }
        _instance = this;


        _sourceAmbience = gameObject.AddComponent<AudioSource>();
        _sourceAmbience.loop = true;
        _sourceAmbience.resource = _ambience;
        _sourceAmbience.volume = 0.1f;
        _sourceAmbience.transform.SetParent(PlayerController.Player.transform, false);
        _sourceAmbience.Play();
    }

    public static void Footstep(Vector3 audioPos)
    {
        AudioSource.PlayClipAtPoint(_instance._step, audioPos);
    }
}
