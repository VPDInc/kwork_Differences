
using _differences.Scripts.Helpers;
using UnityEditor;
using UnityEngine;

namespace _differences.Scripts.Editor
{

    [CustomPropertyDrawer(typeof(EnumAsStringAttribute))]
    public class EnumAsStringPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            {
                position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

                int indent = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;
                {
                    string[] values = System.Enum.GetNames((attribute as EnumAsStringAttribute).enumType);
                    int selectedIndex = System.Array.IndexOf(values, property.stringValue);
                    if (selectedIndex < 0)
                    {
                        ArrayUtility.Insert(ref values, 0, "null");
                        selectedIndex = 0;
                        GUI.color = Color.red;
                    }
                    property.stringValue = values[EditorGUI.Popup(position, selectedIndex, values)];
                    GUI.color = Color.white;
                }
                EditorGUI.indentLevel = indent;
            }
            EditorGUI.EndProperty();
        }
    }
}
