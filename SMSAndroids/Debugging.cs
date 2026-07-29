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
        public bool isMonitoring = false;
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
                //if (Input.GetKeyDown(KeyCode.R))
                //{
                //    ReloadAmberBustTextures();
                //    Core.mainCanvas.gameObject.SetActive(false);
                //    Core.level.Find("5_MyRoom").Find("PlayerRoom_ButtonCanvas").gameObject.SetActive(false);
                //}

                // Check for P key press to print all Proxy Variables
                if (Input.GetKeyDown(KeyCode.P))
                {
                    Minigames.minigameMassage.GetComponent<MassageMinigame>().LoadPatterns();
                    Minigames.minigameMassage.GetComponent<MassageMinigame>().LoadVisualAssets();
                    Minigames.minigameMassage.GetComponent<MassageMinigame>().StartMinigame();
                }

                //if (Input.GetKeyDown(KeyCode.V))
                //{ 
                //    Schedule.anisLocation = "HarborHomeLivingRoomCouchleft";
                //    Debug.Log(Schedule.anisLocation);
                //}
                //if (Input.GetKeyDown(KeyCode.B))
                //{

                //    if (Schedule.amberLocation == "HarborHomeBathroom")
                //    {
                //        Schedule.amberLocation = "HarborHomeBedroom";
                //    }
                //    else
                //    {
                //        Schedule.amberLocation = "HarborHomeBathroom";
                //    }
                //    SaveManager.SetBool("HarborHome_Visit_Amber", true);
                //    Debug.Log(Schedule.amberLocation);
                //}

                //// Check for F key press to find all SetActive instructions in Dialogues
                //if (Input.GetKeyDown(KeyCode.F))
                //{
                //    FindAllSetActiveInstructionsInDialogues();
                //}
                //if (Input.GetKeyDown(KeyCode.I))
                //{
                //    Core.FindAndModifyProxyVariableBool("DailyProc_Tove-Trail-1", true);
                //}
                //StartMonitoringGlobalVariables();
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

            // Print all nodes recursively (if accessible)
            var getNodeMethod = contentType.GetMethod("Get", BindingFlags.Public | BindingFlags.Instance);
            if (getNodeMethod != null && rootIds != null)
            {
                HashSet<int> visitedNodes = new HashSet<int>();
                foreach (var nodeId in rootIds)
                {
                    PrintDialogueNodeRecursive(content, story, contentType, getNodeMethod, nodeId, 0, visitedNodes);
                }
            }

            Debug.Log($"=== End Dialogue Component Info for {targetGameObject.name} ===");
        }

        /// <summary>
        /// Recursively prints dialogue node and its children
        /// </summary>
        private static void PrintDialogueNodeRecursive(object content, object story, Type contentType, MethodInfo getNodeMethod, int nodeId, int depth, HashSet<int> visitedNodes)
        {
            if (visitedNodes.Contains(nodeId))
            {
                return; // Already processed this node
            }

            visitedNodes.Add(nodeId);

            // Get the node
            var node = getNodeMethod.Invoke(content, new object[] { nodeId });
            if (node == null)
            {
                return;
            }

            string indent = new string(' ', depth * 2);
            Debug.Log($"{indent}--- Node {nodeId} ---");

            var nodeType = node.GetType();

            // Print node text
            var textField = nodeType.GetField("m_Text", BindingFlags.NonPublic | BindingFlags.Instance);
            string nodeText = textField?.GetValue(node)?.ToString() ?? "(no text)";
            Debug.Log($"{indent}  Text: {nodeText}");

            // Print node type
            var nodeTypeField = nodeType.GetField("m_NodeType", BindingFlags.NonPublic | BindingFlags.Instance);
            Debug.Log($"{indent}  NodeType: {nodeTypeField?.GetValue(node)}");

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

                Debug.Log($"{indent}  Acting: Actor Asset='{actorAssetName}', Name='{actorName}', Desc='{actorDesc}', ExpressionIdx={exprIdx}, ExpressionName={exprName}, Portrait={portrait}");
            }

            // Get child node IDs from the tree structure
            var childrenMethod = contentType.GetMethod("Children", BindingFlags.Public | BindingFlags.Instance);
            if (childrenMethod != null)
            {
                try
                {
                    var childNodeIds = childrenMethod.Invoke(content, new object[] { nodeId }) as List<int>;
                    if (childNodeIds != null && childNodeIds.Count > 0)
                    {
                        Debug.Log($"{indent}  Child Nodes: [{string.Join(", ", childNodeIds)}]");
                        foreach (var childNodeId in childNodeIds)
                        {
                            PrintDialogueNodeRecursive(content, story, contentType, getNodeMethod, childNodeId, depth + 1, visitedNodes);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"{indent}  Could not get child nodes: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Prints a structured, human-readable dump of a GC2 Dialogue component.
        /// For each node in tree order: text, actor, expression, node type,
        /// conditions (with variable names/values), OnStart instructions, OnFinish instructions.
        /// </summary>
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

            Debug.Log($"╔══════════════════════════════════════════════════════");
            Debug.Log($"║ DIALOGUE: {targetGameObject.name}");
            Debug.Log($"╚═════════���═════════════════════════���══════════════════");

            var storyProp = dialogue.GetType().GetProperty("Story", BindingFlags.Public | BindingFlags.Instance);
            var story = storyProp?.GetValue(dialogue);
            if (story == null) { Debug.Log("Story is null"); return; }

            var contentProp = story.GetType().GetProperty("Content", BindingFlags.Public | BindingFlags.Instance);
            var content = contentProp?.GetValue(story);
            if (content == null) { Debug.Log("Content is null"); return; }

            var contentType = content.GetType();

            // Print roles
            var rolesField = contentType.GetField("m_Roles", BindingFlags.NonPublic | BindingFlags.Instance);
            var roles = rolesField?.GetValue(content) as Array;
            if (roles != null && roles.Length > 0)
            {
                Debug.Log($"\n── Roles ({roles.Length}) ──");
                int idx = 0;
                foreach (var role in roles)
                {
                    string actorName = DialogueDeep_GetActorName(role);
                    Debug.Log($"  [{idx}] {actorName}");
                    idx++;
                }
            }

            // Print dialogue skin name
            var skinField = contentType.GetField("m_DialogueSkin", BindingFlags.NonPublic | BindingFlags.Instance);
            if (skinField != null)
            {
                var skin = skinField.GetValue(content);
                if (skin != null)
                {
                    var skinNameProp = skin.GetType().GetProperty("name", BindingFlags.Public | BindingFlags.Instance);
                    string skinName = skinNameProp?.GetValue(skin)?.ToString() ?? "(unknown)";
                    Debug.Log($"\n── Dialogue Skin: {skinName} ──");
                }
            }

            // Walk the tree
            var rootIdsProp = contentType.GetProperty("RootIds", BindingFlags.Public | BindingFlags.Instance);
            var rootIds = rootIdsProp?.GetValue(content) as int[];
            var getNodeMethod = contentType.GetMethod("Get", BindingFlags.Public | BindingFlags.Instance);
            var childrenMethod = contentType.GetMethod("Children", BindingFlags.Public | BindingFlags.Instance);

            if (rootIds == null || getNodeMethod == null)
            {
                Debug.Log("Could not access RootIds or Get method");
                return;
            }

            Debug.Log($"\n── Nodes (roots: {rootIds.Length}) ──");
            var visited = new HashSet<int>();
            foreach (var rootId in rootIds)
            {
                DialogueDeep_PrintNodeRecursive(content, contentType, getNodeMethod, childrenMethod, rootId, 0, visited);
            }

            Debug.Log($"\n══════════════════════════════════════════════════════");
            Debug.Log($" END DIALOGUE: {targetGameObject.name}");
            Debug.Log($"═════���═══════════��════════════════════════════════════");
        }

        /// <summary>
        /// Recursively prints a dialogue node and all children in tree order.
        /// </summary>
        private static void DialogueDeep_PrintNodeRecursive(object content, Type contentType, MethodInfo getNodeMethod, MethodInfo childrenMethod, int nodeId, int depth, HashSet<int> visited)
        {
            if (visited.Contains(nodeId)) return;
            visited.Add(nodeId);

            var node = getNodeMethod.Invoke(content, new object[] { nodeId });
            if (node == null) return;

            var nodeType = node.GetType();
            string indent = new string(' ', depth * 2);
            string bar = depth > 0 ? "├─ " : "";

            // --- Node header: type + text ---
            string nodeTypeName = DialogueDeep_GetNodeTypeName(node);
            string nodeText = DialogueDeep_GetNodeText(node);

            Debug.Log($"\n{indent}{bar}[{nodeTypeName}] Node {nodeId}");
            if (!string.IsNullOrEmpty(nodeText))
                Debug.Log($"{indent}  Text: \"{nodeText}\"");

            // --- Actor / Expression ---
            var actingField = nodeType.GetField("m_Acting", BindingFlags.NonPublic | BindingFlags.Instance);
            var acting = actingField?.GetValue(node);
            if (acting != null)
            {
                var actorField = acting.GetType().GetField("m_Actor", BindingFlags.NonPublic | BindingFlags.Instance);
                var exprField = acting.GetType().GetField("m_Expression", BindingFlags.NonPublic | BindingFlags.Instance);
                var actor = actorField?.GetValue(acting);
                int exprIdx = exprField != null ? (int)exprField.GetValue(acting) : -1;

                if (actor != null)
                {
                    string actorName = actor.GetType().GetProperty("name", BindingFlags.Public | BindingFlags.Instance)?.GetValue(actor)?.ToString() ?? "(unknown)";
                    string exprName = DialogueDeep_GetExpressionName(actor, exprIdx);
                    Debug.Log($"{indent}  Actor: {actorName}, Expression: {exprName} (idx {exprIdx})");
                }
            }

            // --- Duration ---
            var durationField = nodeType.GetField("m_Duration", BindingFlags.NonPublic | BindingFlags.Instance);
            if (durationField != null)
            {
                var duration = durationField.GetValue(node);
                Debug.Log($"{indent}  Duration: {duration}");
            }

            // --- Jump ---
            var jumpField = nodeType.GetField("m_Jump", BindingFlags.NonPublic | BindingFlags.Instance);
            if (jumpField != null)
            {
                var jump = jumpField.GetValue(node);
                if (jump != null)
                {
                    var jumpTypeField = jump.GetType().GetField("m_Jump", BindingFlags.NonPublic | BindingFlags.Instance);
                    var jumpType = jumpTypeField?.GetValue(jump);
                    if (jumpType != null && jumpType.ToString() != "Continue")
                    {
                        var jumpToField = jump.GetType().GetField("m_JumpTo", BindingFlags.NonPublic | BindingFlags.Instance);
                        var jumpTo = jumpToField?.GetValue(jump);
                        string jumpToStr = jumpTo != null ? DialogueDeep_ResolveGC2Value(jumpTo) : "";
                        Debug.Log($"{indent}  Jump: {jumpType}{(string.IsNullOrEmpty(jumpToStr) ? "" : " -> " + jumpToStr)}");
                    }
                }
            }

            // --- Conditions ---
            DialogueDeep_PrintConditions(node, indent);

            // --- OnStart instructions ---
            DialogueDeep_PrintInstructionsList(node, "m_OnStart", "OnStart", indent);

            // --- OnFinish instructions ---
            DialogueDeep_PrintInstructionsList(node, "m_OnFinish", "OnFinish", indent);

            // --- Recurse into children ---
            if (childrenMethod != null)
            {
                try
                {
                    var childIds = childrenMethod.Invoke(content, new object[] { nodeId }) as List<int>;
                    if (childIds != null && childIds.Count > 0)
                    {
                        foreach (var childId in childIds)
                        {
                            DialogueDeep_PrintNodeRecursive(content, contentType, getNodeMethod, childrenMethod, childId, depth + 1, visited);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"{indent}  (error getting children: {ex.Message})");
                }
            }
        }

        /// <summary>
        /// Gets the concrete node type name (Text, Choice, Random, etc.)
        /// </summary>
        private static string DialogueDeep_GetNodeTypeName(object node)
        {
            var nodeTypeField = node.GetType().GetField("m_NodeType", BindingFlags.NonPublic | BindingFlags.Instance);
            var nodeTypeObj = nodeTypeField?.GetValue(node);
            if (nodeTypeObj == null) return "Unknown";

            string fullName = nodeTypeObj.GetType().Name;
            // Strip "NodeType" prefix for readability: "NodeTypeChoice" -> "Choice"
            if (fullName.StartsWith("NodeType"))
                return fullName.Substring(8);
            return fullName;
        }

        /// <summary>
        /// Extracts the dialogue text from a Node's m_Text field.
        /// </summary>
        private static string DialogueDeep_GetNodeText(object node)
        {
            var textField = node.GetType().GetField("m_Text", BindingFlags.NonPublic | BindingFlags.Instance);
            var nodeText = textField?.GetValue(node);
            if (nodeText == null) return null;

            // NodeText has m_Text (PropertyGetString) -> m_Property -> String
            var innerTextField = nodeText.GetType().GetField("m_Text", BindingFlags.NonPublic | BindingFlags.Instance);
            var propertyGetString = innerTextField?.GetValue(nodeText);
            if (propertyGetString == null) return null;

            // Try the EditorValue property first (doesn't need Args), then String
            var editorValueProp = propertyGetString.GetType().GetProperty("EditorValue", BindingFlags.Public | BindingFlags.Instance);
            if (editorValueProp != null)
            {
                try
                {
                    var val = editorValueProp.GetValue(propertyGetString);
                    if (val != null) return val.ToString();
                }
                catch { }
            }

            // Fall back to m_Property -> String
            var mPropertyField = propertyGetString.GetType().GetField("m_Property", BindingFlags.NonPublic | BindingFlags.Instance);
            var innerProp = mPropertyField?.GetValue(propertyGetString);
            if (innerProp != null)
            {
                var stringProp = innerProp.GetType().GetProperty("String", BindingFlags.Public | BindingFlags.Instance);
                try { return stringProp?.GetValue(innerProp)?.ToString(); } catch { }

                var edVal = innerProp.GetType().GetProperty("EditorValue", BindingFlags.Public | BindingFlags.Instance);
                try { return edVal?.GetValue(innerProp)?.ToString(); } catch { }
            }

            return nodeText.ToString();
        }

        /// <summary>
        /// Resolves an actor expression name by index.
        /// </summary>
        private static string DialogueDeep_GetExpressionName(object actor, int exprIdx)
        {
            if (actor == null || exprIdx < 0) return "(none)";
            var expressionsField = actor.GetType().GetField("m_Expressions", BindingFlags.NonPublic | BindingFlags.Instance);
            var expressions = expressionsField?.GetValue(actor);
            if (expressions == null) return $"(idx {exprIdx})";

            var fromIndexMethod = expressions.GetType().GetMethod("FromIndex", BindingFlags.Public | BindingFlags.Instance);
            if (fromIndexMethod != null)
            {
                try
                {
                    var exprObj = fromIndexMethod.Invoke(expressions, new object[] { exprIdx });
                    if (exprObj != null)
                    {
                        var idField = exprObj.GetType().GetField("m_Id", BindingFlags.NonPublic | BindingFlags.Instance);
                        var idObj = idField?.GetValue(exprObj);
                        if (idObj != null)
                        {
                            var strProp = idObj.GetType().GetProperty("String", BindingFlags.Public | BindingFlags.Instance);
                            return strProp?.GetValue(idObj)?.ToString() ?? $"(idx {exprIdx})";
                        }
                    }
                }
                catch { }
            }

            return $"(idx {exprIdx})";
        }

        /// <summary>
        /// Gets a role's actor display name.
        /// </summary>
        private static string DialogueDeep_GetActorName(object role)
        {
            if (role == null) return "(null)";
            var actorField = role.GetType().GetField("m_Actor", BindingFlags.NonPublic | BindingFlags.Instance);
            var actor = actorField?.GetValue(role);
            if (actor == null) return "(no actor)";
            var nameProp = actor.GetType().GetProperty("name", BindingFlags.Public | BindingFlags.Instance);
            return nameProp?.GetValue(actor)?.ToString() ?? "(unknown)";
        }

        /// <summary>
        /// Prints the conditions on a dialogue node.
        /// Path: Node.m_Conditions (RunConditionsList) -> m_Conditions (ConditionList) -> m_Conditions (Condition[])
        /// </summary>
        private static void DialogueDeep_PrintConditions(object node, string indent)
        {
            var condField = node.GetType().GetField("m_Conditions", BindingFlags.NonPublic | BindingFlags.Instance);
            var runConditionsList = condField?.GetValue(node);
            if (runConditionsList == null) return;

            var condListField = runConditionsList.GetType().GetField("m_Conditions", BindingFlags.NonPublic | BindingFlags.Instance);
            var conditionList = condListField?.GetValue(runConditionsList);
            if (conditionList == null) return;

            var condArrayField = conditionList.GetType().GetField("m_Conditions", BindingFlags.NonPublic | BindingFlags.Instance);
            var conditionsArray = condArrayField?.GetValue(conditionList) as Array;
            if (conditionsArray == null || conditionsArray.Length == 0) return;

            Debug.Log($"{indent}  Conditions ({conditionsArray.Length}):");
            int idx = 0;
            foreach (var condition in conditionsArray)
            {
                if (condition == null) { Debug.Log($"{indent}    [{idx}] (null)"); idx++; continue; }

                var condType = condition.GetType();
                var titleProp = condType.GetProperty("Title", BindingFlags.Public | BindingFlags.Instance);
                string title = titleProp?.GetValue(condition)?.ToString() ?? condType.Name;

                var signField = condType.GetField("m_Sign", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
                bool sign = signField != null ? (bool)signField.GetValue(condition) : true;
                string negated = sign ? "" : " [NEGATED]";

                Debug.Log($"{indent}    [{idx}] {condType.Name}: \"{title}\"{negated}");

                // Print all m_ fields on the condition, walking the full type hierarchy
                DialogueDeep_PrintFieldsRecursive(condition, indent + "      ");

                idx++;
            }
        }

        /// <summary>
        /// Prints an instructions list (OnStart or OnFinish) from a dialogue node.
        /// Path: Node.m_OnStart/m_OnFinish (RunInstructionsList) -> m_Instructions (InstructionList) -> m_Instructions (Instruction[])
        /// </summary>
        private static void DialogueDeep_PrintInstructionsList(object node, string fieldName, string label, string indent)
        {
            var runListField = node.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            var runInstructionsList = runListField?.GetValue(node);
            if (runInstructionsList == null) return;

            var instrListField = runInstructionsList.GetType().GetField("m_Instructions", BindingFlags.NonPublic | BindingFlags.Instance);
            var instructionList = instrListField?.GetValue(runInstructionsList);
            if (instructionList == null) return;

            var instrArrayField = instructionList.GetType().GetField("m_Instructions", BindingFlags.NonPublic | BindingFlags.Instance);
            var instrArray = instrArrayField?.GetValue(instructionList) as Array;
            if (instrArray == null || instrArray.Length == 0) return;

            Debug.Log($"{indent}  {label} ({instrArray.Length}):");
            int idx = 0;
            foreach (var instruction in instrArray)
            {
                if (instruction == null) { Debug.Log($"{indent}    [{idx}] (null)"); idx++; continue; }

                var instrType = instruction.GetType();
                var titleProp = instrType.GetProperty("Title", BindingFlags.Public | BindingFlags.Instance);
                string title = titleProp?.GetValue(instruction)?.ToString() ?? "(no title)";

                var isEnabledProp = instrType.GetProperty("IsEnabled", BindingFlags.Public | BindingFlags.Instance);
                bool enabled = isEnabledProp != null ? (bool)isEnabledProp.GetValue(instruction) : true;
                string disabledStr = enabled ? "" : " [DISABLED]";

                Debug.Log($"{indent}    [{idx}] {instrType.Name}: \"{title}\"{disabledStr}");

                // Print all m_ fields on the instruction, walking the full type hierarchy
                DialogueDeep_PrintFieldsRecursive(instruction, indent + "      ");

                idx++;
            }
        }

        /// <summary>
        /// Prints all m_ fields on an object, walking up the type hierarchy until TPolymorphicItem.
        /// Uses GetFieldValueDisplayString to resolve GC2 property wrappers, Unity objects, etc.
        /// For fields that look like variable detectors (have m_Variable + m_Name), prints the
        /// resolved variable asset name and variable key name.
        /// </summary>
        private static void DialogueDeep_PrintFieldsRecursive(object obj, string indent)
        {
            if (obj == null) return;

            Type currentType = obj.GetType();
            while (currentType != null && currentType != typeof(object) && !currentType.Name.StartsWith("TPolymorphicItem"))
            {
                var fields = currentType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                foreach (var field in fields)
                {
                    // Skip base class plumbing fields
                    if (field.Name == "m_Sign" || field.Name == "m_IsEnabled" || field.Name == "m_Breakpoint")
                        continue;
                    if (!field.Name.StartsWith("m_")) continue;

                    try
                    {
                        var fieldValue = field.GetValue(obj);
                        string valueStr = GetFieldValueDisplayString(fieldValue);

                        // Special handling: if this is a variable reference (m_Variable), also print the asset name
                        if (field.Name == "m_Variable" && fieldValue != null && fieldValue is UnityEngine.Object unityObj)
                        {
                            Debug.Log($"{indent}{field.Name}: {unityObj.name} ({field.FieldType.Name})");
                        }
                        // Special handling: if this is a variable name (m_Name with a .String property)
                        else if (field.Name == "m_Name" && fieldValue != null)
                        {
                            var stringProp = fieldValue.GetType().GetProperty("String", BindingFlags.Public | BindingFlags.Instance);
                            if (stringProp != null)
                            {
                                try
                                {
                                    var nameStr = stringProp.GetValue(fieldValue);
                                    Debug.Log($"{indent}{field.Name}: \"{nameStr}\" ({field.FieldType.Name})");
                                    continue;
                                }
                                catch { }
                            }
                            Debug.Log($"{indent}{field.Name}: {valueStr}");
                        }
                        else
                        {
                            Debug.Log($"{indent}{field.Name}: {valueStr}");
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.Log($"{indent}{field.Name}: <error: {e.Message}>");
                    }
                }
                currentType = currentType.BaseType;
            }
        }

        /// <summary>
        /// Attempts to resolve a GC2 property wrapper or value to a readable string.
        /// Handles primitives, enums, Unity Objects, PropertyGetString, IdString, etc.
        /// </summary>
        private static string DialogueDeep_ResolveGC2Value(object value)
        {
            if (value == null) return "null";
            var type = value.GetType();
            if (type.IsPrimitive || value is string) return value.ToString();
            if (type.IsEnum) return value.ToString();
            if (value is UnityEngine.Object uo) return uo != null ? uo.name : "null (destroyed)";

            var stringProp = type.GetProperty("String", BindingFlags.Public | BindingFlags.Instance);
            if (stringProp != null) { try { return stringProp.GetValue(value)?.ToString(); } catch { } }

            var valueProp = type.GetProperty("Value", BindingFlags.Public | BindingFlags.Instance);
            if (valueProp != null) { try { return valueProp.GetValue(value)?.ToString(); } catch { } }

            return $"({type.Name})";
        }

        /// <summary>
        /// Prints details of a condition's private fields
        /// </summary>
        private static void PrintConditionDetails(object condition, string indent)
        {
            if (condition == null) return;

            var condType = condition.GetType();
            var fields = condType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

            foreach (var field in fields)
            {
                // Skip base class fields we've already handled
                if (field.Name == "m_Sign" || field.Name == "m_IsEnabled" || field.Name == "m_Breakpoint")
                    continue;

                if (field.Name.StartsWith("m_"))
                {
                    try
                    {
                        var value = field.GetValue(condition);
                        string valueStr = GetConditionFieldValueString(value);
                        Debug.Log($"{indent}{field.Name}: {valueStr}");
                    }
                    catch (Exception ex)
                    {
                        Debug.Log($"{indent}{field.Name}: (error reading: {ex.Message})");
                    }
                }
            }
        }

        /// <summary>
        /// Converts a condition field value to a readable string
        /// </summary>
        private static string GetConditionFieldValueString(object value)
        {
            if (value == null) return "null";

            var type = value.GetType();

            // Handle primitives and strings
            if (type.IsPrimitive || value is string || value is decimal)
                return value.ToString();

            // Handle enums
            if (type.IsEnum)
                return $"{value} ({type.Name})";

            // Handle Unity Objects
            if (value is UnityEngine.Object unityObj)
                return $"{type.Name} (name='{unityObj.name}')";

            // Try to get a String property (common in GameCreator property wrappers)
            var stringProp = type.GetProperty("String", BindingFlags.Public | BindingFlags.Instance);
            if (stringProp != null)
            {
                try
                {
                    var str = stringProp.GetValue(value);
                    if (str != null) return str.ToString();
                }
                catch { }
            }

            // Try to get a Value property
            var valueProp = type.GetProperty("Value", BindingFlags.Public | BindingFlags.Instance);
            if (valueProp != null)
            {
                try
                {
                    var val = valueProp.GetValue(value);
                    if (val != null) return $"{val} ({type.Name})";
                }
                catch { }
            }

            // For GlobalNameVariables or similar, try to get the asset name
            if (type.Name.Contains("GlobalNameVariable") || type.Name.Contains("LocalNameVariable"))
            {
                var assetField = type.GetField("m_Variable", BindingFlags.NonPublic | BindingFlags.Instance);
                if (assetField != null)
                {
                    var asset = assetField.GetValue(value);
                    if (asset is UnityEngine.Object assetObj)
                        return $"{type.Name} -> {assetObj.name}";
                }

                var nameField = type.GetField("m_Name", BindingFlags.NonPublic | BindingFlags.Instance);
                if (nameField != null)
                {
                    var nameVal = nameField.GetValue(value);
                    var nameProp = nameVal?.GetType().GetProperty("String", BindingFlags.Public | BindingFlags.Instance);
                    if (nameProp != null)
                    {
                        var nameStr = nameProp.GetValue(nameVal);
                        if (nameStr != null) return $"{type.Name} -> Variable: {nameStr}";
                    }
                }
            }

            // Default: just return type name
            return $"({type.Name})";
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

        public static void PrintActionsComponentInfo(GameObject targetGameObject, bool includeChildren = false)
        {
            if (targetGameObject == null)
            {
                Debug.LogError("Target GameObject is null");
                return;
            }

            var actionsList = includeChildren
                ? targetGameObject.GetComponentsInChildren<MonoBehaviour>(true)
                    .Where(c => c != null && (c.GetType().Name == "Actions" || c.GetType().IsSubclassOf(typeof(MonoBehaviour)) && c.GetType().BaseType?.Name == "BaseActions"))
                    .ToList()
                : targetGameObject.GetComponents<MonoBehaviour>()
                    .Where(c => c != null && (c.GetType().Name == "Actions" || c.GetType().IsSubclassOf(typeof(MonoBehaviour)) && c.GetType().BaseType?.Name == "BaseActions"))
                    .ToList();

            if (actionsList.Count == 0)
            {
                Debug.Log($"[PrintActions] No Actions components found on {targetGameObject.name}{(includeChildren ? " (including children)" : "")}");
                return;
            }

            Debug.Log($"[PrintActions] Found {actionsList.Count} Actions component(s) on {targetGameObject.name}{(includeChildren ? " (including children)" : "")}");

            for (int actionsIdx = 0; actionsIdx < actionsList.Count; actionsIdx++)
            {
                var actions = actionsList[actionsIdx];
                string gameObjectPath = GetGameObjectPath(actions.gameObject);
                Debug.Log($"  === Actions [{actionsIdx}] on: {gameObjectPath} ===");
                Debug.Log($"    Component Type: {actions.GetType().FullName}");
                Debug.Log($"    Enabled: {actions.enabled}, GameObject Active: {actions.gameObject.activeInHierarchy}");

                // Get IsRunning and RunningIndex via reflection (properties on BaseActions)
                var isRunningProp = actions.GetType().GetProperty("IsRunning", BindingFlags.Public | BindingFlags.Instance);
                var runningIndexProp = actions.GetType().GetProperty("RunningIndex", BindingFlags.Public | BindingFlags.Instance);
                if (isRunningProp != null)
                {
                    var isRunning = isRunningProp.GetValue(actions);
                    Debug.Log($"    IsRunning: {isRunning}");
                }
                if (runningIndexProp != null)
                {
                    var runningIndex = runningIndexProp.GetValue(actions);
                    Debug.Log($"    RunningIndex: {runningIndex}");
                }

                // Get m_Instructions field (defined on BaseActions)
                var instructionsField = actions.GetType().GetField("m_Instructions", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
                if (instructionsField == null)
                {
                    // Try walking the hierarchy manually
                    Type currentType = actions.GetType();
                    while (currentType != null && currentType != typeof(object))
                    {
                        instructionsField = currentType.GetField("m_Instructions", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                        if (instructionsField != null) break;
                        currentType = currentType.BaseType;
                    }
                }

                if (instructionsField == null)
                {
                    Debug.LogWarning($"    Could not find m_Instructions field on {actions.GetType().Name}");
                    continue;
                }

                var instructionList = instructionsField.GetValue(actions);
                if (instructionList == null)
                {
                    Debug.Log($"    m_Instructions is null");
                    continue;
                }

                Debug.Log($"    InstructionList Type: {instructionList.GetType().Name}");

                // Check IsRunning / IsCancelled on the InstructionList itself
                var listRunningProp = instructionList.GetType().GetProperty("IsRunning", BindingFlags.Public | BindingFlags.Instance);
                var listCancelledProp = instructionList.GetType().GetProperty("IsCancelled", BindingFlags.Public | BindingFlags.Instance);
                if (listRunningProp != null)
                    Debug.Log($"    InstructionList.IsRunning: {listRunningProp.GetValue(instructionList)}");
                if (listCancelledProp != null)
                    Debug.Log($"    InstructionList.IsCancelled: {listCancelledProp.GetValue(instructionList)}");

                // Get the m_Instructions array inside InstructionList
                var instrArrayField = instructionList.GetType().GetField("m_Instructions", BindingFlags.NonPublic | BindingFlags.Instance);
                if (instrArrayField == null)
                {
                    Debug.LogWarning($"    Could not find m_Instructions array field on InstructionList");
                    continue;
                }

                var instrArray = instrArrayField.GetValue(instructionList) as Instruction[];
                if (instrArray == null)
                {
                    // Try as IEnumerable fallback
                    var instrEnumerable = instrArrayField.GetValue(instructionList) as System.Collections.IEnumerable;
                    if (instrEnumerable == null)
                    {
                        Debug.Log($"    Instructions array is null");
                        continue;
                    }

                    int idx = 0;
                    foreach (var instr in instrEnumerable)
                    {
                        PrintSingleInstruction(instr, idx, "    ");
                        idx++;
                    }
                    if (idx == 0) Debug.Log($"    (No instructions)");
                }
                else
                {
                    Debug.Log($"    Instruction Count: {instrArray.Length}");
                    for (int i = 0; i < instrArray.Length; i++)
                    {
                        PrintSingleInstruction(instrArray[i], i, "    ");
                    }
                }
            }
        }

        private static void PrintSingleInstruction(object instruction, int index, string baseIndent)
        {
            string indent = baseIndent + "  ";
            if (instruction == null)
            {
                Debug.Log($"{indent}[{index}] (null)");
                return;
            }

            var instrType = instruction.GetType();

            // Get Title property
            var titleProp = instrType.GetProperty("Title", BindingFlags.Public | BindingFlags.Instance);
            string title = titleProp?.GetValue(instruction)?.ToString() ?? "(no title)";

            // Get IsEnabled from base TPolymorphicItem
            var isEnabledProp = instrType.GetProperty("IsEnabled", BindingFlags.Public | BindingFlags.Instance);
            string enabledStr = isEnabledProp != null ? $", Enabled: {isEnabledProp.GetValue(instruction)}" : "";

            Debug.Log($"{indent}[{index}] {instrType.Name}: \"{title}\"{enabledStr}");

            // Print all m_ fields declared on this instruction type and its hierarchy up to Instruction
            Type currentType = instrType;
            while (currentType != null && currentType != typeof(object) && currentType.Name != "TPolymorphicItem`1")
            {
                var fields = currentType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                foreach (var field in fields)
                {
                    if (!field.Name.StartsWith("m_")) continue;
                    try
                    {
                        var fieldValue = field.GetValue(instruction);
                        string valueStr = GetFieldValueDisplayString(fieldValue);
                        Debug.Log($"{indent}  {field.Name} ({field.FieldType.Name}): {valueStr}");
                    }
                    catch (Exception e)
                    {
                        Debug.Log($"{indent}  {field.Name}: <error: {e.Message}>");
                    }
                }
                currentType = currentType.BaseType;
            }
        }

        private static string GetFieldValueDisplayString(object value)
        {
            if (value == null) return "null";

            var type = value.GetType();

            if (type.IsPrimitive || value is string || value is decimal)
                return value.ToString();

            if (type.IsEnum)
                return $"{type.Name}.{value}";

            if (value is UnityEngine.Object unityObj)
                return unityObj != null ? $"{unityObj.name} ({type.Name})" : "null (destroyed)";

            // Try .String property (common in GC2 wrappers like IdString, PropertyGetString, etc.)
            var stringProp = type.GetProperty("String", BindingFlags.Public | BindingFlags.Instance);
            if (stringProp != null)
            {
                try
                {
                    var strVal = stringProp.GetValue(value);
                    if (strVal != null) return $"\"{strVal}\" ({type.Name})";
                }
                catch { }
            }

            // Try .Value property
            var valueProp = type.GetProperty("Value", BindingFlags.Public | BindingFlags.Instance);
            if (valueProp != null)
            {
                try
                {
                    var val = valueProp.GetValue(value);
                    if (val != null) return $"{val} ({type.Name})";
                }
                catch { }
            }

            // Try .Get property or .name
            var nameProp = type.GetProperty("name", BindingFlags.Public | BindingFlags.Instance);
            if (nameProp != null)
            {
                try
                {
                    var nameVal = nameProp.GetValue(value);
                    if (nameVal != null) return $"\"{nameVal}\" ({type.Name})";
                }
                catch { }
            }

            return $"({type.Name})";
        }

        private static string GetGameObjectPath(GameObject obj)
        {
            string path = obj.name;
            Transform parent = obj.transform.parent;
            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }
            return path;
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

            Characters.amber.SetActive(true);
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

        /// <summary>
        /// Exhaustive dump of a ButtonInstructions component — everything needed to
        /// replicate the button: GameObject context (path, RectTransform geometry,
        /// sibling components), the full Selectable/Button visual state (transition,
        /// ColorBlock, SpriteState, AnimationTriggers, targetGraphic), every field of
        /// the component across its whole inheritance chain with deep recursion into
        /// GC2 objects (InstructionList → instructions → nested Transitions/getters),
        /// onClick persistent listeners, and a one-line summary of each child GO.
        /// </summary>
        public static void PrintButtonInstructionsFull(GameObject targetGameObject, int maxDepth = 5)
        {
            if (targetGameObject == null)
            {
                Debug.LogError("[BtnFull] Target GameObject is null");
                return;
            }

            ButtonInstructions btn = targetGameObject.GetComponent<ButtonInstructions>();
            if (btn == null)
            {
                Debug.LogError($"[BtnFull] ButtonInstructions component not found on: {targetGameObject.name}");
                return;
            }

            Debug.Log($"[BtnFull] ================ FULL DUMP: {GetGameObjectPath(targetGameObject)} ================");

            // ── GameObject context ─────────────────────────────────────
            Debug.Log($"[BtnFull] GO: name={targetGameObject.name} activeSelf={targetGameObject.activeSelf} activeInHierarchy={targetGameObject.activeInHierarchy} layer={LayerMask.LayerToName(targetGameObject.layer)} tag={targetGameObject.tag} siblingIndex={targetGameObject.transform.GetSiblingIndex()}");
            if (targetGameObject.transform is RectTransform rt)
            {
                Debug.Log($"[BtnFull] RectTransform: anchorMin={rt.anchorMin} anchorMax={rt.anchorMax} pivot={rt.pivot}");
                Debug.Log($"[BtnFull]   anchoredPosition={rt.anchoredPosition} sizeDelta={rt.sizeDelta} offsetMin={rt.offsetMin} offsetMax={rt.offsetMax}");
                Debug.Log($"[BtnFull]   localScale={rt.localScale} localRotation={rt.localEulerAngles} worldPos={rt.position}");
            }

            Debug.Log($"[BtnFull] --- All components on this GO ---");
            foreach (var comp in targetGameObject.GetComponents<Component>())
            {
                if (comp == null) { Debug.Log("[BtnFull]   (missing script)"); continue; }
                string extra = "";
                if (comp is UnityEngine.UI.Image img)
                    extra = $" | sprite={(img.sprite != null ? img.sprite.name : "(none)")} color={img.color} type={img.type} raycastTarget={img.raycastTarget} material={(img.material != null ? img.material.name : "(none)")}";
                else if (comp is CanvasGroup cg)
                    extra = $" | alpha={cg.alpha} interactable={cg.interactable} blocksRaycasts={cg.blocksRaycasts}";
                else if (comp is TMP_Text tmp)
                    extra = $" | text=\"{tmp.text}\" font={(tmp.font != null ? tmp.font.name : "(none)")} size={tmp.fontSize} color={tmp.color}";
                Debug.Log($"[BtnFull]   * {comp.GetType().Name}{extra}");
            }

            // ── Selectable / Button visual state ───────────────────────
            Debug.Log($"[BtnFull] --- Selectable state ---");
            Debug.Log($"[BtnFull]   interactable={btn.interactable} transition={btn.transition}");
            Debug.Log($"[BtnFull]   targetGraphic={(btn.targetGraphic != null ? btn.targetGraphic.name + " (" + btn.targetGraphic.GetType().Name + ")" : "(none)")}");
            if (btn.targetGraphic is UnityEngine.UI.Image tgImg)
                Debug.Log($"[BtnFull]     targetGraphic sprite={(tgImg.sprite != null ? tgImg.sprite.name : "(none)")} color={tgImg.color}");
            var cb = btn.colors;
            Debug.Log($"[BtnFull]   ColorBlock: normal={cb.normalColor} highlighted={cb.highlightedColor} pressed={cb.pressedColor} selected={cb.selectedColor} disabled={cb.disabledColor} multiplier={cb.colorMultiplier} fadeDuration={cb.fadeDuration}");
            var ss = btn.spriteState;
            Debug.Log($"[BtnFull]   SpriteState: highlighted={(ss.highlightedSprite != null ? ss.highlightedSprite.name : "(none)")} pressed={(ss.pressedSprite != null ? ss.pressedSprite.name : "(none)")} selected={(ss.selectedSprite != null ? ss.selectedSprite.name : "(none)")} disabled={(ss.disabledSprite != null ? ss.disabledSprite.name : "(none)")}");
            var at = btn.animationTriggers;
            Debug.Log($"[BtnFull]   AnimationTriggers: normal=\"{at.normalTrigger}\" highlighted=\"{at.highlightedTrigger}\" pressed=\"{at.pressedTrigger}\" selected=\"{at.selectedTrigger}\" disabled=\"{at.disabledTrigger}\"");
            Debug.Log($"[BtnFull]   Navigation: mode={btn.navigation.mode}");

            // ── Every field on the component, whole inheritance chain ──
            Debug.Log($"[BtnFull] --- Deep field dump (type chain: {DescribeTypeChain(btn.GetType())}) ---");
            var seen = new HashSet<object>();
            DumpFieldsRecursive(btn, "", 1, maxDepth, seen);

            // ── onClick persistent listeners ───────────────────────────
            Debug.Log($"[BtnFull] --- onClick persistent listeners ---");
            var onClick = btn.onClick;
            if (onClick == null || onClick.GetPersistentEventCount() == 0)
                Debug.Log("[BtnFull]   (none)");
            else
                for (int i = 0; i < onClick.GetPersistentEventCount(); i++)
                {
                    var target = onClick.GetPersistentTarget(i);
                    Debug.Log($"[BtnFull]   Listener {i}: target={(target != null ? target.name + " (" + target.GetType().Name + ")" : "(null)")} method={onClick.GetPersistentMethodName(i)}");
                }

            // ── Children summary (icons/labels live here) ──────────────
            Debug.Log($"[BtnFull] --- Children ({targetGameObject.transform.childCount}) ---");
            foreach (Transform child in targetGameObject.transform)
            {
                string comps = string.Join(", ", child.GetComponents<Component>()
                    .Where(c => c != null && !(c is Transform))
                    .Select(c => c.GetType().Name).ToArray());
                string visual = "";
                var cImg = child.GetComponent<UnityEngine.UI.Image>();
                if (cImg != null) visual += $" sprite={(cImg.sprite != null ? cImg.sprite.name : "(none)")} color={cImg.color}";
                var cTmp = child.GetComponent<TMP_Text>();
                if (cTmp != null) visual += $" text=\"{cTmp.text}\"";
                Debug.Log($"[BtnFull]   - {child.name} [active={child.gameObject.activeSelf}] ({comps}){visual}");
            }

            Debug.Log($"[BtnFull] ================ END DUMP: {targetGameObject.name} ================");
        }

        private static string DescribeTypeChain(Type type)
        {
            var names = new List<string>();
            for (Type t = type; t != null && t != typeof(object); t = t.BaseType) names.Add(t.Name);
            return string.Join(" → ", names.ToArray());
        }

        /// <summary>
        /// Recursively prints every instance field of an object (public + private,
        /// including private fields declared on base classes, which GetFields()
        /// alone won't return). UnityEngine.Object references are printed by name
        /// without recursion; GC2 POCOs (instructions, transitions, getters,
        /// IdString…) are expanded up to maxDepth; collections are enumerated;
        /// cycles are guarded by reference identity.
        /// </summary>
        private static void DumpFieldsRecursive(object obj, string indent, int depth, int maxDepth, HashSet<object> seen)
        {
            if (obj == null) return;
            Type type = obj.GetType();

            // Collect fields across the whole chain — DeclaredOnly per level so
            // base-class private fields (m_Interactable etc.) are included.
            for (Type t = type; t != null && t != typeof(object); t = t.BaseType)
            {
                foreach (var field in t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    object value;
                    try { value = field.GetValue(obj); }
                    catch (Exception ex) { Debug.Log($"[BtnFull] {indent}{field.Name}: <unreadable: {ex.GetType().Name}>"); continue; }
                    DumpValueRecursive(value, $"{field.Name} ({t.Name})", indent, depth, maxDepth, seen);
                }
            }
        }

        private static void DumpValueRecursive(object value, string label, string indent, int depth, int maxDepth, HashSet<object> seen)
        {
            if (value == null) { Debug.Log($"[BtnFull] {indent}{label}: null"); return; }
            Type vt = value.GetType();

            // Leaves: primitives, strings, enums, common Unity structs.
            if (vt.IsPrimitive || vt.IsEnum || value is string || value is Vector2 || value is Vector3 ||
                value is Vector4 || value is Color || value is Rect || value is Quaternion || value is LayerMask)
            {
                Debug.Log($"[BtnFull] {indent}{label}: {value}");
                return;
            }

            // Unity objects: identify, never recurse (sprites, GOs, materials…).
            if (value is UnityEngine.Object uo)
            {
                Debug.Log($"[BtnFull] {indent}{label}: '{(uo != null ? uo.name : "(destroyed)")}' ({vt.Name})");
                return;
            }

            // Delegates and UnityEvents: summarize, don't expand internals.
            if (value is Delegate del)
            {
                Debug.Log($"[BtnFull] {indent}{label}: <delegate {vt.Name}, {del.GetInvocationList().Length} handler(s)>");
                return;
            }
            if (value is UnityEngine.Events.UnityEventBase ueb)
            {
                Debug.Log($"[BtnFull] {indent}{label}: <UnityEvent {vt.Name}, {ueb.GetPersistentEventCount()} persistent listener(s)>");
                return;
            }

            if (depth > maxDepth) { Debug.Log($"[BtnFull] {indent}{label}: {value} <max depth>"); return; }
            if (!vt.IsValueType)
            {
                if (seen.Contains(value)) { Debug.Log($"[BtnFull] {indent}{label}: <cycle to {vt.Name}>"); return; }
                seen.Add(value);
            }

            // Collections: enumerate elements.
            if (value is IEnumerable en && !(value is string))
            {
                var items = en.Cast<object>().ToList();
                Debug.Log($"[BtnFull] {indent}{label}: {vt.Name} [{items.Count} element(s)]");
                for (int i = 0; i < items.Count; i++)
                    DumpValueRecursive(items[i], $"[{i}]", indent + "  ", depth + 1, maxDepth, seen);
                return;
            }

            // GC2 POCO: title (if any) + recurse into its fields.
            string title = null;
            try { title = vt.GetProperty("Title", BindingFlags.Public | BindingFlags.Instance)?.GetValue(value)?.ToString(); }
            catch { /* some Titles throw outside play state */ }
            Debug.Log($"[BtnFull] {indent}{label}: {vt.Name}{(string.IsNullOrEmpty(title) || title == vt.Name ? "" : $" \"{title}\"")}");
            DumpFieldsRecursive(value, indent + "  ", depth + 1, maxDepth, seen);
        }

        public static void PrintToggleOnValueChanged(GameObject targetGameObject)
        {
            if (targetGameObject == null)
            {
                Debug.LogError("Target GameObject is null");
                return;
            }

            UnityEngine.UI.Toggle toggle = targetGameObject.GetComponent<UnityEngine.UI.Toggle>();
            if (toggle == null)
            {
                Debug.LogError($"Toggle component not found on GameObject: {targetGameObject.name}");
                return;
            }

            Debug.Log($"=== Toggle OnValueChanged for: {targetGameObject.name} ===");
            Debug.Log($"Current isOn value: {toggle.isOn}");

            // Check if it's a TogglePropertyBool
            if (toggle is TogglePropertyBool)
            {
                Debug.Log("Toggle Type: TogglePropertyBool");
                var togglePropertyBool = toggle as TogglePropertyBool;

                // Get the m_SetFromSource field
                var setFromSourceField = typeof(TogglePropertyBool).GetField("m_SetFromSource", BindingFlags.NonPublic | BindingFlags.Instance);
                if (setFromSourceField != null)
                {
                    var setFromSource = setFromSourceField.GetValue(togglePropertyBool);
                    Debug.Log($"  m_SetFromSource: {setFromSource}");
                }

                // Get the m_OnChangeSet field (PropertySetBool)
                var onChangeSetField = typeof(TogglePropertyBool).GetField("m_OnChangeSet", BindingFlags.NonPublic | BindingFlags.Instance);
                if (onChangeSetField != null)
                {
                    var onChangeSet = onChangeSetField.GetValue(togglePropertyBool);
                    if (onChangeSet != null)
                    {
                        Debug.Log($"  m_OnChangeSet: {onChangeSet.GetType().Name}");
                        
                        // Try to get details from PropertySetBool
                        var propertySetBoolType = onChangeSet.GetType();
                        foreach (var field in propertySetBoolType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public))
                        {
                            if (field.Name.StartsWith("m_"))
                            {
                                var fieldValue = field.GetValue(onChangeSet);
                                Debug.Log($"    {field.Name}: {GetConditionFieldValueString(fieldValue)}");
                            }
                        }
                    }
                    else
                    {
                        Debug.Log("  m_OnChangeSet is null");
                    }
                }
            }
            else
            {
                Debug.Log("Toggle Type: Standard Toggle");
            }

            // Inspect OnValueChanged event listeners
            var onValueChangedEvent = toggle.onValueChanged;
            if (onValueChangedEvent != null)
            {
                Debug.Log("\n=== OnValueChanged Event Listeners ===");
                int persistentListenerCount = onValueChangedEvent.GetPersistentEventCount();
                Debug.Log($"Persistent Listener Count: {persistentListenerCount}");

                for (int i = 0; i < persistentListenerCount; i++)
                {
                    var target = onValueChangedEvent.GetPersistentTarget(i);
                    var methodName = onValueChangedEvent.GetPersistentMethodName(i);
                    
                    Debug.Log($"  Listener {i}:");
                    Debug.Log($"    Target: {(target != null ? target.GetType().Name : "null")}");
                    if (target is UnityEngine.Object unityTarget)
                    {
                        Debug.Log($"    Target Name: {unityTarget.name}");
                    }
                    Debug.Log($"    Method: {methodName}");

                    // If the target is a GameObject or has interesting components, analyze it
                    if (target is GameObject targetGO)
                    {
                        Debug.Log($"    Target is GameObject: {targetGO.name}");
                    }
                }

                // Try to get runtime listeners using reflection (these are added via AddListener)
                var onValueChangedField = typeof(UnityEngine.UI.Toggle).GetField("onValueChanged", BindingFlags.NonPublic | BindingFlags.Instance);
                if (onValueChangedField == null)
                {
                    onValueChangedField = typeof(UnityEngine.UI.Toggle).GetField("m_OnValueChanged", BindingFlags.NonPublic | BindingFlags.Instance);
                }

                if (onValueChangedField != null)
                {
                    var unityEvent = onValueChangedField.GetValue(toggle);
                    if (unityEvent != null)
                    {
                        // Try to get runtime calls
                        var callsField = unityEvent.GetType().GetField("m_Calls", BindingFlags.NonPublic | BindingFlags.Instance);
                        if (callsField != null)
                        {
                            var calls = callsField.GetValue(unityEvent);
                            if (calls != null)
                            {
                                var runtimeCallsField = calls.GetType().GetField("m_RuntimeCalls", BindingFlags.NonPublic | BindingFlags.Instance);
                                if (runtimeCallsField != null)
                                {
                                    var runtimeCalls = runtimeCallsField.GetValue(calls) as System.Collections.IList;
                                    if (runtimeCalls != null && runtimeCalls.Count > 0)
                                    {
                                        Debug.Log($"\n=== Runtime Listeners (AddListener) ===");
                                        Debug.Log($"Runtime Listener Count: {runtimeCalls.Count}");
                                        
                                        for (int i = 0; i < runtimeCalls.Count; i++)
                                        {
                                            var call = runtimeCalls[i];
                                            if (call != null)
                                            {
                                                Debug.Log($"  Runtime Listener {i}: {call.GetType().Name}");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                Debug.Log("onValueChanged is null");
            }
        }

        public static void PrintAllGlobalNameVariables()
        {
            var manager = GlobalNameVariablesManager.Instance;
            if (manager == null)
            {
                Debug.LogError("[PrintAllGlobalNameVariables] GlobalNameVariablesManager not initialized");
                return;
            }

            PropertyInfo valuesProp = typeof(GlobalNameVariablesManager).GetProperty(
                "Values",
                BindingFlags.NonPublic | BindingFlags.Instance
            );

            if (valuesProp == null)
            {
                Debug.LogError("[PrintAllGlobalNameVariables] Could not access Values property");
                return;
            }

            var values = valuesProp.GetValue(manager) as Dictionary<IdString, NameVariableRuntime>;
            if (values == null || values.Count == 0)
            {
                Debug.LogWarning("[PrintAllGlobalNameVariables] No global name variables found");
                return;
            }

            var repo = TRepository<VariablesRepository>.Get;
            if (repo == null)
            {
                Debug.LogError("[PrintAllGlobalNameVariables] VariablesRepository not accessible");
                return;
            }

            Debug.Log("=== ALL GLOBAL NAME VARIABLES ===");
            
            foreach (var pair in values)
            {
                var asset = repo.Variables.GetNameVariablesAsset(pair.Key);
                string assetName = asset != null ? asset.name : $"Unknown Asset ({pair.Key})";

                PropertyInfo runtimeVarsProp = typeof(NameVariableRuntime).GetProperty(
                    "Variables",
                    BindingFlags.NonPublic | BindingFlags.Instance
                );

                if (runtimeVarsProp == null) continue;

                var variables = runtimeVarsProp.GetValue(pair.Value) as Dictionary<string, NameVariable>;
                if (variables == null || variables.Count == 0)
                {
                    Debug.Log($"[{assetName}] (No variables)");
                    continue;
                }

                Debug.Log($"[{assetName}] ({variables.Count} variables)");
                foreach (var varPair in variables)
                {
                    object value = varPair.Value?.Value;
                    string typeName = value != null ? value.GetType().Name : "null";
                    Debug.Log($"  - {varPair.Key}: {value ?? "null"} (Type: {typeName})");
                }
            }

            Debug.Log("=== END GLOBAL NAME VARIABLES ===");
        }

        public static void PrintProxyVariables()
        {
            if (Core.proxyVariables == null)
            {
                Debug.LogError("[PrintProxyVariables] Proxy Variables not initialized");
                return;
            }

            Debug.Log("=== PROXY VARIABLES ===");
            Debug.Log($"Asset Name: {Core.proxyVariables.name}");
            Debug.Log($"UniqueID: {Core.proxyVariables.UniqueID}");

            // Get all variable names from the proxy variables
            string[] variableNames = Core.proxyVariables.Names;

            if (variableNames == null || variableNames.Length == 0)
            {
                Debug.LogWarning("[PrintProxyVariables] No variables found in Proxy Variables asset");
                Debug.Log("=== END PROXY VARIABLES ===");
                return;
            }

            Debug.Log($"Total Variables: {variableNames.Length}");

            foreach (string varName in variableNames)
            {
                if (Core.proxyVariables.Exists(varName))
                {
                    object value = Core.proxyVariables.Get(varName);
                    string typeName = value != null ? value.GetType().Name : "null";
                    string title = Core.proxyVariables.Title(varName);
                    
                    Debug.Log($"  - {varName}: {value ?? "null"} (Type: {typeName}, Title: \"{title}\")");
                }
                else
                {
                    Debug.LogWarning($"  - {varName}: [DOES NOT EXIST]");
                }
            }

            Debug.Log("=== END PROXY VARIABLES ===");
        }

        public static IEnumerator PrintAllGlobalNameVariablesDelayed(float delay)
        {
            yield return new WaitForSeconds(delay);
            PrintAllGlobalNameVariables();
        }

        public static void PrintAllSignals()
        {
            Debug.Log("=== ALL ACTIVE SIGNALS ===");

            try
            {
                // Use reflection to access the private SIGNALS dictionary
                var signalsType = typeof(Signals);
                var signalsField = signalsType.GetField("SIGNALS", BindingFlags.NonPublic | BindingFlags.Static);

                if (signalsField == null)
                {
                    Debug.LogError("[PrintAllSignals] Could not access SIGNALS field via reflection");
                    return;
                }

                var signalsDictionary = signalsField.GetValue(null) as Dictionary<PropertyName, List<ISignalReceiver>>;
                if (signalsDictionary == null)
                {
                    Debug.LogWarning("[PrintAllSignals] SIGNALS dictionary is null");
                    Debug.Log("=== END ALL ACTIVE SIGNALS ===");
                    return;
                }

                if (signalsDictionary.Count == 0)
                {
                    Debug.LogWarning("[PrintAllSignals] No active signals found");
                    Debug.Log("=== END ALL ACTIVE SIGNALS ===");
                    return;
                }

                Debug.Log($"Total Active Signals: {signalsDictionary.Count}");

                foreach (var signal in signalsDictionary)
                {
                    int receiverCount = signal.Value?.Count ?? 0;
                    
                    // Try to get the actual signal name from the first receiver (Trigger)
                    // since PropertyName only stores a hash, not the original string
                    string signalName = $"Hash:{signal.Key.GetHashCode()}";
                    
                    if (signal.Value != null && signal.Value.Count > 0)
                    {
                        var firstReceiver = signal.Value[0];
                        string extractedName = TryGetSignalNameFromReceiver(firstReceiver);
                        if (!string.IsNullOrEmpty(extractedName))
                        {
                            signalName = extractedName;
                        }
                    }
                    
                    Debug.Log($"  Signal: '{signalName}' - {receiverCount} receiver(s)");

                    if (signal.Value != null && signal.Value.Count > 0)
                    {
                        for (int i = 0; i < signal.Value.Count; i++)
                        {
                            var receiver = signal.Value[i];
                            if (receiver is MonoBehaviour monoBehaviour)
                            {
                                Debug.Log($"    [{i}] {monoBehaviour.GetType().Name} on GameObject '{monoBehaviour.gameObject.name}'");
                            }
                            else
                            {
                                Debug.Log($"    [{i}] {receiver?.GetType().Name ?? "null"}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PrintAllSignals] Error retrieving signals: {ex.Message}");
            }

            Debug.Log("=== END ALL ACTIVE SIGNALS ===");
        }

        private static string TryGetSignalNameFromReceiver(ISignalReceiver receiver)
        {
            if (receiver == null) return null;

            try
            {
                var receiverType = receiver.GetType();
                
                // Get m_TriggerEvent field (which is EventOnReceiveSignal)
                var triggerEventField = receiverType.GetField("m_TriggerEvent", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                if (triggerEventField != null)
                {
                    var triggerEvent = triggerEventField.GetValue(receiver);
                    if (triggerEvent != null && triggerEvent.GetType().Name == "EventOnReceiveSignal")
                    {
                        // Now look for the signal field inside EventOnReceiveSignal
                        var eventType = triggerEvent.GetType();
                        
                        // Try common field names for the signal
                        string[] signalFieldNames = { "m_Signal", "m_SignalName", "m_Name", "m_PropertyName" };
                        
                        foreach (var fieldName in signalFieldNames)
                        {
                            var signalField = eventType.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                            if (signalField != null)
                            {
                                var signalValue = signalField.GetValue(triggerEvent);
                                if (signalValue != null)
                                {
                                    // If it's a string directly
                                    if (signalValue is string strVal && !string.IsNullOrEmpty(strVal))
                                        return strVal;
                                    
                                    // Try to get String property (GameCreator wrapper types often have this)
                                    var stringProp = signalValue.GetType().GetProperty("String", BindingFlags.Public | BindingFlags.Instance);
                                    if (stringProp != null)
                                    {
                                        var str = stringProp.GetValue(signalValue);
                                        if (str is string s && !string.IsNullOrEmpty(s))
                                            return s;
                                    }
                                    
                                    // Try m_String field
                                    var mStringField = signalValue.GetType().GetField("m_String", BindingFlags.NonPublic | BindingFlags.Instance);
                                    if (mStringField != null)
                                    {
                                        var str = mStringField.GetValue(signalValue);
                                        if (str is string s && !string.IsNullOrEmpty(s))
                                            return s;
                                    }
                                }
                            }
                        }
                        
                        // If we still haven't found it, dump all fields of EventOnReceiveSignal for debugging
                        Debug.Log($"    [Debug] EventOnReceiveSignal fields:");
                        var allEventFields = eventType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                        foreach (var f in allEventFields)
                        {
                            var val = f.GetValue(triggerEvent);
                            Debug.Log($"    [Debug]   {f.Name}: {val} ({val?.GetType().Name ?? "null"})");
                            
                            // If this field looks like it could contain the signal, dig deeper
                            if (val != null && f.Name.ToLower().Contains("signal"))
                            {
                                var innerFields = val.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                                foreach (var innerF in innerFields)
                                {
                                    var innerVal = innerF.GetValue(val);
                                    Debug.Log($"    [Debug]     -> {innerF.Name}: {innerVal} ({innerVal?.GetType().Name ?? "null"})");
                                }
                                var innerProps = val.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
                                foreach (var innerP in innerProps)
                                {
                                    try
                                    {
                                        var innerVal = innerP.GetValue(val);
                                        Debug.Log($"    [Debug]     -> {innerP.Name} (prop): {innerVal} ({innerVal?.GetType().Name ?? "null"})");
                                    }
                                    catch { }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[TryGetSignalNameFromReceiver] Error: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Helper method to write to dialogue debug log file
        /// </summary>
        private static void LogToDialogueFile(string message)
        {
            try
            {
                string logDir = Path.Combine(new DirectoryInfo(Application.dataPath).Parent.FullName);
                string logPath = Path.Combine(logDir, "DialogueSetActiveDebug.log");
                
                // Append timestamp to each message
                string timestampedMessage = $"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}";
                
                // Write to file
                File.AppendAllText(logPath, timestampedMessage + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to write to dialogue log file: {ex.Message}");
            }
        }

        /// <summary>
        /// Finds all GameObjects with a Dialogue component in the scene (active or inactive),
        /// searches through their dialogue structure for nodes with "Set Active" instructions,
        /// and logs the path of nodes needed to reach that instruction along with the target GO name.
        /// </summary>
        public static void FindAllSetActiveInstructionsInDialogues()
        {
            LogToDialogueFile("=== Searching All Dialogues for SetActive Instructions ===");

            // Get all GameObjects in the scene including inactive ones
            var allGameObjects = new List<GameObject>();
            foreach (var rootGO in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                CollectAllGameObjects(rootGO.transform, allGameObjects);
            }

            LogToDialogueFile($"Found {allGameObjects.Count} total GameObjects in scene");

            int dialogueCount = 0;
            int setActiveFoundCount = 0;

            foreach (var go in allGameObjects)
            {
                var dialogue = go.GetComponent(typeof(GameCreator.Runtime.Dialogue.Dialogue));
                if (dialogue == null) continue;

                dialogueCount++;

                // Get the Story property
                var dialogueType = dialogue.GetType();
                var storyProp = dialogueType.GetProperty("Story", BindingFlags.Public | BindingFlags.Instance);
                var story = storyProp?.GetValue(dialogue);
                if (story == null)
                {
                    LogToDialogueFile($"[{go.name}] Dialogue.Story is null");
                    continue;
                }

                // Get Content property
                var storyType = story.GetType();
                var contentProp = storyType.GetProperty("Content", BindingFlags.Public | BindingFlags.Instance);
                var content = contentProp?.GetValue(story);
                if (content == null)
                {
                    LogToDialogueFile($"[{go.name}] Story.Content is null");
                    continue;
                }

                var contentType = content.GetType();

                // Get RootIds
                var rootIdsProp = contentType.GetProperty("RootIds", BindingFlags.Public | BindingFlags.Instance);
                var rootIds = rootIdsProp?.GetValue(content) as int[];
                if (rootIds == null || rootIds.Length == 0)
                {
                    continue;
                }

                // Get methods for traversing the tree
                var getNodeMethod = contentType.GetMethod("Get", BindingFlags.Public | BindingFlags.Instance);
                var childrenMethod = contentType.GetMethod("Children", BindingFlags.Public | BindingFlags.Instance);

                if (getNodeMethod == null || childrenMethod == null)
                {
                    LogToDialogueFile($"[{go.name}] Could not find Get or Children methods on Content");
                    continue;
                }

                // Search through all nodes starting from roots
                HashSet<int> visitedNodes = new HashSet<int>();
                List<int> currentPath = new List<int>();

                foreach (var rootId in rootIds)
                {
                    SearchNodeForSetActiveInstructions(
                        go.name,
                        content,
                        contentType,
                        getNodeMethod,
                        childrenMethod,
                        rootId,
                        visitedNodes,
                        currentPath,
                        ref setActiveFoundCount
                    );
                }
            }

            LogToDialogueFile($"=== Search Complete: Found {dialogueCount} Dialogues, {setActiveFoundCount} SetActive Instructions ===");
        }

        /// <summary>
        /// Recursively collects all GameObjects from a transform hierarchy
        /// </summary>
        private static void CollectAllGameObjects(Transform parent, List<GameObject> result)
        {
            result.Add(parent.gameObject);
            foreach (Transform child in parent)
            {
                CollectAllGameObjects(child, result);
            }
        }

        /// <summary>
        /// Recursively searches a dialogue node and its children for SetActive instructions
        /// </summary>
        private static void SearchNodeForSetActiveInstructions(
            string dialogueGOName,
            object content,
            Type contentType,
            MethodInfo getNodeMethod,
            MethodInfo childrenMethod,
            int nodeId,
            HashSet<int> visitedNodes,
            List<int> currentPath,
            ref int setActiveFoundCount)
        {
            if (visitedNodes.Contains(nodeId))
                return;

            visitedNodes.Add(nodeId);

            // Get the node
            var node = getNodeMethod.Invoke(content, new object[] { nodeId });
            if (node == null)
                return;

            // Add current node to path
            currentPath.Add(nodeId);

            var nodeType = node.GetType();

            // Get node text for identification
            var textField = nodeType.GetField("m_Text", BindingFlags.NonPublic | BindingFlags.Instance);
            string nodeText = textField?.GetValue(node)?.ToString() ?? "(no text)";
            if (nodeText.Length > 50)
                nodeText = nodeText.Substring(0, 50) + "...";

            // Check m_OnStart instructions
            var onStartField = nodeType.GetField("m_OnStart", BindingFlags.NonPublic | BindingFlags.Instance);
            if (onStartField != null)
            {
                var onStart = onStartField.GetValue(node);
                if (onStart != null)
                {
                    CheckInstructionListForSetActive(
                        dialogueGOName,
                        onStart,
                        "OnStart",
                        currentPath,
                        nodeText,
                        content,
                        getNodeMethod,
                        ref setActiveFoundCount
                    );
                }
            }

            // Check m_OnFinish instructions
            var onFinishField = nodeType.GetField("m_OnFinish", BindingFlags.NonPublic | BindingFlags.Instance);
            if (onFinishField != null)
            {
                var onFinish = onFinishField.GetValue(node);
                if (onFinish != null)
                {
                    CheckInstructionListForSetActive(
                        dialogueGOName,
                        onFinish,
                        "OnFinish",
                        currentPath,
                        nodeText,
                        content,
                        getNodeMethod,
                        ref setActiveFoundCount
                    );
                }
            }

            // Get child nodes and recurse
            try
            {
                var childNodeIds = childrenMethod.Invoke(content, new object[] { nodeId }) as List<int>;
                if (childNodeIds != null && childNodeIds.Count > 0)
                {
                    foreach (var childNodeId in childNodeIds)
                    {
                        SearchNodeForSetActiveInstructions(
                            dialogueGOName,
                            content,
                            contentType,
                            getNodeMethod,
                            childrenMethod,
                            childNodeId,
                            visitedNodes,
                            currentPath,
                            ref setActiveFoundCount
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                LogToDialogueFile($"[{dialogueGOName}] Error getting children for node {nodeId}: {ex.Message}");
            }

            // Remove current node from path when backtracking
            currentPath.RemoveAt(currentPath.Count - 1);
        }

        /// <summary>
        /// Checks an instruction list (RunInstructionsList) for SetActive instructions
        /// </summary>
        private static void CheckInstructionListForSetActive(
            string dialogueGOName,
            object runInstructionsList,
            string listName,
            List<int> nodePath,
            string currentNodeText,
            object content,
            MethodInfo getNodeMethod,
            ref int setActiveFoundCount)
        {
            if (runInstructionsList == null)
                return;

            // Get m_Instructions field from RunInstructionsList
            var instructionsField = runInstructionsList.GetType().GetField("m_Instructions", BindingFlags.NonPublic | BindingFlags.Instance);
            if (instructionsField == null)
                return;

            var instructionList = instructionsField.GetValue(runInstructionsList);
            if (instructionList == null)
                return;

            // Get m_Instructions array from InstructionList
            var instructionsArrayField = instructionList.GetType().GetField("m_Instructions", BindingFlags.NonPublic | BindingFlags.Instance);
            if (instructionsArrayField == null)
                return;

            var instructionsArray = instructionsArrayField.GetValue(instructionList) as System.Collections.IEnumerable;
            if (instructionsArray == null)
                return;

            int instIdx = 0;
            foreach (var instruction in instructionsArray)
            {
                if (instruction == null)
                {
                    instIdx++;
                    continue;
                }

                var instType = instruction.GetType();
                string instTypeName = instType.Name;

                // Check if this is a SetActive instruction
                if (instTypeName == "InstructionGameObjectSetActive" || 
                    instTypeName.Contains("SetActive"))
                {
                    setActiveFoundCount++;

                    // Try to get the target GameObject name from m_GameObject field
                    string targetGOName = "(unknown)";
                    var gameObjectField = instType.GetField("m_GameObject", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                    if (gameObjectField == null)
                    {
                        // Try inherited field from TInstructionGameObject
                        gameObjectField = instType.BaseType?.GetField("m_GameObject", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                    }

                    if (gameObjectField != null)
                    {
                        var goProperty = gameObjectField.GetValue(instruction);
                        if (goProperty != null)
                        {
                            // Try to get the string representation which usually contains the GO reference
                            targetGOName = goProperty.ToString();

                            // Also try to dig deeper into PropertyGetGameObject to find actual name
                            var mPropertyField = goProperty.GetType().GetField("m_Property", BindingFlags.NonPublic | BindingFlags.Instance);
                            if (mPropertyField != null)
                            {
                                var mProperty = mPropertyField.GetValue(goProperty);
                                if (mProperty != null)
                                {
                                    // Try m_GameObject field on the property type
                                    var innerGOField = mProperty.GetType().GetField("m_GameObject", BindingFlags.NonPublic | BindingFlags.Instance);
                                    if (innerGOField != null)
                                    {
                                        var innerGO = innerGOField.GetValue(mProperty);
                                        if (innerGO != null)
                                        {
                                            targetGOName = innerGO.ToString();
                                            
                                            // If it's a Unity Object, try to get its name
                                            if (innerGO is UnityEngine.Object unityObj && unityObj != null)
                                            {
                                                targetGOName = unityObj.name;
                                            }
                                        }
                                    }

                                    // Alternative: try String property
                                    var stringProp = mProperty.GetType().GetProperty("String", BindingFlags.Public | BindingFlags.Instance);
                                    if (stringProp != null)
                                    {
                                        var strVal = stringProp.GetValue(mProperty);
                                        if (strVal != null && !string.IsNullOrEmpty(strVal.ToString()))
                                        {
                                            targetGOName = strVal.ToString();
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // Try to get the active value from m_Active field
                    string activeValue = "(unknown)";
                    var activeField = instType.GetField("m_Active", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (activeField != null)
                    {
                        var active = activeField.GetValue(instruction);
                        if (active != null)
                        {
                            activeValue = active.ToString();
                        }
                    }

                    // Build the path description
                    string pathDescription = BuildNodePathDescription(nodePath, content, getNodeMethod);

                    // Log the finding
                    LogToDialogueFile($"[SETACTIVE FOUND] Dialogue GO: '{dialogueGOName}'");
                    LogToDialogueFile($"  Target GO: {targetGOName}");
                    LogToDialogueFile($"  Active Value: {activeValue}");
                    LogToDialogueFile($"  Location: {listName}, Instruction #{instIdx}");
                    LogToDialogueFile($"  Current Node Text: \"{currentNodeText}\"");
                    LogToDialogueFile($"  Node Path: {pathDescription}");
                    LogToDialogueFile("  ---");
                }

                instIdx++;
            }
        }

        /// <summary>
        /// Builds a human-readable description of the node path
        /// </summary>
        private static string BuildNodePathDescription(List<int> nodePath, object content, MethodInfo getNodeMethod)
        {
            if (nodePath == null || nodePath.Count == 0)
                return "(empty path)";

            var pathParts = new List<string>();

            for (int i = 0; i < nodePath.Count; i++)
            {
                int nodeId = nodePath[i];
                var node = getNodeMethod.Invoke(content, new object[] { nodeId });

                string nodeDesc = $"[{nodeId}]";

                if (node != null)
                {
                    var nodeType = node.GetType();

                    // Get node text
                    var textField = nodeType.GetField("m_Text", BindingFlags.NonPublic | BindingFlags.Instance);
                    string text = textField?.GetValue(node)?.ToString() ?? "";
                    if (text.Length > 30)
                        text = text.Substring(0, 30) + "...";

                    // Get node type (Text, Choice, Random, etc.)
                    var nodeTypeField = nodeType.GetField("m_NodeType", BindingFlags.NonPublic | BindingFlags.Instance);
                    string nodeTypeName = "";
                    if (nodeTypeField != null)
                    {
                        var nodeTypeObj = nodeTypeField.GetValue(node);
                        if (nodeTypeObj != null)
                        {
                            nodeTypeName = nodeTypeObj.GetType().Name.Replace("NodeType", "");
                        }
                    }

                    // Get actor name if available
                    string actorName = "";
                    var actingField = nodeType.GetField("m_Acting", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (actingField != null)
                    {
                        var acting = actingField.GetValue(node);
                        if (acting != null)
                        {
                            var actorField = acting.GetType().GetField("m_Actor", BindingFlags.NonPublic | BindingFlags.Instance);
                            var actor = actorField?.GetValue(acting);
                            if (actor != null)
                            {
                                var nameProp = actor.GetType().GetProperty("name", BindingFlags.Public | BindingFlags.Instance);
                                actorName = nameProp?.GetValue(actor) as string ?? "";
                            }
                        }
                    }

                    // Build description
                    if (!string.IsNullOrEmpty(actorName))
                        nodeDesc = $"[{nodeId}:{nodeTypeName}:{actorName}:\"{text}\"]";
                    else if (!string.IsNullOrEmpty(text))
                        nodeDesc = $"[{nodeId}:{nodeTypeName}:\"{text}\"]";
                    else
                        nodeDesc = $"[{nodeId}:{nodeTypeName}]";
                }

                pathParts.Add(nodeDesc);
            }

            return string.Join(" -> ", pathParts);
        }


    }
}
#endregion
#endregion