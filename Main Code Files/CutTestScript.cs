using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;


public class CutTestScript : MonoBehaviour {

    
    Plane CuttingPlane;
    bool HasStruck = false;
    // Use this for initialization
    void Start () {
       
    }
	
	// Update is called once per frame
	void Update () {
        
    }

    public void OnTriggerEnter(Collider other)
    {
        //if the right type of object strikes the cuttable object, and we aren't in the middle of something
        if (other.gameObject.tag == "PlayerWeapon" && !HasStruck)
        {
            HasStruck = true;
            //print(other.name);
            //print("BldDir: " + other.transform.TransformDirection(Vector3.forward) + " BldVel: " + other.GetComponent<WeissSword>().holder.GetComponent<Grabber>().Controller.velocity);
            Vector3 normal = Vector3.Cross(other.transform.TransformDirection(Vector3.forward), other.GetComponent<WeissSword>().holder.GetComponent<Grabber>().Controller.velocity);
            //print("n"  + normal);
            Vector3 position = other.transform.position;
            //print("p" + position);
            CuttingPlane = new Plane(normal.normalized, position);

            //figure out which side becomes a new object
            bool isCOMlower = false;
            if (!CuttingPlane.GetSide(transform.GetComponent<Rigidbody>().centerOfMass))
            {
                isCOMlower = true;
            }
            Mesh[] Parts = SliceMesh(transform.GetComponent<MeshFilter>().mesh, CuttingPlane);
            //no mesh no actions
            if (Parts == null)
            {
                return;
            }

            //TEMP [WIP]
            if (!isCOMlower)
            {
                gameObject.GetComponent<MeshFilter>().mesh = Parts[0];
            }
            else
            {
                gameObject.GetComponent<MeshFilter>().mesh = Parts[1];
            }
            //get the object's mesh, and gen the new object
            gameObject.GetComponent<MeshCollider>().sharedMesh = gameObject.GetComponent<MeshFilter>().mesh;
            gameObject.transform.Find("OtherCollider").GetComponent<MeshCollider>().sharedMesh = gameObject.GetComponent<MeshFilter>().mesh;
            //print(Parts[0].uv);
            GameObject OtherBit = new GameObject();
            OtherBit.transform.position = transform.position;
            OtherBit.transform.rotation = transform.rotation;
            OtherBit.transform.localScale = transform.localScale;
            OtherBit.AddComponent<Rigidbody>();
            OtherBit.AddComponent<MeshFilter>();
            if (!isCOMlower)
            {
                OtherBit.GetComponent<MeshFilter>().mesh = Parts[1];
            }
            else
            {
                OtherBit.GetComponent<MeshFilter>().mesh = Parts[0];
            }
        
            OtherBit.AddComponent<MeshRenderer>();
            OtherBit.GetComponent<MeshRenderer>().material = transform.GetComponent<MeshRenderer>().material;
            OtherBit.AddComponent<MeshCollider>();
            OtherBit.GetComponent<MeshCollider>().sharedMesh = OtherBit.GetComponent<MeshFilter>().mesh;
            OtherBit.GetComponent<MeshCollider>().convex = true;
            OtherBit.layer = 11;
        }
        
    }

    public void OnTriggerExit(Collider other)
    {
        if(other.tag == "PlayerWeapon")
        {
            //have a cooldwon to prevent repeat rapid cutting and other weirdness
            StartCoroutine(ExtentionMethods.Cooldown(0.1f, (x) => HasStruck = !x));
        }
    }


