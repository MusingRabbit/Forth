namespace Assets.Scripts
{
    public enum Team
    {
        None = 0,
        Red = 1,
        Blue = 2
    }

    public enum ActorHealthStatus
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
        Shotgun,
        PlasmaRifle,
        RocketLauncher,
    }

    public enum WeaponTypeSelection
    {
        Pistol = 1,
        AssaultRifle,
        LaserPistol,
        LaserRifle,
        Shotgun,
        PlasmaRifle,
        RocketLauncher,
        Random
    }

    public enum PackType
    {
        None = 0,
        Flag,
    }

    public enum MessageType
    {
        Info,
        Warning,
        Error,
        PlayerKilled,
        PlayerAttacked,
        PlayerSpawned,

    }
}
