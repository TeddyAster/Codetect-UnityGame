// Created by SwanDEV 2017

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public sealed class ProGifPlayerRawImage : ProGifPlayerComponent
{
    [HideInInspector] public RawImage destinationRawImage;                        // The RawImage for displaying textures
    private List<RawImage> m_ExtraRawImages = new List<RawImage>();

	private Texture2D _displayTexture2D = null;

    void Awake()
    {
        if (destinationRawImage == null)
        {
            destinationRawImage = gameObject.GetComponent<RawImage>();
            m_DisplayType = DisplayType.RawImage;
        }
    }

    // Update gif frame for the Player (Update is called once per frame)
    void Update()
    {
        base.ThreadsUpdate();

        if (State == PlayerState.Playing && m_DisplayType == DisplayType.RawImage)
        {
            float time = IgnoreTimeScale ? Time.unscaledTime : Time.time;
            float dt = Mathf.Min(time - _nextFrameTime, interval);
            if (dt >= 0f)
            {
                m_SpriteIndex = (m_SpriteIndex >= m_GifTextures.Count - 1) ? 0 : m_SpriteIndex + 1;
                _nextFrameTime = time + interval / playbackSpeed - dt;

                if (m_SpriteIndex < m_GifTextures.Count)
	            {
	                if (OnPlayingCallback != null) OnPlayingCallback(m_GifTextures[m_SpriteIndex]);

					_SetDisplay(m_SpriteIndex);

	                if(m_ExtraRawImages != null && m_ExtraRawImages.Count > 0)
	                {
						Texture2D  tex = null;
						if(m_OptimizeMemoryUsage)
						{
							tex = _displayTexture2D;
						}
						else
						{
							tex = m_GifTextures[m_SpriteIndex].GetTexture2D();
						}

	                    for(int i = 0; i < m_ExtraRawImages.Count; i++)
	                    {
	                        if(m_ExtraRawImages[i] != null)
	                        {
	                            m_ExtraRawImages[i].texture = tex;
	                        }
	                        else
	                        {
	                            m_ExtraRawImages.RemoveAt(i);
	                            m_ExtraRawImages.TrimExcess();
	                        }
	                    }
	                }
	            }
			}
        }
    }

	public override void Play(RenderTexture[] gifFrames, float fps, bool isCustomRatio, int customWidth, int customHeight, bool optimizeMemoryUsage)
	{
		base.Play(gifFrames, fps, isCustomRatio, customWidth, customHeight, optimizeMemoryUsage);

		destinationRawImage = gameObject.GetComponent<RawImage>();
        m_DisplayType = DisplayType.RawImage;
        _SetDisplay(0);
	}

    protected override void _OnFrameReady(GifTexture gTex, bool isFirstFrame)
    {
        if (isFirstFrame)
        {
            m_DisplayType = DisplayType.RawImage;
            _SetDisplay(0);
        }
    }

	private void _SetDisplay(int frameIndex)
	{
		if(m_OptimizeMemoryUsage)
		{
			m_GifTextures[frameIndex].SetColorsToTexture2D(ref _displayTexture2D);
		}

		if(destinationRawImage != null)
		{
			if(m_OptimizeMemoryUsage)
			{
				destinationRawImage.texture = _displayTexture2D;
			}
			else
			{
				destinationRawImage.texture = m_GifTextures[frameIndex].GetTexture2D();
			}
		}
	}

    public override void Clear(bool clearBytes = true, bool clearCallbacks = true)
    {
		if(_displayTexture2D != null) 
		{
			Destroy(_displayTexture2D);
		}
        base.Clear(clearBytes, clearCallbacks);
    }

    public void ChangeDestination(RawImage rawImage)
    {
        if (destinationRawImage != null) destinationRawImage.texture = null;
        destinationRawImage = rawImage;
    }

    public void AddExtraDestination(RawImage rawImage)
    {
        if(!m_ExtraRawImages.Contains(rawImage))
        {
            m_ExtraRawImages.Add(rawImage);
        }
    }

    public void RemoveFromExtraDestination(RawImage rawImage)
    {
        if(m_ExtraRawImages.Contains(rawImage))
        {
            m_ExtraRawImages.Remove(rawImage);
            m_ExtraRawImages.TrimExcess();
        }
    }
}
