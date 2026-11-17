using UnityEngine;

public static class GameSession
{
    public static bool AutoStart;
    public static bool GameOver;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetState()
    {
        AutoStart = false;
        GameOver = false;
    }
}
