using Moths.StoryCreator.ScriptableObjects;
using UnityEngine;

namespace Moths.StoryCreator.QuestElements
{
    public abstract class QuestElement : GraphObject
    {
        public abstract bool IsRunning { get; protected set; }

        public abstract void Start();

        public abstract void Update();
    }
}