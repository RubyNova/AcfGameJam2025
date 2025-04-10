using System.Collections.Generic;
using ACHNarrativeDriver.Api;
using ACHNarrativeDriver.ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace ACHNarrativeDriver.Editor
{
    public class NarrativeSequenceEditor : EditorWindow
    {
        private NarrativeSequence _currentNarrativeSequence;
        private readonly Interpreter _interpreter = new();
        private bool _currentChoicesValue = false;
        private bool _firstRead = true;
        private PredefinedVariables _predefinedVariables;

        private void OnGUI()
        {
            GUILayout.Label("Narrative Sequence Editor", EditorStyles.boldLabel);
            _currentNarrativeSequence = (NarrativeSequence)EditorGUILayout.ObjectField("Target",
                _currentNarrativeSequence, typeof(NarrativeSequence), false);

            if (_currentNarrativeSequence is null)
            {
                _currentChoicesValue = false;
                _firstRead = true;
                _predefinedVariables = null;
                return;
            }

            if (_firstRead)
            {
                _firstRead = false;
                if (_currentNarrativeSequence.Choices.Count > 0)
                {
                    _currentChoicesValue = true;
                }
            }

            _predefinedVariables = (PredefinedVariables)EditorGUILayout.ObjectField(
                "Predefined Variables", _predefinedVariables, typeof(PredefinedVariables),
                false);

            var previousSprite = _currentNarrativeSequence.BackgroundSprite;
            _currentNarrativeSequence.BackgroundSprite = (Sprite)EditorGUILayout.ObjectField("Background Image",
                _currentNarrativeSequence.BackgroundSprite, typeof(Sprite), false);
            var backgroundSpriteChanged = _currentNarrativeSequence.BackgroundSprite != previousSprite;
            
            GUILayout.Label("Music files", EditorStyles.label);

            bool musicCollectionModified = false;
            //Hakky hack to get Unity Editor to stop complaining about things that don't exist.
            try{
            for (int index = 0; index < _currentNarrativeSequence.MusicFiles.Count; index++)
            {
                var previousClip = _currentNarrativeSequence.MusicFiles[index];
                _currentNarrativeSequence.MusicFiles[index] =
                    (AudioClip)EditorGUILayout.ObjectField($"Music {index}",
                        _currentNarrativeSequence.MusicFiles[index], typeof(AudioClip), false);

                if (previousClip != _currentNarrativeSequence.MusicFiles[index])
                {
                    musicCollectionModified = true;
                }
            }
            }catch{}
            
            if (GUILayout.Button("Add new"))
            {
                _currentNarrativeSequence.MusicFiles.Add(null);
                musicCollectionModified = true;
            }

            if (GUILayout.Button("Remove last") && _currentNarrativeSequence.MusicFiles.Count > 0)
            {
                _currentNarrativeSequence.MusicFiles.RemoveAt(_currentNarrativeSequence.MusicFiles.Count - 1);
                musicCollectionModified = true;
            }
            
            GUILayout.Label("Music files", EditorStyles.label);

            bool soundEffectCollectionModified = false;

            for (int index = 0; index < _currentNarrativeSequence.SoundEffectFiles.Count; index++)
            {
                var previousClip = _currentNarrativeSequence.SoundEffectFiles[index];
                _currentNarrativeSequence.SoundEffectFiles[index] =
                    (AudioClip)EditorGUILayout.ObjectField($"Sound Effect {index}",
                        _currentNarrativeSequence.SoundEffectFiles[index], typeof(AudioClip), false);

                if (previousClip != _currentNarrativeSequence.SoundEffectFiles[index])
                {
                    soundEffectCollectionModified = true;
                }
            }
            
            if (GUILayout.Button("Add new"))
            {
                _currentNarrativeSequence.SoundEffectFiles.Add(null);
                soundEffectCollectionModified = true;
            }

            if (GUILayout.Button("Remove last") && _currentNarrativeSequence.SoundEffectFiles.Count > 0)
            {
                _currentNarrativeSequence.SoundEffectFiles.RemoveAt(_currentNarrativeSequence.SoundEffectFiles.Count - 1);
                soundEffectCollectionModified = true;
            }

            GUILayout.Label("Source Script", EditorStyles.label);
            var previousSourceScript = _currentNarrativeSequence.SourceScript;
            _currentNarrativeSequence.SourceScript = GUILayout.TextArea(_currentNarrativeSequence.SourceScript,
                GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            var sourceScriptChanged = _currentNarrativeSequence.SourceScript != previousSourceScript;

            _currentChoicesValue = GUILayout.Toggle(_currentChoicesValue, "Has Choices");

            bool nextNarrativeSequenceModified = false;
            if (_currentChoicesValue)
            {
                for (var index = 0; index < _currentNarrativeSequence.Choices.Count; index++)
                {
                    var choice = _currentNarrativeSequence.Choices[index];
                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"Choice text {index}");
                    var previousText = choice.ChoiceText;
                    choice.ChoiceText = GUILayout.TextField(choice.ChoiceText);
                    GUILayout.EndHorizontal();
                    var previousResponse = choice.NarrativeResponse;
                    choice.NarrativeResponse = (NarrativeSequence)EditorGUILayout.ObjectField(
                        "Narrative Response", choice.NarrativeResponse, typeof(NarrativeSequence),
                        false);
                    if (previousText != choice.ChoiceText || previousResponse != choice.NarrativeResponse)
                    {
                        nextNarrativeSequenceModified = true;
                    }
                }

                if (GUILayout.Button("Add new"))
                {
                    _currentNarrativeSequence.Choices.Add(new());
                    nextNarrativeSequenceModified = true;
                }

                if (GUILayout.Button("Remove last") && _currentNarrativeSequence.Choices.Count > 0)
                {
                    _currentNarrativeSequence.Choices.RemoveAt(_currentNarrativeSequence.Choices.Count - 1);
                    nextNarrativeSequenceModified = true;
                }
            }
            else
            {
                _currentNarrativeSequence.Choices.Clear();
                var previousSequence = _currentNarrativeSequence.NextSequence;
                _currentNarrativeSequence.NextSequence = (NarrativeSequence)EditorGUILayout.ObjectField(
                    "Next Narrative Sequence", _currentNarrativeSequence.NextSequence, typeof(NarrativeSequence),
                    false);

                if (previousSequence != _currentNarrativeSequence.NextSequence)
                {
                    nextNarrativeSequenceModified = true;
                }
            }

            bool compiledScriptChanged = false;
            if (GUILayout.Button("Save Source Script"))
            {
                compiledScriptChanged = true;
                var listOfStuff = _interpreter.Interpret(_currentNarrativeSequence.SourceScript, _predefinedVariables, _currentNarrativeSequence.MusicFiles.Count, _currentNarrativeSequence.SoundEffectFiles.Count);
                _currentNarrativeSequence.CharacterDialoguePairs = listOfStuff;

                if (_currentNarrativeSequence.Choices is not null && _predefinedVariables is not null)
                {
                    foreach (var choice in _currentNarrativeSequence.Choices)
                    {
                        choice.ChoiceText =
                            _interpreter.ResolvePredefinedVariables(choice.ChoiceText, _predefinedVariables);
                    }
                }
            }

            if (sourceScriptChanged || musicCollectionModified || soundEffectCollectionModified || nextNarrativeSequenceModified ||
                compiledScriptChanged || backgroundSpriteChanged)
            {
                EditorUtility.SetDirty(_currentNarrativeSequence);
            }
        }

        [MenuItem("Window / ACH Narrative Driver / Narrative Sequence Editor")]
        public static void ShowEditor()
        {
            var window = EditorWindow.GetWindow<NarrativeSequenceEditor>(title: "Narrative Sequence Editor");
            window.minSize = new Vector2(500, 500);
        }
    }
}