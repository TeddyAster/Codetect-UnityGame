// Created by SwanDEV 2017

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public sealed class ProGifPlayerImage : ProGifPlayerComponent
{
	[HideInInspector] public Image destinationImage;						// The image for displaying sprites
	private List<Image> m_ExtraImages = new List<Image>();

	private Texture2D _displayTexture2D = null;
	private Sprite _displaySprite = null;

	void Awake()
	{
		if(destinationImage == null)
		{
			destinationImage = gameObject.GetComponent<Image>();
            m_DisplayType = DisplayType.Image;
		}
	}

	// Update gif frame for the Player (Update is called once per frame)
	void Update()
	{
		base.ThreadsUpdate();
        
		if(State == PlayerState.Playing && m_DisplayType == DisplayType.Image)
        {
            float time = IgnoreTimeScale ? Time.unscaledTime : Time.time;
            float dt = Mathf.Min(time - _nextFrameTime, interval);
            if (dt >= 0f)
            {
				m_SpriteIndex = (m_SpriteIndex >= m_GifTextures.Count - 1)? 0 : m_SpriteIndex + 1;
                _nextFrameTime = time + interval / playbackSpeed - dt;

                if (m_SpriteIndex < m_GifTextures.Count)
				{
					if(OnPlayingCallback != null) OnPlayingCallback(m_GifTextures[m_SpriteIndex]);

					_SetDisplay(m_SpriteIndex);

					if(m_ExtraImages != null && m_ExtraImages.Count > 0)
					{
						Sprite sp = null;
						if(m_OptimizeMemoryUsage)
						{
							sp = _displaySprite;
						}
						else
						{
							sp = m_GifTextures[m_SpriteIndex].GetSprite();
						}

						for(int i = 0; i < m_ExtraImages.Count; i++)
						{
							if(m_ExtraImages[i] != null)
							{
								m_ExtraImages[i].sprite = sp;
							}
							else
							{
								m_ExtraImages.RemoveAt(i);
								m_ExtraImages.TrimExcess();
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

		destinationImage = gameObject.GetComponent<UnityEngine.UI.Image>();
        m_DisplayType = DisplayType.Image;
        _SetDisplay(0);
	}

    protected override void _OnFrameReady(GifTexture gTex, bool isFirstFrame)
    {
        if (isFirstFrame)
        {
            m_DisplayType = DisplayType.Image;
            _SetDisplay(0);
        }
	}

	private void _SetDisplay(int frameIndex)
	{
		if(m_OptimizeMemoryUsage)
		{
			_displaySprite = m_GifTextures[frameIndex].GetSprite_OptimizeMemoryUsage(ref _displayTexture2D);
		}

		if(destinationImage != null)
		{
			if(m_OptimizeMemoryUsage)
			{
				destinationImage.sprite = _displaySprite;
			}
			else
			{
				destinationImage.sprite = m_GifTextures[frameIndex].GetSprite();
			}
		}
	}

	public override void Clear(bool clearBytes = true, bool clearCallbacks = true)
	{
		if(m_OptimizeMemoryUsage)
		{
			if(_displayTexture2D != null) 
			{
				Destroy(_displayTexture2D);
				_displayTexture2D = null;
			}

			_displaySprite = null;
		}
		base.Clear(clearBytes, clearCallbacks);
	}

	public void ChangeDestination(UnityEngine.UI.Image image)
    {
        if (destinationImage != null) destinationImage.sprite = null;
        destinationImage = image;
	}

	public void AddExtraDestination(UnityEngine.UI.Image image)
	{
		if(!m_ExtraImages.Contains(image))
		{
			m_ExtraImages.Add(image);
		}
	}

	public void RemoveFromExtraDestination(UnityEngine.UI.Image image)
	{
		if(m_ExtraImages.Contains(image))
		{
			m_ExtraImages.Remove(image);
			m_ExtraImages.TrimExcess();
		}
	}
}
