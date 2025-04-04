using Moths.StoryCreator.Editor.Graphs;
using Moths.StoryCreator.Editor.VisualElements;
using Moths.StoryCreator.ScriptableObjects;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;

namespace Moths.StoryCreator.Editor
{
    using Editor = UnityEditor.Editor;

    public class StoryEditor : EditorWindow
    {
        private HistoryStack _history;

        private VisualElement _graphs;
        private StoryGraph _storyGraph;

        private Story _story;

        public HistoryStack History => _history;
        public GraphObject CurrentGraph{ get; private set; }

        [MenuItem("Story Creator/Create Story")]
        public static void CreateStoryMenu()
        {
            Story story = ScriptableObject.CreateInstance<Story>();
            story.name = "New Story";
            string path = EditorUtility.SaveFilePanelInProject("Create a story", "New Story", "asset", "");
            AssetDatabase.CreateAsset(story, path);
            AssetDatabase.Refresh();
        }

        protected virtual void OnEnable()
        {
            _graphs = new();
            _graphs.StretchToParentSize();
            _history = new();

            rootVisualElement.Add(_graphs);
            rootVisualElement.Add(_history);

        }

        public void OpenGraph<T>(GraphObject data) where T : BaseGraph
        {
            T t = Activator.CreateInstance<T>();
            t.Initialize(this, data);
            _graphs.Clear();
            _graphs.Add(t);
            History.AddToStack(data.Title, () =>
            {
                _graphs.Clear();
                _graphs.Add(t);
            });
        }

        [OnOpenAsset]
        public static bool OpenStoryAsset(int instanceID, int line)
        {
            // This gets called whenever ANY asset is double clicked
            // So we gotta check if the asset is of the proper type
            UnityEngine.Object asset = EditorUtility.InstanceIDToObject(instanceID);
            if (!(asset is Story)) return false;

            StoryEditor window = EditorWindow.CreateWindow<StoryEditor>();

            window.OpenGraph<StoryGraph>((Story)asset);

            return true;
        }
    }

}