using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;

using UnityEngine;
using UnityStandardAssets.ImageEffects;
using UnityEngine.SceneManagement;

namespace DumpData
{
    class DumpPrefab
    {
        public static void OnLoad()
        {
            uConsole.RegisterCommand("dump", new uConsole.DebugCommand(Dump));
            uConsole.RegisterCommand("dump-all", new uConsole.DebugCommand(DumpAll));

            uConsole.RegisterCommand("location", new uConsole.DebugCommand(Location));
            uConsole.RegisterCommand("freeze-aurora", new uConsole.DebugCommand(FreezeAurora));
            uConsole.RegisterCommand("heap", new uConsole.DebugCommand(Heap));

            uConsole.RegisterCommand("Restore-Quality", new uConsole.DebugCommand(RestoreQuality));

            uConsole.RegisterCommand("List-Close", new uConsole.DebugCommand(ListClose));
            uConsole.RegisterCommand("Load-Resource", new uConsole.DebugCommand(LoadResource));
        }

        private static void LoadResource()
        {
            if (uConsole.GetNumParameters() != 1)
            {
                Debug.Log("  Resource name required");
                return;
            }

            string name = uConsole.GetString();
            Debug.Log("  Loading " + name);
            Object resource = Resources.Load(name);
            if (resource == null)
            {
                Debug.Log("  Not found");
                return;
            }

            Debug.Log(DumpUtils.FormatValue(name, resource));
        }

        private static void ListClose()
        {
            float maxDistance = uConsole.GetNumParameters() >= 1 ? uConsole.GetFloat() : 1;
            string name = uConsole.GetNumParameters() >= 2 ? uConsole.GetString() : "";
            name = name.ToLowerInvariant();

            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
            {
                Scene scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);

                List<GameObject> gameObjects = new List<GameObject>();
                gameObjects.AddRange(scene.GetRootGameObjects());

                while (gameObjects.Count > 0)
                {
                    GameObject gameObject = gameObjects.First();
                    gameObjects.RemoveAt(0);

                    float distance = Vector3.Distance(GameManager.GetPlayerTransform().position, gameObject.transform.position);
                    if (distance < maxDistance && gameObject.name.ToLowerInvariant().Contains(name))
                    {
                        Debug.Log(GetPath(gameObject) + ": " + distance + ", " + gameObject.transform.position.ToString("F3") + ", " + gameObject.activeSelf + "/" + gameObject.activeInHierarchy);
                    }

                    for (int j = 0; j < gameObject.transform.childCount; j++)
                    {
                        GameObject child = gameObject.transform.GetChild(j).gameObject;
                        gameObjects.Insert(j, child);
                    }
                }
            }
        }

        private static string GetPath(GameObject gameObject)
        {
            StringBuilder stringBuilder = new StringBuilder();

            Transform current = gameObject.transform;

            while (current != null)
            {
                stringBuilder.Insert(0, current.name);
                stringBuilder.Insert(0, "/");

                current = current.transform.parent;
            }

            return stringBuilder.ToString();
        }

        private static void Heap()
        {
            long total = System.GC.GetTotalMemory(false);
            Debug.Log("Total Memory = " + total / 1024 / 1024 + " MiB");
        }

        private static void RestoreQuality()
        {
            GameManager.GetQualitySettingsManager().ApplyCurrentQualitySettings();
        }

        private static void FreezeAurora()
        {
            AuroraBand[] auroraBands = Resources.FindObjectsOfTypeAll<AuroraBand>();
            foreach (AuroraBand eachAuroraBand in auroraBands)
            {
                if (eachAuroraBand.m_AnimSpeed == 0)
                {
                    eachAuroraBand.m_AnimSpeed = 0.04f;
                }
                else
                {
                    eachAuroraBand.m_AnimSpeed = 0;
                }
            }
        }

        private static void Location()
        {
            Debug.Log(DumpUtils.FormatComponent(null, GameManager.GetPlayerTransform()));
        }

        private static void Dump()
        {
            string name = uConsole.GetString();

            GameObject gameObject = (GameObject)Resources.Load(name);
            Debug.Log(DumpUtils.FormatGameObject(gameObject.name, gameObject));
        }

        private static void DumpAll()
        {
            string name = uConsole.GetString();
            System.Type type = System.Type.GetType(name + ",Assembly-CSharp", false, true);
            if (type == null)
            {
                type = System.Type.GetType(name + ",UnityEngine", false, true);
            }

            if (type == null)
            {
                Debug.LogError("Unknown type '" + name + "'.");
                return;
            }

            foreach (Object eachObject in Resources.FindObjectsOfTypeAll(type))
            {
                if (eachObject is Component)
                {
                    GameObject gameObject = ((Component)eachObject).gameObject;
                    Debug.Log(DumpUtils.FormatGameObject(null, gameObject));
                }
                else
                {
                    Debug.Log(DumpUtils.FormatValue(null, eachObject));
                }
            }
        }
    }
}
