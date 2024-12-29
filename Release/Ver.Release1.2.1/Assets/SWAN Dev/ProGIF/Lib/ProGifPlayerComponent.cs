// Created by SwanDEV 2017
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using ThreadPriority = System.Threading.ThreadPriority;

#if UNITY_2017_3_OR_NEWER
using UnityEngine.Networking;
#endif

[DisallowMultipleComponent]
public abstract class ProGifPlayerComponent : MonoBehaviour
{
    public bool m_DebugLog;

    [Header("[ Save Settings ]")]
    [Tooltip("If 'true', save GIFs that download by URL (non-local files), to the below specified directory.")]
    public bool m_ShouldSaveFromWeb;
    [Tooltip("If 'true', use the Application.temporaryCachePath to save the downloaded GIFs, else use the Application.persistentDataPath(Build) or Application.dataPath(Editor).")]
    public bool m_UseTemporaryCachePath;
    [Tooltip("The sub-folder to store the downloaded GIFs.")]
    public string m_CacheFolder = "My GIFs";
    public string CacheDirectory
    {
        get
        {
            return FilePathName.Instance.GetSaveDirectory(m_UseTemporaryCachePath, m_CacheFolder, createDirectoryIfNotExist: true);
        }
    }

    [Header("[ Play Settings & Results ]")]
    [Tooltip("The local path or URL of the current GIF.")]
    public string m_LoadPath;

    [Tooltip("Common info about the current GIF.")]
    public ProGifDecoder.GifInfo m_GifInfo = null;

    /// <summary>
    /// The decoded GIF frames.
    /// </summary>
    [HideInInspector] public List<GifTexture> m_GifTextures = new List<GifTexture>();

    /// <summary>
    /// Indicates the display target is an Image, Renderer, or GUITexture, etc.
    /// </summary>
    [HideInInspector] public DisplayType m_DisplayType = DisplayType.None;

    /// <summary>
    /// The current sprite index to be played.
    /// </summary>
    [HideInInspector] public int m_SpriteIndex = 0;

    [HideInInspector] public int m_TotalFrame = 0;

    [Tooltip("Filter mode for the textures.")]
    public FilterMode m_FilterMode = FilterMode.Point;

    [Tooltip("Wrap mode for the textures.")]
    public TextureWrapMode m_WrapMode = TextureWrapMode.Clamp;

    [Tooltip("Sets the worker threads priority. This will only affect newly created threads.")]
    public ThreadPriority m_WorkerPriority = ThreadPriority.BelowNormal;


    /// <summary>
    /// Indicates the gif frames is loaded from recorder or decoder
    /// </summary> 
    private bool _isDecoderSource = false;
    /// <summary>
    /// The game time to show next frame.
    /// </summary>
    protected float _nextFrameTime = 0.0f;

    /// <summary>
    /// Gets the progress when load Gif from path/url.
    /// </summary>
    public float LoadingProgress
    {
        get
        {
            return (float)m_GifTextures.Count / (float)m_TotalFrame;
        }
    }

    public bool IsLoadingComplete
    {
        get
        {
            return LoadingProgress >= 1f;
        }
    }

    public bool IgnoreTimeScale
    {
        get
        {
            return _ignoreTimeScale;
        }
        set
        {
            _ignoreTimeScale = value;
            _nextFrameTime = value ? Time.unscaledTime : Time.time;
        }
    }
    private bool _ignoreTimeScale = true;   // Default: true;

    public enum PlayerState
    {
        None,
        Loading,
        Ready,
        Playing,
        Pause,
    }

    private PlayerState _state;
    /// <summary>
    /// The player playback state.
    /// </summary>
    public PlayerState State
    {
        get
        {
            return _state;
        }
        private set
        {
            _state = value;
            _nextFrameTime = IgnoreTimeScale ? Time.unscaledTime : Time.time;
        }
    }
    public void SetState(PlayerState state)
    {
        State = state;
    }

    /// <summary> Animation loop count (0 is infinite) </summary>
    public int loopCount
    {
        get;
        private set;
    }

    /// <summary> Texture width (px) </summary>
    public int width
    {
        get;
        private set;
    }

