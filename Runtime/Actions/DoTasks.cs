using Moths.Serialization;
using System.Collections.Generic;
using UnityEngine;

namespace Moths.Stories.Actions
{
    [StoryAction("Do Tasks", icon: "icons/act_do_tasks")]
    [System.Serializable]
    public class DoTasks : StoryAction
    {
        public override string Description
        {
            get
            {
                if (_tasks == null) return string.Empty;
                List<string> descs = new();
                foreach(var task in _tasks)
                {
                    if (task.Value == null) continue;
                    descs.Add(task.Value.Description);
                }
                return string.Join("\n", descs);
            }
        }

        [SerializeField] InterfaceReference<ITask>[] _tasks;

        protected override Output[] GenerateOutputs()
        {
            return new Output[] { new("Then") };
        }

        public override void Prepare(StoryBeat beat, StoryContext.BeatContext beatContext)
        {

        }


        public override void CleanUp(StoryBeat beat, StoryContext.BeatContext beatContext)
        {
        }

        public override ActionOutput Run(StoryBeat beat, StoryContext.BeatContext beatContext)
        {
            for (int i = 0; i < _tasks.Length; i++)
            {
                _tasks[i].Value.Execute();
            }

            return GetOutputByID("Then");
        }
    }
}