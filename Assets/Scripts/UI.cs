using UnityEngine;
using UnityEngine.UIElements;

public class UI : MonoBehaviour
{
    ProgressBar _fatigue;
    Label _helper;
    Label _gameover;

    static UI _instance;

    void Start()
    {
        if (_instance != null)
        {
            Destroy(this);
            return;
        }

        _instance = this;
        var ui = GetComponent<UIDocument>();
        var root = ui.rootVisualElement;
        _fatigue = root.Q<ProgressBar>("fatigue");
        _helper = root.Q<Label>("helper");
        _gameover = root.Q<Label>("gameover");
        _gameover.visible = false;
        SetHelp("");

        GameManager.OnGameOver += GameOver;
    }

    private void GameOver()
    {
        _gameover.visible = true;
    }

    public static void SetFatigue(float value)
    {
        _instance._fatigue.value = value;
    }

    public static void SetHelp(string text)
    {
        if (text.Length == 0)
            _instance._helper.visible = false;
        else
            _instance._helper.visible = true;

        _instance._helper.text = text;
    }
}
