using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using FSEngine.QoL;
using ImGuiNET;
namespace FSEngine.UI
{
    public static class ImUtils
    {
        public static void DrawTextFormated(string fmt)
        {
            string tmp = "";
            uint c = ImGui.GetColorU32(ImGuiCol.Text);
            float sz = ImGui.GetFontSize();
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0,0));
            ImGuiStylePtr style = ImGui.GetStyle();
            for (int i = 0; i < fmt.Length; i++)
            {

                if(fmt[i] == '[')
                {
                    i++;
                    if (char.IsDigit(fmt[i]))
                    {
                        string col = "";
                        while (char.IsDigit(fmt[i]))
                        {
                            col += fmt[i];
                            i++;
                        }
                        style.Colors[(int)ImGuiCol.Text] = uint.Parse(col).ToColorVec4();

                        if (fmt[i] == ':')
                        {
                            i++;

                            while (fmt[i] != ']')
                            {
                                tmp += fmt[i];
                                i++;
                            }
                            ImGui.SameLine();
                            ImGui.Text(tmp);
                            tmp = "";
                        }
                    }
                    else if (fmt[i] == 't')
                    {
                        i++;
                        if (fmt[i] == ':')
                        {
                            i++;
                            string id = "";
                            while (char.IsDigit(fmt[i]))
                            {
                                id += fmt[i];
                                i++;
                            }
                            uint tex = uint.Parse(id);
                            ImGui.SameLine();
                            ImGui.Image((IntPtr)tex, new Vector2(sz, sz));

                        }
                    }
                }
 
            }
            style.Colors[(int)ImGuiCol.Text] = c.ToColorVec4();
            ImGui.PopStyleVar(1);
        }
    }
}
