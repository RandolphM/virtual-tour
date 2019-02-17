using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if USING_PHOTON
using Photon;
#endif

namespace Vodgets
{
#if USING_PHOTON
    public class PhotonState : Photon.MonoBehaviour
    {
        private void Start()
        {
            if ( photonView != null )
                photonView.ownershipTransfer = OwnershipOption.Takeover;
            //photonView.ObservedComponents.Add(this);
        }

        public void ChangeOwnership(bool v)
        {
            // Check for special case where photon is not active.
            if (!this.photonView || !PhotonNetwork.connected)
                return;

            if (v && ! photonView.isMine)
            {
                this.photonView.RequestOwnership();
            }
        }

    }
#else
    public class PhotonState : MonoBehaviour
    {
        public void ChangeOwnership(bool v)
        {
        }
    }
#endif
}
