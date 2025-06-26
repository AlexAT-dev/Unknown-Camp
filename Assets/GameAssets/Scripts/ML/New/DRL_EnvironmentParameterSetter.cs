using UnityEngine;
using Unity.MLAgents;
using System;

[Obsolete]
public class DRL_EnvironmentParameterSetter : MonoBehaviour
{
    public DRL_LearnerCatcherAgentCurriculum agent;

    private void Start()
    {
        var envParams = Academy.Instance.EnvironmentParameters;

        agent.targetCount = (int)envParams.GetWithDefault("target_count", 1);
        agent.targetsAreMoving = envParams.GetWithDefault("targets_moving", 0f) > 0f;
        agent.useAdvancedDecision = envParams.GetWithDefault("use_advanced", 0f) > 0f;
    }
}
