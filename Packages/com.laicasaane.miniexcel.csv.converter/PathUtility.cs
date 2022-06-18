using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace MiniExcel.Csv.Converter
{
    public static class PathUtility
    {
        /// <summary>
        /// Project path at <see cref="Path"/>.<see cref="Path.Combine"/>(<see cref="Application"/>.<see cref="Application.dataPath"/>, "..")
        /// </summary>
        public static string ProjectPath
            => Path.Combine(Application.dataPath, "..").ToUnixDirectoryPath();

        public static string GetRelativePath(string relativeTo, string path)
            => Path.GetRelativePath(relativeTo, path).ToUnixDirectoryPath();

        public static string GetAbsolutePath(string relativePath)
        {
            var projectPath = Path.Combine(Application.dataPath, "..");

            return  Path.GetFullPath(string.IsNullOrEmpty(relativePath) == false
                        ? Path.Combine(projectPath, relativePath)
                        : projectPath
                    ).ToUnixDirectoryPath();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToUnixDirectoryPath(this string path)
            => path.Replace(Path.DirectorySeparatorChar, '/');
    }
}