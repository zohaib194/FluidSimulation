using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterWake : MonoBehaviour
{
	private Mesh waterMesh;
	private MeshFilter waterMeshFilter;
	private float waterWidth = 3.0f;
	private float gridSpacing = 0.1f;


    // Start is called before the first frame update
    void Start()
    {
		this.waterMeshFilter = this.GetComponent<MeshFilter>();

		List<Vector3[]> heights = GenerateWaterMesh(this.waterMeshFilter, this.waterWidth, this.gridSpacing);

		this.waterMesh = this.waterMeshFilter.mesh;
	
		//Need a box collider so the mouse can interact with the water
		BoxCollider boxCollider = this.GetComponent<BoxCollider>();
		boxCollider.center = new Vector3(this.waterWidth / 2.0f, 0.0f, this.waterWidth / 2.0f);
		boxCollider.size = new Vector3(this.waterWidth, 0.1f, this.waterWidth);
	
		//Center the mesh to make it easier to know where it is
		this.transform.position = new Vector3(-this.waterWidth / 2.0f, 0.0f, -this.waterWidth / 2.0f);        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private List<Vector3[]> GenerateWaterMesh(MeshFilter meshFilter, float width, float gridSpace){
    	int vertPerRow = (int)Mathf.Round(width / gridSpace) + 1;
    	Debug.Log((int)Mathf.Round(width / gridSpace) + 1);

    	return new List<Vector3[]>();
    } 

}
