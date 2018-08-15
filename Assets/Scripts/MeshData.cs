using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

[Serializable]
public struct MeshInfo
{
    public int numVerts;
    public int numMeshes;
    // List to track which vertices are contained in which mesh (meshVerts[meshInd][VertInd])
    public List<List<int>> meshVerts;
}

public static class MeshData  {

    public static void SaveMeshInfo(MeshInfo mi)
    {
        BinaryFormatter binary = new BinaryFormatter();        
        FileStream fStream = File.Create(Application.dataPath + "/StreamingAssets/MeshInfo.minfo");
        binary.Serialize(fStream, mi);
        fStream.Close();
    }

    public static MeshInfo LoadMeshInfo()
    {
        MeshInfo mi;
        if (File.Exists(Application.dataPath + "/StreamingAssets/MeshInfo.minfo"))
        {
            BinaryFormatter binary = new BinaryFormatter();
            FileStream fStream = File.Open(Application.dataPath + "/StreamingAssets/MeshInfo.minfo", FileMode.Open);
            mi = (MeshInfo)binary.Deserialize(fStream);
            fStream.Close();
        }
        else
        {
            mi = new MeshInfo();
            mi.numVerts = -1;
            mi.numMeshes = -1;
        }

        return mi;
    }

}
