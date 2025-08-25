
// using System;
// using System.Linq;
// using UnityEditor;
// using UnityEngine;

// namespace TnT.Systems.UI
// {

//     // public class ChallengeUITypeSelectorAttribute : PropertyAttribute
//     // {
//     // }

//     // [CustomPropertyDrawer(typeof(ChallengeUITypeSelectorAttribute))]
//     public class ChallengeUITypeSelectorDrawer : PropertyDrawer
//     {
//         // Cached list of types and names for dropdown
//         private static Type[] _registeredTypes;
//         private static string[] _typeNames;
//         private static string[] _typeAssemblyQualifiedNames;

//         private void RefreshCache()
//         {
//             _registeredTypes = ChallengeUIFactory.GetRegisteredTypes().ToArray();
//             _typeNames = _registeredTypes.Select(t => t.Name).ToArray();
//             _typeAssemblyQualifiedNames = _registeredTypes.Select(t => t.AssemblyQualifiedName).ToArray();
//         }

//         public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//         {
//             // The 'property' here refers to the whole TypeReference struct, so get the assemblyQualifiedName field inside it
//             var aqnProp = property.FindPropertyRelative("assemblyQualifiedName");

//             if (_registeredTypes == null || _registeredTypes.Length == 0)
//                 RefreshCache();

//             int currentIndex = 0;
//             string currentAQN = aqnProp.stringValue;

//             if (!string.IsNullOrEmpty(currentAQN))
//             {
//                 currentIndex = Array.IndexOf(_typeAssemblyQualifiedNames, currentAQN);
//                 if (currentIndex < 0) currentIndex = 0;
//             }

//             EditorGUI.BeginProperty(position, label, property);

//             string[] shownNames = _typeNames
//                 .Select(n => n.Replace("UIStrategy", ""))
//                 .Select(n => ObjectNames.NicifyVariableName(n))
//                 .ToArray();

//             int selectedIndex = EditorGUI.Popup(position, label.text, currentIndex, shownNames);

//             if (selectedIndex != currentIndex)
//             {
//                 aqnProp.stringValue = _typeAssemblyQualifiedNames[selectedIndex];
//             }

//             EditorGUI.EndProperty();
//         }
//     }
// }