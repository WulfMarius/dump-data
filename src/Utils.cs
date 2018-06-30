using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using UnityEngine;

namespace DumpData
{
    public class DumpUtils
    {
        public static string FormatChildren(string name, GameObject gameObject, int indent = 0)
        {
            return FormatChildren(name, gameObject, indent, new List<object>());
        }

        public static string FormatComponent(string name, Component component)
        {
            return FormatComponent(name, component, 0, new List<object>());
        }

        public static string FormatGameObject(string name, GameObject gameObject, int indent = 0)
        {
            return FormatValue(name, gameObject, indent, new List<object>());
        }

        public static string FormatHierarchy(GameObject gameObject)
        {
            StringBuilder stringBuilder = new StringBuilder();

            int indent = 0;
            Transform current = gameObject == null ? null : gameObject.transform;
            while (current != null)
            {
                stringBuilder.Append(GetIndentation(indent));
                stringBuilder.Append(current.name);
                stringBuilder.Append(" (");
                stringBuilder.Append(current.GetInstanceID());
                stringBuilder.Append(") active: ");
                stringBuilder.Append(current.gameObject.activeSelf);
                stringBuilder.AppendLine();

                current = current.transform.parent;
                indent++;
            }

            return stringBuilder.ToString();
        }

        public static string FormatPath(GameObject gameObject)
        {
            StringBuilder stringBuilder = new StringBuilder();

            Transform current = gameObject == null ? null : gameObject.transform;
            while (current != null)
            {
                stringBuilder.Insert(0, current.name);
                stringBuilder.Insert(0, "/");

                current = current.transform.parent;
            }

            return stringBuilder.ToString();
        }

        internal static string FormatEnumerable(string name, IEnumerable enumerable, int indent = 0, List<object> visitedObjects = null)
        {
            StringWriter writer = new StringWriter();

            writer.Write(GetIndentation(indent));
            WriteName(name, writer);

            writer.Write("[");

            IEnumerator enumerator = enumerable.GetEnumerator();
            if (enumerator.MoveNext())
            {
                while (true)
                {
                    writer.WriteLine();
                    var formattedValue = FormatValue(null, enumerator.Current, indent + 1, visitedObjects).TrimEnd();

                    if (formattedValue.Length == 0)
                    {
                        writer.Write(GetIndentation(indent + 1));
                    }
                    else
                    {
                        writer.Write(formattedValue);
                    }

                    if (enumerator.MoveNext())
                    {
                        writer.Write(",");
                    }
                    else
                    {
                        break;
                    }
                }
            }

            writer.WriteLine();
            writer.Write(GetIndentation(indent));
            writer.Write("]");

            return writer.ToString();
        }

        internal static string FormatLocalizedString(string name, LocalizedString localizedString, int indent = 0)
        {
            StringWriter writer = new StringWriter();

            writer.Write(GetIndentation(indent));
            WriteName(name, writer);
            writer.Write("LocalizedString (");
            writer.Write(localizedString.m_LocalizationID);
            writer.Write(")");

            return writer.ToString();
        }

        internal static string FormatLODGroup(string name, LODGroup lodGroup, int indent, List<object> visitedObjects = null)
        {
            StringWriter writer = new StringWriter();

            writer.Write(GetIndentation(indent));
            WriteName(name, writer);
            writer.WriteLine(lodGroup.ToString());

            writer.WriteLine(DumpUtils.FormatValue("animateCrossFading", lodGroup.animateCrossFading, indent + 1, visitedObjects));
            writer.WriteLine(DumpUtils.FormatValue("fadeMode", lodGroup.fadeMode, indent + 1, visitedObjects));
            writer.WriteLine(DumpUtils.FormatValue("localReferencePoint", lodGroup.localReferencePoint, indent + 1, visitedObjects));
            writer.WriteLine(DumpUtils.FormatValue("LODs", lodGroup.GetLODs(), indent + 1, visitedObjects));

            return writer.ToString();
        }

