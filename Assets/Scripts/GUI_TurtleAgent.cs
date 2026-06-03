using UnityEngine;

public class GUI_TurtleAgent : MonoBehaviour
{

    [SerializeField] private TurtleAgent _turtleAgent; // 引用TurtleAgent组件

    private GUIStyle _defaultStyle = new GUIStyle(); // 默认的GUI样式
    private GUIStyle _positiveStyle = new GUIStyle(); // 显示正数的GUI样式
    private GUIStyle _negativeStyle = new GUIStyle(); // 显示负数的GUI样式






    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _defaultStyle.fontSize = 20;
        _defaultStyle.normal.textColor = Color.yellow;

        _positiveStyle.fontSize = 20;
        _positiveStyle.normal.textColor = Color.green;

        _negativeStyle.fontSize = 20;
        _negativeStyle.normal.textColor = Color.red;
    }

    private void OnGUI()
    {
        string debugEpisode = "Episode: " + _turtleAgent.CurrentEpisode + " - Step: " + _turtleAgent.StepCount;
        string debugReward = "Reward: " + _turtleAgent.CumulativeReward.ToString();

        GUIStyle rewardStyle = _turtleAgent.CumulativeReward < 0 ? _negativeStyle : _positiveStyle;

        GUI.Label(new Rect(20, 20, 500, 30), debugEpisode, _defaultStyle);
        GUI.Label(new Rect(20, 60, 500, 30), debugReward, rewardStyle);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
