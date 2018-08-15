using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FullMeshCreator : MonoBehaviour {

    public GameObject m_MeshPartPrefab;
    public GameObject m_FullModelPrefab;
    public Material m_BaseCortexMat;
    private const int MAX_VERTECIS_PER_MESH = 64998; //64998; // 64998/69 = 942

    private int m_subMeshCount = 600;

    // Use this for initialization
    void Start () {

        print("Building Prefab Model...");

        System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();
        FLATData.FlatRes cortexData = FLATData.Query(100f, 100f, 250f, 1000f, 1000f, 1000f);
        watch.Stop();
        var elapsedMs = watch.ElapsedMilliseconds;
        print("Time taken for query in Ms: " + elapsedMs);
        //FLATData.FlatRes cortexData = FLATData.Query(250f, 1000f, -1669.26f, 1000f, 1895.73f, 3507.66f);

        print("Number verts returned: " + cortexData.numcoords);

        //FLATData.FlatRes cortexData = FLATData.Query(0f, 0f, 0f, 300f, 100f, 200f);
        GameObject fullModel = new GameObject();

        Vector3 min = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
        Vector3 max = new Vector3(-Mathf.Infinity, -Mathf.Infinity, -Mathf.Infinity);

        // Put data into vector3 form
        List<Vector3> cortexVertices = new List<Vector3>();
        for (int i = 0; i < cortexData.numcoords; i += 3)
        {
            //Vector3 v = transform.TransformPoint(new Vector3(cortexData.coords[i], cortexData.coords[i + 1], cortexData.coords[i + 2]));
            Vector3 v = new Vector3(cortexData.coords[i], cortexData.coords[i + 1], cortexData.coords[i + 2]);

            min.x = Mathf.Min(min.x, v.x);
            min.y = Mathf.Min(min.y, v.y);
            min.z = Mathf.Min(min.z, v.z);

            max.x = Mathf.Max(max.x, v.x);
            max.y = Mathf.Max(max.y, v.y);
            max.z = Mathf.Max(max.z, v.z);

            Vector3 p = CoordinateConvertion.FlatToModel(v);

            cortexVertices.Add(p);
        }

        print("Min: " + min.ToString());
        print("Max: " + max.ToString());

        int vertexCount = cortexVertices.Count;
        watch.Reset();
        watch = System.Diagnostics.Stopwatch.StartNew();
        
        int counter = 0;
        if (vertexCount > 0)
        {

            int numMeshesRequired = (vertexCount / MAX_VERTECIS_PER_MESH) + 1;

            for (int i = 0; i < numMeshesRequired; i++)
            {

                Mesh mesh = new Mesh();
                mesh.subMeshCount = m_subMeshCount;
                List<Vector2> UVs = new List<Vector2>();
                List<int> triangles = new List<int>();
                int startInd = i * MAX_VERTECIS_PER_MESH;
                int endInd = Math.Min(startInd + MAX_VERTECIS_PER_MESH, vertexCount);
                counter += endInd - startInd;
                Vector3[] vertices = cortexVertices.GetRange(startInd, endInd - startInd).ToArray();
                mesh.vertices = vertices;

                for (int j = 0; j < endInd - startInd; j++)
                {
                    UVs.Add(new Vector2(vertices[j].x, vertices[j].z));

                    if (j < endInd - startInd - 2 && (j % 3 == 0))
                    {
                        triangles.Add(j);
                        triangles.Add(j + 1);
                        triangles.Add(j + 2);
                    }
                }

                /*
                int trianglesPerSubmesh = (triangles.Count / 3) / m_subMeshCount;
                int remTri = (triangles.Count / 3) % m_subMeshCount;
                int triCounter = 0;

                // Split triangle list for each submesh
                for (int k=0; k < m_subMeshCount - 1; k++)
                {
                    mesh.SetTriangles(triangles.GetRange(triCounter, trianglesPerSubmesh*3), k);
                    triCounter += trianglesPerSubmesh * 3;
                }
                // Final set of triangles + remainders
                mesh.SetTriangles(triangles.GetRange(triCounter, ((trianglesPerSubmesh + remTri) * 3)), m_subMeshCount-1);
                */
                mesh.triangles = triangles.ToArray();
                mesh.uv = UVs.ToArray();
                mesh.RecalculateNormals();

                GameObject meshPart = Instantiate(m_MeshPartPrefab);
                meshPart.transform.SetParent(transform);


                /*
                // Create materials for each submesh
                Material[] m = new Material[m_subMeshCount];
                for (int k = 0; k < m_subMeshCount; k++)
                {
                    m[k] = m_BaseCortexMat;
                }
                meshPart.GetComponent<Renderer>().materials = m;
                */

                MeshFilter mf = meshPart.GetComponent<MeshFilter>();
                if (mf)
                {
                    mf.mesh = mesh;
                }

                meshPart.transform.SetParent(fullModel.transform);

                //UnityEditor.AssetDatabase.CreateAsset(mesh, "Assets/FullCortexModel/Small/MeshPart_small_" + i.ToString() + ".asset");
                //UnityEditor.AssetDatabase.CreateAsset(mesh, "Assets/FullCortexModel/Test3/MeshPart_test3_" + i.ToString() + ".asset");


            }
            //UnityEditor.AssetDatabase.SaveAssets();
            //PrefabUtility.ReplacePrefab(fullModel, m_FullModelPrefab, ReplacePrefabOptions.ConnectToPrefab);
            print("Done");

            print("vertex count: " + vertexCount.ToString());
            //print("counter: " + counter.ToString());
            
            //print("Min x: " + minX.ToString());
            //print("Min z: " + minZ.ToString());
            //print("Max x: " + maxX.ToString());
            //print("Max z: " + maxZ.ToString());
        }

        watch.Stop();
        long elapsedMs2 = watch.ElapsedMilliseconds;
        print("Model Build time in Ms: " + elapsedMs2);


    }

    // Update is called once per frame
    void Update () {
		
	}
}
