using Moths.Graphs.Editor;
using Moths.Stories.Editor.Graphs;
using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Moths.Stories.Editor
{
    public class OutcomeNode : BasicNode, IInspectable
    {
        private Story _story;
        private StoryBeat _beat;
        private Node _node;
        private BeatOutcome _outcome;

        public string InspectorTitle => _outcome.name;

        public event Action<Vector2> NodeMoved;

        public OutcomeNode(Story story, StoryBeat beat, Node node, BeatOutcome outcome)
        {
            _story = story;
            _beat = beat;
            _node = node;
            _outcome = outcome;

            GUID = outcome.guid;
            title = outcome.name;
            position = node.position;
        }

        public override void GeneratePorts()
        {
            var entryPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(ActionNode));
            entryPort.portName = "";
            entryPort.viewDataKey = _outcome.guid;
            inputContainer.Add(entryPort);
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);

            _node.position = newPos.position;
            _node.Update();

            NodeMoved?.Invoke(newPos.position);
        }

        public VisualElement GetInspector()
        {
            var inspector = new VisualElement();

            var textField = new TextField("Name");
            textField.value = _outcome.name;
            textField.RegisterValueChangedCallback(callback =>
            {
                _outcome.name = callback.newValue;
                _beat.UpdateOutcome(_outcome);
                title = _outcome.name;
                EditorUtility.SetDirty(_story);
            });

            inspector.Add(textField);

            return inspector;
        }
    }
}