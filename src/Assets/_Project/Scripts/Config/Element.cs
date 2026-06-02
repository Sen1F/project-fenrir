namespace Fenrir.Config
{
    public enum Element
    {
        // Common
        Fire,
        Water,
        Earth,
        Air,

        // Rare
        Lightning,
        Metal,
        Ice,
        Nature,

        // Very Rare
        Light,
        Darkness,
        Shadow,

        // Supreme
        Space,
        Time,
        Life,
        Death,

        None
    }

    public enum ElementTier
    {
        Common,
        Rare,
        VeryRare,
        Supreme
    }

    public static class ElementExtensions
    {
        public static ElementTier GetTier(this Element element) => element switch
        {
            Element.Fire or Element.Water or Element.Earth or Element.Air
                => ElementTier.Common,
            Element.Lightning or Element.Metal or Element.Ice or Element.Nature
                => ElementTier.Rare,
            Element.Light or Element.Darkness or Element.Shadow
                => ElementTier.VeryRare,
            Element.Space or Element.Time or Element.Life or Element.Death
                => ElementTier.Supreme,
            _ => ElementTier.Common
        };

        /// <summary>
        /// Canonical elemental opposites used by the resistance system.
        /// Fire↔Water, Earth↔Air, Lightning↔Metal, Ice↔Nature,
        /// Light↔Darkness, Shadow↔Light, Space↔Time, Life↔Death.
        /// </summary>
        public static bool AreOpposites(Element a, Element b)
        {
            return (a, b) switch
            {
                (Element.Fire,      Element.Water)     => true,
                (Element.Water,     Element.Fire)      => true,
                (Element.Earth,     Element.Air)       => true,
                (Element.Air,       Element.Earth)     => true,
                (Element.Lightning, Element.Metal)     => true,
                (Element.Metal,     Element.Lightning) => true,
                (Element.Ice,       Element.Nature)    => true,
                (Element.Nature,    Element.Ice)       => true,
                (Element.Light,     Element.Darkness)  => true,
                (Element.Darkness,  Element.Light)     => true,
                (Element.Shadow,    Element.Light)     => true,
                (Element.Space,     Element.Time)      => true,
                (Element.Time,      Element.Space)     => true,
                (Element.Life,      Element.Death)     => true,
                (Element.Death,     Element.Life)      => true,
                _                                      => false
            };
        }
    }
}
