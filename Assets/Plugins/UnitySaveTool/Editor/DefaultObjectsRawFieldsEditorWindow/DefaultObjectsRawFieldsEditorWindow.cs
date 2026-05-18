#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

namespace UnitySaveTool.EditorTools
{
    public sealed class DefaultObjectsRawFieldsEditorWindow : EditorWindow
    {
        private const string DefaultObjectsFolderName = "DefaultObjects";

        private const string WindowUxmlPath = "Assets/Plugins/UnitySaveTool/Editor/DefaultObjectsRawFieldsEditorWindow.uxml";
        private const string WindowUssPath = "Assets/Plugins/UnitySaveTool/Editor/DefaultObjectsRawFieldsEditorWindow.uss";

        private static readonly string[] DefaultInstallerSearchFolders = new string[]
        {
            "Assets/Plugins/UnitySaveTool/DefaultSettings"
        };

        [SerializeField]
        private DefaultFileSystemInstaller _installer;

        private DiContainer _container;

        private IPathFinder _pathFinder;
        private IFolderMetadataFactory _metadataFactory;

        private string _defaultObjectsFullPath;
        private IFolderFilesCollection _filesCollection;

        private readonly List<Row> _allRows = new List<Row>();
        private readonly List<Row> _filteredRows = new List<Row>();

        private ObjectField _installerField;
        private ToolbarButton _reloadButton;
        private ToolbarButton _createDefaultButton;

        private VisualElement _statusHost;
        private HelpBox _statusBox;
        private ToolbarSearchField _searchField;
        private ListView _listView;

        private Label _detailsHeader;
        private ScrollView _detailsScroll;

        private Foldout _schemaFoldout;
        private VisualElement _fieldsRoot;

        private Foldout _rawJsonFoldout;
        private TextField _rawJsonField;

        private Button _revertButton;
        private Button _saveButton;
        private Button _deleteButton;

        private Row _selected;

        private object _jsonDomRoot;
        private bool _reloadQueued;

        private enum EditMode
        {
            Fields = 0,
            RawJson = 1
        }

        private EditMode _editMode;

        [MenuItem("Tools/UnitySaveTool/Default Objects (Raw Fields)")]
        public static void Open()
        {
            DefaultObjectsRawFieldsEditorWindow window = GetWindow<DefaultObjectsRawFieldsEditorWindow>();
            window.titleContent = new GUIContent("Default Objects (Raw Fields)");
            window.minSize = new Vector2(1040.0f, 600.0f);
        }

        private void OnEnable()
        {
            if (_installer == null)
            {
                _installer = TryLoadDefaultInstallerFromFolder();
            }
        }

        private void CreateGUI()
        {
            rootVisualElement.Clear();

            VisualTreeAsset uxml = LoadWindowUxml();
            if (uxml == null)
            {
                rootVisualElement.Add(new HelpBox(
                    "UXML not found.\nExpected: " + WindowUxmlPath,
                    HelpBoxMessageType.Error));

                return;
            }

            StyleSheet uss = LoadWindowUss();
            if (uss != null)
            {
                rootVisualElement.styleSheets.Add(uss);
            }

            VisualElement ui = uxml.Instantiate();
            rootVisualElement.Add(ui);

            CacheVisualElements(ui);
            BindStaticUi();
            SetDetailsEnabled(false);

            if (_installer != null)
            {
                QueueReload(false);
            }
            else
            {
                SetStatus(
                    "Installer not found. Assign DefaultFileSystemInstaller manually.\nExpected folder: " + DefaultInstallerSearchFolders[0],
                    HelpBoxMessageType.Warning);
            }
        }

        private static VisualTreeAsset LoadWindowUxml()
        {
            VisualTreeAsset asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(WindowUxmlPath);
            if (asset != null)
                return asset;

            string[] guids = AssetDatabase.FindAssets("DefaultObjectsRawFieldsEditorWindow t:VisualTreeAsset");
            if (guids == null || guids.Length == 0)
                return null;

            Array.Sort(guids, StringComparer.Ordinal);

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            if (string.IsNullOrEmpty(path))
                return null;

            return AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path);
        }

        private static StyleSheet LoadWindowUss()
        {
            StyleSheet asset = AssetDatabase.LoadAssetAtPath<StyleSheet>(WindowUssPath);
            if (asset != null)
                return asset;

            string[] guids = AssetDatabase.FindAssets("DefaultObjectsRawFieldsEditorWindow t:StyleSheet");
            if (guids == null || guids.Length == 0)
                return null;

            Array.Sort(guids, StringComparer.Ordinal);

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            if (string.IsNullOrEmpty(path))
                return null;

            return AssetDatabase.LoadAssetAtPath<StyleSheet>(path);
        }

