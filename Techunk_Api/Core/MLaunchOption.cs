﻿using System;

namespace Techunk_Api.Core
{
    public class MLaunchOption
    {
        public MProfile StartProfile { get; set; }
        public MSession Session { get; set; }

        public string JavaPath { get; set; } = "";
        public int MaximumRamMb { get; set; } = 1024;
        public string[] JVMArguments { get; set; }

        public string DockName { get; set; }
        public string DockIcon { get; set; }

        public string ServerIp { get; set; }
        public int ServerPort { get; set; } = 25565;

        public int ScreenWidth { get; set; } = 0;
        public int ScreenHeight { get; set; } = 0;

        public string VersionType { get; set; }
        public string GameLauncherName { get; set; }
        public string GameLauncherVersion { get; set; }

        internal void CheckValid()
        {
            var exMsg = ""; // error message

            if (MaximumRamMb < 1)
                exMsg = "MaximumRamMb is too small.";

            if (StartProfile == null)
                exMsg = "StartProfile is null";

            if (Session == null)
                exMsg = "Session is null";

            if (ServerPort < 0 || ServerPort > 65535)
                exMsg = "Invalid ServerPort";

            if (ScreenWidth < 0 || ScreenHeight < 0)
                exMsg = "Screen Size must be greater than or equal to zero.";

            if (exMsg != "") // if launch option is invaild, throw exception
                throw new ArgumentException(exMsg);
        }
    }
}
