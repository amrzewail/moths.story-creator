using Moths.Graphs.Editor;
using Moths.Stories.Actions.Editor;
using Moths.Stories.Editor.Graphs;
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Moths.Stories.Editor
{
    public class ActionNode : BasicNode, IInspectable
    {
        private Story _story;
        private StoryBeat _beat;
        private Node _node;
        private StoryAction _action;

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
        }

        public string InspectorTitle => _action.Name;

        public override void GeneratePorts()
        {
            inputContainer.Clear();
            outputContainer.Clear();
            extensionContainer.Clear();

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

            inspector.Add(textField);

            /// custom inspector
            {
                // 1. Get the exact type of the current action
                Type actionType = _action.GetType();

                // 2. Create the generic base type: CustomStoryActionInspector<ActualActionType>
                Type baseInspectorType = typeof(CustomStoryActionInspector<>).MakeGenericType(actionType);

                // 3. Find a concrete class in the current AppDomain that inherits from this base type
                Type customInspectorType = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .FirstOrDefault(t => t.IsClass && !t.IsAbstract && baseInspectorType.IsAssignableFrom(t));

                if (customInspectorType != null)
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

                    // 4. Instantiate the found inspector class
                    object inspectorInstance = Activator.CreateInstance(customInspectorType);

                    // 5. Set the private '_story' field
                    // Note: Because '_story' is private, we MUST reflect on the base class where it is declared
                    FieldInfo storyField = baseInspectorType.GetField("_story", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (storyField != null)
                    {
                        storyField.SetValue(inspectorInstance, _story);
                    }

                    // 6. Call the UpdateInspector method
                    MethodInfo updateMethod = baseInspectorType.GetMethod("UpdateInspector");
                    if (updateMethod != null)
                    {
                        SerializedObject serializedObject = new SerializedObject(_story);
                        var property = serializedObject.FindProperty($"_beats")
                            .GetArrayElementAtIndex(beatIndex)
                            .FindPropertyRelative("_actions")
                            .GetArrayElementAtIndex(actionIndex)
                            .FindPropertyRelative("_object");

                        updateMethod.Invoke(inspectorInstance, new object[] { inspector, property, this });
                    }
                }
            }

            return inspector;
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);

            _node.position = newPos.position;
            _node.Update();
        }
    }
}