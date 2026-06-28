using UnityEngine;

namespace Moths.Stories.Actions
{
    [StoryAction("Any Action", icon: "icons/act_any")]
    [System.Serializable]
    public class AnyAction : StoryAction
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