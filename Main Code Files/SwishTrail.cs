using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwishTrail : MonoBehaviour {
    
    //settable settings and things that need to be tracked
    public GameObject TrackedGameObject;
    public Material TrailMaterial;
    public int TrailSegments;
    public float MinimumSegmentLength;
    public float UvMultiplier;
    public float ActivationSpeed;
    public float DeactivationSpeed;
    public float FullBrightnessSpeed;
    public Vector3 TipOffset;
    public Vector3 BaseOffset;
    private bool swinging;
    private Vector3[] veloRolling = new Vector3[5] {Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };
    private Mesh TrailMesh;
    private GameObject Trail;
    private float TrailDistance;
    private float LastSegmentDistance;
    // Use this for initialization
    void Start () {

    }

    // Update is called once per frame
    void Update() {
        
        //implement a rolling average of blade tip positions to reject noise

        Vector3 TipPos = TrackedGameObject.transform.TransformPoint(TipOffset);
        float dist = 0;
        for (int i = 0;i < veloRolling.Length-1;i++)
        {
            dist += (veloRolling[i] - veloRolling[i+1]).sqrMagnitude;
            veloRolling[i] = veloRolling[i + 1];
        }
        dist += (veloRolling[4] - TipPos).sqrMagnitude;
        float Velocity = dist / 5f;
        veloRolling[4] = TipPos;
        
        
        //check if the sword is going fast enough to start a swish, then init
        if (Velocity > Mathf.Pow(ActivationSpeed, 2) && !swinging)
        {
            //put current sword blade tip and base positions into an array
            //create a new object to contain the 'swish' graphics
            Trail = new GameObject();
            Trail.name = "Trail";
            Trail.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);
            Trail.AddComponent<MeshFilter>();
            Trail.GetComponent<MeshFilter>().mesh = new Mesh();
            TrailMesh = Trail.GetComponent<MeshFilter>().mesh;

            //init the mesh with the current blade tip and base
            TrailMesh.vertices = new[] {transform.TransformPoint(TipOffset),transform.TransformPoint(BaseOffset) };
            TrailMesh.uv =  new Vector2[2] { new Vector2(1,1), new Vector2(1,0) };
            Trail.AddComponent<MeshRenderer>();
            Trail.GetComponent<MeshRenderer>().material = TrailMaterial;
            TrailDistance = 0; //reset values that may have changed
            LastSegmentDistance = 0;
            swinging = true;
            //print("Now Swinging");
        }

        //if a swish has been generated
        if (swinging)
        {
            //take a working copy of the mesh
            Vector3[] Verticies = TrailMesh.vertices;
            int[] tris = TrailMesh.triangles;
            Vector2[] uvs = TrailMesh.uv;
            Vector3[] normals = TrailMesh.normals;

            TrailDistance += Velocity;
            // //ADD NEW SEGMENT
            //VERTICIES
            //print("Started swing update path");
            Vector3[] newPoints = new Vector3[2];
            newPoints[0] = transform.TransformPoint(TipOffset);
            newPoints[1] = transform.TransformPoint(BaseOffset);
            //init if there's no trail
            if (Verticies.Length == 0)
            {
                Verticies = new Vector3[2];
                Verticies[0] = newPoints[0];
                Verticies[1] = newPoints[1];
                TrailMesh.vertices = Verticies;
                print("Starting swing, creating first points");
            }
            //if the new tip point and the previous tip point are further away that a settable minimum
            else if ((TrailDistance - LastSegmentDistance) > Mathf.Pow(MinimumSegmentLength, 2))
            {
                print("enough distance covered to gen new segment");
                LastSegmentDistance = TrailDistance;
                //add the current points to the array
                Vector3[] workingVerts = new Vector3[Verticies.Length + 2];
                System.Array.Copy(Verticies, 0, workingVerts, 2, Verticies.Length);
                System.Array.Copy(newPoints, 0, workingVerts, 0, 2);
                TrailMesh.vertices = workingVerts;


                //TRIS
                //generate new mesh triangles to join the newly created points. 
                int[] newtris = { workingVerts.Length - 1, workingVerts.Length - 2, workingVerts.Length - 3, workingVerts.Length - 4, workingVerts.Length - 3, workingVerts.Length - 2
                                 ,workingVerts.Length - 1, workingVerts.Length - 3, workingVerts.Length - 2, workingVerts.Length - 4, workingVerts.Length - 2, workingVerts.Length - 3 };
                int[] workingtris = new int[tris.Length + 12];
                if (tris.Length != 0)
                {
                    System.Array.Copy(tris, 0, workingtris, 12, tris.Length);
                }
                System.Array.Copy(newtris, 0, workingtris, 0 , 12);

                TrailMesh.triangles = workingtris;


                //UVS
                //paint the texture onto the new mesh points
                Vector2[] workingUVs = new Vector2[uvs.Length + 2];
                if (TrailDistance * UvMultiplier < 1f)
                {
                    //if the trail is shorter than the texture, just paint the texture.
                    Vector2[] newUVs = new Vector2[2] { new Vector2(1 - TrailDistance * UvMultiplier, 1), new Vector2(1 - TrailDistance * UvMultiplier, 0) };

                    
                    System.Array.Copy(uvs, 0, workingUVs, 2, uvs.Length);
                    System.Array.Copy(newUVs, 0, workingUVs, 0, 2);
                    
                }
                else
                {
                    //This stretches the Texture if the swish is longer than 1 texture length
                    Vector2[] newUVs = new Vector2[2] { new Vector2(0, 0), new Vector2(0, 1) };

                    System.Array.Copy(uvs, 0, workingUVs, 2, uvs.Length);
                    System.Array.Copy(newUVs, 0, workingUVs, 0, 2);

                    for (int i = 0; i < workingUVs.Length; i++)
                    {
                        workingUVs[i] = new Vector2((float)(i - i % 2) / (float)(workingUVs.Length), (i+1)%2);
                    }


                }
                TrailMesh.uv = workingUVs;

            }
            else //if the sword has not moved far enough to add a new segment
            {
                print("not ready to add segment, updating sword pos"); //just update the latest two points to keep with the sword's position
                Verticies[0] = newPoints[0];
                Verticies[1] = newPoints[1];
                TrailMesh.vertices = Verticies; 
                if (TrailDistance * UvMultiplier < 1f)
                {
                    Vector2[] newUVs = new Vector2[2] { new Vector2(1 - TrailDistance * UvMultiplier, 1), new Vector2(1 - TrailDistance * UvMultiplier, 0) };
                    uvs[0] = newUVs[0];
                    uvs[1] = newUVs[1];
                }
                else
                {
                    //This stretches the Texture if the swish is longer than 1 tex length
                    for (int i = 0; i <  uvs.Length; i++) {
                        uvs[i] = new Vector2((float)(i - i%2) / (float)(uvs.Length), (1 + i) % 2);
                    }
                    uvs[0] = new Vector3(0 ,1);
                    uvs[1] = new Vector3(0, 0);
                }
                TrailMesh.uv = uvs;
            }
            
            
        }
        if (swinging && Velocity < Mathf.Pow(DeactivationSpeed, 2)) //if the sword's speed falls below the threshol, delete the swish [WIP]
        {
            swinging = false;
            print("Stopping the swing");
            TrailMesh.Clear();
            Destroy(Trail);
            //TODO fade out the trail more gracefully
        }
        
        //TODO ASSIGN OPACITY FROM SPEED


    }
}
