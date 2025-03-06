using StoryCreator.Models;
using System.Collections.Generic;
using UnityEngine;
using StoryCreator.Attributes;

namespace StoryCreator.ScriptableObjects
{
    public abstract class GraphObject : ScriptableObject
    {
        [System.Serializable]
        public struct Port
        {
            public string name;

            [ReadOnly]
            public string guid;
            public string inputGuid;

            public static Port New(string name)
            {
                return new()
                {
                    name = name,
                    guid = System.Guid.NewGuid().ToString(),
                };
            }
        }

        [SerializeField] string _title = "New Element";
        [SerializeField] List<SerializableScriptableObject> _graphElements = new List<SerializableScriptableObject>();

        public string Title => _title;

        public IReadOnlyList<SerializableScriptableObject> Elements => _graphElements;

        //[HideInInspector]
        [ReadOnly]
        public string Guid;
        [HideInInspector]
        public Vector2 Position;
        //[HideInInspector]
        public List<Port> Ports;

        public GraphObject()
        {
            Guid = System.Guid.NewGuid().ToString();
        }

        public void AddElement(System.Type element)
        {
            SerializableScriptableObject serializable = new();
            serializable.Save(ScriptableObject.CreateInstance(element));
            _graphElements.Add(serializable);
        }

        public void RemoveElement(string elementGuid)
        {
            for (int i = 0; i < _graphElements.Count; i++)
            {
                var element = _graphElements[i].GetValue<GraphObject>();
                if (element.Guid == elementGuid)
                {
                    _graphElements.RemoveAt(i);
                    break;
                }
            }
        }

        public int GetPortIndex(string portGuid)
        {
            for (int i = 0; i < Ports.Count; i++)
            {
                if (Ports[i].guid == portGuid) return i;
            }
            return -1;
        }

        public void SetPortInputGuid(string portGuid, string inputGuid)
        {
            int portIndex = GetPortIndex(portGuid);
            if (portIndex == -1) return;
            var port = Ports[portIndex];
            port.inputGuid = inputGuid;
            Ports[portIndex] = port;
        }
    }
}