using UnityEngine;
using UnityEditor;

public static class LeaderboardResetTool
{
    private const string LeaderboardPrefKey = "ArcheryVR_Top3_Leaderboard";

    /// <summary>
    /// Xóa riêng dữ liệu bảng kỷ lục Top 3 trong PlayerPrefs trên máy kiểm thử hiện tại.
    /// Không xóa các cài đặt hoặc dữ liệu PlayerPrefs khác.
    /// </summary>
    [MenuItem("ArcheryVR/Testing/Reset Top 3 Records", false, 10)]
    public static void ResetTop3Records()
    {
        // 1. Xóa riêng khóa PlayerPrefs lưu bảng thành tích Top 3
        if (PlayerPrefs.HasKey(LeaderboardPrefKey))
        {
            PlayerPrefs.DeleteKey(LeaderboardPrefKey);
            PlayerPrefs.Save();
            Debug.Log("<color=green>[LeaderboardResetTool] Đã xóa thành công khóa PlayerPrefs: " + LeaderboardPrefKey + "</color>");
        }
        else
        {
            Debug.Log("<color=yellow>[LeaderboardResetTool] Khóa PlayerPrefs không tồn tại hoặc đã được xóa trước đó: " + LeaderboardPrefKey + "</color>");
        }

        // 2. Làm mới trạng thái ScoreManager và cập nhật UI trong Scene nếu đang mở
        var scoreManager = Object.FindAnyObjectByType<ScoreManager>();
        if (scoreManager != null)
        {
            Undo.RecordObject(scoreManager, "Reset Top 3 Records");
            scoreManager.ClearLeaderboard();
            EditorUtility.SetDirty(scoreManager);

            var scene = scoreManager.gameObject.scene;
            if (scene.isLoaded)
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
            }

            Debug.Log("<color=green>[LeaderboardResetTool] Đã cập nhật UI bảng thành tích về trạng thái: CHƯA CÓ KỶ LỤC.</color>");
        }
        else
        {
            Debug.Log("[LeaderboardResetTool] Không tìm thấy ScoreManager trong scene hiện tại.");
        }
    }

    /// <summary>
    /// Xem dữ liệu bảng kỷ lục hiện tại trong PlayerPrefs và trong ScoreManager.
    /// </summary>
    [MenuItem("ArcheryVR/Testing/View Top 3 Records", false, 11)]
    public static void ViewTop3Records()
    {
        if (PlayerPrefs.HasKey(LeaderboardPrefKey))
        {
            string json = PlayerPrefs.GetString(LeaderboardPrefKey, "");
            Debug.Log("<color=cyan>[LeaderboardResetTool] PlayerPrefs JSON: " + json + "</color>");
        }
        else
        {
            Debug.Log("<color=yellow>[LeaderboardResetTool] PlayerPrefs: Khóa " + LeaderboardPrefKey + " CHƯA CÓ DỮ LIỆU (Trống).</color>");
        }

        var scoreManager = Object.FindAnyObjectByType<ScoreManager>();
        if (scoreManager != null)
        {
            var records = scoreManager.TopRecords;
            string details = "ScoreManager TopRecords (Số lượng = " + records.Count + "):";
            if (records.Count == 0)
            {
                details += " [CHƯA CÓ KỶ LỤC]";
            }
            else
            {
                for (int i = 0; i < records.Count; i++)
                {
                    details += string.Format("\n  #{0}: {1} điểm | Thời gian: {2}", i + 1, records[i].score, ScoreManager.FormatTime(records[i].timeUsed));
                }
            }
            Debug.Log("<color=cyan>[LeaderboardResetTool] " + details + "</color>");
        }
    }
}

