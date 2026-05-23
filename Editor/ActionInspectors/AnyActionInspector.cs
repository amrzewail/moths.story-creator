using Moths.Stories.Editor;
using Moths.Stories.Editor.Graphs;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Moths.Stories.Actions.Editor
{
    public class AnyActionInspector : CustomStoryActionInspector<AnyAction>
    {
        public override void UpdateInspector(VisualElement inspector, SerializedProperty property, ActionNode actionNode)
        {
            var count = property.FindPropertyRelative("_count");
            var countElement = new UnityEditor.UIElements.PropertyField(count);
            countElement.Bind(count.serializedObject);

            countElement.RegisterValueChangeCallback(callback =>
            {
                actionNode.GeneratePorts();
            });

            inspector.Add(countElement);
        }
    }
}