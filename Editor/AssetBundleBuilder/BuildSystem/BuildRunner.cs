using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace YooAsset.Editor
{
    public class BuildRunner
    {
        private static Stopwatch _buildWatch;

        /// <summary>
        ///     总耗时
        /// </summary>
        public static int TotalSeconds;

        /// <summary>
        ///     执行构建流程
        /// </summary>
        /// <returns>如果成功返回TRUE，否则返回FALSE</returns>
        public static BuildResult Run(List<IBuildTask> pipeline, BuildContext context)
        {
            if (pipeline == null)
                throw new ArgumentNullException("pipeline");
            if (context == null)
                throw new ArgumentNullException("context");

            var buildResult = new BuildResult();
            buildResult.Success = true;
            TotalSeconds = 0;
            for (var i = 0; i < pipeline.Count; i++)
            {
                var task = pipeline[i];
                try
                {
                    _buildWatch = Stopwatch.StartNew();
                    var taskName = task.GetType().Name.Split('_')[0];
                    BuildLogger.Log(
                        $"--------------------------------------------->{taskName}<--------------------------------------------");
                    task.Run(context);
                    _buildWatch.Stop();

                    // 统计耗时
                    var seconds = GetBuildSeconds();
                    TotalSeconds += seconds;
                    BuildLogger.Log($"{taskName} It takes {seconds} seconds in total");
                }
                catch (Exception e)
                {
                    EditorTools.ClearProgressBar();
                    buildResult.FailedTask = task.GetType().Name;
                    buildResult.ErrorInfo = e.ToString();
                    buildResult.Success = false;
                    break;
                }
            }

            // 返回运行结果
            BuildLogger.Log($"Total build process time: {TotalSeconds} seconds");
            return buildResult;
        }

        private static int GetBuildSeconds()
        {
            var seconds = _buildWatch.ElapsedMilliseconds / 1000f;
            return (int)seconds;
        }
    }
}