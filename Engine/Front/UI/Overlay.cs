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
        private static Dictionary<string, int> data = new Dictionary<string, int>();
        private static ImDrawListPtr imDrawList;
        
       /* public static bool Begin()
        {
            ImGuiWindowFlags ni = physical ? ImGuiWindowFlags.None : ImGuiWindowFlags.NoInputs;
            ImGui.SetNextWindowBgAlpha(0f);
            bool b = ImGui.Begin("##Over", ni | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoBringToFrontOnFocus);
            ImGui.SetWindowSize(ImGui.GetMainViewport().Size);
            ImGui.SetWindowPos(Vector2.Zero);
            return b;
        }*/
        public static bool Begin(float alphaonfocus, Vector2 scale)
        {
            ImGuiWindowFlags ni = physical ? ImGuiWindowFlags.None : ImGuiWindowFlags.NoInputs;
            float alpha = physical ? alphaonfocus : 0f;
            physical = false;
            ImGui.SetNextWindowBgAlpha(alpha);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0);
            bool b = ImGui.Begin("##Over", ni | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoBringToFrontOnFocus);
            ImGui.SetWindowSize(ImGui.GetMainViewport().Size * scale);
            ImGui.SetWindowPos(Vector2.Zero);
            imDrawList = ImGui.GetWindowDrawList();
            return b;
        }
        public static void End()
        {
            ImGui.End();
            ImGui.PopStyleVar(1);
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

        public static bool CollapsingHeader(string v)
        {
            return CollapsingHeader(v, new Vector2(ImGui.GetContentRegionAvail().X, 16));
        }
        public static bool CollapsingHeader(string v, Vector2 size)
        {
            if (!data.ContainsKey(v))
                data.Add(v, 0);

            bool on = data[v] == 1 ? true : false;
            

            ImGui.ArrowButton("Stuff", on ? ImGuiDir.Down : ImGuiDir.Right);

            ImGui.SameLine();
            Vector2 cursor = ImGui.GetCursorScreenPos();   
            imDrawList.AddRectFilled(cursor, cursor + size, ImGui.GetColorU32(ImGuiCol.FrameBg));
            imDrawList.AddText(cursor, ImGui.GetColorU32(ImGuiCol.Text), v);
            ImGui.Dummy(size);


            if (ImGui.IsItemClicked())
                on = !on;

            data[v] = on ? 1 : 0;

            physical = physical || ImGui.IsMouseHoveringRect(cursor, cursor+size);
            return on;
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
