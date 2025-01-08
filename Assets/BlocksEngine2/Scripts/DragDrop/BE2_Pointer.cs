using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MG_BlocksEngine2.Core;

namespace MG_BlocksEngine2.DragDrop
{
    // v2.7 - BE2_Pointer refactored to use the BE2 Input Manager
    public class BE2_Pointer : MonoBehaviour
    {
        Transform _transform;
        Vector3 _mousePos;

        // v2.6 - added property Instance in the BE2_Pointer
        static BE2_Pointer _instance;
        public static BE2_Pointer Instance
        {
            get
            {
                if (!_instance)
                {
                    _instance = GameObject.FindObjectOfType<BE2_Pointer>();
                }
                return _instance;
            }
            set => _instance = value;
        }

        // Reference to the camera (use main camera or assign specific one)
        public Camera targetCamera;

        void Awake()
        {
            _transform = transform;

            if (targetCamera == null)
            {
                // If no camera is assigned, use the main camera
                targetCamera = Camera.main;
            }
        }

        public void OnUpdate()
        {
            UpdatePointerPosition();
        }

        public void UpdatePointerPosition()
        {
            // 获取鼠标或指针的屏幕位置（可以替换为触摸输入或其他输入方式）
            _mousePos = BE2_InputManager.Instance.CanvasPointerPosition;

            // 将屏幕坐标转换为世界坐标
            Vector3 worldPos = targetCamera.ScreenToWorldPoint(new Vector3(_mousePos.x, _mousePos.y, targetCamera.nearClipPlane));

            // 更新指针位置
            _transform.position = new Vector3(worldPos.x, worldPos.y, _transform.position.z);

            // 确保指针保持平面上的位置
            _transform.localPosition = new Vector3(_transform.localPosition.x, _transform.localPosition.y, 0);

            // 设置旋转角度为零
            _transform.localEulerAngles = Vector3.zero;
        }
    }
}
