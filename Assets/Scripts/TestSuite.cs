using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Test Suite written to perform unit tests on the specific scene of Mind Explorer VR
public class TestSuite : MonoBehaviour {

    public bool runTests;
    public bool writeToFile;

    public int passed;
    public int failed;

    public LineModelReader m_LineModelReader;
    public FullLineModelRenderer m_fullLineModelReader;
    public CortexDrawer m_cortexDrawer;

    // Use this for initialization
    void Start () {

	}
	
	// Update is called once per frame
	void Update () {
        // Run tests on first frame only
        if (!runTests)
            return;

        print("Running tests....");
        runTests = false;

        string results = "";

        // DATA LOADING TESTS

        // Correct number of vertices loaded for line mdoel
        results += " \n Correct number of vertices loaded for line model: ";
        int vertsLoaded = m_fullLineModelReader.GetNumVerts();
        if(vertsLoaded == 500000)
        {
            passed++;
            results += "PASS \n";
        }
        else
        {
            failed++;
            results += "FAIL. Expected 500000, got " + vertsLoaded + " \n";
        }

        // Correct size of adj file loaded
        results += "\n Correct number of adj data loaded: ";
        int adjLoaded = m_fullLineModelReader.GetAdjListLength();
        if(adjLoaded == 500000)
        {
            passed++;
            results += "PASS \n";
        }
        else
        {
            failed++;
            results += "FAIL. Expected 500000, got " + adjLoaded + "\n";
        }


        // QUERY TESTS
        results += "\n FLAT query returns results: ";
        FLATData.FlatRes res = FLATData.Query(0, 0, 0, 200, 200, 200);
        if(res.numcoords > 0)
        {
            passed++;
            results += "PASS \n";
        }
        else
        {
            failed++;
            results += "FAIL \n";
        }

        results += "\n IsInside cube function behaves correctly: ";
        Vector3 lowerCorner = new Vector3(-1, -1, -1);
        Vector3 UpperCorner = new Vector3(1, 1, 1);
        bool corrRes = m_fullLineModelReader.IsWithinCube(new Vector3(0, 0, 0), lowerCorner, UpperCorner)
            && m_fullLineModelReader.IsWithinCube(new Vector3(1, 1, 1), lowerCorner, UpperCorner)
            && !m_fullLineModelReader.IsWithinCube(new Vector3(0,0,2), lowerCorner, UpperCorner);
        if(corrRes)
        {
            passed++;
            results += "PASS \n";
        }
        else
        {
            failed++;
            results += "FAIL \n";
        }


        // Messaging tests

        results += "\n Intial Message Search behaves correctly: ";
        Vector3 offset = m_cortexDrawer.GetMessageBoxOffset();
        Vector3 lowerCorner2 = new Vector3(0, 0, 0) + offset;
        Vector3 upperCorner2 = new Vector3(100, 100, 100) + offset;
        m_fullLineModelReader.BeginMessage(lowerCorner2, upperCorner2);
        if (m_fullLineModelReader.GetNumActiveVerts() == 2633)
        {
            passed++;
            results += "PASS \n";
        }
        else
        {
            failed++;
            results += "FAIL \n";
        }

        m_fullLineModelReader.IterateMessage();
        results += "\n First Iteration of Message behaves correctly: ";
        if (m_fullLineModelReader.GetNumActiveVerts() == 2762)
        {
            passed++;
            results += "PASS \n";
        }
        else
        {
            failed++;
            results += "FAIL \n";
        }

        m_fullLineModelReader.IterateMessage();
        results += "\n Second Iteration of Message behaves correctly: ";
        if (m_fullLineModelReader.GetNumActiveVerts() == 3022)
        {
            passed++;
            results += "PASS \n";
        }
        else
        {
            failed++;
            results += "FAIL \n";
        }

        m_fullLineModelReader.ResetMessage();

        // Connectivity Tests
         
        results += "\n Connectivity mode behaves correctly: ";
        m_fullLineModelReader.m_connectionRange = 1000;
        m_fullLineModelReader.ShowConnectivity(lowerCorner2, upperCorner2);
        if (m_fullLineModelReader.GetNumActiveVerts() == 3022)
        {
            passed++;
            results += "PASS \n";
        }
        else
        {
            failed++;
            results += "FAIL \n";
        }

        m_fullLineModelReader.ResetMessage();

        results += "\n Tests Ran: " + (passed + failed);
        results += "\n Tests passed: " + passed;
        results += "\n Tests failed: " + failed;

        if (writeToFile)
        {
            System.IO.StreamWriter file = new System.IO.StreamWriter(Application.dataPath + "\\testresults.txt");
            file.WriteLine(results);

            file.Close();
        }
        else
        {
            print(results);
        }

    }
}
