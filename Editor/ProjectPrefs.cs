/******************************************************************************/
/*
  unity-editor-prefs
  https://github.com/TheAllenChou/unity-project-prefs

  by Ming-Lun "Allen" Chou
     http://AllenChou.net
     http://twitter.com/TheAllenChou
*/
/******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;

using UnityEngine;

namespace LongBunnyLabs
{
  public class ProjectPrefs : ScriptableObject
  {
    // modify this to change asset path
    private static readonly string InstancePath = "Assets/ProjectPrefs.asset";

    [Serializable]
    public class Record
    {
      public enum TypeEnum
      {
        Bool, 
        Int, 
        Float, 
        String, 
        Set, 
      }

      public string Key = "";
      public TypeEnum Type = TypeEnum.Bool;
      public string Value = "";

      public void Sort()
      {
        if (Type != TypeEnum.Set)
          return;

        var set = StringToSet(Value);
        set = set.OrderBy(x => x).ToArray();
        Value = SetToString(set);
      }
    }

    [SerializeField] private List<Record> m_records = new List<Record>();
    public List<Record> Records => m_records;

    [SerializeField] private int m_revision = -1;
    public int Revision => m_revision;

    private static ProjectPrefs Instance
    {
      get
      {
        var instance = AssetDatabase.LoadAssetAtPath<ProjectPrefs>(InstancePath);
        
        if (instance == null)
        {
          instance = CreateInstance<ProjectPrefs>();
          instance.m_revision = MudBun.Revision;
          AssetDatabase.CreateAsset(instance, InstancePath);
          AssetDatabase.Refresh();

          instance = AssetDatabase.LoadAssetAtPath<ProjectPrefs>(InstancePath);
          Assert.Unequal(instance, null);
        }

        return instance;
      }
    }

    private static string[] StringToSet(string value)
    {
      return value.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
    }

    private static string SetToString(string[] set)
    {
      return string.Join(";", set);
    }

    private static string AddToSetString(string setStr, string value)
    {
      var set = StringToSet(setStr);
      if (set.Contains(value))
        return setStr;

      set = set.Append(value).ToArray();
      return SetToString(set);
    }

    private static string RemoveFromSetString(string setStr, string value)
    {
      var set = StringToSet(setStr);
      set = set.Where(x => x != value).ToArray();
      return SetToString(set);
    }

    public static bool GetRecord(string key, out Record record)
    {
      var list = Instance.Records.FindAll(x => x.Key.Equals(key));
      if (list == null || list.Count == 0)
      {
        record = null;
        return false;
      }

      record = list.First();
      return true;
    }

    public static void SetRecord(string key, Record record)
    {
      if (GetRecord(key, out Record existingRecord))
      {
        if (existingRecord.Type == record.Type 
            && existingRecord.Value.Equals(record.Value))
            return;

        existingRecord.Type = record.Type;
        existingRecord.Value = record.Value;
      }
      else
      {
        record.Key = key;
        Instance.Records.Add(record);
      }

      EditorUtility.SetDirty(Instance);
      AssetDatabase.SaveAssets();
    }

    public static bool HasKey(string key)
    {
      return Instance.Records.Any(x => x.Key.Equals(key));
    }

    public static void DeleteKey(string key)
    {
      Instance.Records.RemoveAll(x => x.Key.Equals(key));
    }

    public static bool GetBool(string key, bool defaultValue)
    {
      if (!GetRecord(key, out Record record))
        return defaultValue;

      Assert.Equal(record.Type, Record.TypeEnum.Bool);

      if (!bool.TryParse(record.Value, out bool result))
      {
        Debug.LogWarning($"MudBun ProjectPrefs: Cannot parse string \"{record.Value}\" into bool for project preference \"{record.Key}\".");
        return defaultValue;
      }

      return result;
    }

    public static void SetBool(string key, bool value)
    {
      SetRecord(key, new Record() { Type = Record.TypeEnum.Bool, Value = value.ToString() });
    }

    public static int GetInt(string key, int defaultValue)
    {
      if (!GetRecord(key, out Record record))
        return defaultValue;

      Assert.Equal(record.Type, Record.TypeEnum.Int);

      if (!int.TryParse(record.Value, out int result))
      {
        Debug.LogWarning($"MudBun ProjectPrefs: Cannot parse string \"{record.Value}\" into int for project preference \"{record.Key}\".");
        return defaultValue;
      }

      return result;
    }

    public static void SetInt(string key, int value)
    {
      SetRecord(key, new Record() { Type = Record.TypeEnum.Int, Value = value.ToString() });
    }

    public static float GetFloat(string key, float defaultValue)
    {
      if (!GetRecord(key, out Record record))
        return defaultValue;

      Assert.Equal(record.Type, Record.TypeEnum.Float);

      if (!float.TryParse(record.Value, out float result))
      {
        Debug.LogWarning($"MudBun ProjectPrefs: Cannot parse string \"{record.Value}\" into float for project preference \"{record.Key}\".");
        return defaultValue;
      }

      return result;
    }

    public static void SetFloat(string key, float value)
    {
      SetRecord(key, new Record() { Type = Record.TypeEnum.Float, Value = value.ToString() });
    }

    public static string GetString(string key, string defaultValue)
    {
      if (!GetRecord(key, out Record record))
        return defaultValue;

      Assert.Equal(record.Type, Record.TypeEnum.String);

      return record.Value;
    }

    public static void SetString(string key, string value)
    {
      SetRecord(key, new Record() { Type = Record.TypeEnum.String, Value = value });
    }

    public static string[] GetSet(string key, string[] defaultValue)
    {
      if (!GetRecord(key, out Record record))
        return defaultValue;

      Assert.Equal(record.Type, Record.TypeEnum.Set);

      return record.Value.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
    }

    public static bool SetContains(string key, string value)
    {
      var set = GetSet(key, null);
      if (set == null)
        return false;

      return set.Contains(value);
    }

    public static void AddToSet(string key, string value)
    {
      if (!GetRecord(key, out Record record))
      {
        SetRecord(key, new Record() { Type = Record.TypeEnum.Set, Value = value });
        return;
      }

      Assert.Equal(record.Type, Record.TypeEnum.Set);

      record.Value = AddToSetString(record.Value, value);
    }

    public static void RemoveFromSet(string key, string value)
    {
      if (!GetRecord(key, out Record record))
        return;

      Assert.Equal(record.Type, Record.TypeEnum.Set);

      record.Value = RemoveFromSetString(record.Value, value);
    }
  }

  [CustomEditor(typeof(ProjectPrefs))]
  public class ProjectPrefsEditor : MudEditorBase
  {
    private static readonly int TypeWidth = 80;
    private static readonly int SmallButtonWidth = 25;

    public override void OnInspectorGUI()
    {
      serializedObject.Update();

      var prefs = (ProjectPrefs) serializedObject.targetObject;
      var records = prefs.Records;

      if (records == null)
        return;

      EditorGUI.BeginChangeCheck();

      ProjectPrefs.Record recordToMoveUp = null;
      ProjectPrefs.Record recordToMoveDown = null;
      ProjectPrefs.Record recordToDelete = null;
      foreach (var record in records)
      {
        EditorGUILayout.BeginHorizontal();
          record.Key = EditorGUILayout.TextField(record.Key);
          if (GUILayout.Button("↑", GUILayout.MinWidth(SmallButtonWidth), GUILayout.MaxWidth(SmallButtonWidth)))
            recordToMoveUp = record;
          if (GUILayout.Button("↓", GUILayout.MinWidth(SmallButtonWidth), GUILayout.MaxWidth(SmallButtonWidth)))
            recordToMoveDown = record;
          if (GUILayout.Button("-", GUILayout.MinWidth(SmallButtonWidth), GUILayout.MaxWidth(SmallButtonWidth)))
            recordToDelete = record;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
          EditorGUILayout.BeginHorizontal();
            record.Type = (ProjectPrefs.Record.TypeEnum) Convert.ToInt32(EditorGUILayout.EnumPopup(record.Type, GUILayout.MinWidth(TypeWidth), GUILayout.MaxWidth(TypeWidth)));
            switch (record.Type)
            {
            case ProjectPrefs.Record.TypeEnum.Bool:
              bool boolValue;
              if (!bool.TryParse(record.Value, out boolValue))
                boolValue = false;
              record.Value = EditorGUILayout.Toggle(boolValue).ToString();
              break;
            case ProjectPrefs.Record.TypeEnum.Int:
              int intValue;
              if (!int.TryParse(record.Value, out intValue))
                intValue = 0;
              record.Value = EditorGUILayout.IntField(intValue).ToString();
              break;
            case ProjectPrefs.Record.TypeEnum.Float:
              float floatValue;
              if (!float.TryParse(record.Value, out floatValue))
                floatValue = 0.0f;
              record.Value = EditorGUILayout.FloatField(floatValue).ToString();
              break;
            case ProjectPrefs.Record.TypeEnum.Set:
            case ProjectPrefs.Record.TypeEnum.String:
              record.Value = EditorGUILayout.TextField(record.Value);
              break;
            }
          EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
      }

      if (recordToMoveUp != null)
      {
        int i = records.FindIndex(x => x == recordToMoveUp);
        if (i > 0)
        {
          var recordToSwap = records[i - 1];
          records[i - 1] = recordToMoveUp;
          records[i] = recordToSwap;
        }
      }

      if (recordToMoveDown != null)
      {
        int i = records.FindIndex(x => x == recordToMoveDown);
        if (i >= 0 && i < records.Count - 1)
        {
          var recordToSwap = records[i + 1];
          records[i + 1] = recordToMoveDown;
          records[i] = recordToSwap;
        }
      }

      if (recordToDelete != null)
        records.Remove(recordToDelete);

      EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("+", GUILayout.MinWidth(SmallButtonWidth), GUILayout.MaxWidth(SmallButtonWidth)))
        {
          string newRecordKey = "NewRecord";
          while (records.Any(x => x.Key.Equals(newRecordKey)))
            newRecordKey += "+";
          records.Add(new ProjectPrefs.Record() { Key = newRecordKey });
        }
        if (GUILayout.Button("Sort"))
        {
          records.Sort((a, b) => a.Key.CompareTo(b.Key));
          records.ForEach(x => x.Sort());
        }
        if (GUILayout.Button("Save"))
        {
          AssetDatabase.SaveAssets();
        }
      EditorGUILayout.EndHorizontal();

      serializedObject.Update();

      if (EditorGUI.EndChangeCheck())
      {
        EditorUtility.SetDirty(prefs);
      }
    }
  }
}
