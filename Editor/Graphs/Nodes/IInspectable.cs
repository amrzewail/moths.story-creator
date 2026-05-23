using Moths.Stories.Editor.Graphs;
using UnityEngine.UIElements;

namespace Moths.Stories.Editor
{
    public interface IInspectable
    {
        string InspectorTitle { get; }
        VisualElement GetInspector();
    }
}