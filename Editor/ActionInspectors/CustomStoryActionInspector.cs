using Moths.Stories.Editor;
using Moths.Stories.Editor.Graphs;
using UnityEditor;
using UnityEngine.UIElements;

namespace Moths.Stories.Actions.Editor
{
    public abstract class CustomStoryActionInspector<T> where T : StoryAction
    {
        private Story _story;

        public abstract void UpdateInspector(VisualElement inspector, SerializedProperty property, ActionNode actionNode);

        protected void UpdateAsset()
        {
            EditorUtility.SetDirty(_story);
        }
    }
}