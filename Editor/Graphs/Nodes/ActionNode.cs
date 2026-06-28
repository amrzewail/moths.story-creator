using Moths.Graphs.Editor;
using Moths.Stories.Actions.Editor;
using Moths.Stories.Editor.Graphs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.EditorGUI;
using static UnityEngine.Audio.ProcessorInstance.AvailableData;

namespace Moths.Stories.Editor
{
    public class ActionNode : BasicNode, IInspectable, ISerializable
    {
        private Story _story;
        private StoryBeat _beat;
        private Node _node;
        private StoryAction _action;

        public StoryAction Action => _action;

        public event Action PortsUpdated;

        public ActionNode(Story story, StoryBeat beat, Node node, StoryAction action) : base()
        {
            _story = story;
            _beat = beat;
            _node = node;
            _action = action;

            GUID = action.Guid;
            title = action.Name;
            position = node.position;

            UpdateDescription();
        }

        private void UpdateDescription()
        {
            extensionContainer.Clear();
            extensionContainer.Add(new Label(_action.Description));
        }

        public string InspectorTitle => _action.Name;

        public override void GeneratePorts()
        {
            inputContainer.Clear();
            outputContainer.Clear();

            var entryPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(ActionNode));
            entryPort.portName = "Activate";
            entryPort.viewDataKey = _action.Guid;
            inputContainer.Add(entryPort);

            _action.UpdateOutputs();

            if (_action.Outputs != null)
            {
                foreach (var output in _action.Outputs)
                {
                    var p = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(ActionNode));
                    p.portName = output.name;
                    p.viewDataKey = output.guid;

                    outputContainer.Add(p);
                }
            }

            PortsUpdated?.Invoke();
        }

        public VisualElement GetInspector()
        {
            var inspector = new VisualElement();

            var textField = new TextField("Name");
            textField.value = _action.Name;
            textField.RegisterValueChangedCallback(callback =>
            {
                _action.Name = callback.newValue;
                title = _action.Name;
                EditorUtility.SetDirty(_story);
            });
            textField.style.marginBottom = 4;

            inspector.Add(textField);

            /// custom inspector
            {
                System.Type currentType = _action.GetType();

                Stack<System.Type> typeHierarchy = new Stack<System.Type>();

                while (currentType != null && currentType != typeof(object) && currentType != typeof(StoryAction))
                {
                    typeHierarchy.Push(currentType);
                    currentType = currentType.BaseType;
                }

                while (typeHierarchy.Count > 0)
                {
                    var classInspector = new VisualElement();

                    // 1. Get the exact type of the current action
                    Type actionType = typeHierarchy.Pop();

                    // 2. Create the generic base type: CustomStoryActionInspector<ActualActionType>
                    Type baseInspectorType = typeof(CustomStoryActionInspector<>).MakeGenericType(actionType);

                    // 3. Find a concrete class in the current AppDomain that inherits from this base type
                    Type customInspectorType = TypeCache.GetTypesDerivedFrom(baseInspectorType)
                                    .FirstOrDefault(t => t.IsClass && !t.IsAbstract);

                    var property = GetSerializedProperty();

                    if (customInspectorType != null)
                    {
                        object inspectorInstance = Activator.CreateInstance(customInspectorType);

                        FieldInfo storyField = baseInspectorType.GetField("_story", BindingFlags.NonPublic | BindingFlags.Instance);
                        if (storyField != null)
                        {
                            storyField.SetValue(inspectorInstance, _story);
                        }

                        MethodInfo updateMethod = baseInspectorType.GetMethod("UpdateInspector");
                        if (updateMethod != null)
                        {
                            updateMethod.Invoke(inspectorInstance, new object[] { classInspector, property, this });
                        }
                    }
                    else
                    {
                        DrawManagedReferenceFieldsViaReflection(classInspector, actionType, property);
                    }

                    if (classInspector.childCount > 0)
                    {
                        classInspector.style.marginBottom = 4;
                        inspector.Add(classInspector);
                    }
                }
            }

            inspector.RegisterCallback<ChangeEvent<string>>(e => UpdateDescription());
            inspector.RegisterCallback<ChangeEvent<bool>>(e => UpdateDescription());
            inspector.RegisterCallback<ChangeEvent<float>>(e => UpdateDescription());
            inspector.RegisterCallback<ChangeEvent<int>>(e => UpdateDescription());
            inspector.RegisterCallback<ChangeEvent<UnityEngine.Object>>(e => UpdateDescription());
            inspector.RegisterCallback<ChangeEvent<System.Enum>>(e => UpdateDescription());
            inspector.RegisterCallback<ChangeEvent<UnityEngine.Vector2>>(e => UpdateDescription());
            inspector.RegisterCallback<ChangeEvent<UnityEngine.Vector3>>(e => UpdateDescription());
            inspector.RegisterCallback<ChangeEvent<UnityEngine.Color>>(e => UpdateDescription());

            UpdateDescription();

            return inspector;
        }

        private void DrawManagedReferenceFieldsViaReflection(VisualElement inspector, Type type, SerializedProperty managedProperty)
        {
            object targetObject = managedProperty.managedReferenceValue;

            if (targetObject == null) return;

            FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (FieldInfo fieldInfo in fields)
            {
                if (fieldInfo.DeclaringType != type) continue;

                bool isPublic = fieldInfo.IsPublic;
                bool hasSerializeField = fieldInfo.GetCustomAttribute<SerializeField>() != null;

                if (isPublic || hasSerializeField)
                {
                    SerializedProperty childProperty = managedProperty.FindPropertyRelative(fieldInfo.Name);

                    if (childProperty != null)
                    {
                        PropertyField uiField = new PropertyField(childProperty);

                        uiField.BindProperty(managedProperty.serializedObject);

                        inspector.Add(uiField);
                    }
                }
            }
        }

        private SerializedProperty GetSerializedProperty()
        {
            int beatIndex = -1;
            for (int i = 0; i < _story.Beats.Count; i++)
            {
                if (_story.Beats[i] == _beat)
                {
                    beatIndex = i;
                    break;
                }
            }

            int actionIndex = -1;
            for (int i = 0; i < _beat.Actions.Count; i++)
            {
                if (_beat.Actions[i].Value == _action)
                {
                    actionIndex = i;
                    break;
                }
            }

            SerializedObject serializedObject = new SerializedObject(_story);
            var property = serializedObject.FindProperty($"_beats")
                .GetArrayElementAtIndex(beatIndex)
                .FindPropertyRelative("_actions")
                .GetArrayElementAtIndex(actionIndex)
                .FindPropertyRelative("_object");

            return property;
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);

            _node.position = newPos.position;
            _node.Update();
        }

        public string Serialize()
        {
            return JsonUtility.ToJson(_action);
        }
    }
}