    /// <summary> Texture height (px) </summary>
    public int height
    {
        get;
        private set;
    }

    /// <summary> Default waiting time among frames. </summary> 
    private float _interval = 0.1f;
    /// <summary> Get the current frame waiting time. </summary> 
    public float interval
    {
        get
        {
            if (m_GifTextures.Count <= 0) return 0.1f;
            // For GIF that the delaySec is incorrectly set, we use 0.1f as its delaySec (or 10 FPS), which is one of the widely used framerate for GIF.
            // Here we let the maximum FPS of GIF be 60, while in the GIF specification it is 30. 
            return (m_GifTextures[m_SpriteIndex].m_delaySec <= 0.0166f) ? 0.1f : m_GifTextures[m_SpriteIndex].m_delaySec;
        }
    }

    /// <summary> Current playback speed of the GIF. </summary> 
    private float _playbackSpeed = 1.0f;
    /// <summary> Get/Set the playback speed of the GIF. (Default is 1.0f) </summary> 
    public float playbackSpeed
    {
        get
        {
            return _playbackSpeed;
        }
        set
        {
            float prevInterval = interval / _playbackSpeed;

            _playbackSpeed = Mathf.Max(0.01f, value);

            // update the next frame time
            float time = IgnoreTimeScale ? Time.unscaledTime : Time.time;
            float timeLeftPercent = _nextFrameTime > time ? (_nextFrameTime - time) / prevInterval : 0f;
            _nextFrameTime = time + (interval / _playbackSpeed) * timeLeftPercent;
        }
    }

    public enum DisplayType
    {
        None = 0,
        Image,
        Renderer,
        GuiTexture,
        RawImage,
    }

    //Decode settings
    public enum DecodeMode
    {
        /// <summary> Decode all gif frames. </summary>
        Normal = 0,

        /// <summary> Decode gif by skipping some frames, targetDecodeFrameNum is the number of frames to decode. </summary>
        Advanced,
    }
    public enum FramePickingMethod
    {
        /// <summary> Decode all GIF frames normally until the end or until the targetDecodeFrameNum. </summary>
        Default = 0,

        /// <summary> Decode a target amount(targetDecodeFrameNum) of GIF frames(skip frames by an averaged interval). </summary>
        AverageInterval,

        /// <summary> Decode the first half of the GIF frames(not more than targetDecodeFrameNum if provided targetDecodeFrameNum larger than 0). </summary>
        OneHalf,

        /// <summary> Decode the first one-third of the GIF frames(not more than targetDecodeFrameNum if provided targetDecodeFrameNum larger than 0). </summary>
        OneThird,

        /// <summary> Decode the first one-fourth of the GIF frames(not more than targetDecodeFrameNum if provided targetDecodeFrameNum larger than 0). </summary>
        OneFourth
    }
    public enum Decoder
    {
        ProGif_QueuedThread = 0,
        ProGif_Coroutines,
    }

    //Advanced settings ------------------
    [Header("[ Advanced Decode Settings ]")]
    [Tooltip("If 'True', use the settings on the prefab, this will ignore changes from PGif/ProGifManager.")]
    public bool m_UsePresetSettings = false;

    public Decoder m_Decoder = Decoder.ProGif_QueuedThread;

    public DecodeMode m_DecodeMode = DecodeMode.Normal;

    public FramePickingMethod m_FramePickingMethod = FramePickingMethod.Default;

    public int m_TargetDecodeFrameNum = -1;   //if targetDecodeFrameNum <= 0: decode & play all frames (+/- 1 frame)

    [Tooltip("Set to 'true' to take advantage of the highly optimized ProGif playback solution for significantly save the memory usage.")]
    public bool m_OptimizeMemoryUsage = true;
    //Advanced settings ------------------

