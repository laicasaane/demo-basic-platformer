using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using MiniExcelLibs.Controls;
using MiniExcelLibs.Controls.Editor;
using MiniExcelLibs.Utilities;
using System.Linq;

namespace MiniExcelLibs.Csv.Converter.Editor
{
    [CustomEditor(typeof(MiniExcelCsvConverterSettings))]
    public class MiniExcelCsvConverterSettingsEditor : UnityEditor.Editor
    {
        public static readonly string folderBrowserExcelUssName = "folder-browser-excel";
        public static readonly string folderBrowserCsvUssName = "folder-browser-csv";
        public static readonly string ignoreSheetsUssName = "ignore-sheets";
        public static readonly string fileListUssName = "file-list";
        public static readonly string fileItemUssClassName = "file-list__item";
        public static readonly string openButtonUssName = "open-button";
        public static readonly string locateButtonUssName = "locate-button";
        public static readonly string refreshButtonUssName = "refresh-button";
        public static readonly string convertButtonUssName = "convert-button";

        private const string EXCEL_EXT = ".xlsx";
        private const string CSV_EXT = ".csv";
        private const string EXCEL_FILTER = "*.xlsx";
        private const string PROGRESS_TITLE = "Convert Excel to CSV";

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
        private bool _isConverting;

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

