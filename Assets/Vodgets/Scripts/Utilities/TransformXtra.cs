using UnityEngine;

//Extension methods must be defined in a static class
public static class VodgetsExtension
{
    // Invert a scale vector (private).
    static Vector3 InvertScale(Vector3 scale)
    {
        Vector3 invscale = new Vector3(1f / scale.x, 1f / scale.y, 1f / scale.z);
        return invscale;
    }

    //static public void SetLocal(this Transform A, Transform from)
    //{
    //    A.localPosition = from.localPosition;
    //    A.localRotation = from.localRotation;
    //    A.localScale = from.localScale;
    //}

    static public void SetLocal(this Transform A, Srt from)
    {
        A.localPosition = from.localPosition;
        A.localRotation = from.localRotation;
        A.localScale = from.localScale;
    }

    static public void SetLocal(this Transform A, Vector3 p, Quaternion r, Vector3 s)
    {
        A.localPosition = p;
        A.localRotation = r;
        A.localScale = s;
    }

    // Note: Commented the TransformUp and TransformDown out because they would not be useful. 

    //// Transform local values are converted to be a child of the sibling transform argument without changing location in the hierarchy.
    //// Note: This has the same affect as changing parent to sibling without changing world location. 
    //public static void TransformUp(this Transform A, Transform sibling)
    //{
    //    Quaternion inv_rot = Quaternion.Inverse(sibling.localRotation);
    //    Vector3 inv_scale = InvertScale(sibling.localScale);

    //    A.localPosition -= sibling.localPosition;
    //    A.localPosition = inv_rot * A.localPosition;
    //    A.localPosition = Vector3.Scale(A.localPosition, inv_scale);

    //    A.localRotation = inv_rot * A.localRotation;
    //    A.localScale = Vector3.Scale(A.localScale, inv_scale);
    //}

    //// Transform local values are converted to be a child of the sibling Srt argument if it existed in the transform hierarchy.
    //public static void TransformUp(this Transform A, Srt sibling)
    //{
    //    Quaternion inv_rot = Quaternion.Inverse(sibling.localRotation);
    //    Vector3 inv_scale = InvertScale(sibling.localScale);

    //    A.localPosition -= sibling.localPosition;
    //    A.localPosition = inv_rot * A.localPosition;
    //    A.localPosition = Vector3.Scale(A.localPosition, inv_scale);

    //    A.localRotation = inv_rot * A.localRotation;
    //    A.localScale = Vector3.Scale(A.localScale, inv_scale);
    //}

    //public static void TransformUp(this Transform A, Vector3 sibling_scale, Quaternion sibling_rot, Vector3 sibling_pos)
    //{
    //    Quaternion inv_rot = Quaternion.Inverse(sibling_rot);
    //    Vector3 inv_scale = InvertScale(sibling_scale);

    //    A.localPosition -= sibling_pos;
    //    A.localPosition = inv_rot * A.localPosition;
    //    A.localPosition = Vector3.Scale(A.localPosition, inv_scale);

    //    A.localRotation = inv_rot * A.localRotation;
    //    A.localScale = Vector3.Scale(A.localScale, inv_scale);
    //}

    //public static void TransformDown(this Transform A, Transform parent)
    //{
    //    A.localPosition = (parent.localRotation * Vector3.Scale(parent.localScale, A.localPosition)) + parent.localPosition;
    //    A.localRotation = parent.localRotation * A.localRotation;
    //    A.localScale = Vector3.Scale(parent.localScale, A.localScale);
    //}

    //public static void TransformDown(this Transform A, Srt parent)
    //{
    //    A.localPosition = (parent.localRotation * Vector3.Scale(parent.localScale, A.localPosition)) + parent.localPosition;
    //    A.localRotation = parent.localRotation * A.localRotation;
    //    A.localScale = Vector3.Scale(parent.localScale, A.localScale);
    //}

