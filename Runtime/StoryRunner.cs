using System.Collections.Generic;
using UnityEngine;

namespace Moths.Stories
{
    public class StoryRunner : MonoBehaviour
    {
        [SerializeField] Story[] _stories;

        private Dictionary<Story, StoryContext> _contexts = new();
        private HashSet<string> _completedStories = new();

        private void Update()
        {
            for (int i = 0; i < _stories.Length; i++)
            {
                var story = _stories[i];
                
                if (_completedStories.Contains(story.Guid)) continue;

                if (_contexts.TryGetValue(story, out var context))
                {
                    var outcome = story.Run(ref context);
                    _contexts[story] = context;

                    if (outcome == StoryOutcome.Complete)
                    {
                        _completedStories.Add(story.Guid);
                    }
                }
                else
                {
                    if (story.Starter == null || story.Starter.CanStart())
                    {
                        context = new StoryContext();
                        story.StartStory(ref context);
                        _contexts[story] = context;
                    }
                }
            }
        }
    }
}
