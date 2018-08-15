using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Runtime.InteropServices;
using System.IO;

public class CortexDrawer : MonoBehaviour {

    public GameObject m_MeshPartPrefab;
    public GameObject m_MeshParts;
    public GameObject m_FullLineModel;
    public GameObject m_QueryBox;
    public GameObject m_MessageBox;
    public GameObject m_ConnectivityBox;
    public Flash m_queryBoxFlash;
    public NewtonVR.NVRButton m_ResetButton;
    public Display m_ScreenDisplay;

    //private bool m_fadingOut;
    //private bool m_fadingIn;

    public float m_fadeRate;

    private List<GameObject> m_MeshModelParts;
    private const int MAX_VERTICES_PER_MESH = 64998; // Multiple of 3 to keep triangles in order
    // Thread used for performing queries
    private Thread m_queryThread;
    // True if query has returned, and mesh hasn't been rendered yet.
    private bool m_renderDue;
    //Results of last query
    private FLATData.FlatRes m_latestCortexData;
    // Lower corner position of current query
    private Vector3 m_curBottomQueryCorner;
    // Upper corner position of current query
    private Vector3 m_curUpperQueryCorner;
    // center position of current query
    private Vector3 m_QueryCenter;
    private Vector3 m_DefaultQueryCenter;
    // Offset for message box
    private Vector3 m_MessageBoxOffset;
    // Bool tracking if a query is currently in progress
    private bool m_queryInProgress;
    // Bool to track if a query result is currently being shown
    private bool m_QueryShown;
    // Vector to store the current query scale when message mode is used
    private Vector3 m_CurQueryScale;

    //Default model position and scale
    private Vector3 m_DefaultModelPos;
    private Vector3 m_DefaultModelScale;
    private Quaternion m_DefaultModelRotation;
    private float m_defaultNewQueryScale = 1000;

    /*
    // Is scale/position transition in progress?
    private bool m_transitionActive;
    // Rate at which scale/position transition takes place
    public float m_transitionRate;
    // Float to track transition progress
    private float m_TransistionProg;

    // Storage vectors for scale/position transition after query
    private Vector3 m_TransStartPos;
    private Vector3 m_TransStartScale;
    private Quaternion m_TransStartRot;
    private Vector3 m_TransStartMeshPartsPos;
    private Vector3 m_TransStartQueryBoxPos;
    private Vector3 m_TransDestPos;
    private Vector3 m_TransDestScale;
    private Quaternion m_TransDestRot;
    private Vector3 m_TransDestMeshPartsPos;
    private Vector3 m_TransDestQueryBoxPos;
    */

    // Used to time queries 
    private System.Diagnostics.Stopwatch m_watch;
    private long m_QueryTime;


    // Use this for initialization
    void Start () {
        
        // Init mesh model list and add pre-loaded mesh parts to it
        m_MeshModelParts = new List<GameObject>();

        m_queryInProgress = false;
        m_QueryShown = false;

        //m_fadingOut = false;
        //m_fadingIn = false; 

        // Set default query center coords
        m_DefaultQueryCenter = m_FullLineModel.transform.localPosition * -1;
        m_QueryCenter = m_DefaultQueryCenter;
        m_MessageBoxOffset = m_DefaultQueryCenter;

        m_DefaultModelPos = transform.localPosition;
        m_DefaultModelScale = transform.localScale;
        m_DefaultModelRotation = transform.localRotation;
        m_CurQueryScale = m_DefaultModelScale;
    }

    //Resets model to startup state
    public void Reset()
    {
        if (m_queryInProgress)
            return;

        print("Reseting Model");
        transform.localPosition = m_DefaultModelPos;
        transform.localRotation = m_DefaultModelRotation;
        transform.localScale = m_DefaultModelScale;

        m_QueryBox.transform.localPosition = Vector3.zero;
        m_QueryBox.transform.localEulerAngles = Vector3.zero;
        m_QueryBox.transform.localScale = new Vector3(500f, 500f, 500f);

        m_MessageBox.transform.localPosition = Vector3.zero;
        m_MessageBox.transform.localEulerAngles = Vector3.zero;
        m_MessageBox.transform.localScale = new Vector3(200f, 200f, 200f);

        m_ConnectivityBox.transform.localPosition = Vector3.zero;
        m_ConnectivityBox.transform.localEulerAngles = Vector3.zero;
        m_ConnectivityBox.transform.localScale = new Vector3(200f, 200f, 200f);

        print("Destroying old mesh..");
        foreach (GameObject mesh in m_MeshModelParts)
        {
            Destroy(mesh);
        }
        m_MeshModelParts.Clear();

        //m_FullLineModel.SetActive(true);
        m_FullLineModel.GetComponent<FullLineModelRenderer>().StartFadeIn();

        m_QueryShown = false;

        // Set default query center coords
        m_QueryCenter = m_DefaultQueryCenter;
        m_MeshParts.transform.localPosition = -1 * m_QueryCenter;
        
    }


