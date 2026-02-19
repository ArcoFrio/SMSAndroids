using UnityEngine;

namespace SMSAndroidsCore
{
    /// <summary>
    /// Extension methods for Transform to add utility functionality.
    /// </summary>
    public static class TransformExtensions
    {
        /// <summary>
        /// Finds a child transform by name, including inactive objects.
        /// Searches recursively through the transform's hierarchy.
        /// </summary>
        /// <param name="parent">The parent transform to search within</param>
        /// <param name="name">The name of the transform to find</param>
        /// <returns>The found transform, or null if not found</returns>
        public static Transform FindInActiveObjectByName(this Transform parent, string name)
        {
            if (parent == null) return null;
            
            // Search recursively through all children, including inactive ones
            Transform[] allChildren = parent.GetComponentsInChildren<Transform>(includeInactive: true);
            foreach (var child in allChildren)
            {
                if (child.name == name)
                {
                    return child;
                }
            }
            return null;
        }
    }
}
