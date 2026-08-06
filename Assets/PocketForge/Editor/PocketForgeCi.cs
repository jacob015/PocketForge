using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PocketForge.EditorTools
{
    /// <summary>
    /// Batch mode entry points for the build pipeline.
    /// </summary>
    public static class PocketForgeCi
    {
        // The project has no asmdefs, so every test lives in Assembly-CSharp-Editor
        // together with the editor tooling. When that assembly fails to compile the
        // editor keeps the previously built DLL and -runTests happily reports the OLD
        // tests as green, which has already hidden broken test code twice during
        // development. Failing the pipeline here makes that impossible.
        private const string EditorAssemblyName = "Assembly-CSharp-Editor";
        private const string CanaryTestType = "PocketForge.Tests.Editor.SaveCompatibilityTests";

        public static void AssertScriptsCompiled()
        {
            var failures = 0;

            if (EditorUtility.scriptCompilationFailed)
            {
                Debug.LogError("[CI] Script compilation failed; see the errors above.");
                failures++;
            }

            var editorAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(assembly => assembly.GetName().Name == EditorAssemblyName);

            if (editorAssembly == null)
            {
                Debug.LogError($"[CI] {EditorAssemblyName} is not loaded, so no test would run.");
                failures++;
            }
            else if (editorAssembly.GetType(CanaryTestType, false) == null)
            {
                Debug.LogError(
                    $"[CI] {EditorAssemblyName} loaded but {CanaryTestType} is missing. " +
                    "The editor is running a stale assembly; the test results would be meaningless.");
                failures++;
            }
            else
            {
                Debug.Log($"[CI] {EditorAssemblyName} is current ({CanaryTestType} resolved).");
            }

            if (failures > 0)
            {
                EditorApplication.Exit(1);
                return;
            }

            Debug.Log("[CI] Compilation check passed.");
            EditorApplication.Exit(0);
        }
    }
}
