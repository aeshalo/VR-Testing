using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEditor;


public static class ExtentionMethods {

    public static void ApplyEffectiveTorqueAtRelativePosition(this Rigidbody rigidbody,Vector3 torque, Vector3 position)
    {
        rigidbody.AddForce(Vector3.Cross(torque, rigidbody.transform.TransformVector(position))*(1/position.sqrMagnitude));
        //Debug.DrawRay(rigidbody.transform.position, rigidbody.transform.TransformVector(position), Color.blue);
        Debug.DrawRay(rigidbody.transform.position, Vector3.Cross(torque, rigidbody.transform.TransformVector(position)) * (1 / position.sqrMagnitude), Color.red);
    }
    public static IEnumerator Cooldown(float waitTime,System.Action<bool> var)
    {
        yield return new WaitForSecondsRealtime(waitTime);
        //Debug.Log(var + " Has Cooled Down");
        var(true);
    }


}


/*
[System.Serializable]
public class Line : MonoBehaviour
{
    [SerializeField]
    public Vector3 p0, p1;
}

[CustomEditor(typeof(Line))]
public class LineInspector : Editor
{
    private void OnSceneGUI()
    {
        Line line = target as Line;

    }
}
*/
