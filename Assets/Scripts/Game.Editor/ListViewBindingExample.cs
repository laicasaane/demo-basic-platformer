using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.Editor
{
    public class ListViewBindingExample : EditorWindow
    {
        [MenuItem("Window/UI Toolkit/ListView Binding Example")]
        public static void ShowExample()
        {
            var window = GetWindow<ListViewBindingExample>();
            window.titleContent = new GUIContent("ListView Binding Example");
            window.Show();
        }

        [Serializable]
        public struct Item
        {
            public string stringValue;
            public float floatValue;

            public Item(string strValue, float fValue)
            {
                stringValue = strValue;
                floatValue = fValue;
            }
        }

        [SerializeField]
        private List<Item> _itemList;

        private ListView _itemListView;

        private SerializedObject _serializedObject;
        private SerializedProperty _arrayProperty;
        private SerializedProperty _arraySizeProperty;

        public void CreateGUI()
        {
            // Initialize the item list data that will be bound to the UI.
            if (_itemList == null)
            {
                _itemList = new List<Item>();
                for (var i = 0; i < 3; ++i)
                {
                    _itemList.Add(new Item($"Value number {i}", i + 0.5f));
                }
            }

            // Create a serialized object from this window so we can bind data to
            // it.
            _serializedObject = new SerializedObject(this);

            // Create the list view and bind it.
            _itemListView = new ListView {
                name = "item-list",
                showBoundCollectionSize = false,
                fixedItemHeight = 80
            };
            _itemListView.style.flexGrow = 1;
            _itemListView.makeItem = MakeItem;
            _itemListView.bindItem = BindItem;
            _itemListView.bindingPath = nameof(_itemList);
            _itemListView.Bind(_serializedObject);
            rootVisualElement.Add(_itemListView);

            // Create the footer row containing "+" and "-" buttons.
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.justifyContent = Justify.FlexEnd;
            row.Add(new Button(IncreaseArraySize) { text = "+" });
            row.Add(new Button(DecreaseArraySize) { text = "-" });
            rootVisualElement.Add(row);

            // Find the custom item list's array properties.
            _arrayProperty = _serializedObject.FindProperty(nameof(_itemList));
            _arraySizeProperty = _serializedObject.FindProperty(nameof(_itemList) + ".Array.size");
        }

        private VisualElement MakeItem()
        {
            // Create a row to hold a label and "-" button.
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.justifyContent = Justify.SpaceBetween;

            // Create the label.
            var label = new Label("Custom Item");
            label.AddToClassList("custom-label");
            row.Add(label);

            // Create the "-" button.
            var button = new Button { text = "-", tooltip = "Remove this item from the list" };
            button.RegisterCallback<ClickEvent>((evt) =>
            {
                if (evt.target is VisualElement element && element.userData is int index)
                {
                    _arrayProperty.DeleteArrayElementAtIndex(index);
                    _serializedObject.ApplyModifiedProperties();
                }
            });
            row.Add(button);

            // Add the row and the item field editors to a bindable element container.
            var container = new BindableElement();
            container.Add(row);
            container.Add(new TextField() { bindingPath = nameof(Item.stringValue) });
            container.Add(new FloatField() { bindingPath = nameof(Item.floatValue) });

            return container;
        }

        private void BindItem(VisualElement element, int index)
        {
            // Assign the array index to the user data of the "-" button.
            var button = element.Q<Button>();
            if (button != null)
            {
                button.userData = index;
            }

            // Find the first bindable element.
            if (!(element is IBindable field))
            {
                field = (IBindable)element.Query()
                    .Where(x => x is IBindable)
                    .First();
            }

            // Bind the list view source element to the visual element.
            var itemProp = (SerializedProperty)_itemListView.itemsSource[index];
            field.bindingPath = itemProp.propertyPath;
            element.Bind(itemProp.serializedObject);
        }

        private void IncreaseArraySize()
        {
            _arraySizeProperty.intValue++;
            _serializedObject.ApplyModifiedProperties();
        }

        private void DecreaseArraySize()
        {
            if (_arraySizeProperty.intValue > 0)
            {
                _arraySizeProperty.intValue--;
                _serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
