using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using MiniExcelLibs.Controls;
using MiniExcelLibs.Controls.Editor;
using MiniExcelLibs.Utilities;

namespace MiniExcelLibs.Csv.Converter.Editor
{
    [CustomEditor(typeof(MiniExcelCsvConverterSettings))]
    public class MiniExcelCsvConverterSettingsEditor : UnityEditor.Editor
    {
        public static readonly string folderBrowserExcelUssName = "folder-browser-excel";
        public static readonly string folderBrowserCsvUssName = "folder-browser-csv";
        public static readonly string fileListUssName = "file-list";
        public static readonly string fileItemUssClassName = "file-list__item";
        public static readonly string refreshButtonUssName = "refresh-button";
        public static readonly string convertButtonUssName = "convert-button";

        [SerializeField]
        private VisualTreeAsset _visualTree;

        [SerializeField]
        private ThemeStyleSheet _darkTheme;

        [SerializeField]
        private ThemeStyleSheet _lightTheme;

        private readonly Dictionary<string, bool> _fileMapPrev = new();
        private readonly Dictionary<string, bool> _fileMapNew = new();

        private MiniExcelCsvConverterSettings _settings;
        private ListView _excelFileList;

        public override VisualElement CreateInspectorGUI()
        {
            _settings = target as MiniExcelCsvConverterSettings;

            var root = new VisualElement();
            _visualTree.CloneTree(root);

            root.styleSheets.Add(EditorGUIUtility.isProSkin ? _darkTheme : _lightTheme);

            /// FOLDER BROWSER --- EXCEL
            {
                var folderBrowserExcel = root.Q<FolderBrowser>(folderBrowserExcelUssName);

                if (folderBrowserExcel != null)
                {
                    folderBrowserExcel.openFolderHandler = new EditorOpenFolderHandler();
                    folderBrowserExcel.bindingPath = nameof(MiniExcelCsvConverterSettings._relativeExcelFolderPath);
                    folderBrowserExcel.onValueChanged += FolderBrowserExcel_onValueChanged;
                }
            }

            /// FOLDER BROWSER --- CSV
            {
                var folderBrowserCsv = root.Q<FolderBrowser>(folderBrowserCsvUssName);

                if (folderBrowserCsv != null)
                {
                    folderBrowserCsv.openFolderHandler = new EditorOpenFolderHandler();
                    folderBrowserCsv.bindingPath = nameof(MiniExcelCsvConverterSettings._relativeCsvFolderPath);
                    folderBrowserCsv.onValueChanged += FolderBrowserCsv_onValueChanged;
                }
            }

            /// REFRESH EXCEL FILES BUTTON
            {
                var refreshButton = root.Q<Button>(refreshButtonUssName);

                if (refreshButton != null)
                    refreshButton.clicked += RefreshButton_clicked;
            }

            /// CONVERT TO CSV
            {
                var convertButton = root.Q<Button>(convertButtonUssName);

                if (convertButton != null)
                    convertButton.clicked += ConvertButton_clicked;
            }

            /// FILE LIST
            {
                _excelFileList = root.Q<ListView>(fileListUssName);

                if (_excelFileList != null)
                {
                    _excelFileList.bindingPath = nameof(MiniExcelCsvConverterSettings._excelFiles);
                    _excelFileList.makeItem = ExcelFileList_MakeItem;
                    _excelFileList.bindItem = ExcelFileList_BindItem;
                }
            }

            root.Bind(serializedObject);
            return root;
        }

        private void FolderBrowserExcel_onValueChanged(string absolutePath, string relativePath)
        {
            if (_settings is null)
                return;

            _settings._relativeExcelFolderPath = relativePath;

            RefreshFileList(absolutePath);

            EditorUtility.SetDirty(_settings);
            AssetDatabase.SaveAssetIfDirty(target);
        }

        private void FolderBrowserCsv_onValueChanged(string absolutePath, string relativePath)
        {
            if (_settings is null)
                return;

            _settings._relativeCsvFolderPath = relativePath;

            EditorUtility.SetDirty(_settings);
            AssetDatabase.SaveAssetIfDirty(target);
        }

        private void RefreshFileList(string absolutePath)
        {
            _fileMapPrev.Clear();
            _fileMapNew.Clear();
            _settings.CopyFilesToMap(_fileMapPrev);

            foreach (var filePath in Directory.EnumerateFiles(absolutePath, "*.xlsx"))
            {
                var fileName = Path.GetFileName(filePath);

                if (_fileMapPrev.TryGetValue(fileName, out var selected) == false)
                    selected = false;

                _fileMapNew[fileName] = selected;
            }

            _settings.ApplyMapToFiles(_fileMapNew);
        }

        private VisualElement ExcelFileList_MakeItem()
        {
            var fileItem = new BindableElement();
            fileItem.AddToClassList(fileItemUssClassName);
            fileItem.Add(new Toggle { bindingPath = nameof(FileData.selected) });
            fileItem.Add(new Label { bindingPath = nameof(FileData.path) });

            return fileItem;
        }

        private void ExcelFileList_BindItem(VisualElement ve, int index)
        {
            if (!(ve is BindableElement fileItem) 
                || _settings is null 
                || _excelFileList is null
                || _excelFileList.itemsSource is null)
                return;

            var files = _settings._excelFiles;
            var itemsSource = _excelFileList.itemsSource;

            if ((uint)index >= (uint)files.Count
                || (uint)index >= (uint)itemsSource.Count)
                return;

            var fileProp = (SerializedProperty)itemsSource[index];
            fileItem.bindingPath = fileProp.propertyPath;
            fileItem.Bind(fileProp.serializedObject);
        }

        private void RefreshButton_clicked()
        {
            if (_settings is null)
                return;

            RefreshFileList(PathUtility.GetAbsolutePath(_settings._relativeExcelFolderPath));

            EditorUtility.SetDirty(_settings);
            AssetDatabase.SaveAssetIfDirty(target);
        }

        private void ConvertButton_clicked()
        {
            //var path = Path.Combine(Application.dataPath, "..", "Resources/MasterData.xlsx");
            //using var stream = File.OpenRead(path);
            //var sheetNames = new List<string>();
            //MiniExcel.GetSheetNames(stream, sheetNames, x => x.StartsWith("<WIP>") == false);

            //foreach (var sheetName in sheetNames)
            //{
            //    Debug.Log(sheetName);

            //    var csvPath = $"{Application.dataPath}/CSV/{sheetName}";

            //    if (Directory.Exists(csvPath) == false)
            //        Directory.CreateDirectory(csvPath);

            //    using (var csvStream = new FileStream(csvPath, FileMode.CreateNew))
            //    {
            //        var rows = await MiniExcel.QueryAsync(path, false, sheetName, ExcelType.XLSX);
            //        await MiniExcel.SaveAsAsync(csvStream, rows, printHeader: false, excelType: ExcelType.CSV);
            //    }
            //}
        }
    }
}