        private void CacheVisualElements(VisualElement ui)
        {
            _installerField = ui.Q<ObjectField>("InstallerField");
            _reloadButton = ui.Q<ToolbarButton>("ReloadButton");
            _createDefaultButton = ui.Q<ToolbarButton>("CreateDefaultButton");
            _searchField = ui.Q<ToolbarSearchField>("SearchField");

            _statusHost = ui.Q<VisualElement>("StatusHost");

            _listView = ui.Q<ListView>("ListView");

            _detailsHeader = ui.Q<Label>("DetailsHeader");
            _detailsScroll = ui.Q<ScrollView>("DetailsScroll");

            _schemaFoldout = ui.Q<Foldout>("SchemaFoldout");
            _fieldsRoot = ui.Q<VisualElement>("FieldsRoot");

            _rawJsonFoldout = ui.Q<Foldout>("RawJsonFoldout");
            _rawJsonField = ui.Q<TextField>("RawJsonField");

            _revertButton = ui.Q<Button>("RevertButton");
            _saveButton = ui.Q<Button>("SaveButton");
            _deleteButton = ui.Q<Button>("DeleteButton");
        }

        private void BindStaticUi()
        {
            if (_installerField != null)
            {
                _installerField.objectType = typeof(DefaultFileSystemInstaller);
                _installerField.allowSceneObjects = false;
                _installerField.value = _installer;
                _installerField.RegisterValueChangedCallback(OnInstallerChanged);
            }

            if (_reloadButton != null)
            {
                _reloadButton.clicked += OnReloadClicked;
            }

            if (_createDefaultButton != null)
            {
                _createDefaultButton.clicked += OpenCreateRawDefault;
            }

            if (_searchField != null)
            {
                _searchField.RegisterValueChangedCallback(OnSearchChanged);
            }

            BuildStatusBox();


            if (_listView != null)
            {
                _listView.itemsSource = _filteredRows;
                _listView.selectionType = SelectionType.Single;
                _listView.fixedItemHeight = 28.0f;
                _listView.makeItem = MakeRowVisual;
                _listView.bindItem = BindRowVisual;
                _listView.selectionChanged += OnSelectionChanged;
            }

            if (_rawJsonField != null)
            {
                _rawJsonField.RegisterValueChangedCallback(OnRawJsonChanged);
            }

            if (_revertButton != null)
            {
                _revertButton.clicked += RevertSelected;
            }

            if (_saveButton != null)
            {
                _saveButton.clicked += SaveSelected;
            }

            if (_deleteButton != null)
            {
                _deleteButton.clicked += DeleteSelected;
            }
        }

        private void BuildStatusBox()
        {
            if (_statusHost == null)
                return;

            _statusHost.Clear();

            _statusBox = new HelpBox(string.Empty, HelpBoxMessageType.None);
            _statusBox.name = "StatusBox";
            _statusBox.AddToClassList("dos-status");
            _statusBox.style.display = DisplayStyle.None;

            _statusHost.Add(_statusBox);
        }


        private void OnReloadClicked()
        {
            QueueReload(true);
        }

        private void OnInstallerChanged(ChangeEvent<UnityEngine.Object> evt)
        {
            DefaultFileSystemInstaller newInstaller = evt.newValue as DefaultFileSystemInstaller;
            _installer = newInstaller;

            ClearRuntimeState();

            if (_installer != null)
            {
                QueueReload(false);
            }
            else
            {
                SetStatus("Assign DefaultFileSystemInstaller to start.", HelpBoxMessageType.Warning);
            }
        }

        private void OnSearchChanged(ChangeEvent<string> evt)
        {
            ApplyFilter(evt.newValue);
        }

        private void QueueReload(bool preserveSelection)
        {
            if (_reloadQueued)
                return;

            _reloadQueued = true;

            EditorApplication.delayCall += () =>
            {
                _reloadQueued = false;
                ReloadAll(preserveSelection);
            };
        }

