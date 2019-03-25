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
    private Texture panoramaTexture;

    [SerializeField]
    private Vector2 imageSize = new Vector2(512,512);

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
        var vertices = sourceMesh.vertices;
        foreach (var vertex in vertices)
        {
            var rp = RelativePosition(vertex);
            var ll = PositionToLatLong(rp);
            if (ll.x < 0) ll.x += 1f;
            if (ll.y < 0) ll.y += 1f;

            minX = Mathf.Min(minX, ll.x);
            maxX = Mathf.Max(maxX, ll.x);
            minY = Mathf.Min(minY, ll.y);
            maxY = Mathf.Max(maxY, ll.y);
        }

        Debug.LogFormat("{0},{1}  |  {2},{3}   {4} {5}", minX, minY, maxX, maxY, maxX - minX, (maxY-minY));

        var goalRender = new RenderTexture(Mathf.RoundToInt(imageSize.x), Mathf.RoundToInt(imageSize.y), 16);
        var blitMaterial = new Material(Shader.Find("Unlit/FromSkyboxToPatch"));
        blitMaterial.SetFloat("_MinX", minX);
        blitMaterial.SetFloat("_MaxX", maxX);
        blitMaterial.SetFloat("_MinY", minY);
        blitMaterial.SetFloat("_MaxY", maxY);
        Graphics.Blit(panoramaTexture, goalRender, blitMaterial, -1);
        SaveRenderImage(goalRender);
        //save material?
        var patchMaterial = new Material(Shader.Find("Unlit/FromPatchToSkybox"));
        patchMaterial.SetFloat("_MinX", minX);
        patchMaterial.SetFloat("_MaxX", maxX);
        patchMaterial.SetFloat("_MinY", minY);
        patchMaterial.SetFloat("_MaxY", maxY);

        var savedTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/GeneratedTextures/" + patchName + ".png");

        patchMaterial.SetTexture("_Patch", savedTexture);
        AssetDatabase.CreateAsset(patchMaterial, "Assets/GeneratedMaterials/"+patchName+".mat");
        patchMesh.material = patchMaterial;
    }

    private void SaveRenderImage(RenderTexture goalRender)
    {
        
        //save goalRender texture and blitMaterial as resources
        var saveTex = new Texture2D(goalRender.width, goalRender.height, TextureFormat.RGB24, false);
        RenderTexture.active = goalRender;
        saveTex.ReadPixels(new Rect(0, 0, goalRender.width, goalRender.height), 0, 0);
        RenderTexture.active = null;
        byte[] bytes;
        bytes = saveTex.EncodeToPNG();
        System.IO.File.WriteAllBytes("Assets/GeneratedTextures/"+patchName+".png", bytes);
    }

    private Vector2 PositionToLatLong(Vector3 coords)
    {
        var normalizedCoords = coords.normalized;
        float latitude = Mathf.Acos(normalizedCoords.y);
        float longitude = Mathf.Atan2(normalizedCoords.z, normalizedCoords.x);
        Vector2 sphereCoords = new Vector2(longitude * 0.5f / Mathf.PI, latitude* 1.0f / Mathf.PI);
        return new Vector2(-0.25f, 1.0f) - sphereCoords;//-.25 offset is arbitrary but it matches how cubemaps are sampled in Unity
    }

    private Vector3 RelativePosition(Vector3 vertexPosition)
    {
        return patchMesh.transform.TransformPoint(vertexPosition) - cameraPosition.position;
    }

    private float CompareX(Vector3 position)
    {

        return 0;
    }

}
