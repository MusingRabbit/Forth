namespace Assets.Scripts
{
    public enum Team
    {
        None = 0,
        Red = 1,
        Blue = 2
    }

    public enum ActorHealthState
    {
        Live = 0,
        Dying,
        Dead
    }

    public enum PlayerState
    {
        Spectating = 0,
        Playing
    }

    public enum MatchType
    {
        Deathmatch = 0,
        TeamDeathmatch,
        CaptureTheFlag
    }

    public enum MatchState
    {
        PendingStart,
        Running,
        Ended
    }

    public enum GameState
    {
        MainMenu = 0,
        Loading,
        InGame
    }

    public enum SelectedWeapon
    {
        None,
        Main,
        Sidearm,
        Pack
    }

    public enum WeaponType
    {
        None = 0,
        Pistol,
        AssaultRifle,
        LaserPistol,
        LaserRifle,
        PlasmaRifle,
        RocketLauncher,
    }

    public enum MessageType
    {
        Info,
        Warning,
        Error,
        PlayerKilled,
        PlayerSpawned,

    }
}
