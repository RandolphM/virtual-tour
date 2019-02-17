﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using System.Reflection;

namespace Vodgets
{
    public class PhotonVector3 : PhotonState
    {
        // An Elm has its own onChanged event to forward network changes to the local call method.
        // It also has its own Change method to register for local change events.
        // When local change events occur the parent state is notified along with which client index has changed.
        class Elm
        {
            PhotonVector3 state;
            int which;
            public Vector3Event onChanged = new Vector3Event();
            public Elm(PhotonVector3 s, Vector3Event evt, UnityAction<Vector3> call, int w)
            {
                state = s;
                which = w;
                onChanged.AddListener(call);
                evt.AddListener(Change);
            }

            // Called when evt is triggered locally.
            private void Change(Vector3 v)
            {
                state.Change(v, which);
            }
        }

        List<Elm> clients = new List<Elm>();
        public void AddClient(Vector3Event evt, UnityAction<Vector3> call)
        {
            clients.Add(new Elm(this, evt, call, clients.Count));
        }

        // Called when evt is triggered locally.
        public void Change(Vector3 v, int which)
        {
#if USING_PHOTON
            // Check for special case where photon is not active.
            if (!this.photonView || !PhotonNetwork.connected || which >= clients.Count )
            {
                RpcChange(v);
                return;
            }

            if (!this.photonView.isMine)
                return;

            // When running networked and we own the view, notify all clients with a networked RPC.
            // The most common case is having only a single client of this state type for each view.
            // When only one client exists we do not need the extra overhead of sending an index.
            // When multiple clients exist we send the client index. 
            PhotonView photonView = PhotonView.Get(this);
            if ( which == 0)
                photonView.RPC("RpcChange", PhotonTargets.All, v);
            else
                photonView.RPC("RpcListChange", PhotonTargets.All, v, which);
#else
            // When running standalone just notify the local client directly.
            RpcIndexedChange(v, which);
#endif
        }

#if USING_PHOTON
        [PunRPC]
#endif
        public void RpcChange( Vector3 v )
        {
            // < Communicate incoming rpc state change here >            
            clients[0].onChanged.Invoke(v);
        }

#if USING_PHOTON
        [PunRPC]
#endif
        public void RpcListChange(Vector3 v, int which)
        {
            // < Communicate incoming rpc state change here >
            clients[which].onChanged.Invoke(v);
        }
    }
}