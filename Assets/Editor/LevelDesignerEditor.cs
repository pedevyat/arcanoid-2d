#if UNITY_EDITOR
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(LevelDesignerScript))]
public class LevelDesignerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var designer = (LevelDesignerScript)target;

        EditorGUILayout.HelpBox(
            "Конструктор уровней. Размещает блоки под объектом 'Designed Blocks' и сохраняет уровень в Assets/Resources/Levels. " +
            "В режиме игры блоки конструктора скрываются, а уровень загружается из XML.",
            MessageType.Info);

        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Размещение блоков", EditorStyles.boldLabel);

        if (GUILayout.Button("Добавить горизонтальную линию"))
            AddLine(designer, true);

        if (GUILayout.Button("Добавить вертикальную линию"))
            AddLine(designer, false);

        EditorGUILayout.Space();
        if (GUILayout.Button("Добавить смешанную горизонтальную линию"))
            AddMixedLine(designer, true);

        if (GUILayout.Button("Добавить смешанную вертикальную линию"))
            AddMixedLine(designer, false);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Файл уровня", EditorStyles.boldLabel);
        EditorGUILayout.LabelField(LevelPath(designer.levelNumber));

        if (GUILayout.Button("Сохранить уровень в XML"))
            SaveLevel(designer);

        if (GUILayout.Button("Загрузить уровень из XML"))
            LoadLevel(designer);

        if (GUILayout.Button("Очистить размещённые блоки"))
            ClearLevel(designer);
    }

    void AddLine(LevelDesignerScript designer, bool horizontal)
    {
        var root = GetLevelRoot(designer);
        var position = designer.startPosition;
        int count = Mathf.Max(0, designer.count);

        for (int i = 0; i < count; i++)
        {
            AddBlock(designer, root, designer.blockType, position);

            if (horizontal)
                position.x += designer.horizontalSpacing;
            else
                position.y -= designer.verticalSpacing;
        }

        MarkSceneDirty();
    }

    void AddMixedLine(LevelDesignerScript designer, bool horizontal)
    {
        var pattern = new List<LevelBlockType>();
        foreach (var entry in designer.mixedPattern.Split(','))
        {
            LevelBlockType type;
            if (TryParseBlockType(entry, out type))
                pattern.Add(type);
            else if (!string.IsNullOrWhiteSpace(entry))
                Debug.LogWarning("Неизвестный тип блока в шаблоне: " + entry);
        }

        if (pattern.Count == 0)
        {
            Debug.LogWarning("Не задан шаблон блоков.");
            return;
        }

        var root = GetLevelRoot(designer);
        var position = designer.startPosition;
        int count = Mathf.Max(0, designer.mixedCount);

        for (int i = 0; i < count; i++)
        {
            AddBlock(designer, root, pattern[i % pattern.Count], position);

            if (horizontal)
                position.x += designer.horizontalSpacing;
            else
                position.y -= designer.verticalSpacing;
        }

        MarkSceneDirty();
    }

    GameObject AddBlock(LevelDesignerScript designer, Transform root, LevelBlockType type, Vector3 position)
    {
        var prefab = GetPrefab(designer, type);
        if (prefab == null)
        {
            Debug.LogError("Не назначен префаб для типа блока: " + type);
            return null;
        }

        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        if (instance == null)
            return null;

        Undo.RegisterCreatedObjectUndo(instance, "Add Level Block");
        instance.transform.SetParent(root, false);
        instance.transform.position = position;
        return instance;
    }

    void SaveLevel(LevelDesignerScript designer)
    {
        var root = GetLevelRoot(designer);
        var blocks = root.GetComponentsInChildren<BlockScript>(true);

        if (blocks.Length == 0)
        {
            Debug.LogWarning("Нет блоков для сохранения. Сначала разместите блоки на сцене.");
            return;
        }

        var document = new XmlDocument();
        var levelElement = document.CreateElement("Level");
        levelElement.SetAttribute("number", designer.levelNumber.ToString(CultureInfo.InvariantCulture));
        document.AppendChild(levelElement);

        foreach (var block in blocks)
        {
            var type = GetBlockType(block.gameObject.name);
            if (!type.HasValue)
            {
                Debug.LogWarning("Не удалось определить тип блока: " + block.gameObject.name);
                continue;
            }

            var blockElement = document.CreateElement("Block");
            blockElement.SetAttribute("type", type.Value.ToString());

            var position = block.transform.position;
            blockElement.SetAttribute("x", position.x.ToString("R", CultureInfo.InvariantCulture));
            blockElement.SetAttribute("y", position.y.ToString("R", CultureInfo.InvariantCulture));
            blockElement.SetAttribute("z", position.z.ToString("R", CultureInfo.InvariantCulture));

            levelElement.AppendChild(blockElement);
        }

        string path = LevelPath(designer.levelNumber);
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        document.Save(path);
        AssetDatabase.Refresh();

        Debug.Log("Уровень сохранён: " + path);
    }

    void LoadLevel(LevelDesignerScript designer)
    {
        string path = LevelPath(designer.levelNumber);
        if (!File.Exists(path))
        {
            Debug.LogWarning("Файл уровня не найден: " + path);
            return;
        }

        var document = new XmlDocument();
        document.Load(path);

        var levelElement = document.DocumentElement;
        if (levelElement == null)
            return;

        int parsedLevel;
        if (int.TryParse(levelElement.GetAttribute("number"), out parsedLevel))
            designer.levelNumber = parsedLevel;

        var root = GetLevelRoot(designer);
        ClearRoot(root);

        var blockNodes = levelElement.SelectNodes("Block");
        if (blockNodes != null)
        {
            foreach (XmlNode blockNode in blockNodes)
            {
                if (blockNode.Attributes == null)
                    continue;

                var typeAttribute = blockNode.Attributes["type"];
                var xAttribute = blockNode.Attributes["x"];
                var yAttribute = blockNode.Attributes["y"];
                if (typeAttribute == null || xAttribute == null || yAttribute == null)
                    continue;

                LevelBlockType type;
                if (!TryParseBlockType(typeAttribute.Value, out type))
                {
                    Debug.LogWarning("Неизвестный тип блока в XML: " + typeAttribute.Value);
                    continue;
                }

                float x = ParseFloat(xAttribute.Value);
                float y = ParseFloat(yAttribute.Value);
                float z = 0f;
                var zAttribute = blockNode.Attributes["z"];
                if (zAttribute != null)
                    z = ParseFloat(zAttribute.Value);

                AddBlock(designer, root, type, new Vector3(x, y, z));
            }
        }

        MarkSceneDirty();
        Debug.Log("Уровень загружен: " + path);
    }

    void ClearLevel(LevelDesignerScript designer)
    {
        var root = GetLevelRoot(designer);
        ClearRoot(root);
        MarkSceneDirty();
    }

    void ClearRoot(Transform root)
    {
        var children = new List<GameObject>();
        foreach (Transform child in root)
            children.Add(child.gameObject);

        foreach (var child in children)
            Undo.DestroyObjectImmediate(child);
    }

    Transform GetLevelRoot(LevelDesignerScript designer)
    {
        if (designer.levelRoot != null)
            return designer.levelRoot;

        var existing = designer.transform.Find("Designed Blocks");
        if (existing != null)
        {
            designer.levelRoot = existing;
            EditorUtility.SetDirty(designer);
            return existing;
        }

        var rootObject = new GameObject("Designed Blocks");
        Undo.RegisterCreatedObjectUndo(rootObject, "Create Level Root");
        rootObject.transform.SetParent(designer.transform, false);

        designer.levelRoot = rootObject.transform;
        EditorUtility.SetDirty(designer);

        return rootObject.transform;
    }

    GameObject GetPrefab(LevelDesignerScript designer, LevelBlockType type)
    {
        switch (type)
        {
            case LevelBlockType.Blue:
                return designer.bluePrefab;
            case LevelBlockType.Red:
                return designer.redPrefab;
            case LevelBlockType.Green:
                return designer.greenPrefab;
            case LevelBlockType.Yellow:
                return designer.yellowPrefab;
            case LevelBlockType.MovingYellow:
                return designer.movingYellowPrefab;
            case LevelBlockType.MovingBlue:
                return designer.movingBluePrefab;
            case LevelBlockType.RedXO:
                return designer.redXOPrefab;
        }

        return null;
    }

    LevelBlockType? GetBlockType(string objectName)
    {
        string name = objectName.Replace("(Clone)", "").Trim().ToLowerInvariant();

        switch (name)
        {
            case "blue block":
                return LevelBlockType.Blue;
            case "red block":
                return LevelBlockType.Red;
            case "green block":
                return LevelBlockType.Green;
            case "yellow block":
                return LevelBlockType.Yellow;
            case "moving yellow block":
                return LevelBlockType.MovingYellow;
            case "moving blue block":
                return LevelBlockType.MovingBlue;
            case "red xo block":
                return LevelBlockType.RedXO;
        }

        return null;
    }

    bool TryParseBlockType(string text, out LevelBlockType type)
    {
        switch (text.Trim().ToLowerInvariant())
        {
            case "b":
            case "blue":
                type = LevelBlockType.Blue;
                return true;
            case "r":
            case "red":
                type = LevelBlockType.Red;
                return true;
            case "g":
            case "green":
                type = LevelBlockType.Green;
                return true;
            case "y":
            case "yellow":
                type = LevelBlockType.Yellow;
                return true;
            case "my":
            case "movingyellow":
                type = LevelBlockType.MovingYellow;
                return true;
            case "mb":
            case "movingblue":
                type = LevelBlockType.MovingBlue;
                return true;
            case "xo":
            case "redxo":
                type = LevelBlockType.RedXO;
                return true;
        }

        type = LevelBlockType.Red;
        return false;
    }

    float ParseFloat(string value)
    {
        float result;
        if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
            return result;

        return 0f;
    }

    string LevelPath(int levelNumber)
    {
        string fileName = "level_" + levelNumber.ToString("d2", CultureInfo.InvariantCulture) + ".xml";
        return Path.Combine(Application.dataPath, "Resources", "Levels", fileName);
    }

    void MarkSceneDirty()
    {
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
    }
}
#endif
