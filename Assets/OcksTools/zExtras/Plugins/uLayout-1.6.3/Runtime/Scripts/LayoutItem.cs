/*
    Copyright (c) 2026 Alex Howe

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.
*/
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Poke.UI
{
    [
        ExecuteAlways,
        RequireComponent(typeof(RectTransform))
    ]
    public class LayoutItem : MonoBehaviour, ILayoutElement
    {
        [SerializeField] protected bool m_log;
        
        [Header("Layout Item")]
        [SerializeField] protected bool m_ignoreLayout = false;
        [SerializeField] protected SizeModes m_sizing;

        protected float _minWidth;
        protected float _preferredWidth;
        protected float _flexibleWidth;
        protected float _minHeight;
        protected float _preferredHeight;
        protected float _flexibleHeight;
        protected int _layoutPriority;

        public float minWidth => _minWidth;
        public float preferredWidth => _preferredWidth;
        public float flexibleWidth => _flexibleWidth;
        public float minHeight => _minHeight;
        public float preferredHeight => _preferredHeight;
        public float flexibleHeight => _flexibleHeight;
        public int layoutPriority => _layoutPriority;
        
        public bool IgnoreLayout {
            get => m_ignoreLayout;
            set => m_ignoreLayout = value;
        }
        public RectTransform Rect => _rect;
        public DrivenTransformProperties TrackerProps {
            get => _trackerProps;
            set => _trackerProps = value;
        }
        public SizeModes SizeMode => m_sizing;
        
        protected RectTransform _rect;
        protected DrivenRectTransformTracker _tracker;
        protected DrivenTransformProperties _trackerProps;
        protected RectTransform _parentRect;
        protected Layout _parent;
        protected bool _dirty = true;
        protected int _frame;
        
        private Vector2 _parentSize;
        
        [Serializable]
        public struct SizeModes
        {
            public SizingMode x;
            public SizingMode y;
        }

        #region LayoutItem MonoBehavior
        protected virtual void Awake() {
            Log("awake");
            
            _rect = GetComponent<RectTransform>();
            _tracker = new DrivenRectTransformTracker();
            
            _parentSize = _parentRect ? _parentRect.rect.size : default;
        }

        protected virtual void OnEnable() {
            if(transform.parent) {
                _parentRect = transform.parent.GetComponent<RectTransform>();
                _parent = transform.parent.GetComponent<Layout>();
            }

            _trackerProps = DrivenTransformProperties.None;
            _dirty = true;
        }

        public virtual void Update() {
            //Log("update");
            _frame = Time.frameCount;
            
            #if UNITY_EDITOR
            _tracker.Clear();
            _trackerProps = DrivenTransformProperties.None;
            
            SetDrivenProperties();
            
            _tracker.Add(this, _rect, _trackerProps);
            #endif
            
            // Do grow sizing here if parent is not a Layout
            if(!_parent && _parentRect) {
                // only update size if parent size has changed
                if(m_sizing.x == SizingMode.Grow && !Mathf.Approximately(_parentRect.rect.size.x, _parentSize.x)) {
                    _rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _parentRect.rect.size.x);
                    _parentSize = _parentSize.SetX(_parentRect.rect.size.x);
                }
                if(m_sizing.y == SizingMode.Grow && !Mathf.Approximately(_parentRect.rect.size.y, _parentSize.y)) {
                    _rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _parentRect.rect.size.y);
                    _parentSize = _parentSize.SetY(_parentRect.rect.size.y);
                }
            }
        }
        #endregion

        private void Log(object msg) {
            if(m_log) Debug.Log($"[{_frame}] [LI:{gameObject.name}]: {msg}");
        }
        
        protected virtual void SetDrivenProperties() {
            if((m_sizing.x == SizingMode.FitContent && transform.childCount > 0) || m_sizing.x == SizingMode.Grow)
                _trackerProps |= DrivenTransformProperties.SizeDeltaX;
            if((m_sizing.y == SizingMode.FitContent && transform.childCount > 0) || m_sizing.y == SizingMode.Grow)
                _trackerProps |= DrivenTransformProperties.SizeDeltaY;

            if(_parent && !m_ignoreLayout) {
                _trackerProps |= DrivenTransformProperties.AnchoredPosition | DrivenTransformProperties.Anchors;
            }
        }

        public virtual void SetDirty() {
            _dirty = true;
            if(_parent) {
                _parent.SetDirty();
            }
        }

        public virtual void CalculateLayoutInputHorizontal() {
            Log("CalculateLayoutInputHorizontal");
        }
        public virtual void CalculateLayoutInputVertical() {
            Log("CalculateLayoutInputVertical");
        }
    }
}
