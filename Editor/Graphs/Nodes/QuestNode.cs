using Moths.Graphs.Editor;
using Moths.StoryCreator.Attributes;
using Moths.StoryCreator.Models;
using Moths.StoryCreator.QuestElements;
using Moths.StoryCreator.ScriptableObjects;
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
    public class QuestNode : BasicNode
    {
        private Quest _quest;

        public Quest Quest => _quest;

        public QuestNode(Quest quest) 
        {
            _quest = quest;
            base.title = quest.Title;
            base.GUID = quest.Guid;

            base.position = quest.Position;
        }

        public override bool IsCopiable() => false;

        public override void UpdatePresenterPosition()
        {
            base.UpdatePresenterPosition();

            _quest.Position = GetPosition().position;
            EditorUtility.SetDirty(_quest);
        }

        private VisualElement GeneratePort(string portGuid)
        {
            var port = _quest.Ports[_quest.GetPortIndex(portGuid)];
            var p = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(QuestNode));
            p.portName = "";
            //p.Remove(p.Q<Label>("type"));
            p.viewDataKey = port.guid;

            TextField nameField = new TextField();
            nameField.value = port.name;
            nameField.RegisterValueChangedCallback(ev =>
            {
                port.name = ev.newValue;
                _quest.Ports[_quest.GetPortIndex(port.guid)] = port;
                EditorUtility.SetDirty(_quest);
            });
            p.Add(nameField);

            var removeBtn = new Button(() =>
            {
                int index = _quest.GetPortIndex(port.guid);
                _quest.Ports.RemoveAt(index);
                outputContainer.RemoveAt(index);
                EditorUtility.SetDirty(_quest);
            });
            removeBtn.text = "X";

            p.Add(removeBtn);

            return p;
        }

        public override void GeneratePorts()
        {
            inputContainer.Clear();
            outputContainer.Clear();
            extensionContainer.Clear();

            var entryPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(QuestNode));
            entryPort.portName = "Entry";
            entryPort.viewDataKey = _quest.Guid;
            inputContainer.Add(entryPort);

            if (_quest.Ports != null)
            {
                foreach (var port in _quest.Ports)
                {
                    var p = GeneratePort(port.guid);
                    outputContainer.Add(p);
                }
            }

            var addOutcomeBtn = new Button(() =>
            {
                var port = GraphObject.Port.New("New outcome");
                _quest.Ports.Add(port);
                outputContainer.Insert(outputContainer.childCount - 1, GeneratePort(port.guid));
                EditorUtility.SetDirty(_quest);
            });
            addOutcomeBtn.text = "Add Outcome";

            outputContainer.Add(addOutcomeBtn);

        }

    }
}