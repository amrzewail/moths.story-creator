using Moths.Serialization;
using UnityEngine;

namespace Moths.Stories.Actions
{
    public class DoTasks : StoryAction
    {
        [SerializeField] InterfaceReference<ITask>[] _tasks;

        public override string[] GenerateOutcomes()
        {
            return new string[] { "Then" };
        }

        public override void Prepare()
        {
        }


        public override void CleanUp()
        {
        }

        public override ActionOutcome Run()
        {
            for (int i = 0; i < _tasks.Length; i++)
            {
                _tasks[i].Value.Do();
            }

            return GetOutcomeByID("Then");
        }
    }
}