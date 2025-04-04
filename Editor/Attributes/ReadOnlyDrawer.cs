using UnityEditor;
using UnityEngine;
using Moths.StoryCreator.Attributes;

namespace Moths.StoryCreator.Editor.Attributes
{

    [CustomPropertyDrawer(typeof(Moths.StoryCreator.Attributes.ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position,
                                   SerializedProperty property,
                                   GUIContent label)
        {
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }
    }
}