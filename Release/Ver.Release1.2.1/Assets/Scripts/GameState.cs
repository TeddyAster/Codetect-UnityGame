using System;

public enum GameState
{
    Intro,
    Playing,
    Dead,
    Win // 新增一个胜利状态
}

public static class GameStateManager
{
    public static GameState GameState { get; set; }

    static GameStateManager()
    {
        GameState = GameState.Intro;  // 游戏开始时处于 Intro 状态
    }

    public static void SetGameState(GameState newState)
    {
        GameState = newState;
    }
}

