using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FSEngine.IO
{
    public class CSV
    {
        public int rows, columns;
        public string[,] data;

        public CSV(int rows, int columns)
        {
            this.rows = rows;
            this.columns = columns;
            this.data = new string[rows, columns];
        }


    }
    public static class CSVLoader
    {
        public static CSV Load(string file)
        {
            Regex value = new Regex(@",\s+");
            string[] lines = File.ReadAllText(file).Split('\n').Where(_x => !_x.StartsWith("#") && !string.IsNullOrEmpty(_x) && !string.IsNullOrWhiteSpace(_x)).ToArray();
            int col = value.Split(lines[0]).Length;
            CSV csv = new CSV(lines.Length, col);

            int x = 0;
            foreach(string s in lines)
            {
                string[] values = value.Split(s);
                for (int i = 0; i < col; i++)
                {
                    csv.data[x, i] = values[i];
                }
                x++;
            }
            return csv;
        }
    }
}
