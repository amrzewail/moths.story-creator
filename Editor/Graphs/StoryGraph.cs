using Moths.Graphs.Editor;
using Moths.Stories;
using Moths.Stories.Editor.Graphs.Nodes;
using Moths.Stories.Editor.VisualElements;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;

namespace Moths.Stories.Editor.Graphs
{
    [System.Serializable]
    public struct SerializedStoryGraph
    {
        [System.Serializable]
        public struct SerializedConnection
        {
            public string outputNodeGuid;
            public string inputNodeGuid;
            public int outputPortIndex;
            public int inputPortIndex;
        }

        [System.Serializable]
        public struct SerializedNode
        {
            public string type;
            public string guid;
            public Vector2 position;
            public string data;
        }

        public List<SerializedNode> nodes;
        public List<SerializedConnection> connections;
    }

    public class StoryGraph : BaseGraph<Story>, IRefreshable
    {
        private StoryCreator _editor;
        private Story _story;

        public override void Initialize(StoryCreator editor, Story data)
        {
            _editor = editor;
            _story = (Story)data;

            Sidebar sidebar = new Sidebar();
            sidebar.title = _story.Name;

            var editCategory = sidebar.AddCategory("BEATS");

            Button newBeatBtn = new Button();
            newBeatBtn.AddToClassList("new-quest-btn");
            newBeatBtn.text = "New Beat";
            newBeatBtn.clicked += NewQuestCallback;

            editCategory.Content.Add(newBeatBtn);

            _graphView.EdgeCreated += EdgeCreatedCallback;
            _graphView.EdgeRemoved += EdgeRemovedCallback;
            _graphView.NodeSelected += NodeSelectedCallback;
            _graphView.NodeUnselected += NodeUnselectedCallback;
            _graphView.NodeRemoved += NodeRemovedCallback;

            _graphView.serializeGraphElements = SerializeGraphElementsCallback;
            _graphView.canPasteSerializedData = data => true;
            _graphView.unserializeAndPaste = UnserializePasteCallback;

            this.Add(sidebar);

            Refresh();
        }

        private void NodeRemovedCallback(UnityEditor.Experimental.GraphView.Node node)
        {
            if (node is BeatNode beatNode)
            {
                _story.RemoveBeat(beatNode.GUID);

                if (_story.StartingBeat == beatNode.GUID) _story.StartingBeat = string.Empty;

                EditorUtility.SetDirty(_story);
         
                Refresh();
            }

        }

        private void EdgeCreatedCallback(Edge edge)
        {
            if (edge.output.node is StoryStartNode)
            {
                _story.StartingBeat = edge.input.viewDataKey;
                EditorUtility.SetDirty(_story);
            }

            _story.Connections[edge.output.viewDataKey] = edge.input.viewDataKey;
            EditorUtility.SetDirty(_story);

        }
        private void EdgeRemovedCallback(Edge edge)
        {
            try
            {
                if (edge.output.node is StoryStartNode)
                {
                    _story.StartingBeat = string.Empty;
                    EditorUtility.SetDirty(_story);
                    return;
                }

                _story.Connections.Remove(edge.output.viewDataKey);
                EditorUtility.SetDirty(_story);
            }
            catch { }
        }

        private void NodeUnselectedCallback(BasicNode node)
        {
            _editor.Inspect(null, null);
        }

        private void NodeSelectedCallback(BasicNode node)
        {
            if (node is IInspectable inspectable)
            {
                _editor.Inspect(inspectable.InspectorTitle, inspectable.GetInspector());
            }
        }

        public void Refresh()
        {
            RefreshNodes();
            RefreshConnections();
        }


        private void RefreshNodes()
        {
            _graphView.ClearNodes();

            var startNode = new StoryStartNode(_story, "Story Start");
            _graphView.AddNode(startNode);

            var graphNode = _editor.Graph.FindNodeByGuid(EndNode<BeatNode>.GUID, out var isNew);
            var endNode = new EndNode<BeatNode>(graphNode, "Story End");
            _graphView.AddNode(endNode);

            foreach(var beat in _story.Beats)
            {
                graphNode = _editor.Graph.FindNodeByGuid(beat.Guid, out isNew);

                if (isNew) graphNode.position = _graphView.GetViewportCenter();

                var beatNode = new BeatNode(_story, graphNode, beat);

                var b = beat;
                beatNode.EditClicked += () => _editor.OpenGraph<BeatGraph, StoryBeat>(b);

                _graphView.AddNode(beatNode);
            }
        }

