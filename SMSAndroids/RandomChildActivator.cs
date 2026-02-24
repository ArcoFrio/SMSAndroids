using BepInEx.Logging;
using UnityEngine;

namespace SMSAndroidsCore
{
    /// <summary>
    /// Activates a random child GameObject when this GameObject is enabled,
    /// and deactivates all children when this GameObject is disabled.
    /// Remembers the selected child across hierarchy enable/disable cycles.
    /// </summary>
    public class RandomChildActivator : MonoBehaviour
    {
        private static ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("RandomChildActivator");
        
        [SerializeField]
        [Tooltip("If true, picks a new random child every time OnEnable is called. If false, remembers the last selected child.")]
        public bool pickNewOnEnable = false;
        
        private int selectedChildIndex = -1;

        private void OnEnable()
        {
            Logger.LogInfo($"OnEnable called for {gameObject.name}, child count: {transform.childCount}");
            
            // If we should pick a new child, or we haven't selected one yet
            if (pickNewOnEnable)
            {
                ActivateRandomChild();
            }
            else
            {
                // Reactivate the previously selected child
                ActivateStoredChild();
            }
        }

        private void OnDisable()
        {
            Logger.LogInfo($"OnDisable called for {gameObject.name}");
            if (!gameObject.activeSelf)
            {
                DeactivateAllChildren();
                pickNewOnEnable = true;
            } else
            {
                DeactivateAllChildren();
                pickNewOnEnable = false;
            }
        }

        /// <summary>
        /// Deactivates all children, then picks and activates one random child.
        /// </summary>
        private void ActivateRandomChild()
        {
            Transform transform = this.transform;
            int childCount = transform.childCount;

            if (childCount == 0)
            {
                Logger.LogWarning($"No children found on {gameObject.name}");
                selectedChildIndex = -1;
                return;
            }

            // First, deactivate all children
            DeactivateAllChildren();

            // Then activate a random child
            selectedChildIndex = Random.Range(0, childCount);
            Transform randomChild = transform.GetChild(selectedChildIndex);
            Logger.LogInfo($"Picked new random child {selectedChildIndex}: {randomChild.name}");
            randomChild.gameObject.SetActive(true);
            pickNewOnEnable = false;
        }

        /// <summary>
        /// Reactivates the previously selected child without picking a new one.
        /// </summary>
        private void ActivateStoredChild()
        {
            Transform transform = this.transform;
            int childCount = transform.childCount;

            if (selectedChildIndex < 0 || selectedChildIndex >= childCount)
            {
                Logger.LogWarning($"Stored child index {selectedChildIndex} is invalid, picking new random child");
                ActivateRandomChild();
                return;
            }

            DeactivateAllChildren();
            Transform storedChild = transform.GetChild(selectedChildIndex);
            Logger.LogInfo($"Reactivating stored child {selectedChildIndex}: {storedChild.name}");
            storedChild.gameObject.SetActive(true);
        }

        /// <summary>
        /// Deactivates all child GameObjects.
        /// </summary>
        private void DeactivateAllChildren()
        {
            Transform transform = this.transform;
            int childCount = transform.childCount;

            for (int i = 0; i < childCount; i++)
            {
                Transform child = transform.GetChild(i);
                child.gameObject.SetActive(false);
            }
            Logger.LogInfo($"Deactivated all {childCount} children of {gameObject.name}");
        }

        /// <summary>
        /// Forces a new random child to be selected on the next enable.
        /// Call this to reset the selection.
        /// </summary>
        public void ResetSelection()
        {
            Logger.LogInfo($"Selection reset for {gameObject.name}");
            selectedChildIndex = -1;
        }
    }
}
