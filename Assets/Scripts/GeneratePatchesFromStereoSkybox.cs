﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[RequireComponent (typeof (CreatePanoramaPatch))]
public class GeneratePatchesFromStereoSkybox : MonoBehaviour {

    [CustomEditor(typeof(GeneratePatchesFromStereoSkybox))]
    [CanEditMultipleObjects]
    public class PatchMakerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();
            GeneratePatchesFromStereoSkybox patchMaker = (GeneratePatchesFromStereoSkybox)target;
            if (GUILayout.Button("Generate Stereo Patch Textures"))
            {
                //hypothetically, I could loop through a list of textures and generate suffixes for each
                patchMaker.Generate();
            }
            serializedObject.ApplyModifiedProperties();
        }
    }

    public Texture2D LeftEyeTexture;
    public Texture2D RightEyeTexture;

    public string patchName = "patch_{0}";

    public bool generateMaterial = false;

    CreatePanoramaPatch genPatch;

    public void Generate()
    {
        if (genPatch == null)
            genPatch = GetComponent<CreatePanoramaPatch>();
        genPatch.patchName = string.Format(patchName,"L");
        genPatch.GeneratePatchTexture(LeftEyeTexture, "", false);
        genPatch.patchName = string.Format(patchName, "R");
        genPatch.GeneratePatchTexture(RightEyeTexture, "", false);

        if (generateMaterial)
        {

        }
    }
}