    //public static void TransformDown(this Transform A, Vector3 pnt_scale, Quaternion pnt_rot, Vector3 pnt_pos)
    //{
    //    A.localPosition = (pnt_rot * Vector3.Scale(pnt_scale, A.localPosition)) + pnt_pos;
    //    A.localRotation = pnt_rot * A.localRotation;
    //    A.localScale = Vector3.Scale(pnt_scale, A.localScale);
    //}

    // Rotate a transform about a sibling pivot point.
    static public void RotateAboutSiblingPoint(this Transform A, Vector3 pivot_pt, Quaternion dquat)
    {
        // If we created an Srt as a pivot frame converting frame to be a child would only change 
        // the frames position. 
        // Srt frame_pivot = ( pivot_pt, identity, 1f )
        // Srt frame_child = ( frame.localPosition - pivot_pt, frame.localRotation, frame.localScale )
        //
        // We then rotate frame_pivot by the users dquat.
        // frame_pivot = ( pivot_pt, dquat, 1f ) 
        // 
        // Converting frame_child back to world through frame_pivot yeilds.
        A.localPosition = (dquat * (A.localPosition - pivot_pt)) + pivot_pt;
        A.localRotation = dquat * A.localRotation;
    }

    // A point is transformed to its parent frame by first scaling, then rotating, then translating.
    public static Vector3 TransformPointDown(this Transform A, Vector3 child_pt )
    {
        Vector3 position = Vector3.Scale(child_pt, A.localScale);
        position = A.localRotation * position;
        position += A.localPosition;
        return position;
    }

    // A point is transformed to a child frame by reversing the position, then reversing the rotation 
    // and then reversing scale.
    public static Vector3 TransformPointUp(this Transform A, Vector3 sibling_pt )
    {
        Vector3 position = sibling_pt - A.localPosition;
        position = Quaternion.Inverse(A.localRotation) * position;
        position = Vector3.Scale(position, InvertScale(A.localScale));
        return position;
    }

}

// A convienience class
public class Srt
{
    public Vector3 localPosition;
    public Quaternion localRotation;
    public Vector3 localScale;

    public Srt()
    {
        Clear();
    }

    public Srt(Srt s)
    {
        Set(s);
    }

    public Srt(Vector3 p, Quaternion r, Vector3 s)
    {
        Set(p, r, s);
    }
    public Srt(Transform t)
    {
        Set(t);
    }

    public void Clear()
    {
        localPosition = Vector3.zero;
        localRotation = Quaternion.identity;
        localScale = Vector3.one;
    }

    public void Set(Srt s)
    {
        localPosition = s.localPosition;
        localRotation = s.localRotation;
        localScale = s.localScale;
    }

    public void Set(Vector3 pos, Quaternion rot, Vector3 s)
    {
        localPosition = pos;
        localRotation = rot;
        localScale = s;
    }

    public void Set(Transform t)
    {
        localPosition = t.localPosition;
        localRotation = t.localRotation;
        localScale = t.localScale;
    }

    public void Print(string tag)
    {
        Debug.Log(tag + ": " + localPosition + " " + localRotation + " " + localScale);
    }

    // Invert a scale vector.
    Vector3 InvertScale(Vector3 scale)
    {
        Vector3 invscale = new Vector3(1f / scale.x, 1f / scale.y, 1f / scale.z);
        return invscale;
    }

    public void TransformUp(Srt sibling)
    {
        Quaternion inv_rot = Quaternion.Inverse(sibling.localRotation);
        Vector3 inv_scale = InvertScale(sibling.localScale);

        localPosition -= sibling.localPosition;
        localPosition = inv_rot * localPosition;
        localPosition = Vector3.Scale(localPosition, inv_scale);

        localRotation = inv_rot * localRotation;
        localScale = Vector3.Scale(localScale, inv_scale);
    }

