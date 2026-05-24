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

namespace Moths.Stories.Editor.Graphs
{
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
            if (edge.output.node is StoryStartNode)
            {
                _story.StartingBeat = string.Empty;
                EditorUtility.SetDirty(_story);
                return;
            }

            _story.Connections.Remove(edge.output.viewDataKey);
            EditorUtility.SetDirty(_story);
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

    }
}