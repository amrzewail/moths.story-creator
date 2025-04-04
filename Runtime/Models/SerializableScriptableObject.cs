using System;
using UnityEngine;

namespace Moths.StoryCreator.Models
{
    [System.Serializable]
    public class SerializableScriptableObject
    {
        [Serializable]
        public struct SerializedScriptableObject
        {
            public string type;
            public string json;

            public Type Type => Type.GetType(type);
        }

        public SerializedScriptableObject serialized;

        private ScriptableObject _loadedObject;

        public Type Type => serialized.Type;

        public T GetValue<T>() where T : ScriptableObject
        {
            return (T)GetValue(typeof(T));
        }

        public ScriptableObject GetValue(Type type)
        {
            if (!_loadedObject) Load();
            return _loadedObject;
        }

        public static SerializableScriptableObject New<T>() where T : ScriptableObject
        {
            return New(typeof(T));
        }
        public static SerializableScriptableObject New(Type t)
        {
            SerializableScriptableObject serializable = new();
            serializable.Save(ScriptableObject.CreateInstance(t));
            return serializable;
        }

        public void Load()
        {
            if (!_loadedObject) _loadedObject = ScriptableObject.CreateInstance(serialized.Type);
            JsonUtility.FromJsonOverwrite(serialized.json, _loadedObject);
        }

        public void Save(ScriptableObject obj)
        {
            serialized.json = JsonUtility.ToJson(obj);
            serialized.type = obj.GetType().FullName;
            _loadedObject = obj;
        }

        public void Change<T>(Action<T> change) where T : ScriptableObject
        {
            T obj = GetValue<T>();
            change(obj);
            Save(obj);
        }
    }
}