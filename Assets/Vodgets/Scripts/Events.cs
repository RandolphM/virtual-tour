using UnityEngine;
using UnityEngine.Events;

namespace Vodgets
{
    [SerializeField]
    public class BoolEvent : UnityEvent<bool> { }

    [SerializeField]
    public class Vector3Event : UnityEvent<Vector3> { }
}