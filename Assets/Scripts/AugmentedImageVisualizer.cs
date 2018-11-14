using GoogleARCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class AugmentedImageVisualizer : MonoBehaviour {

    [SerializeField] private VideoClip[] _videoClips;
    public GoogleARCore.AugmentedImage Image;
    private VideoPlayer _vp;

	void Start () {
        _vp = GetComponent<VideoPlayer>();
        _vp.loopPointReached += OnStop;
	}
	
    private void OnStop(VideoPlayer src)
    {
        gameObject.SetActive(false);
    }

	void Update () {
		if (Image == null || Image.TrackingState != TrackingState.Tracking) {
            return; // exit if no image or not tracking
        }

        if (!_vp.isPlaying)
        {
            _vp.clip = _videoClips[Image.DatabaseIndex];
            _vp.Play();
        }

        transform.localScale = new Vector3(Image.ExtentX, Image.ExtentZ, 1);
    }
}
