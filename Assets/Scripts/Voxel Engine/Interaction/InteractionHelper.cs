using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class InteractionHelper
{
    /// <summary>
    /// Direction of cube face of point in cube grid
    /// </summary>
    /// <param name="localPos">Pos of point relative to cube center</param>
    /// <param name="size"></param>
    /// <returns></returns>
    public static Vector3 Cube(Vector3 localPos, Vector3 size)
    {
        size /= 2f; // compare it with half size
        if (Mathf.Abs(localPos.x - +size.x) < 1e-2f)
        {
            // right
            return Vector3.up;
        }
        else if (Mathf.Abs(localPos.x - -size.x) < 1e-2f)
        {
            // left
            return Vector3.up;
        }

        else if (Mathf.Abs(localPos.y - +size.y) < 1e-2f)
        {
            // right
            return Vector3.forward;
        }
        else if (Mathf.Abs(localPos.y - -size.y) < 1e-2f)
        {
            // left
            return Vector3.forward;
        }

        else if (Mathf.Abs(localPos.z - +size.z) < 1e-2f)
        {
            // right
            return Vector3.up;
        }
        else if (Mathf.Abs(localPos.z - -size.z) < 1e-2f)
        {
            // left
            return Vector3.up;
        }
        return Vector3.forward;
    }

}
