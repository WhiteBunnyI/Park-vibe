using UnityEngine;
using UnityEngine.UIElements;

public class UIMainMenu : MonoBehaviour
{
    private void Start()
    {
        var ui = GetComponent<UIDocument>();
        var root = ui.rootVisualElement;

        var start = root.Q<Button>("start");
        var exit = root.Q<Button>("exit");

        start.RegisterCallback<MouseUpEvent>(e => GameManager.LoadGame());
        exit.RegisterCallback<MouseUpEvent>(e => GameManager.ExitGame());
    }
}
