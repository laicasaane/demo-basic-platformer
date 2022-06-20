using System;
using System.Collections.Generic;
using UnityEngine;

namespace MiniExcel.Csv.Converter
{
    [CreateAssetMenu(fileName = nameof(MiniExcelCsvConverterSettings), 
                     menuName = "MiniExcel/Csv Converter Settings", order = 1)]
    public class MiniExcelCsvConverterSettings : ScriptableObject
    {
        [SerializeField]
        internal string _relativeDirectoryPath;

        [SerializeField]
        internal ExcelFileList _files;

        [Serializable]
        internal struct ExcelFile
        {
            public string path;
            public bool selected;
        }

        [Serializable]
        internal class ExcelFileList
            : Dictionary<string, bool>
            , ISerializationCallbackReceiver
        {
            [SerializeField]
            private ExcelFile[] _items;

            public void OnAfterDeserialize()
            {
                Clear();

                foreach (var pair in _items)
                {
                    Add(pair.path, pair.selected);
                }
            }

            public void OnBeforeSerialize()
            {
                _items = new ExcelFile[this.Count];
                var i = 0;

                foreach (var kv in this)
                {
                    _items[i] = new ExcelFile { path = kv.Key, selected = kv.Value };
                }
            }
        }
    }
}
