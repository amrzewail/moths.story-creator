using Scripts.Graphs.Editor;
using StoryCreator.Editor.Graphs.Nodes;
using StoryCreator.Editor.VisualElements;
using StoryCreator.ScriptableObjects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace StoryCreator.Editor.Graphs
{
    public class StoryGraph : BaseGraph
    {
        private StoryEditor _editor;
        private Story _story;
        private Button _newQuestBtn;
        private VisualElement _questList;

        public override void Initialize(StoryEditor editor, GraphObject data)
        {
            _editor = editor;
            _story = (Story)data;

            Sidebar sidebar = new Sidebar();
            sidebar.title = "QUESTS LIST";

            Button newQuestBtn = new Button();
            newQuestBtn.AddToClassList("new-quest-btn");
            newQuestBtn.text = "NEW QUEST";
            newQuestBtn.clicked += NewQuestCallback;

            _questList = new VisualElement();
            _questList.AddToClassList("quest-list");

            sidebar.Content.Add(_questList);
            sidebar.Content.Add(newQuestBtn);

            _graphView.EdgeCreated += EdgeCreatedCallback;
            _graphView.EdgeRemoved += EdgeRemovedCallback;

            this.Add(sidebar);

            Refresh();
        }


        private void EdgeCreatedCallback(Edge edge)
        {

            if (edge.output.node is StartNode<QuestNode>)
            {
                typeof(Story).GetField("_startingQuest", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(_story, ((QuestNode)edge.input.node).Quest);
                EditorUtility.SetDirty(_story);
                AssetDatabase.SaveAssets();
            }

            if (edge.output.node is QuestNode)
            {
                var quest = ((QuestNode)edge.output.node).Quest;
                quest.SetPortInputGuid(edge.output.viewDataKey, ((QuestNode)edge.input.node).GUID);
                EditorUtility.SetDirty(quest);
            }

        }
        private void EdgeRemovedCallback(Edge edge)
        {
            if (edge.output.node is StartNode<QuestNode>)
            {
                typeof(Story).GetField("_startingQuest", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(_story, null);
                EditorUtility.SetDirty(_story);
                AssetDatabase.SaveAssets();
            }

            if (edge.output.node is QuestNode)
            {
                var quest = ((QuestNode)edge.output.node).Quest;
                quest.SetPortInputGuid(edge.output.viewDataKey, "");
                EditorUtility.SetDirty(quest);
            }
        }

        public void Refresh()
        {
            _questList.Clear();

            var quests = _story.Quests;
            if (quests == null) return;
            for (int i = 0; i < quests.Count; i++)
            {
                _questList.Add(CreateQuestButtonVisualElement(quests[i]));
            }

            RefreshNodes();
            RefreshConnections();
        }


        private void RefreshNodes()
        {
            _graphView.ClearNodes();

            StartNode<QuestNode> startNode = new();
            _graphView.AddNode(startNode);

            foreach(var quest in _story.Quests)
            {
                QuestNode questNode = new QuestNode(quest);
                _graphView.AddNode(questNode);
            }

        }

        private void RefreshConnections()
        {
            _graphView.ClearEdges();

            if (_story.StartingQuest)
            {
                _graphView.LinkNodes(StartNode<QuestNode>.OUTPUT_GUID, _story.StartingQuest.Guid);
            }

            foreach (var quest in _story.Quests)
            {
                if (quest.Ports == null) continue;
                for (int i = 0; i < quest.Ports.Count; i++)
                {
                    var port = quest.Ports[i];
                    if (string.IsNullOrEmpty(port.inputGuid)) continue;
                    _graphView.LinkNodes(port.guid, port.inputGuid);
                }
            }
        }

        private VisualElement CreateQuestButtonVisualElement(Quest quest)
        {
            VisualElement element = new();
            element.AddToClassList("quest-btn");
            element.Add(new Button(() =>
            {
                _editor.OpenGraph<QuestGraph>(quest);
            })
            {
                text = quest.Title,
            });
            return element;
        }

        private void NewQuestCallback()
        {
            Quest newQuest = ScriptableObject.CreateInstance<Quest>();
            newQuest.name = "New Quest";
            AssetDatabase.AddObjectToAsset(newQuest, _story);
            EditorUtility.SetDirty(newQuest);

            List<Quest> quests = (List<Quest>)typeof(Story).GetField("_quests", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(_story);

            quests.Add(newQuest);

            EditorUtility.SetDirty(_story);

            AssetDatabase.SaveAssets();

            Refresh();
        }

        private void SaveStoryObject()
        {
            EditorUtility.SetDirty(_story);
            AssetDatabase.SaveAssetIfDirty(_story);
        }

    }
}