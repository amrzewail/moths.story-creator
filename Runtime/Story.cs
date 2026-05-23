using Moths.Serialization;
using Moths.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Moths.Stories
{
    public struct StoryContext
    {
        public struct BeatContext
        {
            public string beatGuid;
            public HashSet<string> currentActions;
        }

        public BeatContext currentBeat;
    }

    [CreateAssetMenu(menuName = "Moths/StoryCreator/Story")]
    public class Story : ScriptableObject
    {
        [SerializeField] string _guid;
        [SerializeField] string _name;
        [SerializeField] string _description;
        
        [SerializeField] InterfaceReference<IStoryStarter> _starter;
        [SerializeField] string _startingBeat;
        [SerializeField] List<StoryBeat> _beats;
        [SerializeField] SerializableDictionary<string, string> _outcomesMapping;

        public string Guid => _guid;
        public IStoryStarter Starter => _starter.Value;

    
        public void StartStory(ref StoryContext context)
        {
            StartBeat(ref context, _startingBeat);
        }

        public void Run(ref StoryContext context)
        {
            if (string.IsNullOrEmpty(context.currentBeat.beatGuid)) return;

            var beat = FindBeat(context.currentBeat.beatGuid);

            if (beat == null)
            {
                Debug.LogError("[Story] Current beat not found.");
                return;
            }

            var outcome = beat.Run(this, context.currentBeat);
        
            if (!string.IsNullOrEmpty(outcome.guid))
            {
                var nextBeat = _outcomesMapping[outcome.guid];
                StartBeat(ref context, nextBeat);
            }
        }

        public void StartBeat(ref StoryContext context, string beatGuid)
        {
            var beat = FindBeat(_startingBeat);

            if (beat == null)
            {
                Debug.LogError("[Story] Beat not found.");
                return;
            }

            context.currentBeat = new()
            {
                beatGuid = beatGuid,
                currentActions = new() { beat.StartingAction }
            };
        }

        public StoryBeat FindBeat(string guid)
        {
            for (int i = 0; i < _beats.Count; i++)
            {
                var beat = _beats[i];

                if (beat.Guid != guid) continue;

                return beat;
            }

            return null;
        }
    }
}