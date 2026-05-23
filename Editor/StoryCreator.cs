using Moths.Stories;
using Moths.Stories.Editor.Graphs;
using Moths.Stories.Editor.VisualElements;
using System;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;

namespace Moths.Stories.Editor
{
    using Editor = UnityEditor.Editor;

    public class StoryCreator : EditorWindow
    {
        private HistoryStack _history;

        private VisualElement _graphs;

        private Story _story;
        private StoryGraphProperties _properties;
        private Sidebar _nodeInspector;

        public HistoryStack History => _history;
        public Story Story => _story;
        public StoryGraphProperties Graph => _properties;

        [MenuItem("Moths/Story Creator/Create Story")]
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
            
            _nodeInspector = new();
            _nodeInspector.AddToClassList("right-align");
            _nodeInspector.AddToClassList("inspector");
            _nodeInspector.visible = false;

            rootVisualElement.Add(_graphs);
            rootVisualElement.Add(_history);

            rootVisualElement.Add(_nodeInspector);
        }

        public void OpenGraph<T, TData>(TData data) where T : BaseGraph<TData>
        {
            T t = Activator.CreateInstance<T>();
            t.Initialize(this, data);
            _graphs.Clear();
            _graphs.Add(t);

            History.AddToStack((data as INameable).Name, () =>
            {
                _graphs.Clear();
                _graphs.Add(t);
                if (t is IRefreshable refreshable) refreshable.Refresh();
            });
        }

        public void Inspect(string title, VisualElement element)
        {
            if (element != null)
            {
                _nodeInspector.title = title;

                _nodeInspector.Content.Clear();
                _nodeInspector.Content.Add(element);
                _nodeInspector.visible = true;
                return;
            }

            _nodeInspector.visible = false;
        }


        public void SetAssetDirty()
        {
            EditorUtility.SetDirty(_story);
        }

        [OnOpenAsset]
        public static bool OpenStoryAsset(int instanceID, int line)
        {
            // This gets called whenever ANY asset is double clicked
            // Check if the asset is of the proper type
            UnityEngine.Object asset = EditorUtility.EntityIdToObject(instanceID);
            if (!(asset is Story storyAsset)) return false;

            // 1. Get the path to the main Story asset
            string assetPath = AssetDatabase.GetAssetPath(storyAsset);

            // 2. Load all assets (including sub-assets) at this path
            UnityEngine.Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath);

            StoryGraphProperties graphProperties = null;

            // 3. Check if StoryGraphProperties already exists as a sub-asset
            foreach (var obj in allAssets)
            {
                if (obj is StoryGraphProperties)
                {
                    graphProperties = obj as StoryGraphProperties;
                    break;
                }
            }

            if (graphProperties == null)
            {
                graphProperties = ScriptableObject.CreateInstance<StoryGraphProperties>();
                graphProperties.name = "Graph"; // Sets the name in the Project window

                AssetDatabase.AddObjectToAsset(graphProperties, assetPath);
                AssetDatabase.SaveAssets();
            }

            StoryCreator window = EditorWindow.CreateWindow<StoryCreator>();

            window._properties = graphProperties;
            window._story = storyAsset;

            window.OpenGraph<StoryGraph, Story>(storyAsset);

            return true;
        }
    }

}