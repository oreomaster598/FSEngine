using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FSEngine
{
    public static class Language
    {
        private static Dictionary<string, string> localized = new Dictionary<string, string>();

        public static char[] elder_futhark = "ᚢᚦᚨᚲᚷᚹᚺᚾᛁᛃᛈᛇᛊᛉᛏᛒᛖᛗᛚᛜᛞᛟᚠᚱ".ToCharArray();
        public static string[] convert = {  "([uU]|[o]{2})", "[Tt][Hh]", "[Aa][Ww]{0,1}",
            "([Kk]|[Cc])", "([Gg]|[Cc][Hh])", "([Ww]|[Vv])", "[Hh]", "[Nn]", "[Ii](?=[Tt])", "([Jj]|[Yy])",
            "([Ii]|[Yy])","[Pp]", "([Ss]|([Cc](?=e)))", "([Zz]|[Ss])", "[Tt]", "[Bb]", "[Ee]", "[Mm]", "[Ll]",
            "[Nn][Gg]", "[Dd]", "[Oo]", "[Ff]", "[Rr]", };
        public static string[] map = { "[Xx]", "ᚲᛋ", "[Qq]", "ᚲ" };
        public static string ToRunes(this string s)
        {
            for(int i = 0; i != convert.Length; i++)
            {
                s = new Regex(convert[i]).Replace(s, elder_futhark[i].ToString());   
            }
            for(int i = 0; i!= map.Length;)
            {
                Regex rx = new Regex(map[i]);
                i++;
                s = rx.Replace(s, map[i]);
                i++;
            }
            return s;
        }
        public static void Reset()
        {
            localized.Clear();
        }
        public static void AddLocalization(string type, string LocalizedName)
        {
            localized.Add(type, LocalizedName);
        }
        public static string Localize(string s)
        {
            if(localized.TryGetValue(s, out string o))
                return o;
           return s;
        }
    }
}
