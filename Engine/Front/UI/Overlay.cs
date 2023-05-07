using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace FSEngine.UI
{
    public static class ImOverlay
    {
        private static bool physical = false;

        
        public static bool Begin()
        {
            ImGuiWindowFlags ni = physical ? ImGuiWindowFlags.None : ImGuiWindowFlags.NoInputs;
            ImGui.SetNextWindowBgAlpha(0f);
            bool b = ImGui.Begin("##Over", ni | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoBringToFrontOnFocus);
            ImGui.SetWindowSize(ImGui.GetMainViewport().Size);
            ImGui.SetWindowPos(Vector2.Zero);
            return b;
        }
        public static bool Begin(float alphaonfocus)
        {
            ImGuiWindowFlags ni = physical ? ImGuiWindowFlags.None : ImGuiWindowFlags.NoInputs;
            float alpha = physical ? alphaonfocus : 0f;
            physical = false;
            ImGui.SetNextWindowBgAlpha(alpha);
            bool b = ImGui.Begin("##Over", ni | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoBringToFrontOnFocus);
            ImGui.SetWindowSize(ImGui.GetMainViewport().Size);
            ImGui.SetWindowPos(Vector2.Zero);
            return b;
        }
        public static void End()
        {
            ImGui.End();
        }
        public static bool Button(string label)
        {
            bool click = ImGui.Button(label);
            physical = physical || ImGui.IsMouseHoveringRect(ImGui.GetItemRectMin(), ImGui.GetItemRectMax());
            return click;
        }
        public static bool RadioButton(string label, bool active)
        {
            bool click = ImGui.RadioButton(label, active);
            physical = physical || ImGui.IsMouseHoveringRect(ImGui.GetItemRectMin(), ImGui.GetItemRectMax());
            return click;
        }
        public static bool Checkbox(string label, ref bool b)
        {
            bool click = ImGui.Checkbox(label, ref b);
            physical = physical || ImGui.IsMouseHoveringRect(ImGui.GetItemRectMin(), ImGui.GetItemRectMax());
            return click;
        }
        public static void SliderInt(string label, ref int v, int v_min, int v_max)
        {
            ImGui.SliderInt(label, ref v, v_min, v_max);
            physical = physical || ImGui.IsMouseHoveringRect(ImGui.GetItemRectMin(), ImGui.GetItemRectMax());
        }
        public static void SliderFloat(string label, ref float v, float v_min, float v_max)
        {
            ImGui.SliderFloat(label, ref v, v_min, v_max);
            physical = physical || ImGui.IsMouseHoveringRect(ImGui.GetItemRectMin(), ImGui.GetItemRectMax());
        }
        public static void InputInt(string label, ref int v)
        {
            ImGui.InputInt(label, ref v);
            physical = physical || ImGui.IsMouseHoveringRect(ImGui.GetItemRectMin(), ImGui.GetItemRectMax());
        }
        public static void InputFloat(string label, ref float v)
        {
            ImGui.InputFloat(label, ref v);
            physical = physical || ImGui.IsMouseHoveringRect(ImGui.GetItemRectMin(), ImGui.GetItemRectMax());
        }
        public static void InputText(string label, ref string intput, uint max_lengt)
        {
            ImGui.InputText(label, ref intput, max_lengt);
            physical = physical || ImGui.IsMouseHoveringRect(ImGui.GetItemRectMin(), ImGui.GetItemRectMax());
        }

        public static void MakeInteractable()
        {
            physical = physical || ImGui.IsMouseHoveringRect(ImGui.GetItemRectMin(), ImGui.GetItemRectMax());
        }
    }
}
