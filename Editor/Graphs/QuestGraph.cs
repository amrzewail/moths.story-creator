using Scripts.Graphs.Editor;
using StoryCreator.Attributes;
using StoryCreator.Editor.Graphs.Nodes;
using StoryCreator.Editor.VisualElements;
using StoryCreator.Models;
using StoryCreator.QuestElements;
using StoryCreator.ScriptableObjects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace StoryCreator.Editor.Graphs
{
    using Category = StoryCreator.Editor.VisualElements.Category;
    using GraphObject = StoryCreator.ScriptableObjects.GraphObject;

    public class QuestGraph : BaseGraph
    {
        private Quest _quest;
        private Sidebar _sidebar;
        private Button _newQuestBtn;

        private Category _objectivesCategory;

        public override void Initialize(StoryEditor editor, GraphObject data)
        {
            styleSheets.Add(Resources.Load<StyleSheet>("StoryCreator/Styles"));

            _quest = (Quest)data;

            _sidebar = new Sidebar();
            _sidebar.title = _quest.Title;

            this.Add(_sidebar);

            _graphView.EdgeCreated += EdgeCreatedCallback;
            _graphView.EdgeRemoved += EdgeRemovedCallback;
            _graphView.NodeRemoved += NodeRemovedCallback;
            Refresh();
        }


        private void EdgeCreatedCallback(Edge edge)
        {
            if (edge.output.node is StartNode<QuestElementNode>)
            {
                _quest.startGuid = ((BasicNode)edge.input.node).GUID;
                SaveQuestObject();
                return;
            }


            var questElement = ((QuestElementNode)edge.output.node).questElement;

            questElement.SetPortInputGuid(edge.output.viewDataKey, ((QuestElementNode)edge.input.node).GUID);

            ((QuestElementNode)edge.output.node).serializableObject.Save(questElement);
            SaveQuestObject();
        }
        private void EdgeRemovedCallback(Edge edge)
        {
            if (edge.output.node is StartNode<QuestElementNode>)
            {
                _quest.startGuid = "";
                SaveQuestObject();
                return;
            }


            var questElement = ((QuestElementNode)edge.output.node).questElement;
            questElement.SetPortInputGuid(edge.output.viewDataKey, "");

            ((QuestElementNode)edge.output.node).serializableObject.Save(questElement);
            SaveQuestObject();
        }
        private void NodeRemovedCallback(Node node)
        {
            _quest.RemoveElement(((QuestElementNode)node).GUID);
            SaveQuestObject();
            RefreshConnections();
        }

        public void Refresh()
        {
            RefreshCategories();
            RefreshNodes();
            RefreshConnections();
        }

        private void RefreshCategories()
        {
            _sidebar.ClearCategories();

            var types = TypeCache.GetTypesWithAttribute<QuestElementInfoAttribute>();

            foreach (var type in types)
            {
                var t = type;
                var attr = type.GetCustomAttribute<QuestElementInfoAttribute>();
                VisualElement element = new VisualElement();
                Image bg = new Image() { image = Resources.Load<Texture>("StoryCreator/icons/quest_element_bg") };
                bg.tintColor = Color.red;
                bg.AddToClassList("background");
                element.Add(bg);
                Image icon = new Image() { image = Resources.Load<Texture>($"StoryCreator/{attr.Icon}") };
                icon.AddToClassList("icon");
                element.Add(icon);
                element.Add(new Label(attr.Name));
                element.AddToClassList("quest-element");
                element.styleSheets.Add(Resources.Load<StyleSheet>("StoryCreator/Styles"));
                element.RegisterCallback<ClickEvent>(ev =>
                {
                    ElementClickCallback(t);
                });
                _sidebar.AddToCategory(attr.Category, element);
            }

        }


        private void RefreshNodes()
        {
            _graphView.ClearNodes();

            StartNode<QuestElementNode> startNode = new();
            _graphView.AddNode(startNode);

            foreach(var element in _quest.Elements)
            {
                QuestElement questElement = element.GetValue<QuestElement>();
                QuestElementNode node = new QuestElementNode(element);
                node.position = questElement.Position;
                _graphView.AddNode(node);
            }
        }

        private void RefreshConnections()
        {
            _graphView.ClearEdges();

            if (!string.IsNullOrEmpty(_quest.startGuid))
            {
                _graphView.LinkNodes(StartNode<QuestElementNode>.OUTPUT_GUID, _quest.startGuid);
            }

            foreach (var element in _quest.Elements)
            {
                QuestElement questElement = element.GetValue<QuestElement>();
                if (questElement.Ports == null) continue;
                for (int i = 0; i < questElement.Ports.Count; i++)
                {
                    var port = questElement.Ports[i];
                    if (string.IsNullOrEmpty(port.inputGuid)) continue;
                    _graphView.LinkNodes(port.guid, port.inputGuid);
                }
            }
        }

        private void ElementClickCallback(Type type)
        {
            Debug.Log(type.Name);
            if (type.BaseType == typeof(QuestBehaviour))
            {
                if (QuestElementNode.currentSelectedNode == null) return;
                SerializableScriptableObject serializableObj = QuestElementNode.currentSelectedNode.serializableObject;
                if (serializableObj.Type.BaseType != typeof(QuestObjective)) return;

                serializableObj.Change((QuestObjective objective) =>
                {
                    var serializableBehaviour = SerializableScriptableObject.New(type);
                    objective.Behaviours.Add(serializableBehaviour);
                });
            }
            else
            {
                _quest.AddElement(type);
            }
            Refresh();
        }

        private void SaveQuestObject()
        {
            EditorUtility.SetDirty(_quest);
            AssetDatabase.SaveAssetIfDirty(_quest);
        }
    }
}