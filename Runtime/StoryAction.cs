using Moths.Serialization;
using System.Collections.Generic;
using UnityEngine;

namespace Moths.Stories
{
    [System.Serializable]
    public struct ActionOutcome
    {
        public string id;
        public string name;

        public string guid;
    }

    [System.Serializable]
    public abstract class StoryAction
    {
        [SerializeField] string _guid;
        [SerializeField] string _name;

        [SerializeField] ActionOutcome[] _outcomes;

        public string Guid => _guid;

        public abstract string[] GenerateOutcomes();

        public abstract void Prepare();
        public abstract ActionOutcome Run();
        public abstract void CleanUp();

        protected ActionOutcome GetOutcomeByID(string id)
        {
            for (int i = 0; i < _outcomes.Length; i++)
            {
                if (_outcomes[i].id == id) return _outcomes[i];
            }
            return default;
        }
    }
}