﻿// Created by SWAN DEV 2017

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using SDev.GiphyAPI;

public class ProGifGiphyDemo : MonoBehaviour
{
    public Dropdown m_Dropdown;
    public Toggle m_ToggleDecodeOption;
    public InputField m_InputField;
    public InputField m_InputField_JsonText;
    public InputField m_InputFieldKeyWord;
    public InputField m_InputFieldResultOffset;

    [Header("[ Scroll View Layout ]")]
    public GridLayoutGroup m_GridLayoutGroup;
    public DImageDisplayHandler m_ImageDisplayHandler;
    public RectTransform m_ImageContainer;
    public Image[] m_PlayerImages;

    [Header("[ Max Size Viewer ]")]
    public DImageDisplayHandler m_ImageDisplayHandler_Full;
    public GameObject m_FullsizeViewerContainer;
    private Image _fullViewerImage;
    private ProGifPlayerImage _player;


    #region ----- GIF Player Settings -----
    [Header("[ GIF Player Decode Settings ]")]
    public ProGifPlayerComponent.Decoder m_DecoderOption = ProGifPlayerComponent.Decoder.ProGif_Coroutines;
    public ProGifPlayerComponent.DecodeMode m_DecodeMode = ProGifPlayerComponent.DecodeMode.Normal;
    public ProGifPlayerComponent.FramePickingMethod m_FramePickingMethod = ProGifPlayerComponent.FramePickingMethod.Default;
    [Range(-1, 9999)] public int m_TargetDecodeFrameNum = -1;       //if targetDecodeFrameNum <= 0: decode & play all frames
    public bool m_OptimizeMemoryUsage = true;
    public bool m_HighQualityPreview = true;
    public bool m_TryDecodeBrokenGif;
    private bool _hasMultiThreadMode;
    #endregion


    private string _shareTitle = "This is a Title";
    private string _shareText = "This is a Message";
    private string _shareGifId = "";            //The Giphy unique gif id

    //Reminds: some social platforms support short link preview, while some support preview of full url with .gif extension
    private string _shareGifBitlyUrl = "";      //The bitly gif url
    private string _shareGifFullUrl = "";       //Url with .gif extension

    void Start()
    {
        string[] names = System.Enum.GetNames(typeof(ProGifPlayerComponent.Decoder));
        foreach (string n in names)
        {
            if (n == "ProGif_MultiThreaded") _hasMultiThreadMode = true;
        }

        if (m_ImageContainer)
        {
            m_PlayerImages = m_ImageContainer.GetComponentsInChildren<Image>();
        }

        if (m_ImageDisplayHandler_Full)
        {
            _fullViewerImage = m_ImageDisplayHandler_Full.gameObject.GetComponent<Image>();
        }

        if (m_FullsizeViewerContainer)
        {
            m_FullsizeViewerContainer.gameObject.SetActive(false);

            _fullViewerImage.gameObject.AddComponent<Button>().onClick.AddListener(() =>
            {
                m_FullsizeViewerContainer.gameObject.SetActive(false);
                if (_player) _player.RemoveFromExtraDestination(_fullViewerImage);
            });

            foreach (Image image in m_PlayerImages)
            {
                Button btn = image.gameObject.AddComponent<Button>();
                btn.onClick.AddListener(() =>
                {
                    _player = image.gameObject.GetComponent<ProGifPlayerImage>();
                    if (_player && _player.State == ProGifPlayerComponent.PlayerState.Playing)
                    {
                        m_FullsizeViewerContainer.gameObject.SetActive(true);
                        _player.AddExtraDestination(_fullViewerImage);
                        m_ImageDisplayHandler_Full.SetImage(_fullViewerImage, _player.width, _player.height);
                    }
                });
            }
        }

        if (m_ImageContainer)
        {
            m_ImageContainer.sizeDelta = new Vector2(m_ImageContainer.sizeDelta.x, m_PlayerImages.Length / 3 * 330 + 30);
        }

        if (m_InputFieldResultOffset)
        {
            m_InputFieldResultOffset.contentType = InputField.ContentType.IntegerNumber;
            m_InputFieldResultOffset.onValueChanged.AddListener((inputText) =>
            {
                int offset = string.IsNullOrEmpty(inputText) ? 0 : int.Parse(inputText);
                GiphyManager.Instance.m_ResultOffset = Mathf.Max(0, offset);
            });
        }
    }

