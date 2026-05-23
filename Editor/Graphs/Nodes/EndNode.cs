using Moths.Graphs.Editor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Moths.Stories.Editor.Graphs.Nodes
{
    [System.Serializable]
    public class EndNode<TTargetType> : BasicNode
    {
        public new const string GUID = "__STORY_END__";

        private Node _node;

        public EndNode(Node node, string title)
        {
            _node = node;

            base.title = title;
            base.GUID = GUID;

            base.position = node.position;
        }

        public override bool IsCopiable() => false;

        public override void GeneratePorts()
        {
            inputContainer.Clear();
            outputContainer.Clear();
            extensionContainer.Clear();

            var p = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(TTargetType));
            p.portName = "";
            p.viewDataKey = GUID;
            outputContainer.Add(p);

        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);

            _node.position = newPos.position;
            _node.Update();
        }
    }
}