        private void ReloadAll(bool preserveSelection)
        {
            if (_installer == null)
            {
                ClearRuntimeState();
                SetStatus("Assign DefaultFileSystemInstaller first.", HelpBoxMessageType.Warning);
                return;
            }

            Type previouslySelectedType = preserveSelection && _selected != null ? _selected.DataType : null;

            try
            {
                BuildCoreDependenciesFromInstaller();

                _defaultObjectsFullPath = _pathFinder.GetFullPath(true, new string[] { DefaultObjectsFolderName });
                _filesCollection = _metadataFactory.GetFilesCollection(_defaultObjectsFullPath);

                Dictionary<Type, string> raw = _filesCollection.GetAllWithoutConvertation();

                BuildRows(raw);
                ApplyFilter(_searchField != null ? _searchField.value : string.Empty);

                if (previouslySelectedType != null)
                {
                    if (TrySelectByType(previouslySelectedType) == false)
                        ClearDetails();
                }
                else
                {
                    ClearDetails();
                }

                SetStatus("Loaded from: " + _defaultObjectsFullPath, HelpBoxMessageType.Info);
            }
            catch (Exception ex)
            {
                ClearRuntimeState();
                SetStatus(ex.ToString(), HelpBoxMessageType.Error);
            }
        }

        private void BuildCoreDependenciesFromInstaller()
        {
            _container = new DiContainer();
            InstallIntoContainer(_installer, _container);

            _pathFinder = _container.Resolve<IPathFinder>();
            _metadataFactory = _container.Resolve<IFolderMetadataFactory>();
        }

        private void BuildRows(Dictionary<Type, string> raw)
        {
            _allRows.Clear();

            foreach (KeyValuePair<Type, string> pair in raw)
            {
                _allRows.Add(new Row(pair.Key, pair.Value));
            }

            _allRows.Sort(Row.CompareByName);
        }

        private void ApplyFilter(string query)
        {
            _filteredRows.Clear();

            string q = query != null ? query.Trim() : string.Empty;

            if (string.IsNullOrEmpty(q))
            {
                _filteredRows.AddRange(_allRows);
            }
            else
            {
                for (int i = 0; i < _allRows.Count; i++)
                {
                    Row row = _allRows[i];
                    if (row.TypeDisplayName.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0)
                        _filteredRows.Add(row);
                }
            }

            if (_listView != null)
                _listView.Rebuild();
        }

        private void OnSelectionChanged(IEnumerable<object> selected)
        {
            Row row = ExtractFirstRow(selected);
            if (row == null)
            {
                ClearDetails();
                return;
            }

            _selected = row;
            BuildDetailsForSelected();
        }

        private VisualElement MakeRowVisual()
        {
            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;

            Label label = new Label();
            label.name = "TypeLabel";
            label.style.flexGrow = 1.0f;
            label.style.marginLeft = 8.0f;

            Button editButton = new Button();
            editButton.name = "EditButton";
            editButton.text = "Edit";
            editButton.style.marginRight = 6.0f;
            editButton.RegisterCallback<ClickEvent>(OnInlineEdit);

            Button deleteButton = new Button();
            deleteButton.name = "DeleteButton";
            deleteButton.text = "Del";
            deleteButton.style.marginRight = 8.0f;
            deleteButton.RegisterCallback<ClickEvent>(OnInlineDelete);

            row.Add(label);
            row.Add(editButton);
            row.Add(deleteButton);

            return row;
        }

        private void BindRowVisual(VisualElement element, int index)
        {
            if (index < 0 || index >= _filteredRows.Count)
                return;

            Row row = _filteredRows[index];

            Label label = element.Q<Label>("TypeLabel");
            if (label != null)
                label.text = row.TypeDisplayName;

            Button edit = element.Q<Button>("EditButton");
            Button del = element.Q<Button>("DeleteButton");

            if (edit != null)
                edit.userData = row;

            if (del != null)
                del.userData = row;
        }

        private void OnInlineEdit(ClickEvent evt)
        {
            Button btn = evt.currentTarget as Button;
            Row row = btn != null ? btn.userData as Row : null;
            if (row == null)
                return;

            _selected = row;
            BuildDetailsForSelected();
        }

        private void OnInlineDelete(ClickEvent evt)
        {
            Button btn = evt.currentTarget as Button;
            Row row = btn != null ? btn.userData as Row : null;
            if (row == null)
                return;

            _selected = row;
            DeleteSelected();
        }