    public void OnButtonShare()
    {
        Debug.Log("OnButtonShare - BitlyUrl: " + _shareGifBitlyUrl + " | Id: " + _shareGifId + " | FullUrl: " + _shareGifFullUrl);
        OnButtonFB();
        OnButtonTwitter_Mobile();
        OnButtonTumblr();
        OnButtonSkype();
    }

    public void OnButtonFB()
    {
        Debug.Log("OnButtonFB: " + _shareGifBitlyUrl);
        GifSocialShare gifShare = new GifSocialShare();
        gifShare.ShareTo(GifSocialShare.Social.Facebook, _shareTitle, _shareText, _shareGifBitlyUrl, _shareGifBitlyUrl);
    }

    public void OnButtonTwitter()
    {
        Debug.Log("OnButtonTwitter: " + _shareGifId);
        GifSocialShare gifShare = new GifSocialShare();
        gifShare.ShareTo(GifSocialShare.Social.Twitter, "hashtag", _shareText, _shareGifId, "https://www.swanob2.com");
    }

    public void OnButtonTwitter_Mobile()
    {
        Debug.Log("OnButtonTwitter_Mobile: " + _shareGifId);
        GifSocialShare gifShare = new GifSocialShare();
        //Short link(BitlyUrl) is better supported by Twitter
        gifShare.ShareTo(GifSocialShare.Social.Twitter_Mobile, "hashtag", _shareText, "https://www.swanob2.com", _shareGifBitlyUrl);
    }

    public void OnButtonTumblr()
    {
        Debug.Log("OnButtonTumblr: " + _shareGifFullUrl);
        GifSocialShare gifShare = new GifSocialShare();
        gifShare.ShareTo(GifSocialShare.Social.Tumblr, _shareTitle, _shareText, _shareGifFullUrl, _shareGifFullUrl);
    }

    public void OnButtonVK()
    {
        Debug.Log("OnButtonVK: " + _shareGifBitlyUrl);
        GifSocialShare gifShare = new GifSocialShare();
        gifShare.ShareTo(GifSocialShare.Social.VK, _shareTitle, _shareText, _shareGifBitlyUrl, _shareGifBitlyUrl);
    }

    public void OnButtonPinterest()
    {
        Debug.Log("OnButtonPinterest: " + _shareGifFullUrl);
        GifSocialShare gifShare = new GifSocialShare();
        gifShare.ShareTo(GifSocialShare.Social.Pinterest, _shareTitle, _shareText, _shareGifFullUrl, _shareGifFullUrl);
    }

    public void OnButtonLinkedIn()
    {
        Debug.Log("OnButtonPLinkedIn: " + _shareGifFullUrl);
        GifSocialShare gifShare = new GifSocialShare();
        gifShare.ShareTo(GifSocialShare.Social.LinkedIn, _shareTitle, _shareText, _shareGifFullUrl, _shareGifFullUrl);
    }

    public void OnButtonOKRU()
    {
        Debug.Log("OnButtonOKRU: " + _shareGifBitlyUrl);
        GifSocialShare gifShare = new GifSocialShare();
        gifShare.ShareTo(GifSocialShare.Social.Odnoklassniki, _shareTitle, _shareText, _shareGifBitlyUrl, _shareGifBitlyUrl);
    }

    public void OnButtonReddit()
    {
        Debug.Log("OnButtonReddit: " + _shareGifBitlyUrl);
        GifSocialShare gifShare = new GifSocialShare();
        gifShare.ShareTo(GifSocialShare.Social.Reddit, _shareTitle, _shareText, _shareGifBitlyUrl, _shareGifBitlyUrl);
    }

    public void OnButtonGooglePlus()
    {
        Debug.Log("OnButtonGooglePlus: " + _shareGifBitlyUrl);
        GifSocialShare gifShare = new GifSocialShare();
        gifShare.ShareTo(GifSocialShare.Social.GooglePlus, _shareTitle, _shareText, _shareGifBitlyUrl, _shareGifBitlyUrl);
    }