    //the mesh splitting function
    public Mesh[] SliceMesh (Mesh Victim, Plane CuttingPlane)
    {
        //get references to original mesh data
        Vector3[] verticies = Victim.vertices;
        Vector2[] uvs = Victim.uv;
        Vector3[] normals = Victim.normals;
        int[] tris = Victim.triangles;
        //create resizable lists for the data of each new part
        List<Vector3> verticiesUpperList = new List<Vector3>();
        List<Vector2> newUVsUpperList = new List<Vector2>();
        List<Vector3> normalsUpperList = new List<Vector3>();
        List<int> trisUpperList = new List<int>();

        List<Vector3> verticiesLowerList = new List<Vector3>();
        List<Vector2> newUVsLowerList = new List<Vector2>();
        List<Vector3> normalsLowerList = new List<Vector3>();
        List<int> trisLowerList = new List<int>();

        List<Vector3> NewPoints = new List<Vector3>();
        //creat an array to keep track of which side of the plane each vertex is on
        bool[] VertexAbovePlane = new bool[verticies.Length];

        //these int arrays hold the new positions in verticiesUpper/Lower of every vertex from verticies.
        //each array's use is as follows: VertexUpperLookup[vertex pos in verticies] = vertex pos in verticiesupper. etc. no actual vertex data is stored
        int[] VertexUpperLookup = new int[verticies.Length]; 
        int[] VertexLowerLookup = new int[verticies.Length]; 
        

        //find out which vertex is on which side of the cut and add to respective list
        for (int i = 0; i < verticies.Length ; i++)
        {
            if(CuttingPlane.GetSide(transform.TransformPoint(verticies[i])))
            {
                verticiesUpperList.Add(verticies[i]);
                normalsUpperList.Add(normals[i]);
                VertexUpperLookup[i] = verticiesUpperList.Count - 1;
                VertexAbovePlane[i] = true;
                newUVsUpperList.Add(uvs[i]);
                
            }
            else
            {
                verticiesLowerList.Add(verticies[i]);
                normalsLowerList.Add(normals[i]);
                VertexLowerLookup[i] = verticiesLowerList.Count - 1;
                VertexAbovePlane[i] = false;
                newUVsLowerList.Add(uvs[i]);

            }

        }
        if(verticiesUpperList.Count == 0 || verticiesLowerList.Count == 0)
        {
            print("bad plane");
            return null;
        }
        ////Deal with each tri in turn. do something different depending on it's relationship to the cutting plane (above,through,below)

        //loop over every tri in the array
        for (int i = 0; i < tris.Length; i += 3)
        {
            //print("Dealing with tri: " + i / 3 +" This Tri has verticies: "+ tris[i] + " , " + tris[i+1] + " , " + tris[i + 2]);
            //add trangles that are all above the plane to trisUpper
            if((VertexAbovePlane[tris[i]] && VertexAbovePlane[tris[i +1]] && VertexAbovePlane[tris[i + 2]]))
            {
                //copy the old tri into the new list as we want to keep this one
                //note: the new tri will have references to verticiesUpper, not verticies. hence the v -> vU lookup.
                //print("tri with starting position " + i + " is fully above the plane");
                //print("newtris Length: " + newtris.Count + " tris Length: " + tris.Length + " VertexLookup Length: " + VertexLookup.Length + " verticiesUpperList Length: " + verticiesUpperList.Count + " VertexLookup[tris[i]],[tris[i + 1]],[tris[i + 2]]: " + VertexLookup[tris[i]] + " " + VertexLookup[tris[i+1]] + " " + VertexLookup[tris[i+2]]);
                trisUpperList.Add(VertexUpperLookup[tris[i]]);
                trisUpperList.Add(VertexUpperLookup[tris[i + 1]]);
                trisUpperList.Add(VertexUpperLookup[tris[i + 2]]);

            }
            //add trangles that are all below the plane to trisLower
            else if ((!VertexAbovePlane[tris[i]] && !VertexAbovePlane[tris[i + 1]] && !VertexAbovePlane[tris[i + 2]]))
            {
                //print("tri with starting position " + i + " is fully below the plane");
                trisLowerList.Add(VertexLowerLookup[tris[i]]);
                trisLowerList.Add(VertexLowerLookup[tris[i + 1]]);
                trisLowerList.Add(VertexLowerLookup[tris[i + 2]]);
            }
            //if neither, then tri cuts the plane. therefore "cut" the tri: figure out the correct new tri/tris and generate all their data(pos,uv etc) FORNOW above only
            else
            {
                //print("tri with starting position " + i + " cuts the plane");
                int[] NewTriEdgeVerticiesUpper = new int[4]; //trmporarily stores the 3-4 vetex references of the new upper tri/s
                int[] NewTriEdgeVerticiesLower = new int[4]; //trmporarily stores the 3-4 vetex references of the new lower tri/s
                NewTriEdgeVerticiesUpper[3] = 65565; //temp value, if left unchanged then NTEVU[3] has not been used. a branch depends on this later so certainty is neccesary (0 might get used in some circumstances)
                NewTriEdgeVerticiesLower[3] = 65565; //ditto
                int ArrayPointerUpper = 0; //counts the current place when assembling the NTEVU Array
                int ArrayPointerLower = 0; //ditto
                //test each edge to see where it intersects the plane
                for (int j = 0; j < 3; j++)
                {
                    //test point i + j

                    if (VertexAbovePlane[tris[i + j]] == true)
                    {
                        //if point is above the plane, find reference to it in VertexUpper and feed into NTEVU
                        NewTriEdgeVerticiesUpper[ArrayPointerUpper] = VertexUpperLookup[tris[i + j]];
                        ArrayPointerUpper++;
                    }
                    else
                    {
                        //if point is below the plane, find reference to it in VertexLower and feed into NTEVL
                        NewTriEdgeVerticiesLower[ArrayPointerLower] = VertexLowerLookup[tris[i + j]];
                        ArrayPointerLower++;
                    }
                    //if points i+j and the next point, i+j+1, are on different side of the plane....
                    if (VertexAbovePlane[tris[i + j]] != VertexAbovePlane[tris[i + (j + 1) % 3]])
                    {

                        //Find the position on the edge that intersects the plane. it should be added to both the uppel and lower meshes
                        float distance;
                        //print("okay, testing points in tris cells " + (i + j) + " and " + (i + (j + 1) % 3) + ". These points are verticies " + tris[i + j] + " and " + tris[(i + (j + 1) % 3)] + ".");
                        CuttingPlane.Raycast(new Ray(transform.TransformPoint(verticies[tris[i + j]]), transform.TransformPoint(verticies[tris[(i + (j + 1) % 3)]]) - transform.TransformPoint(verticies[tris[i + j]])), out distance);
                        distance = distance / (transform.TransformPoint(verticies[tris[(i + (j + 1) % 3)]]) - transform.TransformPoint(verticies[tris[i + j]])).magnitude;

                        //add that point where it should go
                        Vector3 newpoint = verticies[tris[i + j]] + (verticies[tris[(i + (j + 1) % 3)]] - verticies[tris[i + j]]) * distance;
                        Vector2 newuv = uvs[tris[i + j]] + (uvs[tris[(i + (j + 1) % 3)]] - uvs[tris[i + j]]) * distance;
                        Vector3 newnormal = normals[tris[i + j]] + (normals[tris[(i + (j + 1) % 3)]] - normals[tris[i + j]]) * distance;

                        //add the newly found point into the upper arrays
                        verticiesUpperList.Add(newpoint); //add the point into verticies upper as the newest point
                        normalsUpperList.Add(newnormal);
                        newUVsUpperList.Add(newuv);
                        NewTriEdgeVerticiesUpper[ArrayPointerUpper] = verticiesUpperList.Count-1; //as the newly added point will always be the last point in the array, it's index is Count-1
                        ArrayPointerUpper++; //point added, move onto next space

                        //add the newly found point into the lower arrays
                        verticiesLowerList.Add(newpoint); //add the point into verticies lower as the newest point
                        normalsLowerList.Add(newnormal);
                        newUVsLowerList.Add(newuv);
                        NewTriEdgeVerticiesLower[ArrayPointerLower] = verticiesLowerList.Count - 1; //as the newly added point will always be the last point in the array, it's index is Count-1
                        ArrayPointerLower++; //point added, move onto next space

                        NewPoints.Add(newpoint); //add point for use later when gening the cut face's tris

                    }
                }

                //      |\
                //      | \     Upper
                //      |__\    _____  OR VICE VERSA
                //      |  /\
                //      | /  \  Lower
                //      |/____\

                //Make new tri/tris
                if ((NewTriEdgeVerticiesUpper[3] == 65565) && !(NewTriEdgeVerticiesLower[3] == 65565)) //if there is only one tri to make [3] will have not been changed
                {
                    //make upper tris
                    trisUpperList.Add(NewTriEdgeVerticiesUpper[0]); //first tri, points 0,1,2
                    trisUpperList.Add(NewTriEdgeVerticiesUpper[1]);
                    trisUpperList.Add(NewTriEdgeVerticiesUpper[2]);
                    //make lower tris
                    trisLowerList.Add(NewTriEdgeVerticiesLower[0]); //first tri, points 0,1,2
                    trisLowerList.Add(NewTriEdgeVerticiesLower[1]);
                    trisLowerList.Add(NewTriEdgeVerticiesLower[2]);
                    trisLowerList.Add(NewTriEdgeVerticiesLower[2]); //first tri, points 2,3,0
                    trisLowerList.Add(NewTriEdgeVerticiesLower[3]);
                    trisLowerList.Add(NewTriEdgeVerticiesLower[0]);
                }
                else if(!(NewTriEdgeVerticiesUpper[3] == 65565) && (NewTriEdgeVerticiesLower[3] == 65565)) //or if there are two tris to make:
                {
                    //make upper tris
                    trisUpperList.Add(NewTriEdgeVerticiesUpper[0]); //first tri, points 0,1,2
                    trisUpperList.Add(NewTriEdgeVerticiesUpper[1]);
                    trisUpperList.Add(NewTriEdgeVerticiesUpper[2]);
                    trisUpperList.Add(NewTriEdgeVerticiesUpper[2]); //first tri, points 2,3,0
                    trisUpperList.Add(NewTriEdgeVerticiesUpper[3]);
                    trisUpperList.Add(NewTriEdgeVerticiesUpper[0]);
                    //make lower tris
                    trisLowerList.Add(NewTriEdgeVerticiesLower[0]); //first tri, points 0,1,2
                    trisLowerList.Add(NewTriEdgeVerticiesLower[1]);
                    trisLowerList.Add(NewTriEdgeVerticiesLower[2]);
                }
                else
                {
                    print("you should never get here");
                }
                 
            }
        }
        
        //Add the triangles on the cut face for both meshes
        Vector3 average = Vector3.zero;
        for (int i = 0;i < NewPoints.Count;i++)
        {
            average += NewPoints[i]; //find the center of the cut points
        }
        average = (average / NewPoints.Count);

        //sort points into clockwise order
        NewPoints = OrderClockwise(NewPoints, average, CuttingPlane);

        NewPoints.Add(average); //add this point to newpoints
        verticiesUpperList.AddRange(NewPoints); //add these points to each array again(as they have different uvs/normals, so new points are needed
        verticiesLowerList.AddRange(NewPoints);
        for (int i = 0; i < NewPoints.Count; i++)
        {
            newUVsUpperList.Add(new Vector2(240,360)); //make sure points have assosiated data 
            normalsUpperList.Add(CuttingPlane.normal); //TEMP hardcoded data
            newUVsLowerList.Add(new Vector2(240, 360)); //as this is a cut normals should be perpendicular, no smoothing needed here
            normalsLowerList.Add(CuttingPlane.normal);
        }
        
        for (int i = (NewPoints.Count - 1); i > 0; i--) //gen tris
        {
            trisUpperList.Add(verticiesUpperList.Count-1); //last point is the center (remember zero indexed)
            trisUpperList.Add((verticiesUpperList.Count-1) - (i)); //1,2,3....10                    ////           i = 10,9,8,7,6,5,4,3,2,1
            trisUpperList.Add((verticiesUpperList.Count-1) - (((i - 2) % (NewPoints.Count - 1)) + 1)); //2,3,4....10,1 //// ((i-2)%10)+1 = 9,8,7,6,5,4,3,2,1,10
            trisLowerList.Add(verticiesLowerList.Count - 1); //last point is the center (remember zero indexed)
            trisLowerList.Add((verticiesLowerList.Count - 1) - (((i - 2) % (NewPoints.Count - 1)) + 1)); //2,3,4....10,1 //// ((i-2)%10)+1 = 9,8,7,6,5,4,3,2,1,10
            trisLowerList.Add((verticiesLowerList.Count - 1) - (i)); //1,2,3....10                    ////           i = 10,9,8,7,6,5,4,3,2,1
        }
        
        //recast lists to arrays
        Vector3[] verticiesUpper = verticiesUpperList.ToArray();
        int[] trisUpperListArr = trisUpperList.ToArray();
        Vector2[] newUVsUpper = newUVsUpperList.ToArray();
        Vector3[] newNormalsUpper = normalsUpperList.ToArray();

        Vector3[] verticiesLower = verticiesLowerList.ToArray();
        int[] trisLowerListArr = trisLowerList.ToArray();
        Vector2[] newUVsLower = newUVsLowerList.ToArray();
        Vector3[] newNormalsLower = normalsLowerList.ToArray();
        

        

        //gen new meshes
        Mesh Upper = new Mesh();
        Upper.vertices = verticiesUpper;
        Upper.triangles = trisUpperListArr;
        Upper.uv = newUVsUpper;
        Upper.normals = newNormalsUpper;

        Mesh Lower = new Mesh();
        Lower.vertices = verticiesLower;
        Lower.triangles = trisLowerListArr;
        Lower.uv = newUVsLower;
        Lower.normals = newNormalsLower;
        
        //build return array
        Mesh[] Parts = new Mesh[2];
        Parts[0] = Upper;
        Parts[1] = Lower;
        return (Parts);
    }

    //To generate the freshly cut face on each mesh, new triangles must be generated and textured. 
    //This can be reliably achieved for convex shapes by forming a circle of tris between two neghbour points and the center of the cut face
    //to do that the points should be ordered clockwise, ensuring two neighbour points in the array are neighbours in the game world 

    public List<Vector3> OrderClockwise(List <Vector3> points,Vector3 center,Plane cutPlane)
    {
        Vector3[] pointsArr = points.ToArray();
        
        List <float> angles = new List <float>();
        pointsArr[0] = points[0];
        angles.Add(0);
        for(int i = 1;i < points.Count;i++)
        {
            angles.Add(Vector3.Angle(points[0] - center, points[i] - center));
        }
        pointsArr.OrderBy(angle => angle.magnitude);
        return pointsArr.ToList();
    }
    private void OnDrawGizmos()
    {

        Handles.color = Color.red;
        Handles.DrawSolidDisc(CuttingPlane.distance * CuttingPlane.normal * -1, CuttingPlane.normal, 0.5f);
    }
}


