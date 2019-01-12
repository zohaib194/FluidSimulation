using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WaterWake : MonoBehaviour
{
	private Mesh waterMesh;
	private MeshFilter waterMeshFilter;
	private float waterWidth = 3.0f;
	private float gridSpacing = 0.1f;

		
	private Vector3[] vertices;// = new Vector3[vertPerRow*vertPerRow];
	private List<int> indicies = new List<int>();


    // Start is called before the first frame update
    void Start()
    {
		this.waterMeshFilter = this.GetComponent<MeshFilter>();

		GenerateWaterVertices();
		GenerateWaterIndicies();

		this.waterMesh = new Mesh();
		this.waterMesh.vertices = vertices;
		this.waterMesh.triangles = indicies.ToArray();
		this.waterMesh.RecalculateBounds();
		this.waterMesh.RecalculateNormals();
		this.waterMesh.name = "WaterMesh";

		waterMeshFilter.mesh.Clear();
		this.waterMeshFilter.mesh = this.waterMesh;
	
		BoxCollider boxCollider = this.GetComponent<BoxCollider>();
		boxCollider.center = new Vector3(this.waterWidth / 2.0f, 0.0f, this.waterWidth / 2.0f);
		boxCollider.size = new Vector3(this.waterWidth, 0.1f, this.waterWidth);
	
		this.transform.position = new Vector3(-this.waterWidth / 2.0f, 0.0f, -this.waterWidth / 2.0f);        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void GenerateWaterVertices(){
    	int vertPerRow = (int)Mathf.Round(this.waterWidth / this.gridSpacing) + 1;
    	//Debug.Log((int)Mathf.Round(width / gridSpace) + 1);
		List<Vector3[]> verts = new List<Vector3[]>();

		for(int z = 0; z < vertPerRow; z++){
			verts.Add(new Vector3[vertPerRow]);

			for(int x = 0; x < vertPerRow; x++){

				Vector3 point = new Vector3();

				point.x = x * this.gridSpacing;
				point.y = 0.0f;
				point.z = z * this.gridSpacing;

				verts[z][x] = point;

			}
		}

    	this.vertices = new Vector3[vertPerRow*vertPerRow];
		for(int i = 0; i < verts.Count; i++){
			verts[i].CopyTo(vertices, i * vertPerRow);	
		}
    } 

    private void GenerateWaterIndicies(){
    	int vertPerRow = (int)Mathf.Round(this.waterWidth / this.gridSpacing) + 1;

    	for(int z = 1; z < vertPerRow / 3; z++){

			for(int x = 1; x < vertPerRow / 3; x++){

				Debug.Log(x + (vertPerRow * z));
				Debug.Log((x + 1)  + vertPerRow * z);
				Debug.Log(x + (vertPerRow * (z + 1)));
				
				Debug.Log(x + (vertPerRow * (z + 1 )));
				Debug.Log((x + 1) + (vertPerRow * z));
				Debug.Log((x + 1) + (vertPerRow * (z + 1)));

				this.indicies.Add(x + (vertPerRow * (z + 1)));
				this.indicies.Add((x + 1)  + vertPerRow * z);
				this.indicies.Add(x + (vertPerRow * z));

				this.indicies.Add((x + 1) + (vertPerRow * (z + 1)));
				this.indicies.Add((x + 1) + (vertPerRow * z));
				this.indicies.Add(x + (vertPerRow * (z + 1 )));
				

				//this.indicies.Add(x 		+ z * vertPerRow);
				//this.indicies.Add(x 		+ (z-1) * vertPerRow);
				//this.indicies.Add((x-1) 	+ (z-1) * vertPerRow);

				//this.indicies.Add(x 		+ z * vertPerRow);
				//this.indicies.Add((x-1) 	+ (z-1) * vertPerRow);
				//this.indicies.Add((x-1)	+ z * vertPerRow);

			}
		}
    }
}
