using System;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace UnityEngine.UI
{
    /// <summary>
    /// A standard toggle that has an on / off state.
    /// </summary>
    /// <remarks>
    /// The toggle component is a Selectable that controls a child graphic which displays the on / off state.
    /// When a toggle event occurs a callback is sent to any registered listeners of UI.Toggle._onValueChanged.
    /// </remarks>
    [AddComponentMenu("UI/MyToggle", 30)]
    [RequireComponent(typeof(RectTransform))]
    public class MyToggle : Toggle
    {
    }
}
