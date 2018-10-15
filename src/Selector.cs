using Harmony;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

using UnityEngine.SceneManagement;

namespace DumpData
{
    public class Selector
    {
        private static Stack<object> selected = new Stack<object>();

        private static float stopwatchStartGameTime;
        private static float stopwatchStopGameTime;

        private static float stopwatchStartRealTime;
        private static float stopwatchStopRealTime;

        public static void OnLoad()
        {
            uConsole.RegisterCommand("cd", new uConsole.DebugCommand(Select));
            uConsole.RegisterCommand("select", new uConsole.DebugCommand(Select));
            uConsole.RegisterCommand("back", new uConsole.DebugCommand(Back));
            uConsole.RegisterCommand("get", new uConsole.DebugCommand(Get));
            uConsole.RegisterCommand("set", new uConsole.DebugCommand(Set));
            uConsole.RegisterCommand("show", new uConsole.DebugCommand(Show));

            uConsole.RegisterCommand("culling-mask", new uConsole.DebugCommand(CullingMask));
            uConsole.RegisterCommand("type", new uConsole.DebugCommand(PrintType));
            uConsole.RegisterCommand("hierarchy", new uConsole.DebugCommand(PrintHierarchy));
            uConsole.RegisterCommand("path", new uConsole.DebugCommand(PrintPath));
            uConsole.RegisterCommand("transform", new uConsole.DebugCommand(PrintTransform));
            uConsole.RegisterCommand("children", new uConsole.DebugCommand(PrintChildren));
            uConsole.RegisterCommand("components", new uConsole.DebugCommand(PrintComponents));
            uConsole.RegisterCommand("details", new uConsole.DebugCommand(PrintDetails));
            uConsole.RegisterCommand("invoke", new uConsole.DebugCommand(Invoke));
            uConsole.RegisterCommand("toggle-active", new uConsole.DebugCommand(ToggleActive));
            uConsole.RegisterCommand("toggle-active-children", new uConsole.DebugCommand(ToggleActiveChildren));
            uConsole.RegisterCommand("set-position", new uConsole.DebugCommand(SetPosition));
            uConsole.RegisterCommand("set-rotation", new uConsole.DebugCommand(SetRotation));
            uConsole.RegisterCommand("export-obj", new uConsole.DebugCommand(ExportObj));

            uConsole.RegisterCommand("begin-stopwatch", new uConsole.DebugCommand(StartStopwatch));
            uConsole.RegisterCommand("end-stopwatch", new uConsole.DebugCommand(StopStopwatch));
            uConsole.RegisterCommand("stopwatch", new uConsole.DebugCommand(ShowStopwatch));
        }

        private static void Back()
        {
            if (!HasSelection())
            {
                Debug.Log("Cannot go back, if the current selection is empty.");
                return;
            }

            if (selected.Count == 1)
            {
                Debug.Log("Cannot go back, no more previous entries.");
            }
            else
            {
                selected.Pop();
            }

            PrintSelected();
        }

        private static void CullingMask()
        {
            if (!HasSelection())
            {
                Debug.Log("  Current selection is empty.");
                return;
            }

            bool setValue = uConsole.GetNumParameters() == 1;

            Light light = CurrentSelection() as Light;
            if (light != null)
            {
                if (setValue)
                {
                    light.cullingMask = uConsole.GetInt();
                }

                Debug.Log("  " + light + ".cullingMask is " + light.cullingMask);
                return;
            }

            Debug.Log("Culling mask is not support for " + CurrentSelection().GetType());
        }

        private static GameObject CurrentGameObject()
        {
            var currentSelection = CurrentSelection();
            if (currentSelection is GameObject)
            {
                return (GameObject)currentSelection;
            }

            if (currentSelection is Component)
            {
                return ((Component)currentSelection).gameObject;
            }

            return null;
        }

        private static object CurrentSelection()
        {
            if (selected.Count == 0)
            {
                return null;
            }

            return selected.Peek();
        }

        private static void ExportObj()
        {
            if (!HasSelection())
            {
                Debug.Log("  Current selection is empty.");
                return;
            }

            GameObject gameObject = CurrentGameObject();
            if (gameObject == null)
            {
                Debug.Log("   Cannot export obj if the current selection is not a GameObject.");
                return;
            }

            ObjExporter.DoExport(gameObject, true);
        }

        private static string FormatDuration(float seconds)
        {
            string result = "";

            if (seconds > 3600)
            {
                result += (seconds / 3600).ToString("F0") + "h";
                seconds %= 3600;
            }

            if (seconds > 60)
            {
                result += (seconds / 60).ToString("F0") + "m";
                seconds %= 60;
            }

            if (seconds > 0)
            {
                result += seconds.ToString() + "s";
            }

            return result;
        }

