﻿using System;

namespace XapDeployer
{
    public class Localization
    {
        public Localization()
        {
        }
        private static LocalizedResources res = new LocalizedResources();

        public static LocalizedResources Resources { get { return res; } }

    }
}
