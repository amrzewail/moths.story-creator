using Scripts.Graphs.Editor;
using StoryCreator.Editor.VisualElements;
using StoryCreator.ScriptableObjects;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace StoryCreator.Editor.Graphs
{
    public abstract class BaseGraph : BasicGraphVisualElement
    {

        public abstract void Initialize(StoryEditor editor, GraphObject data);

    }
}