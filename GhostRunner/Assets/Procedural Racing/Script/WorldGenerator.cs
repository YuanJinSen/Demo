using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Car
{
    public class WorldGenerator : MonoBehaviour
    {
        public Material mtl;
        public Vector2 size;
        public int scale;

        public float perlinScale;
        public float offset;
        public float waveHeight;
        private void Start()
        {
            GenerateCyliner();
        }

        private void GenerateCyliner()
        {
            GameObject newCyliner = new GameObject("WorldPiece");

            MeshFilter meshFilter = newCyliner.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = newCyliner.AddComponent<MeshRenderer>();

            meshRenderer.material = mtl;

            meshFilter.mesh = GenerateMesh();

            newCyliner.AddComponent<MeshCollider>();
        }

        private Mesh GenerateMesh()
        {
            Mesh mesh = new Mesh();
            mesh.name = "MESH";

            //¶¥µã¡¢Èý½ÇÐÎ¡¢UV
            Vector3[] vertices = null;
            Vector2[] uvs = null;
            int[] triangles = null;

            CreateShape(ref vertices, ref uvs, ref triangles);

            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;

			mesh.RecalculateNormals();

			return mesh;
        }

        private void GenerateData(ref Vector3[] vertices, ref Vector2[] uvs, ref int[] triangles)
        {
            int xCount = (int)size.x;
            int zCount = (int)size.y;

            vertices = new Vector3[(xCount + 1) * (zCount + 1)];
            uvs = new Vector2[(xCount + 1) * (zCount + 1)];

            float radius = xCount * scale * 0.5f;
            int index = 0;
            for (int x = 0; x <= xCount; x++)
            {
                float angle = x * 2f * Mathf.PI / xCount;
                Vector3 dir = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);
                for (int z = 0; z <= zCount; z++)
                {
                    vertices[index] = new Vector3(dir.x * radius, dir.y * radius, z * scale);
                    uvs[index] = new Vector2(x * scale, z * scale);
                    float pX = vertices[index].x * perlinScale + offset;
                    float pY = vertices[index].z * perlinScale + offset;
                    vertices[index] += dir * Mathf.PerlinNoise(pX, pY) * waveHeight;
                }
            }

            triangles = new int[xCount * zCount * 6];
            index = 0;
            for (int x = 0; x < xCount; x++)
            {
                int[] baseIndex = new int[6]
                {
                    x * (zCount + 1),
                    x * (zCount + 1) + 1,
                    (x + 1) * (zCount + 1),
                    x * (zCount + 1) + 1,
                    (x + 1) * (zCount + 1) + 1,
                    (x + 1) * (zCount + 1),
                };
                for (int z = 0; z < zCount; z++)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        triangles[index++] = baseIndex[i]++;
                    }
                }
            }
        }


		void CreateShape(ref Vector3[] vertices, ref Vector2[] uvs, ref int[] triangles)
		{

			//get the size for this piece on the x and z axis
			int xCount = (int)size.x;
			int zCount = (int)size.y;

			//initialize the vertices and uv arrays using the desired dimensions
			vertices = new Vector3[(xCount + 1) * (zCount + 1)];
			uvs = new Vector2[(xCount + 1) * (zCount + 1)];

			int index = 0;

			//get the cylinder radius
			float radius = xCount * scale * 0.5f;

			//nest two loops to go through all vertices on the x and z axis
			for (int x = 0; x <= xCount; x++)
			{
				for (int z = 0; z <= zCount; z++)
				{
					//get the angle in the cylinder to position this vertice correctly
					float angle = x * Mathf.PI * 2f / xCount;

					//use cosinus and sinus of the angle to set this vertice
					vertices[index] = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, z * scale * Mathf.PI);

					//also update the uvs so we can texture the terrain
					uvs[index] = new Vector2(x * scale, z * scale);

					//now use our perlin scale and offset to create x and z values for the perlin noise
					float pX = (vertices[index].x * perlinScale) + offset;
					float pZ = (vertices[index].z * perlinScale) + offset;

					//get the center of the cylinder but keep the z of this vertice so we can point inwards to the center
					Vector3 center = new Vector3(0, 0, vertices[index].z);
					//now move this vertice inwards towards the center using perlin noise and the desired wave height
					vertices[index] += (center - vertices[index]).normalized * Mathf.PerlinNoise(pX, pZ) * waveHeight;

					//this part handles smooth transition between world pieces:

					//check if there are begin points and if we're at the start of the mesh (z means the forward direction, so through the cylinder)
					//if (z < startTransitionLength && beginPoints[0] != Vector3.zero)
					//{
					//	//if so, we must combine the perlin noise value with the begin points
					//	//we need to increase the percentage of the vertice that comes from the perlin noise 
					//	//and decrease the percentage from the begin point
					//	//this way it will transition from the last world piece to the new perlin noise values

					//	//the percentage of perlin noise in the vertices will increase while we're moving further into the cylinder
					//	float perlinPercentage = z * (1f / startTransitionLength);
					//	//don't use the z begin point since it will not have the correct position and we only care about the noise on x and y axis
					//	Vector3 beginPoint = new Vector3(beginPoints[x].x, beginPoints[x].y, vertices[index].z);

					//	//combine the begin point(which are the last vertices from the previous world piece) and original vertice to smoothly transition to the new world piece
					//	vertices[index] = (perlinPercentage * vertices[index]) + ((1f - perlinPercentage) * beginPoint);
					//}
					//else if (z == zCount)
					//{
					//	//it these are the last vertices, update the begin points so the next piece will transition smoothly as well
					//	beginPoints[x] = vertices[index];
					//}

					////spawn items at random positions using the mesh vertices
					//if (Random.Range(0, startObstacleChance) == 0 && !(gate == null && obstacles.Length == 0))
					//	CreateItem(vertices[index], x);

					//increase the current vertice index
					index++;
				}
			}

			//initialize the array of triangles (x * z is the number of squares, and each square has two triangles so 6 vertices)
			triangles = new int[xCount * zCount * 6];

			//create the base for our squares (which makes the generation algorithm easier)
			int[] boxBase = new int[6];

			int current = 0;

			//for all x positions
			for (int x = 0; x < xCount; x++)
			{
				//create a new base that we can use to populate a new row of squares on the z axis
				boxBase = new int[]{
				x * (zCount + 1),
				x * (zCount + 1) + 1,
				(x + 1) * (zCount + 1),
				x * (zCount + 1) + 1,
				(x + 1) * (zCount + 1) + 1,
				(x + 1) * (zCount + 1),
			};

				//this was used to close the mesh (it would connect the last triangles to the first triangles)
				//it messes up the texture so I decided not to use it
				//if(x == xCount - 1){
				//	boxBase[2] = 0;
				//	boxBase[4] = 1;
				//	boxBase[5] = 0;
				//}

				//for all z positions
				for (int z = 0; z < zCount; z++)
				{
					//increase all vertice indexes in the box by one to go to the next square on this z row
					for (int i = 0; i < 6; i++)
					{
						boxBase[i] = boxBase[i] + 1;
					}

					//assign 2 new triangles based upon 6 vertices to fill in one new square
					for (int j = 0; j < 6; j++)
					{
						triangles[current + j] = boxBase[j] - 1;
					}

					//now increase current by 6 to go to the next square
					current += 6;
				}
			}
		}
	}
}