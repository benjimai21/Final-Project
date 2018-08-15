using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshColorTest : MonoBehaviour {

    private Mesh mesh;
    private int counter;
    private bool alt;


    // Use this for initialization
    void Start () {
        mesh = new Mesh();
        List<Vector3> verts = new List<Vector3>();
        verts.Add(new Vector3(0, 0, 0));
        verts.Add(new Vector3(0, 0, 1));
        verts.Add(new Vector3(1, 0, 0));
        verts.Add(new Vector3(1, 0, 1));

        List<Vector2> UVs = new List<Vector2>();
        List<int> triangles = new List<int>();

        triangles.Add(0);
        triangles.Add(2);
        triangles.Add(1);

        triangles.Add(1);
        triangles.Add(2);
        triangles.Add(3);

        for (int j = 0; j < 4; j++)
        {
            UVs.Add(new Vector2(verts[j].x, verts[j].z));
        }

        mesh.vertices = verts.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = UVs.ToArray();

        GetComponent<MeshFilter>().mesh = mesh;
        Color[] cs = { Color.blue, Color.blue, Color.green, Color.green };
        mesh.colors = cs;

        counter = 0;
        alt = false;
    }
	
	// Update is called once per frame
	void Update () {
        counter++; 
        if(counter >= 30)
        {
            Color[] cs =  new Color[4];
            counter = 0;
            if (alt)
            {
                cs[0] = Color.green;
                cs[1] = Color.green;
                cs[2] = Color.red;
                cs[3] = Color.cyan; 
                
            }
            else
            {
                cs[0] = Color.blue;
                cs[1] = Color.yellow;
                cs[2] = Color.magenta;
                cs[3] = Color.white;
            }
            alt = !alt;
            GetComponent<MeshFilter>().mesh.colors = cs;
        }
	}
}
