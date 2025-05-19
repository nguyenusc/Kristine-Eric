using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// Keeps per‑level metrics *inside one run*, then snapshots the whole run
/// to disk when a new session starts or when the app quits.
/// </summary>
public static class MetricsTracker
{
    // ---------- internal data ------------------------------------------------

    private class SessionData
    {
        public readonly string timeStamp;                       // nice header
        public readonly Dictionary<string, LevelMetrics> levels
            = new Dictionary<string, LevelMetrics>();

        public SessionData()
        {
            timeStamp = System.DateTime.Now.ToString("yyyy‑MM‑dd HH:mm:ss");
        }
    }

    private static readonly List<SessionData> sessions = new List<SessionData>();
    private static SessionData currentSession;                 // pointer
    private static string currentLevel = "";

    // One file per *application* launch — timestamp in file name
    private static readonly string filePath = Path.Combine(
        Directory.GetParent(Application.dataPath).FullName,
        "LevelMetrics_" + System.DateTime.Now.ToString("yyyy‑MM‑dd_HH‑mm‑ss") + ".txt"
    );

    // ---------- public API ---------------------------------------------------

    public static void StartLevel(string levelName)
    {
        // first call ever? start session #1
        if (currentSession == null)
            StartNewSession();

        currentLevel = levelName;

        if (!currentSession.levels.ContainsKey(levelName))
            currentSession.levels[levelName] = new LevelMetrics();
    }

    public static void IncrementMetric(string metricName)
    {
        if (string.IsNullOrEmpty(currentLevel) || currentSession == null) return;

        LevelMetrics m = currentSession.levels[currentLevel];
        switch (metricName)
        {
            case "UpdateFill": m.updateFillCalls++; break;
            case "JumpAttempt": m.jumpAttempts++; break;
            case "JumpExecuted": m.jumpsExecuted++; break;
            case "DoubleJump": m.doubleJumpCount++; break;
            case "TripleJump": m.tripleJumpCount++; break;
            case "MissedLanding": m.missedBallLandings++; break;
            case "Knockback": m.obstacleKnockbacks++; break;
            case "SuccessfulLand": m.successfulLandings++; break;
        }
    }

    public static void SetFinalTime(string timeText)
    {
        if (string.IsNullOrEmpty(currentLevel) || currentSession == null) return;
        currentSession.levels[currentLevel].finalTimeText = timeText;
    }

    /// <summary>Call this once right before re‑loading the scene.</summary>
    public static void EndCurrentSessionAndBeginNew()
    {
        FlushSessionToDisk(currentSession);   // write last run
        StartNewSession();                    // reset counters
    }

    // ---------- private helpers ---------------------------------------------

    private static void StartNewSession()
    {
        currentSession = new SessionData();
        sessions.Add(currentSession);
    }

    private static void FlushSessionToDisk(SessionData session)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"=== Session #{sessions.Count} ({session.timeStamp}) ===");

        foreach (var kvp in session.levels)
        {
            string levelName = kvp.Key;
            LevelMetrics m = kvp.Value;

            sb.AppendLine($"{levelName}:");
            sb.AppendLine($"‑ UpdateFillAmount() calls:           {m.updateFillCalls}");
            sb.AppendLine($"‑ Jump attempts (PlayerJump calls):   {m.jumpAttempts}");
            sb.AppendLine($"‑ Jumps executed (Jump coroutine):    {m.jumpsExecuted}");
            sb.AppendLine($"‑ Double jumps:                       {m.doubleJumpCount}");
            sb.AppendLine($"‑ Triple jumps:                       {m.tripleJumpCount}");
            sb.AppendLine($"‑ Missed ball landings:               {m.missedBallLandings}");
            sb.AppendLine($"‑ Obstacle knockbacks:                {m.obstacleKnockbacks}");
            sb.AppendLine($"‑ Successful landings:                {m.successfulLandings}");
            if (!string.IsNullOrEmpty(m.finalTimeText))
                sb.AppendLine($"‑ Final time:                         {m.finalTimeText}");
            sb.AppendLine();
        }
        sb.AppendLine();

        File.AppendAllText(filePath, sb.ToString());   // append, never overwrite
    }

    // Write unfinished session if player Alt‑F4s
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void RegisterQuitHandler()
    {
        Application.quitting += () =>
        {
            if (currentSession != null)
                FlushSessionToDisk(currentSession);
        };
    }
}
