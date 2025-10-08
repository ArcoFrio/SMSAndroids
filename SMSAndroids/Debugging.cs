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
        public  bool isMonitoring = false;
        private Coroutine monitoringCoroutine;
        private bool monitorRoomTalk = false; // Switch for activating/deactivating monitoring

        // Scene change tracking
        private string lastSceneName = "";
        private int lastSceneBuildIndex = -1;
        private bool sceneChangeLoggingEnabled = true;

        public void Awake()
        {
            // Subscribe to scene change events
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            
            //Logger.LogInfo("Scene change monitoring initialized");
        }

        public void OnDestroy()
        {
            // Unsubscribe from scene change events
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        private void OnSceneLoaded(Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            if (!sceneChangeLoggingEnabled) return;

            string modeText = mode == UnityEngine.SceneManagement.LoadSceneMode.Single ? "Single" : "Additive";
            Debug.Log($"[Scene Change] Scene LOADED: '{scene.name}' (Build Index: {scene.buildIndex}, Mode: {modeText})");
            
            // Check if this is a scene reload (same scene name)
            if (scene.name == lastSceneName)
            {
                Debug.Log($"[Scene Change] SCENE RELOAD detected: '{scene.name}' was reloaded");
            }
            
            lastSceneName = scene.name;
            lastSceneBuildIndex = scene.buildIndex;
        }

        private void OnSceneUnloaded(Scene scene)
        {
            if (!sceneChangeLoggingEnabled) return;
            
            Debug.Log($"[Scene Change] Scene UNLOADED: '{scene.name}' (Build Index: {scene.buildIndex})");
        }

        public void Update()
        {
            if (Core.currentScene.name == "CoreGameScene")
            {
                // Check for R key press to reload Amber bust textures
                if (Input.GetKeyDown(KeyCode.R))
                {
                    ReloadAmberBustTextures();
                }
            }
            // Check for J key press to trigger Debug
            //if (Input.GetKeyDown(KeyCode.J))
            //{
            //    var mapButtons = Core.mainCanvas?.Find("Navigator")?.Find("MapButtons")?.gameObject;
            //    if (mapButtons != null)
            //    {
            //        PrintLocalListVariablesValues(mapButtons);
            //    }
            //    else
            //    {
            //        Debug.LogError("MapButtons GameObject not found!");
            //    }
            //}

            // Check for K key to toggle scene change logging
            //if (Input.GetKeyDown(KeyCode.K))
            //{
            //    sceneChangeLoggingEnabled = !sceneChangeLoggingEnabled;
            //    Debug.Log($"[Scene Change] Scene change logging {(sceneChangeLoggingEnabled ? "ENABLED" : "DISABLED")}");
            //}

            //// Check for L key to print current scene info
            //if (Input.GetKeyDown(KeyCode.L))
            //{
            //    var currentScene = SceneManager.GetActiveScene();
            //    Debug.Log($"[Scene Change] Current Scene: '{currentScene.name}' (Build Index: {currentScene.buildIndex})");
            //    Debug.Log($"[Scene Change] Last Scene: '{lastSceneName}' (Build Index: {lastSceneBuildIndex})");
            //    Debug.Log($"[Scene Change] Total loaded scenes: {SceneManager.sceneCount}");
                
            //    // List all loaded scenes
            //    for (int i = 0; i < SceneManager.sceneCount; i++)
            //    {
            //        var scene = SceneManager.GetSceneAt(i);
            //        Debug.Log($"[Scene Change] Loaded Scene {i}: '{scene.name}' (Build Index: {scene.buildIndex}, IsLoaded: {scene.isLoaded})");
            //    }
            //}

            // // Check for M key to toggle room talk monitoring
            //if (Input.GetKeyDown(KeyCode.M))
            //{
            //    monitorRoomTalk = !monitorRoomTalk;
            //    Debug.Log($"[Debugging] RoomTalk monitoring {(monitorRoomTalk ? "ENABLED" : "DISABLED")}");
            //}

            //if (monitorRoomTalk && Core.roomTalk != null)
            //{
            //    CheckRoomTalkChildren();
            //}
        }

        private readonly HashSet<GameObject> checkedRoomTalkChildren = new HashSet<GameObject>();

        private void CheckRoomTalkChildren()
        {
            foreach (Transform child in Core.roomTalk)
            {
                GameObject childGO = child.gameObject;

                if (childGO == null || childGO.name == "Always_Active")
                {
                    continue;
                }

                if (childGO.activeSelf && !checkedRoomTalkChildren.Contains(childGO))
                {
                    // Run the CheckAllConditions method for this child
                    Debug.Log($"[Debugging] Running CheckAllConditions for {childGO.name}");
                    bool conditionsMet = Places.CheckAllConditions(childGO, new GameCreator.Runtime.Common.Args(childGO));

                    Debug.Log($"[Debugging] Conditions met for {childGO.name}: {conditionsMet}");

                    // Print Conditions only
                    PrintConditions(childGO);

                    checkedRoomTalkChildren.Add(childGO);

                    // Optionally, deactivate the child after checking, or perform other actions
                    //child.gameObject.SetActive(false);
                }
            }
        }

        private void PrintConditions(GameObject targetGameObject)
        {
            if (targetGameObject == null)
            {
                Debug.LogError("Target GameObject is null");
                return;
            }

            // Print all Conditions
            var conditionsComponents = targetGameObject.GetComponents<MonoBehaviour>()
                .Where(c => c != null && c.GetType().Name == "Conditions");
            foreach (var cond in conditionsComponents)
            {
                Debug.Log($"Found Conditions component on {targetGameObject.name}");
                var branchesField = cond.GetType().GetField("m_Branches", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
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

                        var branchesArrayField = branches.GetType().GetField("m_Branches", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
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

                                        // Print details of the condition within the branch
                                        Debug.Log($"      Attempting to get m_ConditionList field from Branch {branchIdx}.");
                                        var conditionListField = branch.GetType().GetField("m_ConditionList", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                                        Debug.Log($"      conditionListField found: {conditionListField != null}");
                                        if (conditionListField != null)
                                        {
                                            var conditionList = conditionListField.GetValue(branch) as ConditionList;
                                            if (conditionList != null)
                                            {
                                                Debug.Log($"      ConditionList: {conditionList.GetType().Name} - {conditionList.ToString()}");

                                                for (int i = 0; i < conditionList.Length; i++)
                                                {
                                                    var condition = conditionList.Get(i);
                                                    if (condition != null)
                                                    {
                                                        Debug.Log($"          Condition {i}: {condition.GetType().Name} - {condition.Title}");
													// Print all fields (especially m_ fields)
                                                        foreach (var field in condition.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public))
                                                        {
                                                                var fieldValue = field.GetValue(condition);
                                                                Debug.Log($"             {field.Name}: {fieldValue}");
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Debug.Log($"          Condition {i}: null");
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                Debug.Log($"      ConditionList for Branch {branchIdx} is null");
                                            }
                                        }
                                        else
                                        {
                                            Debug.Log($"      Field 'm_ConditionList' not found on Branch for Branch {branchIdx}.");
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
        }

        #region GlobalNameVariable monitoring

        #region GlobalNameVariable monitoring

        #region GlobalNameVariable monitoring

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
                                // Print all private fields (especially m_ fields)
                                foreach (var field in instType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public))
                                {
                                    if (field.Name.StartsWith("m_"))
                                    {
                                        var fieldValue = field.GetValue(instruction);
                                        Debug.Log($"{indent}    {field.Name}: {fieldValue}");
                                    }
                                }
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
                                        Debug.Log($"      Attempting to get m_InstructionList field from Branch {branchIdx}.");
                                        var instructionListField = branch.GetType().GetField("m_InstructionList", BindingFlags.NonPublic | BindingFlags.Instance);
                                        Debug.Log($"      instructionListField found: {instructionListField != null}");
                                        if (instructionListField != null)
                                        {
                                            var instructionList = instructionListField.GetValue(branch);
                                            PrintInstructionListDetails(instructionList, "        ");
                                        }
                                        else
                                        {
                                            Debug.Log($"      Field 'm_InstructionList' not found on Branch for Branch {branchIdx}.");
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

        public static void PrintDialogueComponentInfo(GameObject targetGameObject)
        {
            if (targetGameObject == null)
            {
                Debug.LogError("Target GameObject is null");
                return;
            }

            var dialogue = targetGameObject.GetComponent(typeof(GameCreator.Runtime.Dialogue.Dialogue));
            if (dialogue == null)
            {
                Debug.Log($"No Dialogue component found on {targetGameObject.name}");
                return;
            }

            Debug.Log($"=== Dialogue Component Info for {targetGameObject.name} ===");

            // Get the Story property
            var dialogueType = dialogue.GetType();
            var storyProp = dialogueType.GetProperty("Story", BindingFlags.Public | BindingFlags.Instance);
            var story = storyProp?.GetValue(dialogue);
            if (story == null)
            {
                Debug.Log("Dialogue.Story is null");
                return;
            }

            // Get Content property
            var storyType = story.GetType();
            var contentProp = storyType.GetProperty("Content", BindingFlags.Public | BindingFlags.Instance);
            var content = contentProp?.GetValue(story);
            if (content == null)
            {
                Debug.Log("Story.Content is null");
                return;
            }

            var contentType = content.GetType();

            // Print Roles
            var rolesField = contentType.GetField("m_Roles", BindingFlags.NonPublic | BindingFlags.Instance);
            var roles = rolesField?.GetValue(content) as Array;
            Debug.Log("--- Roles ---");
            if (roles != null)
            {
                int idx = 0;
                foreach (var roleObj in roles)
                {
                    if (roleObj == null) continue;
                    var roleType = roleObj.GetType();
                    var actorField = roleType.GetField("m_Actor", BindingFlags.NonPublic | BindingFlags.Instance);
                    var targetField = roleType.GetField("m_Target", BindingFlags.NonPublic | BindingFlags.Instance);
                    var actor = actorField?.GetValue(roleObj);
                    var target = targetField?.GetValue(roleObj);

                    string actorName = "(null)";
                    string actorDesc = "(null)";
                    string actorAssetName = "(null)";
                    if (actor != null)
                    {
                        // Try to get name and description via Actant
                        var actantField = actor.GetType().GetField("m_Actant", BindingFlags.NonPublic | BindingFlags.Instance);
                        var actant = actantField?.GetValue(actor);
                        if (actant != null)
                        {
                            var getNameMethod = actant.GetType().GetMethod("GetName", BindingFlags.Public | BindingFlags.Instance);
                            var getDescMethod = actant.GetType().GetMethod("GetDescription", BindingFlags.Public | BindingFlags.Instance);
                            actorName = getNameMethod?.Invoke(actant, new object[] { null }) as string ?? "(no name)";
                            actorDesc = getDescMethod?.Invoke(actant, new object[] { null }) as string ?? "(no desc)";
                        }
                        // Try to get Unity asset name
                        var nameProp = actor.GetType().GetProperty("name", BindingFlags.Public | BindingFlags.Instance);
                        actorAssetName = nameProp?.GetValue(actor) as string ?? "(no asset name)";
                    }

                    string targetDesc = "(null)";
                    if (target != null)
                    {
                        // Try to get the target GameObject (requires Args, so just print type for now)
                        targetDesc = target.ToString();
                    }

                    Debug.Log($"Role {idx}: Actor Asset='{actorAssetName}', Name='{actorName}', Desc='{actorDesc}', Target={targetDesc}");
                    idx++;
                }
            }
            else
            {
                Debug.Log("No roles found.");
            }

            // Print RootIds
            var rootIdsProp = contentType.GetProperty("RootIds", BindingFlags.Public | BindingFlags.Instance);
            var rootIds = rootIdsProp?.GetValue(content) as int[];
            if (rootIds != null)
            {
                Debug.Log($"RootIds: [{string.Join(", ", rootIds)}]");
            }

            // Print all nodes (if accessible)
            var getNodeMethod = contentType.GetMethod("Get", BindingFlags.Public | BindingFlags.Instance);
            if (getNodeMethod != null && rootIds != null)
            {
                foreach (var nodeId in rootIds)
                {
                    var node = getNodeMethod.Invoke(content, new object[] { nodeId });
                    if (node != null)
                    {
                        Debug.Log($"--- Node {nodeId} ---");
                        var nodeType = node.GetType();

                        // Print node text
                        var textField = nodeType.GetField("m_Text", BindingFlags.NonPublic | BindingFlags.Instance);
                        string nodeText = textField?.GetValue(node)?.ToString() ?? "(no text)";
                        Debug.Log($"  Text: {nodeText}");

                        // Print node type
                        var nodeTypeField = nodeType.GetField("m_NodeType", BindingFlags.NonPublic | BindingFlags.Instance);
                        Debug.Log($"  NodeType: {nodeTypeField?.GetValue(node)}");

                        // Print Acting
                        var actingField = nodeType.GetField("m_Acting", BindingFlags.NonPublic | BindingFlags.Instance);
                        var acting = actingField?.GetValue(node);
                        if (acting != null)
                        {
                            var actingType = acting.GetType();
                            var actorField = actingType.GetField("m_Actor", BindingFlags.NonPublic | BindingFlags.Instance);
                            var exprField = actingType.GetField("m_Expression", BindingFlags.NonPublic | BindingFlags.Instance);
                            var portraitField = actingType.GetField("m_Portrait", BindingFlags.NonPublic | BindingFlags.Instance);

                            var actor = actorField?.GetValue(acting);
                            int exprIdx = exprField != null ? (int)exprField.GetValue(acting) : -1;
                            var portrait = portraitField?.GetValue(acting);

                            string actorName = "(null)";
                            string actorDesc = "(null)";
                            string actorAssetName = "(null)";
                            string exprName = "(unknown)";
                            if (actor != null)
                            {
                                // Try to get name and description via Actant
                                var actantField = actor.GetType().GetField("m_Actant", BindingFlags.NonPublic | BindingFlags.Instance);
                                var actant = actantField?.GetValue(actor);
                                if (actant != null)
                                {
                                    var getNameMethod = actant.GetType().GetMethod("GetName", BindingFlags.Public | BindingFlags.Instance);
                                    var getDescMethod = actant.GetType().GetMethod("GetDescription", BindingFlags.Public | BindingFlags.Instance);
                                    actorName = getNameMethod?.Invoke(actant, new object[] { null }) as string ?? "(no name)";
                                    actorDesc = getDescMethod?.Invoke(actant, new object[] { null }) as string ?? "(no desc)";
                                }
                                // Try to get Unity asset name
                                var nameProp = actor.GetType().GetProperty("name", BindingFlags.Public | BindingFlags.Instance);
                                actorAssetName = nameProp?.GetValue(actor) as string ?? "(no asset name)";

                                // Try to get expression name
                                var expressionsField = actor.GetType().GetField("m_Expressions", BindingFlags.NonPublic | BindingFlags.Instance);
                                var expressions = expressionsField?.GetValue(actor);
                                if (expressions != null)
                                {
                                    var fromIndexMethod = expressions.GetType().GetMethod("FromIndex", BindingFlags.Public | BindingFlags.Instance);
                                    var exprObj = fromIndexMethod?.Invoke(expressions, new object[] { exprIdx });
                                    if (exprObj != null)
                                    {
                                        var exprNameProp = exprObj.GetType().GetProperty("name", BindingFlags.Public | BindingFlags.Instance);
                                        exprName = exprNameProp?.GetValue(exprObj) as string ?? exprObj.ToString();
                                    }
                                }
                            }

                            Debug.Log($"  Acting: Actor Asset='{actorAssetName}', Name='{actorName}', Desc='{actorDesc}', ExpressionIdx={exprIdx}, ExpressionName={exprName}, Portrait={portrait}");
                        }
                    }
                    else
                    {
                        Debug.Log($"Node {nodeId} is null");
                    }
                }
            }

            Debug.Log($"=== End Dialogue Component Info for {targetGameObject.name} ===");
        }

        public static void PrintDialogueComponentDeep(GameObject targetGameObject, int maxDepth = 4)
        {
            if (targetGameObject == null)
            {
                Debug.LogError("Target GameObject is null");
                return;
            }

            var dialogue = targetGameObject.GetComponent(typeof(GameCreator.Runtime.Dialogue.Dialogue));
            if (dialogue == null)
            {
                Debug.Log($"No Dialogue component found on {targetGameObject.name}");
                return;
            }

            Debug.Log($"=== DEEP Dialogue Component Info for {targetGameObject.name} ===");
            PrintObjectRecursive(dialogue, "Dialogue", 0, maxDepth, new HashSet<object>());
            Debug.Log($"=== END DEEP Dialogue Component Info for {targetGameObject.name} ===");
        }

        private static void PrintObjectRecursive(object obj, string label, int depth, int maxDepth, HashSet<object> visited)
        {
            if (obj == null)
            {
                Debug.Log($"{new string(' ', depth * 2)}{label}: null");
                return;
            }
            if (visited.Contains(obj))
            {
                Debug.Log($"{new string(' ', depth * 2)}{label}: (already printed, skipping to avoid recursion)");
                return;
            }
            Type type = obj.GetType();
            string indent = new string(' ', depth * 2);

            // Special-case GameObject
            if (obj is UnityEngine.GameObject go)
            {
                Debug.Log($"{indent}{label}: GameObject name='{go.name}' tag='{go.tag}' layer={go.layer}");
                return;
            }

            // Special-case Component (but not MonoBehaviour/ScriptableObject)
            if (obj is UnityEngine.Component comp && !(obj is MonoBehaviour))
            {
                Debug.Log($"{indent}{label}: {type.Name} (Component) on GameObject='{comp.gameObject.name}'");
                return;
            }

            // For MonoBehaviour and ScriptableObject, print fields/properties as normal
            // For all other UnityEngine.Objects, print name/type and stop
            if (obj is UnityEngine.Object unityObj && !(obj is MonoBehaviour) && !(obj is ScriptableObject))
            {
                Debug.Log($"{indent}{label}: {type.Name} (Unity Object) name='{unityObj.name}'");
                return;
            }

            // Print simple types directly
            if (type.IsPrimitive || obj is string || obj is decimal || obj is Enum)
            {
                Debug.Log($"{indent}{label}: {obj} ({type.Name})");
                return;
            }

            // Print collections (print up to 5 elements in detail)
            if (obj is System.Collections.IEnumerable enumerable && !(obj is string))
            {
                Debug.Log($"{indent}{label}: {type.Name} (IEnumerable)");
                int idx = 0;
                foreach (var item in enumerable)
                {
                    if (idx > 4)
                    {
                        Debug.Log($"{indent}  ... (truncated)");
                        break;
                    }
                    PrintObjectRecursive(item, $"[{idx}]", depth + 1, maxDepth, visited);
                    idx++;
                }
                return;
            }

            // Always print fields for these types, even at maxDepth
            bool alwaysPrintFields = type.FullName == "GameCreator.Runtime.Dialogue.Role"
                || type.FullName == "GameCreator.Runtime.Dialogue.Actor"
                || type.FullName == "GameCreator.Runtime.Dialogue.Actant"
                || type.IsSubclassOf(typeof(ScriptableObject))
                || type.IsSubclassOf(typeof(MonoBehaviour));

            if (depth >= maxDepth && !alwaysPrintFields)
            {
                Debug.Log($"{indent}{label}: (max depth reached)");
                return;
            }

            visited.Add(obj);

            Debug.Log($"{indent}{label}: {type.FullName}");

            // Print all fields
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            foreach (var field in fields)
            {
                object value = null;
                try { value = field.GetValue(obj); }
                catch { value = "(unreadable)"; }
                PrintObjectRecursive(value, field.Name, depth + 1, maxDepth, visited);
            }

            // Print all properties (skip indexers and Unity's 'hideFlags')
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
                .Where(p => p.GetIndexParameters().Length == 0 && p.Name != "hideFlags");
            foreach (var prop in props)
            {
                object value = null;
                try { value = prop.GetValue(obj); }
                catch { value = "(unreadable)"; }
                PrintObjectRecursive(value, prop.Name, depth + 1, maxDepth, visited);
            }
        }

        public static void PrintAllActorExpressions(Actor actor)
        {
            if (actor == null)
            {
                Debug.Log("[PrintAllActorExpressions] Actor is null.");
                return;
            }

            var expressions = actor.GetType().GetField("m_Expressions", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(actor);
            if (expressions == null)
            {
                Debug.Log($"[PrintAllActorExpressions] No expressions found for actor: {actor.name}");
                return;
            }

            var lengthProp = expressions.GetType().GetProperty("Length", BindingFlags.Public | BindingFlags.Instance);
            int length = (int)(lengthProp?.GetValue(expressions) ?? 0);

            var fromIndexMethod = expressions.GetType().GetMethod("FromIndex", BindingFlags.Public | BindingFlags.Instance);

            Debug.Log($"[PrintAllActorExpressions] Expressions for actor: {actor.name} (Total: {length})");
            for (int i = 0; i < length; i++)
            {
                var expression = fromIndexMethod.Invoke(expressions, new object[] { i });
                if (expression != null)
                {
                    var idProp = expression.GetType().GetProperty("Id", BindingFlags.Public | BindingFlags.Instance);
                    var id = idProp?.GetValue(expression)?.ToString() ?? "(null)";

                    // Sprite
                    var spriteField = expression.GetType().GetField("m_Sprite", BindingFlags.NonPublic | BindingFlags.Instance);
                    var spriteObj = spriteField?.GetValue(expression);
                    string spriteInfo = "(null)";
                    if (spriteObj != null)
                    {
                        var getMethod = spriteObj.GetType().GetMethod("Get", new[] { typeof(GameCreator.Runtime.Common.Args) });
                        if (getMethod != null)
                        {
                            var sprite = getMethod.Invoke(spriteObj, new object[] { null });
                            spriteInfo = sprite != null ? sprite.ToString() : "(null)";
                        }
                    }

                    // Speech Skin
                    var speechSkinField = expression.GetType().GetField("m_OverrideSpeechSkin", BindingFlags.NonPublic | BindingFlags.Instance);
                    var speechSkin = speechSkinField?.GetValue(expression);
                    string speechSkinInfo = "(none)";
                    if (speechSkin != null)
                    {
                        var prefabProp = speechSkin.GetType().GetProperty("Value", BindingFlags.Public | BindingFlags.Instance);
                        var prefab = prefabProp?.GetValue(speechSkin) as GameObject;
                        speechSkinInfo = prefab != null ? prefab.name : "(has SpeechSkin, prefab is null)";
                    }

                    // On Start Instructions
                    var onStartField = expression.GetType().GetField("m_InstructionsOnStart", BindingFlags.NonPublic | BindingFlags.Instance);
                    var onStart = onStartField?.GetValue(expression);
                    string onStartInfo = "(none)";
                    if (onStart != null)
                    {
                        var instrListField = onStart.GetType().GetField("m_Instructions", BindingFlags.NonPublic | BindingFlags.Instance);
                        var instrList = instrListField?.GetValue(onStart);
                        if (instrList != null)
                        {
                            var instrLengthProp = instrList.GetType().GetProperty("Length", BindingFlags.Public | BindingFlags.Instance);
                            int instrLength = (int)(instrLengthProp?.GetValue(instrList) ?? 0);
                            onStartInfo = $"{instrLength} instruction(s)";
                            for (int j = 0; j < instrLength; j++)
                            {
                                var getInstrMethod = instrList.GetType().GetMethod("Get", new[] { typeof(int) });
                                var instr = getInstrMethod?.Invoke(instrList, new object[] { j });
                                if (instr != null)
                                {
                                    string instrDetails = $"      [{j}] {instr.GetType().Name}: {instr}";
                                    // Special details for InstructionArithmeticSetNumber
                                    if (instr.GetType().Name == "InstructionArithmeticSetNumber")
                                    {
                                        // Title
                                        var titleProp = instr.GetType().GetProperty("Title", BindingFlags.Public | BindingFlags.Instance);
                                        string title = titleProp?.GetValue(instr)?.ToString() ?? "(null)";
                                        instrDetails += $"\n        Title: {title}";
                                        // m_Set
                                        var setField = instr.GetType().GetField("m_Set", BindingFlags.NonPublic | BindingFlags.Instance);
                                        var setObj = setField?.GetValue(instr);
                                        string setInfo = "(null)";
                                        if (setObj != null)
                                        {
                                            var mPropertyField = setObj.GetType().GetField("m_Property", BindingFlags.NonPublic | BindingFlags.Instance);
                                            var mProperty = mPropertyField?.GetValue(setObj);
                                            if (mProperty != null)
                                            {
                                                string setType = mProperty.GetType().Name;
                                                // Try to get m_Variable field if present
                                                var mVarField = mProperty.GetType().GetField("m_Variable", BindingFlags.NonPublic | BindingFlags.Instance);
                                                string varName = mVarField != null ? mVarField.GetValue(mProperty)?.ToString() : null;
                                                setInfo = $"{setType}" + (varName != null ? $" (variable: {varName})" : "");
                                            }
                                        }
                                        instrDetails += $"\n        Set Target: {setInfo}";
                                        // m_From
                                        var fromField = instr.GetType().GetField("m_From", BindingFlags.NonPublic | BindingFlags.Instance);
                                        var fromObj = fromField?.GetValue(instr);
                                        string fromInfo = "(null)";
                                        if (fromObj != null)
                                        {
                                            var mPropertyField = fromObj.GetType().GetField("m_Property", BindingFlags.NonPublic | BindingFlags.Instance);
                                            var mProperty = mPropertyField?.GetValue(fromObj);
                                            if (mProperty != null)
                                            {
                                                string fromType = mProperty.GetType().Name;
                                                // Try to get m_Value or m_Variable field if present
                                                var mValueField = mProperty.GetType().GetField("m_Value", BindingFlags.NonPublic | BindingFlags.Instance);
                                                var mVarField = mProperty.GetType().GetField("m_Variable", BindingFlags.NonPublic | BindingFlags.Instance);
                                                if (mValueField != null)
                                                {
                                                    var val = mValueField.GetValue(mProperty);
                                                    fromInfo = $"{fromType} (value: {val})";
                                                }
                                                else if (mVarField != null)
                                                {
                                                    var varName = mVarField.GetValue(mProperty)?.ToString();
                                                    fromInfo = $"{fromType} (variable: {varName})";
                                                }
                                                else
                                                {
                                                    fromInfo = fromType;
                                                }
                                            }
                                        }
                                        instrDetails += $"\n        From Value: {fromInfo}";
                                    }
                                    onStartInfo += $"\n{instrDetails}";
                                }
                            }
                        }
                    }

                    // On End Instructions
                    var onEndField = expression.GetType().GetField("m_InstructionsOnEnd", BindingFlags.NonPublic | BindingFlags.Instance);
                    var onEnd = onEndField?.GetValue(expression);
                    string onEndInfo = "(none)";
                    if (onEnd != null)
                    {
                        var instrListField = onEnd.GetType().GetField("m_Instructions", BindingFlags.NonPublic | BindingFlags.Instance);
                        var instrList = instrListField?.GetValue(onEnd);
                        if (instrList != null)
                        {
                            var instrLengthProp = instrList.GetType().GetProperty("Length", BindingFlags.Public | BindingFlags.Instance);
                            int instrLength = (int)(instrLengthProp?.GetValue(instrList) ?? 0);
                            onEndInfo = $"{instrLength} instruction(s)";
                            for (int j = 0; j < instrLength; j++)
                            {
                                var getInstrMethod = instrList.GetType().GetMethod("Get", new[] { typeof(int) });
                                var instr = getInstrMethod?.Invoke(instrList, new object[] { j });
                                if (instr != null)
                                {
                                    string instrDetails = $"      [{j}] {instr.GetType().Name}: {instr}";
                                    // Special details for InstructionArithmeticSetNumber
                                    if (instr.GetType().Name == "InstructionArithmeticSetNumber")
                                    {
                                        // Title
                                        var titleProp = instr.GetType().GetProperty("Title", BindingFlags.Public | BindingFlags.Instance);
                                        string title = titleProp?.GetValue(instr)?.ToString() ?? "(null)";
                                        instrDetails += $"\n        Title: {title}";
                                        // m_Set
                                        var setField = instr.GetType().GetField("m_Set", BindingFlags.NonPublic | BindingFlags.Instance);
                                        var setObj = setField?.GetValue(instr);
                                        string setInfo = "(null)";
                                        if (setObj != null)
                                        {
                                            var mPropertyField = setObj.GetType().GetField("m_Property", BindingFlags.NonPublic | BindingFlags.Instance);
                                            var mProperty = mPropertyField?.GetValue(setObj);
                                            if (mProperty != null)
                                            {
                                                string setType = mProperty.GetType().Name;
                                                // Try to get m_Variable field if present
                                                var mVarField = mProperty.GetType().GetField("m_Variable", BindingFlags.NonPublic | BindingFlags.Instance);
                                                string varName = mVarField != null ? mVarField.GetValue(mProperty)?.ToString() : null;
                                                setInfo = $"{setType}" + (varName != null ? $" (variable: {varName})" : "");
                                            }
                                        }
                                        instrDetails += $"\n        Set Target: {setInfo}";
                                        // m_From
                                        var fromField = instr.GetType().GetField("m_From", BindingFlags.NonPublic | BindingFlags.Instance);
                                        var fromObj = fromField?.GetValue(instr);
                                        string fromInfo = "(null)";
                                        if (fromObj != null)
                                        {
                                            var mPropertyField = fromObj.GetType().GetField("m_Property", BindingFlags.NonPublic | BindingFlags.Instance);
                                            var mProperty = mPropertyField?.GetValue(fromObj);
                                            if (mProperty != null)
                                            {
                                                string fromType = mProperty.GetType().Name;
                                                // Try to get m_Value or m_Variable field if present
                                                var mValueField = mProperty.GetType().GetField("m_Value", BindingFlags.NonPublic | BindingFlags.Instance);
                                                var mVarField = mProperty.GetType().GetField("m_Variable", BindingFlags.NonPublic | BindingFlags.Instance);
                                                if (mValueField != null)
                                                {
                                                    var val = mValueField.GetValue(mProperty);
                                                    fromInfo = $"{fromType} (value: {val})";
                                                }
                                                else if (mVarField != null)
                                                {
                                                    var varName = mVarField.GetValue(mProperty)?.ToString();
                                                    fromInfo = $"{fromType} (variable: {varName})";
                                                }
                                                else
                                                {
                                                    fromInfo = fromType;
                                                }
                                            }
                                        }
                                        instrDetails += $"\n        From Value: {fromInfo}";
                                    }
                                    onEndInfo += $"\n{instrDetails}";
                                }
                            }
                        }
                    }

                    Debug.Log($"  Expression {i}: ID='{id}', Sprite={spriteInfo}\n    SpeechSkin: {speechSkinInfo}\n    OnStart: {onStartInfo}\n    OnEnd: {onEndInfo}");
                }
                else
                {
                    Debug.Log($"  Expression {i}: (null)");
                }
            }
        }
        #endregion

        private void ReloadAmberBustTextures()
        {
            if (Characters.amber == null)
            {
                Debug.LogWarning("[Debug] Amber bust not found. Make sure Characters.cs has been executed first.");
                return;
            }

            Debug.Log("[Debug] Reloading Amber bust textures...");

            GameObject amberBust = Characters.amber;
            GameObject mBase = amberBust.transform.Find("MBase1").gameObject;
            GameObject blink = mBase.transform.Find("Blink").gameObject;
            GameObject mouth = mBase.transform.Find("Mouth").gameObject;
            GameObject expressions = mBase.transform.Find("Expressions").gameObject;

            string[] expressionNames = { "Happy", "Angry", "Sad", "Flirty" };

            try
            {
                // Reload base sprite
                Texture2D tex = new Texture2D(256, 256, TextureFormat.RGBA32, false);
                var rawData = System.IO.File.ReadAllBytes(Core.bustPath + "TEST\\T.PNG");
                tex.LoadImage(rawData);
                tex.filterMode = FilterMode.Point;
                Sprite newSprite = Sprite.Create(tex, new Rect(0, 0, 256, 256), new Vector2(0.5f, 0.5f));
                mBase.GetComponent<SpriteRenderer>().sprite = newSprite;
                Debug.Log("[Debug] Reloaded base sprite");

                // Reload blink sprite
                tex = new Texture2D(256, 256, TextureFormat.RGBA32, false);
                rawData = System.IO.File.ReadAllBytes(Core.bustPath + "TEST\\B.PNG");
                tex.LoadImage(rawData);
                tex.filterMode = FilterMode.Point;
                newSprite = Sprite.Create(tex, new Rect(0, 0, 256, 256), new Vector2(0.5f, 0.5f));
                blink.GetComponent<SpriteRenderer>().sprite = newSprite;
                Debug.Log("[Debug] Reloaded blink sprite");

                // Reload mask texture
                tex = new Texture2D(256, 256, TextureFormat.RGBA32, false);
                rawData = System.IO.File.ReadAllBytes(Core.bustPath + "TEST\\TM.PNG");
                tex.LoadImage(rawData);
                tex.filterMode = FilterMode.Point;
                mBase.GetComponent<SpriteRenderer>().material.SetTexture("_MaskTex", tex);
                Debug.Log("[Debug] Reloaded mask texture");

                // Reload mouth sprites
                for (int i = 1; i <= 4; i++)
                {
                    tex = new Texture2D(256, 256, TextureFormat.RGBA32, false);
                    rawData = System.IO.File.ReadAllBytes(Core.bustPath + "TEST\\Mouth" + i + ".PNG");
                    tex.LoadImage(rawData);
                    tex.filterMode = FilterMode.Point;
                    newSprite = Sprite.Create(tex, new Rect(0, 0, 256, 256), new Vector2(0.5f, 0.5f));
                    mouth.transform.Find(i.ToString()).GetComponent<SpriteRenderer>().sprite = newSprite;
                }
                Debug.Log("[Debug] Reloaded mouth sprites");

                // Reload expression sprites
                foreach (string expressionName in expressionNames)
                {
                    tex = new Texture2D(256, 256, TextureFormat.RGBA32, false);
                    rawData = System.IO.File.ReadAllBytes(Core.bustPath + "TEST\\Expression" + expressionName + ".PNG");
                    tex.LoadImage(rawData);
                    tex.filterMode = FilterMode.Point;
                    newSprite = Sprite.Create(tex, new Rect(0, 0, 256, 256), new Vector2(0.5f, 0.5f));
                    expressions.transform.Find(expressionName).GetComponent<SpriteRenderer>().sprite = newSprite;
                }
                Debug.Log("[Debug] Reloaded expression sprites");

                Debug.Log("[Debug] Amber bust texture reload completed successfully!");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[Debug] Error reloading Amber bust textures: {ex.Message}");
            }
        }

        public static void PrintAllActorExpressionsFromDialogue(GameObject dialogueGO, string actorName)
        {
            if (dialogueGO == null)
            {
                Debug.LogWarning("[PrintAllActorExpressionsFromDialogue] dialogueGO is null.");
                return;
            }

            var dialogue = dialogueGO.GetComponent(typeof(GameCreator.Runtime.Dialogue.Dialogue));
            if (dialogue == null)
            {
                Debug.LogWarning($"[PrintAllActorExpressionsFromDialogue] Dialogue component not found on {dialogueGO.name}.");
                return;
            }

            var storyProp = dialogue.GetType().GetProperty("Story", BindingFlags.Public | BindingFlags.Instance);
            var story = storyProp?.GetValue(dialogue);
            var contentProp = story?.GetType().GetProperty("Content", BindingFlags.Public | BindingFlags.Instance);
            var content = contentProp?.GetValue(story);

            var rolesField = content?.GetType().GetField("m_Roles", BindingFlags.NonPublic | BindingFlags.Instance);
            var roles = rolesField?.GetValue(content) as Array;
            if (roles == null)
            {
                Debug.LogWarning($"[PrintAllActorExpressionsFromDialogue] No roles found in dialogue content for {dialogueGO.name}.");
                return;
            }

            foreach (var roleObj in roles)
            {
                var actorField = roleObj.GetType().GetField("m_Actor", BindingFlags.NonPublic | BindingFlags.Instance);
                var actor = actorField?.GetValue(roleObj) as Actor;
                if (actor != null)
                {
                    var nameProp = actor.GetType().GetProperty("name", BindingFlags.Public | BindingFlags.Instance);
                    string thisActorName = nameProp?.GetValue(actor) as string;
                    if (thisActorName == actorName)
                    {
                        Debug.Log($"[PrintAllActorExpressionsFromDialogue] Found actor '{actorName}' in dialogue '{dialogueGO.name}'.");
                        PrintAllActorExpressions(actor);
                        return;
                    }
                }
            }
            Debug.LogWarning($"[PrintAllActorExpressionsFromDialogue] Actor '{actorName}' not found in dialogue '{dialogueGO.name}'.");
        }

        public static void PrintButtonInstructions(GameObject targetGameObject)
        {
            if (targetGameObject == null)
            {
                Debug.LogError("Target GameObject is null");
                return;
            }

            ButtonInstructions buttonInstructions = targetGameObject.GetComponent<ButtonInstructions>();
            if (buttonInstructions == null)
            {
                Debug.LogError($"ButtonInstructions component not found on GameObject: {targetGameObject.name}");
                return;
            }

            var instructionsField = buttonInstructions.GetType().GetField("m_Instructions", BindingFlags.NonPublic | BindingFlags.Instance);
            if (instructionsField != null)
            {
                var instructionListObj = instructionsField.GetValue(buttonInstructions);

                if (instructionListObj == null) return;
                Debug.Log($"Instructions: {instructionListObj.GetType().Name}");

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
                                Debug.Log($"  Instruction {instIdx}: {title} (Type: {instType.Name})");
                                // Print all private fields (especially m_ fields)
                                foreach (var field in instType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public))
                                {
                                    if (field.Name.StartsWith("m_"))
                                    {
                                        var fieldValue = field.GetValue(instruction);
                                        Debug.Log($"    {field.Name}: {fieldValue}");
                                    }
                                }
                            }
                            else
                            {
                                Debug.Log($"  Instruction {instIdx}: null");
                            }
                            instIdx++;
                        }
                    }
                    else
                    {
                        Debug.Log($"  m_Instructions array is null or not IEnumerable.");
                    }
                }
                else
                {
                    Debug.Log($"  Field 'm_Instructions' not found on InstructionList.");
                }
            }
            else
            {
                Debug.LogError("m_Instructions field not found on ButtonInstructions component.");
            }

            // Inspect OnClick event
            var onClickEvent = buttonInstructions.onClick;
            if (onClickEvent != null)
            {
                Debug.Log("\n=== OnClick Event Listeners ===");
                for (int i = 0; i < onClickEvent.GetPersistentEventCount(); i++)
                {
                    var target = onClickEvent.GetPersistentTarget(i);
                    var methodName = onClickEvent.GetPersistentMethodName(i);

                    Debug.Log($"  Listener {i}: Target={target?.GetType().Name}, Method={methodName}");

                    // If the method is RunInstructions, we already printed those
                    if (methodName == "RunInstructions" && target == buttonInstructions)
                    {
                        Debug.Log("    (Skipping RunInstructions - already printed above)");
                        continue;
                    }

                    //If the target is a GameObject, we should check its components for instructionLists
                    if (target is GameObject targetGO)
                    {
                        Debug.Log($"    Target is a GameObject: {targetGO.name}");
                        PrintConditionsAndTriggers(targetGO); // Use PrintConditionsAndTriggers to check this GO
                    }
                    else if (target != null)  //If target isnt null check it for instruction lists
                    {
                        Debug.Log($"    Target is: {target.GetType().Name}");
                        PrintConditionsAndTriggers(target as GameObject);
                    }

                    // Check if the target has an InstructionList and print it (if we can access it)
                    // (This part is tricky because we need to know the field name and type)
                    /*
                    if (target != null)
                    {
                        //Attempt to get InstructionList via reflection (if it exists)
                        var instructionListField = target.GetType().GetField("m_Instructions", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                        if (instructionListField != null)
                        {
                            var instructionList = instructionListField.GetValue(target);
                            if (instructionList != null)
                            {
                                Debug.Log($"    Found InstructionList on target: {instructionList.GetType().Name}");
                                //PrintInstructionListDetails(instructionList); // Print instruction list details - using the embedded function
                                PrintInstructionListDetails(instructionList); // Print instruction list details
                            }
                        }
                    }
                    */
                }
            }

            // Print all Instructions, Conditions, and Triggers
            PrintConditionsAndTriggers(targetGameObject);

        }

    }
}
#endregion
#endregion