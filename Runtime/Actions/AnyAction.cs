using UnityEngine;

namespace Moths.Stories.Actions
{
    [StoryAction("Any Action")]
    public class AnyAction : StoryAction
    {
        [SerializeField] int _count;

        protected override Output[] GenerateOutputs()
        {
            var outputs = new Output[_count + 1];
            for (int i = 0; i < outputs.Length - 1; i++)
            {
                outputs[i] = new Output(i.ToString());
            }
            outputs[outputs.Length - 1] = new Output("Then");
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
            for (int i = 0; i < _count; i++)
            {
                var output = GetOutputByID(i.ToString());
                if (beatContext.completedActions.Contains(beat.ActionMappings[output.guid])) return GetOutputByID("Then");
            }

            return default;
        }

        public override void CleanUp(StoryBeat beat, StoryContext.BeatContext beatContext)
        {
        }

    }
}