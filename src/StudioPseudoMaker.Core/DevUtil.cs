using System;
using System.Linq;
using System.Reflection;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;

namespace PseudoMaker
{
    public static class DevelopmentUtil
    {
        public static void TestKKAPIReflection(ManualLogSource Logger)
        {
            
        }
        
        public static void GetHarmonyPatches(Type declaringType, string methodName, ManualLogSource Logger)
        {
            // Blyat my smooth brain cannot handle this
            if (declaringType == null)
            {
                Logger.LogError("Error: declaringType cannot be null.");
                return;
            }

            if (string.IsNullOrEmpty(methodName))
            {
                Logger.LogError("Error: methodName cannot be null or empty.");
                return;
            }

            try
            {
                int patchesFound = 0;

                if (Chainloader.PluginInfos == null)
                {
                    Logger.LogError("Error: Chainloader.PluginInfos is null.");
                    return;
                }

                foreach (var plugin in Chainloader.PluginInfos)
                {
                    if (plugin.Value == null || plugin.Value.Instance == null)
                    {
                        continue;
                    }

                    try
                    {
                        var assembly = plugin.Value.Instance.GetType()?.Assembly;
                        if (assembly == null) continue;

                        Type[] types;
                        try
                        {
                            types = assembly.GetTypes();
                        }
                        catch (ReflectionTypeLoadException ex)
                        {
                            types = ex.Types?.Where(t => t != null).ToArray() ?? Type.EmptyTypes;
                            Logger.LogWarning(
                                $"Some types could not be loaded from {assembly.GetName().Name}: {ex.Message}");
                        }
                        catch (Exception ex)
                        {
                            Logger.LogWarning($"Could not get types from {assembly.GetName().Name}: {ex.Message}");
                            continue;
                        }

                        if (types == null) continue;

                        foreach (var type in types)
                        {
                            if (type == null) continue;

                            MethodInfo[] methods;
                            try
                            {
                                methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic |
                                                          BindingFlags.Static | BindingFlags.Instance);
                            }
                            catch (Exception ex)
                            {
                                Logger.LogWarning($"Could not get methods for type {type.FullName}: {ex.Message}");
                                continue;
                            }

                            if (methods == null) continue;

                            foreach (var methodInfo in methods)
                            {
                                if (methodInfo == null) continue;

                                try
                                {
                                    object[] attributes;
                                    try
                                    {
                                        attributes = methodInfo.GetCustomAttributes(typeof(HarmonyPatch), false);
                                    }
                                    catch (Exception ex)
                                    {
                                        continue;
                                    }

                                    if (attributes == null || attributes.Length == 0)
                                        continue;

                                    foreach (var attrObj in attributes)
                                    {
                                        if (attrObj == null) continue;

                                        HarmonyPatch attr = attrObj as HarmonyPatch;
                                        if (attr == null) continue;

                                        Type attrDeclaringType = attr.info.declaringType;
                                        string attrMethodName = attr.info.methodName;

                                        if (attrDeclaringType == null || attrMethodName == null)
                                        {
                                            continue;
                                        }

                                        if (attrDeclaringType == declaringType &&
                                            string.Equals(attrMethodName, methodName, StringComparison.Ordinal))
                                        {
                                            Logger.LogInfo(
                                                $"Add Patch: {methodInfo.Name} from {assembly.GetName().Name}");
                                            patchesFound++;
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Logger.LogWarning($"Error processing method {methodInfo.Name}: {ex.Message}");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogWarning($"Error processing plugin {plugin.Key}: {ex.Message}");
                    }
                }

                Logger.LogInfo($"Done. Found {patchesFound} patches.");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Fatal error in GetHarmonyPatches: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}