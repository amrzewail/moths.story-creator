using Moths.Graphs.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Moths.Stories.Editor.Graphs.Nodes
{
    [System.Serializable]
    public class StoryStartNode : StartNode<BeatNode>, IInspectable
    {
        private Story _story;
        public StoryStartNode(Story story, string title) : base(title)
        {
            _story = story;
        }

        public string InspectorTitle => "Story Start";

        public VisualElement GetInspector()
        {
            var inspector = new VisualElement();

            var serializedObject = new SerializedObject(_story);

            var starterProperty = serializedObject.FindProperty("_starter");
            var starterElement = new PropertyField(starterProperty);
            starterElement.Bind(serializedObject);

            inspector.Add(starterElement);

            return inspector;
        }
    }

    [System.Serializable]
    public class StartNode<TTargetType> : BasicNode
    {
        public new const string GUID = "$$START_NODE";

        public StartNode(string title) 
        {
            base.title = title;
            base.GUID = GUID;
        }

        public override bool IsMovable() => false;
        public override bool IsCopiable() => false;

        public override void GeneratePorts()
        {
            inputContainer.Clear();
            outputContainer.Clear();
            extensionContainer.Clear();

            var p = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(TTargetType));
            p.portName = "";
            p.viewDataKey = GUID;
            outputContainer.Add(p);

        }
    }
}