        internal static string FormatMaterial(string name, Material material, int indent)
        {
            StringWriter writer = new StringWriter();

            writer.Write(GetIndentation(indent));
            WriteName(name, writer);
            writer.WriteLine(material.name);

            if (material.HasProperty("_Color"))
            {
                writer.WriteLine(FormatValue("color", material.color, indent + 1));
            }

            writer.WriteLine(FormatValue("mainTexture", material.mainTexture, indent + 1));
            writer.WriteLine(FormatValue("renderQueue", material.renderQueue, indent + 1));
            writer.WriteLine(FormatValue("shader", material.shader, indent + 1));
            writer.WriteLine(FormatValue("shaderKeywords", material.shaderKeywords, indent + 1));

            List<string> passes = new List<string>();
            for (int i = 0; i < material.passCount; i++)
            {
                passes.Add(material.GetPassName(i));
            }

            writer.WriteLine(FormatValue("passes", passes, indent + 1));

            return writer.ToString();
        }

        internal static string FormatPlayerStateTransitions(string name, PlayerStateTransitions playerStateTransitions, int indent = 0)
        {
            StringWriter writer = new StringWriter();

            writer.Write(GetIndentation(indent));
            WriteName(name, writer);
            writer.WriteLine();

            FieldInfo fieldInfo = playerStateTransitions.GetType().GetField("m_ValidTransitions", BindingFlags.NonPublic | BindingFlags.Instance);
            object value = fieldInfo.GetValue(playerStateTransitions);
            writer.WriteLine(DumpUtils.FormatValue(fieldInfo.Name, value, indent + 1));

            fieldInfo = playerStateTransitions.GetType().GetField("m_InvalidTransitions", BindingFlags.NonPublic | BindingFlags.Instance);
            value = fieldInfo.GetValue(playerStateTransitions);
            writer.WriteLine(DumpUtils.FormatValue(fieldInfo.Name, value, indent + 1));

            return writer.ToString();
        }

        internal static string FormatQuaternion(string name, Quaternion quaternion, int indent)
        {
            StringWriter writer = new StringWriter();

            writer.Write(GetIndentation(indent));
            WriteName(name, writer);
            writer.Write(quaternion.ToString() + " / " + quaternion.eulerAngles);

            return writer.ToString();
        }

        internal static string FormatRenderer(string name, Renderer renderer, int indent, List<object> visitedObjects = null)
        {
            StringWriter writer = new StringWriter();

            writer.Write(GetIndentation(indent));
            WriteName(name, writer);
            writer.WriteLine(renderer.ToString());

            if (renderer != null)
            {
                writer.WriteLine(DumpUtils.FormatValue("isPartOfStaticBatch", renderer.isPartOfStaticBatch, indent + 1, visitedObjects));
                writer.WriteLine(DumpUtils.FormatValue("bounds", renderer.bounds.ToString("F3"), indent + 1, visitedObjects));
                writer.WriteLine(DumpUtils.FormatValue("enabled", renderer.enabled, indent + 1, visitedObjects));
                writer.WriteLine(DumpUtils.FormatValue("isVisible", renderer.isVisible, indent + 1, visitedObjects));
                writer.WriteLine(DumpUtils.FormatValue("materials", renderer.materials, indent + 1, visitedObjects));
            }

            return writer.ToString();
        }

