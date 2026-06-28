using UnityEngine;

namespace Moths.Stories.Samples
{
    [System.Serializable]
    public class StoryTimedStarter : IStoryStarter
    {
        [SerializeField] float _startTime = 5;

        public bool CanStart()
        {
            return Time.time > _startTime;
        }
    }

    public abstract class ActionBase : StoryAction
    {
        [SerializeField] int serparentNumber;
        public int parentNumber;
    }

    [System.Serializable]
    [StoryAction("test action child")]
    public class TestActionChild : ActionBase
    {
        public override string Description => "ACtion child";

        public string stringValue;

        public override void CleanUp(StoryBeat beat, StoryContext.BeatContext beatContext)
        {
            throw new System.NotImplementedException();
        }

        public override void Prepare(StoryBeat beat, StoryContext.BeatContext beatContext)
        {
            throw new System.NotImplementedException();
        }

        public override ActionOutput Run(StoryBeat beat, StoryContext.BeatContext beatContext)
        {
            throw new System.NotImplementedException();
        }

        protected override Output[] GenerateOutputs()
        {
            return new Output[] { new("Then") };
        }
    }


    [System.Serializable]
    [StoryAction("test action")]
    public class TestAction : StoryAction
    {
        public override string Description => "TEst Action description";

        public int number;

        public override void CleanUp(StoryBeat beat, StoryContext.BeatContext beatContext)
        {
            throw new System.NotImplementedException();
        }

        public override void Prepare(StoryBeat beat, StoryContext.BeatContext beatContext)
        {
            throw new System.NotImplementedException();
        }

        public override ActionOutput Run(StoryBeat beat, StoryContext.BeatContext beatContext)
        {
            throw new System.NotImplementedException();
        }

        protected override Output[] GenerateOutputs()
        {
            return new Output[] { new ("Then") };
        }
    }
}