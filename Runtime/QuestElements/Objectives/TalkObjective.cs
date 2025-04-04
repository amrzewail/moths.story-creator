using Moths.StoryCreator.Attributes;
using UnityEngine;

namespace Moths.StoryCreator.QuestElements.Objectives
{

    [QuestElementInfo("OBJECTIVES", "Talk", "icons/obj_talk")]
    public class TalkObjective : QuestObjective
    {
        public override bool IsRunning { get; protected set; }

        public string talkerName;
        public Vector3 talkPosition;

        public TalkObjective()
        {
        }

        public override void Start()
        {
            IsRunning = true;

            Debug.Log("START TALK OBJECTIVE");
        }

        public override void Update()
        {
            Debug.Log("UPDATE TALK OBJECTIVE");
        }
    }

}