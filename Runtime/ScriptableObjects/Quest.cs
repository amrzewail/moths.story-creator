using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Moths.StoryCreator.Models;
using Moths.StoryCreator.QuestElements.Objectives;
using Moths.StoryCreator.Attributes;

namespace Moths.StoryCreator.ScriptableObjects
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