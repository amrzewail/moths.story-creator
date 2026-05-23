using Moths.Collections;
using Moths.Serialization;
using System.Collections.Generic;
using UnityEngine;

namespace Moths.Stories
{
    [System.Serializable]
    public struct BeatOutcome
    {
        public string guid;
        public string name;
    }


    [System.Serializable]
    public class StoryBeat
    {
        [SerializeField] string _guid;
        [SerializeField] string _name;
        [SerializeField] string _startingAction;
        [SerializeField] InterfaceReference<StoryAction>[] _actions;
        [SerializeField] List<BeatOutcome> _outcomes;
        [SerializeField] SerializableDictionary<string, string> _actionMappings;
        
        public string Guid => _guid;
        public string StartingAction => _startingAction;


        public BeatOutcome Run(Story story, StoryContext.BeatContext context)
        {
            for (int i = 0; i < _actions.Length; i++)
            {
                var action = _actions[i].Value;

                if (!context.currentActions.Contains(action.Guid)) continue;

                var outcome = action.Run();

                if (string.IsNullOrEmpty(outcome.guid)) continue;

                action.CleanUp();
                context.currentActions.Remove(action.Guid);

                var nextGuid = _actionMappings[outcome.guid];

                var nextBeat = FindOutcome(nextGuid);
                if (!string.IsNullOrEmpty(nextBeat.guid))
                {
                    foreach(var actionGuid in context.currentActions)
                    {
                        var currAction = FindAction(actionGuid);
                        currAction.CleanUp();
                    }
                    context.currentActions.Clear();

                    return nextBeat;
                }

                var nextAction = nextGuid;
                StartAction(nextAction, context);
            }

            return default;
        }

        public void StartAction(string actionGuid, StoryContext.BeatContext context)
        {
            if (!context.currentActions.Contains(actionGuid))
            {
                var action = FindAction(actionGuid);

                if (action == null)
                {
                    Debug.LogError($"[StoryBeat] Failed to find action");
                    return;
                }

                context.currentActions.Add(actionGuid);
                action.Prepare();
            }
        }

        private StoryAction FindAction(string actionGuid)
        {
            for (int i = 0; i < _actions.Length; i++)
            {
                var action = _actions[i].Value;
                if (action.Guid == actionGuid) return action;
            }
            return null;
        }

        private BeatOutcome FindOutcome(string guid)
        {
            for (int i = 0; i < _outcomes.Count; i++)
            {
                if (_outcomes[i].guid == guid) return _outcomes[i];
            }
            return default;

        }
    }
}