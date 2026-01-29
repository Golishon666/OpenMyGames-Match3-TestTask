using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LevelData))]
public class LevelDataEditor : Editor
{
    private SerializedProperty _widthProp;
    private SerializedProperty _heightProp;
    private SerializedProperty _elementsProp;

    private void OnEnable()
    {
        _widthProp = serializedObject.FindProperty("width");
        _heightProp = serializedObject.FindProperty("height");
        _elementsProp = serializedObject.FindProperty("elements");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var oldWidth = _widthProp.intValue;
        var oldHeight = _heightProp.intValue;

        EditorGUILayout.PropertyField(_widthProp);
        EditorGUILayout.PropertyField(_heightProp);

        var width = _widthProp.intValue;
        var height = _heightProp.intValue;

        if (width < 0) width = 0;
        if (height < 0) height = 0;
        
        if (width != oldWidth || height != oldHeight || _elementsProp.arraySize != width * height)
        {
            _widthProp.intValue = width;
            _heightProp.intValue = height;
            ResizeArray(oldWidth, oldHeight, width, height);
        }

        if (width > 0 && height > 0)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Level Grid", EditorStyles.boldLabel);

            for (var y = height - 1; y >= 0; y--)
            {
                EditorGUILayout.BeginHorizontal();
                for (var x = 0; x < width; x++)
                {
                    var index = y * width + x;
                    if (index < _elementsProp.arraySize)
                    {
                        var element = _elementsProp.GetArrayElementAtIndex(index);
                        EditorGUILayout.PropertyField(element, GUIContent.none, GUILayout.Width(60));
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void ResizeArray(int oldWidth, int oldHeight, int newWidth, int newHeight)
    {
        var oldSize = _elementsProp.arraySize;
        var oldElements = new ElementType[oldSize];
        for (var i = 0; i < oldSize; i++)
        {
            oldElements[i] = (ElementType)_elementsProp.GetArrayElementAtIndex(i).enumValueIndex;
        }

        _elementsProp.arraySize = newWidth * newHeight;
        
        for (var y = 0; y < newHeight; y++)
        {
            for (var x = 0; x < newWidth; x++)
            {
                var newIndex = y * newWidth + x;
                if (x < oldWidth && y < oldHeight)
                {
                    var oldIndex = y * oldWidth + x;
                    if (oldIndex < oldElements.Length)
                    {
                        _elementsProp.GetArrayElementAtIndex(newIndex).enumValueIndex = (int)oldElements[oldIndex];
                    }
                }
                else
                {
                    _elementsProp.GetArrayElementAtIndex(newIndex).enumValueIndex = (int)ElementType.None;
                }
            }
        }
    }
}
