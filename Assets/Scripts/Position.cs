using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// Serializable version of a Vector3, used for storage

[Serializable]
public class Position
{
    public float x = 0;
    public float y = 0;
    public float z = 0;

    public Position(Transform transform)
    {
        x = transform.position.x;
        y = transform.position.y;
        z = transform.position.z;
    }

    public Position(Vector3 v)
    {
        x = v.x;
        y = v.y;
        z = v.z;
    }

    public Position(float posX, float posY, float posZ)
    {
        x = posX;
        y = posY;
        z = posZ;
    }

    public Vector3 GetVector()
    {
        return new Vector3(x, y, z);
    }
}

