
using StoryCreator.Models;
using System.Collections.Generic;
using UnityEngine;

namespace StoryCreator.QuestElements
{
    public abstract class QuestObjective : QuestElement
    {
        [HideInInspector]
        public List<SerializableScriptableObject> Behaviours = new();
    }
}