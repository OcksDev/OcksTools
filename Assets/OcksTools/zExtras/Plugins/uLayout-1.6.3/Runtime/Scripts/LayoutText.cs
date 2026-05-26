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
using TMPro;
using UnityEngine.UI;

namespace Poke.UI
{
    [RequireComponent(typeof(TMP_Text))]
    public class LayoutText : LayoutItem
    {
        private TMP_Text _text;
        private Vector2 _preferredSize;
        private float _fontSize;
        private string _str;
        private bool _textChanged;
        
        protected override void Awake() {
            base.Awake();
            _text = GetComponent<TMP_Text>();
        }

        protected override void OnEnable() {
            base.OnEnable();
            Log("enable");
            
            _str = _text.text;
            _fontSize = _text.fontSize;
            _text.ForceMeshUpdate(true, true);

            _preferredSize = _text.GetPreferredValues();
        }

        public override void Update() {
            base.Update();
            
            _text.textWrappingMode = m_sizing.x != SizingMode.FitContent ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;

            if(String.CompareOrdinal(_str, _text.text) != 0) {
                _str = _text.text;
                SetDirty();
                _textChanged = true;
            }

            if(!Mathf.Approximately(_text.fontSize, _fontSize)) {
                _fontSize = _text.fontSize;
                SetDirty();
            }

            if(_dirty) {
                Log("Marking for rebuild");
                if(_parent)
                    LayoutRebuilder.MarkLayoutForRebuild(_rect);
                else {
                    CalculateLayoutInputHorizontal();
                    CalculateLayoutInputVertical();
                }
            }
        }

        protected override void SetDrivenProperties() {
            if(m_sizing.x == SizingMode.FitContent || m_sizing.x == SizingMode.Grow)
                _trackerProps |= DrivenTransformProperties.SizeDeltaX;
            if(m_sizing.y == SizingMode.FitContent || m_sizing.y == SizingMode.Grow)
                _trackerProps |= DrivenTransformProperties.SizeDeltaY;

            if(_parent && !m_ignoreLayout) {
                _trackerProps |= DrivenTransformProperties.AnchoredPosition | DrivenTransformProperties.Anchors;
            }
        }

        private void Log(object msg) {
            if(m_log) Debug.Log($"[{_frame}] [LT:{gameObject.name}]: {msg}");
        }

        public override void CalculateLayoutInputHorizontal() {

            if(_dirty) {
                Log("CalculateLayoutInputHorizontal");
                _text.ForceMeshUpdate(true, _textChanged);
                _preferredSize = _text.GetPreferredValues();

                if(m_sizing.x == SizingMode.FitContent) {
                    Log($"fitting x ({_preferredSize.x})");
                    _rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _preferredSize.x);
                }
            }
        }

        public override void CalculateLayoutInputVertical() {
            if(_dirty) {
                Log("CalculateLayoutInputVertical");
                
                if(m_sizing.y == SizingMode.FitContent) {
                    Log($"fitting y ({_preferredSize.y})");
                    _rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _preferredSize.y);
                }
            }

            _dirty = false;
            _textChanged = false;
        }

        public void HandleGrowSizingX() {
            if(m_sizing.y == SizingMode.FitContent) {
                _text.ForceMeshUpdate();
                _preferredSize = _text.GetPreferredValues();
                Log($"resizing y based on x growth ({_preferredSize.y})");
            
                _rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _preferredSize.y);
            }
        }
    }
}
