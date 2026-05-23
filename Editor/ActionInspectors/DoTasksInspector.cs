using Moths.Stories.Editor;
using Moths.Stories.Editor.Graphs;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Moths.Stories.Actions.Editor
{
    public class DoTasksInspector : CustomStoryActionInspector<DoTasks>
    {
        public override void UpdateInspector(VisualElement inspector, SerializedProperty property, ActionNode actionNode)
        {
            var tasks = property.FindPropertyRelative("_tasks");

            var tasksElement = new UnityEditor.UIElements.PropertyField(tasks);

            tasksElement.Bind(tasks.serializedObject);

            inspector.Add(tasksElement);
        }
    }
}