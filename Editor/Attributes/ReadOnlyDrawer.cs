using UnityEditor;
using UnityEngine;
using StoryCreator.Attributes;

namespace StoryCreator.Editor.Attributes
{

    [CustomPropertyDrawer(typeof(StoryCreator.Attributes.ReadOnlyAttribute))]
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