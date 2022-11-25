using System;
using UnityEngine;

namespace BzKovSoft.ObjectSlicer.MeshGenerator
{
	public class Capsule
	{
		/// <param name="nbLong">Longitude</param>
		/// <param name="nbLat">Latitude</param>
		public static Mesh Create(float radius, float height, int direction, Vector3 center, int nbLong = 12, int nbLat = 4)
		{
			Mesh mesh = new Mesh();

			#region Vertices
			Vector3[] vertices = new Vector3[nbLong * nbLat * 2 + 2];
			height = Mathf.Max(height / 2f - radius, 0f);
			const float _2pi = Mathf.PI * 2f;

			var shift = Vector3.up * height;
			vertices[0] = Vector3.up * radius + shift;
			for (int lat = 0; lat < nbLat; lat++)
			{
				float a1 = Mathf.PI * (lat + 1) / (nbLat) / 2;
				float sin1 = Mathf.Sin(a1);
				float cos1 = Mathf.Cos(a1);

				for (int lon = 0; lon < nbLong; lon++)
				{
					float a2 = _2pi * lon / nbLong;
					float sin2 = Mathf.Sin(a2);
					float cos2 = Mathf.Cos(a2);

					vertices[
						1 +             // up vertex
						lat * nbLong +  // lat shift
						lon             // lon shift
						] = shift + new Vector3(sin1 * cos2, cos1, sin1 * sin2) * radius;
				}
			}

			shift = Vector3.up * -height;
			for (int lat = 0; lat < nbLat; lat++)
			{
				float a1 = Mathf.PI / 2f + Mathf.PI * (lat) / (nbLat) / 2;
				float sin1 = Mathf.Sin(a1);
				float cos1 = Mathf.Cos(a1);

				for (int lon = 0; lon < nbLong; lon++)
				{
					float a2 = _2pi * lon / nbLong;
					float sin2 = Mathf.Sin(a2);
					float cos2 = Mathf.Cos(a2);

					vertices[
						nbLong * nbLat +
						1 +             // up vertex
						lat * nbLong +  // lat shift
						lon             // lon shift
						] = shift + new Vector3(sin1 * cos2, cos1, sin1 * sin2) * radius;
				}
			}

			vertices[vertices.Length - 1] = Vector3.up * -radius + shift;
			#endregion

			//#region Normales		
			//Vector3[] normales = new Vector3[vertices.Length];
			//for (int n = 0; n < vertices.Length; n++)
			//	normales[n] = vertices[n].normalized;
			//#endregion

			//#region UVs
			//Vector2[] uvs = new Vector2[vertices.Length];
			//uvs[0] = Vector2.up;
			//uvs[uvs.Length - 1] = Vector2.zero;
			//for (int lat = 0; lat < nbLat; lat++)
			//	for (int lon = 0; lon <= nbLong; lon++)
			//		uvs[lon + lat * (nbLong + 1) + 1] = new Vector2((float)lon / nbLong, 1f - (float)(lat + 1) / (nbLat + 1));
			//#endregion

			#region Triangles
			int caps = nbLong * 2;
			int middle = ((nbLat - 1) * 2 + 1) * nbLong * 2;

			int[] triangles = new int[(caps + middle) * 3];

			//Top Cap
			int i = 0;
			for (int lon = 0; lon < nbLong - 1; lon++)
			{
				triangles[i++] = lon + 2;
				triangles[i++] = lon + 1;
				triangles[i++] = 0;
			}
			triangles[i++] = 1;
			triangles[i++] = nbLong;
			triangles[i++] = 0;

			//Middle
			for (int lat = 0; lat < nbLat * 2 - 1; lat++)
			{
				for (int lon = 0; lon < nbLong - 1; lon++)
				{
					int current = lon + lat * nbLong + 1;
					int next = current + nbLong;

					triangles[i++] = current;
					triangles[i++] = current + 1;
					triangles[i++] = next + 1;

					triangles[i++] = current;
					triangles[i++] = next + 1;
					triangles[i++] = next;
				}

				triangles[i++] = nbLong * (lat + 1);
				triangles[i++] = nbLong * lat + 1;
				triangles[i++] = nbLong * (lat + 1) + 1;

				triangles[i++] = nbLong * (lat + 1);
				triangles[i++] = nbLong * (lat + 1) + 1;
				triangles[i++] = nbLong * (lat + 2);
			}

			//Bottom Cap
			for (int lon = 0; lon < nbLong - 1; lon++)
			{
				triangles[i++] = vertices.Length - 1;
				triangles[i++] = vertices.Length - (lon + 2) - 1;
				triangles[i++] = vertices.Length - (lon + 1) - 1;
			}
			triangles[i++] = vertices.Length - 1;
			triangles[i++] = vertices.Length - 1 - 1;
			triangles[i++] = vertices.Length - (nbLong) - 1;
			#endregion

			#region Rotate
			if (direction != 1)
				for (int ii = 0; ii < vertices.Length; ii++)
				{
					var v = vertices[ii];
					if (direction == 0)
						v = new Vector3(v.y, v.z, v.x);
					if (direction == 2)
						v = new Vector3(v.z, v.x, v.y);
					vertices[ii] = v;
				}
			#endregion

			#region Shift vertices
			if (Math.Abs(center.sqrMagnitude) > Mathf.Epsilon)
				for (int ii = 0; ii < vertices.Length; ii++)
				{
					vertices[ii] = vertices[ii] + center;
				}
			#endregion

			mesh.vertices = vertices;
			//mesh.normals = normales;
			//mesh.uv = uvs;
			mesh.triangles = triangles;

			mesh.RecalculateBounds();
			;

			return mesh;
		}

		public static SliceResult IntersectsPlane(Transform transform, float radius, float height, int direction, Vector3 center, Plane plane)
		{
			var pointA = new Vector3(0f, height / 2f, 0f);
			var pointB = -pointA;
			pointA = transform.TransformPoint(pointA);
			pointB = transform.TransformPoint(pointB);

			var radiusShiftA = plane.normal * radius;
			var radiusShiftB = plane.normal * radius;

			bool sideA1 = plane.GetSide(pointA + radiusShiftA);
			bool sideA2 = plane.GetSide(pointA - radiusShiftA);
			bool sideB1 = plane.GetSide(pointB + radiusShiftB);
			bool sideB2 = plane.GetSide(pointB - radiusShiftB);

			if (
				(sideA1 == true  & sideA2 == true  & sideB1 == true  & sideB2 == true) |
				(sideA1 == false & sideA2 == false & sideB1 == false & sideB2 == false))
			{
				return sideA1 ? SliceResult.Pos : SliceResult.Neg;
			}

			return SliceResult.Sliced;
		}
	}
}
