using Scripts.Graphs.Editor;
using StoryCreator.Attributes;
using StoryCreator.Models;
using StoryCreator.QuestElements;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace StoryCreator.Editor.Graphs.Nodes
{
    [System.Serializable]
    public class QuestElementNode : BasicNode
    {
        public SerializableScriptableObject serializableObject { get; private set; }
        public static QuestElementNode currentSelectedNode { get; private set; }

        public QuestElement questElement => serializableObject.GetValue<QuestElement>();

        public QuestElementNode(SerializableScriptableObject serializableObject) 
        {
            this.serializableObject = serializableObject;

            var attr = this.serializableObject.serialized.Type.GetCustomAttribute<QuestElementInfoAttribute>();
            base.title = attr.Name;

        }

        public override void OnSelected()
        {
            base.OnSelected();

            currentSelectedNode = this;
        }

        public override void OnUnselected()
        {
            base.OnUnselected();

            if (currentSelectedNode != this) return;
            currentSelectedNode = null;
        }

        public override void UpdatePresenterPosition()
        {
            base.UpdatePresenterPosition();

            var element = serializableObject.GetValue<QuestElement>();
            element.Position = GetPosition().position;
            serializableObject.Save(element);
        }

        public override void GeneratePorts()
        {
            inputContainer.Clear();
            outputContainer.Clear();
            extensionContainer.Clear();

            QuestElement element = serializableObject.GetValue<QuestElement>();

            this.GUID = element.Guid;

            var entryPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(QuestElementNode));
            entryPort.portName = "Entry";
            entryPort.viewDataKey = element.Guid;
            inputContainer.Add(entryPort);

            if (element.Ports != null)
            {
                foreach (var port in element.Ports)
                {
                    var p = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(QuestElementNode));
                    p.portName = port.name;
                    p.viewDataKey = port.guid;
                    outputContainer.Add(p);
                }
            }

            SerializedObject serialized = new SerializedObject(element);
            InspectorElement serializedInspector = new InspectorElement(serialized);

            serializedInspector.TrackSerializedObjectValue(serialized, s =>
            {
                serializableObject.Save((QuestElement)s.targetObject);
            });

            extensionContainer.Add(serializedInspector);
            serializedInspector.AddToClassList("objective-container");

            RefreshBehaviours();
            RefreshExpandedState();
        }


        private void RefreshBehaviours()
        {
            QuestElement element = serializableObject.GetValue<QuestElement>();

            if (element is QuestObjective)
            {
                QuestObjective objective = (QuestObjective)element;

                if (objective.Behaviours != null && objective.Behaviours.Count > 0)
                {
                    var behavioursLabel = new Label("Behaviours");
                    extensionContainer.Add(behavioursLabel);
                    behavioursLabel.AddToClassList("behaviour-label");

                    int i = 0;
                    foreach (var behaviour in objective.Behaviours)
                    {
                        int index = i;
                        VisualElement container = new VisualElement();
                        container.AddToClassList("behaviour-container");

                        VisualElement header = new VisualElement();
                        header.AddToClassList("behaviour-header");

                        Label titleLabel = new Label(behaviour.serialized.Type.GetCustomAttribute<QuestElementInfoAttribute>().Name);
                        titleLabel.AddToClassList("title-label");
                        header.Add(titleLabel);

                        Button deleteButton = new Button(() =>
                        {
                            objective.Behaviours.RemoveAt(index);
                            GeneratePorts();
                        });
                        deleteButton.text = "X";
                        deleteButton.AddToClassList("delete-btn");
                        header.Add(deleteButton);

                        container.Add(header);

                        var beh = behaviour;
                        var serialized = new SerializedObject(beh.GetValue<QuestBehaviour>());
                        var serializedInspector = new InspectorElement(serialized);
                        serializedInspector.TrackSerializedObjectValue(serialized, s =>
                        {
                            beh.Save((ScriptableObject)s.targetObject);
                        });
                        serializedInspector.AddToClassList("behaviour-inspector");


                        container.Add(serializedInspector);

                        extensionContainer.Add(container);

                        i++;
                    }
                }
            }
        }
    }
}