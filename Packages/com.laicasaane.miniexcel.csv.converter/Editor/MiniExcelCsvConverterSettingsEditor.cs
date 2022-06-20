using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using MiniExcel.Controls;
using MiniExcel.Controls.Editor;
using System.IO;

namespace MiniExcel.Csv.Converter.Editor
{
    [CustomEditor(typeof(MiniExcelCsvConverterSettings))]
    public class MiniExcelCsvConverterSettingsEditor : UnityEditor.Editor
    {
        [SerializeField]
        private VisualTreeAsset _visualTree;

        [SerializeField]
        private ThemeStyleSheet _darkTheme;

        [SerializeField]
        private ThemeStyleSheet _lightTheme;

        private MiniExcelCsvConverterSettings _settings;

        public override VisualElement CreateInspectorGUI()
        {
            _settings = target as MiniExcelCsvConverterSettings;

            var root = new VisualElement();
            _visualTree.CloneTree(root);

            root.styleSheets.Add(EditorGUIUtility.isProSkin ? _darkTheme : _lightTheme);
            
            var folderBrowser = root.Q<FolderBrowser>(className: FolderBrowser.ussClassName);

            if (folderBrowser != null)
            {
                folderBrowser.openFolderHandler = new EditorOpenFolderHandler();
                folderBrowser.bindingPath = nameof(MiniExcelCsvConverterSettings._relativeDirectoryPath);
                folderBrowser.onValueChanged += FolderBrowser_onValueChanged;
            }

            root.Bind(serializedObject);
            return root;
        }

        private void FolderBrowser_onValueChanged(string absolutePath, string relativePath)
        {
            if (_settings is null)
                return;

            _settings._relativeDirectoryPath = relativePath;

            var files = _settings._files;

            foreach (var filePath in Directory.EnumerateFiles(absolutePath, "*.xlsx"))
            {
                var fileName = Path.GetFileName(filePath);
                
                if (files.ContainsKey(fileName) == false)
                    files.Add(fileName, false);
            }

            EditorUtility.SetDirty(_settings);
        }
    }
}
