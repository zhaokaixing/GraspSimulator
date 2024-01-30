using System;
using System.Collections.Generic;

namespace TriLibEditor
{
    public static class TriLibSettings 
    {
        public static bool DisableNativePluginsChecking
        {
            get { return TriLibDefineSymbolsHelper.IsSymbolDefined("TRILIB_DISABLE_PLUGINS_CHECK"); }
            set { TriLibDefineSymbolsHelper.UpdateSymbol("TRILIB_DISABLE_PLUGINS_CHECK", value); }
        }

        public static bool DisableOldVersionsChecking
        {
            get { return TriLibDefineSymbolsHelper.IsSymbolDefined("TRILIB_DISABLE_OLD_VER_CHECK"); }
            set { TriLibDefineSymbolsHelper.UpdateSymbol("TRILIB_DISABLE_OLD_VER_CHECK", value); }
        }

        public static bool DisableEditorAutomaticImporting
        {
            get { return TriLibDefineSymbolsHelper.IsSymbolDefined("TRILIB_DISABLE_AUTO_IMPORT"); }
            set { TriLibDefineSymbolsHelper.UpdateSymbol("TRILIB_DISABLE_AUTO_IMPORT", value); }
        }

        public static bool EnableZipLoading
        {
            get { return TriLibDefineSymbolsHelper.IsSymbolDefined("TRILIB_USE_ZIP"); }
            set { TriLibDefineSymbolsHelper.UpdateSymbol("TRILIB_USE_ZIP", value); }
        }

        public static bool EnableOutputMessages
        {
            get { return TriLibDefineSymbolsHelper.IsSymbolDefined("TRILIB_OUTPUT_MESSAGES"); }
            set { TriLibDefineSymbolsHelper.UpdateSymbol("TRILIB_OUTPUT_MESSAGES", value); }
        }

        public static bool UseIOSSimulator
        {
            get { return TriLibDefineSymbolsHelper.IsSymbolDefined("TRILIB_USE_IOS_SIMULATOR"); }
            set { TriLibDefineSymbolsHelper.UpdateSymbol("TRILIB_USE_IOS_SIMULATOR", value); }
        }

        public static bool EnableIOSFileSharing
        {
            get { return TriLibDefineSymbolsHelper.IsSymbolDefined("TRILIB_ENABLE_IOS_FILE_SHARING"); }
            set { TriLibDefineSymbolsHelper.UpdateSymbol("TRILIB_ENABLE_IOS_FILE_SHARING", value); }
        }

        public static void UpdateBatchSettings()
        {
            if (!UnityEditorInternal.InternalEditorUtility.inBatchMode)
            {
                return;
            }
            DisableNativePluginsChecking = false;
            DisableOldVersionsChecking = false;
            DisableEditorAutomaticImporting = false;
            EnableZipLoading = false;
            EnableOutputMessages = false;
            UseIOSSimulator = false;
            EnableIOSFileSharing = false;
            var arguments = Environment.GetCommandLineArgs();
            foreach (var argument in arguments)
            {
                switch (argument)
                {
                    case "TRILIB_DISABLE_PLUGINS_CHECK":
                        DisableNativePluginsChecking = true;
                        break;
                    case "TRILIB_DISABLE_OLD_VER_CHECK":
                        DisableOldVersionsChecking = true;
                        break;
                    case "TRILIB_DISABLE_AUTO_IMPORT":
                        DisableEditorAutomaticImporting = true;
                        break;
                    case "TRILIB_USE_ZIP":
                        EnableZipLoading = true;
                        break;
                    case "TRILIB_OUTPUT_MESSAGES":
                        EnableOutputMessages = true;
                        break;
                    case "TRILIB_USE_IOS_SIMULATOR":
                        UseIOSSimulator = true;
                        break;
                    case "TRILIB_ENABLE_IOS_FILE_SHARING":
                        EnableIOSFileSharing = true;
                        break;
                }
            }
        }
    }
}
