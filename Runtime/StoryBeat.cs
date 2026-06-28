using Moths.Collections;
using Moths.Serialization;
using System;
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
        [SerializeField] List<InterfaceReference<StoryAction>> _actions = new();
        [SerializeField] List<BeatOutcome> _outcomes = new();
        [SerializeField] SerializableDictionary<string, string> _actionMappings = new();

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
                if (context.deadendActions.Contains(action.Guid)) continue;

                var output = action.Run(this, context);

                if (string.IsNullOrEmpty(output.guid)) continue;

#if UNITY_EDITOR
                Debug.Log($"[Story] Complete action {Name}: {action.Name}");
#endif
                action.CleanUp(this, context);
                context.completedActions.Add(action.Guid);

                if (!_actionMappings.ContainsKey(output.guid))
                {
#if UNITY_EDITOR
                    Debug.Log($"[Story] Action {Name}: {action.Name} deadend");
#endif
                    context.deadendActions.Add(action.Guid);
                    continue;
                }

                context.currentActions.Remove(action.Guid);

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

        public void StartAction(string actionGuid, StoryContext.BeatContext context, bool forcePlay = false)
        {
            if (!context.currentActions.Contains(actionGuid) || forcePlay)
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
                if (!context.currentActions.Contains(actionGuid))
                {
                    context.currentActions.Add(actionGuid);
                }

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

        public void CleanInvalidActions()
        {
            for (int i = _actions.Count - 1; i >= 0; i--)
            {
                if (_actions[i].Value == null) _actions.RemoveAt(i);
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

        public void SortOutcomes(Comparison<BeatOutcome> comparison)
        {
            _outcomes.Sort(comparison);
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

        [System.Serializable]
        private struct BeatSerializationData
        {
            public string guid;
            public string name;
            public string startingAction;
            public List<string> actions;
            public List<BeatOutcome> outcomes;
            public SerializableDictionary<string, string> actionMappings;
        }

        public string Serialize()
        {
            BeatSerializationData data;
            data.guid = _guid;
            data.name = _name;
            data.startingAction = _startingAction;
            data.actions = _actions.Select(a => a.Serialize()).ToList();
            data.outcomes = _outcomes;
            data.actionMappings = _actionMappings;

            return JsonUtility.ToJson(data);
        }

        public void Deserialize(string json)
        {
            BeatSerializationData data = JsonUtility.FromJson<BeatSerializationData>(json);
            _guid = data.guid;
            _name = data.name;
            _startingAction = data.startingAction;
            _actions = data.actions.Select(s =>
            {
                var ir = new InterfaceReference<StoryAction>();
                ir.Deserialize(s);
                return ir;
            }).ToList();
            _outcomes = data.outcomes;
            _actionMappings = new SerializableDictionary<string, string>();
            if (data.actionMappings != null)
            {
                foreach(var pair in data.actionMappings)
                {
                    _actionMappings[pair.key] = pair.value;
                }
            }
        }

        public void ResetGUIDs()
        {
            Dictionary<string, string> mapping = new Dictionary<string, string>();

            string oldBeatGuid = _guid;
            _guid = System.Guid.NewGuid().ToString();
            mapping[oldBeatGuid] = _guid;

            // 1. Outcomes
            for (int i = 0; i < _outcomes.Count; i++)
            {
                string oldO = _outcomes[i].guid;
                string newO = System.Guid.NewGuid().ToString();
                mapping[oldO] = newO;
                var o = _outcomes[i];
                o.guid = newO;
                _outcomes[i] = o;
            }

            // 2. Actions
            foreach (var actionRef in _actions)
            {
                var action = actionRef.Value;
                var actionMapping = action.ResetGUIDs();
                foreach (var pair in actionMapping)
                {
                    mapping[pair.Key] = pair.Value;
                }
            }

            // 3. Update Starting Action
            if (mapping.ContainsKey(_startingAction))
            {
                _startingAction = mapping[_startingAction];
            }

            // 4. Update Action Mappings
            var newMappings = new SerializableDictionary<string, string>();
            foreach (var pair in _actionMappings)
            {
                string key = pair.key;
                string val = pair.value;

                if (mapping.ContainsKey(key)) key = mapping[key];
                if (mapping.ContainsKey(val)) val = mapping[val];

                newMappings[key] = val;
            }
            _actionMappings = newMappings;
        }
    }
}