            /// IGNORE SHEETS START WITH CHARACTERS
            {
                var ignoreSheets = root.Q<TextField>(ignoreSheetsUssName);

                if (ignoreSheets != null)
                {
                    ignoreSheets.bindingPath = nameof(MiniExcelCsvConverterSettings._ignoreSheetsStartWith);
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

            foreach (var filePath in Directory.EnumerateFiles(absolutePath, EXCEL_FILTER))
            {
                var fileName = Path.GetFileNameWithoutExtension(filePath);

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

            var openButton = new Button { text = "Open" };
            openButton.name = openButtonUssName;

            var locateButton = new Button { text = "Locate" };
            locateButton.name = locateButtonUssName;

            fileItem.Add(openButton);
            fileItem.Add(locateButton);

            return fileItem;
        }

        private void ExcelFileList_BindItem(VisualElement ve, int index)
        {
            if (!(ve is BindableElement fileItem) 
                || _settings is null 
                || _excelFileList is null
                || _excelFileList.itemsSource is null)
                return;

            var itemsSource = _excelFileList.itemsSource;

            if ((uint)index >= (uint)itemsSource.Count)
                return;

            var fileProp = (SerializedProperty)itemsSource[index];
            fileItem.bindingPath = fileProp.propertyPath;
            fileItem.Bind(fileProp.serializedObject);

            var openButton = fileItem.Q<Button>(openButtonUssName);

            if (openButton != null)
                openButton.clicked += () => OpenFileAt(index);

            var locateButton = fileItem.Q<Button>(locateButtonUssName);

            if (locateButton != null)
                locateButton.clicked += () => LocateFileAt(index);
        }

        private void RefreshButton_clicked()
        {
            if (_settings is null)
                return;

            RefreshFileList(PathUtility.GetAbsolutePath(_settings._relativeExcelFolderPath));

            EditorUtility.SetDirty(_settings);
            AssetDatabase.SaveAssetIfDirty(target);
        }

        private bool TryGetFilePathAt(int index, out string filePath)
        {
            var files = _settings._excelFiles;

            if ((uint)index >= (uint)files.Count)
            {
                filePath = string.Empty;
                return false;
            }

            var file = files[index];
            var folderPath = PathUtility.GetAbsolutePath(_settings._relativeExcelFolderPath);
            filePath = Path.Combine(folderPath, $"{file.path}{EXCEL_EXT}").ToPlatformPath();
            return true;
        }

        private void OpenFileAt(int index)
        {
            if (TryGetFilePathAt(index, out var filePath))
                System.Diagnostics.Process.Start(filePath);
        }

        private void LocateFileAt(int index)
        {
            if (TryGetFilePathAt(index, out var filePath))
            {
                Debug.Log(filePath);
                var argument = $"/e,/select,\"{filePath}\"";
                System.Diagnostics.Process.Start("explorer.exe", argument);
            }
        }

        private void ConvertButton_clicked()
        {
            if (_isConverting)
                return;

            _isConverting = true;

            ConvertExcelToCsv();

            _isConverting = false;
        }

        private void ConvertExcelToCsv()
        {
            var relativeCsvFolderPath = _settings._relativeCsvFolderPath;
            var absoluteCsvFolderPath = PathUtility.GetAbsolutePath(relativeCsvFolderPath);
            var absoluteExcelFolderPath = PathUtility.GetAbsolutePath(_settings._relativeExcelFolderPath);
            var ignoreSheetsStartWith = _settings._ignoreSheetsStartWith;
            var files = _settings._excelFiles;
            var filesToConvert = new List<string>();

            foreach (var file in files)
            {
                if (file.selected)
                    filesToConvert.Add(file.path);
            }

            // Delete Folder + Create Folder + File Count
            var steps = 2 + filesToConvert.Count;
            var step = 1f / steps;

            for (var i = 0; i < steps; i++)
            {
                if (i == 0)
                {
                    DeleteCsvFolder(relativeCsvFolderPath, absoluteCsvFolderPath, step * i);
                    continue;
                }

                if (i == 1)
                {
                    CreateCsvFolder(relativeCsvFolderPath, absoluteCsvFolderPath, step * i);
                    continue;
                }

                var index = i - 2;
                var filePath = filesToConvert[index];
                ConvertCsv(
                    absoluteExcelFolderPath
                    , absoluteCsvFolderPath
                    , filePath
                    , ignoreSheetsStartWith
                    , step * i
                    , step
                );
            }
        }

        private static void DeleteCsvFolder(string relative, string absolute, float progress)
        {
            EditorUtility.DisplayProgressBar(
                PROGRESS_TITLE
                , $"Delete directory: {relative}"
                , progress
            );

            if (Directory.Exists(absolute))
            {
                Directory.Delete(absolute, true);
            }

            EditorUtility.ClearProgressBar();
        }

        private static void CreateCsvFolder(string relative, string absolute, float progress)
        {
            EditorUtility.DisplayProgressBar(
                PROGRESS_TITLE
                , $"Create directory: {relative}"
                , progress
            );

            if (Directory.Exists(absolute) == false)
            {
                Directory.CreateDirectory(absolute);
            }

            EditorUtility.ClearProgressBar();
        }

        private static void ConvertCsv(
            string excelFolderPath
            , string csvFolderPath
            , string filePath
            , string ignoreSheetsStartWith
            , float progress
            , float step
        )
        {
            var excelFilePath = Path.Combine(excelFolderPath, $"{filePath}{EXCEL_EXT}").ToPlatformPath();

            if (File.Exists(excelFilePath) == false)
                return;

            using var stream = File.OpenRead(excelFilePath);

            var sheetNames = new List<string>();
            MiniExcel.GetSheetNames(stream, sheetNames, x => x.StartsWith(ignoreSheetsStartWith) == false);

            if (sheetNames.Count <= 0)
                return;

            var miniStep = step / sheetNames.Count;
            var sheetCsvFolderPath = Path.Combine(csvFolderPath, filePath).ToPlatformPath();

            if (Directory.Exists(sheetCsvFolderPath) == false)
                Directory.CreateDirectory(sheetCsvFolderPath);

            for (var i = 0; i < sheetNames.Count; i++)
            {
                var sheetName = sheetNames[i];

                EditorUtility.DisplayProgressBar(
                    PROGRESS_TITLE
                    , $"Convert: {filePath} : {sheetName}"
                    , progress + (miniStep * i)
                );

                var sheetFilePath = Path.Combine(sheetCsvFolderPath, $"{sheetName}{CSV_EXT}");

                using (var csvStream = new FileStream(sheetFilePath, FileMode.CreateNew))
                {
                    var rows = MiniExcel.Query(excelFilePath, false, sheetName, ExcelType.XLSX)
                        .Where(x => x.Keys.Any(k => x[k] != null));

                    MiniExcel.SaveAs(csvStream, rows, printHeader: false, excelType: ExcelType.CSV);
                }

                EditorUtility.ClearProgressBar();
            }
        }
    }
}
