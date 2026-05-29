using Moths.Serialization;
using System.Collections.Generic;
using UnityEngine;

namespace Moths.Stories
{
    [System.Serializable]
    public struct ActionOutput
    {
        public string id;
        public string name;

        public string guid;
    }

    public class StoryActionAttribute : System.Attribute
    {
        public string path { get; private set; }
        public string img { get; private set; }

        public StoryActionAttribute(string path, string icon = null)
        {
            this.path = path;
            this.img = icon;
        }
    }

    [System.Serializable]
    public abstract class StoryAction
    {
        public struct Output
        {
            public string id;
            public string name;

            public Output(string id, string name)
            {
                this.id = id;
                this.name = name;
            }

            public Output(string id) : this(id, id) { }
        }

        [SerializeField] string _guid;
        [SerializeField] string _name;

        [SerializeField] ActionOutput[] _outputs;

        public string Guid => _guid;
        public string Name { get => _name; set => _name = value; }
        public abstract string Description { get; }

        public IReadOnlyList<ActionOutput> Outputs => _outputs;

        public void Initialize(string guid, string name)
        {
            _guid = guid;
            _name = name;

            UpdateOutputs();
        }

        public void UpdateOutputs()
        {
            var outputs = GenerateOutputs();
            if (outputs == null) outputs = new Output[0];

            var newOutputs = new ActionOutput[outputs.Length];
            for (int i = 0; i < outputs.Length; i++)
            {
                string guid = GetOutputByID(outputs[i].id).guid;

                newOutputs[i] = new ActionOutput
                {
                    id = outputs[i].id,
                    name = outputs[i].name,
                    guid = string.IsNullOrEmpty(guid) ? System.Guid.NewGuid().ToString() : guid
                };
            }

            _outputs = newOutputs;
        }

        protected abstract Output[] GenerateOutputs();

        public abstract void Prepare(StoryBeat beat, StoryContext.BeatContext beatContext);
        public abstract ActionOutput Run(StoryBeat beat, StoryContext.BeatContext beatContext);
        public abstract void CleanUp(StoryBeat beat, StoryContext.BeatContext beatContext);

        protected ActionOutput GetOutputByID(string id)
        {
            if (_outputs == null) return default;
            for (int i = 0; i < _outputs.Length; i++)
            {
                if (_outputs[i].id == id) return _outputs[i];
            }
            return default;
        }

        public Dictionary<string, string> ResetGUIDs()
        {
            Dictionary<string, string> mapping = new Dictionary<string, string>();
            
            string oldGuid = _guid;
            _guid = System.Guid.NewGuid().ToString();
            mapping[oldGuid] = _guid;

            Dictionary<string, string> oldOutputGuids = new Dictionary<string, string>();
            if (_outputs != null)
            {
                foreach (var output in _outputs)
                {
                    oldOutputGuids[output.id] = output.guid;
                }
            }

            _outputs = null;
            UpdateOutputs();

            if (_outputs != null)
            {
                foreach (var output in _outputs)
                {
                    if (oldOutputGuids.ContainsKey(output.id))
                    {
                        mapping[oldOutputGuids[output.id]] = output.guid;
                    }
                }
            }

            return mapping;
        }
    }
}