    public void TransformUp(Transform sibling)
    {
        Quaternion inv_rot = Quaternion.Inverse(sibling.localRotation);
        Vector3 inv_scale = InvertScale(sibling.localScale);

        localPosition -= sibling.localPosition;
        localPosition = inv_rot * localPosition;
        localPosition = Vector3.Scale(localPosition, inv_scale);

        localRotation = inv_rot * localRotation;
        localScale = Vector3.Scale(localScale, inv_scale);
    }

    public void TransformUp(Vector3 sibling_scale, Quaternion sibling_rot, Vector3 sibling_pos)
    {
        Quaternion inv_rot = Quaternion.Inverse(sibling_rot);
        Vector3 inv_scale = InvertScale(sibling_scale);

        localPosition -= sibling_pos;
        localPosition = inv_rot * localPosition;
        localPosition = Vector3.Scale(localPosition, inv_scale);

        localRotation = inv_rot * localRotation;
        localScale = Vector3.Scale(localScale, inv_scale);
    }

    public void TransformDown(Srt parent)
    {
        localPosition = (parent.localRotation * Vector3.Scale(parent.localScale, localPosition)) + parent.localPosition;
        localRotation = parent.localRotation * localRotation;
        localScale = Vector3.Scale(parent.localScale, localScale);
    }
    public void TransformDown(Transform parent)
    {
        localPosition = (parent.localRotation * Vector3.Scale(parent.localScale, localPosition)) + parent.localPosition;
        localRotation = parent.localRotation * localRotation;
        localScale = Vector3.Scale(parent.localScale, localScale);
    }

    public void TransformDown( Vector3 pnt_scale, Quaternion pnt_rot, Vector3 pnt_pos )
    {
        localPosition = (pnt_rot * Vector3.Scale(pnt_scale, localPosition)) + pnt_pos;
        localRotation = pnt_rot * localRotation;
        localScale = Vector3.Scale(pnt_scale, localScale);
    }

    public void RotateAboutSiblingPoint(Vector3 pivot_pt, Quaternion dquat)
    {
        // If we created an Srt as a pivot frame converting frame to be a child would only change 
        // the frames position. 
        // Srt frame_pivot = ( pivot_pt, identity, 1f )
        // Srt frame_child = ( frame.localPosition - pivot_pt, frame.localRotation, frame.localScale )
        //
        // We then rotate frame_pivot by the users dquat.
        // frame_pivot = ( pivot_pt, dquat, 1f ) 
        // 
        // Converting frame_child back to world through frame_pivot yeilds.
        localPosition = (dquat * (localPosition - pivot_pt)) + pivot_pt;
        localRotation = dquat * localRotation;
    }

    public Vector3 TransformPointDown(Vector3 child_pt)
    {
        Vector3 position = Vector3.Scale(child_pt, localScale);
        position = localRotation * position;
        position += localPosition;
        return position;
    }

    public Vector3 TransformPointUp(Vector3 sibling_pt)
    {
        Vector3 position = sibling_pt - localPosition;
        position = Quaternion.Inverse(localRotation) * position;
        position = Vector3.Scale(position, InvertScale(localScale));
        return position;
    }


}

// Deprecated
public class TransformXtra
{
    // Copy the local transform location
    static public void CopyLocal(Transform from, Transform to)
    {
        to.localPosition = from.localPosition;
        to.localRotation = from.localRotation;
        to.localScale = from.localScale;
    }
    static public void CopyLocal(Srt from, Transform to)
    {
        to.localPosition = from.localPosition;
        to.localRotation = from.localRotation;
        to.localScale = from.localScale;
    }
    static public void CopyLocal(Transform from, Srt to)
    {
        to.localPosition = from.localPosition;
        to.localRotation = from.localRotation;
        to.localScale = from.localScale;
    }
    static public void CopyLocal(Srt from, Srt to)
    {
        to.localPosition = from.localPosition;
        to.localRotation = from.localRotation;
        to.localScale = from.localScale;
    }

