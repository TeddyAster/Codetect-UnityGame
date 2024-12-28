// Created by SwanDEV 2017

using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public sealed class ProGifPlayerRenderer : ProGifPlayerComponent
{
	[HideInInspector] public Renderer destinationRenderer;				// The renderer for displaying textures
	private List<Renderer> m_ExtraRenderers = new List<Renderer>();

	private Texture2D _displayTexture2D = null;

	void Awake()
	{
		if(destinationRenderer == null)
		{
			destinationRenderer = gameObject.GetComponent<Renderer>();
            m_DisplayType = DisplayType.Renderer;
        }
	}

	// Update gif frame for the Player (Update is called once per frame)
	void Update()
	{
		base.ThreadsUpdate();

		if(State == PlayerState.Playing && m_DisplayType == DisplayType.Renderer)
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

					if(m_ExtraRenderers != null && m_ExtraRenderers.Count > 0)
					{
						Texture2D tex = null;
						if(m_OptimizeMemoryUsage)
						{
							tex = _displayTexture2D;
						}
						else
						{
							tex = m_GifTextures[m_SpriteIndex].GetTexture2D();
						}

						for(int i = 0; i < m_ExtraRenderers.Count; i++)
						{
							if(m_ExtraRenderers[i] != null)
							{
								m_ExtraRenderers[i].material.mainTexture = tex;
							}
							else
							{
								m_ExtraRenderers.RemoveAt(i);
								m_ExtraRenderers.TrimExcess();
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

		destinationRenderer = gameObject.GetComponent<Renderer>();
        m_DisplayType = DisplayType.Renderer;
        _SetDisplay(0);
	}

    protected override void _OnFrameReady(GifTexture gTex, bool isFirstFrame)
	{
        if (isFirstFrame)
        {
            m_DisplayType = DisplayType.Renderer;
            _SetDisplay(0);
        }
	}

	private void _SetDisplay(int frameIndex)
	{
		if(m_OptimizeMemoryUsage)
		{
			m_GifTextures[frameIndex].SetColorsToTexture2D(ref _displayTexture2D);
		}

		if(destinationRenderer != null && destinationRenderer.material != null) 
		{
			if(m_OptimizeMemoryUsage)
			{
				destinationRenderer.material.mainTexture = _displayTexture2D;
			}
			else
			{
				destinationRenderer.material.mainTexture = m_GifTextures[frameIndex].GetTexture2D();
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

	public void ChangeDestination(Renderer renderer)
	{
        if(destinationRenderer != null && destinationRenderer.material != null) destinationRenderer.material.mainTexture = null;
        destinationRenderer = renderer;
	}

	public void AddExtraDestination(Renderer renderer)
	{
		if(!m_ExtraRenderers.Contains(renderer))
		{
			m_ExtraRenderers.Add(renderer);
		}
	}

	public void RemoveFromExtraDestination(Renderer renderer)
	{
		if(m_ExtraRenderers.Contains(renderer))
		{
			m_ExtraRenderers.Remove(renderer);
			m_ExtraRenderers.TrimExcess();
		}
	}
}
