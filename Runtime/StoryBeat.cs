using Moths.Collections;
using Moths.Serialization;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Moths.Stories
{
    [System.Serializable]
    public struct BeatOutcome
    {
        public string name;
        public string guid;
    }


    [System.Serializable]
    public class StoryBeat : INameable
    {
        [SerializeField] string _guid;
        [SerializeField] string _name;
        [SerializeField] string _startingAction;
        [SerializeField] List<InterfaceReference<StoryAction>> _actions = new List<InterfaceReference<StoryAction>>();
        [SerializeField] List<BeatOutcome> _outcomes = new List<BeatOutcome>();
        [SerializeField] SerializableDictionary<string, string> _actionMappings;

        public string Name { get => _name; set => _name = value; }
        public string Guid => _guid;
        public string StartingAction { get => _startingAction; set => _startingAction = value; }
        public IReadOnlyList<BeatOutcome> Outcomes => _outcomes;
        public IReadOnlyList<InterfaceReference<StoryAction>> Actions => _actions;
        public SerializableDictionary<string, string> ActionMappings => _actionMappings;

        public StoryBeat(string guid)
        {
            _guid = guid;
            _name = "New Beat";
        }

        public BeatOutcome Run(Story story, StoryContext.BeatContext context)
        {
            for (int i = 0; i < _outcomes.Count; i++)
            {
                var outcome = _outcomes[i];
                if (!context.currentActions.Contains(outcome.guid)) continue;
                foreach (var actionGuid in context.currentActions)
                {
                    var currAction = FindAction(actionGuid);
                    if (currAction == null) continue;
                    currAction.CleanUp(this, context);
                }
                return outcome;
            }

            for (int i = 0; i < _actions.Count; i++)
            {
                var action = _actions[i].Value;

                if (!context.currentActions.Contains(action.Guid)) continue;

                var output = action.Run(this, context);

                if (string.IsNullOrEmpty(output.guid)) continue;

#if UNITY_EDITOR
                Debug.Log($"[Story] Complete action {Name}: {action.Name}");
#endif
                action.CleanUp(this, context);
                context.currentActions.Remove(action.Guid);
                context.completedActions.Add(action.Guid);

                if (!_actionMappings.ContainsKey(output.guid)) continue;

                var nextGuid = _actionMappings[output.guid];

                var nextBeat = FindOutcome(nextGuid);
                if (!string.IsNullOrEmpty(nextBeat.guid))
                {
                    foreach(var actionGuid in context.currentActions)
                    {
                        var currAction = FindAction(actionGuid);
                        if (currAction == null) continue;
                        currAction.CleanUp(this, context);
                    }
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

#if UNITY_EDITOR
                Debug.Log($"[Story] Start action {Name}: {action.Name}");
#endif
                context.currentActions.Add(actionGuid);
                action.Prepare(this, context);
            }
        }

        public void AddAction(StoryAction action)
        {
            _actions.Add(new InterfaceReference<StoryAction>(action));
        }

        public void RemoveAction(string guid)
        {
            for (int i = 0; i < _actions.Count; i++)
            {
                if (_actions[i].Value.Guid == guid)
                {
                    _actions.RemoveAt(i);
                    return;
                }
            }
        }

        public void AddOutcome()
        {
            _outcomes.Add(new BeatOutcome
            {
                guid = System.Guid.NewGuid().ToString(),
                name = "New Outcome"
            });
        }

        public void RemoveOutcome(string guid)
        {
            for (int i = 0; i < _outcomes.Count; i++)
            {
                if (_outcomes[i].guid == guid)
                {
                    _outcomes.RemoveAt(i);
                    return;
                }
            }
        }

        public void UpdateOutcome(BeatOutcome outcome)
        {
            for (int i = 0; i < _outcomes.Count; i++)
            {
                if (_outcomes[i].guid == outcome.guid)
                {
                    _outcomes[i] = outcome;
                    return;
                }
            }
        }

        private StoryAction FindAction(string actionGuid)
        {
            for (int i = 0; i < _actions.Count; i++)
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