    // A point is transformed to its parent frame by first scaling, then rotating, then translating.
    static public Vector3 PointToParent(Vector3 child_pt, Transform parent)
    {
        Vector3 position = Vector3.Scale(child_pt, parent.localScale);
        position = parent.localRotation * position;
        position += parent.localPosition;
        return position;
    }
    static public Vector3 PointToParent(Vector3 child_pt, Srt parent)
    {
        Vector3 position = Vector3.Scale(child_pt, parent.localScale);
        position = parent.localRotation * position;
        position += parent.localPosition;
        return position;
    }

    // A point is transformed to a child frame by reversing the position, then reversing the rotation 
    // and then reversing scale.
    static public Vector3 PointToChild(Vector3 sibling_pt, Transform sibling)
    {
        Vector3 position = sibling_pt - sibling.localPosition;
        position = Quaternion.Inverse(sibling.localRotation) * position;
        position = Vector3.Scale(position, InvertScale(sibling.localScale));
        return position;
    }
    static public Vector3 PointToChild(Vector3 sibling_pt, Srt sibling)
    {
        Vector3 position = sibling_pt - sibling.localPosition;
        position = Quaternion.Inverse(sibling.localRotation) * position;
        position = Vector3.Scale(position, InvertScale(sibling.localScale));
        return position;
    }

    // Invert a scale vector.
    static public Vector3 InvertScale(Vector3 scale)
    {
        Vector3 invscale = new Vector3(1f / scale.x, 1f / scale.y, 1f / scale.z);
        return invscale;
    }

    // Print a formatted vector3 with a user specified tag.
    static public void PrintVector3(string tag, Vector3 vec)
    {
        Debug.Log(tag + "(" + vec.x + ", " + vec.y + ", " + vec.z + ")");
    }

    // Rotate a transform about a sibling pivot point.
    static public void RotateAboutSiblingPoint(Transform A, Vector3 pivot_pt, Quaternion dquat)
    {
        // If we created an Srt as a pivot frame converting frame to be a child would only change 
        // the frames position. 
        // Srt frame_pivot = ( pivot_pt, identity, 1f )
        // Srt frame_child = ( frame.localPosition - pivot_pt, frame.localRotation, frame.localScale )
        //
        // We then rotate frame_pivot by the users dquat.
        // frame_pivot = ( pivot_pt, dquat, 1f ) 
        // 
        // Converting frame_child back to world through frame_pivot yeilds.
        A.localPosition = (dquat * (A.localPosition - pivot_pt)) + pivot_pt;
        A.localRotation = dquat * A.localRotation;
    }

    static public void RotateAboutSiblingPoint(Srt A, Vector3 pivot_pt, Quaternion dquat)
    {
        // If we created an Srt as a pivot frame converting frame to be a child would only change 
        // the frames position. 
        // Srt frame_pivot = ( pivot_pt, identity, 1f )
        // Srt frame_child = ( frame.localPosition - pivot_pt, frame.localRotation, frame.localScale )
        //
        // We then rotate frame_pivot by the users dquat.
        // frame_pivot = ( pivot_pt, dquat, 1f ) 
        // 
        // Converting frame_child back to world through frame_pivot yeilds.
        A.localPosition = (dquat * (A.localPosition - pivot_pt)) + pivot_pt;
        A.localRotation = dquat * A.localRotation;
    }

    static public void SiblingToChild(Transform A, Transform sibling)
    {
        Quaternion inv_rot = Quaternion.Inverse(sibling.localRotation);
        Vector3 inv_scale = InvertScale(sibling.localScale);

        A.localPosition -= sibling.localPosition;
        A.localPosition = inv_rot * A.localPosition;
        A.localPosition = Vector3.Scale(A.localPosition, inv_scale);

        A.localRotation = inv_rot * A.localRotation;
        A.localScale = Vector3.Scale(A.localScale, inv_scale);
    }

