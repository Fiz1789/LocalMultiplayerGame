// using UnityEngine;
// using System.Collections.Generic;

// namespace RowdyRobots
// {
//     public class DebugVisionCone : MonoBehaviour
//     {
//         [Header("Debug Settings")]
//         [SerializeField] private bool showDebugInfo = true;
//         [SerializeField] private KeyCode debugKey = KeyCode.D;
        
//         private AlienVisionCone visionCone;
        
//         void Start()
//         {
//             visionCone = GetComponent<AlienVisionCone>();
//             if (visionCone == null)
//             {
//                 Debug.LogError($"[DebugVisionCone] No AlienVisionCone on {gameObject.name}!");
//             }
//         }
        
//         void Update()
//         {
//             if (Input.GetKeyDown(debugKey))
//             {
//                 TestVisionCone();
//             }
//         }
        
//         void TestVisionCone()
//         {
//             if (visionCone == null)
//             {
//                 Debug.LogError("[DebugVisionCone] VisionCone is null!");
//                 return;
//             }
            
//             Debug.Log("=== VISION CONE TEST ===");
            
//             // Test 1: Get all visible targets (uses the CORRECT logic from AlienVisionCone)
//             var visibleTargets = visionCone.GetVisibleTargets();
//             Debug.Log($"Visible targets count: {visibleTargets.Count}");
            
//             foreach (var target in visibleTargets)
//             {
//                 Debug.Log($"  - {target.name}");
//             }
            
//             // Test 2: Get priority target
//             var priorityTarget = visionCone.GetPriorityTarget();
//             if (priorityTarget != null)
//             {
//                 Debug.Log($"Priority target: {priorityTarget.name}");
//             }
//             else
//             {
//                 Debug.Log("No priority target found");
//             }
            
//             // Test 3: Manual raycast test (SIMPLIFIED - just for reference)
//             // NOTE: This is just a rough check - trust GetVisibleTargets() above!
//             Collider[] players = Physics.OverlapSphere(transform.position, 10f, LayerMask.GetMask("Player"));
//             Debug.Log($"Players in radius (raw): {players.Length}");
            
//             foreach (var player in players)
//             {
//                 Vector3 direction = (player.transform.position - transform.position).normalized;
//                 float angle = Vector3.Angle(transform.forward, direction);
//                 float distance = Vector3.Distance(transform.position, player.transform.position);

//                 Debug.Log($"  Player: {player.name} | Angle: {angle:F1}° | Distance: {distance:F1}");

//                 // Use combined layer mask for raycast
//                 int combinedMask = visionCone.ObstacleLayer.value | visionCone.TargetLayer.value;
//                 if (Physics.Raycast(transform.position, direction, out RaycastHit hit, distance, combinedMask))
//                 {
//                     // Compare root transforms and instance IDs for clarity
//                     bool isSameObject = hit.collider.gameObject == player;
//                     bool isSameRoot = hit.collider.transform.root == player.transform.root;
//                     Debug.Log($"    Raycast: Hit {hit.collider.name} | Hit InstanceID: {hit.collider.gameObject.GetInstanceID()} | Player InstanceID: {player.GetInstanceID()} | SameObject: {isSameObject} | SameRoot: {isSameRoot}");
//                     if (isSameObject || isSameRoot)
//                     {
//                         Debug.Log($"    Raycast: Hit player directly = VISIBLE (trust GetVisibleTargets)");
//                     }
//                     else
//                     {
//                         Debug.Log($"    Raycast: Hit {hit.collider.name} = BLOCKED");
//                     }
//                 }
//                 else
//                 {
//                     Debug.Log($"    Raycast: CLEAR = VISIBLE (trust GetVisibleTargets)");
//                 }
//             }
            
//             Debug.Log("=== END VISION CONE TEST ===");
//         }
        
//         // Draw debug sphere in Scene view
//         void OnDrawGizmosSelected()
//         {
//             Gizmos.color = Color.cyan;
//             Gizmos.DrawWireSphere(transform.position, 10f);
//         }
//     }
// }