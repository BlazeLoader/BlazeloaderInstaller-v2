using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazeloaderInstaller {
    class Configs {
        public const string MINECRAFT_VERSION = "1.8";
        
        public const string LITELOADER_VERSION = "1.8";
        public const string LITELOADER_URL = "http://dl.liteloader.com/versions";

        public const string BLAZELOADER_VERSION = "1.2";
        public const string BLAZELOADER_URL = "http://dl.blazeloader.com/versions";

        public const string LAUNCH_WRAPPER = "1.11";
        public const string ASM = "5.0.3";

        public const string LITELOADER_TWEAK = "--tweakClass com.mumfrey.liteloader.launch.LiteLoaderTweaker";
        public const string BLAZELOADER_TWEAK = "--api com.blazeloader.bl.main.BlazeLoaderAPI";

        public const string LAUNCHER_ARGS = "-Xmx1G -XX:+UseConcMarkSweepGC -XX:+CMSIncrementalMode -XX:-UseAdaptiveSizePolicy -Xmn128M";
    }
}
