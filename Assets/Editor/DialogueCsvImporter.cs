using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class DialogueCsvImporter : AssetPostprocessor
{
    private const string CsvPath    = "Assets/Editor/Data/dialogue.csv";
    private const string OutputPath = "Assets/Kkyakdugi/Data/DialogueDatabase.asset";

    static void OnPostprocessAllAssets(
        string[] importedAssets, string[] deletedAssets,
        string[] movedAssets,    string[] movedFromPaths)
    {
        foreach (string path in importedAssets)
        {
            if (path == CsvPath)
            {
                Import();
                return;
            }
        }
    }

    [MenuItem("Game/Import Dialogue CSV")]
    public static void Import()
    {
        if (!File.Exists(CsvPath))
        {
            Debug.LogError($"[DialogueCsvImporter] CSV not found: {CsvPath}");
            return;
        }

        string raw       = File.ReadAllText(CsvPath, Encoding.GetEncoding(949));
        var    sequences = ParseCsv(raw);

        var db = AssetDatabase.LoadAssetAtPath<DialogueDatabase>(OutputPath);
        if (db == null)
        {
            db = ScriptableObject.CreateInstance<DialogueDatabase>();
            string dir = Path.GetDirectoryName(OutputPath);
            if (!AssetDatabase.IsValidFolder(dir))
                Directory.CreateDirectory(dir);
            AssetDatabase.CreateAsset(db, OutputPath);
        }

        db.sequences = sequences.ToArray();
        EditorUtility.SetDirty(db);
        AssetDatabase.SaveAssets();
        Debug.Log($"[DialogueCsvImporter] {sequences.Count} sequences imported -> {OutputPath}");
    }

    private static List<DialogueSequence> ParseCsv(string csv)
    {
        var rawMap = new Dictionary<string, List<(int order, DialogueLine line)>>();

        string[] rows = csv.Split('\n');
        for (int i = 1; i < rows.Length; i++)
        {
            string row = rows[i].Trim('\r', '\n', ' ');
            if (string.IsNullOrEmpty(row)) continue;

            var cols = SplitRow(row);
            if (cols.Count < 5) continue;

            string sequenceId = cols[0].Trim();
            if (!int.TryParse(cols[1].Trim(), out int order)) continue;

            var line = new DialogueLine
            {
                speaker    = cols[2].Trim(),
                portraitKey = cols[3].Trim(),
                text       = cols[4].Trim()
            };

            if (!rawMap.ContainsKey(sequenceId))
                rawMap[sequenceId] = new List<(int, DialogueLine)>();
            rawMap[sequenceId].Add((order, line));
        }

        var result = new List<DialogueSequence>();
        foreach (var kvp in rawMap)
        {
            kvp.Value.Sort((a, b) => a.order.CompareTo(b.order));

            var lines = new DialogueLine[kvp.Value.Count];
            for (int i = 0; i < kvp.Value.Count; i++)
                lines[i] = kvp.Value[i].line;

            result.Add(new DialogueSequence { sequenceId = kvp.Key, lines = lines });
        }
        return result;
    }

    private static List<string> SplitRow(string row)
    {
        var  fields   = new List<string>();
        var  field    = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < row.Length; i++)
        {
            char c = row[i];
            if (c == '"')
            {
                if (inQuotes && i + 1 < row.Length && row[i + 1] == '"')
                {
                    field.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                fields.Add(field.ToString());
                field.Clear();
            }
            else
            {
                field.Append(c);
            }
        }
        fields.Add(field.ToString());
        return fields;
    }
}
