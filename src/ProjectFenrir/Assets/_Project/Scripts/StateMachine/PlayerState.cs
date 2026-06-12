namespace Fenrir.StateMachine
{
    public enum PlayerState
    {
        Idle,
        Moving,
        Dodging,
        Attacking,
        Blocking,
        Staggered,
        Dead,
        Interacting,
        InCutscene,     // locked during cutscene / dialogue / evolution
        InEvolution,
    }
}
