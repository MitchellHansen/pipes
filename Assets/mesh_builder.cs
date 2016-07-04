using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Random = UnityEngine.Random;

// ReSharper disable InconsistentNaming

public class mesh_builder : MonoBehaviour
{
    MeshFilter mesh_filter;
    Mesh mesh;

    float radius = 1.0f;

    Vector3 direction_vector;
    Vector3 position_vector;

    List<Vector3> verticies;
    List<Vector2> uv;
    List<int> triangles;

    int direction = 0;
    int length = 0;
    bool done = true;

    // Flip flop for calculating the mesh
    bool flip = false;

    int face_count = 10;
    int count = 0;

    void Start()
    {
        // Assigns a material named "Assets/Resources/DEV_Orange" to the object.

        string[] colors = new string[4] {"Green", "Red", "White", "Blue"};

        int index = Random.Range(0, 4);

        string color = "Mat_" + colors[index] + "_Grid_256";
        Material newMat = Resources.Load(color, typeof (Material)) as Material;

        GetComponent<Renderer>().material = newMat;

        mesh_filter = GetComponent<MeshFilter>();

        mesh = new Mesh();
        mesh.name = "ScriptedMesh";

        verticies = new List<Vector3>();
        uv = new List<Vector2>();
        triangles = new List<int>();

        direction_vector = new Vector3(0.0f, 1.0f, 0.0f);
        position_vector = new Vector3(0, 5, 0);
    }
    
    void Update()
    {
        //radius += Random.Range (-0.1f, 0.1f);

        //		if (radius < 0.8f)
        //			radius += 0.1f;
        //		else if (radius > 1.0f)
        //			radius -= 0.1f;

        // Limit the pipe to 500 units in length
        if (count <= 500)
        {
            count++;


            // 10 faces = (3.14 * 2) / 10
            float face_radians = (Mathf.PI*2)/face_count;

            direction_vector.Normalize();

            for (float i = 0; i < Mathf.PI*2; i += face_radians)
            {
                
                // The two perpendicular vectors A and B

                // New style using a function which takes into acount the edge cases
                Vector3 a = getPerpendicularVector(direction_vector);
                a.Normalize();
                Vector3 b = Vector3.Cross(a, direction_vector);


                // Old style of perpendicular vectors, causes wrapping and warping
                //Vector3 a = new Vector3 (0, 0, 1);
                //Vector3 b = new Vector3 (direction_vector.y * -1, direction_vector.x, 0);

                float x = position_vector.x + (radius*Mathf.Cos(i)*a.x + radius*Mathf.Sin(i)*b.x);
                float y = position_vector.y + (radius*Mathf.Cos(i)*a.y + radius*Mathf.Sin(i)*b.y);
                float z = position_vector.z + (radius*Mathf.Cos(i)*a.z + radius*Mathf.Sin(i)*b.z);


                // The verticy for the point
                Vector3 t1 = new Vector3(x, y, z);

                verticies.Add(new Vector3(x, y, z));
            }

            mesh.vertices = verticies.ToArray();


            // Only build the mesh every other cycle. 
            // Two reasons, performance, and to not build the mesh for the first round
            // where it would error out
            if (flip) {
                flip = false;
                BuildMesh();
            }
            else {
                flip = true;
            }

//		// "Fluttering" movement
//		if (done) {
//			direction = Random.Range (0, 6);
//			done = false;
//		}
//		if (direction == 0) {
//			direction_vector.x -= Random.Range (0.1f, 0.4f);
//			if (direction_vector.x < -0.9f) {
//				done = true;
//			}
//		} else if (direction == 1) {
//			direction_vector.y -= Random.Range (0.1f, 0.4f);
//			if (direction_vector.y < -0.9f) {
//				done = true;
//			}
//		} else if (direction == 2) {
//			direction_vector.z -= Random.Range (0.1f, 0.4f);
//			if (direction_vector.z < -0.9f) {
//				done = true;
//			}
//		}else if (direction == 3) {
//			direction_vector.x += Random.Range (0.1f, 0.4f);
//			if (direction_vector.x > 0.9f) {
//				done = true;
//			}
//		} else if (direction == 4) {
//			direction_vector.y += Random.Range (0.1f, 0.4f);
//			if (direction_vector.y > 0.9f) {
//				done = true;
//			}
//		} else if (direction == 5) {
//			direction_vector.z += Random.Range (0.1f, 0.4f);
//			if (direction_vector.z > 0.9f) {
//				done = true;
//			}
//		}


            // Problem with 0, causes pipes to rise into the sky never deviating from their path
            // I presume it's some sort of logic error that has to do with vectors

            // "Pipe" movement
            if (done && length <= 0) {
                direction = Random.Range(1, 6);
                length = Random.Range(20, 40);
                done = false;
            }
            else {
                length--;
            }
            if (direction == 1) {
                direction_vector.x -= Random.Range(0.1f, 0.4f);
                if (direction_vector.x < -0.9f) {
                    done = true;
                }
            }
            else if (direction == 0) {
                direction_vector.y -= Random.Range(0.1f, 0.4f);
                if (direction_vector.y < -0.9f) {
                    done = true;
                }
            }
            else if (direction == 2) {
                direction_vector.z -= Random.Range(0.1f, 0.4f);
                if (direction_vector.z < -0.9f) {
                    done = true;
                }
            }
            else if (direction == 3) {
                direction_vector.x += Random.Range(0.1f, 0.4f);
                if (direction_vector.x > 0.9f) {
                    done = true;
                }
            }
            else if (direction == 4) {
                direction_vector.y += Random.Range(0.1f, 0.4f);
                if (direction_vector.y > 0.9f) {
                    done = true;
                }
            }
            else if (direction == 5) {
                direction_vector.z += Random.Range(0.1f, 0.4f);
                if (direction_vector.z > 0.9f) {
                    done = true;
                }
            }

            // Update the position and sale its speed
            position_vector += direction_vector;
            position_vector.Scale(new Vector3(1.0f, 1.0f, 1.0f));
        }
    }