    static public void SiblingToChild(Srt A, Srt sibling)
    {
        Quaternion inv_rot = Quaternion.Inverse(sibling.localRotation);
        Vector3 inv_scale = InvertScale(sibling.localScale);

        A.localPosition -= sibling.localPosition;
        A.localPosition = inv_rot * A.localPosition;
        A.localPosition = Vector3.Scale(A.localPosition, inv_scale);

        A.localRotation = inv_rot * A.localRotation;
        A.localScale = Vector3.Scale(A.localScale, inv_scale);
    }

    static public void SiblingToChild(Srt A, Transform sibling)
    {
        Quaternion inv_rot = Quaternion.Inverse(sibling.localRotation);
        Vector3 inv_scale = InvertScale(sibling.localScale);

        A.localPosition -= sibling.localPosition;
        A.localPosition = inv_rot * A.localPosition;
        A.localPosition = Vector3.Scale(A.localPosition, inv_scale);

        A.localRotation = inv_rot * A.localRotation;
        A.localScale = Vector3.Scale(A.localScale, inv_scale);
    }

    static public void SiblingToChild(Transform A, Srt sibling)
    {
        Quaternion inv_rot = Quaternion.Inverse(sibling.localRotation);
        Vector3 inv_scale = InvertScale(sibling.localScale);

        A.localPosition -= sibling.localPosition;
        A.localPosition = inv_rot * A.localPosition;
        A.localPosition = Vector3.Scale(A.localPosition, inv_scale);

        A.localRotation = inv_rot * A.localRotation;
        A.localScale = Vector3.Scale(A.localScale, inv_scale);
    }

    static public void SiblingToChild(ref Vector3 A_scale, ref Quaternion A_rot, ref Vector3 A_pos, Vector3 sibling_scale, Quaternion sibling_rot, Vector3 sibling_pos)
    {
        Quaternion inv_rot = Quaternion.Inverse(sibling_rot);
        Vector3 inv_scale = InvertScale(sibling_scale);

        A_pos -= sibling_pos;
        A_pos = inv_rot * A_pos;
        A_pos = Vector3.Scale(A_pos, inv_scale);

        A_rot = inv_rot * A_rot;
        A_scale = Vector3.Scale(A_scale, inv_scale);
    }

    static public void ChildToSibling(Transform A, Transform parent)
    {
        A.localPosition = (parent.localRotation * Vector3.Scale(parent.localScale, A.localPosition)) + parent.localPosition;
        A.localRotation = parent.localRotation * A.localRotation;
        A.localScale = Vector3.Scale(parent.localScale, A.localScale);
    }
    static public void ChildToSibling(Srt A, Srt parent)
    {
        A.localPosition = (parent.localRotation * Vector3.Scale(parent.localScale, A.localPosition)) + parent.localPosition;
        A.localRotation = parent.localRotation * A.localRotation;
        A.localScale = Vector3.Scale(parent.localScale, A.localScale);
    }
    static public void ChildToSibling(Transform A, Srt parent)
    {
        A.localPosition = (parent.localRotation * Vector3.Scale(parent.localScale, A.localPosition)) + parent.localPosition;
        A.localRotation = parent.localRotation * A.localRotation;
        A.localScale = Vector3.Scale(parent.localScale, A.localScale);
    }
    static public void ChildToSibling(Srt A, Transform parent)
    {
        A.localPosition = (parent.localRotation * Vector3.Scale(parent.localScale, A.localPosition)) + parent.localPosition;
        A.localRotation = parent.localRotation * A.localRotation;
        A.localScale = Vector3.Scale(parent.localScale, A.localScale);
    }
    static public void ChildToSibling(ref Vector3 A_scale, ref Quaternion A_rot, ref Vector3 A_pos, Vector3 pnt_scale, Quaternion pnt_rot, Vector3 pnt_pos)
    {
        A_pos = (pnt_rot * Vector3.Scale(pnt_scale, A_pos)) + pnt_pos;
        A_rot = pnt_rot * A_rot;
        A_scale = Vector3.Scale(pnt_scale, A_scale);
    }


}