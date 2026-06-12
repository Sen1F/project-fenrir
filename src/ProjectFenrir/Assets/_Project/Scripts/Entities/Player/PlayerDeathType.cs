namespace Fenrir.Entities.Player
{
    /// <summary>
    /// Classification of how the player died. Set by PlayerHealth.EmitDeathEvent,
    /// read by DeathMessageProvider to select the contextual death message.
    /// </summary>
    public enum PlayerDeathType
    {
        None,         // not dead yet
        Ambush,       // died within 5s of combat start
        PatternFail,  // 3rd+ death vs same enemy type
        Sacrifice,    // attacked at low HP, minor damage dealt or group fight
        Reckless,     // took 3+ unblocked hits with no dodge
        Overwhelmed   // fallback
    }
}
