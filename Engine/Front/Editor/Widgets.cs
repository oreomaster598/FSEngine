using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSEngine.Editor
{
    public static class Widgets
    {
        public static void EnumCombo<T>(string label, ref T selected)
        {

            if (ImGui.BeginCombo(label, selected.ToString()))
            {
                foreach (string s in Enum.GetNames(typeof(T)))
                {
                    if (ImGui.Selectable(s))
                        selected = (T)Enum.Parse(typeof(T), s);
                }
                ImGui.EndCombo();
            }
        }
    }
}
