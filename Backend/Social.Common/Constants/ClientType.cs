namespace Social.Common.Constants
{
    /// <summary>
    /// Which kind of app a login/session came from. Sessions are tracked per
    /// (UserId, ClientType) — see UserSessions — so logging in again on the same
    /// ClientType kicks out the previous session there, while a different
    /// ClientType gets its own independent session. Today only Web exists; Mobile
    /// is reserved for the future native app so it can stay signed in alongside
    /// a web session instead of fighting it for the single-session slot.
    ///
    /// Serialized as its member name (see Program.cs's JsonStringEnumConverter),
    /// so the TypeScript mirror in Frontend/WebFe/src/app/services/auth.service.ts
    /// MUST use the exact same names. Keep the two in sync whenever this enum changes.
    /// </summary>
    public enum ClientType
    {
        Web,
        Mobile
    }
}
