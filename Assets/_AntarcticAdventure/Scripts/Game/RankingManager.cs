using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

[Serializable]
public class RankingEntry
{
    public string playerName;
    public int score;
    public int distanceMeter;
    public int itemScore;
    public string dateText;
}

[Serializable]
public class RankingSaveData
{
    public List<RankingEntry> entries = new List<RankingEntry>();
}

public class RankingManager : MonoBehaviour
{
    public static RankingManager Instance { get; private set; }

    private const string OldRankingPlayerPrefsKey = "AntarcticAdventure_Ranking";
    private const string LastPlayerNameKey = "AntarcticAdventure_LastPlayerName";

    [Header("Ranking Storage")]
    [SerializeField] private int maxStoredEntryCount = 1000;
    [SerializeField] private string rankingJsonFileName = "AntarcticAdventure_Ranking.json";
    [SerializeField] private string rankingCsvFileName = "AntarcticAdventure_Ranking.csv";

    [Header("Name")]
    [SerializeField] private int maxNameLength = 10;
    [SerializeField] private string defaultPlayerName = "PLAYER";

    [Header("Debug")]
    [SerializeField] private bool showSavePathLog = true;

    public IReadOnlyList<RankingEntry> Entries => rankingData.entries;
    public string LastPlayerName { get; private set; }

    public string JsonSavePath => Path.Combine(Application.persistentDataPath, rankingJsonFileName);
    public string CsvSavePath => Path.Combine(Application.persistentDataPath, rankingCsvFileName);

    private RankingSaveData rankingData = new RankingSaveData();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        LoadRanking();
        LoadLastPlayerName();

