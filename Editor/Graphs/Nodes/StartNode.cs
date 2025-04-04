using Moths.Graphs.Editor;
using Moths.StoryCreator.Attributes;
using Moths.StoryCreator.Models;
using Moths.StoryCreator.QuestElements;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Moths.StoryCreator.Editor.Graphs.Nodes
{
    [System.Serializable]
    public class StartNode<TTargetType> : BasicNode
    {
        public new const string GUID = "$$START_NODE";
        public const string OUTPUT_GUID = "$$START_OUTPUT_PORT";

        public StartNode() 
        {
            base.title = "START";
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
            p.portName = "Output";
            p.viewDataKey = OUTPUT_GUID;
            outputContainer.Add(p);

        }

    }
}