    public void OnButtonQQ()
    {
        Debug.Log("OnButtonQQ: " + _shareGifFullUrl);
        GifSocialShare gifShare = new GifSocialShare();
        gifShare.ShareTo(GifSocialShare.Social.QQZone, _shareTitle, _shareText, _shareGifFullUrl, _shareGifFullUrl);
    }

    public void OnButtonWeibo()
    {
        Debug.Log("OnButtonWeibo: " + _shareGifBitlyUrl);
        GifSocialShare gifShare = new GifSocialShare();
        gifShare.ShareTo(GifSocialShare.Social.Weibo, _shareTitle, _shareText, _shareGifBitlyUrl, _shareGifBitlyUrl);
    }

    public void OnButtonMySpace()
    {
        Debug.Log("OnButtonMySpace: " + _shareGifFullUrl);
        GifSocialShare gifShare = new GifSocialShare();
        gifShare.ShareTo(GifSocialShare.Social.MySpace, _shareTitle, _shareText, _shareGifFullUrl, _shareGifFullUrl);
    }

    public void OnButtonLineMe()
    {
        Debug.Log("OnButtonLineMe: " + _shareGifBitlyUrl);
        GifSocialShare gifShare = new GifSocialShare();
        gifShare.ShareTo(GifSocialShare.Social.LineMe, _shareTitle, _shareText, _shareGifBitlyUrl, _shareGifBitlyUrl);
    }

    public void OnButtonSkype()
    {
        Debug.Log("OnButtonSkype: " + _shareGifFullUrl);
        GifSocialShare gifShare = new GifSocialShare();
        gifShare.ShareTo(GifSocialShare.Social.Skype, _shareTitle, _shareText, _shareGifFullUrl, _shareGifFullUrl);
    }

    public void OnButtonBaidu()
    {
        Debug.Log("OnButtonBaidu: " + _shareGifBitlyUrl);
        GifSocialShare gifShare = new GifSocialShare();
        gifShare.ShareTo(GifSocialShare.Social.Baidu, _shareTitle, _shareText, _shareGifBitlyUrl, _shareGifBitlyUrl);
    }

    //-------------------------------------------------
    public void OnButtonSend()
    {
        Debug.Log("OnButtonSend: " + m_Dropdown.options[m_Dropdown.value].text);
        _GiphyApi(m_Dropdown.options[m_Dropdown.value].text);
    }

    private string _SetDisplayString(string text)
    {
        //return "Response received!";
        return (text.Length > 10000) ? text.Substring(0, 10000) : text;
    }

