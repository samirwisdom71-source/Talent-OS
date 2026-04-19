namespace TalentSystem.Domain.Enums;

/// <summary>
/// Discrete 9-box cell codes (1–9). Stored as a byte for stable APIs, reporting, and indexes.
/// String codes like "P1" were avoided to prevent drift between display and persisted identity.
/// </summary>
public enum NineBoxCode : byte
{
    Box1 = 1,

    Box2 = 2,

    Box3 = 3,

    Box4 = 4,

    Box5 = 5,

    Box6 = 6,

    Box7 = 7,

    Box8 = 8,

    Box9 = 9
}
