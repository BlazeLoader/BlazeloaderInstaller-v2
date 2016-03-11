using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazeloaderInstaller {
    public class VersionNumber {
        private static string[] TYPES = new string[] { "dev", "pre", "alpha", "beta", "rc", "" };
        public static VersionNumber BLAZELOADER_VERSION_NUM = new VersionNumber(Configs.BLAZELOADER_VERSION);

        int type;
        int[] parts;
        public string Raw {
            get; private set;
        }

        public VersionNumber(string ver) {
            Raw = ver;
            string[] split = ver.Split('.');
            string type = "";
            int i = 0;
            char c;
            while (!Char.IsDigit(c = split[0][i++])) type += c;
            split[0] = split[0].Substring(i-1);
            parts = new int[split.Length];
            for (i = 0; i < parts.Length; i++) {
                Int32.TryParse(split[i], out parts[i]);
            }
            this.type = GetType(type);
        }

        private int GetType(string type) {
            return Array.IndexOf(TYPES, type.ToLower());
        }

        public bool? Compare(VersionNumber other) {
            if (other.Equals(this)) return null;
            if (type != other.type) return type > other.type;
            for (int i = 0; i < parts.Length || i < other.parts.Length; i++) {
                if (i >= parts.Length) return false;
                if (i >= other.parts.Length) return true;
                if (parts[i] != other.parts[i]) return parts[i] > other.parts[i];
            }
            return null;
        }

        public bool IsOlder() {
            return Compare(BLAZELOADER_VERSION_NUM) == false;
        }
    }
}