    public Vector3 GetQueryCenter()
    {
        return m_QueryCenter;
    }

    public Vector3 GetMessageBoxOffset()
    {
        return m_MessageBoxOffset;
    }

    // Puts model back to default scale, storing current scale
    public void SetModelToLineScale()
    {
        m_CurQueryScale = transform.localScale;
        transform.localScale = m_DefaultModelScale;
    }

    // Restores model to stored query scale
    public void RestoreQueryScale()
    {
        transform.localScale = m_CurQueryScale;
    }


    public void DrawNewQuery(float p0, float p1, float p2, float p3, float p4, float p5)
    {
        if (!m_queryInProgress)
        {
            print("performing Query...");
            m_queryBoxFlash.SetFlashActive(true);
            m_ScreenDisplay.ShowQueryLoading(true);
            m_queryInProgress = true;

            m_curBottomQueryCorner = new Vector3(p0, p1, p2);
            m_curUpperQueryCorner = new Vector3(p3, p4, p5);
            m_QueryCenter = (m_curUpperQueryCorner + m_curBottomQueryCorner) / 2;
            m_queryThread = new Thread(() => QueryThread(p0, p1, p2, p3, p4, p5));
            m_queryThread.Start();
        }
        else
        {
            print("Query already in progress!!");
        }
    }

    private void QueryThread(float p0, float p1, float p2, float p3, float p4, float p5)
    {
        m_watch = System.Diagnostics.Stopwatch.StartNew();
        m_latestCortexData = FLATData.Query(p0, p1, p2, p3, p4, p5);
        m_watch.Stop();
        m_QueryTime = m_watch.ElapsedMilliseconds;
        m_renderDue = true;
    }

    private void DrawModel(FLATData.FlatRes cortexData)
    {
        // Destroy old model
        if(m_MeshModelParts.Count > 0)
        {
            print("Destroying old mesh..");
            foreach (GameObject mesh in m_MeshModelParts)
            {
                Destroy(mesh);
            }
            m_MeshModelParts.Clear();
        }
        //m_FullLineModel.SetActive(false);
        m_FullLineModel.GetComponent<FullLineModelRenderer>().StartFadeOut();

        // Set meshPartMat to transparent to start
        m_MeshPartPrefab.GetComponent<Renderer>().sharedMaterial.color -= new Color(0, 0, 0, 1);

        print("Query done. Building Mesh...");
        //Reset model position/rotation/scale
        transform.localPosition = m_DefaultModelPos;
        transform.localRotation = m_DefaultModelRotation;
        float newQueryBoxScale = m_QueryBox.transform.localScale.x; // x,y,z all same
        float newScale = m_defaultNewQueryScale / newQueryBoxScale;
        transform.localScale = newScale * m_DefaultModelScale;
        m_CurQueryScale = transform.localScale;


        // Put data into vector3 form
        List<Vector3> cortexVertices = new List<Vector3>();
        for (int i = 0; i < cortexData.numcoords; i += 3)
        {
            Vector3 v = new Vector3(cortexData.coords[i], cortexData.coords[i + 1], cortexData.coords[i + 2]);

            cortexVertices.Add(v);
        }

        //print("Min: " + min.ToString());
        //print("Max: " + max.ToString());

        int vertexCount = cortexVertices.Count;

        m_watch = System.Diagnostics.Stopwatch.StartNew();

        if (vertexCount > 0)
        {

            // Calculate number of meshes needed
            int numMeshesRequired = (vertexCount / MAX_VERTICES_PER_MESH) + 1;

            for (int i = 0; i < numMeshesRequired; i++)
            {

                // Create mesh and vertecis, uv and triangles lists
                Mesh mesh = new Mesh();
                List<Vector2> UVs = new List<Vector2>();
                List<int> triangles = new List<int>();

                // Calculate start and end vert indexes for current mesh
                int startInd = i * MAX_VERTICES_PER_MESH;
                int endInd = Mathf.Min(startInd + MAX_VERTICES_PER_MESH, vertexCount);

                // Get vertices for current mesh
                Vector3[] vertices = cortexVertices.GetRange(startInd, endInd - startInd).ToArray();
               
                // Set mesh vertices
                mesh.vertices = vertices;

                // Set UVs and triangles for mesh
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

                mesh.triangles = triangles.ToArray();
                mesh.uv = UVs.ToArray();
                mesh.RecalculateNormals();

                // Instantiate new game object to hold mesh and set is as child of 'meshparts' object
                GameObject meshPart = Instantiate(m_MeshPartPrefab);
                meshPart.transform.SetParent(m_MeshParts.transform);

                // Add mesh to gameobject
                MeshFilter mf = meshPart.GetComponent<MeshFilter>();
                if (mf)
                {
                    mf.mesh = mesh;
                }

                // Add to mesh parts list
                m_MeshModelParts.Add(meshPart);

                // Reset transform of new game object
                meshPart.transform.localPosition = new Vector3(0, 0, 0);
                meshPart.transform.localScale = new Vector3(1, 1, 1);
            }
        }

        print("Model build Time: " + m_watch.ElapsedMilliseconds);

        // Center the new model
        m_MeshParts.transform.localPosition = -1 * m_QueryCenter;
        m_QueryBox.transform.localPosition = Vector3.zero;

        // Start transition of centering and resizing new model
        //ModelTransition();
        StartQueryFadeIn();

        m_QueryShown = true;

        // Set control mode back to explore
        ControlModeManager cmm = GetComponent<ControlModeManager>();
        if(cmm)
            cmm.SetControlMode(ControlModeManager.CONTROL_MODE.QUERY_MODEL);

        // Deactivate 'loading' visuals
        m_queryInProgress = false;
        m_queryBoxFlash.SetFlashActive(false);
        m_ScreenDisplay.ShowQueryLoading(false);
        print("Done");
    }

