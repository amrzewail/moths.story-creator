using StoryCreator.Attributes;
using UnityEngine;

namespace StoryCreator.QuestElements.Objectives
{

    [QuestElementInfo("OBJECTIVES", "Collect", "icons/obj_collect")]
    public class CollectObjective : QuestObjective
    {
        public override bool IsRunning { get; protected set; }

        public CollectObjective()
        {
            Ports = new()
            {
                new Port
                {
                    guid = "success",
                    name = "Success",
                }
            };
        }

        public override void Start()
        {
            IsRunning = true;

            Debug.Log("START COLLECT OBJECTIVE");
        }

        public override void Update()
        {
            Debug.Log("UPDATE COLLECT OBJECTIVE");
        }
    }

}