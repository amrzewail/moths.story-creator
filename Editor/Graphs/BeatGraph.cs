using Moths.Graphs.Editor;
using Moths.Stories;
using Moths.Stories.Editor;
using Moths.Stories.Editor.Graphs.Nodes;
using Moths.Stories.Editor.VisualElements;
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Moths.Stories.Editor.Graphs
{
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

    }
}