    /*
    private void ModelTransition()
    {
        m_transitionActive = true;
        m_TransStartPos = transform.localPosition;
        m_TransStartScale = transform.localScale;
        m_TransStartRot = transform.localRotation;
        m_TransStartMeshPartsPos = m_MeshParts.transform.localPosition;
        m_TransStartQueryBoxPos = m_QueryBox.transform.localPosition;

        m_TransDestPos = m_DefaultModelPos;
        float newQueryBoxScale = m_QueryBox.transform.localScale.x; // x,y,z all same
        float newScale = m_defaultNewQueryScale / newQueryBoxScale;
        m_TransDestScale = newScale * m_DefaultModelScale;
        m_TransDestRot = m_DefaultModelRotation;
        m_TransDestMeshPartsPos = -1 * m_QueryCenter;
        m_TransDestQueryBoxPos = Vector3.zero;

        m_TransistionProg = 0;
    }
    */

    public bool IsQueryShown()
    {
        return m_QueryShown;
    }

    public void StartQueryFadeOut()
    {
        m_MeshParts.SetActive(false);
        //m_fadingOut = true;
        //m_fadingIn = false;
    }

    public void StartQueryFadeIn()
    {
       // m_fadingIn = true;
       // m_fadingOut = false;
        m_MeshParts.SetActive(true);
    }

    // Update is called once per frame
    void Update () {
        if (m_renderDue)
        {
            print("Query time: " + m_QueryTime);
            print("Number of ordinates returned: " + m_latestCortexData.numcoords);
            DrawModel(m_latestCortexData);
            m_renderDue = false;
        }

        /*
        if(m_fadingOut)
        {
            float lastA = 0;
            foreach (Renderer r in m_MeshParts.GetComponentsInChildren<Renderer>())
            {
                r.material.color -= new Color(0, 0, 0, m_fadeRate);
                lastA = r.material.color.a;
            }

            if (GetComponentsInChildren<Renderer>().Length == 0 || lastA <= 0)
                m_fadingOut = false;
        }
        
        if (m_fadingIn)
        {
            float lastA = 0;
            foreach (Renderer r in m_MeshParts.GetComponentsInChildren<Renderer>())
            {
                r.material.color += new Color(0, 0, 0, m_fadeRate);
                lastA = r.material.color.a;
            }
            
            if(GetComponentsInChildren<Renderer>().Length == 0 || lastA >= 1)
                m_fadingIn = false;
        }


        if(m_transitionActive)
        {
            // lerp between old and new. Stop when transProg >= 1
            m_TransistionProg += m_transitionRate;

            transform.localPosition = Vector3.Lerp(m_TransStartPos, m_TransDestPos, m_TransistionProg);
            transform.localScale = Vector3.Lerp(m_TransStartScale, m_TransDestScale, m_TransistionProg);
            transform.localRotation = Quaternion.Lerp(m_TransStartRot, m_TransDestRot, m_TransistionProg);

            m_MeshParts.transform.localPosition = Vector3.Lerp(m_TransStartMeshPartsPos, m_TransDestMeshPartsPos, m_TransistionProg);
            m_QueryBox.transform.localPosition = Vector3.Lerp(m_TransStartQueryBoxPos, m_TransDestQueryBoxPos, m_TransistionProg);

            if(m_TransistionProg >= 1)
                m_transitionActive = false;
        }
        */
    }
}
