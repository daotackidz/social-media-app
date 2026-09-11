namespace Social.Common.Constants
{
    /// <summary>
    /// Stable, machine-readable failure reasons returned in every non-2xx
    /// <c>ApiResponse&lt;T&gt;.ErrorCode</c>. The frontend should branch on this
    /// field for logic (e.g. "show a resend-OTP link"), never on
    /// <c>Message</c> — that text is user-facing and changes with language.
    ///
    /// Serialized as its member name (see Program.cs's JsonStringEnumConverter),
    /// so the TypeScript mirror in Frontend/WebFe/src/app/core/api/error-code.ts
    /// MUST use the exact same names. Keep the two in sync whenever this enum changes.
    /// </summary>
    public enum ErrorCode
    {
        /// <summary>Model-binding / DataAnnotations validation failed (automatic 400 from [ApiController]).</summary>
        VALIDATION_ERROR,

        /// <summary>Email/password did not match any active account.</summary>
        INVALID_CREDENTIALS,

        /// <summary>Endpoint requires authentication and none (or an invalid one) was supplied.</summary>
        UNAUTHORIZED,

        /// <summary>Registration attempted with an email that already has an account.</summary>
        EMAIL_EXISTS,

        /// <summary>Registration (or a profile lookup) used a username that is already taken.</summary>
        USERNAME_EXISTS,

        /// <summary>No user found for the given @username (e.g. viewing a profile).</summary>
        USERNAME_NOT_FOUND,

        /// <summary>No account exists for the given email (e.g. forgot-password).</summary>
        EMAIL_NOT_FOUND,

        /// <summary>No user record found for an otherwise-valid identifier.</summary>
        USER_NOT_FOUND,

        /// <summary>No profile record found for the current user.</summary>
        PROFILE_NOT_FOUND,

        /// <summary>No pending (not-yet-verified) registration found for the email.</summary>
        PENDING_REGISTRATION_NOT_FOUND,

        /// <summary>The OTP code has passed its expiry time.</summary>
        OTP_EXPIRED,

        /// <summary>The OTP code does not match what was issued.</summary>
        OTP_INVALID,

        /// <summary>An OTP was requested again before the resend cooldown elapsed.</summary>
        OTP_COOLDOWN,

        /// <summary>NewPassword and ConfirmPassword did not match.</summary>
        PASSWORD_MISMATCH,

        /// <summary>A required file was not supplied.</summary>
        FILE_REQUIRED,

        /// <summary>A required storage container name was not supplied.</summary>
        CONTAINER_REQUIRED,

        /// <summary>The requested file does not exist (or was already deleted).</summary>
        FILE_NOT_FOUND,

        /// <summary>The requested search-history entry does not exist (or was already deleted) for the current user.</summary>
        SEARCH_HISTORY_NOT_FOUND,

        /// <summary>Unhandled server-side exception.</summary>
        INTERNAL_ERROR
    }
}
