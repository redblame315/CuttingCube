using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace PwhSoft.Additional_Assets.Scripts
{
	public static class GameObjectExtensions {
		public static void SetXZero(this GameObject gameObject) {
			if (gameObject == null)
				return;
			var lp = gameObject.transform.localPosition;
			lp.x = 0f;
			gameObject.transform.localPosition = lp;
		}

		/// <summary>
		/// Finds all children on the next deeper edge of gameobject
		/// </summary>
		/// <param name="gameObject">The gameobject to search in</param>
		/// <returns></returns>
		public static IEnumerable<GameObject> FindChildrenOnNextHierarchyEdge(this GameObject gameObject, bool isActive = true)
		{
			if (gameObject == null)
				return null;
			var gameObjectChildren = new List<GameObject>();
			foreach (Transform t in gameObject.transform)
			{
				if (isActive && t.gameObject.activeInHierarchy)
				{
					gameObjectChildren.Add(t.gameObject);
				}
			}
			return gameObjectChildren;
		}

		static List<GameObject> GetChildren(GameObject go) {
			if (go == null)
				return null;
			var gos = new List<GameObject>();
			foreach (Transform t in go.transform) {
				gos.Add(t.gameObject);

				foreach (Transform ts in t) {
					gos.Add(ts.gameObject);


					foreach (Transform t2 in ts)
					{
						gos.Add(t2.gameObject);


						foreach (Transform t3 in t2)
						{
							gos.Add(t3.gameObject);


							foreach (Transform t4 in t3)
							{
								gos.Add(t4.gameObject);


								foreach (Transform t5 in t4)
								{
									gos.Add(t5.gameObject);

								}
							}
						}
					}
				}
			}
			return gos;
		}

		public static GameObject[] GetAllChildren(GameObject go) {
			if (go == null)
				return null;
			var gos = GetChildren(go);
			for(var i = 0; i < gos.Count; i++){
				var gosMore = GetChildren(gos[i]);
				gos.AddRange(gosMore);
			}
			return gos.ToArray();
		}

		public static GameObject[] FindChildrenByName(this GameObject go, string nam) {
			if (go == null)
				return null;
			var gos = GetAllChildren(go);
			var imgs = new List<GameObject>();
			foreach (var gC in gos) {
				if (nam == gC.name) {
					if (gC != null)
						imgs.Add(gC);
				}
			}
			return imgs.ToArray();
		}

		public static T FindChildByType<T>(this GameObject go) {
			if (go == null)
				return default(T);
			var childrens = go.FindChildrenByType<T>();
			return (childrens.Count > 0) ? (T)childrens[0] : default(T);
		}
		public static T FindChildrenByTypeAndName<T>(this GameObject go, string name, bool onlyStartsWithName = false) {
			if (go == null)
				return default(T);
			var gos = GetAllChildren(go);
			var searchedObj = default(T);
			foreach (var gC in gos) {
				var k = gC.GetComponent<T>();
				if (k != null) {
					if (( onlyStartsWithName ) ? ( gC.name.StartsWith(name) ) : ( gC.name == name ) ) {
						searchedObj = k;
						break;
					}
				}
			}
			return searchedObj;
		}
		public static List<T> FindChildrenByType<T>(this GameObject go, bool needToBeActiveInHierarchy = false) {
			if (go == null)
				return null;
			var gos = GetAllChildren(go);
			var imgs = new List<T>();
			foreach (var gC in gos) {
				if (needToBeActiveInHierarchy && !gC.activeInHierarchy)
				{
					continue;
				}

				var k = gC.GetComponent<T>();
				if (k != null && imgs.Contains(k) == false) {

					imgs.Add(k);
				}
			}
			return imgs;
		}
		public static List<Tuple<TFirst,TSecond>> FindChildrenByTypes<TFirst, TSecond>(this GameObject go, bool needToBeActiveInHierarchy = false)
			where TFirst : Component
			where TSecond : Component
		{
			if (go == null)
				return null;
			var allChildren = GetAllChildren(go);
			var result = new List<Tuple<TFirst,TSecond>>();
			foreach (var gC in allChildren) {
				if (needToBeActiveInHierarchy && !gC.activeInHierarchy)
				{
					continue;
				}

				var tFirst = gC.GetComponent<TFirst>();
				var tSecond = gC.GetComponent<TSecond>();
				if (tFirst != null && tSecond != null && !result.Contains(new Tuple<TFirst, TSecond>(tFirst,tSecond))) {
					result.Add(new Tuple<TFirst, TSecond>(tFirst,tSecond));
				}
			}
			return result;
		}

		public static GameObject[] FindChildrenStartName(this GameObject go, string nam) {
			if (go == null)
				return null;
			var gos = GetAllChildren(go);
			var imgs = new List<GameObject>();
			foreach (var gC in gos)
			{
				if (!gC.name.StartsWith(nam)) continue;
				if (gC != null)
					imgs.Add(gC);
			}
			return imgs.ToArray();
		}

		public static GameObject[] FindChildrenByName(this GameObject go, string[] names) {
			if (go == null)
				return null;
			var imgs = new List<GameObject>();
			foreach(var nam in names)
				imgs.AddRange(go.FindChildrenByName(nam));
			return imgs.ToArray();
		}

		public static IEnumerable<Image> FindImageChildrenByName(this GameObject go, string name, bool startsOnlyWithName) {
			if (go == null)
				return null;
			var gos = GetAllChildren(go);
			var images = new List<Image>();
			foreach (var gC in gos) {
				if (startsOnlyWithName == false ? (name != gC.name) : (!gC.name.StartsWith(name))) continue;
			
				var img = gC.GetComponent<Image>();
				if (img != null)
					images.Add(img);
			}
			return images.ToArray();
		}

		public static GameObject FindChildByName(this GameObject go, string nam) {
			if (go == null)
				return null;
			var gos = go.FindChildrenByName(nam);
			if (gos.Length>0)
				return gos[0];
			else return null;
		}

		public static Image[] FindImageChildrenByName(this GameObject go, string[] names, bool startsOnlyWithName) {
			if (go == null)
				return null;
			var imgs = new List<Image>();
			foreach(var nam in names)
				imgs.AddRange(go.FindImageChildrenByName(nam, startsOnlyWithName));
			return imgs.ToArray();
		}

		public static Sprite[] FindChildrenBySpriteType(this GameObject go)
		{
			if (go == null)
				return null;
			return go.FindChildrenByType<Sprite>().ToArray();
		}

		public static void SetActiveRecursivelyNew (this GameObject go, bool active) {
			if (go == null)
				return;
			go.SetActive (active);
			foreach (Transform t in go.transform) {
				SetActiveRecursivelyNew (t.gameObject, active);
			}
		}

		public static void DestroyComponentIfPossible(this GameObject go, Type type, bool destroyImmediate = false)
		{
			if (go == null)
				return;
			var goC = go.GetComponent(type);
			if (goC == null) return;
		
			if (destroyImmediate)
				UnityEngine.Object.Destroy(goC);

			else 
				UnityEngine.Object.DestroyImmediate(goC);
		}

		public static void DestroyComponentsIfPossible(this GameObject go, Type[] types, bool destroyImmediate = false)
		{
			if (types == null)
				return;
			foreach (var type in types)
				DestroyComponentIfPossible(go, type, destroyImmediate);
		}


		/// <summary>
		/// Gets the size of the max children mesh.
		/// </summary>
		/// <returns>The max children mesh size.</returns>
		public static Vector3? GetMaxChildrenMeshSize(this GameObject gameObject)
		{
			var allChildren = gameObject.FindChildrenByType<MeshFilter>();

			if (allChildren == null)
				return null;

			var maxXSize = allChildren.Select(x => x.mesh.bounds.size.x).Max();
			var maxYSize = allChildren.Select(x => x.mesh.bounds.size.y).Max();
			var maxZSize = allChildren.Select(x => x.mesh.bounds.size.z).Max();

			return new Vector3(maxXSize, maxYSize, maxZSize);
		}
	}
}