    /// <summary> Resets the decode settings(Set the decodeMode as Normal, simply decodes the entire gif without applying advanced settings) </summary>
    public void ResetDecodeSettings()
    {
        if (m_UsePresetSettings)
        {
#if UNITY_EDITOR
            if (m_DebugLog) Debug.Log("UsePresetSettings is selected, the decoder will use the settings on the prefab and ignore changes from PGif/ProGifManager.");
#endif
            return;
        }
        m_Decoder = Decoder.ProGif_QueuedThread;
        m_DecodeMode = DecodeMode.Normal;
        m_FramePickingMethod = FramePickingMethod.Default;
        m_TargetDecodeFrameNum = -1;
        m_OptimizeMemoryUsage = true;
    }

    /// <summary> Sets the decodeMode as Advanced, apply the advanced settings(targetDecodeFrameNum, framePickingMethod..) </summary>
    public void SetAdvancedDecodeSettings(Decoder decoder, int targetDecodeFrameNum = -1, FramePickingMethod framePickingMethod = FramePickingMethod.Default, bool optimizeMemoryUsage = true)
    {
        if (m_UsePresetSettings)
        {
#if UNITY_EDITOR
            if (m_DebugLog) Debug.Log("UsePresetSettings is selected, the decoder will use the settings on the prefab and ignore changes from PGif/ProGifManager.");
#endif
            return;
        }
        this.m_Decoder = decoder;
        this.m_DecodeMode = DecodeMode.Advanced;
        this.m_FramePickingMethod = framePickingMethod;
        this.m_TargetDecodeFrameNum = targetDecodeFrameNum;
        this.m_OptimizeMemoryUsage = optimizeMemoryUsage;
    }

    /// <summary> Indicates if loading file from local or Web. </summary>
    private bool _loadingFile = false;
    void OnEnable()
    {
        if (!_loadingFile && _gifBytes == null && !string.IsNullOrEmpty(m_LoadPath))
        {
            Play(m_LoadPath, m_ShouldSaveFromWeb);
        }
    }

    private byte[] _gifBytes = null;
    public byte[] GetBytes()
    {
        return _gifBytes;
    }

    public void SetBytes(byte[] bytes, bool play = false)
    {
        _gifBytes = bytes;
        if (play) PlayWithLoadedBytes(false);
    }

    public void ClearBytes()
    {
        _gifBytes = null;
    }

    public void PlayWithLoadedBytes(bool clearCallbacks = false)
    {
        if (_gifBytes == null) return;
        m_ShouldSaveFromWeb = false;
        Clear(false, clearCallbacks);
        m_GifTextures = new List<GifTexture>();
        _PlayWithBytes(_gifBytes);
    }

    public void Play(string loadPath, bool shouldSaveFromWeb)
    {
        this.m_ShouldSaveFromWeb = shouldSaveFromWeb;
        Clear();
        m_GifTextures = new List<GifTexture>();
        LoadGifFromUrl(loadPath);
        this.m_LoadPath = loadPath;
    }

    /// <summary> Setup to play the stored textures from gif recorder. </summary>
    public virtual void Play(RenderTexture[] gifFrames, float fps, bool isCustomRatio, int customWidth, int customHeight, bool optimizeMemoryUsage)
    {
        m_GifTextures = new List<GifTexture>();
        
        this.m_OptimizeMemoryUsage = optimizeMemoryUsage;

        _isDecoderSource = false;

        _interval = 1.0f / fps;

        m_GifInfo = new ProGifDecoder.GifInfo()
        {
            m_FPS = fps,
        };

        Clear();

        m_TotalFrame = gifFrames.Length;

        StartCoroutine(_AddGifTextures(gifFrames, fps, isCustomRatio, customWidth, customHeight, optimizeMemoryUsage, 0, yieldPerFrame: true));

        StartCoroutine(_DelayCallback());

        State = PlayerState.Playing;
    }

