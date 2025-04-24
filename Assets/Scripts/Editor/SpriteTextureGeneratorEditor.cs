using UnityEditor;
using UnityEngine;
using System.IO;

namespace ACH.Utilities.Editor
{
    public class SpriteTextureGeneratorEditor : EditorWindow
    {
        private Material _targetMaterial;

        private void OnGUI()
        {
            _targetMaterial = (Material)EditorGUILayout.ObjectField("Target Material", _targetMaterial, typeof(Material), false);


            if (GUILayout.Button("Generate textures!"))
            {
                var texture = _targetMaterial.GetTexture("_MainTex");
                var path = AssetDatabase.GetAssetPath(texture);
                var folderPath = Path.GetDirectoryName(path);
                var assets = AssetDatabase.FindAssets("t:Texture2D", new[] { folderPath });

                foreach (var frame in assets)
                {
                    var frameTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(frame));
                    _targetMaterial.SetTexture("_MainTex", frameTexture);
                    var outputTexture = new Texture2D(texture.width, texture.height, TextureFormat.ARGB32, false);
                    
                    RenderTexture target = new(frameTexture.width, frameTexture.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                    Graphics.Blit(outputTexture, _targetMaterial, -1);
                    RenderTexture.active = target;
                    outputTexture.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0, false);
                    var outputPath = Path.Combine(folderPath, Path.GetFileName(frameTexture.name + "RecolourAndOrEmissionMap.png"));
                    File.WriteAllBytes(outputPath, outputTexture.EncodeToPNG());
                    //break;
                }
            }


            //GUILayout.Label("Narrative Sequence Editor", EditorStyles.boldLabel);
            //_currentNarrativeSequence = (NarrativeSequence)EditorGUILayout.ObjectField("Target",
            //    _currentNarrativeSequence, typeof(NarrativeSequence), false);

            //if (_currentNarrativeSequence is null)
            //{
            //    _currentChoicesValue = false;
            //    _firstRead = true;
            //    _predefinedVariables = null;
            //    return;
            //}

            //if (_firstRead)
            //{
            //    _firstRead = false;
            //    if (_currentNarrativeSequence.Choices.Count > 0)
            //    {
            //        _currentChoicesValue = true;
            //    }
            //}

            //_predefinedVariables = (PredefinedVariables)EditorGUILayout.ObjectField(
            //    "Predefined Variables", _predefinedVariables, typeof(PredefinedVariables),
            //    false);

            //GUILayout.Label("Source Script", EditorStyles.label);
            //var previousSourceScript = _currentNarrativeSequence.SourceScript;
            //_currentNarrativeSequence.SourceScript = GUILayout.TextArea(_currentNarrativeSequence.SourceScript,
            //    GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            //var sourceScriptChanged = _currentNarrativeSequence.SourceScript != previousSourceScript;

            //_currentChoicesValue = GUILayout.Toggle(_currentChoicesValue, "Has Choices");

            //bool nextNarrativeSequenceModified = false;
            //if (_currentChoicesValue)
            //{
            //    for (var index = 0; index < _currentNarrativeSequence.Choices.Count; index++)
            //    {
            //        var choice = _currentNarrativeSequence.Choices[index];
            //        GUILayout.BeginHorizontal();
            //        GUILayout.Label($"Choice text {index}");
            //        var previousText = choice.ChoiceText;
            //        choice.ChoiceText = GUILayout.TextField(choice.ChoiceText);
            //        GUILayout.EndHorizontal();
            //        var previousResponse = choice.NarrativeResponse;
            //        choice.NarrativeResponse = (NarrativeSequence)EditorGUILayout.ObjectField(
            //            "Narrative Response", choice.NarrativeResponse, typeof(NarrativeSequence),
            //            false);
            //        if (previousText != choice.ChoiceText || previousResponse != choice.NarrativeResponse)
            //        {
            //            nextNarrativeSequenceModified = true;
            //        }
            //    }

            //    if (GUILayout.Button("Add new"))
            //    {
            //        _currentNarrativeSequence.Choices.Add(new());
            //        nextNarrativeSequenceModified = true;
            //    }

            //    if (GUILayout.Button("Remove last") && _currentNarrativeSequence.Choices.Count > 0)
            //    {
            //        _currentNarrativeSequence.Choices.RemoveAt(_currentNarrativeSequence.Choices.Count - 1);
            //        nextNarrativeSequenceModified = true;
            //    }

        }

        [MenuItem("Window / ACH Utilities / Sprite Texture Generator")]
        public static void ShowEditor()
        {
            var window = EditorWindow.GetWindow<SpriteTextureGeneratorEditor>(title: "Sprite Texture Generator");
            window.minSize = new Vector2(500, 500);
        }
    }
}