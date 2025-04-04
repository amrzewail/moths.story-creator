using Moths.StoryCreator.Attributes;
using UnityEngine;

namespace Moths.StoryCreator.QuestElements.Objectives
{

    [QuestElementInfo("OBJECTIVES", "Escort", "icons/obj_escort")]
    public class EscortObjective : QuestObjective
    {
        public override bool IsRunning { get; protected set; }

        public string talkerName;
        public Vector3 talkPosition;


        public EscortObjective()
        {
            Ports = new()
            {
                new() { name = "Success" },
                new() { name = "Too far" },
                new() { name = "Died" },
            };
        }


        public override void Start()
        {
            IsRunning = true;

            Debug.Log("START ESCORT OBJECTIVE");
        }

        public override void Update()
        {
            Debug.Log("UPDATE ESCORT OBJECTIVE");
        }
    }

}