using UnityEngine;
using System.Collections.Generic;

namespace RowdyRobots
{
public static class NoiseManager
{
    public static List<PlayerWalk> activePlayers = new();
    
    // public static Vector3 GetNoisiestPlayerPosition()
    // {
    //     // Loop through activePlayers, find highest currentNoise, return position
    //     // You'll implement this when building Alien AI
    // }
}
}
// // In PlayerWalk Awake():
// NoiseManager.activePlayers.Add(this);

// // In PlayerWalk OnDestroy():
// NoiseManager.activePlayers.Remove(this);
