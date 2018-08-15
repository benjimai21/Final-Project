using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FullLineModelRenderer : MonoBehaviour {

    private bool m_messagingActive;

    public float m_fadeRate;

    public Color m_defaultVertexCol = new Color(0.0f, 0.0f, 1.0f, 1.0f);
    public Color m_activatedvertexCol = new Color(1.0f, 0.0f, 0.0f, 1.0f);
    public Color m_connectivityVertexCol;
    public Color m_connectivityOutlineCol;

    // Game object containing meshes used to give an 'outline' effect during connectivity mode
    public GameObject m_OutlineModelObj;

    private const int MAX_VERTICES_PER_MESH = 64998;
    private int MaxVertIndexPerMesh = ((MAX_VERTICES_PER_MESH / 4) - 1);

    // Number of steps taken for vertices to fade back to original colour
    public int m_steps;

    // Range of connectivity shown
    public int m_connectionRange;

    //Controls speed of message (bigger number = slower)
    public int m_iterationDelay;
    private int m_iterationCounter;

    public LineModelReader m_LineModelReader;

    private int m_numVerts;
    private int m_numMeshes;
    // List to track which vertices are contained in which mesh (meshVerts[meshInd][VertInd])
    // private List<List<int>> m_meshVerts;
    // List to hold all vertices from file
    private List<Vector3> m_verts;
    // Lists to hold adjacency info from file
    private List<List<int>> m_adj;
    // List to hold state of each vertex for messaging
    private List<int> m_vertexState;
    // List of current colours for each vertex
    private List<Color> m_VertexColours;
    // List of current colours for the 'outline' model
    private List<Color> m_OutlineColours;
    // List of all mesh parts vertex managers
    private VertexManager[] m_VertexManagers;
    // list of all vertex managers for the 'outline' model
    //private VertexManager[] m_OutlineVertexManagers;
    // List of all currently active vertices
    private List<int> m_activeVerts;
    // List of all vertices activated on this message iteration
    private List<int> m_newActiveVerts;

    // Load mesh/vertex data of line model
    void Start()
    {
        MeshInfo mi = MeshData.LoadMeshInfo();
        m_numVerts = mi.numVerts;
        // if numVert is -1, no mesh data found. Build a new line model mesh
        if (m_numVerts == -1)
        {
            m_LineModelReader.BuildLineMeshes();
            mi = m_LineModelReader.GetMeshInfo();
            m_numVerts = mi.numVerts;

            // If numVerts still -1, build failed
            if(m_numVerts == -1)
            {
                print("ERROR: Line mesh build failed");
                return;
            }
        }

        m_numMeshes = mi.numMeshes;
        //m_meshVerts = mi.meshVerts;

        if (m_numMeshes == -1)
            print("-1 meshes");

        // Initialise all vertex states to -2 (unactivated)
        m_vertexState = new List<int>();
        for (int i = 0; i < m_numVerts; i++)
            m_vertexState.Add(-2);

        // Get each VertexManager component (should be children of this gameobject)
        // and add them to m_VertexManagers list accordingly based on meshID
        m_VertexManagers = new VertexManager[m_numMeshes];
        foreach (VertexManager vd in transform.GetComponentsInChildren<VertexManager>())
        {
            int meshID = vd.m_MeshID;
            m_VertexManagers[meshID] = vd;
        }

        // do the same for the 'outline' model
        //m_OutlineVertexManagers = new VertexManager[m_numMeshes];
        //foreach (VertexManager vd in m_OutlineModelObj.transform.GetComponentsInChildren<VertexManager>())
        //{
        //    int meshID = vd.m_MeshID;
        //    m_OutlineVertexManagers[meshID] = vd;
        //}

        // Get Vertex and Adjacancy data from reader
        m_verts = m_LineModelReader.GetVertices();
        m_adj = m_LineModelReader.GetAdjData();

        // Initialise colour list
        m_VertexColours = new List<Color>();
        m_OutlineColours = new List<Color>();
        for (int i = 0; i < m_numVerts*4; i++)
        {
            // Colour main model with given colour
            m_VertexColours.Add(m_defaultVertexCol);

            // Set 'outline' to transparent
            m_OutlineColours.Add(new Color(0, 0, 0, 0));
        }

        m_iterationCounter = 0;

        SetVertexColours();

        print("Loaded data successfully!");
    }

    // Begins messaging from vertices containes within cube given by corners lowerc and upperc
    public void BeginMessage(Vector3 lowerc, Vector3 upperc)
    {

        ResetMessage();

        m_messagingActive = true;
        m_iterationCounter = 0;

        m_activeVerts = new List<int>();

        // Check all verts for those in initial cube
        for(int i = 0; i < m_numVerts; i++)
        {
            if(IsWithinCube(m_verts[i], lowerc, upperc))
            {
                m_vertexState[i] = m_steps;
                m_activeVerts.Add(i);
            }
        }
    }

    // Perform one iteration of message propagation
    public void IterateMessage()
    {
        m_newActiveVerts = new List<int>();
        List<int> completeVerts = new List<int>();

        float colChangeFactor = 1f / (m_steps / 2f);
        Color fadeActiveCol = new Color(m_activatedvertexCol.r * colChangeFactor, m_activatedvertexCol.g * colChangeFactor, m_activatedvertexCol.b * colChangeFactor, 0);
        Color fadeDefaultCol = new Color(m_defaultVertexCol.r * colChangeFactor, m_defaultVertexCol.g * colChangeFactor, m_defaultVertexCol.b * colChangeFactor, 0);

        //Run through all active vertices and Set colours
        for (int i = 0; i < m_activeVerts.Count; i++)
        {
            int vert = m_activeVerts[i];
            int state = m_vertexState[vert];

            // If vertex only just activated
            if(state == m_steps)
            {
                int numNeighbours = m_adj[vert].Count;

                // For each adjacent vertex
                for(int j = 0; j < numNeighbours; j++)
                {
                    int neighbour = m_adj[vert][j];
                    //If neighbour inactive, activate it
                    if(m_vertexState[neighbour] == -2)
                    {
                        m_vertexState[neighbour] = m_steps;
                        m_newActiveVerts.Add(neighbour);
                    }
                }
            }

            // Shift colour to activated
            if( state <= m_steps && state >= m_steps/2)
            {
                int[] vs = FileIndexToVertexIndex(vert);
                for (int j = 0; j < vs.Length; j++)
                {
                    m_VertexColours[vs[j]] -= fadeDefaultCol;
                    m_VertexColours[vs[j]] += fadeActiveCol;
                }

                m_vertexState[vert]--;
            }

            //Move colour back to default
            else if (state <= m_steps/2 && state > 0)
            {
                int[] vs = FileIndexToVertexIndex(vert);
                for(int j=0; j < vs.Length; j++)
                {
                    m_VertexColours[vs[j]] += fadeDefaultCol;
                    m_VertexColours[vs[j]] -= fadeActiveCol;
                }

                m_vertexState[vert]--;
            }

            // All steps complete. Set back to default col, remove from active list and set state to -1 (done)
            else if (state == 0)
            {
                int[] vs = FileIndexToVertexIndex(vert);
                for (int j = 0; j < vs.Length; j++)
                {
                    m_VertexColours[vs[j]] = m_defaultVertexCol;
                    m_VertexColours[vs[j]] = m_defaultVertexCol;
                }
                m_vertexState[vert] = -1;
                completeVerts.Add(vert);
            }
        }

        // Remove complete verts from activeVerts
        foreach(int vert in completeVerts)
        {
            m_activeVerts.Remove(vert);
        }

        // Add new active verts
        m_activeVerts.AddRange(m_newActiveVerts);

        // If we're out of active verts, finish message propagation
        if (m_activeVerts.Count == 0)
        {
            m_messagingActive = false;
            ResetMessage();
        }

        SetVertexColours();
    }

    private void SetVertexColours()
    {
        // Pass colours to vertexManagers
        for(int i = 0; i < m_numMeshes; i++)
        {
            VertexManager vm = m_VertexManagers[i];
            int meshVertCount = vm.m_VertexIDs.Count*4;
            vm.SetMeshCols(m_VertexColours.GetRange(MaxVertIndexPerMesh * i * 4, meshVertCount));

            //VertexManager vmOutline = m_OutlineVertexManagers[i];
            //vmOutline.SetMeshCols(m_OutlineColours.GetRange(MaxVertIndexPerMesh * i * 4, meshVertCount));
        }
    }

    // Resets state and colour of all vertices
    public void ResetMessage()
    {
        m_messagingActive = false;
        m_newActiveVerts = new List<int>();
        for (int i = 0; i < m_vertexState.Count; i++)
        {
            m_vertexState[i] = -2;
            int[] vs = FileIndexToVertexIndex(i);
            for (int j = 0; j < vs.Length; j++)
            {
                m_VertexColours[vs[j]] = m_defaultVertexCol;

                m_OutlineColours[vs[j]] = new Color(0, 0, 0, 0);
            }
        }

        SetVertexColours();
    }

    // Returns true if given vector a is within the cube made by corners lowerc and upperc
    public bool IsWithinCube(Vector3 a, Vector3 lowerc, Vector3 upperc)
    {
        return
            a.x >= lowerc.x && a.x <= upperc.x
            && a.y >= lowerc.y && a.y <= upperc.y
            && a.z >= lowerc.z && a.z <= upperc.z;
    }

    // Converts from vertex index in data file to vertex index used with meshes
    public int[] FileIndexToVertexIndex(int fileInd)
    {
        int[] res = new int[4];
        for(int i = 0; i < 4; i++)
            res[i] = fileInd * 4 + i;

        return res;
    }

    // No longer face in/out due to line model shader behaving oddly with 
    // transparency
    public void StartFadeOut()
    {
        gameObject.SetActive(false);
    }

    public void StartFadeIn()
    {
        gameObject.SetActive(true);
    }

    void Update()
    {
        if (m_messagingActive && (m_iterationCounter % m_iterationDelay == 0))
        {
            IterateMessage();
            m_iterationCounter = 0;
        }

        m_iterationCounter++;
    }

    public void ShowConnectivity(Vector3 lowerCorner, Vector3 upperCorner)
    {
        ResetMessage();

        List<int> activeVerts = new List<int>();
        List<int> newActiveVerts = new List<int>();
        bool done = false;

        // Check all verts for those in initial cube
        for (int i = 0; i < m_numVerts; i++)
        {
            if (IsWithinCube(m_verts[i], lowerCorner, upperCorner))
            {
                activeVerts.Add(i);
                // Using state of -3 to indicate vertex has been reached
                m_vertexState[i] = -3;
            }
        }

        int range = 0;

        while (!done && range <= m_connectionRange)
        {
            //Run through all active vertices and Set colours
            for (int i = 0; i < activeVerts.Count; i++)
            {
                int vert = activeVerts[i];

                int numNeighbours = m_adj[vert].Count;
                if (numNeighbours > 3)
                    range++;

                // for each adjacent vertex
                for (int j = 0; j < numNeighbours; j++)
                {
                    int neighbour = m_adj[vert][j];
                    //If neighbour inactive, activate it
                    if (m_vertexState[neighbour] == -2)
                    {
                        // Using state of -3 to indicate vertex has been reached
                        m_vertexState[neighbour] = -3;
                        newActiveVerts.Add(neighbour);
                    }
                }

                // Set colour of vertex to 'connected' and enable outline of that vertex
                int[] vs = FileIndexToVertexIndex(vert);
                for (int j = 0; j < vs.Length; j++)
                {
                    m_VertexColours[vs[j]] = m_connectivityVertexCol;
                    m_OutlineColours[vs[j]] = m_connectivityOutlineCol;
                }
            }

            // clear active verts and replace with new active verts
            activeVerts.Clear();
            activeVerts.AddRange(newActiveVerts);
            newActiveVerts.Clear();

            if (activeVerts.Count == 0)
            {
                done = true;
            }

        }

        SetVertexColours();
    }

    // For testing
    public int GetNumVerts()
    {
        return m_numVerts;
    }

    public int GetNumMeshes()
    {
        return m_numMeshes;
    }

    public int GetAdjListLength()
    {
        return m_adj.Count;
    }

    public int GetNumActiveVerts()
    {
        return m_activeVerts.Count;
    }

}