        if (showSavePathLog)
        {
            Debug.Log($"[Ranking] JSON Save Path: {JsonSavePath}");
            Debug.Log($"[Ranking] CSV Save Path: {CsvSavePath}");
        }
    }

    public void RegisterScore(string playerName, int score, int distanceMeter, int itemScore)
    {
        string safeName = SanitizeName(playerName);

        RankingEntry entry = new RankingEntry
        {
            playerName = safeName,
            score = score,
            distanceMeter = distanceMeter,
            itemScore = itemScore,
            dateText = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };

        rankingData.entries.Add(entry);
        SortRanking();

        TrimStoredEntriesIfNeeded();

        LastPlayerName = safeName;
        PlayerPrefs.SetString(LastPlayerNameKey, LastPlayerName);
        PlayerPrefs.Save();

        SaveRanking();

        Debug.Log($"[Ranking] Registered: {safeName}, Score: {score}, Total Saved: {rankingData.entries.Count}");
    }

    public string GetRankingText(int displayCount)
    {
        if (rankingData.entries == null || rankingData.entries.Count == 0)
            return "NO RANKING DATA";

        SortRanking();

        StringBuilder builder = new StringBuilder();

        int count = Mathf.Min(displayCount, rankingData.entries.Count);

        for (int i = 0; i < count; i++)
        {
            RankingEntry entry = rankingData.entries[i];

            builder.AppendLine(
                $"{i + 1}. {entry.playerName}  SCORE {entry.score:000000}  DIST {entry.distanceMeter:0000}m"
            );
        }

        return builder.ToString();
    }

    private void SortRanking()
    {
        rankingData.entries.Sort(CompareRankingEntry);
    }

    private int CompareRankingEntry(RankingEntry a, RankingEntry b)
    {
        int scoreCompare = b.score.CompareTo(a.score);

        if (scoreCompare != 0)
            return scoreCompare;

        int distanceCompare = b.distanceMeter.CompareTo(a.distanceMeter);

        if (distanceCompare != 0)
            return distanceCompare;

        return string.Compare(a.dateText, b.dateText, StringComparison.Ordinal);
    }

    private void TrimStoredEntriesIfNeeded()
    {
        if (maxStoredEntryCount <= 0)
            return;

        while (rankingData.entries.Count > maxStoredEntryCount)
        {
            rankingData.entries.RemoveAt(rankingData.entries.Count - 1);
        }
    }

    private string SanitizeName(string playerName)
    {
        if (string.IsNullOrWhiteSpace(playerName))
            playerName = defaultPlayerName;

        playerName = playerName.Trim();
        playerName = playerName.Replace("\n", "");
        playerName = playerName.Replace("\r", "");
        playerName = playerName.Replace("\t", " ");

        if (playerName.Length > maxNameLength)
            playerName = playerName.Substring(0, maxNameLength);

        return playerName;
    }

    private void SaveRanking()
    {
        SaveJsonFile();
        SaveCsvFile();
    }

    private void SaveJsonFile()
    {
        try
        {
            string json = JsonUtility.ToJson(rankingData, true);
            File.WriteAllText(JsonSavePath, json, Encoding.UTF8);
        }
        catch (Exception exception)
        {
            Debug.LogError($"[Ranking] JSON 저장 실패: {exception.Message}", this);
        }
    }

    private void SaveCsvFile()
    {
        try
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine("Rank,PlayerName,Score,DistanceMeter,ItemScore,Date");

            for (int i = 0; i < rankingData.entries.Count; i++)
            {
                RankingEntry entry = rankingData.entries[i];

                builder.AppendLine(
                    $"{i + 1}," +
                    $"{EscapeCsv(entry.playerName)}," +
                    $"{entry.score}," +
                    $"{entry.distanceMeter}," +
                    $"{entry.itemScore}," +
                    $"{EscapeCsv(entry.dateText)}"
                );
            }

            File.WriteAllText(CsvSavePath, builder.ToString(), Encoding.UTF8);
        }
        catch (Exception exception)
        {
            Debug.LogError($"[Ranking] CSV 저장 실패: {exception.Message}", this);
        }
    }

    private string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value))
            return "";

        bool needsQuote =
            value.Contains(",") ||
            value.Contains("\"") ||
            value.Contains("\n") ||
            value.Contains("\r");

        value = value.Replace("\"", "\"\"");

        return needsQuote
            ? $"\"{value}\""
            : value;
    }

    private void LoadRanking()
    {
        if (File.Exists(JsonSavePath))
        {
            LoadJsonFile();
            return;
        }

        TryMigrateOldPlayerPrefsRanking();
    }

    private void LoadJsonFile()
    {
        try
        {
            string json = File.ReadAllText(JsonSavePath, Encoding.UTF8);
            rankingData = JsonUtility.FromJson<RankingSaveData>(json);

            if (rankingData == null || rankingData.entries == null)
                rankingData = new RankingSaveData();

            SortRanking();
        }
        catch (Exception exception)
        {
            Debug.LogError($"[Ranking] JSON 로드 실패: {exception.Message}", this);
            rankingData = new RankingSaveData();
        }
    }

    private void TryMigrateOldPlayerPrefsRanking()
    {
        string oldJson = PlayerPrefs.GetString(OldRankingPlayerPrefsKey, "");

        if (string.IsNullOrEmpty(oldJson))
        {
            rankingData = new RankingSaveData();
            return;
        }

        try
        {
            rankingData = JsonUtility.FromJson<RankingSaveData>(oldJson);

            if (rankingData == null || rankingData.entries == null)
                rankingData = new RankingSaveData();

            SortRanking();
            SaveRanking();

            Debug.Log("[Ranking] 기존 PlayerPrefs 랭킹 데이터를 파일 저장 방식으로 이전했습니다.");
        }
        catch (Exception exception)
        {
            Debug.LogError($"[Ranking] 기존 PlayerPrefs 랭킹 이전 실패: {exception.Message}", this);
            rankingData = new RankingSaveData();
        }
    }

    private void LoadLastPlayerName()
    {
        LastPlayerName = PlayerPrefs.GetString(LastPlayerNameKey, defaultPlayerName);
    }

    [ContextMenu("Open Save Folder Log")]
    public void LogSaveFolder()
    {
        Debug.Log($"[Ranking] Save Folder: {Application.persistentDataPath}", this);
    }

    [ContextMenu("Export CSV Now")]
    public void ExportCsvNow()
    {
        SortRanking();
        SaveCsvFile();

        Debug.Log($"[Ranking] CSV exported: {CsvSavePath}", this);
    }

    [ContextMenu("Reset Ranking")]
    public void ResetRanking()
    {
        PlayerPrefs.DeleteKey(OldRankingPlayerPrefsKey);
        PlayerPrefs.DeleteKey(LastPlayerNameKey);
        PlayerPrefs.Save();

        DeleteFileIfExists(JsonSavePath);
        DeleteFileIfExists(CsvSavePath);

        rankingData = new RankingSaveData();
        LastPlayerName = defaultPlayerName;

        Debug.Log("[Ranking] Ranking reset.");
    }

    private void DeleteFileIfExists(string path)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch (Exception exception)
        {
            Debug.LogError($"[Ranking] 파일 삭제 실패: {path}\n{exception.Message}", this);
        }
    }
}