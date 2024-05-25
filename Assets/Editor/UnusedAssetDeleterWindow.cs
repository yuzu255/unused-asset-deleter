using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;


namespace UnusedAssetDeleter
{
    public class UnusedAssetDeleterWindow : EditorWindow
    {
        private static List<TogglableAsset> _detectedAssets = new List<TogglableAsset>();
        
        private static List<string> _assetTypes = new List<string>
        {
            "t:Scene",
            "t:Prefab",
            "t:Texture",
            "t:Material",
            "t:Mesh",
            "t:Animation",
            "t:AnimatorController",
            "t:AudioClip",
            "t:Shader",
            "t:Script",
            "t:ScriptableObject",
            "t:Sprite",
            "t:Font",
            "t:GUISkin",
            "t:VideoClip",
            "t:RenderTexture",
            "t:ComputeShader",
            "t:ShaderVariantCollection",
            "t:LightingSettings",
            "t:Terrain",
            "t:LightmapParameters",
            "t:NavMeshData",
            "t:TimelineAsset",
            "t:PlayableAsset",
            "t:AvatarMask",
            "t:UnityEditor.DefaultAsset"
        };
        private static List<string> _excludedAssetTypes = new List<string>
        {
            "t:Scene",
            "t:Script",
            "t:ScriptableObject"
        };
        private static string[] _searchInFolders = new string[] { "Assets" };


        private static Vector2 _windowSize = new Vector2(600, 400);
        private Vector2 _scrollPos = Vector2.zero;

        [MenuItem("Tool/UnusedAssetDeleter")]
        private static void ShowWindow()
        {
            // initialize members
            _detectedAssets.Clear();

            // find assets
            var assets = new List<Asset>();
            var targetAssets = new List<Asset>();
            foreach(var assetType in _assetTypes)
            {
                var typedAssets = AssetDatabase.FindAssets(assetType, _searchInFolders)
                    .Where(guid => !assets.Any(asset => asset.Guid == guid))
                    .Select(guid => Asset.CreateByGuid(guid))
                    .ToList();
                assets.AddRange(typedAssets);
                if (!_excludedAssetTypes.Contains(assetType))
                {
                    targetAssets.AddRange(typedAssets);
                }
            }

            // get asset dependencies
            var dependencyDict = new Dictionary<Asset, List<Asset>>();
            for (int i = 0; i < assets.Count; i++)
            {
                var asset = assets[i];
                dependencyDict.Add(asset, AssetDatabase.GetDependencies(asset.Path).Select(Asset.CreateByPath).ToList());
                EditorUtility.DisplayCancelableProgressBar("Finding references", asset.Path, (float)i / assets.Count);
            }

            // detect unused assets
            for (int i = 0; i < targetAssets.Count; i++)
            {
                var asset = targetAssets[i];
                bool contained = false;

                foreach (var group in dependencyDict.Where(pair => !asset.Equals(pair.Key)))
                {
                    if (group.Value.Any(value => asset.Equals(value)))
                    {
                        contained = true;
                        break;
                    }
                }   
                if (!contained)
                {
                    _detectedAssets.Add(new TogglableAsset(asset, true));
                }
                EditorUtility.DisplayCancelableProgressBar("Detecting unused assets", asset.Path, (float)i / targetAssets.Count);
            }
            EditorUtility.ClearProgressBar();

            // get window
            var window = GetWindow<UnusedAssetDeleterWindow>();
            window.maxSize = window.minSize = _windowSize;
        }

        void OnGUI()
        {
            // inform the number of detected assets
            EditorGUILayout.LabelField($"{_detectedAssets.Count} unused assets are detected.");
            
            // arrange toggles
            _scrollPos = GUILayout.BeginScrollView(_scrollPos);
            _detectedAssets.ForEach(asset => asset.Toggle = EditorGUILayout.ToggleLeft(asset.Asset.Path, asset.Toggle));
            GUILayout.EndScrollView();

            // arrange toggle on/off in one batch buttons
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("All", GUILayout.Width(60)))
            {
                _detectedAssets.ForEach(asset => asset.Toggle = true);
            }
            if (GUILayout.Button("None", GUILayout.Width(60)))
            {
                _detectedAssets.ForEach(asset => asset.Toggle = false);
            }
            EditorGUILayout.Space();

            // arrange delete button
            if (GUILayout.Button("Delete", GUILayout.Width(60)))
            {
                var selectedAssets = _detectedAssets.Where(asset => asset.Toggle).ToList();
                selectedAssets.ForEach(asset => {
                    AssetDatabase.DeleteAsset(asset.Asset.Path);
                    _detectedAssets.Remove(asset);
                });
            }
            GUILayout.EndHorizontal();
        }
    }

    class TogglableAsset
    {
        public Asset Asset {  get; private set; }
        public bool Toggle {  get; set; }

        public TogglableAsset(Asset asset, bool toggle)
        {
            Asset = asset;
            Toggle = toggle;
        }
    }
}

