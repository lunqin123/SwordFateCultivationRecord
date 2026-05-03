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
        //                                 Speed  Chance  ReqProg  Stam  HP   CmbMult  Cost(灵石)
        //                                 Speed  Chance  ReqProg  Stam  HP   CmbMult  Cost
        new RealmInfo(0,  0,     0,    120,  0,    1.0,  0),      // Mortal
        new RealmInfo(6,  0.60,  180,  120,  20,   1.5,  50),     // QiRefining
        new RealmInfo(5,  0.50,  350,  150,  50,   2.5,  150),    // Foundation
        new RealmInfo(4,  0.35,  700,  200,  100,  4.0,  500),    // CoreFormation
        new RealmInfo(3,  0.25,  1400, 300,  200,  6.0,  2000),   // NascentSoul
        new RealmInfo(2,  0.15,  2500, 500,  400,  9.0,  6000),   // SpiritTransformation
        new RealmInfo(2,  0.10,  4500, 800,  800,  15.0, 18000),  // Tribulation
        new RealmInfo(1,  0.08,  7000, 1200, 1500, 25.0, 50000),  // GreatAscension
    };

    public static RealmInfo GetInfo(CultivationRealm realm) => _realms[(int)realm];

    public static int MaxRealmLayer(CultivationRealm realm) =>
        realm switch { CultivationRealm.Mortal => 0, CultivationRealm.GreatAscension => 1, _ => 9 };

    public static bool IsMaxRealm(CultivationRealm realm, int layer)
    {
        if (realm == CultivationRealm.GreatAscension) return true;
        return false; // Mortal always promotes to QiRefining
    }
}
