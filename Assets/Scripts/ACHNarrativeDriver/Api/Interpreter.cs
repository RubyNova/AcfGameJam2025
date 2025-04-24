using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ACHNarrativeDriver.ScriptableObjects;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace ACHNarrativeDriver.Api
{
    public class Interpreter
    {
        #if UNITY_EDITOR
        public List<NarrativeSequence.CharacterDialogueInfo> Interpret(string sourceScript,
            PredefinedVariables predefinedVariables, int musicFilesCount, int soundEffectsCount)
        {
            var characterPaths = AssetDatabase.FindAssets("t:Character").Select(AssetDatabase.GUIDToAssetPath);
            var characterAssets = characterPaths.Select(AssetDatabase.LoadAssetAtPath<Character>);
            List<NarrativeSequence.CharacterDialogueInfo> returnList = new();

            // remove any invalid new line strings

            if (sourceScript.Contains("\r"))
            {
                sourceScript = sourceScript.Replace("\r", "\n");
            }

            var sourceSplit = sourceScript.Split("\n", StringSplitOptions.RemoveEmptyEntries);
            Character character = null;
            Character characterTwo = null;

            for (var index = 0; index < sourceSplit.Length; index++)
            {
                //Get singular line
                var line = sourceSplit[index];
                //Split line by : symbol
                var splitLines = line.Split(": ", StringSplitOptions.RemoveEmptyEntries);

                if (splitLines.Length >= 6)
                {
                    throw new FormatException(
                        $"Invalid narrative script was provided to the interpreter. {splitLines.Length} arguments were provided when the maximum is 5. Invalid line number: {index + 1}");
                }

                //Set Characters
                var characterName = splitLines[0];
                string secondCharacterName = string.Empty;

                bool splitLinesLengthLargerThanOne = splitLines.Length > 1;
                bool multipleCharacters = false;

                //Check if character name has two characters with a , between
                if(characterName.Contains(','))
                {
                    multipleCharacters = true;
                    //Split the names
                    var names = characterName.Split(',');
                    //Get the first character name
                    characterName = ResolvePredefinedVariables(names[0], predefinedVariables);
                    
                    if (splitLinesLengthLargerThanOne && !string.IsNullOrWhiteSpace(characterName) &&
                    !characterName.All(char.IsNumber))
                    {
                        character = characterAssets.FirstOrDefault(x =>
                            x.Name.Equals(characterName, StringComparison.OrdinalIgnoreCase));
                    }

                    secondCharacterName = ResolvePredefinedVariables(names[1], predefinedVariables);

                    //Repeat for the second character
                    if (splitLinesLengthLargerThanOne && !string.IsNullOrWhiteSpace(secondCharacterName) &&
                    !secondCharacterName.All(char.IsNumber))
                    {
                        characterTwo = characterAssets.FirstOrDefault(x => 
                            x.Name.Equals(secondCharacterName, StringComparison.OrdinalIgnoreCase));
                    }
                }
                else
                {
                    characterName = ResolvePredefinedVariables(characterName, predefinedVariables);

                    if (splitLinesLengthLargerThanOne && !string.IsNullOrWhiteSpace(characterName) &&
                    !characterName.All(char.IsNumber))
                    {
                        character = characterAssets.FirstOrDefault(x =>
                            x.Name.Equals(characterName, StringComparison.OrdinalIgnoreCase));
                    }
                }

                if (character is null)
                {
                    throw new FileNotFoundException(
                        $"The character {(string.IsNullOrWhiteSpace(characterName) ? "NO_CHARACTER_NAME" : characterName)} cannot be found in the asset database. Please ensure the character has been created and that the name has been spelt correctly. Line number: {index + 1}");
                }

                if (multipleCharacters && characterTwo is null)
                {
                    throw new FileNotFoundException(
                        $"The character {(string.IsNullOrWhiteSpace(secondCharacterName) ? "NO_CHARACTER_NAME" : secondCharacterName)} cannot be found in the asset database. Please ensure the character has been created and that the name has been spelt correctly. Line number: {index + 1}");
                }

                //Set poses
                var posesString = splitLines[1];
                var allPoses = posesString.Split(',');
                var poseIndexString = allPoses.FirstOrDefault(x => x.All(char.IsNumber));
                string secondPoseString = allPoses.Length > 1 ? allPoses[1] : string.Empty;
                
                int? poseIndex = null;
                int? secondPoseIndex = null;

                if (!string.IsNullOrWhiteSpace(poseIndexString))
                {
                    poseIndexString = ResolvePredefinedVariables(poseIndexString, predefinedVariables);
                    poseIndex = int.Parse(poseIndexString);
                }
                
                if (poseIndex >= character.Poses.Count)
                {
                    throw new IndexOutOfRangeException(
                        $"Character Pose Index was outside the bounds of the Poses collection. Length: {character.Poses.Count}, Index: {poseIndex}. Line number: {index + 1}");
                }

                if (poseIndex.HasValue && !string.IsNullOrWhiteSpace(secondPoseString))
                {
                    secondPoseString = ResolvePredefinedVariables(secondPoseString, predefinedVariables);
                    secondPoseIndex = int.Parse(secondPoseString);
                }
                
                if (multipleCharacters && secondPoseIndex >= characterTwo.Poses.Count)
                {
                    throw new IndexOutOfRangeException(
                        $"Character Pose Index was outside the bounds of the Poses collection. Length: {characterTwo.Poses.Count}, Index: {poseIndex}. Line number: {index + 1}");
                }            

                //Set directional bubble
                bool leftSpeakerTalking = true;
                bool narratorSpeaking = false;
                if(multipleCharacters && splitLines.Length > 3)
                {
                    var charName = splitLines[2];
                    if(!string.IsNullOrWhiteSpace(charName))
                    {
                        if(charName == "Narrator")
                        {
                            narratorSpeaking = true;
                        }
                        else if(charName == characterTwo.Name)
                        {
                            leftSpeakerTalking = false;
                        }
                    }
                }
                //Just the narrator
                else if (character.Name == "Narrator")
                {
                    narratorSpeaking = true;
                }

                //Set Text
                var text = splitLines.Last();
                text = ResolvePredefinedVariables(text, predefinedVariables);

                NarrativeSequence.CharacterDialogueInfo info = new()
                {
                    Character = character,
                    CharacterTwo = characterTwo,
                    PoseIndex = poseIndex,
                    SecondPoseIndex = secondPoseIndex,
                    Text = text,
                    LeftCharacterTalking = leftSpeakerTalking,
                    NarratorSpeaking = narratorSpeaking
                };

                returnList.Add(info);
            }

            return returnList;
        }
        #endif

        public string ResolvePredefinedVariables(string targetString, PredefinedVariables variables)
        {
            if (variables is null)
            {
                return targetString;
            }

            var unresolvedVariables = targetString.Split(" ", StringSplitOptions.RemoveEmptyEntries)
                .Where(x => x[0] == '$').Select(x => x.Replace("$", string.Empty));

            foreach (var variable in unresolvedVariables)
            {
                var variableValue = variables.Variables.FirstOrDefault(x => x.Key == variable);
                
                if (variableValue is null)
                {
                    continue;
                }

                targetString = targetString.Replace($"${variable}", variableValue.Value);
            }

            return targetString;
        }

        public string ResolveRuntimeVariables(string targetString, IReadOnlyDictionary<string, string> variables)
        {
            if (variables is null)
            {
                return targetString;
            }
            
            var unresolvedVariables = targetString.Split(" ", StringSplitOptions.RemoveEmptyEntries)
                .Where(x => x[0] == '$').Select(x => x.Replace("$", string.Empty));

            foreach (var variable in unresolvedVariables)
            {
                if (!variables.TryGetValue(variable, out var outValue))
                {
                    continue;
                }

                targetString = targetString.Replace($"${variable}", outValue);
            }

            return targetString;
        }
    }
}