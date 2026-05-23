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
            public HashSet<string> completedActions;
        }

        public BeatContext currentBeat;
    }

    public enum StoryOutcome
    {
        Pending,
        Complete
    };

    [CreateAssetMenu(menuName = "Moths/StoryCreator/Story")]
    public class Story : ScriptableObject, INameable
    {
        [SerializeField, HideInInspector] string _guid;
        [SerializeField] string _name;
        [SerializeField, TextArea(2, 5)] string _description;
        [SerializeField] InterfaceReference<IStoryStarter> _starter;

        [SerializeField, HideInInspector] string _startingBeat;
        [SerializeField, HideInInspector] List<StoryBeat> _beats;
        [SerializeField, HideInInspector] SerializableDictionary<string, string> _connections;

        public string Name => _name;
        public string Description => _description;
        public string Guid => _guid;
        public string StartingBeat { get => _startingBeat; set => _startingBeat = value; }
        public IStoryStarter Starter => _starter.Value;
        public IReadOnlyList<StoryBeat> Beats => _beats;
        public SerializableDictionary<string, string> Connections => _connections;

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(_guid))
            {
                _guid = System.Guid.NewGuid().ToString();
            }
        }

        public void StartStory(ref StoryContext context)
        {
            StartBeat(ref context, _startingBeat);
        }

        public StoryOutcome Run(ref StoryContext context)
        {
            if (string.IsNullOrEmpty(context.currentBeat.beatGuid)) return StoryOutcome.Pending;

            var beat = FindBeat(context.currentBeat.beatGuid);

            if (beat == null)
            {
                Debug.LogError("[Story] Current beat not found.");
                return StoryOutcome.Pending;
            }

            var outcome = beat.Run(this, context.currentBeat);

            if (!string.IsNullOrEmpty(outcome.guid))
            {
                var nextBeat = _connections[outcome.guid];

                if (nextBeat == "__STORY_END__")
                {
                    return StoryOutcome.Complete;
                }

                StartBeat(ref context, nextBeat);
            }

            return StoryOutcome.Pending;
        }

        public void StartBeat(ref StoryContext context, string beatGuid)
        {
            var beat = FindBeat(beatGuid);

            if (beat == null)
            {
                Debug.LogError("[Story] Beat not found.");
                return;
            }


            if (context.currentBeat.currentActions == null) context.currentBeat.currentActions = new();
            if (context.currentBeat.completedActions == null) context.currentBeat.completedActions = new();

            context.currentBeat.beatGuid = beatGuid;

            context.currentBeat.currentActions.Clear();
            context.currentBeat.completedActions.Clear();

            context.currentBeat.currentActions.Add(beat.StartingAction);
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

        public void AddBeat(StoryBeat beat)
        {
            _beats.Add(beat);
        }

        public void RemoveBeat(string guid)
        {
            for (int i = 0; i < _beats.Count; i++)
            {
                if (_beats[i].Guid == guid)
                {
                    _beats.RemoveAt(i);
                    return;
                }
            }
        }
    }
}