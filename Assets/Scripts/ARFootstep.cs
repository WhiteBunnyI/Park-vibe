using Unity.VisualScripting;
using UnityEngine;

public class ARFootstep : MonoBehaviour
{
    [SerializeField] private float _threshold;
    [SerializeField] private Transform _footL;
    [SerializeField] private Transform _footR;

    private float _timeL;
    private float _timeR;
    private void OnFootstepL() => PlayFootstep(_footL, ref _timeL);
    private void OnFootstepR() => PlayFootstep(_footR, ref _timeR);

    private void PlayFootstep(Transform tran, ref float time)
    {
        if (Time.time - time < _threshold) return;
        AudioManager.Footstep(tran.position);
        time = Time.time;
    }
}
