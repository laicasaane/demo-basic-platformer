using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace MiniExcel.Csv.Converter.Editor
{
    [CustomEditor(typeof(MiniExcelCsvConverterSettings))]
    public class MiniExcelCsvConverterSettingsEditor : UnityEditor.Editor
    {
        [SerializeField]
        private VisualTreeAsset _visualTree;

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement {
                name = "miniexcel-csv-converter-settings"
            };

            _visualTree.CloneTree(root);

            DirectoryGroup(root);

            root.Bind(serializedObject);
            return root;
        }

        private void DirectoryGroup(VisualElement root)
        {
            const string DIRECTORY_GROUP = "directory-group";

            TextField dirText = root.Query(DIRECTORY_GROUP).Children<TextField>().First();
            Button dirSelectButton = root.Query(DIRECTORY_GROUP).Children<Button>().First();

            if (dirText != null)
            {
                dirText.bindingPath = nameof(MiniExcelCsvConverterSettings._relativeDirectoryPath);
                dirText.RegisterValueChangedCallback(x => {
                    DirectoryInfo(root, x.newValue);
                });
            }

            if (dirSelectButton != null)
                dirSelectButton.clicked += DirSelectButton_clicked;
        }

        private void DirSelectButton_clicked()
        {
            if (!(target is MiniExcelCsvConverterSettings settings))
                return;

            const string SELECT_DIRECTORY = "Select Directory";

            var path = PathUtility.GetAbsolutePath(settings.RelativeDirectoryPath);
            path = EditorUtility.OpenFolderPanel(SELECT_DIRECTORY, path, "");

            Undo.RecordObject(settings, SELECT_DIRECTORY);

            settings.RelativeDirectoryPath = path;

            EditorUtility.SetDirty(settings);
        }

        private void DirectoryInfo(VisualElement root, string value)
        {
            const string DIRECTORY_INFO = "directory-info";

            Label label = root.Query(DIRECTORY_INFO).Children<Label>().First();

            if (label != null)
                label.text = PathUtility.GetAbsolutePath(value);
        }
    }
}