    private void _GiphyApi(string text)
    {
        string tempStr = "";

        m_InputField_JsonText.text = _SetDisplayString(GiphyManager.Instance.FullJsonResponseText);

        //------ GIF API -------
        if (text == "Search")
        {
            if (string.IsNullOrEmpty(m_InputFieldKeyWord.text))
            {
                Debug.LogWarning("_GiphyApi > Search, KeyWord is Empty!");
            }
            else
            {
                Debug.Log("_GiphyApi > Search");
                GiphyManager.Instance.Search(new List<string> { m_InputFieldKeyWord.text }, (result) => {
                    if (result == null || result.data.Count <= 0)
                    {
                        tempStr = "1. Search - All GIF URLs (KeyWord = " + m_InputFieldKeyWord.text + "): \nNo matched result!";
                        m_InputField_JsonText.text = tempStr;
                    }
                    else
                    {
                        tempStr = "1. Search - All GIF URLs (KeyWord = " + m_InputFieldKeyWord.text + "): ";
                        for (int i = 0; i < result.data.Count; i++)
                        {
                            tempStr += "\n(" + i + ") " + result.data[i].bitly_gif_url;
                        }
                        _shareGifBitlyUrl = result.data[0].bitly_gif_url;
                        _shareGifFullUrl = result.data[0].images.original.url;
                        _shareGifId = result.data[0].id;

                        tempStr += "\n\nGif Id: " + _shareGifId
                            + "\nbitly_gif_url: " + result.data[0].bitly_gif_url
                            + "\nbitly_url: " + result.data[0].bitly_url
                            + "\ncontent_url: " + result.data[0].content_url
                            + "\nembed_url: " + result.data[0].embed_url
                            + "\nurl: " + result.data[0].url
                            + "\nimages.original.url: " + result.data[0].images.original.url
                            + "\nimages.original.mp4: " + result.data[0].images.original.mp4
                            + "\nimages.original.webp: " + result.data[0].images.original.webp
                            + "\nimages.original_mp4.mp4: " + result.data[0].images.original_mp4.mp4;
                        Debug.Log(tempStr);
                        m_InputField.text = tempStr;
                        m_InputField_JsonText.text = _SetDisplayString(GiphyManager.Instance.FullJsonResponseText);


                        // Display gif (ProGIF Required)
                        if (m_PlayerImages != null)
                        {
                            for (int i = 0; i < m_PlayerImages.Length; i++)
                            {
                                ClearGifPlayer(m_PlayerImages[i]);
                                if (result.data.Count >= i + 1)
                                {
                                    //Smaller size animated gifs preview
                                    PlayGif(result.data[i].images.fixed_width_small.url, result.data[i].images.fixed_width.url, m_PlayerImages[i]);
                                }
                            }
                        }
                    }

                });
            }
        }

        if (text == "GetByIds")
        {
            Debug.Log("_GiphyApi > GetByIds");
            GiphyManager.Instance.GetByIds(new List<string> { "ZXKZWB13D6gFO", "3oEdva9BUHPIs2SkGk", "u23zXEvNsIbfO" }, (result) => {
                if (result == null || result.data.Count <= 0)
                {
                    tempStr = "2. GetByIds - (Ids hard-coded for demo = " + "ZXKZWB13D6gFO, 3oEdva9BUHPIs2SkGk, u23zXEvNsIbfO): \nNo matched result!";
                    m_InputField_JsonText.text = tempStr;
                }
                else
                {
                    tempStr = "2. GetByIds - (Ids hard-coded for demo = " + "ZXKZWB13D6gFO, 3oEdva9BUHPIs2SkGk, u23zXEvNsIbfO): ";
                    for (int i = 0; i < result.data.Count; i++)
                    {
                        tempStr += "\n(" + i + ") " + result.data[i].bitly_gif_url;
                    }
                    _shareGifBitlyUrl = result.data[0].bitly_gif_url;
                    _shareGifFullUrl = result.data[0].images.original.url;
                    _shareGifId = result.data[0].id;

                    tempStr += "\n\n_shareGifBitlyUrl: " + _shareGifBitlyUrl + "\n_shareGifFullUrl: " + _shareGifFullUrl + "\n_shareGifId: " + _shareGifId;
                    Debug.Log(tempStr);
                    m_InputField.text = tempStr;
                    m_InputField_JsonText.text = _SetDisplayString(GiphyManager.Instance.FullJsonResponseText);


                    // Display gif (ProGIF Required)
                    if (m_PlayerImages != null)
                    {
                        for (int i = 0; i < m_PlayerImages.Length; i++)
                        {
                            ClearGifPlayer(m_PlayerImages[i]);
                            if (result.data.Count >= i + 1)
                            {
                                //Smaller size animated gifs preview 
                                PlayGif(result.data[i].images.fixed_width_small.url, result.data[i].images.fixed_width.url, m_PlayerImages[i]);
                            }
                        }
                    }
                }
            });
        }

        if (text == "Random")
        {
            Debug.Log("_GiphyApi > Random");
            GiphyManager.Instance.Random((result) => {
                if (result == null || result.data == null)
                {
                    tempStr = "3. Random result: \nNo matched result!";
                    m_InputField_JsonText.text = tempStr;
                }
                else
                {
                    tempStr = "3. Random result: ";
                    _shareGifBitlyUrl = result.data.image_url;
                    _shareGifFullUrl = result.data.image_original_url;
                    _shareGifId = result.data.id;

                    tempStr += "\n\n_shareGifBitlyUrl: " + _shareGifBitlyUrl + "\n_shareGifFullUrl: " + _shareGifFullUrl + "\n_shareGifId: " + _shareGifId;
                    Debug.Log(tempStr);
                    m_InputField.text = tempStr;
                    m_InputField_JsonText.text = _SetDisplayString(GiphyManager.Instance.FullJsonResponseText);


                    // Display gif (ProGIF Required)
                    if (m_PlayerImages != null)
                    {
                        for (int i = 0; i < m_PlayerImages.Length; i++)
                        {
                            ClearGifPlayer(m_PlayerImages[i]);

                            //Smaller size animated gifs preview 
                            if (i == 0) PlayGif(result.data.fixed_width_small_url, result.data.image_url, m_PlayerImages[i]);
                        }
                    }
                }
            });
        }

        if (text == "Translate")
        {
            if (string.IsNullOrEmpty(m_InputFieldKeyWord.text))
            {
                Debug.LogWarning("_GiphyApi > Translate, KeyWord is Empty!");
            }
            else
            {
                Debug.Log("_GiphyApi > Translate");
                GiphyManager.Instance.Translate(m_InputFieldKeyWord.text, 3, (result) => {
                    if (result == null || result.data == null)
                    {
                        tempStr = "4. Translate result (KeyWord = " + m_InputFieldKeyWord.text + "): \nNo matched result!";
                        m_InputField_JsonText.text = tempStr;
                    }
                    else
                    {
                        tempStr = "4. Translate result (KeyWord = " + m_InputFieldKeyWord.text + "): ";
                        _shareGifBitlyUrl = result.data.bitly_gif_url;
                        _shareGifFullUrl = result.data.images.original.url;
                        _shareGifId = result.data.id;

                        tempStr += "\n\n_shareGifBitlyUrl: " + _shareGifBitlyUrl + "\n_shareGifFullUrl: " + _shareGifFullUrl + "\n_shareGifId: " + _shareGifId;
                        Debug.Log(tempStr);
                        m_InputField.text = tempStr;
                        m_InputField_JsonText.text = _SetDisplayString(GiphyManager.Instance.FullJsonResponseText);


                        // Display gif (ProGIF Required)
                        if (m_PlayerImages != null)
                        {
                            for (int i = 0; i < m_PlayerImages.Length; i++)
                            {
                                //Smaller size animated gifs preview 
                                ClearGifPlayer(m_PlayerImages[i]);
                                if (i == 0) PlayGif(result.data.images.fixed_width_small.url, result.data.images.fixed_width.url, m_PlayerImages[i]);
                            }
                        }
                    }
                });
            }
        }


        if (text == "Trending")
        {
            Debug.Log("_GiphyApi > Trending");
            GiphyManager.Instance.Trending((result) => {
                if (result == null || result.data.Count <= 0)
                {
                    tempStr = "5. Trending - All GIF URLs: \nNo matched result!";
                    m_InputField_JsonText.text = tempStr;
                }
                else
                {
                    tempStr = "5. Trending - All GIF URLs: ";
                    for (int i = 0; i < result.data.Count; i++)
                    {
                        tempStr += "\n(" + i + ") " + result.data[i].bitly_gif_url;
                    }
                    _shareGifBitlyUrl = result.data[0].bitly_gif_url;
                    _shareGifFullUrl = result.data[0].images.original.url;
                    _shareGifId = result.data[0].id;

                    tempStr += "\n\n_shareGifBitlyUrl: " + _shareGifBitlyUrl + "\n_shareGifFullUrl: " + _shareGifFullUrl + "\n_shareGifId: " + _shareGifId;
                    Debug.Log(tempStr);
                    m_InputField.text = tempStr;
                    m_InputField_JsonText.text = _SetDisplayString(GiphyManager.Instance.FullJsonResponseText);


                    // Display gif (ProGIF Required)
                    if (m_PlayerImages != null)
                    {
                        for (int i = 0; i < m_PlayerImages.Length; i++)
                        {
                            ClearGifPlayer(m_PlayerImages[i]);
                            if (result.data.Count >= i + 1)
                            {
                                //Smaller size animated gifs preview
                                PlayGif(result.data[i].images.fixed_width_small.url, result.data[i].images.fixed_width.url, m_PlayerImages[i]);
                            }
                        }
                    }
                }
            });
        }


        //------ Stickers API -------
        if (text == "Search Sticker")
        {
            if (string.IsNullOrEmpty(m_InputFieldKeyWord.text))
            {
                Debug.LogWarning("_GiphyApi > Search Sticker, KeyWord is Empty!");
            }
            else
            {
                Debug.Log("_GiphyApi > Search Sticker");
                GiphyManager.Instance.Search_Sticker(new List<string> { m_InputFieldKeyWord.text }, (result) => {
                    if (result == null || result.data.Count <= 0)
                    {
                        tempStr = "6. Search_Sticker - All GIF URLs (KeyWord = " + m_InputFieldKeyWord.text + "): \nNo matched result!";
                        m_InputField_JsonText.text = tempStr;
                    }
                    else
                    {
                        tempStr = "6. Search_Sticker - All GIF URLs (KeyWord = " + m_InputFieldKeyWord.text + "): ";
                        for (int i = 0; i < result.data.Count; i++)
                        {
                            tempStr += "\n(" + i + ") " + result.data[i].bitly_gif_url;
                        }
                        _shareGifBitlyUrl = result.data[0].bitly_gif_url;
                        _shareGifFullUrl = result.data[0].images.original.url;
                        _shareGifId = result.data[0].id;

                        tempStr += "\n\n_shareGifBitlyUrl: " + _shareGifBitlyUrl + "\n_shareGifFullUrl: " + _shareGifFullUrl + "\n_shareGifId: " + _shareGifId;
                        Debug.Log(tempStr);
                        m_InputField.text = tempStr;
                        m_InputField_JsonText.text = _SetDisplayString(GiphyManager.Instance.FullJsonResponseText);


                        // Display gif (ProGIF Required)
                        if (m_PlayerImages != null)
                        {
                            for (int i = 0; i < m_PlayerImages.Length; i++)
                            {
                                ClearGifPlayer(m_PlayerImages[i]);
                                if (result.data.Count >= i + 1)
                                {
                                    //Smaller size animated gifs preview
                                    PlayGif(result.data[i].images.fixed_width_small.url, result.data[i].images.fixed_width.url, m_PlayerImages[i]);
                                }
                            }
                        }
                    }
                });
            }
        }

        if (text == "Trending Sticker")
        {
            Debug.Log("_GiphyApi > Trending Sticker");
            GiphyManager.Instance.Trending_Sticker((result) => {
                if (result == null || result.data.Count <= 0)
                {
                    tempStr = "7. Trending_Sticker - All GIF URLs: \nNo matched result!";
                    m_InputField_JsonText.text = tempStr;
                }
                else
                {
                    tempStr = "7. Trending_Sticker - All GIF URLs: ";
                    for (int i = 0; i < result.data.Count; i++)
                    {
                        tempStr += "\n(" + i + ") " + result.data[i].bitly_gif_url;
                    }
                    _shareGifBitlyUrl = result.data[0].bitly_gif_url;
                    _shareGifFullUrl = result.data[0].images.original.url;
                    _shareGifId = result.data[0].id;

                    tempStr += "\n\n_shareGifBitlyUrl: " + _shareGifBitlyUrl + "\n_shareGifFullUrl: " + _shareGifFullUrl + "\n_shareGifId: " + _shareGifId;
                    Debug.Log(tempStr);
                    m_InputField.text = tempStr;
                    m_InputField_JsonText.text = _SetDisplayString(GiphyManager.Instance.FullJsonResponseText);


                    // Display gif (ProGIF Required)
                    if (m_PlayerImages != null)
                    {
                        for (int i = 0; i < m_PlayerImages.Length; i++)
                        {
                            ClearGifPlayer(m_PlayerImages[i]);
                            if (result.data.Count >= i + 1)
                            {
                                //Smaller size animated gifs preview
                                PlayGif(result.data[i].images.fixed_width_small.url, result.data[i].images.fixed_width.url, m_PlayerImages[i]);
                            }
                        }
                    }
                }
            });
        }

        if (text == "Random Sticker")
        {
            Debug.Log("_GiphyApi > Random Sticker");
            GiphyManager.Instance.Random_Sticker((result) => {
                if (result == null || result.data == null)
                {
                    tempStr = "8. Random_Sticker result: \nNo matched result!";
                    m_InputField_JsonText.text = tempStr;
                }
                else
                {
                    tempStr = "8. Random_Sticker result: ";
                    _shareGifBitlyUrl = result.data.image_url;
                    _shareGifFullUrl = result.data.image_original_url;
                    _shareGifId = result.data.id;

                    tempStr += "\n\n_shareGifBitlyUrl: " + _shareGifBitlyUrl + "\n_shareGifFullUrl: " + _shareGifFullUrl + "\n_shareGifId: " + _shareGifId;
                    Debug.Log(tempStr);
                    m_InputField.text = tempStr;
                    m_InputField_JsonText.text = _SetDisplayString(GiphyManager.Instance.FullJsonResponseText);


                    // Display gif (ProGIF Required)
                    if (m_PlayerImages != null)
                    {
                        for (int i = 0; i < m_PlayerImages.Length; i++)
                        {
                            //Smaller size animated gifs preview 
                            ClearGifPlayer(m_PlayerImages[i]);
                            if (i == 0) PlayGif(result.data.fixed_width_small_url, result.data.image_url, m_PlayerImages[i]);
                        }
                    }
                }
            });
        }

        if (text == "Random Sticker with Tag")
        {
            if (string.IsNullOrEmpty(m_InputFieldKeyWord.text))
            {
                Debug.LogWarning("_GiphyApi > Random Sticker with Tag, KeyWord is Empty!");
            }
            else
            {
                Debug.Log("_GiphyApi > Random Sticker with Tag");
                GiphyManager.Instance.Random_Sticker(m_InputFieldKeyWord.text, (result) => {
                    if (result == null || result.data == null)
                    {
                        tempStr = "9. Random_Sticker with tag result (Tag = " + m_InputFieldKeyWord.text + "): \nNo matched result!";
                        m_InputField_JsonText.text = tempStr;
                    }
                    else
                    {
                        tempStr = "9. Random_Sticker with tag result (Tag = " + m_InputFieldKeyWord.text + "): ";
                        _shareGifBitlyUrl = result.data.image_url;
                        _shareGifFullUrl = result.data.image_original_url;
                        _shareGifId = result.data.id;

                        tempStr += "\n\n_shareGifBitlyUrl: " + _shareGifBitlyUrl + "\n_shareGifFullUrl: " + _shareGifFullUrl + "\n_shareGifId: " + _shareGifId;
                        Debug.Log(tempStr);
                        m_InputField.text = tempStr;
                        m_InputField_JsonText.text = _SetDisplayString(GiphyManager.Instance.FullJsonResponseText);


                        // Display gif (ProGIF Required)
                        if (m_PlayerImages != null)
                        {
                            for (int i = 0; i < m_PlayerImages.Length; i++)
                            {
                                //Smaller size animated gifs preview 
                                ClearGifPlayer(m_PlayerImages[i]);
                                if (i == 0) PlayGif(result.data.fixed_width_small_url, result.data.image_url, m_PlayerImages[i]);
                            }
                        }
                    }
                });
            }
        }

        if (text == "Translate Sticker")
        {
            if (string.IsNullOrEmpty(m_InputFieldKeyWord.text))
            {
                Debug.LogWarning("_GiphyApi > Translate Sticker, KeyWord is Empty!");
            }
            else
            {
                Debug.Log("Translate Sticker");
                GiphyManager.Instance.Translate_Sticker(m_InputFieldKeyWord.text, 3, (result) => {
                    if (result == null || result.data == null)
                    {
                        tempStr = "10. Translate_Sticker result (KeyWord = " + m_InputFieldKeyWord.text + "): \nNo matched result!";
                        m_InputField_JsonText.text = tempStr;
                    }
                    else
                    {
                        tempStr = "10. Translate_Sticker result (KeyWord = " + m_InputFieldKeyWord.text + "): ";
                        _shareGifBitlyUrl = result.data.bitly_gif_url;
                        _shareGifFullUrl = result.data.images.original.url;
                        _shareGifId = result.data.id;

                        tempStr += "\n\n_shareGifBitlyUrl: " + _shareGifBitlyUrl + "\n_shareGifFullUrl: " + _shareGifFullUrl + "\n_shareGifId: " + _shareGifId;
                        Debug.Log(tempStr);
                        m_InputField.text = tempStr;
                        m_InputField_JsonText.text = _SetDisplayString(GiphyManager.Instance.FullJsonResponseText);


                        // Display gif (ProGIF Required)
                        if (m_PlayerImages != null)
                        {
                            for (int i = 0; i < m_PlayerImages.Length; i++)
                            {
                                //Smaller size animated gifs preview 
                                ClearGifPlayer(m_PlayerImages[i]);
                                if (i == 0) PlayGif(result.data.images.fixed_width_small.url, result.data.images.fixed_width.url, m_PlayerImages[i]);
                            }
                        }
                    }
                });
            }
        }

    }

