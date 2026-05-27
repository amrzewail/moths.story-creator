using Codice.Utils;
using Moths.Graphs.Editor;
using Moths.Stories.Editor.Graphs;
using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Moths.Stories.Editor
{
    public class BeatNode : BasicNode, IInspectable
    {
        private Story _story;
        private Node _node;
        private StoryBeat _beat;

        public event Action EditClicked;

        public BeatNode(Story story, Node node, StoryBeat beat) : base()
        {
            _story = story;
            _node = node;
            _beat = beat;

            GUID = beat.Guid;
            title = beat.Name;
            position = node.position;
        }

        public string InspectorTitle => _beat.Name;

        public override void GeneratePorts()
        {
            inputContainer.Clear();
            outputContainer.Clear();
            extensionContainer.Clear();

            var entryPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(BeatNode));
            entryPort.portName = "Activate";
            entryPort.viewDataKey = _beat.Guid;
            inputContainer.Add(entryPort);

            if (_beat.Outcomes != null)
            {
                foreach (var outcome in _beat.Outcomes)
                {
                    var p = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(BeatNode));
                    p.portName = outcome.name;
                    p.viewDataKey = outcome.guid;
                    
                    outputContainer.Add(p);
                }
            }

            extensionContainer.Add(new Button(EditClicked)
            {
                text = "Edit"
            });
        }

        public VisualElement GetInspector()
        {
            var inspector = new VisualElement();

            var textField = new TextField("Name");
            textField.value = _beat.Name;
            textField.RegisterValueChangedCallback(callback =>
            {
                _beat.Name = callback.newValue;
                title = _beat.Name;
                EditorUtility.SetDirty(_story);
            });

            inspector.Add(textField);


            inspector.Add(new Button(EditClicked)
            {
                text = "Edit"
            });

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