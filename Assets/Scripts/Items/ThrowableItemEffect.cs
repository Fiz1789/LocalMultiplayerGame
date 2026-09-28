using UnityEngine;

namespace RowdyRobots
{
    /// <summary>
    /// Base class for all throwable item on-land effects.
    /// Create a subclass for each effect type (Electrify, Freeze, Knockback, etc.)
    /// </summary>
    public abstract class ThrowableItemEffect : ScriptableObject
    {
        /// <summary>
        /// Called when the thrown item lands. 
        /// Use 'runner' to start coroutines since ScriptableObjects can't do so themselves.
        /// </summary>
        public abstract void Apply(Vector3 position, MonoBehaviour runner);
    }
}