    /// <summary>
    /// Upload API. For upload your local gif to your own channel on Giphy.com
    /// To call this API, you need to create an APP in Giphy Dashboard, and Request a production key with Upload API
    /// Kindly Reminded: read their instructions and terms before your request!
    /// </summary>
    /// <param name="gifFilePath">GIF file path.</param>
    public void UploadApi(string gifFilePath)
    {
        GiphyManager.Instance.Upload(gifFilePath, new List<string> { "GifTagTest", "SwanOB2", "AssetPackage" },
            (uploadResult) => {
                Debug.Log("Upload response, Unique Gif Id: " + uploadResult.data.id);

                //FOR EXAMPLE:
                //Call GetById API with GIF Id to get the GIF links and other data
                GiphyManager.Instance.GetById(uploadResult.data.id, (getByIdResult) => {
                    Debug.Log("GetById response, gif short link: " + getByIdResult.data.bitly_gif_url);

                    //Your Code Here: e.g. Share GIF link on social networks like Facebook, Twitter
                    //For Facebook put the GIF link as the first link in the post for fetching GIF preview
                    //For Twitter put the GIF link as the last link in the post for fetching GIF preview

                    m_InputField_JsonText.text = _SetDisplayString(GiphyManager.Instance.FullJsonResponseText);
                });

            },
            (progress) => {
                Debug.Log("Upload Progress: " + progress);
            }
        );
    }

