//using UnityEngine;
//using UnityEditor;

//// TODO : 1. 빌드 시 해당 파일 에러남

//[CustomPropertyDrawer(typeof(ArrayLayout))]
//public class CustPropertyDrawer : PropertyDrawer
//{
//    private int width = 7;
//    private int height = 7;

//    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//    {
//        EditorGUI.PrefixLabel(position, label);
//        Rect newposition = position;
//        newposition.y += 144f;
//        SerializedProperty data = property.FindPropertyRelative("rows");
//        //data.rows[0][]
//        if (data.arraySize != height)
//            data.arraySize = height;
//        for (int j = 0; j < height; j++)
//        {
//            SerializedProperty row = data.GetArrayElementAtIndex(j).FindPropertyRelative("row");
//            newposition.height = 18f;
//            if (row.arraySize != width)
//                row.arraySize = width;
//            newposition.width = position.width / width;
//            for (int i = 0; i < width; i++)
//            {
//                EditorGUI.PropertyField(newposition, row.GetArrayElementAtIndex(i), GUIContent.none);
//                newposition.x += newposition.width;
//            }

//            newposition.x = position.x;
//            newposition.y -= 18f;
//        }
//    }

//    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
//    {
//        return 18f * 10;
//    }
//}