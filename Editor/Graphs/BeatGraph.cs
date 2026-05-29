using Moths.Graphs.Editor;
using Moths.Stories;
using Moths.Stories.Editor;
using Moths.Stories.Editor.Graphs.Nodes;
using Moths.Stories.Editor.VisualElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Moths.Stories.Editor.Graphs
{
    [System.Serializable]
    public struct SerializedBeatGraph
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
            public string concreteType;
            public string guid;
            public Vector2 position;
            public string data;
        }

        public List<SerializedNode> nodes;
        public List<SerializedConnection> connections;
    }

    public class BeatGraph : BaseGraph<StoryBeat>, IRefreshable
    {
        private StoryCreator _editor;
        private StoryBeat _beat;

        private Sidebar _sidebar;

        public override void Initialize(StoryCreator editor, StoryBeat data)
        {
            _editor = editor;
            _beat = data;

            _sidebar = new Sidebar();
            _sidebar.title = data.Name;

            _graphView.EdgeCreated += EdgeCreatedCallback;
            _graphView.EdgeRemoved += EdgeRemovedCallback;
            _graphView.NodeSelected += NodeSelectedCallback;
            _graphView.NodeUnselected += NodeUnselectedCallback;
            _graphView.NodeRemoved += NodeRemovedCallback;

            _graphView.serializeGraphElements = SerializeGraphElementsCallback;
            _graphView.canPasteSerializedData = data => true;
            _graphView.unserializeAndPaste = UnserializePasteCallback;

            this.Add(_sidebar);

            var beatOutcomes = _sidebar.AddCategory("OUTCOMES");

            Button newOutcomeBtn = new Button();
            newOutcomeBtn.AddToClassList("new-quest-btn");
            newOutcomeBtn.text = "New Outcome";
            newOutcomeBtn.clicked += NewOutcomeCallback;

            beatOutcomes.Add(newOutcomeBtn);

            CreateBeatActionsList();

            Refresh();
        }

        private void EdgeCreatedCallback(Edge edge)
        {
            if (edge.output.node is StartNode<ActionNode>)
            {
                _beat.StartingAction = edge.input.viewDataKey;
                _editor.SetAssetDirty();
                return;
            }


            _beat.ActionMappings[edge.output.viewDataKey] = edge.input.viewDataKey;
            _editor.SetAssetDirty();
        }

        private void EdgeRemovedCallback(Edge edge)
        {
            if (edge.output.node is StartNode<BeatNode>)
            {
                _beat.StartingAction = string.Empty;
                _editor.SetAssetDirty();
                return;
            }

            _beat.ActionMappings.Remove(edge.output.viewDataKey);
            _editor.SetAssetDirty();
        }

        public void Refresh()
        {
            RefreshNodes();
            RefreshConnections();
        }

        private void RefreshNodes()
        {
            _graphView.ClearNodes();

            var startNode = new StartNode<ActionNode>("Beat Start");
            _graphView.AddNode(startNode);

            _beat.CleanInvalidActions();

            foreach (var action in _beat.Actions)
            {
                var graphNode = _editor.Graph.FindNodeByGuid(action.Value.Guid, out var isNew);
                if (isNew) graphNode.position = _graphView.GetViewportCenter();
                var actionNode = new ActionNode(_editor.Story, _beat, graphNode, action);
                _graphView.AddNode(actionNode);
                actionNode.PortsUpdated += RefreshConnections;
            }

            foreach (var outcome in _beat.Outcomes)
            {
                var graphNode = _editor.Graph.FindNodeByGuid(outcome.guid, out var isNew);
                if (isNew) graphNode.position = _graphView.GetViewportCenter();
                var outcomeNode = new OutcomeNode(_editor.Story, _beat, graphNode, outcome);
                _graphView.AddNode(outcomeNode);
                outcomeNode.NodeMoved += OutcomeNodeMovedCallback;
            }
        }

        private void RefreshConnections()
        {
            _graphView.ClearEdges();

            if (!string.IsNullOrEmpty(_beat.StartingAction))
            {
                if (_graphView.GetNodeByGUID(_beat.StartingAction) == null)
                {
                    _beat.StartingAction = string.Empty;
                    _editor.SetAssetDirty();
                }
                else
                {
                    _graphView.LinkNodes(StartNode<ActionNode>.GUID, _beat.StartingAction);
                }
            }

            foreach (var connection in _beat.ActionMappings)
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

        private void CreateBeatActionsList()
        {
            var beatActions = _sidebar.AddCategory("ACTIONS");

            var types = TypeCache.GetTypesWithAttribute<StoryActionAttribute>().ToList();
            types.Sort((x, y) => x.Name.CompareTo(y.Name));

            foreach (var type in types)
            {
                var attr = type.GetCustomAttribute<StoryActionAttribute>();
                string path = attr != null ? attr.path : type.Name;
                string name = path.Split('/').Last();

                Button newActionBtn = new Button();
                newActionBtn.AddToClassList("action-btn");
                newActionBtn.clicked += () => AddAction(type);

                if (!string.IsNullOrEmpty(attr.img))
                {
                    newActionBtn.iconImage = Resources.Load<Texture2D>("Moths.StoryCreator/" + attr.img);
                    newActionBtn.tooltip = name;
                }
                else
                {
                    newActionBtn.text = name;
                }

                beatActions.Content.Add(newActionBtn);
            }
        }


        private void NewActionCallback()
        {
            GenericMenu menu = new GenericMenu();
            var types = TypeCache.GetTypesWithAttribute<StoryActionAttribute>();

            foreach (var type in types)
            {
                var attr = type.GetCustomAttribute<StoryActionAttribute>();
                string path = attr != null ? attr.path : type.Name;
                menu.AddItem(new GUIContent(path), false, () => AddAction(type));
            }

            menu.ShowAsContext();
        }

        private void AddAction(Type type)
        {
            var action = (StoryAction)Activator.CreateInstance(type);
            var attr = type.GetCustomAttribute<StoryActionAttribute>();
            string name = attr != null ? attr.path : type.Name;

            action.Initialize(Guid.NewGuid().ToString(), name);
            _beat.AddAction(action);
            _editor.SetAssetDirty();
            Refresh();
        }


        private void NewOutcomeCallback()
        {
            _beat.AddOutcome();
            _editor.SetAssetDirty();
            Refresh();
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
        private void NodeRemovedCallback(UnityEditor.Experimental.GraphView.Node node)
        {
            if (node is OutcomeNode outcomeNode)
            {
                _beat.RemoveOutcome(outcomeNode.GUID);
                _editor.SetAssetDirty();
                Refresh();
            }
            else if (node is ActionNode actionNode)
            {
                _beat.RemoveAction(actionNode.GUID);

                if (_beat.StartingAction == actionNode.GUID) _beat.StartingAction = string.Empty;
                
                _editor.SetAssetDirty();
                Refresh();
            }
        }


        private void OutcomeNodeMovedCallback(Vector2 vector)
        {
            var positions = new Dictionary<string, float>();
            _graphView.Query<OutcomeNode>().ForEach(outcome => positions[outcome.GUID] = -outcome.GetPosition().position.y);
            _beat.SortOutcomes((a, b) => positions[b.guid].CompareTo(positions[a.guid]));
            _editor.SetAssetDirty();
        }

        private string SerializeGraphElementsCallback(IEnumerable<GraphElement> elements)
        {
            SerializedBeatGraph serialized;
            serialized.nodes = new();
            serialized.connections = new();
            foreach (var element in elements)
            {
                if (element is BasicNode node)
                {
                    if (element is ISerializable serializable)
                    {
                        string concreteType = string.Empty;
                        if (element is ActionNode actionNode) concreteType = actionNode.Action.GetType().AssemblyQualifiedName;

                        serialized.nodes.Add(new()
                        {
                            position = element.GetPosition().position,
                            type = element.GetType().FullName,
                            concreteType = concreteType,
                            guid = node.GUID,
                            data = serializable.Serialize()
                        });
                    }
                }
                else if (element is Edge edge)
                {
                    if (edge.output == null || edge.input == null) continue;
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

            SerializedBeatGraph copyData = JsonUtility.FromJson<SerializedBeatGraph>(data);
            if (copyData.nodes == null) return;
            List<string> elementsToSelect = new List<string>();

            Dictionary<string, BasicNode> oldGuidToNewNodeMap = new();
            foreach (var node in copyData.nodes)
            {
                var type = Type.GetType(node.type);
                BasicNode newNode = null;

                if (type == typeof(ActionNode))
                {
                    var actionType = Type.GetType(node.concreteType);
                    if (actionType != null)
                    {
                        var action = (StoryAction)Activator.CreateInstance(actionType);
                        JsonUtility.FromJsonOverwrite(node.data, action);
                        action.ResetGUIDs();

                        _beat.AddAction(action);

                        var graphNode = _editor.Graph.FindNodeByGuid(action.Guid, out var isNew);
                        graphNode.position = node.position - copyData.nodes[0].position + (Vector2)_graphView.GetViewportCenter();

                        newNode = new ActionNode(_editor.Story, _beat, graphNode, action);
                        _graphView.AddNode(newNode);
                        ((ActionNode)newNode).PortsUpdated += RefreshConnections;
                    }
                }
                else if (type == typeof(OutcomeNode))
                {
                    _beat.AddOutcome();
                    var outcome = _beat.Outcomes.Last();
                    var oldOutcomeData = JsonUtility.FromJson<BeatOutcome>(node.data);
                    outcome.name = oldOutcomeData.name;
                    _beat.UpdateOutcome(outcome);

                    var graphNode = _editor.Graph.FindNodeByGuid(outcome.guid, out var isNew);
                    graphNode.position = node.position - copyData.nodes[0].position + (Vector2)_graphView.GetViewportCenter();

                    newNode = new OutcomeNode(_editor.Story, _beat, graphNode, outcome);
                    _graphView.AddNode(newNode);
                    ((OutcomeNode)newNode).NodeMoved += OutcomeNodeMovedCallback;
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
                        _beat.ActionMappings[edge.output.viewDataKey] = edge.input.viewDataKey;
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
