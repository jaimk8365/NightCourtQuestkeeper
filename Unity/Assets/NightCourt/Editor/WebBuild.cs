using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace NightCourt.Editor
{
    public static class WebBuild
    {
        private const string ScenePath = "Assets/NightCourt/Scenes/FaeCottage.unity";
        private const string OutputPath = "Builds/WebGL";

        public static void Build()
        {
            if (!File.Exists(Path.Combine(Directory.GetCurrentDirectory(), ScenePath)))
            {
                throw new FileNotFoundException("The Fae Cottage scene must be generated before building.", ScenePath);
            }

            PlayerSettings.productName = "Night Court Questkeeper";
            PlayerSettings.companyName = "Jaimi Kyte";
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
            PlayerSettings.WebGL.dataCaching = true;

            Directory.CreateDirectory(OutputPath);
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = OutputPath,
                target = BuildTarget.WebGL,
                options = BuildOptions.CleanBuildCache
            });

            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Web build failed: {report.summary.result} ({report.summary.totalErrors} errors)."
                );
            }

            Debug.Log($"Web build succeeded: {report.summary.outputPath}");
        }
    }
}
