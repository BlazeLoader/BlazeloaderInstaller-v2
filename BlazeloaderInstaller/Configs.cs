using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazeloaderInstaller {
    class Configs {
        public const string MINECRAFT_VERSION = "1.8.9";
        public const string MAIN_CLASS = "net.minecraft.launchwrapper.Launch";

        public const string LITELOADER_VERSION = "1.8.9";
        public const string LITELOADER_LIB = "com.mumfrey:liteloader:" + LITELOADER_VERSION;
        public const string LITELOADER_URL = "http://dl.liteloader.com/versions";
        public const string LITELOADER_TWEAK = "--tweakClass com.mumfrey.liteloader.launch.LiteLoaderTweaker";

        public const string BLAZELOADER_VERSION = "beta1.0";
        public const string BLAZELOADER_LIB = "com.blazeloader:blazeloader:" + BLAZELOADER_VERSION;
        public const string BLAZELOADER_URL = "http://dl.blazeloader.com/versions";
        public const string BLAZELOADER_TWEAK = "--api com.blazeloader.bl.main.BlazeLoaderAPI";

        public const string LAUNCH_WRAPPER = "net.minecraft:launchwrapper:1.11";
        public const string ASM = "org.ow2.asm:asm-all:5.0.3";
        
        public const string LAUNCHER_ARGS = "-Xmx1G -XX:+UseConcMarkSweepGC -XX:+CMSIncrementalMode -XX:-UseAdaptiveSizePolicy -Xmn128M";

        public const string LIBRARIES =
@"{
    name: """ + BLAZELOADER_LIB + @""",
    url: """ + BLAZELOADER_URL + @"""
}, {
    name: """ + LITELOADER_LIB + @""",
    url: """ + LITELOADER_URL + @"""
}, {
    name: """ + LAUNCH_WRAPPER + @"""
}, {
    name: """ + ASM + @"""
}";
    }
}
