using System;
using System.Collections.Generic;
using System.IO;
using Vector2 = System.Numerics.Vector2;
using ImGuiNET;
using System.Drawing;

namespace FSEngine.QoL
{
    public static class Ext
    {
        public static System.Numerics.Vector4 ToVec4(this Color color)
        {
            return new System.Numerics.Vector4(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
        }
        public static System.Numerics.Vector4 ToColorVec4(this uint t)
        {
            Color color = Color.FromArgb((int)t);
            return new System.Numerics.Vector4(color.R / 255f, color.G / 255f, color.B / 255f, 1);
        }
    }
    public class TooltipLine
    {
        public Color color;
        public string text;
        public string name;

        public TooltipLine(string text, string name, Color color)
        {
            this.text = text;
            this.name = name;
            this.color = color;
        }
        public void Draw()
        {
            ImGui.PushStyleColor(ImGuiCol.Text, color.ToVec4());
            ImGui.Text(text);
            ImGui.PopStyleColor();
        }
    }

    public class Item
    {
        public string Name;
        public string Description;
        public int count;
        public int id;
        public bool stackable;
        public IntPtr texture;
        public virtual void UpdateInventory()
        {

        }
        public virtual void ModifyTooltips(ref List<TooltipLine> tooltipLines)
        {

        }
        public virtual void DrawInUI()
        {
            ImGui.Image(texture, new Vector2(64, 64));
        }
        public Item Clone()
        {
            return (Item)MemberwiseClone();
        }
        public virtual List<TooltipLine> GetTooltips()
        {
            List<TooltipLine> lines = new List<TooltipLine>();
            string[] lns = Description.Split('\n');
            for (int i = 0; i < lns.Length; i++)
            {
                lines.Add(new TooltipLine(lns[i], $"Tooltip{i}", Color.White));
            }
            return lines;
        }
    }
    public static class InventoryManager
    {
        internal static Item hover;
        internal static bool item_swap = true;
        internal static Container item_src_inventory;
        internal static int item_src = 0;
        internal static List<Container> containers = new List<Container>();

        public static void Add(Container c)
        {
            containers.Add(c);
        }

        public static void Remove(Container c)
        {
            containers.Remove(c);
        }

        public static void Draw()
        {
            if (hover != null)
            {
                ImGui.PushStyleColor(ImGuiCol.PopupBg, 0);
                ImGui.PushStyleColor(ImGuiCol.Border, 0);
                ImGui.BeginTooltip();
                // ImGui.Image(hover.texture, new Vector2(64, 64));
                hover.DrawInUI();
                ImGui.EndTooltip();
                ImGui.PopStyleColor(2);
            }

            int idx = 0;
            foreach(Container c in containers)
            {
                if(c.open)
                {
                    ImGui.Begin($"##c_{idx}", ImGuiWindowFlags.NoTitleBar  | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollbar);
                    c.Draw();
                    ImGui.End();
                }
                idx++;
            }
        }
    }
    public class Slot
    {
        public Item item;

