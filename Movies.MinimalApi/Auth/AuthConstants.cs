namespace Movies.MinimalApi.Auth;

public static class AuthConstants
{
    public const string AdminUserPolicyName = "Admin";
    public const string AdminUserClaimName = "admin";
    
    public const string TrustedMemberPolicyName = "admin";
    public const string TrustedMemberClaimName = "admin";

    public const string ApiKeyHeaderName = "x-api-key";
}