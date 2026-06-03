using Fenrir.Combat;
using Fenrir.Config;
using Fenrir.Core;
using Fenrir.Entities.Player;
using UnityEngine;

namespace Fenrir.UI
{
    /// <summary>
    /// Returns contextual death messages based on how the player died.
    /// Messages are dramatic, second-person, and carry a short verdict line.
    /// Two-line format: dramatic body + one-word or short verdict.
    /// </summary>
    public static class DeathMessageProvider
    {
        public struct DeathMessage
        {
            public string Body;
            public string Verdict;
        }

        // ── Message pools ─────────────────────────────────────────────────────

        private static readonly DeathMessage[] _ambush =
        {
            new() { Body = "You never saw it coming. The forest was faster.", Verdict = "Unfair." },
            new() { Body = "It was waiting. You weren't. Sometimes the timing is simply wrong.", Verdict = "Ambushed." },
            new() { Body = "The shadows moved before you did. That's not on you.", Verdict = "Careful next time." },
        };

        private static readonly DeathMessage[] _patternFail =
        {
            new() { Body = "Same enemy. Same result. Three times now. This is starting to look like a choice.", Verdict = "Questionable." },
            new() { Body = "It does the same thing every time. You noticed. You went in anyway.", Verdict = "Bold." },
            new() { Body = "Again. There is a word for that. Several of them. None flattering.", Verdict = "Stubborn." },
            new() { Body = "At some point this stops being misfortune and starts being commitment.", Verdict = "To what, exactly." },
        };

        private static readonly DeathMessage[] _reckless =
        {
            new() { Body = "You walked through every swing without stepping aside once. They appreciated the convenience.", Verdict = "Reckless." },
            new() { Body = "Every hit taken. No attempt to dodge. That is a philosophy. Not a good one.", Verdict = "Bold. Costly." },
            new() { Body = "You took everything they had. Chin first. Repeatedly. On purpose, apparently.", Verdict = "Questionable." },
            new() { Body = "There was a gap between you and the attack. You closed it yourself.", Verdict = "Remarkable." },
        };

        private static readonly DeathMessage[] _sacrificeGroup =
        {
            new() { Body = "You entered a wolf's den while bleeding out. They did not waste the meal.", Verdict = "Questionable." },
            new() { Body = "Three of them. One of you. Half your blood already on the ground. The rest was math.", Verdict = "Poor odds." },
            new() { Body = "They were already watching you when you came in. Wounded prey. You obliged.", Verdict = "Brave. Briefly." },
            new() { Body = "A den of wolves, a body already failing, a decision made anyway. That is something.", Verdict = "Something." },
        };

        private static readonly DeathMessage[] _sacrificeMinorDamage =
        {
            new() { Body = "You kept swinging. Nothing you threw could reach them. They had no such difficulty.", Verdict = "The gap showed." },
            new() { Body = "Your attacks grazed them. Theirs did not return the courtesy.", Verdict = "Outmatched." },
            new() { Body = "Brave. The kind of brave that does not account for effectiveness.", Verdict = "Wise? Questionable." },
            new() { Body = "You fought hard. The distance between effort and result was unfortunate.", Verdict = "Try differently." },
        };

        private static readonly DeathMessage[] _overwhelmed =
        {
            new() { Body = "The numbers were always against you. No shame in that. Well. A little.", Verdict = "Overwhelmed." },
            new() { Body = "They came from everywhere at once. That is not a failure. That is arithmetic.", Verdict = "The count was wrong." },
            new() { Body = "More than you could handle. Most things have a limit.", Verdict = "Found yours." },
        };

        // ── Public API ────────────────────────────────────────────────────────

        public static DeathMessage Get(PlayerDeathType deathType)
        {
            return deathType switch
            {
                PlayerDeathType.Ambush      => Pick(_ambush),
                PlayerDeathType.PatternFail => Pick(_patternFail),
                PlayerDeathType.Reckless    => Pick(_reckless),
                PlayerDeathType.Sacrifice   => PickSacrifice(),
                _                           => Pick(_overwhelmed),
            };
        }

        // ── Internals ─────────────────────────────────────────────────────────

        private static DeathMessage PickSacrifice()
        {
            // Re-read CombatContext to distinguish group vs. minor-damage sacrifice
            if (ServiceLocator.TryGet<CombatContext>(out CombatContext ctx))
            {
                bool groupFight = ctx.GetAwareEnemyCount() >= GameConfig.SacrificeGroupEnemyMin;
                return groupFight ? Pick(_sacrificeGroup) : Pick(_sacrificeMinorDamage);
            }
            return Pick(_sacrificeGroup); // fallback
        }

        private static DeathMessage Pick(DeathMessage[] pool)
        {
            return pool[Random.Range(0, pool.Length)];
        }
    }
}