        private static void Get()
        {
            if (!HasSelection())
            {
                Debug.Log("  Current selection is empty.");
                return;
            }

            int numParameters = uConsole.GetNumParameters();
            if (numParameters != 1)
            {
                Debug.Log("  Field required.");
                return;
            }

            object currentSelection = CurrentSelection();
            var name = uConsole.GetString();

            FieldInfo fieldInfo = AccessTools.Field(currentSelection.GetType(), name);
            Debug.Log("fieldInfo = " + fieldInfo);
            if (fieldInfo != null)
            {
                Debug.Log("  " + fieldInfo.GetValue(currentSelection));
                return;
            }

            PropertyInfo propertyInfo = AccessTools.Property(currentSelection.GetType(), name);
            Debug.Log("propertyInfo = " + propertyInfo);
            if (propertyInfo != null)
            {
                Debug.Log("  " + propertyInfo.GetValue(currentSelection, null));
                return;
            }

            Debug.Log("  Unknown field/property '" + name + "'.");
        }

        private static bool HasSelection()
        {
            return selected.Count > 0;
        }

        private static void Invoke()
        {
            if (!HasSelection())
            {
                Debug.Log("  Current selection is empty.");
                return;
            }

            int numParameters = uConsole.GetNumParameters();
            if (numParameters == 0)
            {
                Debug.Log("  Method name required.");
                return;
            }

            object currentSelection = CurrentSelection();
            MethodInfo methodInfo = AccessTools.Method(currentSelection.GetType(), uConsole.GetString(), new Type[0]);
            if (methodInfo == null)
            {
                Debug.Log("  Method not found. (Methods with parameters currently not supported.)");
                return;
            }

            Debug.Log("  Invoking " + methodInfo);
            var result = methodInfo.Invoke(currentSelection, new object[0]);
            Debug.Log("  result = " + result);
        }

        private static void PrintChildren()
        {
            if (!HasSelection())
            {
                Debug.Log("Current selection is empty.");
                return;
            }

            GameObject gameObject = CurrentGameObject();
            if (gameObject != null)
            {
                foreach (Transform eachChild in gameObject.transform)
                {
                    Debug.Log("  " + eachChild.name);
                }
                return;
            }

            object currentSelection = CurrentSelection();
            if (currentSelection is Scene)
            {
                Scene scene = (Scene)currentSelection;
                foreach (GameObject eachRootGameObject in scene.GetRootGameObjects())
                {
                    Debug.Log("  " + eachRootGameObject.name);
                }
                return;
            }

            Debug.Log(CurrentSelection().GetType());

            System.Collections.IEnumerable enumerable = CurrentSelection() as System.Collections.IEnumerable;
            if (enumerable != null)
            {
                int index = 0;
                foreach (object value in enumerable)
                {
                    Debug.Log("  " + index++ + ": " + value);
                }
                return;
            }

            Debug.Log("Cannot print children of " + CurrentSelection() + ".");
        }

        private static void PrintComponents()
        {
            if (!HasSelection())
            {
                Debug.Log("  Current selection is empty.");
                return;
            }

            GameObject gameObject = CurrentGameObject();
            if (gameObject == null)
            {
                Debug.Log("   Cannot print components if the current selection is not a GameObject.");
                return;
            }

            Component[] components = gameObject.GetComponents<Component>();
            foreach (Component eachComponent in components)
            {
                Debug.Log("  " + eachComponent);
            }
        }

        private static void PrintDetails()
        {
            if (!HasSelection())
            {
                Debug.Log("Current selection is empty.");
                return;
            }

            GameObject gameObject = CurrentGameObject();
            if (gameObject != null)
            {
                Debug.Log(DumpUtils.FormatGameObject(gameObject.name, gameObject));
                return;
            }

            System.Collections.IEnumerable enumerable = CurrentSelection() as System.Collections.IEnumerable;
            if (enumerable != null)
            {
                foreach (object value in enumerable)
                {
                    Debug.Log(DumpUtils.FormatValue(null, value));
                }
                return;
            }

            Debug.Log("Cannot print details of " + CurrentSelection());
        }

        private static void PrintHierarchy()
        {
            if (!HasSelection())
            {
                Debug.Log("Current selection is empty.");
                return;
            }

            GameObject gameObject = CurrentGameObject();
            if (gameObject == null)
            {
                Debug.Log("Cannot print hierarchy if the turrent selection is not a GameObject.");
                return;
            }

            Debug.Log(DumpUtils.FormatHierarchy(gameObject));
        }

