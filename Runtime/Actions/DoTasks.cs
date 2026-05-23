using Moths.Serialization;
using UnityEngine;

namespace Moths.Stories.Actions
{
    [StoryAction("Do Tasks")]
    public class DoTasks : StoryAction
    {
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
                _tasks[i].Value.Do();
            }

            return GetOutputByID("Then");
        }
    }
}