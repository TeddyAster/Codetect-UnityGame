﻿// Created by SwanDEV 2018
using UnityEngine;

/// <summary>
/// DynamicUI - Image Display Handler for UGUI Image & RawImage.
/// How to use: (1) add as a base class (Inherits), (2) Drop this script in a GameObject and reference it.
/// Call the SetImage/SetRawImage method.
/// </summary>
public class DImageDisplayHandler : MonoBehaviour
{
    public enum BoundingTarget
    {
        /// <summary> Constraints the target image with the Vector2 size(m_Size). </summary>
        Size,

        /// <summary> Constraints the target image with the sizeDelta of the RectTranform(m_RectTransform). </summary>
        RectTransform,

        /// <summary> Constraints the target image with the device screen size. </summary>
        Screen,
    }

    public enum BoundingType
    {
        SetNativeSize = 0,

        /// <summary> Sets the entire image RectTransform to be constrained within a specified area size (width and height). </summary>
        WidthAndHeight,

        /// <summary> Sets the image RectTransform's width equal to the width of specified area size, the image height is not constrained. </summary>
        Width,

        /// <summary> Sets the image RectTransform's height equal to the height of specified area size, the image width is not constrained. </summary>
        Height,

        /// <summary> Sets the image RectTransform to cover the entire specified area size, either the image width or height will exceed that size if the aspect ratio is different. </summary>
        FillAll,
    }

    [Header("[ Image Display Handler ]")]
    public BoundingTarget m_BoundingTarget = BoundingTarget.Size;
    public RectTransform m_RectTransform;
    public Vector2 m_Size = new Vector2(512, 512);

    [Space()]
    public BoundingType m_BoundingType = BoundingType.SetNativeSize;

    [Space()]
    public float m_ScaleFactor = 1f;

    /// <summary>Auto clear the texture of the last set Image/RawImage before setting a new one.</summary>
    [Space()]
    [Tooltip("Auto clear the texture of the last set Image/RawImage before setting a new one.")]
    public bool m_AutoClearTexture = true;

    /// <summary>
    /// Is the target display(Image/RawImage) RectTranform (eulerAngles.z) rotated by 90/-90.
    /// </summary>
    [HideInInspector] public bool m_Rotated_90 = false;

    public void SetImage(UnityEngine.UI.Image displayImage, Sprite sprite)
    {
        if (m_AutoClearTexture) Clear(displayImage);
        displayImage.sprite = sprite;
        _SetSize(displayImage);
    }

    public void SetImage(UnityEngine.UI.Image displayImage, Texture2D texture2D)
    {
        if (m_AutoClearTexture) Clear(displayImage);
        displayImage.sprite = _TextureToSprite(texture2D);
        _SetSize(displayImage);
    }

    public void SetRawImage(UnityEngine.UI.RawImage displayImage, Sprite sprite)
    {
        if (m_AutoClearTexture) Clear(displayImage);
        displayImage.texture = (Texture)sprite.texture;
        _SetSize(displayImage);
    }

    public void SetRawImage(UnityEngine.UI.RawImage displayImage, Texture2D texture2D)
    {
        if (m_AutoClearTexture) Clear(displayImage);
        displayImage.texture = (Texture)texture2D;
        _SetSize(displayImage);
    }

    public void SetRawImage(UnityEngine.UI.RawImage displayImage, Texture texture)
    {
        if (m_AutoClearTexture) Clear(displayImage);
        displayImage.texture = texture;
        _SetSize(displayImage);
    }

    public void SetImage(UnityEngine.UI.Image displayImage, float width, float height)
    {
        displayImage.rectTransform.sizeDelta = _CalculateSize(new Vector2(width, height));
        _ApplyScaleFactor(displayImage.transform);
    }

    public void SetRawImage(UnityEngine.UI.RawImage displayImage, float width, float height)
    {
        displayImage.rectTransform.sizeDelta = _CalculateSize(new Vector2(width, height));
        _ApplyScaleFactor(displayImage.transform);
    }

