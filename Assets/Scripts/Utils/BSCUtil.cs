using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BSCUtil
{
   public static bool InRange(Vector3 target, Vector3 source, float range)
   {
        return (target - source).sqrMagnitude <= (range * range);
   }

    public static bool OutOfRange(Vector3 target, Vector3 source, float range)
    {
        return (target - source).sqrMagnitude > (range * range);
    }
}
