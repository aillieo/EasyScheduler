// -----------------------------------------------------------------------
// <copyright file="PackageValidator.cs" company="AillieoTech">
// Copyright (c) AillieoTech. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AillieoUtils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using UnityEditor;
    using UnityEditor.Compilation;
    using UnityEditor.PackageManager;
    using UnityEditor.PackageManager.Requests;
    using UnityEngine;
    using UnityEngine.Assertions;
    using PackageInfo = UnityEditor.PackageManager.PackageInfo;

    internal static class RequestExtensions
    {
        public static void OnComplete<T>(this T request, Action<T> onComplete)
            where T : Request
        {
            void onUpdate()
            {
                if (request.IsCompleted)
                {
                    EditorApplication.update -= onUpdate;
                    onComplete?.Invoke(request);
                }
            }

            EditorApplication.update += onUpdate;
        }
    }

    [InitializeOnLoad]
    internal static class PackageValidator
    {
        // 用于防止多个package中的 PackageValidator 同时请求下载
        private static readonly string globalSessionKey = $"Global: {typeof(PackageValidator).FullName}.{nameof(CheckDependencies)}";

        private static readonly string finishSign = ":happy:coding:";

        private static readonly Regex concernedPackagePattern = new Regex(@"com\.aillieo\.[\d\w]*");

        private static readonly Queue<DependencyInfo> dependencyQueue = new Queue<DependencyInfo>();

        // 用于记录下载中的状态 防止重复下载
        private static bool busy = false;

        static PackageValidator()
        {
            // CompilationPipeline.compilationStarted += _ => CheckDependencies();
            CompilationPipeline.compilationFinished += _ => CheckDependencies();
            EditorApplication.projectChanged += CheckDependencies;
        }

        // [MenuItem("AillieoUtils/Check Dependencies")]
        private static void CheckDependencies()
        {
            PackageInfo currentPackageInfo = GetCurrentPackageInfo();

            if (currentPackageInfo == null)
            {
                Debug.LogError("Failed to get package info.");
                return;
            }

            var workingPackage = SessionState.GetString(globalSessionKey, null);

            if (workingPackage == finishSign)
            {
                return;
            }

            var className = nameof(PackageValidator);
            if (workingPackage != currentPackageInfo.name && !string.IsNullOrEmpty(workingPackage))
            {
                Debug.Log($"Stop checking dependencies. Current {className}: {currentPackageInfo.name}, working {className}: {workingPackage}.");
                return;
            }

            Debug.Log($"Begin checking dependencies. Current {className}: {currentPackageInfo.name}.");
            SessionState.SetString(globalSessionKey, currentPackageInfo.name);
            busy = true;

            ListRequest request = Client.List(true);
            request.OnComplete(OnListPackageInfo);
        }

        private static void OnListPackageInfo(ListRequest request)
        {
            if (request.Status != StatusCode.Success)
            {
                Debug.LogError("Failed to get installed packages: " + request.Error.message);
                return;
            }

            var installedPackages = request.Result;

            HashSet<string> installedPackageNames = new HashSet<string>(installedPackages.Select(r => r.name), StringComparer.OrdinalIgnoreCase);

            List<PackageInfo> concernedPackages = new List<PackageInfo>();
            List<DependencyInfo> missingDependencies = new List<DependencyInfo>();

            foreach (var package in installedPackages)
            {
                if (concernedPackagePattern.IsMatch(package.name))
                {
                    concernedPackages.Add(package);

                    foreach (var dep in package.dependencies)
                    {
                        if (!installedPackageNames.Contains(dep.name))
                        {
                            missingDependencies.Add(dep);
                        }
                    }
                }
            }

            RemoveDuplicated(missingDependencies);

            if (missingDependencies.Count <= 0)
            {
                var currentPackage = GetCurrentPackageInfo();

                Debug.Log($"No missing dependencies found. Finished by {nameof(PackageValidator)} from {currentPackage.name}.");

                Assert.AreEqual(SessionState.GetString(globalSessionKey, null), currentPackage.name);
                SessionState.SetString(globalSessionKey, finishSign);
                return;
            }

            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendLine($"Checked for packages:");
            foreach (var package in concernedPackages)
            {
                stringBuilder.AppendLine($" - {package.name}");
            }

            stringBuilder.AppendLine($"Missing dependencies found:");
            foreach (var dependency in missingDependencies)
            {
                stringBuilder.AppendLine($" - {dependency.name}");
            }

            Debug.Log(stringBuilder);

            var names = string.Join("\n", missingDependencies.Select(p => p.name));

            var ok = EditorUtility.DisplayDialog($"Missing package found", $"Missing package found\nDownload?\n{names}", "OK");
            if (ok)
            {
                BeginDownload(missingDependencies);
            }
        }

        private static PackageInfo GetCurrentPackageInfo()
        {
            return PackageInfo.FindForAssembly(typeof(PackageValidator).Assembly);
        }

        private static void BeginDownload(IEnumerable<DependencyInfo> dependencies)
        {
            foreach (var dep in dependencies)
            {
                dependencyQueue.Enqueue(dep);
            }

            CheckAndProcessNext();
        }

        private static void CheckAndProcessNext()
        {
            if (dependencyQueue.Count == 0)
            {
                busy = false;
                AssetDatabase.Refresh();
                return;
            }

            var dependency = dependencyQueue.Dequeue();
            var nameWithVersion = $"{dependency.name}@{dependency.version}";
            UnityEngine.Debug.Log($"Begin request {nameWithVersion}.");

            var request = Client.Add(nameWithVersion);
            request.OnComplete(HandleResult);
        }

        private static void HandleResult(AddRequest request)
        {
            if (request.Status == StatusCode.Success)
            {
                Debug.Log($"Dependency package downloaded successfully: {request.Result.packageId}.");
            }
            else if (request.Status == StatusCode.Failure)
            {
                Debug.LogError($"Failed to download dependency package: {request.Error.message}.");
            }

            CheckAndProcessNext();
        }

        private static void RemoveDuplicated(List<DependencyInfo> dependencies)
        {
            var dict = new Dictionary<string, DependencyInfo>();
            foreach (var dep in dependencies)
            {
                var key = $"{dep.name}@{dep.version}";
                if (!dict.ContainsKey(key))
                {
                    dict.Add(key, dep);
                }
            }

            dependencies.Clear();
            foreach (var pair in dict)
            {
                dependencies.Add(pair.Value);
            }
        }
    }
}