        internal static string FormatSerializable(string name, object serializable, int indent = 0, List<object> visitedObjects = null)
        {
            if (visitedObjects != null)
            {
                if (visitedObjects.Contains(serializable))
                {
                    return FormatString(name, serializable.ToString() + " [RECURSION]", indent);
                }

                visitedObjects.Add(serializable);
            }

            StringWriter writer = new StringWriter();

            writer.Write(GetIndentation(indent));
            WriteName(name, writer);
            writer.WriteLine(serializable.ToString());

            FieldInfo[] fieldInfos = serializable.GetType().GetFields();
            foreach (FieldInfo eachFieldInfo in fieldInfos)
            {
                if (skip(serializable, eachFieldInfo.Name))
                {
                    continue;
                }

                object value = eachFieldInfo.GetValue(serializable);
                writer.WriteLine(DumpUtils.FormatValue(eachFieldInfo.Name, value, indent + 1, visitedObjects));
            }

            PropertyInfo[] propertyInfos = serializable.GetType().GetProperties();
            foreach (PropertyInfo eachPropertyInfo in propertyInfos)
            {
                if (eachPropertyInfo.GetCustomAttributes(false).OfType<ObsoleteAttribute>().Count() > 0)
                {
                    continue;
                }

                if (skip(serializable, eachPropertyInfo.Name))
                {
                    continue;
                }

                if (eachPropertyInfo.CanRead)
                {
                    MethodInfo methodInfo = eachPropertyInfo.GetGetMethod();
                    if (methodInfo.GetParameters().Length > 0)
                    {
                        Debug.Log(methodInfo);
                        continue;
                    }

                    object value = GetMethodResult(serializable, methodInfo);
                    writer.WriteLine(DumpUtils.FormatValue(eachPropertyInfo.Name, value, indent + 1, visitedObjects));
                }
            }

            return writer.ToString();
        }

        internal static string FormatShader(string name, Shader shader, int indent)
        {
            StringWriter writer = new StringWriter();

            writer.Write(GetIndentation(indent));
            WriteName(name, writer);
            writer.WriteLine();

            writer.WriteLine(FormatValue("name", shader.name, indent + 1));
            writer.WriteLine(FormatValue("renderQueue", shader.renderQueue, indent + 1));

            return writer.ToString();
        }

        internal static string FormatString(string name, string value, int indent = 0)
        {
            StringWriter writer = new StringWriter();

            writer.Write(GetIndentation(indent));
            WriteName(name, writer);

            if (value == null)
            {
                writer.Write("NULL");
            }
            else
            {
                writer.Write(value.Replace("\n", "\\n"));
            }

            return writer.ToString();
        }

        internal static string FormatTextAsset(string name, TextAsset textAsset, int indent)
        {
            StringWriter writer = new StringWriter();

            writer.Write(GetIndentation(indent));
            WriteName(name, writer);
            writer.WriteLine();

            writer.WriteLine(FormatValue("name", textAsset.name, indent + 1));
            writer.WriteLine(FormatValue("text", textAsset.text.Replace("\r", "\\r").Replace("\n", "\\n"), indent + 1));

            return writer.ToString();
        }

        internal static string FormatTransform(string name, Transform transform, int indent, List<System.Object> visitedObjects = null)
        {
            StringWriter writer = new StringWriter();

            writer.Write(GetIndentation(indent));
            WriteName(name, writer);
            writer.WriteLine(transform.ToString());

            writer.WriteLine(DumpUtils.FormatValue("instanceID", transform.gameObject.GetInstanceID(), indent + 1, visitedObjects));
            writer.WriteLine(DumpUtils.FormatValue("localPosition", transform.localPosition, indent + 1, visitedObjects));
            writer.WriteLine(DumpUtils.FormatValue("localRotation", transform.localRotation, indent + 1, visitedObjects));
            writer.WriteLine(DumpUtils.FormatValue("localScale", transform.localScale, indent + 1, visitedObjects));
            writer.WriteLine(DumpUtils.FormatValue("position", transform.position, indent + 1, visitedObjects));
            writer.WriteLine(DumpUtils.FormatValue("rotation", transform.rotation, indent + 1, visitedObjects));
            writer.WriteLine(DumpUtils.FormatValue("lossyScale", transform.lossyScale, indent + 1, visitedObjects));
            writer.WriteLine(DumpUtils.FormatValue("parent", transform.parent, indent + 1, visitedObjects));

            return writer.ToString();
        }