        private void BuildDetailsForSelected()
        {
            if (_selected == null)
            {
                ClearDetails();
                return;
            }

            if (_detailsHeader != null)
                _detailsHeader.text = _selected.TypeDisplayName;

            BuildSchema(_selected.DataType);

            string json = string.IsNullOrWhiteSpace(_selected.Json) ? "{}" : _selected.Json;

            if (_rawJsonField != null)
                _rawJsonField.SetValueWithoutNotify(json);

            _editMode = EditMode.Fields;

            object dom;
            string error;
            if (TryParseJson(json, out dom, out error) == false)
            {
                SetStatus("Invalid JSON. Fix in Raw JSON and Save.\n" + error, HelpBoxMessageType.Error);
                dom = new Dictionary<string, object>();
                _editMode = EditMode.RawJson;

                if (_rawJsonFoldout != null)
                    _rawJsonFoldout.value = true;
            }

            _jsonDomRoot = EnsureObjectRoot(dom);

            RebuildFieldsUi();

            SetDetailsEnabled(true);
        }

        private void BuildSchema(Type type)
        {
            if (_schemaFoldout == null)
                return;

            _schemaFoldout.Clear();

            if (type == null)
            {
                _schemaFoldout.text = "Schema";
                return;
            }

            List<FieldInfo> fields = UnitySerializedFieldScanner.GetUnitySerializedFields(type);
            _schemaFoldout.text = "Schema (" + fields.Count + ")";

            for (int i = 0; i < fields.Count; i++)
            {
                FieldInfo f = fields[i];

                VisualElement row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;
                row.style.justifyContent = Justify.SpaceBetween;

                Label name = new Label(f.Name);
                name.style.flexGrow = 1.0f;

                string typeName = f.FieldType.FullName != null ? f.FieldType.FullName : f.FieldType.Name;
                Label t = new Label(typeName);

                row.Add(name);
                row.Add(t);

                _schemaFoldout.Add(row);
            }
        }

        private void RebuildFieldsUi()
        {
            if (_fieldsRoot == null)
                return;

            _fieldsRoot.Clear();

            if (_selected == null)
                return;

            Dictionary<string, object> rootObj = _jsonDomRoot as Dictionary<string, object>;
            if (rootObj == null)
                rootObj = new Dictionary<string, object>();

            List<FieldInfo> fields = UnitySerializedFieldScanner.GetUnitySerializedFields(_selected.DataType);

            for (int i = 0; i < fields.Count; i++)
            {
                FieldInfo field = fields[i];
                VisualElement editor = JsonFieldEditorFactory.CreateEditorForField(
                    field,
                    rootObj,
                    OnFieldsChanged);

                _fieldsRoot.Add(editor);
            }
        }

        private void OnFieldsChanged()
        {
            _editMode = EditMode.Fields;

            if (_rawJsonField != null)
            {
                string pretty = MiniJson.Serialize(_jsonDomRoot, true);
                _rawJsonField.SetValueWithoutNotify(pretty);
            }
        }

        private void OnRawJsonChanged(ChangeEvent<string> evt)
        {
            _editMode = EditMode.RawJson;
        }

        private void SaveSelected()
        {
            if (CanOperateWithSelection() == false)
                return;

            try
            {
                Type type = _selected.DataType;

                string jsonToSave;

                if (_editMode == EditMode.RawJson)
                {
                    object dom;
                    string error;
                    if (TryParseJson(_rawJsonField != null ? _rawJsonField.value : string.Empty, out dom, out error) == false)
                    {
                        SetStatus("Cannot save: JSON is invalid.\n" + error, HelpBoxMessageType.Error);
                        return;
                    }

                    _jsonDomRoot = EnsureObjectRoot(dom);
                    jsonToSave = MiniJson.Serialize(_jsonDomRoot, true);
                }
                else
                {
                    jsonToSave = MiniJson.Serialize(_jsonDomRoot, true);
                }

                if (string.IsNullOrWhiteSpace(jsonToSave))
                    jsonToSave = "{}";

                _filesCollection.ResetWithoutConvertation(type, jsonToSave);

                QueueReload(true);
                SetStatus("Saved raw JSON for: " + type.FullName, HelpBoxMessageType.Info);
            }
            catch (Exception ex)
            {
                SetStatus(ex.ToString(), HelpBoxMessageType.Error);
            }
        }