        private static void PrintPath()
        {
            if (!HasSelection())
            {
                Debug.Log("Current selection is empty.");
                return;
            }

            GameObject gameObject = CurrentGameObject();
            if (gameObject == null)
            {
                Debug.Log("Cannot print hierarchy if the turrent selection is not a GameObject.");
                return;
            }

            Debug.Log(DumpUtils.FormatPath(gameObject));
        }

        private static void PrintSelected()
        {
            Debug.Log("  Selected: " + CurrentSelection());
        }

        private static void PrintTransform()
        {
            if (!HasSelection())
            {
                Debug.Log("  Current selection is empty.");
                return;
            }

            GameObject gameObject = CurrentGameObject();
            if (gameObject == null)
            {
                Debug.Log("   Cannot print position if the current selection is not a GameObject.");
                return;
            }

            Debug.Log("  Position: " + gameObject.transform.localPosition.ToString("F3") + " / " + gameObject.transform.position.ToString("F3"));
            Debug.Log("  Rotation: " + gameObject.transform.localRotation.eulerAngles.ToString("F3") + " / " + gameObject.transform.rotation.eulerAngles.ToString("F3"));
            Debug.Log("  Scale: " + gameObject.transform.localScale.ToString("F3"));
        }

        private static void PrintType()
        {
            if (!HasSelection())
            {
                Debug.Log("  Current selection is empty.");
                return;
            }

            Debug.Log("  " + CurrentSelection().GetType());
        }

        private static void Select()
        {
            int numParameters = uConsole.GetNumParameters();
            if (numParameters == 0)
            {
                PrintSelected();
                return;
            }

            if (numParameters == 1)
            {
                string value = uConsole.GetString();
                string[] parts = value.Split('/');
                foreach (string eachPart in parts)
                {
                    if (!Select(eachPart))
                    {
                        Debug.Log("  Could not select '" + eachPart + "'.");
                        break;
                    }
                }
            }
            else
            {
                Debug.Log("  Unsupported number of parameters.");
            }

            PrintSelected();
        }

