
using Moths.StoryCreator.Models;
using System.Collections.Generic;
using UnityEngine;

namespace Moths.StoryCreator.QuestElements
{
    public abstract class QuestObjective : QuestElement
    {
        [HideInInspector]
        public List<SerializableScriptableObject> Behaviours = new();
    }
}