namespace Helper
{
    public partial class SsoConfig : BaseHttpClientConfig
    {
        public string UserUrl { get; set; }
        public string TokenUrl { get; set; }
        public string AdminClientId { get; set; }
        public string AdminClientSecret { get; set; }
        public string AdminUsername { get; set; }
        public string AdminPassword { get; set; }
        public string PositronClientId { get; set; }
        public string PositronClientSecret { get; set; }
        public string UserInfoUrl { get; set; }
        public string PublicKey { get; set; }
        public string MetadataAddress { get; set; }
        public string Authority { get; set; }
        public string ValidIssuer { get; set; }
        public string ResetPasswordUrl { get; set; }
        public string GetClientUrl { get; set; }
        public string ClientRoleUrl { get; set; }
        public string AssignRoleUrl { get; set; }
    }
}
