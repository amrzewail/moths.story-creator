using Moths.Graphs.Editor;
using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Moths.Stories.Editor
{
    public class BeatNode : BasicNode, IInspectable, ISerializable
    {
        private Story _story;
        private Node _node;
        private StoryBeat _beat;
        private Label _descriptionLabel;

        public StoryBeat Beat => _beat;

        public event Action EditClicked;

        public BeatNode(Story story, Node node, StoryBeat beat) : base()
        {
            _story = story;
            _node = node;
            _beat = beat;

            GUID = beat.Guid;
            title = beat.Name;
            position = node.position;

            this.RegisterCallback<MouseDownEvent>(OnMouseDown);
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

            extensionContainer.Add(_descriptionLabel = new Label(_beat.Description));
        }

        private void OnMouseDown(MouseDownEvent evt)
        {
            // Check if it's a Left-Click (button 0) and a Double-Click (clickCount 2)
            if (evt.button == 0 && evt.clickCount == 2)
            {
                // Trigger your edit event
                EditClicked?.Invoke();

                // Stop propagation so the GraphView doesn't process the double-click further 
                // (prevents unwanted default behaviors like framing or opening other menus)
                evt.StopPropagation();
            }
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

            var descriptionField = new TextField("Description");
            descriptionField.multiline = true;
            descriptionField.value = _beat.Description;
            descriptionField.RegisterValueChangedCallback(callback =>
            {
                _beat.Description = callback.newValue;
                EditorUtility.SetDirty(_story);

                _descriptionLabel.text = _beat.Description;
            });

            inspector.Add(textField);
            inspector.Add(descriptionField);


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

        public string Serialize()
        {
            // We'll implement a custom serialization for StoryBeat to handle InterfaceReference
            return _beat.Serialize();
        }
    }
}
