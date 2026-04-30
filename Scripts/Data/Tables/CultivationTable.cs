namespace SwordFateCultivationRecord;

public static class CultivationTable
{
    public record RealmInfo(
        int BaseCultivationSpeed, // daily progress (0-100 scale)
        double BreakthroughChance, // base chance 0.0-1.0
        int BreakthroughRequiredProgress, // accumulated progress needed
        int MaxStamina,
        int HpBonus,
        double CombatPowerMultiplier,
        int BreakthroughCost // spirit stones consumed per attempt
    );

    private static readonly RealmInfo[] _realms = new[]
    {
        new RealmInfo(0, 0, 0, 120, 0, 1.0, 0),                // Mortal
        new RealmInfo(10, 0.60, 100, 120, 20, 1.5, 50),         // QiRefining
        new RealmInfo(8, 0.45, 200, 150, 50, 2.5, 200),         // Foundation
        new RealmInfo(6, 0.30, 400, 200, 100, 4.0, 800),        // CoreFormation
        new RealmInfo(5, 0.20, 800, 300, 200, 6.0, 3000),       // NascentSoul
        new RealmInfo(4, 0.12, 1500, 500, 400, 9.0, 10000),     // SpiritTransformation
        new RealmInfo(3, 0.05, 3000, 800, 800, 15.0, 50000),    // Tribulation
        new RealmInfo(2, 0.02, 5000, 1200, 1500, 25.0, 200000), // GreatAscension
    };

    public static RealmInfo GetInfo(CultivationRealm realm) => _realms[(int)realm];

    public static int MaxRealmLayer(CultivationRealm realm) =>
        realm == CultivationRealm.Mortal || realm == CultivationRealm.GreatAscension ? 1 : 9;

    public static bool IsMaxRealm(CultivationRealm realm, int layer)
    {
        if (realm == CultivationRealm.GreatAscension) return true;
        return realm == CultivationRealm.Mortal && layer >= 1;
    }
}
