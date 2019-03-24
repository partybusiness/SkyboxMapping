using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CreatePanoramaPatch : MonoBehaviour {

    [CustomEditor(typeof(CreatePanoramaPatch))]
    [CanEditMultipleObjects]
    public class PatchMakerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();
            CreatePanoramaPatch patchMaker = (CreatePanoramaPatch)target;
            if (GUILayout.Button("Generate Patch Texture"))
            {
                patchMaker.GeneratePatchTexture();
            }
            serializedObject.ApplyModifiedProperties();
        }
    }

    [SerializeField]
    Transform cameraPosition;

    /// <summary>
    /// How far in any direction should the camera be able to move
    /// </summary>
    [SerializeField]
    float postitionOffset = 0.4f;

    [SerializeField]
    private MeshRenderer patchMesh;

    [SerializeField]
    private string patchName = "patch";

    public void GeneratePatchTexture()
    {
        //find min and max for selected mesh
        var patchTransform = patchMesh.transform;
        var patchMF = patchMesh.GetComponent<MeshFilter>();
        var sourceMesh = patchMF.sharedMesh;
        var minX = Mathf.Infinity;
        var maxX = -Mathf.Infinity;
        var minY = Mathf.Infinity;
        var maxY = -Mathf.Infinity;

        //loop through all points in the mesh?

        Debug.LogFormat("{0},{1}  |  {2},{3}", minX, maxX, minY, maxY);
    }

    private Vector2 PositionToLatLong(Vector3 coords)
    {
        var normalizedCoords = coords.normalized;
        float latitude = Mathf.Acos(normalizedCoords.y);
        float longitude = Mathf.Atan2(normalizedCoords.z, normalizedCoords.x);
        Vector2 sphereCoords = new Vector2(longitude * 0.5f / Mathf.PI, latitude* 1.0f / Mathf.PI);
        return new Vector2(-0.25f, 1.0f) - sphereCoords;//???
    }

    private float CompareX(Vector3 position)
    {
        return 0;
    }

}
