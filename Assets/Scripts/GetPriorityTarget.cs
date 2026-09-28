using System;
using Unity.Properties;
using UnityEngine;
using RowdyRobots;

namespace Unity.Behavior
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Get Priority Target",
        description: "Continuously checks for the noisiest visible player within the alien's vision cone and updates the PriorityTarget Blackboard variable.",
        category: "Action/Find",
        story: "[Agent] finds priority target based on noise and visibility",
        id: "2a02698a-3b5f-47cc-a0d0-531fde51e95d")]
    internal partial class GetPriorityTarget : Action
    {
        // === BLACKBOARD VARIABLES (Inputs/Outputs) ===
        // Follows exact PatrolAction pattern: [SerializeReference] + BlackboardVariable<Type> + default value
        [SerializeReference] public BlackboardVariable<GameObject> Agent;
        [SerializeReference] public BlackboardVariable<GameObject> CurrentTarget;
        [SerializeReference] public BlackboardVariable<int> VisiblePlayerCount;
        [SerializeReference] public BlackboardVariable<Vector3> LastNoisePosition;

        [SerializeReference] public BlackboardVariable<AlienState> CurrentAlienState;

        [SerializeReference] public BlackboardVariable<bool> ExitOnDetection = new(true);

        [SerializeReference] public BlackboardVariable<bool> IgnoreDistantNoise = new(false);

        [SerializeReference] public BlackboardVariable<bool> ExitOnChaseOnly = new(false);

        // === INTERNAL STATE (Serialized for graph save/load) ===
        // Follows exact PatrolAction pattern: [CreateProperty] + private + m_ prefix
        [CreateProperty] private AlienVisionCone m_VisionCone;
        [CreateProperty] private bool m_HasInitialized;

        [CreateProperty] private bool m_ReadyToSucceed;


        // === LIFECYCLE: OnStart ===
        // Follows exact PatrolAction pattern: validate inputs, initialize, return Status.Running
        protected override Status OnStart()
        {
            // Validate required inputs (same pattern as PatrolAction)
            if (Agent.Value == null)
            {
                LogFailure("Agent is null - cannot find priority target.");
                CurrentTarget.Value = null;
                VisiblePlayerCount.Value = 0;
                return Status.Running; // ← Always Running for continuous nodes (like PatrolAction)
            }

            // Cache vision cone component (same pattern as PatrolAction caching NavMeshAgent)
            m_VisionCone = Agent.Value.GetComponent<AlienVisionCone>();
            if (m_VisionCone == null)
            {
                LogFailure($"No AlienVisionCone component found on {Agent.Value.name}");
                CurrentTarget.Value = null;
                VisiblePlayerCount.Value = 0;
                return Status.Running; // ← Always Running
            }

            m_HasInitialized = true;

            // Return Running to keep node active (same as PatrolAction)
            return Status.Running;
        }

        // === LIFECYCLE: OnUpdate ===
        // Follows exact PatrolAction pattern: check validity, run logic, ALWAYS return Status.Running
        protected override Status OnUpdate()
        {
            if (Agent.Value == null || m_VisionCone == null)
                return Status.Running;

            ExecuteLogic();

            // Debug.Log($"[OnUpdate] CurrentTarget: {CurrentTarget.Value?.name ?? "NULL"}");
            // Debug.Log($"[OnUpdate] LastNoisePosition: {LastNoisePosition.Value}");

            if (ExitOnDetection.Value)
            {
                bool shouldExit = ExitOnChaseOnly.Value
                    ? CurrentTarget.Value != null  // Chase only — ignore LastNoisePosition
                    : CurrentTarget.Value != null || LastNoisePosition.Value != Vector3.zero; // Any detection

                if (shouldExit)
                {
                    if (m_ReadyToSucceed)
                    {
                        m_ReadyToSucceed = false;
                        return Status.Success;
                    }
                    m_ReadyToSucceed = true;
                    return Status.Running;
                }
                m_ReadyToSucceed = false;
            }

            return Status.Running;
        }
        // === LIFECYCLE: OnEnd ===
        // Follows exact PatrolAction pattern: cleanup/reset state
        protected override void OnEnd()
        {
            m_HasInitialized = false;
            // ← Do NOT clear CurrentTarget or LastNoisePosition here
            // Values need to persist for the next branch to read them
        }
        // === LIFECYCLE: OnDeserialize ===
        // Follows exact PatrolAction pattern: re-initialize after graph load
        protected override void OnDeserialize()
        {
            // Re-initialize after graph is loaded (same pattern as PatrolAction)
            if (Agent.Value != null && !m_HasInitialized)
            {
                m_VisionCone = Agent.Value.GetComponent<AlienVisionCone>();
                m_HasInitialized = true;
            }
        }

        // === MAIN LOGIC (private helper method, called by OnUpdate) ===
        // Follows PatrolAction pattern: private void method that does the actual work
        private void ExecuteLogic()
        {
            if (!m_HasInitialized || m_VisionCone == null)
            {
                CurrentTarget.Value = null;
                LastNoisePosition.Value = Vector3.zero;
                VisiblePlayerCount.Value = 0;
                return;
            }

            var allVisible = m_VisionCone.GetVisibleTargets();
            VisiblePlayerCount.Value = allVisible != null ? allVisible.Count : 0;

            var (target, detectionType) = m_VisionCone.DetectPriorityTarget();
            // Debug.Log($"[ExecuteLogic] detectionType: {detectionType} | target: {target?.name ?? "NULL"}");

            // ← ADD THIS RIGHT HERE, before anything else uses target
            if (detectionType == AlienVisionCone.DetectionType.DistantNoise && IgnoreDistantNoise.Value)
            {
                // CurrentTarget.Value = null;
                // LastNoisePosition.Value = Vector3.zero;
                return;
            }

            if (target != null)
            {
                float distance = Vector3.Distance(Agent.Value.transform.position, target.transform.position);
                var playerScript = target.GetComponent<PlayerWalk>();
                float noise = playerScript != null ? playerScript.CurrentNoise : -1f;

                bool shouldChase = detectionType == AlienVisionCone.DetectionType.Vision
                                || detectionType == AlienVisionCone.DetectionType.CloseNoise;

                if (shouldChase)
                {
                    CurrentTarget.Value = target;
                    LastNoisePosition.Value = target.transform.position; // ← track player every frame
                    CurrentAlienState.Value = AlienState.Chase;
                    // Debug.Log($"[GetPriorityTarget] CHASE via {detectionType}: {target.name}");
                }
                else
                {
                    CurrentTarget.Value = null;
                    LastNoisePosition.Value = target.transform.position;
                    CurrentAlienState.Value = AlienState.Check;
                }
            }
            else
            {
                // Player just disappeared — remember where they were
                if (CurrentTarget.Value != null)
                {
                    LastNoisePosition.Value = CurrentTarget.Value.transform.position;
                }
                CurrentTarget.Value = null;
                // Debug.Log($"[GetPriorityTarget] NONE | Visible: {VisiblePlayerCount.Value}");
            }
        }
    }
}