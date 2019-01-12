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

	// Water Mesh
	private Vector3[] vertices;
	private List<int> indicies = new List<int>();

	// Used to calculate kernel lookup table
	public float alpha = 0.9f;
	private int P = 8;
	private float g = -9.81f;
	private float[,] storedKernelArray;

	//Used in the iWave loop
	Vector3[][] height;
	Vector3[][] previousHeight;
	Vector3[][] verticalDerivative;
	public Vector3[][] source;
	public Vector3[][] obstruction;
	//To update the mesh we need a 1d array
	public Vector3[] unfolded_verts;
	//To be able to add ambient waves
	public Vector3[][] heightDifference;
	//Faster to calculate this once
	int arrayLength;

	float updateTimer = 0f;
    // Start is called before the first frame update
    void Start()
    {
		this.waterMeshFilter = this.GetComponent<MeshFilter>();

		this.height = this.GenerateWaterVertices().ToArray();
		this.GenerateWaterIndicies();

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


		this.storedKernelArray = new float[P * 2 + 1, P * 2 + 1];
		
		this.PrecomputeKernelValues();

		//Need to clone these
		this.previousHeight 		= height;
		this.verticalDerivative 	= height;
		this.source				= height;
		this.obstruction 		= height;
		this.heightDifference 	= height;

		//Create this once here, so we dont need to create it each update
		this.unfolded_verts = new Vector3[height.Length * height.Length];

		this.arrayLength = this.height.Length;
    }

    // Update is called once per frame
    void Update()
    {
        updateTimer += Time.deltaTime;
	
		if (updateTimer > 0.02f) {
			MoveWater(0.02f);
			updateTimer = 0f;
		}
    }

    private List<Vector3[]> GenerateWaterVertices(){
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

		return verts;
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

    /**
    	iWave Algorithm
    **/




    /**
		reference: https://www.habrador.com/tutorials/water-wakes/2-water-wakes/
    **/
    //Precompute the kernel values G(k,l)
	void PrecomputeKernelValues() {
		float G_zero = CalculateG_zero();
		
		//P - kernel size
		for (int k = -P; k <= P; k++) {
			for (int l = -P; l <= P; l++) {
				//Need "+ P" because we iterate from -P and not 0, which is how they are stored in the array
				storedKernelArray[k + P, l + P] = CalculateG((float)k,(float)l, G_zero);
			}
		} 
	}


	//G(k,l)
	private float CalculateG(float k, float l, float G_zero) {
		float delta_q = 0.001f;
		float sigma = 1f;
		float r = Mathf.Sqrt(k*k + l*l);
		
		float G = 0f;
		for (int n = 1; n <= 10000; n++) {
			float q_n = ((float)n * delta_q);
			float q_n_square = q_n * q_n; 
			
			G += q_n_square * Mathf.Exp(-sigma * q_n_square) * BesselFunction(q_n * r);
		}
		
		G /= G_zero;
		
		return G;
	}


	//G_zero
	private float CalculateG_zero() {		
		float delta_q = 0.001f;
		float sigma = 1f;
		
		float G_zero = 0f;
		for (int n = 1; n <= 10000; n++) {
			float q_n_square = ((float)n * delta_q) * ((float)n * delta_q);
			
			G_zero += q_n_square * Mathf.Exp(-sigma * q_n_square);	
		}
		
		return G_zero;
	}


	private float BesselFunction(float x) {
		//From http://people.math.sfu.ca/~cbm/aands/frameindex.htm
		//page 369 - 370
		
		float J_zero_of_X = 0f;
		
		//Test to see if the bessel functions are valid
		//Has to be above -3
		if (x <= -3f) {
			Debug.Log("smaller");
		}
		
		
		//9.4.1
		//-3 <= x <= 3
		if (x <= 3f) {
			//Ignored the small rest term at the end
			J_zero_of_X = 
				1f - 
					2.2499997f * Mathf.Pow (x / 3f, 2f) + 
					1.2656208f * Mathf.Pow (x / 3f, 4f) -
					0.3163866f * Mathf.Pow (x / 3f, 6f) +
					0.0444479f * Mathf.Pow (x / 3f, 8f) - 
					0.0039444f * Mathf.Pow (x / 3f, 10f)+
					0.0002100f * Mathf.Pow (x / 3f, 12f);
		}
		//9.4.3
		//3 <= x <= infinity
		else {
			//Ignored the small rest term at the end
			float f_zero = 
				0.79788456f -
					0.00000077f * Mathf.Pow (3f / x, 1f) -
					0.00552740f * Mathf.Pow (3f / x, 2f) -
					0.00009512f * Mathf.Pow (3f / x, 3f) -
					0.00137237f * Mathf.Pow (3f / x, 4f) -
					0.00072805f * Mathf.Pow (3f / x, 5f) + 
					0.00014476f * Mathf.Pow (3f / x, 6f);
			
			//Ignored the small rest term at the end
			float theta_zero = 
				x -
					0.78539816f - 
					0.04166397f * Mathf.Pow (3f / x, 1f) -
					0.00003954f * Mathf.Pow (3f / x, 2f) -
					0.00262573f * Mathf.Pow (3f / x, 3f) -
					0.00054125f * Mathf.Pow (3f / x, 4f) -
					0.00029333f * Mathf.Pow (3f / x, 5f) + 
					0.00013558f * Mathf.Pow (3f / x, 6f);
			
			//Should be cos and not acos
			J_zero_of_X = Mathf.Pow(x, -1f/3f) * f_zero * Mathf.Cos(theta_zero);
		}
		
		return J_zero_of_X;
	}

	/**


	**/

}

