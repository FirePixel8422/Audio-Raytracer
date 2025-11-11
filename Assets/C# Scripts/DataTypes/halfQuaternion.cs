using System;
using Unity.Mathematics;



[System.Serializable]
public struct halfQuaternion
{
    public half x;
    public half y;
    public half z;

    public halfQuaternion(quaternion q)
    {
        // Ensure w is positive to resolve sign ambiguity
        if (q.value.w < 0)
        {
            x = (half)(-q.value.x);
            y = (half)(-q.value.y);
            z = (half)(-q.value.z);
        }
        else
        {
            x = (half)q.value.x;
            y = (half)q.value.y;
            z = (half)q.value.z;
        }
    }


    /// <summary>
    /// Value calculated through a regular quaternion
    /// </summary>
    public quaternion QuaternionValue
    {
        get
        {
            float xx = (float)x;
            float yy = (float)y;
            float zz = (float)z;
            float wSquared = 1f - (xx * xx + yy * yy + zz * zz);
            float w = wSquared > 0f ? math.sqrt(wSquared) : 0f;

            quaternion q = new quaternion(xx, yy, zz, w);
            return math.normalize(q);
        }
        set
        {
            quaternion q = value;
            if (q.value.w < 0f)
            {
                x = (half)(-q.value.x);
                y = (half)(-q.value.y);
                z = (half)(-q.value.z);
            }
            else
            {
                x = (half)q.value.x;
                y = (half)q.value.y;
                z = (half)q.value.z;
            }
        }
    }

    public static bool operator ==(halfQuaternion a, halfQuaternion b)
    {
        return a.x == b.x && a.y == b.y && a.z == b.z;
    }

    public static bool operator !=(halfQuaternion a, halfQuaternion b)
    {
        return !(a == b);
    }

    public override bool Equals(object obj)
    {
        return obj is halfQuaternion other && this == other;
    }
    public override int GetHashCode()
    {
        return HashCode.Combine(x, y, z);
    }
}