        private void RefreshConnections()
        {
            _graphView.ClearEdges();

            if (!string.IsNullOrEmpty(_story.StartingBeat))
            {
                if (_graphView.GetNodeByGUID(_story.StartingBeat) == null)
                {
                    _story.StartingBeat = string.Empty;
                    _editor.SetAssetDirty();
                }
                else
                {
                    _graphView.LinkNodes(StoryStartNode.GUID, _story.StartingBeat);
                }
            }

            foreach (var connection in _story.Connections)
            {
                var output = connection.key;
                var input = connection.value;

                try
                {
                    _graphView.LinkNodes(output, input);
                }
                catch
                {

                }
            }
        }

        private void NewQuestCallback()
        {
            StoryBeat beat = new StoryBeat(Guid.NewGuid().ToString());

            _story.AddBeat(beat);

            _editor.SetAssetDirty();

            Refresh();
        }

        private string SerializeGraphElementsCallback(IEnumerable<GraphElement> elements)
        {
            SerializedStoryGraph serialized;
            serialized.nodes = new();
            serialized.connections = new();
            foreach (var element in elements)
            {
                if (element is BasicNode node)
                {
                    if (element is ISerializable serializable)
                    {
                        serialized.nodes.Add(new()
                        {
                            position = element.GetPosition().position,
                            type = element.GetType().FullName,
                            guid = node.GUID,
                            data = serializable.Serialize()
                        });
                    }
                }
                else if (element is Edge edge)
                {
                    serialized.connections.Add(new()
                    {
                        outputNodeGuid = ((BasicNode)edge.output.node).GUID,
                        outputPortIndex = edge.output.node.Query<Port>().ToList().IndexOf(edge.output),
                        inputNodeGuid = ((BasicNode)edge.input.node).GUID,
                        inputPortIndex = edge.input.node.Query<Port>().ToList().IndexOf(edge.input),
                    });
                }

            }
            return JsonUtility.ToJson(serialized);
        }

        private void UnserializePasteCallback(string operationName, string data)
        {
            _graphView.ClearSelection();

            SerializedStoryGraph copyData = JsonUtility.FromJson<SerializedStoryGraph>(data);
            if (copyData.nodes == null) return;
            List<string> elementsToSelect = new List<string>();

            Dictionary<string, BasicNode> oldGuidToNewNodeMap = new();
            foreach (var node in copyData.nodes)
            {
                var type = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()).FirstOrDefault(t => t.FullName == node.type);
                BasicNode newNode = null;

                if (type == typeof(BeatNode))
                {
                    StoryBeat newBeat = new StoryBeat(Guid.NewGuid().ToString());
                    newBeat.Deserialize(node.data);
                    newBeat.ResetGUIDs();

                    _story.AddBeat(newBeat);

                    var graphNode = _editor.Graph.FindNodeByGuid(newBeat.Guid, out var isNew);
                    graphNode.position = node.position - copyData.nodes[0].position + (Vector2)_graphView.GetViewportCenter();

                    newNode = new BeatNode(_story, graphNode, newBeat);
                    var b = newBeat;
                    ((BeatNode)newNode).EditClicked += () => _editor.OpenGraph<BeatGraph, StoryBeat>(b);
                    _graphView.AddNode(newNode);
                }

                if (newNode != null)
                {
                    oldGuidToNewNodeMap[node.guid] = newNode;
                    elementsToSelect.Add(newNode.GUID);
                }
            }

            foreach (var connection in copyData.connections)
            {
                if (oldGuidToNewNodeMap.TryGetValue(connection.outputNodeGuid, out BasicNode newOutputNode) &&
                    oldGuidToNewNodeMap.TryGetValue(connection.inputNodeGuid, out BasicNode newInputNode))
                {
                    Port outputPort = newOutputNode.Query<Port>().AtIndex(connection.outputPortIndex);
                    Port inputPort = newInputNode.Query<Port>().AtIndex(connection.inputPortIndex);

                    if (outputPort != null && inputPort != null)
                    {
                        Edge edge = _graphView.LinkNodes(outputPort, inputPort);
                        _story.Connections[edge.output.viewDataKey] = edge.input.viewDataKey;
                    }
                }
            }

            _editor.SetAssetDirty();
            Refresh();

            _graphView.schedule.Execute(() =>
            {
                foreach (var element in elementsToSelect)
                {
                    _graphView.AddToSelection(_graphView.GetNodeByGUID(element));
                }
            });
        }
    }
}
