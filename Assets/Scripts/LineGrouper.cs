using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineGrouper {

    // We need to break vertices into groups of neighbouring segments to build larger meshes. 
    // Takes a list of lines represented by 2 vector3's, and groups them into lists containing continuous points of a given size
    public static List<List<Vector3>> GroupPoints(List<Vector3> points, int MaxGroupSize)
    {
        //Lists co contain groups both being built, and completed
        List<List<Vector3>> completedLists = new List<List<Vector3>>();
        List<List<Vector3>> consideredLists = new List<List<Vector3>>();

        int numPoints = points.Count;
        // Take points from list 2 at a time
        for(int i = 0; i < numPoints - 1; i += 2)
        {
            Vector3 a = points[i];
            Vector3 b = points[i + 1];

            // Check if points a or b are already contained in a group(line)
            bool newGroup = true;
            foreach(List<Vector3> group in consideredLists)
            {
                int indxB = group.IndexOf(b);
                // If they are in a group but not at the end of a group(line), 
                // we'll start a new group(line)
                if (indxB == 0)
                {
                    group.Insert(0, a);
                    newGroup = false;
                }
                else if (indxB == group.Count - 1)
                {
                    group.Add(a);
                    newGroup = false;
                }
                else
                {
                    int indxA = group.IndexOf(a);

                    if (indxA == 0)
                    {
                        group.Insert(0, b);
                        newGroup = false;
                    }
                    else if (indxA == group.Count - 1)
                    {
                        group.Add(b);
                        newGroup = false;
                    }
                }
                //Group is full. add it to completed list
                if(group.Count >= MaxGroupSize)
                {
                    completedLists.Add(group);
                }
            }

            //Points are not in a group already. start a new one and add them
            if(newGroup)
            {
                List<Vector3> nGroup = new List<Vector3>();
                nGroup.Add(a);
                nGroup.Add(b);
                consideredLists.Add(nGroup);
            }



            // remove all completed lists from consideration
            foreach (List<Vector3> group in completedLists)
            {
                consideredLists.Remove(group);
            }
        }

        //out of points. Add all lists to 'completed' and return
        foreach(List<Vector3> group in consideredLists)
        {
            completedLists.Add(group);
        }

        return completedLists;

    }
}
