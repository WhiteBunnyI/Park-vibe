using UnityEngine;

public class Bench : MonoBehaviour, IHelper
{
    [SerializeField] private string _sittingHelp;
    [SerializeField] private string _standingHelp;
    public bool IsUsing;

    public string Help => IsUsing ? _sittingHelp : _standingHelp;

}
