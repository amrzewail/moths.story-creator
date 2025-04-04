using Moths.StoryCreator.Attributes;
using UnityEngine;

namespace Moths.StoryCreator.QuestElements.Objectives
{

    [QuestElementInfo("OBJECTIVES", "Kill", "icons/obj_kill")]
    public class KillObjective : QuestObjective
    {
        public override bool IsRunning { get; protected set; }

        public string talkerName;
        public Vector3 talkPosition;

        public KillObjective()
        {
            Ports = new()
            {
                Port.New("Success"),
                Port.New("Failure"),
            };
        }

        public override void Start()
        {
            IsRunning = true;

            Debug.Log("START KILL OBJECTIVE");
        }

        public override void Update()
        {
            Debug.Log("UPDATE KILL OBJECTIVE");
        }
    }

}