        private static bool Select(string selector)
        {
            if ("GameManager".Equals(selector, StringComparison.InvariantCultureIgnoreCase))
            {
                FieldInfo fieldInfo = AccessTools.Field(typeof(GameManager), "m_Instance");
                selected.Clear();
                selected.Push(fieldInfo.GetValue(null));
                return true;
            }

            if ("HUD".Equals(selector, StringComparison.InvariantCultureIgnoreCase))
            {
                selected.Push(InterfaceManager.m_Panel_HUD);
                return true;
            }

            if ("InterfaceManager".Equals(selector, StringComparison.InvariantCultureIgnoreCase))
            {
                selected.Push(typeof(InterfaceManager));
                return true;
            }

            if ("TodMaterial".Equals(selector, StringComparison.InvariantCultureIgnoreCase))
            {
                selected.Push(typeof(TodMaterial));
                return true;
            }

            if ("Panel_Inventory_Examine".Equals(selector, StringComparison.InvariantCultureIgnoreCase))
            {
                selected.Push(InterfaceManager.m_Panel_Inventory_Examine);
                return true;
            }

            if ("MainMenu".Equals(selector, StringComparison.InvariantCultureIgnoreCase))
            {
                selected.Push(InterfaceManager.m_Panel_MainMenu);
                return true;
            }

            if (".." == selector || "Parent".Equals(selector, StringComparison.InvariantCultureIgnoreCase))
            {
                return SelectParent();
            }

            if ("Scene".Equals(selector, StringComparison.InvariantCultureIgnoreCase))
            {
                selected.Push(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
                return true;
            }

            return SelectChild(selector);
        }

        private static bool SelectChild(string name)
        {
            if (!HasSelection())
            {
                Debug.Log("Cannot select a child if the current selection is empty.");
                return false;
            }

            GameObject gameObject = CurrentGameObject();
            if (gameObject != null)
            {
                Transform child = gameObject.transform.Find(name);
                if (child != null)
                {
                    selected.Push(child.gameObject);
                    return true;
                }
            }

            if (CurrentSelection() is Scene scene)
            {
                foreach (GameObject eachRootGameObject in scene.GetRootGameObjects())
                {
                    if (eachRootGameObject.name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                    {
                        selected.Push(eachRootGameObject);
                        return true;
                    }
                }
            }

            return SelectComponent(name);
        }

        private static bool SelectComponent(string name)
        {
            if (!HasSelection())
            {
                Debug.Log("Cannot select a component if the current selection is empty.");
                return false;
            }

            GameObject gameObject = CurrentGameObject();
            if (gameObject != null)
            {
                Component component = gameObject.GetComponent(name);
                if (component != null)
                {
                    selected.Push(component);
                    return true;
                }
            }

            return SelectValue(name);
        }

        private static bool SelectIndex(string value)
        {
            if (!HasSelection())
            {
                Debug.Log("Cannot select an index if the current selection is empty.");
                return false;
            }

            int index;
            if (!int.TryParse(value, out index))
            {
                return false;
            }

            //Array array = CurrentSelection() as Array;
            //if (array != null)
            //{
            //    object valueAtIndex = array.GetValue(index);
            //    if (valueAtIndex != null)
            //    {
            //        selected.Push(valueAtIndex);
            //        return true;
            //    }
            //}

            System.Collections.IList list = CurrentSelection() as System.Collections.IList;
            if (list != null)
            {
                object valueAtIndex = list[index];
                if (valueAtIndex != null)
                {
                    selected.Push(valueAtIndex);
                    return true;
                }
            }

            return false;
        }

        private static bool SelectParent()
        {
            if (!HasSelection())
            {
                Debug.Log("Cannot select the parent if the current selection is empty.");
                return false;
            }

            GameObject gameObject = CurrentGameObject();
            if (gameObject == null)
            {
                Debug.Log("Cannot select the parent if the current selection is not a GameObject.");
                return false;
            }

            Transform parent = gameObject.transform.parent;
            if (parent == null)
            {
                Debug.Log("Current selection has no parent.");
                return false;
            }

            selected.Push(parent.gameObject);
            return true;
        }

        private static bool SelectValue(string name)
        {
            if (!HasSelection())
            {
                Debug.Log("Cannot select a value if the current selection is empty.");
                return false;
            }

            object currentSelection = CurrentSelection();
            if (currentSelection is Type)
            {
                FieldInfo fieldInfo = AccessTools.Field(currentSelection as Type, name);
                if (fieldInfo != null)
                {
                    object value = fieldInfo.GetValue(null);
                    if (value != null)
                    {
                        selected.Push(value);
                        return true;
                    }
                }
            }
            else
            {
                FieldInfo fieldInfo = AccessTools.Field(currentSelection.GetType(), name);
                if (fieldInfo != null)
                {
                    object value = fieldInfo.GetValue(currentSelection);
                    if (value != null)
                    {
                        selected.Push(value);
                        return true;
                    }
                }
            }

            return SelectIndex(name);
        }

        private static void Set()
        {
            if (!HasSelection())
            {
                Debug.Log("  Current selection is empty.");
                return;
            }

            int numParameters = uConsole.GetNumParameters();
            if (numParameters != 2)
            {
                Debug.Log("  Field name and value required.");
                return;
            }

            object currentSelection = CurrentSelection();
            var name = uConsole.GetString();

            FieldInfo fieldInfo = AccessTools.Field(currentSelection.GetType(), name);
            Debug.Log("fieldInfo = " + fieldInfo);
            if (fieldInfo != null)
            {
                if (fieldInfo.FieldType == typeof(string))
                {
                    fieldInfo.SetValue(currentSelection, uConsole.GetString());
                    return;
                }

                if (fieldInfo.FieldType == typeof(bool))
                {
                    fieldInfo.SetValue(currentSelection, uConsole.GetBool());
                    return;
                }

                if (fieldInfo.FieldType == typeof(float))
                {
                    fieldInfo.SetValue(currentSelection, uConsole.GetFloat());
                    return;
                }

                if (fieldInfo.FieldType == typeof(int))
                {
                    fieldInfo.SetValue(currentSelection, (int)uConsole.GetFloat());
                    return;
                }

                Debug.Log("  field type '" + fieldInfo.FieldType + "' not supported.");
                return;
            }

            PropertyInfo propertyInfo = AccessTools.Property(currentSelection.GetType(), name);
            Debug.Log("propertyInfo = " + propertyInfo);
            if (propertyInfo != null)
            {
                if (propertyInfo.PropertyType == typeof(string))
                {
                    propertyInfo.SetValue(currentSelection, uConsole.GetString(), null);
                    return;
                }

                if (propertyInfo.PropertyType == typeof(bool))
                {
                    propertyInfo.SetValue(currentSelection, uConsole.GetBool(), null);
                    return;
                }

                if (propertyInfo.PropertyType == typeof(float))
                {
                    propertyInfo.SetValue(currentSelection, uConsole.GetFloat(), null);
                    return;
                }

                if (propertyInfo.PropertyType == typeof(int))
                {
                    uConsole.GetString().Split(',');

                    propertyInfo.SetValue(currentSelection, (int)uConsole.GetFloat(), null);
                    return;
                }

                Debug.Log("  property type '" + propertyInfo.PropertyType + "' not supported.");
                return;
            }

            Debug.Log("  Unknown field/property '" + name + "'.");
        }

        private static void SetPosition()
        {
            if (!HasSelection())
            {
                Debug.Log("  Current selection is empty.");
                return;
            }

            GameObject gameObject = CurrentGameObject();
            if (gameObject == null)
            {
                Debug.Log("   Cannot set position if the current selection is not a GameObject.");
                return;
            }

            if (uConsole.GetNumParameters() != 3)
            {
                Debug.Log("   Need 3 parameters (x, y, z) to set the local position.");
                return;
            }

            gameObject.transform.localPosition = new Vector3(uConsole.GetFloat(), uConsole.GetFloat(), uConsole.GetFloat());
        }

        private static void SetRotation()
        {
            if (!HasSelection())
            {
                Debug.Log("  Current selection is empty.");
                return;
            }

            GameObject gameObject = CurrentGameObject();
            if (gameObject == null)
            {
                Debug.Log("   Cannot set position if the current selection is not a GameObject.");
                return;
            }

            if (uConsole.GetNumParameters() != 3)
            {
                Debug.Log("   Need 3 parameters (x, y, z) to set the local position.");
                return;
            }

            gameObject.transform.localRotation = Quaternion.Euler(uConsole.GetFloat(), uConsole.GetFloat(), uConsole.GetFloat());
        }

        private static void Show()
        {
            int numParameters = uConsole.GetNumParameters();
            if (numParameters == 0)
            {
                PrintSelected();
                return;
            }

            if (numParameters == 1)
            {
                string value = uConsole.GetString();
                if (Select(value))
                {
                    PrintSelected();
                    Back();
                }
            }
            else
            {
                Debug.Log("  Unsupported number of parameters.");
            }
        }

        private static void ShowStopwatch()
        {
            float elapsedGameTime = (Mathf.Min(GameManager.GetTimeOfDayComponent().GetNormalizedTime(), stopwatchStopGameTime) - stopwatchStartGameTime) * 86400;
            Debug.Log("Elapsed game time: " + FormatDuration(elapsedGameTime));

            float elapsedRealTime = Mathf.Min(Time.realtimeSinceStartup, stopwatchStopRealTime) - stopwatchStartRealTime;
            Debug.Log("Elapsed real time: " + FormatDuration(elapsedRealTime));
        }

        private static void StartStopwatch()
        {
            stopwatchStartGameTime = GameManager.GetTimeOfDayComponent().GetNormalizedTime();
            stopwatchStopGameTime = float.PositiveInfinity;

            stopwatchStartRealTime = Time.realtimeSinceStartup;
            stopwatchStopRealTime = float.PositiveInfinity;
        }

        private static void StopStopwatch()
        {
            stopwatchStopGameTime = GameManager.GetTimeOfDayComponent().GetNormalizedTime();
            stopwatchStopRealTime = Time.realtimeSinceStartup;

            ShowStopwatch();
        }

        private static void ToggleActive()
        {
            if (!HasSelection())
            {
                Debug.Log("  Current selection is empty.");
                return;
            }

            if (uConsole.GetNumParameters() == 1)
            {
                Select(uConsole.GetString());
            }

            GameObject gameObject = CurrentGameObject();
            if (gameObject == null)
            {
                Debug.Log("   Cannot toggle active if the current selection is not a GameObject.");
                return;
            }

            gameObject.SetActive(!gameObject.activeSelf);
            Debug.Log("  " + gameObject.name + " is now " + (gameObject.activeSelf ? "active" : "inactive"));

            if (uConsole.GetNumParameters() == 1)
            {
                Back();
            }
        }

        private static void ToggleActiveChildren()
        {
            if (!HasSelection())
            {
                Debug.Log("  Current selection is empty.");
                return;
            }

            GameObject gameObject = CurrentGameObject();
            if (gameObject == null)
            {
                Debug.Log("   Cannot toggle active if the current selection is not a GameObject.");
                return;
            }

            foreach (Transform child in gameObject.transform)
            {
                child.gameObject.SetActive(!child.gameObject.activeSelf);
                Debug.Log("  " + child.name + " is now " + (child.gameObject.activeSelf ? "active" : "inactive"));
            }
        }
    }
}