using BepInEx;
using GameCreator;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Common.Audio;
using GameCreator.Runtime.Common.UnityUI;
using GameCreator.Runtime.Dialogue;
using GameCreator.Runtime.Dialogue.UnityUI;
using GameCreator.Runtime.Variables;
using GameCreator.Runtime.VisualScripting;
using HarmonyLib;
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.XPath;
using TMPro;
using TransitionsPlusDemos;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

namespace SMSAndroidsCore
{
    [BepInPlugin(pluginGuid, Core.pluginName + " - Debug", Core.pluginVersion)]
    internal class Debugging : BaseUnityPlugin
    {
        #region Plugin Info
        public const string pluginGuid = "treboy.starmakerstory.smsandroidscore.debug";
        #endregion

        private Dictionary<IdString, GlobalVariableChangeTracker> variableTrackers = new Dictionary<IdString, GlobalVariableChangeTracker>();
        private float monitoringInterval = 0.1f; // Check every 0.1 seconds
        private bool isMonitoring = false;
        private Coroutine monitoringCoroutine;

        public void Update()
        {
            if (Core.currentScene.name == "CoreGameScene")
            {
            }
                // Check for J key press to trigger Debug
                if (Input.GetKeyDown(KeyCode.J))
            {
                var mapButtons = Core.mainCanvas?.Find("Navigator")?.Find("MapButtons")?.gameObject;
                if (mapButtons != null)
                {
                    PrintLocalListVariablesValues(mapButtons);
                }
                else
                {
                    Debug.LogError("MapButtons GameObject not found!");
                }
            }
        }

        #region GlobalNameVariable monitoring
        public class GlobalVariableChangeTracker
        {
            private Dictionary<string, object> previousValues = new Dictionary<string, object>();
            private string assetName;

            public GlobalVariableChangeTracker(string assetName)
            {
                this.assetName = assetName;
            }

            public void TrackVariable(string variableName, object currentValue)
            {
                if (!previousValues.ContainsKey(variableName))
                {
                    previousValues[variableName] = currentValue;
                    return;
                }

                var previousValue = previousValues[variableName];
                if (!Equals(previousValue, currentValue))
                {
                    Debug.Log($"[Global Variable Changed] {assetName}.{variableName}: {previousValue} -> {currentValue}");
                    previousValues[variableName] = currentValue;
                }
            }
        }

        public void StartMonitoringGlobalVariables()
        {
            if (!isMonitoring)
            {
                isMonitoring = true;
                monitoringCoroutine = StartCoroutine(MonitorGlobalVariablesCoroutine());
                Debug.Log("Started monitoring global variables");
            }
        }

        public void StopMonitoringGlobalVariables()
        {
            if (isMonitoring)
            {
                isMonitoring = false;
                if (monitoringCoroutine != null)
                {
                    StopCoroutine(monitoringCoroutine);
                    monitoringCoroutine = null;
                }
                Debug.Log("Stopped monitoring global variables");
            }
        }

        private IEnumerator MonitorGlobalVariablesCoroutine()
        {
            var waitInterval = new WaitForSeconds(monitoringInterval);

            while (isMonitoring)
            {
                MonitorGlobalVariableChanges();
                yield return waitInterval;
            }
        }

        public void MonitorGlobalVariableChanges()
        {
            var manager = GlobalNameVariablesManager.Instance;
            if (manager == null) return;

            PropertyInfo valuesProp = typeof(GlobalNameVariablesManager).GetProperty(
                "Values",
                BindingFlags.NonPublic | BindingFlags.Instance
            );

            if (valuesProp == null) return;

            var values = valuesProp.GetValue(manager) as Dictionary<IdString, NameVariableRuntime>;
            if (values == null) return;

            var repo = TRepository<VariablesRepository>.Get;
            if (repo == null) return;

            foreach (var pair in values)
            {
                var asset = repo.Variables.GetNameVariablesAsset(pair.Key);
                string assetName = asset != null ? asset.name : $"Unknown Asset ({pair.Key})";

                // Get or create tracker for this asset
                if (!variableTrackers.TryGetValue(pair.Key, out var tracker))
                {
                    tracker = new GlobalVariableChangeTracker(assetName);
                    variableTrackers[pair.Key] = tracker;
                }

                PropertyInfo runtimeVarsProp = typeof(NameVariableRuntime).GetProperty(
                    "Variables",
                    BindingFlags.NonPublic | BindingFlags.Instance
                );

                if (runtimeVarsProp == null) continue;

                var variables = runtimeVarsProp.GetValue(pair.Value) as Dictionary<string, NameVariable>;
                if (variables == null) continue;

                foreach (var varPair in variables)
                {
                    object value = varPair.Value?.Value;
                    tracker.TrackVariable(varPair.Key, value);
                }
            }
        }
        #endregion

