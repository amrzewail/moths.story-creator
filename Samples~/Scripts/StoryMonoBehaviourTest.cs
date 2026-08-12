using Moths.Animations;
using Moths.Collections;
using Moths.Dialogues;
using Moths.Serialization;
using System.Collections.Generic;
using UnityEngine;

namespace Moths.Stories.Samples
{
    public abstract class BaseStoryAction : StoryAction
    {
        [SerializeField] string[] _inheritedPreparations = new string[0];
        [SerializeField] InterfaceReference<ITask>[] _preparationTasks;

        protected string CreateDescription(string description)
        {
            var desc = string.Empty;

            List<string> list = new();
            GetPreparationTasksDescription(list);

            desc = string.Join("\n", list);

            if (string.IsNullOrEmpty(desc)) return description;

            return string.Join("\n_____\n", desc, description);
        }

        private void GetPreparationTasksDescription(List<string> list)
        {
            if (_inheritedPreparations != null && _inheritedPreparations.Length > 0)
            {
                foreach (var actionName in _inheritedPreparations)
                {
                    list.Add($"Inherits preparations from <b>{actionName}</b>");
                }
            }

            if (_preparationTasks != null && _preparationTasks.Length > 0)
            {
                InterfaceReference<ITask>[] tasks = _preparationTasks;
                for (int i = 0; i < tasks.Length; i++)
                {
                    InterfaceReference<ITask> interfaceReference = tasks[i];
                    if (interfaceReference.Value != null)
                    {
                        list.Add(interfaceReference.Value.Description);
                    }
                }
            }
        }

        private void CallPreparationTasks(StoryBeat beat, StoryContext.BeatContext beatContext)
        {
            foreach (var actionName in _inheritedPreparations)
            {
                BaseStoryAction action = null;
                foreach (var act in beat.Actions)
                {
                    if (!act) continue;
                    if (act.Value.Name == actionName)
                    {
                        action = act.Value as BaseStoryAction;
                        break;
                    }
                }

                if (action == null) continue;

                action.CallPreparationTasks(beat, beatContext);
            }

            foreach (var task in _preparationTasks)
            {
                task.Value.Execute();
            }
        }

        public override void Prepare(StoryBeat beat, StoryContext.BeatContext beatContext)
        {
            CallPreparationTasks(beat, beatContext);
        }
    }

    [System.Serializable]
    public struct Properties
    {
        public bool toggle;
        public string str;
        public OptionalProperty<AnimatorState> optional;
        [SerializeField] OptionalProperty<LString> _locale;
        public OverrideableProperty<AnimatorState> overridable;
        [SerializeField] OptionalProperty<Dialogue> _optionalDialogue;
    }

    [System.Serializable]
    public class TestTask : ITask
    {
        public string Description => "";

        [SerializeField] bool _toggle;
        [SerializeField] string _string;
        [SerializeField] OptionalProperty<string> _test;
        [SerializeField] OptionalProperty<LString> _locale;
        [SerializeField] OptionalProperty<AnimatorState> _animation;
        [SerializeField] OverrideableProperty<string> _test2;
        [SerializeField] OptionalProperty<Properties> _optionalProps;
        [SerializeField] Dialogue _dialogue; 
        [SerializeField] OptionalProperty<Dialogue> _optionalDialogue; 

        public void Execute()
        {
            throw new System.NotImplementedException();
        }
    }

    [System.Serializable]
    public class StoryMonoBehaviourTest : MonoBehaviour
    {
        [SerializeField] OptionalProperty<string> _testOptional;
        [SerializeField] OptionalProperty<LString> _locale;
        [SerializeField] OverrideableProperty<string> _testOverride;
        [SerializeField] OptionalProperty<Properties> _optionalProps;
        [SerializeField] OptionalProperty<Dialogue> _optionalDialogue;
        [SerializeField] OverrideableProperty<Dialogue> _overrideableDialogue;
        [SerializeField] Dialogue _dialogue;
    }

    public abstract class ActionBase : StoryAction
    {
        [SerializeField] int serparentNumber;
        public int parentNumber;
    }

    [System.Serializable]
    [StoryAction("test action child")]
    public class TestActionChild : BaseStoryAction
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