    private IEnumerator _AddGifTextures(RenderTexture[] gifFrames, float fps, bool isCustomRatio, int customWidth, int customHeight, bool optimizeMemory, int currentIndex, bool yieldPerFrame)
    {
        int i = currentIndex;

        if (isCustomRatio)
        {
            width = customWidth;
            height = customHeight;
            Texture2D tex = new Texture2D(width, height);
            RenderTexture.active = gifFrames[i];
            tex.ReadPixels(new Rect((gifFrames[i].width - tex.width) / 2, (gifFrames[i].height - tex.height) / 2, tex.width, tex.height), 0, 0);
            tex.Apply();
            m_GifTextures.Add(new GifTexture(tex, _interval, optimizeMemory));
        }
        else
        {
            width = gifFrames[0].width;
            height = gifFrames[0].height;
            Texture2D tex = new Texture2D(gifFrames[i].width, gifFrames[i].height);
            RenderTexture.active = gifFrames[i];
            tex.ReadPixels(new Rect(0.0f, 0.0f, gifFrames[i].width, gifFrames[i].height), 0, 0);
            tex.Apply();
            m_GifTextures.Add(new GifTexture(tex, _interval, optimizeMemory));
        }

        if (currentIndex == 1) OnLoading(LoadingProgress);

        if (yieldPerFrame) yield return new WaitForEndOfFrame();

        if (OnLoading != null) OnLoading(LoadingProgress);

        currentIndex++;

        if (currentIndex < gifFrames.Length)
        {
            StartCoroutine(_AddGifTextures(gifFrames, fps, isCustomRatio, customWidth, customHeight, optimizeMemory, currentIndex, yieldPerFrame));
        }
        else
        {
            // Texture import finished
        }
    }

    private IEnumerator _DelayCallback()
    {
        yield return new WaitForEndOfFrame();
        _OnFrameReady(m_GifTextures[0], true);
        if (m_GifTextures != null && m_GifTextures.Count > 0) _OnFirstFrameReady(m_GifTextures[0]);
    }

    public void Pause()
    {
        State = PlayerState.Pause;
    }

    public void Resume()
    {
        State = PlayerState.Playing;
    }

    public void Stop()
    {
        State = PlayerState.Pause;
        m_SpriteIndex = 0;
    }

    /// <summary>
    /// Set GIF texture from url
    /// </summary>
    /// <param name="url">GIF image url (Web link or local path)</param>
    public void LoadGifFromUrl(string url)
    {
        if (string.IsNullOrEmpty(url)) return;
        StartCoroutine(LoadGifRoutine(url, (gifBytes) =>
        {
            _PlayWithBytes(gifBytes);
        }));
    }

    public IEnumerator LoadGifRoutine(string url, Action<byte[]> onLoaded)
    {
        if (string.IsNullOrEmpty(url))
        {
#if UNITY_EDITOR
            Debug.LogError("URL is nothing.");
#endif
            yield break;
        }

        if (State == PlayerState.Loading)
        {
#if UNITY_EDITOR
            if (m_DebugLog) Debug.LogWarning("Already loading.");
#endif
            yield break;
        }
        State = PlayerState.Loading;

        FilePathName filePathName = FilePathName.Instance;

        bool isFromWeb = false;
        string path = url;
        if (path.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            // from WEB
            isFromWeb = true;
        }
        else if (path.StartsWith("/idbfs/", StringComparison.OrdinalIgnoreCase))
        {
            // from WebGL index DB
            _gifBytes = filePathName.ReadFileToBytes(path);
        }
        else
        {
            // from Local
            path = filePathName.EnsureLocalPath(path);

#if UNITY_EDITOR
            if (m_DebugLog) Debug.Log("(ProGifPlayerComponent) Local file path: " + path);
#endif
        }

        if (_gifBytes != null)
        {
            onLoaded(_gifBytes);
        }
        else
        {
            // Load file
            _loadingFile = true;

#if UNITY_2017_3_OR_NEWER
            using (UnityWebRequest uwr = UnityWebRequest.Get(path))
            {
                uwr.SendWebRequest();
                while (!uwr.isDone) yield return null;

#if UNITY_2020_1_OR_NEWER
                if (uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError || uwr.result == UnityWebRequest.Result.DataProcessingError)
#else
                if (uwr.isNetworkError || uwr.isHttpError)
#endif
                {
#if UNITY_EDITOR
                    Debug.LogError("File load error.\n" + uwr.error);
#endif
                    State = PlayerState.None;
                    yield break;
                }
                else
                {
                    _gifBytes = uwr.downloadHandler.data;

                    onLoaded(_gifBytes);

                    //Save bytes to gif file if it is downloaded from web
                    if (isFromWeb && m_ShouldSaveFromWeb)
                    {
                        string cachePath = System.IO.Path.Combine(CacheDirectory, filePathName.GetGifFileName() + ".gif");
                        filePathName.FileStreamTo(cachePath, _gifBytes);
                    }
                }
            }
#else
            using (WWW www = new WWW(path))
            {
                yield return www;
                if (string.IsNullOrEmpty(www.error) == false)
                {
#if UNITY_EDITOR
                    Debug.LogError("File load error.\n" + www.error);
#endif
                    State = PlayerState.None;
                    yield break;
                }
                _loadingFile = false;

                State = PlayerState.Loading; // PlayerState.Loading = Decoding

                _gifBytes = www.bytes;

                onLoaded(_gifBytes);

                //Save bytes to gif file if it is downloaded from web
                if (isFromWeb && shouldSaveFromWeb)
                {
                    filePathName.FileStreamTo(filePathName.GetDownloadedGifSaveFullPath(), _gifBytes);
                }
            }
#endif
        }
    }

