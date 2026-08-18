using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections;

public class TurtleAgent : Agent
{

    [SerializeField] private Transform _goal; // 目标位置，海龟需要移动到这个位置
    [SerializeField] private Renderer _groundRenderer; // 地面渲染器，用于改变地面颜色以提供视觉反馈
    [SerializeField] private float _moveSpeed = 1.5f; // 海龟的移动速度
    [SerializeField] private float _rotationSpeed = 180f; // 海龟的旋转速度

    private Renderer _renderer; // 海龟的渲染器，用于改变颜色

    [HideInInspector] public int CurrentEpisode = 0; // 当前回合数
    [HideInInspector] public float CumulativeReward = 0f; // 累积奖励

    private Color _defaultGroundColor; // 默认的地面颜色
    private Coroutine _flashGroundCoroutine; // 用于闪烁地面颜色的协程

    public override void Initialize()
    {
        // 初始化
        Debug.Log("Agent Initialized");

        _renderer = GetComponent<Renderer>();
        CurrentEpisode = 0;
        CumulativeReward = 0f;

        if (_groundRenderer != null)
        {
            _defaultGroundColor = _groundRenderer.material.color; // 存储默认的地面颜色
        }
    }

    // 
    public override void OnEpisodeBegin()
    {

        // 不能重写EndEpisode方法，所以在OnEpisodeBegin中处理回合结束时的逻辑
        if (_groundRenderer != null && CumulativeReward != 0f)
        {
            Color flashColor = (CumulativeReward > 0f) ? Color.green : Color.red; // 根据奖励的正负选择闪烁颜色

            if (_flashGroundCoroutine != null)
            {
                StopCoroutine(_flashGroundCoroutine); // 停止之前的闪烁协程
            }

            _flashGroundCoroutine = StartCoroutine(FlashGround(flashColor, 3.0f)); // 开始新的闪烁协程
        }

        // 每次新的回合开始时调用
        Debug.Log("Episode Started");

        CurrentEpisode++;
        CumulativeReward = 0f;
        _renderer.material.color = Color.blue; // 重置颜色为蓝色

        SpawnObjects(); // 生成新的目标位置
    }

    private IEnumerator FlashGround(Color targetColor, float duration)
    {
        float elapsedTime = 0f;

        _groundRenderer.material.color = targetColor; // 设置地面颜色为目标颜色

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime; // 增加经过的时间
            _groundRenderer.material.color = Color.Lerp(targetColor, _defaultGroundColor, elapsedTime / duration); // 逐渐将颜色从目标颜色过渡回默认颜色
            yield return null; // 等待下一帧
        }
    }

    private void SpawnObjects()
    {
        // 重置海龟位置和旋转
        transform.localRotation = Quaternion.identity; // 重置海龟旋转
        transform.localPosition = new Vector3(0f, 0.15f, 0f); // 重置海龟位置到起始点


        // 随机生成目标位置
        float randomAngle = Random.Range(0f, 360f);
        Vector3 randomDirection = Quaternion.Euler(0f, randomAngle, 0f) * Vector3.forward;

        float randomDistance = Random.Range(1f, 2.5f);

        // 计算目标位置，其位置在海龟当前位置的基础上，沿着随机方向移动一定距离
        Vector3 goalPosition = transform.localPosition + randomDirection * randomDistance;

        _goal.localPosition = new Vector3(goalPosition.x, 0.3f, goalPosition.z); // 确保目标位置在地面
    }


    public override void CollectObservations(VectorSensor sensor)
    {


        float turtlePosX_normalized = transform.localPosition.x / 5f; // 将海龟位置的X坐标归一化到[-1, 1]范围内
        float turtlePosZ_normalized = transform.localPosition.z / 5f;

        float turtleRotation_normalized = (transform.localRotation.eulerAngles.y / 360f )* 2f - 1f; // 将海龟的旋转角度归一化到[-1, 1]范围内

        sensor.AddObservation(turtlePosX_normalized); // 添加海龟位置的X坐标
        sensor.AddObservation(turtlePosZ_normalized); // 添加海龟位置的Z坐标
        sensor.AddObservation(turtleRotation_normalized); // 添加海龟的旋转角度
        // sensor是一个VectorSensor对象，用于收集环境的状态信息。通过调用AddObservation方法，我们将目标位置和海龟的位置以及旋转角度添加到传感器中，以便智能体能够感知环境的状态并做出相应的决策。
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // 手动控制智能体的行为，主要用于测试和调试
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = 0; // 默认动作：不移动

        if (Input.GetKey(KeyCode.UpArrow))
        {
            discreteActionsOut[0] = 1; // 向前移动
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            discreteActionsOut[0] = 2; // 向左旋转
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            discreteActionsOut[0] = 3; // 向右旋转
        }
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // actionBuffers 中存储了决策输出
        MoveAgent(actionBuffers.DiscreteActions); // 根据智能体的动作来移动海龟
        AddReward(-2f / MaxStep); // 每一步都给予一个小的负奖励，鼓励智能体尽快完成任务
        CumulativeReward = GetCumulativeReward(); // 获取当前的累积奖励
    }

    public void MoveAgent(ActionSegment<int> act)
    {
        var action = act[0]; // 获取移动动作

        switch (action)
        {
            case 1: // 向前移动
                transform.position += transform.forward * _moveSpeed * Time.deltaTime;
                break;
            case 2: // 向左旋转
                transform.Rotate(0f, -_rotationSpeed * Time.deltaTime, 0f);
                break;
            case 3: // 向右旋转
                transform.Rotate(0f, _rotationSpeed * Time.deltaTime, 0f);
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Goal"))
        {
            GoalReached();
        }
    }

    private void GoalReached()
    {
        AddReward(1.0f); // 达到目标位置时给予奖励
        CumulativeReward = GetCumulativeReward(); // 获取当前的累积奖励
        EndEpisode(); // 结束当前回合
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            AddReward(-0.5f); // 碰撞到墙壁时给予负奖励
            if (_renderer != null)
            {
                _renderer.material.color = Color.red; // 碰撞时将颜色变为红色
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            AddReward(-0.01f*Time.fixedDeltaTime); // 持续碰撞时给予持续的负奖励
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            if (_renderer != null)
            {
                _renderer.material.color = Color.blue; // 离开碰撞时将颜色恢复为蓝色
            }
        }
    }
}
