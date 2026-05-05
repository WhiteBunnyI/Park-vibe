using UnityEngine;

public class SimpleHelper : MonoBehaviour, IHelper
{
    [SerializeField] private string _help;

    public string Help =>_help;
}
