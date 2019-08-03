using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

[RequireComponent(typeof(CreatePanoramaPatch))]
public class GeneratePatchFromVideo : MonoBehaviour {

    [SerializeField]
    VideoPlayer player;

    [SerializeField]
    string patchName = "patch_{0}";

    CreatePanoramaPatch patchMaker;

	void Start () {
        Time.captureFramerate = Mathf.RoundToInt((float)player.clip.frameRate);
	}
	
	void Update () {
		if (player.isPlaying)
        {
            if (patchMaker == null)
                patchMaker = GetComponent<CreatePanoramaPatch>();
            //player.frame
            patchMaker.patchName = string.Format(patchName, player.frame);
            patchMaker.GeneratePatchTexture(player.texture, "", false);
        }
        ScreenCapture.CaptureScreenshotAsTexture();
    }
}
