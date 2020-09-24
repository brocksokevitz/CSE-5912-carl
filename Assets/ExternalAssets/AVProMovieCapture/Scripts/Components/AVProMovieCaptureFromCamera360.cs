#if UNITY_5
	#if !UNITY_5_0 && !UNITY_5_1
		#define AVPRO_MOVIECAPTURE_GLISSUEEVENT_52
	#endif
	#if !UNITY_5_0 && !UNITY_5_1 && !UNITY_5_2 && !UNITY_5_3
		#define AVPRO_MOVIECAPTURE_RENDERTEXTUREDIMENSIONS_54
	#endif
#endif
using UnityEngine;

//-----------------------------------------------------------------------------
// Copyright 2012-2017 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

[RequireComponent(typeof(Camera))]
[AddComponentMenu("AVPro Movie Capture/From Camera 360 VR (requires Camera)")]
public class AVProMovieCaptureFromCamera360 : AVProMovieCaptureBase
{
	[Header("Cube map")]
	public int _cubemapResolution = 2048;
	public int _cubemapDepth = 24;
	public bool _supportGUI = true;
	public bool _supportCameraRotation = true;

	private RenderTexture _faceTarget;
	private Material _blitMaterial;
	private Material _cubemapToEquirectangularMaterial;
	private RenderTexture _cubeTarget;
	private RenderTexture _finalTarget;
	private Camera _camera;
	private int _propFlipX;

	protected bool IsManualCubemapRendering()
	{
		return (_supportGUI || _supportCameraRotation);
	}

#if false
    private void OnRenderImage(RenderTexture source, RenderTexture dest)
	{
#if false
		if (_capturing && !_paused)
		{
			while (_handle >= 0 && !AVProMovieCapturePlugin.IsNewFrameDue(_handle))
			{
				System.Threading.Thread.Sleep(1);
			}
			if (_handle >= 0)
			{
                if (_audioCapture && _audioDeviceIndex < 0 && !_noAudio)
                {
                    uint bufferLength = (uint)_audioCapture.BufferLength;
                    if (bufferLength > 0)
                    {
                        AVProMovieCapturePlugin.EncodeAudio(_handle, _audioCapture.BufferPtr, bufferLength);
                        _audioCapture.FlushBuffer();
                    }
                }

                // In Direct3D the RT can be flipped vertically
                /*if (source.texelSize.y < 0)
                {

                }*/

				Graphics.Blit(_cubeTarget, _target, _cubemapToEquirectangularMaterial);

#if AVPRO_MOVIECAPTURE_GLISSUEEVENT_52
				GL.IssuePluginEvent(_renderEventFunction, AVProMovieCapturePlugin.PluginID | (int)AVProMovieCapturePlugin.PluginEvent.CaptureFrameBuffer | _handle);
#else
                GL.IssuePluginEvent(AVProMovieCapturePlugin.PluginID | (int)AVProMovieCapturePlugin.PluginEvent.CaptureFrameBuffer | _handle);
#endif
				GL.InvalidateState();
				
				UpdateFPS();
			}
		}
#endif
		// Pass-through

		if (_cubeTarget != null)
		{
			Graphics.Blit(_cubeTarget, dest, _cubemapToEquirectangularMaterial);
		}
		else
		{
			Graphics.Blit(source, dest);
		}
	}
#endif

