// Created by SwanDEV 2017

using System;
using UnityEngine;

public sealed class ProGifPlayerTexture2D : ProGifPlayerComponent
{
    [HideInInspector] public Texture2D m_Texture2D;

    public Action<Texture2D> OnTexture2DCallback;

    void Awake()
    {
        m_DisplayType = DisplayType.None;
    }

    // Update gif frame for the Player (Update is called once per frame)
    void Update()
    {
        base.ThreadsUpdate();

        if (State == PlayerState.Playing && m_DisplayType == DisplayType.None)
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

                    _SetTexture(m_SpriteIndex);
                }
            }
        }
    }

    public override void Play(RenderTexture[] gifFrames, float fps, bool isCustomRatio, int customWidth, int customHeight, bool optimizeMemoryUsage)
    {
        base.Play(gifFrames, fps, isCustomRatio, customWidth, customHeight, optimizeMemoryUsage);
        
        m_DisplayType = DisplayType.None;
        _SetTexture(0);
    }

    protected override void _OnFrameReady(GifTexture gTex, bool isFirstFrame)
    {
        if (isFirstFrame)
        {
            m_DisplayType = DisplayType.None;
            _SetTexture(0);
        }
    }

    private void _SetTexture(int frameIndex)
    {
        if (m_OptimizeMemoryUsage)
        {
            m_GifTextures[frameIndex].SetColorsToTexture2D(ref m_Texture2D);
            if (OnTexture2DCallback != null) OnTexture2DCallback(m_Texture2D);
        }
    }

    public override void Clear(bool clearBytes = true, bool clearCallbacks = true)
    {
        if (m_Texture2D != null)
        {
            Destroy(m_Texture2D);
        }
        base.Clear(clearBytes, clearCallbacks);
    }
}
