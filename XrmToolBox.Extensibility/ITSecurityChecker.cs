﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;

namespace XrmToolBox.Extensibility
{
    public class ItSecurityChecker
    {
        private List<string> plugins;

        public bool HasPluginsRestriction => plugins?.Count > 0 && plugins.All(p => p != "*");

        public bool IsCheckForUpdateDisabled()
        {
            return ReturnBooleanKeyValue("IsCheckForUpdateDisabled");
        }

        public bool IsDisabled()
        {
            return ReturnBooleanKeyValue("IsDisabled");
        }

        public bool IsPluginAllowed(string plugin)
        {
            if (plugins == null) return true;

            if (plugins.Any(p => p == "*")) return true;

            if (plugins.Any(p => plugin?.ToLower().IndexOf(p.ToLower(), StringComparison.Ordinal) >= 0)) return true;

            return false;
        }

        public bool IsStatisticsCollectDisabled()
        {
            return ReturnBooleanKeyValue("IsStatisticsCollectDisabled");
        }

        public void LoadAllowedPlugins()
        {
            using (RegistryKey hklm = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default))
            {
                using (RegistryKey registryKey = hklm.OpenSubKey(@"SOFTWARE\MscrmTools\XrmToolBox"))
                {
                    if (registryKey != null)
                    {
                        if (registryKey.GetValueNames().Contains("AllowedPlugins"))
                        {
                            plugins = new List<string>((string[])registryKey.GetValue("AllowedPlugins"));
                        }
                    }
                }
            }

            if (plugins == null)
            {
                using (RegistryKey hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default))
                {
                    using (RegistryKey registryKey = hklm.OpenSubKey(@"SOFTWARE\MscrmTools\XrmToolBox"))
                    {
                        if (registryKey != null)
                        {
                            if (registryKey.GetValueNames().Contains("AllowedPlugins"))
                            {
                                plugins = new List<string>((string[])registryKey.GetValue("AllowedPlugins"));
                            }
                        }
                    }
                }
            }
        }

        private bool ReturnBooleanKeyValue(string key)
        {
            var isDisabled = false;

            using (RegistryKey hklm = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default))
            {
                using (RegistryKey registryKey = hklm.OpenSubKey(@"SOFTWARE\MscrmTools\XrmToolBox"))
                {
                    if (registryKey != null)
                    {
                        if (registryKey.GetValueNames().Contains(key))
                        {
                            isDisabled = (int)registryKey.GetValue(key) == 1;
                        }
                    }
                }
            }

            if (isDisabled == false)
            {
                using (RegistryKey hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default))
                {
                    using (RegistryKey registryKey = hklm.OpenSubKey(@"SOFTWARE\MscrmTools\XrmToolBox"))
                    {
                        if (registryKey != null)
                        {
                            if (registryKey.GetValueNames().Contains(key))
                            {
                                isDisabled = (int)registryKey.GetValue(key) == 1;
                            }
                        }
                    }
                }
            }

            return isDisabled;
        }
    }
}