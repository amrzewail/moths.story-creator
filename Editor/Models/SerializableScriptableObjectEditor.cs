using StoryCreator.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace StoryCreator.Editor.Models
{
    using Editor = UnityEditor.Editor;

    [CustomPropertyDrawer(typeof(SerializableScriptableObject), true)]
    public class SerializableScriptableObjectEditor : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {

            VisualElement element = new VisualElement();

            var serializedJson = property.FindPropertyRelative(nameof(SerializableScriptableObject.serialized))
                .FindPropertyRelative(nameof(SerializableScriptableObject.SerializedScriptableObject.json));
            var serializedType = property.FindPropertyRelative(nameof(SerializableScriptableObject.serialized))
                .FindPropertyRelative(nameof(SerializableScriptableObject.SerializedScriptableObject.type));

            if (string.IsNullOrEmpty(serializedJson.stringValue) || string.IsNullOrEmpty(serializedType.stringValue))
            {
                element.Add(new Label("\tUndefined ScriptableObject"));
                return element;
            }

            ScriptableObject obj = ScriptableObject.CreateInstance(serializedType.stringValue);
            JsonUtility.FromJsonOverwrite(serializedJson.stringValue, obj);

            SerializedObject serialized = new SerializedObject(obj);
            InspectorElement serializedInspector = new InspectorElement(serialized);

            Foldout foldout = new Foldout();
            //foldout.style.position = Position.Absolute;
            foldout.text = property.displayName;
            foldout.style.width = Length.Percent(100);
            element.Add(foldout);

            foldout.Add(serializedInspector);

            serializedInspector.TrackSerializedObjectValue(serialized, s =>
            {
                property.serializedObject.Update();
                serializedJson.stringValue = JsonUtility.ToJson(s.targetObject);
                property.serializedObject.ApplyModifiedProperties();
            });

            return element;
        }

    }
}