        public Slot(Item item)
        {
            this.item = item;
        }
        public Slot()
        {

        }
        internal void Update(Container inventory, ImDrawListPtr drawList)
        {
            int index = inventory.current_index;
            Item item = inventory.slots[index].item;
            if (item != null)
            {
                if (item.texture != IntPtr.Zero && InventoryManager.hover != item)
                {
                    ImGui.PushID($"itm:{index}");
                    inventory.RenderSlotFull(inventory.slots[index], drawList);
                    if (ImGui.IsItemClicked())
                    {
                        if (InventoryManager.hover == null)
                        {
                            InventoryManager.hover = item;
                            InventoryManager.item_src = index;
                            InventoryManager.item_src_inventory = inventory;
                        }
                        else if (InventoryManager.item_swap)
                        {
                            if (item.id == InventoryManager.hover.id && InventoryManager.hover.stackable)
                            {
                                inventory.slots[index].item.count += InventoryManager.hover.count;
                                InventoryManager.hover = null;
                                InventoryManager.item_src_inventory.slots[InventoryManager.item_src].item = null;
                            }
                            else
                            {
                                inventory.slots[index].item = InventoryManager.hover;
                                InventoryManager.item_src_inventory.slots[InventoryManager.item_src].item = item;
                                InventoryManager.hover = null;
                            }

                        }

                    }
                    ImGui.PopID();
                }
                else
                {
                    inventory.RenderSlotEmpty(inventory.slots[index], drawList);
                    if (ImGui.IsItemClicked())
                    {
                        if (InventoryManager.hover == null)
                        {
                            InventoryManager.hover = item;
                            InventoryManager.item_src = index;
                            InventoryManager.item_src_inventory = inventory;
                        }
                        else if (InventoryManager.item_swap)
                        {
                            inventory.slots[index].item = InventoryManager.hover;
                            InventoryManager.item_src_inventory.slots[InventoryManager.item_src].item = item;
                            InventoryManager.hover = null;
                        }

                    }
                }


                if (ImGui.IsItemHovered() && InventoryManager.hover == null)
                {
                    ImGui.BeginTooltip();

                    List<TooltipLine> lines = item.GetTooltips();
                    lines.Insert(0, new TooltipLine(item.Name, "Name", Color.White));
                    item.ModifyTooltips(ref lines);
                    for (int i = 0; i < lines.Count; i++)
                    {
                        lines[i].Draw();
                    }

                    ImGui.EndTooltip();

                }

            }
            else
            {
                inventory.RenderSlotEmpty(inventory.slots[index], drawList);
                if (ImGui.IsItemClicked())
                {
                    if (InventoryManager.hover != null)
                    {
                        inventory.slots[index].item = InventoryManager.hover;
                        InventoryManager.item_src_inventory.slots[InventoryManager.item_src].item = null;
                        InventoryManager.hover = null;
                    }
                }
            }
        }
    }
    public class Container
    {
        public int columns, rows;
        public Slot[] slots;
        public int current_index;
        public bool open = true;
        public int slot_size = 64;
        public Container(int columns, int rows)
        {
            this.columns = columns;
            this.rows = rows;
            this.slots = new Slot[columns * rows]; for (int i = 0; i < slots.Length; i++)
            {
                slots[i] = new Slot();
            }
        }

        public virtual void RenderSlotFull(Slot item, ImDrawListPtr drawList)
        {
            Vector2 cursor = ImGui.GetCursorScreenPos();

            drawList.AddRectFilled(cursor, ImGui.GetCursorScreenPos() + new Vector2(slot_size, slot_size), col);



            //drawList.AddImage(item.item.texture, cursor + new Vector2(2, 2), cursor + new Vector2(slot_size-2, slot_size-2));
            item.item.DrawInUI();
            if (item.item.count > 1)
                drawList.AddText(cursor, 0xffffffff, item.item.count.ToString());

            //ImGui.Dummy(new Vector2(slot_size, slot_size));

        }
        public virtual void RenderSlotEmpty(Slot slot, ImDrawListPtr drawList)
        {

            drawList.AddRectFilled(ImGui.GetCursorScreenPos(), ImGui.GetCursorScreenPos() + new Vector2(slot_size, slot_size), col);

            ImGui.Dummy(new Vector2(slot_size, slot_size));
        }
        uint col = 0;
        public void Draw()
        {
            ImGui.SetWindowSize(new Vector2(slot_size * columns + 10 * (columns - 1), slot_size * rows + 10 * (rows - 1)));
            col = ImGui.GetColorU32(ImGuiCol.FrameBg);
            if (ImGui.BeginTable("##table", columns, ImGuiTableFlags.NoBordersInBody | ImGuiTableFlags.SizingFixedSame))
            {
                ImDrawListPtr drawList = ImGui.GetWindowDrawList();
                for (int y = 0; y < rows; y++)
                {
                    ImGui.TableNextRow();
                    for (int x = 0; x < columns; x++)
                    {
                        ImGui.TableNextColumn();
                        current_index = y * columns + x;
                        Slot slot = slots[current_index];

                        slot.Update(this, drawList);
                    }

                }

                ImGui.EndTable();
            }
        }
    }
}