    private ProGifDecoder _proGifDecoder;
    private void _PlayWithBytes(byte[] gifBytes)
    {
        isFirstFrame = true;

#if UNITY_EDITOR
        if (m_DebugLog) Debug.Log((m_Decoder == Decoder.ProGif_QueuedThread ? "Decode process run in Threads: " : "Decode process run in Coroutines: ") + gameObject.name);
        startDecodeTime = Time.time;
#endif

        _isDecoderSource = true;

        if (m_Decoder == Decoder.ProGif_QueuedThread) // decode in worker thread
        {
            currentDecodeIndex = 0;
            decodeCompletedFlag = false;

            if (_proGifDecoder != null)
            {
                ProGifDeWorker.GetInstance().DeQueueDecoder(_proGifDecoder);
            }

            _proGifDecoder = new ProGifDecoder(_gifBytes,
                (gifTexList, loop, w, h) =>
                {
                    if (gifTexList != null)
                    {
                        this.loopCount = loop;
                        this.width = w;
                        this.height = h;
                        decodeCompletedFlag = true;
                    }
                    else
                    {
                        State = PlayerState.None;
                    }
                },
                m_FilterMode, m_WrapMode, m_DebugLog,
                (gTex) =>
                {
                    _AddGifTexture(gTex);
                },
                (gifInfo) =>
                {
                    m_GifInfo = gifInfo;
                    m_TotalFrame = gifInfo.m_TotalFrame;
                },
                OnGifErrorCallback
            );

            if (m_DecodeMode == DecodeMode.Normal) _proGifDecoder.ResetDecodeSettings();
            else _proGifDecoder.SetAdvancedDecodeSettings(m_TargetDecodeFrameNum, m_FramePickingMethod);

            _proGifDecoder.SetOptimizeMemoryUsgae(m_OptimizeMemoryUsage);

            ProGifDeWorker.GetInstance(m_WorkerPriority).QueueDecoder(_proGifDecoder);
            ProGifDeWorker.GetInstance().Start();
        }
        else // decode in coroutine
        {
            _proGifDecoder = new ProGifDecoder(_gifBytes,
                (gifTexList, loop, w, h) =>
                {
                    if (gifTexList != null)
                    {
#if UNITY_EDITOR
                        if (m_DebugLog) Debug.Log(gameObject.name + " - Total Decode Time: " + (Time.time - startDecodeTime));
#endif
                        this.loopCount = loop;
                        this.width = w;
                        this.height = h;

                        _UnlockColors(gifTexList);

                        _OnComplete();
                    }
                    else
                    {
#if UNITY_EDITOR
                        Debug.LogError("Gif texture get error.");
#endif
                        State = PlayerState.None;
                    }
                },
                m_FilterMode, m_WrapMode, m_DebugLog,
                (gTex) =>
                {
                    _AddGifTexture(gTex);
                    _OnFrameReady(gTex, isFirstFrame);
                    if (isFirstFrame) _OnFirstFrameReady(gTex);
                    if (OnLoading != null) OnLoading(LoadingProgress);

                    isFirstFrame = false;
                },
                (gifInfo) =>
                {
                    m_GifInfo = gifInfo;
                    m_TotalFrame = gifInfo.m_TotalFrame;
                },
                OnGifErrorCallback
            );

            if (m_DecodeMode == DecodeMode.Normal) _proGifDecoder.ResetDecodeSettings();
            else _proGifDecoder.SetAdvancedDecodeSettings(m_TargetDecodeFrameNum, m_FramePickingMethod);

            _proGifDecoder.SetOptimizeMemoryUsgae(m_OptimizeMemoryUsage);

            StartCoroutine(_proGifDecoder.GetTextureListCoroutine());
        }
    }

