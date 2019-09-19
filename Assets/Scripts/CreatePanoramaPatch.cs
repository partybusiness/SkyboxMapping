using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CreatePanoramaPatch : MonoBehaviour {

	[CustomEditor (typeof (CreatePanoramaPatch))]
	[CanEditMultipleObjects]
	public class PatchMakerEditor : Editor {

		public override void OnInspectorGUI () {
			serializedObject.Update ();
			DrawDefaultInspector ();
			CreatePanoramaPatch patchMaker = (CreatePanoramaPatch) target;
			if (GUILayout.Button ("Generate Patch Texture")) {
				//hypothetically, I could loop through a list of textures and generate suffixes for each
				patchMaker.GeneratePatchTexture (patchMaker.panoramaTexture, "");
			}
			serializedObject.ApplyModifiedProperties ();
		}
	}

	[SerializeField]
	private Transform cameraPosition;

	/// <summary>
	/// How far in any direction should the camera be able to move
	/// </summary>
	[SerializeField]
	private float positionOffset = 0.4f;

	[SerializeField]
	private MeshRenderer patchMesh;

	[SerializeField]
	private Texture panoramaTexture;

	[SerializeField]
	private Cubemap cubemap;

	[SerializeField]
	private Vector2 imageSize = new Vector2 (512, 512);

	[SerializeField]
	public string patchName = "patch";

	[SerializeField]
	public bool overrideDimensions;

	[SerializeField]
	public Vector2 newDimensions;

	/// <summary>
	/// offset the resulting material settings around the Y axis
	/// </summary>
	[SerializeField]
	private float offsetYAxis = 0f;

	private Vector2 MaxOffset (Vector3 centre, Vector3 size) {
		Debug.Log (size);
		var results = Vector2.zero;
		var direction = centre.normalized;
		var orthoX = Vector3.Cross (-direction, Vector3.up).normalized;
		var orthoY = Vector3.Cross (direction, orthoX).normalized;

		//check every corner and find max offset for x and y
		for (int i = 0; i < 8; i++) {
			//assign each of 8 corners
			var corner = centre + new Vector3 (size.x * ((i & 1) > 0 ? 1 : -1), size.y * ((i & 2) > 0 ? 1 : -1), size.z * ((i & 4) > 0 ? 1 : -1));
			corner = corner.normalized;
			var cornerResults = new Vector2 (Mathf.Abs (Vector3.Dot (orthoX, corner)), Mathf.Abs (Vector3.Dot (orthoY, corner)));
			var parallel = Vector3.Dot (corner, direction);
			cornerResults /= parallel;
			results.x = Mathf.Max (results.x, cornerResults.y);
			results.y = Mathf.Max (results.y, cornerResults.x);
		}

		return results * 2f;
	}

	public void GeneratePatchTexture (Texture sourceTexture, string imageOffset = "", bool newMaterial = true) {
		//find min and max for selected mesh
		var patchTransform = patchMesh.transform;
		var patchMF = patchMesh.GetComponent<MeshFilter> ();
		var sourceMesh = patchMF.sharedMesh;

		var patchBounds = patchMesh.bounds;
		var patchLatLong = PositionToLatLong (patchBounds.center - cameraPosition.position);

		//find ratio between distance and offset
		var widths = MaxOffset (patchBounds.center - cameraPosition.position, patchBounds.size / 2f + (Vector3.one * positionOffset));
		if (overrideDimensions)
			widths = newDimensions;
		else
			newDimensions = widths;

		var goalRender = new RenderTexture (Mathf.RoundToInt (imageSize.x), Mathf.RoundToInt (imageSize.y), 16);
		var blitMaterial = new Material (Shader.Find ("Unlit/FromSkyboxToPatch2"));

		//blitMaterial.SetTexture("_Cube", cubemap);
		//SkyboxToPatch2 relies entirely on the source texture rather than the cubemap
		blitMaterial.SetFloat ("_OffsetX", patchLatLong.x);
		blitMaterial.SetFloat ("_OffsetY", patchLatLong.y);
		blitMaterial.SetFloat ("_Height", widths.x);
		blitMaterial.SetFloat ("_Width", widths.y);

		//Debug.LogFormat("rendering at position {0},{1} with widths {2},{3}", patchLatLong.x, patchLatLong.y, widths.x, widths.y);

		//save texture
		Graphics.Blit (sourceTexture, goalRender, blitMaterial, -1);
		SaveRenderImage (goalRender);

		//create patch material
		var patchMaterial = new Material (Shader.Find ("Unlit/FromPatchToSkybox2"));
		patchMaterial.SetFloat ("_OffsetX", patchLatLong.x + offsetYAxis / 360f);
		patchMaterial.SetFloat ("_OffsetY", patchLatLong.y);
		patchMaterial.SetFloat ("_Height", widths.x);
		patchMaterial.SetFloat ("_Width", widths.y);

		_valuesAsText = string.Format ("{0},{1},{2},{3}", patchLatLong.x, patchLatLong.y, widths.y, widths.x);

		if (newMaterial) {
			//load texture
			var savedTexture = AssetDatabase.LoadAssetAtPath<Texture2D> ("Assets/GeneratedTextures/" + patchName + imageOffset + ".png");
			patchMaterial.SetTexture ("_Patch", savedTexture);

			//save material
			AssetDatabase.CreateAsset (patchMaterial, "Assets/GeneratedMaterials/" + patchName + ".mat");
			patchMesh.material = patchMaterial;
		}
	}

	private string _valuesAsText;

	public string ValuesAsText {
		get {
			return _valuesAsText;
		}
	}

	private void SaveRenderImage (RenderTexture goalRender) {
		//save goalRender texture and blitMaterial as resources
		var saveTex = new Texture2D (goalRender.width, goalRender.height, TextureFormat.RGB24, false);
		RenderTexture.active = goalRender;
		saveTex.ReadPixels (new Rect (0, 0, goalRender.width, goalRender.height), 0, 0);
		RenderTexture.active = null;
		byte[] bytes;
		bytes = saveTex.EncodeToPNG ();
		System.IO.File.WriteAllBytes ("Assets/GeneratedTextures/" + patchName + ".png", bytes);
	}

	private Vector2 PositionToLatLong (Vector3 coords) {
		var normalizedCoords = coords.normalized;
		float latitude = Mathf.Acos (normalizedCoords.y);
		float longitude = Mathf.Atan2 (normalizedCoords.z, normalizedCoords.x);
		//return new Vector2(longitude, latitude);
		Vector2 sphereCoords = new Vector2 ((longitude * 0.5f) / Mathf.PI, (latitude * 1.0f) / Mathf.PI);
		return new Vector2 (-0.25f, 1.0f) - sphereCoords;//-.25 offset is arbitrary but it matches how cubemaps are sampled in Unity

		/*  var normalizedCoords = coords.normalized;
		  float latitude = Mathf.Acos(normalizedCoords.y);
		  float longitude = Mathf.Atan2(normalizedCoords.z, normalizedCoords.x);
		  return new Vector2(-longitude+Mathf.PI/2f, latitude -Mathf.PI/2f);//- */
	}

	private Vector3 RelativePosition (Vector3 vertexPosition) {
		return patchMesh.transform.TransformPoint (vertexPosition) - cameraPosition.position;
	}

	private float CompareX (Vector3 position) {
		return 0;
	}

	private Vector2 angles;

	[ExecuteInEditMode]
	private void Update () {
		angles = PositionToLatLong (patchMesh.transform.position - cameraPosition.position);
		var rotatedAngle = Quaternion.Euler (0, angles.x * Mathf.Rad2Deg, 0) * Quaternion.Euler (angles.y * Mathf.Rad2Deg, 0, 0);// new Vector3(,, 0);
		var rayForward = rotatedAngle * Vector3.forward;
		Debug.DrawRay (cameraPosition.position, rayForward * 30f, Color.red);
	}

	private void OnGUI () {
		Handles.Label (patchMesh.transform.position, string.Format ("{0} {1}", angles.x.ToString ("F2"), angles.y.ToString ("F2")));
	}
}