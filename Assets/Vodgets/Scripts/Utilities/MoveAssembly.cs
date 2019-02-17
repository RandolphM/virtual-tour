using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vodgets_Student
{

    public class MoveAssembly : MonoBehaviour
    {

        public float min_height = 0f;
        public float max_height = 1f;

        public void SetPctHeight(float pct)
        {
            Vector3 pos = transform.localPosition;
            pos.y = min_height + (max_height - min_height) * (1f - pct);
            transform.localPosition = pos;
        }

        public void SetRotationDegrees(float degrees)
        {
            transform.localRotation = Quaternion.Euler(0f, degrees, 0f);
        }

        public void SetButtonState( bool state )
        {
            Debug.Log("Button:" + state);
        }
    }
}