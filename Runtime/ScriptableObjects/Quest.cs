using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using StoryCreator.Models;
using StoryCreator.QuestElements.Objectives;
using StoryCreator.Attributes;

namespace StoryCreator.ScriptableObjects
{//
    public class Quest : GraphObject
    {

        [ReadOnly]
        public string startGuid;

        public Quest()
        {
            Ports = new()
            {
                Port.New("Success"),
                Port.New("Failure"),
            };
        }

        private void OnValidate()
        {
            if (name != Title)
            {
                name = Title;
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
                UnityEditor.AssetDatabase.SaveAssetIfDirty(this);
#endif
            }
        }

        public void Start()
        {

        }

    }
}