using StoryCreator.Attributes;
using UnityEngine;

namespace StoryCreator.QuestElements.Objectives
{

    [QuestElementInfo("OBJECTIVES", "Goto", "icons/obj_goto")]
    public class GotoObjective : QuestObjective
    {
        public override bool IsRunning { get; protected set; }

        public string talkerName;
        public Vector3 talkPosition;

        public GotoObjective()
        {
        }

        public override void Start()
        {
            IsRunning = true;

            Debug.Log("START GOTO OBJECTIVE");
        }

        public override void Update()
        {
            Debug.Log("UPDATE GOTO OBJECTIVE");
        }
    }

}