	public override void UpdateFrame()
	{
		if (_capturing && !_paused)
		{
			if (_cubeTarget != null && _camera != null)
			{
				bool canGrab = true;

				if (_useMotionBlur && !_isRealTime && _motionBlur != null)
				{
					// TODO: fix motion blur
					//this._motionBlur.RenderImage()
					// If the motion blur is still accumulating, don't grab this frame
					canGrab = _motionBlur.IsFrameAccumulated;
				}

				if (canGrab)
				{
					while (_handle >= 0 && !AVProMovieCapturePlugin.IsNewFrameDue(_handle))
					{
						System.Threading.Thread.Sleep(1);
					}
					if (_handle >= 0)
					{
						if (_audioCapture && _audioDeviceIndex < 0 && !_noAudio && _isRealTime)
						{
							uint bufferLength = (uint)_audioCapture.BufferLength;
							if (bufferLength > 0)
							{
								AVProMovieCapturePlugin.EncodeAudio(_handle, _audioCapture.BufferPtr, bufferLength);
								_audioCapture.FlushBuffer();
							}
						}

						// In Direct3D the RT can be flipped vertically
						/*if (source.texelSize.y < 0)
						{

						}*/

						//_cubeCamera.transform.position = _camera.transform.position;
						//_cubeCamera.transform.rotation = _camera.transform.rotation;

						Camera camera = _camera;

						if (!IsManualCubemapRendering())
						{
							// Note: Camera.RenderToCubemap() doesn't support camera rotation
							camera.RenderToCubemap(_cubeTarget, 63);
						}
						else
						{
							// Cache old camera values
							float prevFieldOfView = _camera.fieldOfView;
							RenderTexture prevtarget = _camera.targetTexture;
							Quaternion prevRotation = camera.transform.rotation;
							Quaternion xform = camera.transform.rotation;

							// Ignore the camera rotation
							if (!_supportCameraRotation)
							{
								xform = Quaternion.identity;
							}

							camera.targetTexture = _faceTarget;
							camera.fieldOfView = 90f;

							camera.transform.rotation = xform * Quaternion.LookRotation(Vector3.forward, Vector3.down);
							camera.Render();
							Graphics.SetRenderTarget(_cubeTarget, 0, CubemapFace.PositiveZ);
							Graphics.Blit(_faceTarget, _blitMaterial);

							camera.transform.rotation = xform * Quaternion.LookRotation(Vector3.back, Vector3.down);
							camera.Render();
							Graphics.SetRenderTarget(_cubeTarget, 0, CubemapFace.NegativeZ);
							Graphics.Blit(_faceTarget, _blitMaterial);

							camera.transform.rotation = xform * Quaternion.LookRotation(Vector3.right, Vector3.down);
							camera.Render();
							Graphics.SetRenderTarget(_cubeTarget, 0, CubemapFace.NegativeX);
							Graphics.Blit(_faceTarget, _blitMaterial);

							camera.transform.rotation = xform * Quaternion.LookRotation(Vector3.left, Vector3.down);
							camera.Render();
							Graphics.SetRenderTarget(_cubeTarget, 0, CubemapFace.PositiveX);
							Graphics.Blit(_faceTarget, _blitMaterial);
							
							camera.transform.rotation = xform * Quaternion.LookRotation(Vector3.up, Vector3.forward);
							camera.Render();
							Graphics.SetRenderTarget(_cubeTarget, 0, CubemapFace.PositiveY);
							Graphics.Blit(_faceTarget, _blitMaterial);

							camera.transform.rotation = xform * Quaternion.LookRotation(Vector3.down, Vector3.back);
							camera.Render();
							Graphics.SetRenderTarget(_cubeTarget, 0, CubemapFace.NegativeY);
							Graphics.Blit(_faceTarget, _blitMaterial);
							
							Graphics.SetRenderTarget(null);

							// Restore camera values
							camera.transform.rotation = prevRotation;
							camera.targetTexture = prevtarget;
							camera.fieldOfView = prevFieldOfView;
						}

						Graphics.Blit(_cubeTarget, _finalTarget, _cubemapToEquirectangularMaterial);

						// TODO: motion blur accumulate here!  Currently it's grabbing from the camera which is wrong because it's not the full 360 capture...

						// TODO: cache GetNativeTexturePtr() as it causes a GPU thread sync!
						AVProMovieCapturePlugin.SetTexturePointer(_handle, _finalTarget.GetNativeTexturePtr());

#if AVPRO_MOVIECAPTURE_GLISSUEEVENT_52
						GL.IssuePluginEvent(_renderEventFunction, AVProMovieCapturePlugin.PluginID | (int)AVProMovieCapturePlugin.PluginEvent.CaptureFrameBuffer | _handle);
#else
						GL.IssuePluginEvent(AVProMovieCapturePlugin.PluginID | (int)AVProMovieCapturePlugin.PluginEvent.CaptureFrameBuffer | _handle);
#endif
						GL.InvalidateState();

						UpdateFPS();
					}
				}
			}
		}

		base.UpdateFrame();
	}

