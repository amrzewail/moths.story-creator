using Moths.Graphs.Editor;
using Moths.StoryCreator.Editor.VisualElements;
using Moths.StoryCreator.ScriptableObjects;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Moths.StoryCreator.Editor.Graphs
{
    public abstract class BaseGraph : BasicGraphVisualElement
    {

        public abstract void Initialize(StoryEditor editor, GraphObject data);

    }
}