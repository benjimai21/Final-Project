using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoordinateConvertion {

    /*
    private static float m_scalex = 0.00110786603989463811937829614023291928431854f; //0.0042076896685808f;
    private static float m_scaley = 0.00210281448576328880185573378368610236763769f;
    private static float m_scalez = 0.0012362563068388153573939717051837772266135f;
    private static Vector3 shift = new Vector3(876.505f, 373.96f, 919.2f);
    */

    private static float m_scale = 0.0042076896685808f;
    private static Vector3 shift = new Vector3(194.385f, 911.41955f, 276.339f);


    // Converts point from line model scale to scale used by FLAT
    public static Vector3 ModelToFlat(Vector3 point)
    {
        float x = (point.x / m_scale) + shift.x;
        float y = (point.y / m_scale) + shift.y;
        float z = (point.z / m_scale) + shift.z;

        return new Vector3(x,y,z);
    }

    public static Vector3 FlatToModel(Vector3 point)
    {
        float x = (point.x - shift.x) * m_scale;
        float y = (point.y - shift.y) * m_scale;
        float z = (point.z - shift.z) * m_scale;

        return new Vector3(x, y, z);
    }
}
