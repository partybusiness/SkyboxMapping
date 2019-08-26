using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SplitSkyboxes : MonoBehaviour {

	[CustomEditor (typeof (SplitSkyboxes))]
	[CanEditMultipleObjects]
	public class SplitSkyboxEditor : Editor {

		public override void OnInspectorGUI () {
			serializedObject.Update ();
			DrawDefaultInspector ();
			SplitSkyboxes skyboxSplitter = (SplitSkyboxes) target;
			if (GUILayout.Button ("Split Skyboxes")) {
				//hypothetically, I could loop through a list of textures and generate suffixes for each
				skyboxSplitter.Split ();
			}
			serializedObject.ApplyModifiedProperties ();
		}
	}

	[SerializeField]
	private Texture2D[] sourceSkyboxes;

	[SerializeField]
	private string[] skyboxNames;

	public void Split () {
		for (int i = 0; i < sourceSkyboxes.Length; i++) {
			SplitSingle (sourceSkyboxes[i], skyboxNames[i]);
		}
	}

	private void SplitSingle (Texture2D texture, string name) {
		SaveSplitImage (texture, name + "_Left", texture.height / 2);
		SaveSplitImage (texture, name + "_Right", 0);
	}

	private void SaveSplitImage (Texture2D sourceTex, string fileName, int offset) {
		//save goalRender texture and blitMaterial as resources
		var saveTex = new Texture2D (sourceTex.width, sourceTex.height / 2, TextureFormat.RGB24, false);
		saveTex.SetPixels (sourceTex.GetPixels (0, offset, sourceTex.width, sourceTex.height / 2));
		byte[] bytes;
		bytes = saveTex.EncodeToJPG ();
		System.IO.File.WriteAllBytes ("Assets/GeneratedTextures/" + fileName + ".jpg", bytes);
	}
}