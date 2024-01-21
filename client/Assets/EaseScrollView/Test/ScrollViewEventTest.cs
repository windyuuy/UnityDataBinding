using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollViewEventTest : MonoBehaviour
{
    [SerializeField]
    private ScrollRect _scrollRect;
    /// <summary>
    /// Cached reference to the active cell view container
    /// </summary>
    [SerializeField]
    private RectTransform _container;

    /// <summary>
    /// Cached reference to the scrollRect's transform
    /// </summary>
    [SerializeField]
    private RectTransform _scrollRectTransform;

    void OnEnable()
    {
        // when the scroller is enabled, add a listener to the onValueChanged handler
        _scrollRect.onValueChanged.AddListener(_ScrollRect_OnValueChanged);
    }

    void OnDisable()
    {
        // when the scroller is disabled, remove the listener
        _scrollRect.onValueChanged.RemoveListener(_ScrollRect_OnValueChanged);
    }
    /// <summary>
    /// The size of the active cell view container minus the visibile portion
    /// of the scroller
    /// </summary>
    public float ScrollSize
    {
        get
        {
            return Mathf.Max(_container.rect.height - _scrollRectTransform.rect.height, 0);
        }
    }

    /// <summary>
    /// The scrollers position
    /// </summary>    
    private float _scrollPosition;

    /// <summary>
    /// Handler for when the scroller changes value
    /// </summary>
    /// <param name="val">The scroll rect's value</param>
    private void _ScrollRect_OnValueChanged(Vector2 val)
    {
        // set the internal scroll position
        var scrollPosition = (1f - val.y) * ScrollSize;
        //_refreshActive = true;
        scrollPosition = Mathf.Clamp(scrollPosition, 0, ScrollSize);
        if (scrollPosition.CompareTo(_scrollPosition) != 0)
        {
            _scrollPosition = scrollPosition;
            var scrollBottom = scrollPosition + _scrollRectTransform.rect.height;
            Debug.Log($"scrollPosition: {scrollPosition}, scrollBottom:{scrollBottom}, ScrollSize:{ScrollSize}");
            
        }
    }

}
