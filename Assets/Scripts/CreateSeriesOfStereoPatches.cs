using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[RequireComponent (typeof (GeneratePatchesFromStereoSkybox))]
public class CreateSeriesOfStereoPatches : MonoBehaviour {

	[CustomEditor (typeof (CreateSeriesOfStereoPatches))]
	[CanEditMultipleObjects]
	public class PatchMakerEditor : Editor {

		public override void OnInspectorGUI () {
			serializedObject.Update ();
			DrawDefaultInspector ();
			CreateSeriesOfStereoPatches patchMaker = (CreateSeriesOfStereoPatches) target;
			if (GUILayout.Button ("Generate Stereo Patch Textures")) {
				//hypothetically, I could loop through a list of textures and generate suffixes for each
				patchMaker.Generate ();
			}
			serializedObject.ApplyModifiedProperties ();
		}
	}

	[SerializeField]
	private Mesh[] meshes;

	[SerializeField]
	private string[] fileNames;

	[SerializeField]
	private Texture2D[] leftEyes;

	[SerializeField]
	private Texture2D[] rightEyes;

	[SerializeField]
	private MeshFilter mf;

	public void Generate () {
		var patchGenerator = GetComponent<GeneratePatchesFromStereoSkybox> ();
		for (int i = 0; i < meshes.Length; i++) {
			mf.sharedMesh = meshes[i];
			patchGenerator.patchName = fileNames[i];
			patchGenerator.LeftEyeTexture = leftEyes[i];
			patchGenerator.RightEyeTexture = rightEyes[i];
			patchGenerator.Generate ();
		}
	}
}