	public override bool PrepareCapture()
	{
		if (_capturing)
		{
			return false;
		}
		
		// Setup material
		_pixelFormat = AVProMovieCapturePlugin.PixelFormat.RGBA32;
        _isTopDown = true;
		_camera = this.GetComponent<Camera>();

		// Resolution
		int finalWidth = Mathf.FloorToInt(_camera.pixelRect.width);
		int finalHeight = Mathf.FloorToInt(_camera.pixelRect.height);
		if (_renderResolution == Resolution.Custom)
		{
			finalWidth = _renderWidth;
			finalHeight = _renderHeight;
		}
		else if (_renderResolution != Resolution.Original)
		{
			GetResolution(_renderResolution, ref finalWidth, ref finalHeight);
		}

		// Setup rendering a different render target if we're overriding resolution or anti-aliasing
		//if (_renderResolution != Resolution.Original || _renderAntiAliasing != QualitySettings.antiAliasing)
		{


			// Anti-aliasing 
			int aaLevel = QualitySettings.antiAliasing;
			if (aaLevel == 0)
			{
				aaLevel = 1;
			}
			if (_renderAntiAliasing > 0)
			{
				aaLevel = _renderAntiAliasing;
			}
			if (aaLevel != 1 && aaLevel != 2 && aaLevel != 4 && aaLevel != 8)
			{
				Debug.LogError("[AVProMovieCapture] Invalid antialiasing value, must be 1, 2, 4 or 8.  Defaulting to 1. >> " + aaLevel);
				aaLevel = 1;
			}

			if (!Mathf.IsPowerOfTwo(_cubemapResolution))
			{
				_cubemapResolution = Mathf.ClosestPowerOfTwo(_cubemapResolution);
				Debug.LogWarning("[AVProMovieCapture] Cubemap must be power-of-2 dimensions, resizing to closest = " + _cubemapResolution);
			}

			// Create the final render target
			if (_finalTarget != null)
			{
				_finalTarget.DiscardContents();
				if (_finalTarget.width != finalWidth || _finalTarget.height != finalHeight)
				{
					RenderTexture.ReleaseTemporary(_finalTarget);
					_finalTarget = null;
				}
			}
			if (_finalTarget == null)
			{
				_finalTarget = RenderTexture.GetTemporary(finalWidth, finalHeight, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Default, 1);
				_finalTarget.name = "[AVProMovieCapture] 360 Final Target";
			}

			// Create the per-face render target (only when need to support GUI rendering)
			if (_faceTarget != null)
			{
				_faceTarget.DiscardContents();
				if (_faceTarget.width != _cubemapResolution || _faceTarget.height != _cubemapResolution || aaLevel != _faceTarget.antiAliasing)
				{
					RenderTexture.Destroy(_faceTarget);
					_faceTarget = null;
				}
			}
			if (IsManualCubemapRendering())
			{
				if (_faceTarget == null)
				{
					_faceTarget = new RenderTexture(_cubemapResolution, _cubemapResolution, _cubemapDepth, RenderTextureFormat.Default, RenderTextureReadWrite.Default);
					_faceTarget.name = "[AVProMovieCapture] 360 Face Target";
					_faceTarget.isPowerOfTwo = true;
					_faceTarget.wrapMode = TextureWrapMode.Clamp;
					_faceTarget.filterMode = FilterMode.Bilinear;
					_faceTarget.autoGenerateMips = false;
					_faceTarget.antiAliasing = aaLevel;
				}

				_cubemapToEquirectangularMaterial.SetFloat(_propFlipX, 0.0f);
			}
			else
			{
				// Unity's RenderToCubemap result needs flipping
				_cubemapToEquirectangularMaterial.SetFloat(_propFlipX, 1.0f);
			}

			// Create the cube render target
			if (_cubeTarget != null)
			{
				_cubeTarget.DiscardContents();
				if (_cubeTarget.width != _cubemapResolution || _cubeTarget.height != _cubemapResolution || aaLevel != _cubeTarget.antiAliasing)
				{
					RenderTexture.Destroy(_cubeTarget);
					_cubeTarget = null;
				}
			}
			if (_cubeTarget == null)
			{
				int depth = 0;
				if (!IsManualCubemapRendering())
				{
					depth = _cubemapDepth;
				}
				_cubeTarget = new RenderTexture(_cubemapResolution, _cubemapResolution, depth, RenderTextureFormat.Default, RenderTextureReadWrite.Default);
				_cubeTarget.name = "[AVProMovieCapture] 360 Cube Target";
				_cubeTarget.isPowerOfTwo = true;

#if AVPRO_MOVIECAPTURE_RENDERTEXTUREDIMENSIONS_54
				_cubeTarget.dimension = UnityEngine.Rendering.TextureDimension.Cube;
#else
				_cubeTarget.isCubemap = true;
#endif

				_cubeTarget.useMipMap = false;
				_cubeTarget.autoGenerateMips = false;
				_cubeTarget.antiAliasing = 1;
				_cubeTarget.wrapMode = TextureWrapMode.Clamp;
				_cubeTarget.filterMode = FilterMode.Bilinear;
				if (!IsManualCubemapRendering())
				{
					_cubeTarget.antiAliasing = aaLevel;
				}
			}

			if (_useMotionBlur)
			{
				_motionBlurCameras = new Camera[1];
				_motionBlurCameras[0] = _camera;
			}
		}

		SelectRecordingResolution(finalWidth, finalHeight);

		GenerateFilename();

		return base.PrepareCapture();
	}

	public override void Start()
	{
		Shader shader = Resources.Load<Shader>("CubemapToEquirectangular");
		if (shader != null)
		{
			_cubemapToEquirectangularMaterial = new Material(shader);
		}
		else
		{
			Debug.LogError("[AVProMovieCapture] Can't find CubemapToEquirectangular shader");
		}

		_blitMaterial = new Material(Shader.Find("Hidden/BlitCopy"));
		_propFlipX = Shader.PropertyToID("_FlipX");

		base.Start();
	}

	public override void OnDestroy()
	{
		if (_blitMaterial != null)
		{
			Material.Destroy(_blitMaterial);
			_blitMaterial = null;
		}

		if (_faceTarget != null)
		{
			RenderTexture.Destroy(_faceTarget);
			_faceTarget = null;
		}
		if (_cubeTarget != null)
		{
			RenderTexture.Destroy(_cubeTarget);
			_cubeTarget = null;
		}
		if (_finalTarget != null)
		{
			RenderTexture.ReleaseTemporary(_finalTarget);
			_finalTarget = null;
		}
		base.OnDestroy();
	}
}