using StoryCreator.Attributes;
using UnityEngine;

namespace StoryCreator.QuestElements.Objectives
{

    [QuestElementInfo("OBJECTIVES", "Give", "icons/obj_give")]
    public class GiveObjective : QuestObjective
    {
        public override bool IsRunning { get; protected set; }

        public string talkerName;
        public Vector3 talkPosition;


        public override void Start()
        {
            IsRunning = true;

            Debug.Log("START GIVE OBJECTIVE");
        }

        public override void Update()
        {
            Debug.Log("UPDATE GIVE OBJECTIVE");
        }
    }

}