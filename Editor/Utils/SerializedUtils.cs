﻿using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Utils
{
    public static class SerializedUtils
    {
        public static SerializedProperty FindPropertyByAutoPropertyName(SerializedObject obj, string propName)
        {
            return obj.FindProperty($"<{propName}>k__BackingField");
        }

        // public static SerializedProperty FindPropertyRelativeByAutoPropertyName(SerializedProperty prop, string propName)
        // {
        //     return prop.FindPropertyRelative($"<{propName}>k__BackingField");
        // }

        // public static T GetAttribute<T>(SerializedProperty property) where T : class
        // {
        //     T[] attributes = GetAttributes<T>(property);
        //     return attributes.Length > 0 ? attributes[0] : null;
        // }

        private struct FileOrProp
        {
            public bool IsFile;
            public FieldInfo FileInfo;
            public PropertyInfo PropertyInfo;
        }

        public static (T[] attributes, object parent) GetAttributesAndDirectParent<T>(SerializedProperty property) where T : class
        {

            string originPath = property.propertyPath;
            string[] propPaths = originPath.Split('.');
            int usePathLength = propPaths.Length;
            if(propPaths.Length > 2)
            {
                string lastPart = propPaths[propPaths.Length - 1];
                string secLastPart = propPaths[propPaths.Length - 2];
                bool isArray = secLastPart == "Array" && lastPart.StartsWith("data[") && lastPart.EndsWith("]");
                if (isArray)
                {
                    // Debug.Log($"use sub length {originPath}");
                    usePathLength -= 2;
                }
                // else
                // {
                //     Debug.Log($"use normal length {originPath}");
                // }
            }
            // else
            // {
            //     Debug.Log($"use normal length {originPath}");
            // }

            // SerializedObject serObj = property.serializedObject;
            // SerializedProperty targetProp = null;
            //
            // IReadOnlyList<Type> types = ReflectUtils.GetSelfAndBaseTypes(targetObj);
            // foreach (Type eachType in types)
            // {
            //     IEnumerable<FieldInfo> fieldInfos = eachType
            //         .GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly);
            //
            //     foreach (FieldInfo fieldInfo in fieldInfos)
            //     {
            //         Debug.Log(fieldInfo.Name);
            //     }
            // }
            //
            // return null;

            object sourceObj = property.serializedObject.targetObject;
            FileOrProp fileOrProp = default;

            bool preNameIsArray = false;
            foreach (int propIndex in Enumerable.Range(0, usePathLength))
            {
                string propSegName = propPaths[propIndex];
                // Debug.Log($"check key {propSegName}");
                if(propSegName == "Array")
                {
                    preNameIsArray = true;
                    continue;
                }
                if (propSegName.StartsWith("data[") && propSegName.EndsWith("]"))
                {
                    Debug.Assert(preNameIsArray);
                    // Debug.Log(propSegName);
                    // Debug.Assert(targetProp != null);
                    preNameIsArray = false;

                    int elemIndex = Convert.ToInt32(propSegName.Substring(5, propSegName.Length - 6));

                    object useObject;

                    if(fileOrProp.FileInfo is null && fileOrProp.PropertyInfo is null)
                    {
                        useObject = sourceObj;
                    }
                    else
                    {
                        useObject = fileOrProp.IsFile
                            // ReSharper disable once PossibleNullReferenceException
                            ? fileOrProp.FileInfo.GetValue(sourceObj)
                            : fileOrProp.PropertyInfo.GetValue(sourceObj);
                    }

                    // Debug.Log($"Get index from obj {useObject}[{elemIndex}]");
                    sourceObj = GetValueAtIndex(useObject, elemIndex);
                    // Debug.Log($"Get index from obj [{useObject}] returns {sourceObj}");
                    fileOrProp = default;
                    // Debug.Log($"[index={elemIndex}]={targetObj}");
                    continue;
                }

                preNameIsArray = false;

                // if (propSegName.StartsWith("<") && propSegName.EndsWith(">k__BackingField"))
                // {
                //     propSegName = propSegName.Substring(1, propSegName.Length - 17);
                // }

                // Debug.Log($"get obj {sourceObj}.{propSegName}");
                if(fileOrProp.FileInfo is null && fileOrProp.PropertyInfo is null)
                {
                    fileOrProp = GetFileOrProp(sourceObj, propSegName);
                }
                else
                {
                    sourceObj = fileOrProp.IsFile
                        // ReSharper disable once PossibleNullReferenceException
                        ? fileOrProp.FileInfo.GetValue(sourceObj)
                        : fileOrProp.PropertyInfo.GetValue(sourceObj);
                    fileOrProp = GetFileOrProp(sourceObj, propSegName);
                }
                // targetFieldName = propSegName;
                // Debug.Log($"[{propSegName}]={targetObj}");
            }


            // if (!fileOrProp.isFile)
            // {
            //     Debug.Log($"check prop {fileOrProp.PropertyInfo.Name}/{fileOrProp.PropertyInfo.GetCustomAttributes().Count()}");
            //     foreach (CustomAttributeData customAttributeData in fileOrProp.PropertyInfo.CustomAttributes)
            //     {
            //         Debug.Log($"check attr {customAttributeData.AttributeType}");
            //         // if (customAttributeData.AttributeType == typeof(T))
            //         // {
            //         //     Debug.Log($"return attr {customAttributeData.AttributeType}");
            //         //     yield return (T)fileOrProp.PropertyInfo.GetValue(sourceObj);
            //         // }
            //     }
            // }

            // Debug.Log($"return result for {property.propertyPath}: {fileOrProp.FileInfo?.Name ?? fileOrProp.PropertyInfo.Name}");
            T[] attributes = fileOrProp.IsFile
                ? fileOrProp.FileInfo.GetCustomAttributes(typeof(T), true).Cast<T>().ToArray()
                : fileOrProp.PropertyInfo.GetCustomAttributes(typeof(T), true).Cast<T>().ToArray();
            return (attributes, sourceObj);
        }

        private static FileOrProp GetFileOrProp(object source, string name)
        {
            Type type = source.GetType();
            // Debug.Log($"get type {type}");

            while (type != null)
            {
                FieldInfo field = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (field != null)
                {
                    // Debug.Log($"return field {field.Name}");
                    return new FileOrProp()
                    {
                        IsFile = true,
                        PropertyInfo = null,
                        FileInfo = field,
                    };
                }

                PropertyInfo property = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (property != null)
                {
                    // return property.GetValue(source, null);
                    // Debug.Log($"return prop {property.Name}");
                    return new FileOrProp
                    {
                        IsFile = false,
                        PropertyInfo = property,
                        FileInfo = null,
                    };
                }

                type = type.BaseType;
            }

            throw new Exception($"Unable to get type from {source}");
        }

        private static object GetValueAtIndex(object source, int index)
        {
            if (!(source is IEnumerable enumerable))
            {
                throw new Exception($"Not a enumerable {source}");
            }

            int searchIndex = 0;
            // Debug.Log($"start check index in {source}");
            foreach (object result in enumerable)
            {
                // Debug.Log($"check index {searchIndex} in {source}");
                if(searchIndex == index)
                {
                    return result;
                }
                searchIndex++;
            }

            throw new Exception($"Not found index {index} in {source}");

            // IEnumerator enumerator = enumerable.GetEnumerator();
            // for (int i = 0; i <= index; i++)
            // {
            //     if (!enumerator.MoveNext())
            //     {
            //         return null;
            //     }
            // }
            //
            // return enumerator.Current;
        }

        public static Type GetType(SerializedProperty prop)
        {
            //gets parent type info
            string[] slices = prop.propertyPath.Split('.');
            object targetObj = prop.serializedObject.targetObject;

            foreach (Type eachType in ReflectUtils.GetSelfAndBaseTypes(targetObj))
            {
                // foreach (FieldInfo field in type!.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
                // {
                //     Debug.Log($"name={field.Name}");
                // }
                Type getType = eachType;

                for(int i = 0; i < slices.Length; i++)
                {
                    if (slices[i] == "Array")
                    {
                        i++; //skips "data[x]"
                        // type = type!.GetElementType(); //gets info on array elements
                        Debug.Assert(getType != null);
                        getType = getType.GetElementType();
                    }
                    else  //gets info on field and its type
                    {
                        // Debug.Log($"{slices[i]}, {type!.GetField(slices[i], BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Instance)}");
                        Debug.Assert(getType != null);
                        FieldInfo field = getType.GetField(slices[i],
                            BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy |
                            BindingFlags.Instance);
                        if (field != null)
                        {
                            return field.FieldType;
                        }
                        // getType =
                        //     !.FieldType;
                    }
                }

                //type is now the type of the property
                // return type;
            }

            throw new Exception($"Unable to get type from {targetObj}");

            // Type type = prop.serializedObject.targetObject.GetType()!;
            // Debug.Log($"{prop.propertyPath}, {type}");
            // foreach (FieldInfo field in type!.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            // {
            //     Debug.Log($"name={field.Name}");
            // }
            //
            // for(int i = 0; i < slices.Length; i++)
            // {
            //     if (slices[i] == "Array")
            //     {
            //         i++; //skips "data[x]"
            //         type = type!.GetElementType(); //gets info on array elements
            //     }
            //     else  //gets info on field and its type
            //     {
            //         Debug.Log($"{slices[i]}, {type!.GetField(slices[i], BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Instance)}");
            //         type = type
            //             !.GetField(slices[i], BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Instance)
            //             !.FieldType;
            //     }
            // }
            //
            // //type is now the type of the property
            // return type;
        }

        // public static object GetValue(SerializedProperty property)
        // {
        //     Object targetObject = property.serializedObject.targetObject;
        //     Type targetObjectClassType = targetObject.GetType();
        //     FieldInfo field = targetObjectClassType.GetField(property.propertyPath);
        //     // if (field != null)
        //     // {
        //     //     var value = field.GetValue(targetObject);
        //     //     // Debug.Log(value.s);
        //     // }
        //     Debug.Assert(field != null, $"{property.propertyPath}/{targetObject}");
        //     return field!.GetValue(targetObject);
        // }

    }
}
