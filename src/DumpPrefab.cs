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
            uConsole.RegisterCommand("cameras", new uConsole.DebugCommand(PrintCameras));
            uConsole.RegisterCommand("vp_weapons", new uConsole.DebugCommand(PrintWeapons));
            uConsole.RegisterCommand("UniStorm", new uConsole.DebugCommand(UniStorm));
            uConsole.RegisterCommand("location", new uConsole.DebugCommand(Location));
            uConsole.RegisterCommand("camera-effects", new uConsole.DebugCommand(CameraEffects));
            uConsole.RegisterCommand("toggle-bloom", new uConsole.DebugCommand(ToggleBloom));
            uConsole.RegisterCommand("toggle-chroma", new uConsole.DebugCommand(ToggleChroma));
            uConsole.RegisterCommand("freeze-aurora", new uConsole.DebugCommand(FreezeAurora));
            uConsole.RegisterCommand("clouds", new uConsole.DebugCommand(Clouds));
            uConsole.RegisterCommand("heap", new uConsole.DebugCommand(Heap));

            uConsole.RegisterCommand("Shoot", new uConsole.DebugCommand(Shoot));

            uConsole.RegisterCommand("Pass-Out", new uConsole.DebugCommand(PassOut));

            uConsole.RegisterCommand("Restore-Quality", new uConsole.DebugCommand(RestoreQuality));

            uConsole.RegisterCommand("Log-Textures", new uConsole.DebugCommand(LogTextures));

            uConsole.RegisterCommand("List-Close", new uConsole.DebugCommand(ListClose));
            uConsole.RegisterCommand("Mark-Cleanables", new uConsole.DebugCommand(MarkCleanables));
            uConsole.RegisterCommand("Remove-Cleanables", new uConsole.DebugCommand(RemoveCleanables));

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

            Scene scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();

            List<GameObject> gameObjects = new List<GameObject>();
            gameObjects.AddRange(scene.GetRootGameObjects());

            while (gameObjects.Count > 0)
            {
                GameObject gameObject = gameObjects.First();
                gameObjects.RemoveAt(0);

                //if (!gameObject.activeSelf)
                //{
                //    continue;
                //}

                float distance = Vector3.Distance(GameManager.GetPlayerTransform().position, gameObject.transform.position);
                if (distance < maxDistance && gameObject.name.ToLowerInvariant().Contains(name))
                {
                    Debug.Log(GetPath(gameObject) + ": " + distance + ", " + gameObject.transform.position.ToString("F3") + ", " + gameObject.activeSelf + "/" + gameObject.activeInHierarchy);
                }

                for (int i = 0; i < gameObject.transform.childCount; i++)
                {
                    GameObject child = gameObject.transform.GetChild(i).gameObject;
                    gameObjects.Insert(i, child);
                }
            }
        }

        private static void MarkCleanables()
        {
            Scene scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();

            List<GameObject> gameObjects = new List<GameObject>();
            gameObjects.AddRange(scene.GetRootGameObjects());

            while (gameObjects.Count > 0)
            {
                GameObject gameObject = gameObjects.First();
                gameObjects.RemoveAt(0);

                if (gameObject.name.Equals("FX") || gameObject.name.Equals("Lighting") || gameObject.name.Equals("Containers"))
                {
                    continue;
                }

                if (IsRemovableDeco(gameObject))
                {
                    Debug.Log("MARKED " + GetPath(gameObject));

                    MeshRenderer renderer = gameObject.GetComponentInChildren<MeshRenderer>();
                    if (renderer != null)
                    {
                        if (renderer.material != null)
                        {
                            renderer.material.color = new Color(0, 1, 0);
                            renderer.material.shader = Shader.Find("Shader Forge/TLD_StandardObjMove");
                        }
                    }
                    continue;
                }

                Debug.Log(GetPath(gameObject));

                for (int i = 0; i < gameObject.transform.childCount; i++)
                {
                    GameObject child = gameObject.transform.GetChild(i).gameObject;
                    gameObjects.Insert(i, child);
                }
            }
        }

        private static void RemoveCleanables()
        {
            Scene scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();

            List<GameObject> gameObjects = new List<GameObject>();
            gameObjects.AddRange(scene.GetRootGameObjects());

            while (gameObjects.Count > 0)
            {
                GameObject gameObject = gameObjects.First();
                gameObjects.RemoveAt(0);

                if (gameObject.name.Equals("FX") || gameObject.name.Equals("Lighting"))
                {
                    continue;
                }

                if (IsRemovableDeco(gameObject))
                {
                    Debug.Log("REMOVED " + GetPath(gameObject));

                    gameObject.SetActive(false);
                    continue;
                }

                Debug.Log(GetPath(gameObject));

                for (int i = 0; i < gameObject.transform.childCount; i++)
                {
                    GameObject child = gameObject.transform.GetChild(i).gameObject;
                    gameObjects.Insert(i, child);
                }
            }
        }

        private static bool IsRemovableDeco(GameObject gameObject)
        {
            if (gameObject.layer == vp_Layer.Gear)
            {
                return false;
            }

            if (gameObject.GetComponent<BreakDown>() != null || gameObject.GetComponent<Container>() != null || gameObject.GetComponent<GearItem>() != null)
            {
                return false;
            }

            //return true;
            return gameObject.name.Contains("BandageHeavyBloody") ||
                gameObject.name.Contains("Decal-") ||
                gameObject.name.Contains("SingleCableD") ||
                gameObject.name.Contains("SingleCableE") ||
                gameObject.name.Contains("SingleCableF") ||
                gameObject.name.Contains("StickyNote") ||
                gameObject.name.Contains("PaperDeco") ||
                gameObject.name.Contains("PaintCan") ||
                gameObject.name.Contains("ClipBoard") ||
                gameObject.name.Contains("Cobweb") ||
                gameObject.name.Contains("ComputerLaptop") ||
                gameObject.name.Contains("CannedBeansUsed") ||
                gameObject.name.Contains("PaperDecoReciept") ||
                gameObject.name.Contains("KitchenCabinetDoor") ||
                gameObject.name.Contains("WallDeco") ||
                gameObject.name.Contains("Pitcher") ||
                gameObject.name.Contains("DishPlate") ||
                gameObject.name.Contains("DishBowl") ||
                gameObject.name.Contains("PaperDebris") ||
                gameObject.name.Contains("CupA") ||
                gameObject.name.Contains("Drawer") ||
                gameObject.name.Contains("Toy");
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

        private static void LogTextures()
        {
            UITexture[] textures = Resources.FindObjectsOfTypeAll<UITexture>();
            foreach (UITexture eachTexture in textures)
            {
                if (eachTexture.isActiveAndEnabled && eachTexture.mainTexture)
                {
                    Debug.Log(DumpData.DumpUtils.FormatGameObject(eachTexture.name, eachTexture.gameObject));
                }
            }
        }

        private static void RestoreQuality()
        {
            GameManager.GetQualitySettingsManager().ApplyCurrentQualitySettings();
        }

        private static void PassOut()
        {
            GameManager.GetRestComponent().BeginSleeping(new Bed(), 1, 1);
        }

        private static void Shoot()
        {
            if (uConsole.GetNumParameters() != 6)
            {
                Debug.Log("6 parameters required");
                return;
            }

            vp_FPSCamera camera = GameManager.GetVpFPSCamera();
            camera.AddForce2(uConsole.GetFloat(), uConsole.GetFloat(), uConsole.GetFloat());
            camera.AddRotationForce(uConsole.GetFloat(), uConsole.GetFloat(), uConsole.GetFloat());
        }

        private static void Clouds()
        {
            UniStormWeatherSystem uniStorm = GameManager.GetUniStorm();

            Debug.Log(DumpUtils.FormatGameObject("m_lightClouds1", uniStorm.m_LightClouds1));
            Debug.Log(DumpUtils.FormatGameObject("m_lightClouds2", uniStorm.m_LightClouds2));
            Debug.Log(DumpUtils.FormatGameObject("m_HighClouds1", uniStorm.m_HighClouds1));
            Debug.Log(DumpUtils.FormatGameObject("m_MostlyCloudyClouds", uniStorm.m_MostlyCloudyClouds));
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

        private static void ToggleBloom()
        {
            CameraEffects cameraEffects = GameManager.GetUniStorm().m_MainCamera.GetComponentInChildren<CameraEffects>();
            SkyBloom skyBloom = cameraEffects.GetComponentInChildren<SkyBloom>();
            if (skyBloom != null)
            {
                skyBloom.enabled = !skyBloom.enabled;
                Debug.Log("SkyBloom.enabled = " + skyBloom.enabled);
            }
        }

        private static float originalChromaticAberration;

        private static void ToggleChroma()
        {
            CameraEffects cameraEffects = GameManager.GetUniStorm().m_MainCamera.GetComponentInChildren<CameraEffects>();

            if (cameraEffects.m_Vignetting.chromaticAberration == 0)
            {
                cameraEffects.m_Vignetting.chromaticAberration = originalChromaticAberration;
            }
            else
            {
                originalChromaticAberration = cameraEffects.m_Vignetting.chromaticAberration;
                cameraEffects.m_Vignetting.chromaticAberration = 0;
            }
        }

        private static void CameraEffects()
        {
            CameraEffects cameraEffects = GameManager.GetUniStorm().m_MainCamera.GetComponentInChildren<CameraEffects>();
            Debug.Log(DumpUtils.FormatGameObject(null, cameraEffects.gameObject));
        }

        private static void Location()
        {
            Debug.Log(DumpUtils.FormatComponent(null, GameManager.GetPlayerTransform()));
        }

        private static void AddToLootTable()
        {
            LootTable lootTable = Resources.FindObjectsOfTypeAll<LootTable>().First(l => l.name == "LootTableSafe");
            if (lootTable == null)
            {
                Debug.Log("LootTable not found.");
                return;
            }

            lootTable.m_Prefabs.Insert(0, (GameObject)Resources.Load("GEAR_Binoculars"));
            lootTable.m_Weights.Insert(0, 1);
        }

        private static void UniStorm()
        {
            UniStormWeatherSystem uniStormWeatherSystem = GameManager.GetUniStorm();
            GameObject starSphere = uniStormWeatherSystem.m_StarSphere;
            Debug.Log(DumpUtils.FormatGameObject("starSphere", starSphere));

            ObjExporter.DoExport(starSphere, true);

            Renderer starSphereRenderer = starSphere.GetComponent<Renderer>();
            Material material = starSphereRenderer.material;
            Debug.Log(DumpUtils.FormatValue("material", material));
            Debug.Log(DumpUtils.FormatValue("mainTexture", material.mainTexture));
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

        private static void PrintCameras()
        {
            using (StreamWriter writer = new StreamWriter(@"C:\Users\st_dg\OneDrive\TLD\Camera.txt"))
            {
                foreach (Component eachItem in Resources.FindObjectsOfTypeAll<Camera>())
                {
                    writer.WriteLine(DumpUtils.FormatGameObject(eachItem.name, eachItem.gameObject));
                }
            }
        }

        private static void PrintWeapons()
        {
            using (StreamWriter writer = new StreamWriter(@"C:\Users\st_dg\OneDrive\TLD\vp_FPSWeapon.txt"))
            {
                foreach (Component eachItem in Resources.FindObjectsOfTypeAll<vp_FPSWeapon>())
                {
                    writer.WriteLine(DumpUtils.FormatComponent(eachItem.name, eachItem));
                }
            }
        }
    }
}