        internal static string FormatValue(string name, object value, int indent = 0, List<object> visitedObjects = null)
        {
            if (visitedObjects == null)
            {
                visitedObjects = new List<object>();
            }

            if (value == null)
            {
                return FormatString(name, "NULL", indent);
            }

            if (value is Matrix4x4)
            {
                return FormatString(name, "MATRIX", indent);
            }

            if (value is Vector3)
            {
                return FormatString(name, ((Vector3)value).ToString("F3"), indent);
            }

            if (value is string)
            {
                return FormatString(name, (string)value, indent);
            }

            if (value is Quaternion)
            {
                return FormatQuaternion(name, (Quaternion)value, indent);
            }

            if (value is LocalizedString)
            {
                return FormatLocalizedString(name, (LocalizedString)value, indent);
            }

            if (value is Component)
            {
                return FormatComponent(name, (Component)value, indent, visitedObjects);
            }

            if (value is GameObject)
            {
                return FormatGameObject(name, (GameObject)value, indent, visitedObjects);
            }

            if (value is PlayerStateTransitions)
            {
                return FormatPlayerStateTransitions(name, (PlayerStateTransitions)value, indent);
            }

            if (value is Material)
            {
                return FormatMaterial(name, value as Material, indent);
            }

            if (value is Shader)
            {
                return FormatShader(name, value as Shader, indent);
            }

            if (value is UnityEngine.SceneManagement.Scene)
            {
                return FormatString(name, ((UnityEngine.SceneManagement.Scene)value).name, indent);
            }

            if (value is IEnumerable)
            {
                return FormatEnumerable(name, (IEnumerable)value, indent, visitedObjects);
            }

            if (value is TextAsset)
            {
                return FormatTextAsset(name, value as TextAsset, indent);
            }

            if (value.GetType().IsSerializable && !value.GetType().IsPrimitive && !value.GetType().IsEnum)
            {
                return FormatSerializable(name, value, indent, visitedObjects);
            }

            return FormatString(name, value.ToString(), indent);
        }

        private static string FormatChildren(string name, GameObject gameObject, int indent = 0, List<object> visitedObjects = null)
        {
            if (visitedObjects != null)
            {
                if (visitedObjects.Contains(gameObject))
                {
                    return FormatString(name, gameObject.ToString() + " [RECURSION]", indent);
                }

                visitedObjects.Add(gameObject);
            }

            StringWriter writer = new StringWriter();

            writer.WriteLine(FormatValue(name, gameObject.name, indent));

            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                GameObject child = gameObject.transform.GetChild(i).gameObject;
                writer.Write(FormatChildren(child.name, child, indent + 1, visitedObjects));
            }

            return writer.ToString();
        }

        private static string FormatComponent(string name, Component component, int indent = 0, List<object> visitedObjects = null)
        {
            if (component is Transform)
            {
                return FormatTransform(name, component as Transform, indent, visitedObjects);
            }

            if (component is Renderer)
            {
                return FormatRenderer(name, component as Renderer, indent, visitedObjects);
            }

            if (component is LODGroup)
            {
                return FormatLODGroup(name, component as LODGroup, indent, visitedObjects);
            }

            if (visitedObjects != null)
            {
                if (visitedObjects.Contains(component))
                {
                    return FormatString(name, component.ToString() + " [RECURSION]", indent);
                }

                visitedObjects.Add(component);
            }

            StringWriter writer = new StringWriter();

            writer.Write(GetIndentation(indent));
            WriteName(name, writer);
            writer.WriteLine(component.ToString());

            FieldInfo[] fieldInfos = component.GetType().GetFields();
            foreach (FieldInfo eachFieldInfo in fieldInfos)
            {
                if (skip(component, eachFieldInfo.Name))
                {
                    continue;
                }

                object value = eachFieldInfo.GetValue(component);
                writer.WriteLine(DumpUtils.FormatValue(eachFieldInfo.Name, value, indent + 1, visitedObjects));
            }

            PropertyInfo[] propertyInfos = component.GetType().GetProperties();
            foreach (PropertyInfo eachPropertyInfo in propertyInfos)
            {
                if (eachPropertyInfo.GetCustomAttributes(false).OfType<ObsoleteAttribute>().Count() > 0)
                {
                    continue;
                }

                if (skip(component, eachPropertyInfo.Name))
                {
                    continue;
                }

                if (eachPropertyInfo.CanRead)
                {
                    MethodInfo methodInfo = eachPropertyInfo.GetGetMethod();
                    if (methodInfo.GetParameters().Length > 0)
                    {
                        Debug.Log(methodInfo);
                        continue;
                    }

                    object value = GetMethodResult(component, methodInfo);
                    writer.WriteLine(DumpUtils.FormatValue(eachPropertyInfo.Name, value, indent + 1, visitedObjects));
                }
            }

            return writer.ToString();
        }