    // ProGIF player example, download and display gif previews 
    public void PlayGif(string lowQualityPreviewURL, string highQualityPreviewURL, Image playerImage) //, string playerName)
    {
        playerImage.gameObject.SetActive(true);

        string gifPath = m_HighQualityPreview ? highQualityPreviewURL : lowQualityPreviewURL;

#if UNITY_EDITOR
        Debug.Log("GiphyDemo - PlayGif: " + gifPath + "\nDecoderOption: " + m_DecoderOption);
#endif

        m_DecoderOption = (m_ToggleDecodeOption.isOn) ? m_DecoderOption = (ProGifPlayerComponent.Decoder)(_hasMultiThreadMode ? 2 : 0) : ProGifPlayerComponent.Decoder.ProGif_Coroutines;

        switch (m_DecodeMode)
        {
            case ProGifPlayerComponent.DecodeMode.Advanced:
                PGif.iSetAdvancedPlayerDecodeSettings(m_DecoderOption, m_TargetDecodeFrameNum, m_FramePickingMethod, m_OptimizeMemoryUsage);
                break;
            case ProGifPlayerComponent.DecodeMode.Normal:
                PGif.iResetPlayerDecodeSettings();
                break;
        }

        ProGifDecoder.TryDecodeBrokenGif = m_TryDecodeBrokenGif;

        PGif.Instance.m_Decoder = m_DecoderOption;
        PGif.Instance.m_OptimizeMemoryUsage = m_OptimizeMemoryUsage;

        PGif.iPlayGif(gifPath, playerImage, playerImage.name);

        // Sets the first frame callback and handle the UIImage size in the callback.
        PGif.iGetPlayer(playerImage.name).SetOnFirstFrameCallback((firstFrame) => {

            m_ImageDisplayHandler.SetImage(playerImage, firstFrame.width, firstFrame.height);

            m_ImageContainer.sizeDelta = new Vector2(m_ImageContainer.sizeDelta.x, m_GridLayoutGroup.preferredHeight);
        });
    }

    public void ClearGifPlayer(Image playerImage)
    {
        playerImage.gameObject.SetActive(false);
        PGif.iClearPlayer(playerImage.name);
    }

}