    bool decodeCompletedFlag = false;
    float startDecodeTime = 0f;
    int currentDecodeIndex = 0;
    bool isFirstFrame = true;
    /// Update for decoder using thread.
    protected void ThreadsUpdate()
    {
        if (!_isDecoderSource || m_Decoder != Decoder.ProGif_QueuedThread) return;

        if (currentDecodeIndex < m_GifTextures.Count)
        {
            currentDecodeIndex++;
            _OnFrameReady(m_GifTextures[currentDecodeIndex - 1], isFirstFrame);
        }

        if (isFirstFrame && m_GifTextures.Count > 0)
        {
            isFirstFrame = false;
            _OnFirstFrameReady(m_GifTextures[0]);
        }

        if (OnLoading != null && m_GifTextures.Count > 0) OnLoading(LoadingProgress);

        if (decodeCompletedFlag)
        {
#if UNITY_EDITOR
            if (m_DebugLog) Debug.Log(gameObject.name + " - Total Decode Time: " + (Time.time - startDecodeTime));
#endif
            decodeCompletedFlag = false;

            _UnlockColors(m_GifTextures);

            _OnComplete();
        }
    }

    // Optional to override
    protected virtual void _AddGifTexture(GifTexture gTex)
    {
        m_GifTextures.Add(gTex);
    }

    /// <summary>
    /// This is called on every gif frame decode finish
    /// </summary>
    /// <param name="gTex">GifTexture.</param>
    protected abstract void _OnFrameReady(GifTexture gTex, bool isFirstFrame);

    public void _OnFirstFrameReady(GifTexture gifTex)
    {
        State = PlayerState.Playing;
        _interval = gifTex.m_delaySec;
        width = gifTex.m_Width;
        height = gifTex.m_Height;
        if (OnFirstFrame != null)
        {
            OnFirstFrame(new FirstGifFrame()
            {
                gifTexture = gifTex,
                width = this.width,
                height = this.height,
                interval = this.interval,
                totalFrame = this.m_TotalFrame,
                loopCount = m_GifInfo.m_LoopCount,
                byteLength = m_GifInfo.m_ByteLength,
                fps = m_GifInfo.m_FPS,
                comments = m_GifInfo.m_Comments
            });
        }
    }

    private void _OnComplete()
    {
        if (OnDecodeComplete != null)
        {
            OnDecodeComplete(new DecodedResult()
            {
                gifTextures = this.m_GifTextures,
                loopCount = this.loopCount,
                width = this.width,
                height = this.height,
                interval = this.interval,
                totalFrame = this.m_TotalFrame,
            });
        }
    }

    public Action<FirstGifFrame> OnFirstFrame = null;
    public void SetOnFirstFrameCallback(Action<FirstGifFrame> onFirstFrame)
    {
        OnFirstFrame = onFirstFrame;
    }

    public class FirstGifFrame
    {
        public GifTexture gifTexture;
        public int width;
        public int height;
        public float interval;
        public int totalFrame;
        public int loopCount = -1; // -1 = no loop, 0 = loop
        public int byteLength = 0;
        public float fps = 0;

        /// <summary>
        /// A message embed in the GIF Comment-Extension. Could be an image description, image credit, or other human-readable metadata such as the GPS location of the image capture.
        /// (This is an Optional field of GIF, it could be empty)
        /// </summary>
        public string comments;
    }

    public Action<float> OnLoading = null;
    public void SetLoadingCallback(Action<float> onLoading)
    {
        OnLoading = onLoading;
    }

