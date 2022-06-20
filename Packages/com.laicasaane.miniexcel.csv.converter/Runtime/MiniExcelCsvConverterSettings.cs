using UnityEngine;

namespace MiniExcel.Csv.Converter
{
    [CreateAssetMenu(fileName = nameof(MiniExcelCsvConverterSettings), 
                     menuName = "MiniExcel/Csv Converter Settings", order = 1)]
    public class MiniExcelCsvConverterSettings : ScriptableObject
    {
        [SerializeField]
        internal string _relativeDirectoryPath;
    }
}
