using UnityEngine;

namespace MiniExcel.Csv.Converter
{
    [CreateAssetMenu(fileName = nameof(MiniExcelCsvConverterSettings), menuName = "MiniExcel/Csv Converter Settings", order = 1)]
    public class MiniExcelCsvConverterSettings : ScriptableObject
    {
        [SerializeField]
        internal string _relativeDirectoryPath;

        /// <summary>
        /// Directory path that is relative to <see cref="PathUtility.ProjectPath"/>
        /// </summary>
        public string RelativeDirectoryPath
        {
            get => _relativeDirectoryPath;

            set => _relativeDirectoryPath = string.IsNullOrEmpty(value) == false 
                ? PathUtility.GetRelativePath(PathUtility.ProjectPath, value) 
                : string.Empty;
        }
    }
}