        public static List<object> GetLocalListVariablesValues(GameObject targetGameObject)
        {
            if (targetGameObject == null)
            {
                Debug.LogError("Target GameObject is null");
                return new List<object>();
            }

            LocalListVariables localListVariables = targetGameObject.GetComponent<LocalListVariables>();
            if (localListVariables == null)
            {
                Debug.LogError($"LocalListVariables component not found on GameObject: {targetGameObject.name}");
                return new List<object>();
            }

            List<object> values = new List<object>();
            
            for (int i = 0; i < localListVariables.Count; i++)
            {
                object value = localListVariables.Get(i);
                values.Add(value);
            }

            Debug.Log($"Retrieved {values.Count} values from LocalListVariables on GameObject: {targetGameObject.name}");
            return values;
        }

        public static void PrintLocalListVariablesValues(GameObject targetGameObject)
        {
            if (targetGameObject == null)
            {
                Debug.LogError("Target GameObject is null");
                return;
            }

            LocalListVariables localListVariables = targetGameObject.GetComponent<LocalListVariables>();
            if (localListVariables == null)
            {
                Debug.LogError($"LocalListVariables component not found on GameObject: {targetGameObject.name}");
                return;
            }

            Debug.Log($"=== LocalListVariables values from GameObject: {targetGameObject.name} ===");
            Debug.Log($"Count: {localListVariables.Count}");
            Debug.Log($"TypeID: {localListVariables.TypeID}");
            
            // Try to access the private m_Runtime field to get more info
            FieldInfo runtimeField = typeof(LocalListVariables).GetField("m_Runtime", BindingFlags.NonPublic | BindingFlags.Instance);
            if (runtimeField != null)
            {
                var runtime = runtimeField.GetValue(localListVariables) as ListVariableRuntime;
                if (runtime != null)
                {
                    Debug.Log($"Runtime Count: {runtime.Count}");
                    
                    // Try to call OnStartup if it hasn't been called yet
                    if (runtime.Count == 0)
                    {
                        Debug.Log("Attempting to call OnStartup()...");
                        MethodInfo onStartupMethod = typeof(ListVariableRuntime).GetMethod("OnStartup", BindingFlags.Public | BindingFlags.Instance);
                        if (onStartupMethod != null)
                        {
                            onStartupMethod.Invoke(runtime, null);
                            Debug.Log($"Runtime Count after OnStartup: {runtime.Count}");
                        }
                    }
                    
                    // Try to access the private m_List field
                    FieldInfo listField = typeof(ListVariableRuntime).GetField("m_List", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (listField != null)
                    {
                        var list = listField.GetValue(runtime);
                        Debug.Log($"m_List: {list}");
                        
                        // Try to get the Length property of m_List
                        PropertyInfo lengthProp = list?.GetType().GetProperty("Length");
                        if (lengthProp != null)
                        {
                            int length = (int)lengthProp.GetValue(list);
                            Debug.Log($"m_List.Length: {length}");
                            
                            // Try to access individual items in the IndexList
                            for (int i = 0; i < length; i++)
                            {
                                MethodInfo getMethod = list.GetType().GetMethod("Get", new Type[] { typeof(int) });
                                if (getMethod != null)
                                {
                                    var item = getMethod.Invoke(list, new object[] { i });
                                    Debug.Log($"m_List[{i}]: {item}");
                                }
                            }
                        }
                        
                        // Try to access the TypeID of the IndexList
                        PropertyInfo typeIDProp = list?.GetType().GetProperty("TypeID");
                        if (typeIDProp != null)
                        {
                            var typeID = typeIDProp.GetValue(list);
                            Debug.Log($"m_List.TypeID: {typeID}");
                        }
                        
                        // Try to access the serialized data in the IndexList
                        FieldInfo[] listFields = list?.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
                        if (listFields != null)
                        {
                            Debug.Log("IndexList internal fields:");
                            foreach (var field in listFields)
                            {
                                var fieldValue = field.GetValue(list);
                                Debug.Log($"  {field.Name}: {fieldValue}");
                            }
                        }
                    }
                    
                    // Try to access the Variables list
                    PropertyInfo variablesProp = typeof(ListVariableRuntime).GetProperty("Variables", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (variablesProp != null)
                    {
                        var variables = variablesProp.GetValue(runtime) as List<object>;
                        Debug.Log($"Variables list count: {variables?.Count ?? 0}");
                        
                        if (variables != null && variables.Count > 0)
                        {
                            Debug.Log("Variables list contents:");
                            for (int i = 0; i < variables.Count; i++)
                            {
                                Debug.Log($"  Variables[{i}]: {variables[i]}");
                            }
                        }
                    }
                }
            }
            
            for (int i = 0; i < localListVariables.Count; i++)
            {
                object value = localListVariables.Get(i);
                Debug.Log($"Index {i}: {value}");
            }
            
            Debug.Log($"=== End of LocalListVariables (Total: {localListVariables.Count} values) ===");
        }

        public static void PrintConditionsAndTriggers(GameObject targetGameObject)
        {
            if (targetGameObject == null)
            {
                Debug.LogError("Target GameObject is null");
                return;
            }

            // Helper to print m_Variable and m_Name fields for detector objects
            void PrintDetectorVariableDetails(object obj, string indent = "        ")
            {
                if (obj == null) return;
                var type = obj.GetType();

                Debug.Log($"{indent}--- DETECTOR DETAILS FOR TYPE: {type.Name} ---");

                Type currentType = type;
                bool mVariableFound = false;
                bool mNameFound = false;

                // Iterate through the type hierarchy to find fields
                while (currentType != null && currentType != typeof(object))
                {
                    Debug.Log($"{indent}  Searching fields on type: {currentType.Name}");
                    // Get only fields declared in the current type
                    foreach (var field in currentType.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                    {
                        Debug.Log($"{indent}    Found field: {field.Name} (Declared in: {field.DeclaringType.Name})");
                        try
                        {
                            var fieldValue = field.GetValue(obj);
                            Debug.Log($"{indent}      Value: {fieldValue} (Type: {field.FieldType.Name})");

                            if (field.Name == "m_Variable")
                            {
                                mVariableFound = true;
                                if (fieldValue != null)
                                {
                                    var assetNameProp = fieldValue.GetType().GetProperty("name", BindingFlags.Public | BindingFlags.Instance);
                                    if (assetNameProp != null)
                                    {
                                        var assetName = assetNameProp.GetValue(fieldValue);
                                        Debug.Log($"{indent}        GLOBAL VARIABLE ASSET: {assetName}");
                                    }
                                    else
                                    {
                                        Debug.Log($"{indent}        GLOBAL VARIABLE ASSET (no 'name' property): {fieldValue}");
                                    }
                                }
                                else
                                {
                                    Debug.Log($"{indent}        GLOBAL VARIABLE ASSET: null");
                                }
                            }
                            else if (field.Name == "m_Name")
                            {
                                mNameFound = true;
                                if (fieldValue != null)
                                {
                                    var stringProp = fieldValue.GetType().GetProperty("String", BindingFlags.Public | BindingFlags.Instance);
                                    if (stringProp != null)
                                    {
                                        var variableName = stringProp.GetValue(fieldValue);
                                        Debug.Log($"{indent}        VARIABLE NAME (from m_Name.String): {variableName}");
                                    }
                                    else
                                    {
                                        Debug.Log($"{indent}        m_Name found but no 'String' property: {fieldValue.GetType().Name}");
                                    }
                                }
                                else
                                {
                                    Debug.Log($"{indent}        m_Name field value is null");
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning($"{indent}      Could not get value for {field.Name}: {e.Message}");
                        }
                    }
                    currentType = currentType.BaseType;
                }

                if (!mVariableFound)
                {
                    Debug.LogWarning($"{indent}Field 'm_Variable' was NOT found after full hierarchy scan.");
                }
                if (!mNameFound)
                {
                    Debug.LogWarning($"{indent}Field 'm_Name' was NOT found after full hierarchy scan.");
                }

                Debug.Log($"{indent}---------------------------------------------------");
            }

            // Helper to print details of an InstructionList
            void PrintInstructionListDetails(object instructionListObj, string indent = "        ")
            {
                if (instructionListObj == null) return;
                Debug.Log($"{indent}Instructions: {instructionListObj.GetType().Name}");

                var instructionsArrayField = instructionListObj.GetType().GetField("m_Instructions", BindingFlags.NonPublic | BindingFlags.Instance);
                if (instructionsArrayField != null)
                {
                    var instructionsArray = instructionsArrayField.GetValue(instructionListObj) as System.Collections.IEnumerable;
                    if (instructionsArray != null)
                    {
                        int instIdx = 0;
                        foreach (var instruction in instructionsArray)
                        {
                            if (instruction != null)
                            {
                                var instType = instruction.GetType();
                                var titleProp = instType.GetProperty("Title", BindingFlags.Public | BindingFlags.Instance);
                                string title = titleProp?.GetValue(instruction)?.ToString() ?? instType.Name;
                                Debug.Log($"{indent}  Instruction {instIdx}: {title} (Type: {instType.Name})");
                            }
                            else
                            {
                                Debug.Log($"{indent}  Instruction {instIdx}: null");
                            }
                            instIdx++;
                        }
                    }
                    else
                    {
                        Debug.Log($"{indent}  m_Instructions array is null or not IEnumerable.");
                    }
                }
                else
                {
                    Debug.Log($"{indent}  Field 'm_Instructions' not found on InstructionList.");
                }
            }

            // Print all Conditions
            var conditionsComponents = targetGameObject.GetComponents<MonoBehaviour>()
                .Where(c => c != null && c.GetType().Name == "Conditions");
            foreach (var cond in conditionsComponents)
            {
                Debug.Log($"Found Conditions component on {targetGameObject.name}");
                var branchesField = cond.GetType().GetField("m_Branches", BindingFlags.NonPublic | BindingFlags.Instance);
                if (branchesField != null)
                {
                    var branches = branchesField.GetValue(cond);
                    Debug.Log($"  m_Branches: {branches.GetType().Name}");

                    if (branches != null)
                    {
                        // Check the actual length of the BranchList
                        PropertyInfo lengthProp = branches.GetType().GetProperty("Length", BindingFlags.Public | BindingFlags.Instance);
                        if (lengthProp != null)
                        {
                            int branchListLength = (int)lengthProp.GetValue(branches);
                            Debug.Log($"  BranchList Length: {branchListLength}");
                        }

                        var branchesArrayField = branches.GetType().GetField("m_Branches", BindingFlags.NonPublic | BindingFlags.Instance);
                        if (branchesArrayField != null)
                        {
                            var branchesArray = branchesArrayField.GetValue(branches) as System.Collections.IEnumerable;
                            if (branchesArray != null)
                            {
                                int branchIdx = 0;
                                foreach (var branch in branchesArray)
                                {
                                    if (branch != null)
                                    {
                                        Debug.Log($"    Branch {branchIdx}: {branch.GetType().Name}");
                                        
                                        // List ALL fields on the Branch object to see what's actually available
                                        Debug.Log($"      === ALL FIELDS ON BRANCH {branchIdx} ===");
                                        Type currentBranchType = branch.GetType();
                                        while (currentBranchType != null && currentBranchType != typeof(object))
                                        {
                                            Debug.Log($"      Fields declared in {currentBranchType.Name}:");
                                            var allFields = currentBranchType.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                                            if (allFields.Length == 0)
                                            {
                                                Debug.Log($"        (No fields declared in this type)");
                                            }
                                            else
                                            {
                                                foreach (var field in allFields)
                                                {
                                                    try
                                                    {
                                                        var fieldValue = field.GetValue(branch);
                                                        Debug.Log($"        {field.Name}: {fieldValue} (Type: {field.FieldType.Name}, Access: {(field.IsPrivate ? "Private" : field.IsPublic ? "Public" : "Other")})");
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        Debug.Log($"        {field.Name}: ERROR - {e.Message}");
                                                    }
                                                }
                                            }
                                            currentBranchType = currentBranchType.BaseType;
                                        }
                                        Debug.Log($"      === END FIELDS ON BRANCH {branchIdx} ===");

                                        // Print details of the condition within the branch
                                        Debug.Log($"      Attempting to get m_Condition field from Branch {branchIdx}.");
                                        var conditionField = branch.GetType().GetField("m_Condition", BindingFlags.NonPublic | BindingFlags.Instance);
                                        Debug.Log($"      conditionField found: {conditionField != null}");
                                        if (conditionField != null)
                                        {
                                            var condition = conditionField.GetValue(branch);
                                            if (condition != null)
                                            {
                                                Debug.Log($"      Condition: {condition.GetType().Name}");
                                                // If it's a TDetectorNameVariable, print its details
                                                if (condition.GetType().Name.Contains("DetectorGlobalNameVariable") || condition.GetType().IsSubclassOf(typeof(GameCreator.Runtime.Variables.TDetectorNameVariable<GameCreator.Runtime.Variables.GlobalNameVariables>)))
                                                {
                                                    PrintDetectorVariableDetails(condition, "        ");
                                                }
                                                // Else, just print its basic fields for Debug
                                                else
                                                {
                                                    foreach (var field in condition.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
                                                    {
                                                        var fieldValue = field.GetValue(condition);
                                                        Debug.Log($"        {field.Name}: {fieldValue} (Type: {field.FieldType.Name})");
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                Debug.Log($"      Condition for Branch {branchIdx} is null");
                                            }
                                        }
                                        else
                                        {
                                            Debug.Log($"      Field 'm_Condition' not found on Branch for Branch {branchIdx}.");
                                        }

                                        // Print details of the instructions within the branch
                                        Debug.Log($"      Attempting to get m_Instructions field from Branch {branchIdx}.");
                                        var instructionsField = branch.GetType().GetField("m_Instructions", BindingFlags.NonPublic | BindingFlags.Instance);
                                        Debug.Log($"      instructionsField found: {instructionsField != null}");
                                        if (instructionsField != null)
                                        {
                                            var instructions = instructionsField.GetValue(branch);
                                            PrintInstructionListDetails(instructions, "        ");
                                        }
                                        else
                                        {
                                            Debug.Log($"      Field 'm_Instructions' not found on Branch for Branch {branchIdx}.");
                                        }
                                    }
                                    else
                                    {
                                        Debug.Log($"    Branch {branchIdx}: null");
                                    }
                                    branchIdx++;
                                }
                            }
                            else
                            {
                                Debug.Log($"  m_Branches array is null or not IEnumerable within BranchList.");
                            }
                        }
                        else
                        {
                            Debug.Log($"  Field 'm_Branches' not found on BranchList directly.");
                        }
                    }
                    else
                    {
                        Debug.Log($"  m_Branches object is null.");
                    }
                }
                else
                {
                    Debug.Log($"  Field 'm_Branches' not found on Conditions component.");
                }
            }

            // Print all Triggers
            var triggerComponents = targetGameObject.GetComponents<MonoBehaviour>()
                .Where(c => c != null && c.GetType().Name == "Trigger");
            foreach (var trig in triggerComponents)
            {
                Debug.Log($"Found Trigger component on {targetGameObject.name}");
                // Get m_Instructions directly from the Trigger component (it inherits from BaseActions)
                var instructionsField = trig.GetType().GetField("m_Instructions", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
                if (instructionsField != null)
                {
                    Debug.Log($"  Found m_Instructions field on Trigger. Declared in: {instructionsField.DeclaringType.Name}");
                    var instructions = instructionsField.GetValue(trig);
                    if (instructions != null)
                    {
                        PrintInstructionListDetails(instructions, "  "); // Adjust indent
                    }
                    else
                    {
                        Debug.Log($"  m_Instructions field value is null on Trigger.");
                    }
                }
                else
                {
                    Debug.Log($"  Field 'm_Instructions' not found on Trigger (or its base types via FlattenHierarchy).");
                }

                var eventField = trig.GetType().GetField("m_TriggerEvent", BindingFlags.NonPublic | BindingFlags.Instance);
                if (eventField != null)
                {
                    var triggerEvent = eventField.GetValue(trig);
                    Debug.Log($"  m_TriggerEvent: {triggerEvent.GetType().Name}");

                    if (triggerEvent != null)
                    {
                        var eventType = triggerEvent.GetType();
                        Debug.Log($"    Event type: {eventType.Name}");
                        var eventFields = eventType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                        foreach (var field in eventFields)
                        {
                            var value = field.GetValue(triggerEvent);
                            Debug.Log($"      {field.Name}: {value}");
                            if (value != null && (value.GetType().Name.Contains("DetectorGlobalNameVariable") || value.GetType().IsSubclassOf(typeof(GameCreator.Runtime.Variables.TDetectorNameVariable<GameCreator.Runtime.Variables.GlobalNameVariables>))))
                            {
                                PrintDetectorVariableDetails(value, "        ");
                            }
                        }
                    }
                }
            }
        }
    }
}
