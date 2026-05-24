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
}