        private void RevertSelected()
        {
            if (CanOperateWithSelection() == false)
                return;

            try
            {
                Type type = _selected.DataType;
                string json = _filesCollection.GetWithoutConvertation(type);

                if (string.IsNullOrWhiteSpace(json))
                    json = "{}";

                if (_rawJsonField != null)
                    _rawJsonField.SetValueWithoutNotify(json);

                object dom;
                string error;
                if (TryParseJson(json, out dom, out error) == false)
                {
                    SetStatus("Invalid JSON on disk.\n" + error, HelpBoxMessageType.Error);
                    _jsonDomRoot = new Dictionary<string, object>();
                    _editMode = EditMode.RawJson;

                    if (_rawJsonFoldout != null)
                        _rawJsonFoldout.value = true;
                }
                else
                {
                    _jsonDomRoot = EnsureObjectRoot(dom);
                    _editMode = EditMode.Fields;
                }

                RebuildFieldsUi();

                SetStatus("Reverted.", HelpBoxMessageType.Info);
            }
            catch (Exception ex)
            {
                SetStatus(ex.ToString(), HelpBoxMessageType.Error);
            }
        }

        private void DeleteSelected()
        {
            if (CanOperateWithSelection() == false)
                return;

            Type type = _selected.DataType;

            bool confirm = EditorUtility.DisplayDialog(
                "Delete default",
                "Delete raw default for:\n\n" + type.FullName + "\n\nThis cannot be undone.",
                "Delete",
                "Cancel");

            if (confirm == false)
                return;

            try
            {
                _filesCollection.Remove(type);

                QueueReload(false);
                SetStatus("Deleted: " + type.FullName, HelpBoxMessageType.Info);
            }
            catch (Exception ex)
            {
                SetStatus(ex.ToString(), HelpBoxMessageType.Error);
            }
        }

        private void OpenCreateRawDefault()
        {
            if (_filesCollection == null)
            {
                SetStatus("Not ready. Assign installer and wait for load.", HelpBoxMessageType.Warning);
                return;
            }

            List<Type> candidates = SerializableReferenceTypeCatalog.GetOrBuild();

            TypePickerWindow.Open(
                "Create Raw Default",
                candidates,
                OnTypePickedForCreate);
        }

        private void OnTypePickedForCreate(Type type)
        {
            if (type == null)
                return;

            if (_filesCollection == null)
            {
                SetStatus("Not ready. Assign installer and wait for load.", HelpBoxMessageType.Warning);
                return;
            }

            if (type.IsClass == false)
            {
                SetStatus("Only reference types (class) are allowed.", HelpBoxMessageType.Warning);
                return;
            }

            try
            {
                bool exists = _filesCollection.HasType(type);
                if (exists)
                {
                    bool replace = EditorUtility.DisplayDialog(
                        "Replace existing default",
                        "Raw default already exists for:\n\n" + type.FullName + "\n\nReplace it?",
                        "Replace",
                        "Cancel");

                    if (replace == false)
                        return;
                }

                Dictionary<string, object> dom = JsonTemplateBuilder.BuildTemplateDom(type);
                string json = MiniJson.Serialize(dom, true);

                _filesCollection.ResetWithoutConvertation(type, json);

                QueueReload(true);
                SetStatus("Created raw default for: " + type.FullName, HelpBoxMessageType.Info);
            }
            catch (Exception ex)
            {
                SetStatus(ex.ToString(), HelpBoxMessageType.Error);
            }
        }

        private bool CanOperateWithSelection()
        {
            if (_installer == null)
            {
                SetStatus("Assign DefaultFileSystemInstaller first.", HelpBoxMessageType.Warning);
                return false;
            }

            if (_filesCollection == null)
            {
                SetStatus("Data not loaded. Press Reload.", HelpBoxMessageType.Warning);
                return false;
            }

            if (_selected == null)
            {
                SetStatus("Select an entry first.", HelpBoxMessageType.Warning);
                return false;
            }

            return true;
        }

        private bool TrySelectByType(Type type)
        {
            if (_listView == null)
                return false;

            for (int i = 0; i < _filteredRows.Count; i++)
            {
                Row row = _filteredRows[i];
                if (row.DataType == type)
                {
                    _listView.SetSelection(i);
                    _selected = row;
                    BuildDetailsForSelected();
                    return true;
                }
            }

            return false;
        }

        private void ClearDetails()
        {
            _selected = null;

            if (_detailsHeader != null)
                _detailsHeader.text = "No selection";

            if (_schemaFoldout != null)
            {
                _schemaFoldout.text = "Schema";
                _schemaFoldout.Clear();
            }

            if (_fieldsRoot != null)
                _fieldsRoot.Clear();

            if (_rawJsonField != null)
                _rawJsonField.SetValueWithoutNotify(string.Empty);

            _jsonDomRoot = null;
            _editMode = EditMode.Fields;

            SetDetailsEnabled(false);
        }