    private void _SetSize(UnityEngine.UI.Image displayImage)
    {
        if (m_BoundingType == BoundingType.SetNativeSize)
        {
            displayImage.SetNativeSize();
        }
        else
        {
            displayImage.rectTransform.sizeDelta = _CalculateSize(new Vector2(displayImage.sprite.texture.width, displayImage.sprite.texture.height));
        }

        _ApplyScaleFactor(displayImage.transform);
    }

    private void _SetSize(UnityEngine.UI.RawImage displayImage)
    {
        if (m_BoundingType == BoundingType.SetNativeSize)
        {
            displayImage.SetNativeSize();
        }
        else
        {
            displayImage.rectTransform.sizeDelta = _CalculateSize(new Vector2(displayImage.texture.width, displayImage.texture.height));
        }

        _ApplyScaleFactor(displayImage.transform);
    }

    private void _ApplyScaleFactor(Transform displayImageT)
    {
        displayImageT.localScale = new Vector3(m_ScaleFactor, m_ScaleFactor, 1f);
    }

    private Vector2 _CalculateSize(Vector2 textureSize)
    {
        Vector2 boundarySize = Vector2.zero;

        switch (m_BoundingTarget)
        {
            case BoundingTarget.Size:
                boundarySize = m_Size;
                break;
            case BoundingTarget.RectTransform:
                boundarySize = m_RectTransform.GetComponent<RectTransform>().rect.size;
                break;
            case BoundingTarget.Screen:
                boundarySize = new Vector2(Screen.width, Screen.height);
                break;
        }

        float newWidth = textureSize.x;
        float newHeight = textureSize.y;
        float imageRatio = newWidth / newHeight;

        switch (m_BoundingType)
        {
            case BoundingType.FillAll:
                if (m_Rotated_90)
                {
                    newHeight = boundarySize.x;
                    newWidth = newHeight * imageRatio;

                    if (newWidth < boundarySize.y)
                    {
                        newWidth = boundarySize.y;
                        newHeight = newWidth / imageRatio;
                    }
                }
                else
                {
                    newWidth = boundarySize.x;
                    newHeight = newWidth / imageRatio;

                    if (newHeight < boundarySize.y)
                    {
                        newHeight = boundarySize.y;
                        newWidth = newHeight * imageRatio;
                    }
                }
                break;

            case BoundingType.WidthAndHeight:
                if (m_Rotated_90)
                {
                    newHeight = boundarySize.x;
                    newWidth = newHeight * imageRatio;

                    if (newWidth > boundarySize.y)
                    {
                        newWidth = boundarySize.y;
                        newHeight = newWidth / imageRatio;
                    }
                }
                else
                {
                    newWidth = boundarySize.x;
                    newHeight = newWidth / imageRatio;

                    if (newHeight > boundarySize.y)
                    {
                        newHeight = boundarySize.y;
                        newWidth = newHeight * imageRatio;
                    }
                }
                break;

            case BoundingType.Width:
                if (m_Rotated_90)
                {
                    newHeight = boundarySize.x;
                    newWidth = newHeight * imageRatio;
                }
                else
                {
                    newWidth = boundarySize.x;
                    newHeight = newWidth / imageRatio;
                }
                break;

            case BoundingType.Height:
                if (m_Rotated_90)
                {
                    newWidth = boundarySize.y;
                    newHeight = newWidth / imageRatio;
                }
                else
                {
                    newHeight = boundarySize.y;
                    newWidth = newHeight * imageRatio;
                }
                break;

            default:
                newWidth = textureSize.x;
                newHeight = textureSize.y;
                break;
        }

        return new Vector2(newWidth, newHeight);
    }

    private Sprite _TextureToSprite(Texture2D texture)
    {
        if (texture == null) return null;

        Vector2 pivot = new Vector2(0.5f, 0.5f);
        float pixelPerUnit = 100;
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), pivot, pixelPerUnit);
    }

    public void Clear(UnityEngine.UI.Image displayImage)
    {
        if (displayImage != null && displayImage.sprite != null && displayImage.sprite.texture != null)
        {
            Destroy(displayImage.sprite.texture);
            displayImage.sprite = null;
        }
    }

    public void Clear(UnityEngine.UI.RawImage displayImage)
    {
        if (displayImage != null && displayImage.texture != null)
        {
            Destroy(displayImage.texture);
            displayImage.texture = null;
        }
    }

}
