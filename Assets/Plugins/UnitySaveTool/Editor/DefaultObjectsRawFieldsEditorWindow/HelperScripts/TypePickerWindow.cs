using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnitySaveTool.EditorTools
{
    public class TypePickerWindow : EditorWindow
    {
        private Action<Type> _onPicked;
        private List<Type> _types;
        private readonly List<Type> _filtered = new List<Type>();

        private ToolbarSearchField _search;
        private ListView _list;
        private Button _pickButton;

        private Type _selectedType;

        public static void Open(string title, List<Type> types, Action<Type> onPicked)
        {
            TypePickerWindow w = CreateInstance<TypePickerWindow>();
            w.titleContent = new GUIContent(title);
            w._types = types != null ? types : new List<Type>();
            w._onPicked = onPicked;
            w.minSize = new Vector2(600.0f, 600.0f);
            w.maxSize = new Vector2(1100.0f, 1100.0f);
            w.ShowUtility();
        }

        private void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            root.style.flexDirection = FlexDirection.Column;
            root.style.minHeight = 0.0f;

            Toolbar tb = new Toolbar();

            _search = new ToolbarSearchField();
            _search.style.flexGrow = 1.0f;
            _search.RegisterValueChangedCallback(OnSearchChanged);

            tb.Add(_search);
            root.Add(tb);

            _list = new ListView();
            _list.style.flexGrow = 1.0f;
            _list.style.minHeight = 0.0f;
            _list.selectionType = SelectionType.Single;

            _list.makeItem = MakeTypeItem;
            _list.bindItem = BindTypeItem;

            _list.selectionChanged += OnSelection;
            _list.itemsChosen += OnItemsChosen;

            root.Add(_list);
            root.Add(BuildFooter());

            ApplyFilter(string.Empty);
        }

        private VisualElement MakeTypeItem()
        {
            Label l = new Label();
            l.style.marginLeft = 8.0f;
            l.style.unityTextAlign = TextAnchor.MiddleLeft;
            return l;
        }

        private void BindTypeItem(VisualElement e, int i)
        {
            Label l = e as Label;
            Type t = _filtered[i];
            l.text = t.FullName;
        }

        private VisualElement BuildFooter()
        {
            VisualElement footer = new VisualElement();
            footer.style.flexDirection = FlexDirection.Row;
            footer.style.justifyContent = Justify.FlexEnd;
            footer.style.paddingLeft = 8.0f;
            footer.style.paddingRight = 8.0f;
            footer.style.paddingTop = 8.0f;
            footer.style.paddingBottom = 8.0f;
            footer.style.flexShrink = 0.0f;

            Button cancel = new Button(Close) { text = "Cancel" };

            _pickButton = new Button(Pick) { text = "Pick" };
            _pickButton.SetEnabled(false);

            footer.Add(cancel);
            footer.Add(_pickButton);

            return footer;
        }

        private void OnSearchChanged(ChangeEvent<string> evt)
        {
            ApplyFilter(evt.newValue);
        }

        private void ApplyFilter(string q)
        {
            _filtered.Clear();

            string query = q != null ? q.Trim() : string.Empty;

            if (string.IsNullOrEmpty(query))
            {
                _filtered.AddRange(_types);
            }
            else
            {
                for (int i = 0; i < _types.Count; i++)
                {
                    Type t = _types[i];
                    string name = t.FullName;
                    if (name != null && name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                        _filtered.Add(t);
                }
            }

            _list.itemsSource = _filtered;
            _list.Rebuild();

            _selectedType = null;
            _pickButton.SetEnabled(false);
        }

        private void OnSelection(IEnumerable<object> items)
        {
            _selectedType = null;

            if (items != null)
            {
                foreach (object o in items)
                {
                    Type t = o as Type;
                    if (t != null)
                    {
                        _selectedType = t;
                        break;
                    }
                }
            }

            _pickButton.SetEnabled(_selectedType != null);
        }

        private void OnItemsChosen(IEnumerable<object> items)
        {
            OnSelection(items);
            Pick();
        }

        private void Pick()
        {
            if (_selectedType == null)
                return;

            Action<Type> cb = _onPicked;
            Close();
            if (cb != null)
                cb(_selectedType);
        }
    }
}