        private static string FormatGameObject(string name, GameObject gameObject, int indent = 0, List<object> visitedObjects = null)
        {
            if (gameObject == null)
            {
                return "NULL";
            }

            if (visitedObjects != null)
            {
                if (visitedObjects.Contains(gameObject))
                {
                    return FormatString(name, gameObject.ToString() + " [RECURSION]", indent);
                }

                visitedObjects.Add(gameObject);
            }

            StringWriter writer = new StringWriter();

            writer.WriteLine(FormatValue(name, gameObject.name, indent));

            writer.WriteLine(FormatValue("activeSelf", gameObject.activeSelf, indent + 1));
            writer.WriteLine(FormatValue("activeInHierarchy", gameObject.activeInHierarchy, indent + 1));
            writer.WriteLine(FormatValue("instanceID", gameObject.GetInstanceID(), indent + 1));
            writer.WriteLine(FormatValue("isStatic", gameObject.isStatic, indent + 1));
            writer.WriteLine(FormatValue("layer", gameObject.layer, indent + 1));
            writer.WriteLine(FormatValue("scene", gameObject.scene, indent + 1));
            writer.WriteLine(FormatValue("tag", gameObject.tag, indent + 1));

            Component[] components = gameObject.GetComponents<Component>();
            foreach (Component eachComponent in components)
            {
                writer.WriteLine(FormatComponent(eachComponent.name, eachComponent, indent + 1, visitedObjects));
            }

            return writer.ToString();
        }

        private static string GetIndentation(int indent)
        {
            StringWriter writer = new StringWriter();

            for (int i = 0; i < indent; i++)
            {
                writer.Write("  ");
            }

            return writer.ToString();
        }

        private static object GetMethodResult(object @object, MethodInfo methodInfo)
        {
            try
            {
                return methodInfo.Invoke(@object, new object[0]);
            }
            catch (Exception e)
            {
                return "Failed to invoke method: " + e.Message;
            }
        }

        private static bool skip(object value, string fieldName)
        {
            if (value is AkGameObj && fieldName == "ListenerList")
            {
                return true;
            }

            if (value is UIWidget && fieldName == "panel")
            {
                return true;
            }

            if (value is UICamera)
            {
                return "currentCamera" == fieldName || "current" == fieldName;
            }

            if (value is Camera)
            {
                return "allCameras" == fieldName || "current" == fieldName || "main" == fieldName;
            }

            if (value is UIRoot)
            {
                return "list" == fieldName || "root" == fieldName;
            }

            if (value is UIPanel)
            {
                return "list" == fieldName || "root" == fieldName || "widgets" == fieldName;
            }

            if (value is UISprite)
            {
                return "anchorCamera" == fieldName || "parent" == fieldName || "root" == fieldName;
            }

            if (value is UIButton)
            {
                return "current" == fieldName || "onClick" == fieldName || "parent" == fieldName || "root" == fieldName;
            }

            if (value is UILabel)
            {
                return "anchorCamera" == fieldName || "parent" == fieldName || "root" == fieldName;
            }

            if (value is BreakDown)
            {
                return "m_BreakDownObjects" == fieldName;
            }

            return false;
        }

        private static void WriteName(string name, StringWriter writer)
        {
            if (name != null)
            {
                writer.Write(name);
                writer.Write(": ");
            }
        }
    }
}