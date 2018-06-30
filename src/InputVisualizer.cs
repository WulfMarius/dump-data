using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Harmony;

namespace DumpData
{
    [HarmonyPatch(typeof(Panel_HUD), "Start")]
    class InputVisualizer
    {
        private const int Y = 320;
        private const int LABEL_SIZE = 50;

        public static void Postfix(Panel_HUD __instance)
        {
            GameObject root = NGUITools.GetRoot(__instance.m_FullScreenMessageObject);

            int x = -150;
            CreateLabel(KeyCode.R, root, x);
            x += LABEL_SIZE;
            CreateLabel(KeyCode.L, root, x);
            x += LABEL_SIZE;
            CreateLabel(KeyCode.P, root, x);
            x += LABEL_SIZE;
            CreateLabel(KeyCode.LeftControl, root, x);
            x += LABEL_SIZE;
            CreateLabel(KeyCode.LeftShift, root, x);
            x += LABEL_SIZE;
            CreateWheelLabel(root, x);
        }

        private static UILabel CreateLabel(KeyCode keyCode, GameObject parent, int x)
        {
            UILabel label = NGUITools.AddChild<UILabel>(parent);

            label.text = getName(keyCode);
            label.border = new Vector4(2, 2, 2, 2);
            label.alignment = NGUIText.Alignment.Center;
            label.color = new Color(0.9f, 0.9f, 0.9f, 0.9f);
            label.trueTypeFont = Font.CreateDynamicFontFromOSFont("Arial", 20);
            label.fontSize = 20;
            label.pivot = UIWidget.Pivot.Bottom;
            label.SetRect(x, Y, LABEL_SIZE, LABEL_SIZE);

            ShowKey showKey = label.gameObject.AddComponent<ShowKey>();
            showKey.KeyCode = keyCode;
            showKey.Label = label;

            return label;
        }

        private static string getName(KeyCode keyCode)
        {
            if (keyCode == KeyCode.LeftControl)
            {
                return "CTRL";
            }

            if (keyCode == KeyCode.LeftShift)
            {
                return "SHIFT";
            }

            return Enum.GetName(typeof(KeyCode), keyCode);
        }

        private static UILabel CreateWheelLabel(GameObject parent, int x)
        {
            UILabel label = NGUITools.AddChild<UILabel>(parent);

            label.text = "Mouse Wheel";
            label.border = new Vector4(2, 2, 2, 2);
            label.alignment = NGUIText.Alignment.Center;
            label.color = new Color(0.9f, 0.9f, 0.9f, 0.9f);
            label.trueTypeFont = Font.CreateDynamicFontFromOSFont("Arial", 20);
            label.fontSize = 20;
            label.pivot = UIWidget.Pivot.Bottom;
            label.SetRect(x, Y, LABEL_SIZE, LABEL_SIZE);

            ShowWheel showWheel = label.gameObject.AddComponent<ShowWheel>();
            showWheel.Label = label;

            return label;
        }
    }

    internal class ShowKey : MonoBehaviour
    {
        public KeyCode KeyCode;
        public UILabel Label;

        void Update()
        {
            float targetAlpha = Input.GetKey(KeyCode) ? 1 : 0;
            Label.alpha = Mathf.Lerp(Label.alpha, targetAlpha, 0.2f);
        }
    }

    internal class ShowWheel : MonoBehaviour
    {
        public UILabel Label;

        void Update()
        {
            if (Input.mouseScrollDelta.y == 0)
            {
                Label.alpha = Mathf.Lerp(Label.alpha, 0, 0.1f);
            } else
            {
                Label.alpha = Mathf.Lerp(Label.alpha, 5, 0.2f);
            }
        }
    }
}