    public Action<DecodedResult> OnDecodeComplete = null;
    public void SetOnDecodeCompleteCallback(Action<DecodedResult> onDecodeComplete)
    {
        OnDecodeComplete = onDecodeComplete;
    }

    public class DecodedResult
    {
        public List<GifTexture> gifTextures;
        public int width;
        public int height;
        public float interval;
        public int loopCount;
        public int totalFrame;

        public float fps
        {
            get
            {
                return 1f / interval;
            }
        }
    }

    public Action<GifTexture> OnPlayingCallback = null;
    public void SetOnPlayingCallback(Action<GifTexture> onPlayingCallback)
    {
        OnPlayingCallback = onPlayingCallback;
    }

    public Action<string> OnGifErrorCallback = null;
    public void SetOnGifErrorCallback(Action<string> onGifErrorCallback)
    {
        OnGifErrorCallback = onGifErrorCallback;
    }

    /// <summary>
    /// Sets the flag to clear the colors in the GifTexture list
    /// </summary>
    protected void _UnlockColors(List<GifTexture> gifTexList)
    {
        if (gifTexList != null)
        {
            for (int i = 0; i < gifTexList.Count; i++)
            {
                if (gifTexList[i] != null)
                {
                    gifTexList[i].m_LockColorData = false;
                }
            }
        }
    }

    /// <summary>
    /// Clear the sprite, texture2D and colors in the GifTexture list
    /// </summary>
    protected void _ClearGifTextures(List<GifTexture> gifTexList)
    {
        if (gifTexList != null)
        {
            for (int i = 0; i < gifTexList.Count; i++)
            {
                if (gifTexList[i] != null)
                {
                    gifTexList[i].m_Colors = null;

                    if (gifTexList[i].m_texture2d != null)
                    {
                        Destroy(gifTexList[i].m_texture2d);
                        gifTexList[i].m_texture2d = null;
                    }

                    if (gifTexList[i].m_Sprite != null && gifTexList[i].m_Sprite.texture != null)
                    {
                        Destroy(gifTexList[i].m_Sprite.texture);
                        Destroy(gifTexList[i].m_Sprite);
                        gifTexList[i].m_Sprite = null;
                    }
                }
            }
        }
    }

    private void OnDestroy()
    {
        Clear();
    }

    public virtual void Clear(bool clearBytes = true, bool clearCallbacks = true)
    {
        State = PlayerState.None;
        m_SpriteIndex = 0;
        _nextFrameTime = 0f;

        // Clear callbacks
        if (clearCallbacks)
        {
            OnLoading = null;
            OnFirstFrame = null;
            OnDecodeComplete = null;
            OnPlayingCallback = null;
            OnGifErrorCallback = null;
        }

        StopAllCoroutines();

        if (clearBytes)
        {
            ClearBytes();
        }

        //Clear gifTextures in loading coroutines/threads
        if (_proGifDecoder != null)
        {
            ProGifDeWorker.GetInstance().DeQueueDecoder(_proGifDecoder);
        }

        //Clear gifTextures of the PlayerComponent
        _ClearGifTextures(m_GifTextures);
    }

    
    //-- Resize --------
    //private int newFps = -1;
    //private Vector2 newSize = Vector2.zero;
    //private bool keepRatioForNewSize = true;
    //public void Resize_AdvancedMode(GifTexture gTex)
    //{
    //    ImageResizer imageResizer = null;
    //    bool reSize = false;
    //    if (newSize.x > 0 && newSize.y > 0 && decodeMode == ProGifPlayerComponent.DecodeMode.Advanced)
    //    {
    //        imageResizer = new ImageResizer();
    //        reSize = true;
    //    }

    //    if (reSize) gTex.m_texture2d = (keepRatioForNewSize) ?
    //             imageResizer.ResizeTexture32_KeepRatio(gTex.m_texture2d, (int)newSize.x, (int)newSize.y) :
    //             imageResizer.ResizeTexture32(gTex.m_texture2d, (int)newSize.x, (int)newSize.y);
    //}
    //-- Resize ----------------

}
