using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StoryCreator.ScriptableObjects
{

    public class Story : GraphObject
    {

        [SerializeField] List<Quest> _quests = new List<Quest>();
        [SerializeField] Quest _startingQuest;

        public IReadOnlyList<Quest> Quests => _quests;

        public Quest StartingQuest => _startingQuest;


        public void Start()
        {
            if (!_startingQuest) return;
            _startingQuest.Start();
        }

    }
}