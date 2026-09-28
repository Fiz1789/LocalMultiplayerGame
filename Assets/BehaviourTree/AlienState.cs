using System;
using Unity.Properties;
using UnityEngine;
using RowdyRobots;
using Unity.Behavior;

[BlackboardEnum]
public enum AlienState
{
	Idle,
	Patrol,
	Chase,
	Attack,
	Check
}