        private void SetDetailsEnabled(bool enabled)
        {
            if (_revertButton != null)
                _revertButton.SetEnabled(enabled);

            if (_saveButton != null)
                _saveButton.SetEnabled(enabled);

            if (_deleteButton != null)
                _deleteButton.SetEnabled(enabled);

            if (_detailsScroll != null)
                _detailsScroll.SetEnabled(enabled);
        }

        private void ClearRuntimeState()
        {
            _container = null;
            _pathFinder = null;
            _metadataFactory = null;

            _defaultObjectsFullPath = null;
            _filesCollection = null;

            _allRows.Clear();
            _filteredRows.Clear();

            if (_listView != null)
                _listView.Rebuild();

            ClearDetails();
        }

        private void SetStatus(string message, HelpBoxMessageType type)
        {
            if (_statusBox == null)
                return;

            if (string.IsNullOrEmpty(message))
            {
                _statusBox.style.display = DisplayStyle.None;
                return;
            }

            _statusBox.text = message;
            _statusBox.messageType = type;
            _statusBox.style.display = DisplayStyle.Flex;
        }

        private static Row ExtractFirstRow(IEnumerable<object> selected)
        {
            if (selected == null)
                return null;

            foreach (object obj in selected)
            {
                Row row = obj as Row;
                if (row != null)
                    return row;
            }

            return null;
        }

        private static bool TryParseJson(string json, out object dom, out string error)
        {
            dom = null;
            error = null;

            try
            {
                dom = MiniJson.Deserialize(json);
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        private static object EnsureObjectRoot(object dom)
        {
            Dictionary<string, object> obj = dom as Dictionary<string, object>;
            if (obj != null)
                return obj;

            return new Dictionary<string, object>();
        }

        private static void InstallIntoContainer(ScriptableObjectInstaller installer, DiContainer container)
        {
            SetInstallerContainerByReflection(installer, container);
            installer.InstallBindings();
        }

        private static void SetInstallerContainerByReflection(ScriptableObjectInstaller installer, DiContainer container)
        {
            Type type = installer.GetType();

            PropertyInfo p = FindContainerProperty(type);
            if (p != null)
            {
                p.SetValue(installer, container);
                return;
            }

            FieldInfo f = FindContainerField(type);
            if (f != null)
            {
                f.SetValue(installer, container);
                return;
            }

            throw new MissingMemberException("Cannot set Zenject installer container. Expected a 'Container' property or a DiContainer field.");
        }

        private static PropertyInfo FindContainerProperty(Type installerType)
        {
            Type t = installerType;
            while (t != null)
            {
                PropertyInfo p = t.GetProperty("Container", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (p != null && p.CanWrite && p.PropertyType == typeof(DiContainer))
                    return p;

                t = t.BaseType;
            }

            return null;
        }

        private static FieldInfo FindContainerField(Type installerType)
        {
            Type t = installerType;
            while (t != null)
            {
                FieldInfo[] fields = t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                for (int i = 0; i < fields.Length; i++)
                {
                    if (fields[i].FieldType == typeof(DiContainer))
                        return fields[i];
                }

                t = t.BaseType;
            }

            return null;
        }

        private static DefaultFileSystemInstaller TryLoadDefaultInstallerFromFolder()
        {
            string[] guids = AssetDatabase.FindAssets("t:DefaultFileSystemInstaller", DefaultInstallerSearchFolders);
            if (guids == null || guids.Length == 0)
                return null;

            Array.Sort(guids, StringComparer.Ordinal);

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            if (string.IsNullOrEmpty(path))
                return null;

            DefaultFileSystemInstaller installer = AssetDatabase.LoadAssetAtPath<DefaultFileSystemInstaller>(path);
            return installer;
        }

        [Serializable]
        private sealed class Row
        {
            public readonly Type DataType;
            public readonly string Json;
            public readonly string TypeDisplayName;

            public Row(Type dataType, string json)
            {
                DataType = dataType;
                Json = json;
                TypeDisplayName = dataType != null
                    ? (dataType.FullName != null ? dataType.FullName : dataType.Name)
                    : "<null>";
            }

            public static int CompareByName(Row a, Row b)
            {
                string an = a != null ? a.TypeDisplayName : string.Empty;
                string bn = b != null ? b.TypeDisplayName : string.Empty;
                return string.Compare(an, bn, StringComparison.Ordinal);
            }
        }
    }
}
#endif
