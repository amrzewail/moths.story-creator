using StoryCreator.Attributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StoryCreator.QuestElements.Behaviours
{
    [QuestElementInfo("BEHAVIOURS", "Animate", "icons/beh_animate")]
    public class AnimateBehaviour : QuestBehaviour
    {
        public override bool IsRunning { get => throw new System.NotImplementedException(); protected set => throw new System.NotImplementedException(); }

        public AnimationState animation;

        public override void Start()
        {
            throw new System.NotImplementedException();
        }

        public override void Update()
        {
            throw new System.NotImplementedException();
        }

        public override void CleanUp()
        {
            throw new System.NotImplementedException();
        }

    }

}