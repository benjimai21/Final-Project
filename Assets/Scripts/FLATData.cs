using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Runtime.InteropServices;
using System;
using System.IO;
#if UNITY_EDITOR_32
using UnityEditor;
[InitializeOnLoad]
#elif UNITY_EDITOR_64
using UnityEditor;
[InitializeOnLoad]
#endif
public static class FLATData {

    public struct FlatRes
    {
        public float[] coords;
        public int numcoords;
    };

    public static string path;

    static FLATData() // static Constructor
    {
        var currentPath = Environment.GetEnvironmentVariable("PATH",
            EnvironmentVariableTarget.Process);
#if UNITY_EDITOR_32
    var dllPath = Application.dataPath + "/Plugins/";
#elif UNITY_EDITOR_64
        var dllPath = Application.dataPath + "/Plugins/";
#else // Player
    var dllPath = Application.dataPath + "/Plugins/";

#endif
        if (currentPath != null && currentPath.Contains(dllPath) == false)
            Environment.SetEnvironmentVariable("PATH", currentPath + Path.PathSeparator
                + dllPath, EnvironmentVariableTarget.Process);

        path = dllPath;

    }

    public static string GetPath()
    {
        return path;
    }

    [DllImport("FLATDLL2")]
    private static extern bool PerformQuery(ref IntPtr ptrResVerts, ref int resVertsLen, 
        float p0, float p1, float p2, float p3, float p4, float p5);

    [DllImport("FLATDLL2")]
    private static extern bool DeleteFlatManager();

    public static FlatRes Query(float p0, float p1, float p2, float p3, float p4, float p5)
    {
        IntPtr ptrResVerts = IntPtr.Zero;
        int resVertsLen = 0;

        bool success = PerformQuery(ref ptrResVerts, ref resVertsLen, p0, p1, p2, p3, p4, p5);

        FlatRes r = new FlatRes();
        r.coords = null;
        r.numcoords = -1;
        if(success)
        {
            r.numcoords = resVertsLen;
            r.coords = new float[resVertsLen];

            Marshal.Copy(ptrResVerts, r.coords, 0, resVertsLen);
        }

        //Clear memory in from DLL call
        DeleteFlatManager();

        return r; 
    }
}
