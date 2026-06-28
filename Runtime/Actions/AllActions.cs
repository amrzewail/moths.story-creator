using UnityEngine;

namespace Moths.Stories.Actions
{
    [StoryAction("All Actions", icon: "icons/act_all")]
    [System.Serializable]
    public class AllActions : StoryAction
    {
        [SerializeField] int _count;

        public override string Description => "";

        protected override Output[] GenerateOutputs()
        {
            var outputs = new Output[_count + 1];
            outputs[0] = new Output("Then");
            for (int i = 0; i < outputs.Length - 1; i++)
            {
                outputs[i + 1] = new Output(i.ToString());
            }
            return outputs;
        }

        public override void Prepare(StoryBeat beat, StoryContext.BeatContext beatContext)
        {
            for (int i = 0; i < _count; i++)
            {
                var output = GetOutputByID(i.ToString());
                beat.StartAction(beat.ActionMappings[output.guid], beatContext);
            }
        }

        public override ActionOutput Run(StoryBeat beat, StoryContext.BeatContext beatContext)
        {
            int completedCount = 0;
            for (int i = 0; i < _count; i++)
            {
                var output = GetOutputByID(i.ToString());
                if (beatContext.completedActions.Contains(beat.ActionMappings[output.guid])) completedCount++;
            }

            if (completedCount == _count) return GetOutputByID("Then");

            return default;
        }

        public override void CleanUp(StoryBeat beat, StoryContext.BeatContext beatContext)
        {
        }

    }
}