    private void BuildMesh()
    {
        // "Calculate" the normals. hah
        List<Vector2> oovs = new List<Vector2>();
        for (int i = 0; i < mesh.vertexCount; i++)
        {
            oovs.Add(new Vector2(1, 1));
        }

        mesh.uv = oovs.ToArray();

        // Make the faces
        List<int> tri = new List<int>();
        for (int i = 0; i + face_count + 1 < mesh.vertexCount; i++)
        {
            tri.Add(i);
            tri.Add(i + face_count);
            tri.Add(i + 1);

            tri.Add(i + 1);
            tri.Add(i + face_count);
            tri.Add(i + face_count + 1);
        }

        mesh.triangles = tri.ToArray();

        mesh.RecalculateNormals();

        mesh_filter.mesh = mesh;
    }

    private Vector3 getPerpendicularVector(Vector3 vector)
    {
        if (vector.x.Equals(0) && vector.y.Equals(0) && vector.z.Equals(0))
        {
            Debug.Log("Error, zero vector");
            return new Vector3(0.0f, 0.0f, 0.0f);
        }

        if (vector.x.Equals(0))
            return new Vector3(1.0f, 0.0f, 0.0f);
        if (vector.y.Equals(0))
            return new Vector3(0.0f, 1.0f, 0.0f);
        if (vector.z.Equals(0))
            return new Vector3(0.0f, 0.0f, 1.0f);

        // 
        //if (vector.z < 0)
        //{
        //    return new Vector3(1.0f, 1.0f, 1.0f * (vector.x + vector.y) / vector.z);
        //}
        return new Vector3(1.0f, 1.0f, -1.0f*(vector.x + vector.y)/vector.z);
    }
}
