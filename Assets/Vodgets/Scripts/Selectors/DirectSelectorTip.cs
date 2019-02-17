using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vodgets
{

    public class DirectSelectorTip : MonoBehaviour
    {
        public DirectSelector selector = null;

        private void Awake()
        {
            if (selector != null)
                selector.SetTip(transform);
        }

        void OnTriggerEnter(Collider other)
        {
            if (selector != null)
                selector.OnTriggerEnter(other);
        }

        void OnTriggerExit(Collider other)
        {
            if (selector != null)
                selector.OnTriggerExit(other);
        }
    }
}