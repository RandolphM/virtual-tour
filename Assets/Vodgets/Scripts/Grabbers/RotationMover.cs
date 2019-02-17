using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationMover : MonoBehaviour {

    bool fixed_mode = false;
    bool save_isKinematic = false;

    Rigidbody body = null;

    public void Set( bool m, bool sk )
    {
        fixed_mode = m;
        save_isKinematic = sk;
        body = gameObject.GetComponent<Rigidbody>();
        if (body == null)
            Destroy(this);
    }

    int mode = 0; 

	// Update is called once per frame
	void FixedUpdate () {

        // Rotation correction can induce unwanted velocities for kinematic objects.
        if (save_isKinematic)
        {
            body.velocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }

        mode = (++mode) % 2;
        Vector3 vec = (mode == 0 || !fixed_mode) ? Vector3.up : Vector3.forward;
        Quaternion dq = (fixed_mode) ? transform.localRotation : Quaternion.FromToRotation(transform.localRotation * vec, vec);

        float angle;
        Vector3 axis;
        dq.ToAngleAxis(out angle, out axis);

        //Debug.Log("Angle:" + angle);

        if (Mathf.Abs(angle) > 359f || Mathf.Abs(angle) < 1f )
        {
            if (fixed_mode)
                body.MoveRotation(Quaternion.identity);
            else
                body.MoveRotation(dq * transform.localRotation);

            body.isKinematic = save_isKinematic;
            Destroy(this);
        } else
        {
            body.angularVelocity = axis * angle;
        }
    }
}