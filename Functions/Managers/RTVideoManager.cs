using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.Video;

namespace RTFunctions.Functions.Managers
{
    public class RTVideoManager : MonoBehaviour
	{
		public static RTVideoManager inst;

		public static string className = "[<color=#e65100>RTVideoManager</color>] \n";

		public enum RenderType
        {
			Camera, // Always renders at the camera's resolution and position.
			Background // Renders at a set spot.
        }

		public RenderType renderType = RenderType.Background;

		public VideoPlayer videoPlayer;

		public GameObject videoTexture;

		public event Action<bool, float, float> UpdatedAudioPos;

		bool prevPlaying;
		float prevTime;
		float prevPitch;

		bool canUpdate = true;

		public static void Init()
        {
			var gameObject = new GameObject("VideoManager");
			gameObject.transform.SetParent(SystemManager.inst.transform);
			gameObject.AddComponent<RTVideoManager>();
        }

		void Awake()
        {
			inst = this;

			var videoObject = new GameObject("VideoPlayer");
			videoObject.transform.SetParent(SystemManager.inst.transform);
			videoPlayer = videoObject.AddComponent<VideoPlayer>();
			//videoPlayer.targetCamera = Camera.main;
			videoPlayer.renderMode = renderType == RenderType.Camera ? VideoRenderMode.CameraFarPlane : VideoRenderMode.MaterialOverride;
			videoPlayer.source = VideoSource.VideoClip;
			//videoPlayer.targetCameraAlpha = 1f;
			videoPlayer.timeSource = VideoTimeSource.GameTimeSource;
			videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
			videoPlayer.isLooping = false;
			videoPlayer.waitForFirstFrame = false;

			UpdatedAudioPos += UpdateTime;
		}

		void Update()
        {
            //if (videoPlayer != null && videoPlayer.enabled && videoPlayer.isPrepared
            //    && (prevPlaying != AudioManager.inst.CurrentAudioSource.isPlaying
            //    || prevTime != AudioManager.inst.CurrentAudioSource.time
            //    || prevPitch != AudioManager.inst.CurrentAudioSource.pitch))
            //{
            //    prevPlaying = AudioManager.inst.CurrentAudioSource.isPlaying;
            //    prevTime = AudioManager.inst.CurrentAudioSource.time;
            //    prevPitch = AudioManager.inst.CurrentAudioSource.pitch;
            //    UpdatedAudioPos?.Invoke(AudioManager.inst.CurrentAudioSource.isPlaying, AudioManager.inst.CurrentAudioSource.time, AudioManager.inst.CurrentAudioSource.pitch);
            //}

            //if (canUpdate)
            //{
            //    float t = AudioManager.inst.CurrentAudioSource.time;

            //    //if (t < videoPlayer.time)
            //    //    t = -((float)videoPlayer.time) + t;

            //    if (videoPlayer != null && videoPlayer.enabled && videoPlayer.isPrepared)
            //        UpdatedAudioPos?.Invoke(AudioManager.inst.CurrentAudioSource.isPlaying, t, AudioManager.inst.CurrentAudioSource.pitch);
            //    prevPlaying = AudioManager.inst.CurrentAudioSource.isPlaying;
            //    prevTime = t;
            //    prevPitch = AudioManager.inst.CurrentAudioSource.pitch;
            //}
            if (canUpdate && (prevTime != AudioManager.inst.CurrentAudioSource.time || prevPlaying != AudioManager.inst.CurrentAudioSource.isPlaying))
			{
				if (videoPlayer != null && videoPlayer.enabled && videoPlayer.isPrepared)
				{
					UpdatedAudioPos?.Invoke(AudioManager.inst.CurrentAudioSource.isPlaying, AudioManager.inst.CurrentAudioSource.time, AudioManager.inst.CurrentAudioSource.pitch);
				}
			}
			prevPlaying = AudioManager.inst.CurrentAudioSource.isPlaying;
			prevTime = AudioManager.inst.CurrentAudioSource.time;
			prevPitch = AudioManager.inst.CurrentAudioSource.pitch;
		}

		public void SetType(RenderType renderType)
		{
			this.renderType = renderType;
			videoPlayer.renderMode = this.renderType == RenderType.Camera ? VideoRenderMode.CameraFarPlane : VideoRenderMode.MaterialOverride;
			if (!videoTexture && GameObject.Find("ExtraBG") && GameObject.Find("ExtraBG").transform.childCount > 0)
			{
				videoTexture = GameObject.Find("ExtraBG").transform.GetChild(0).gameObject;
				videoPlayer.targetMaterialRenderer = videoTexture.GetComponent<MeshRenderer>();
			}

			Play(currentURL, currentAlpha);
		}

		void UpdateTime(bool isPlaying, float time, float pitch)
		{
			//videoPlayer.playbackSpeed = pitch < 0f ? -pitch : pitch;
			if (isPlaying)
			{
				if (!videoPlayer.isPlaying)
					videoPlayer.Play();
				videoPlayer.Pause();

				videoPlayer.time = time;
			}
            else
			{
				videoPlayer.Pause();
			}
		}

		public string currentURL;
		public float currentAlpha;
		public bool didntPlay = false;

        public void Play(string url, float alpha)
		{
			currentURL = url;
			currentAlpha = alpha;

			if (!FunctionsPlugin.EnableVideoBackground.Value)
			{
				videoPlayer.enabled = false;
				videoTexture?.SetActive(false);
				didntPlay = true;
				return;
			}

			if (!videoTexture && GameObject.Find("ExtraBG") && GameObject.Find("ExtraBG").transform.childCount > 0)
			{
				videoTexture = GameObject.Find("ExtraBG").transform.GetChild(0).gameObject;
				videoPlayer.targetMaterialRenderer = videoTexture.GetComponent<MeshRenderer>();
			}

			Debug.Log($"{className}Playing Video from {url}");
			videoTexture?.SetActive(renderType == RenderType.Background);
			videoPlayer.enabled = true;
			videoPlayer.targetCameraAlpha = alpha;
			videoPlayer.source = VideoSource.Url;
			videoPlayer.url = url;
			videoPlayer.Prepare();
			didntPlay = false;
		}

		public void Stop()
		{
			Debug.Log($"{className}Stopping Video.");
			videoPlayer.enabled = false;
			videoTexture?.SetActive(false);
		}
	}
}
