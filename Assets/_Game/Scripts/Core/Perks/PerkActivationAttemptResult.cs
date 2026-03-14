namespace PocketSquire.Arena.Core.Perks
{
    public class PerkActivationAttemptResult
    {
        public bool Succeeded { get; set; }
        public string FailureReason { get; set; } = string.Empty;

        public static PerkActivationAttemptResult Success()
        {
            return new PerkActivationAttemptResult
            {
                Succeeded = true
            };
        }

        public static PerkActivationAttemptResult Failure(string reason)
        {
            return new PerkActivationAttemptResult
            {
                Succeeded = false,
                FailureReason = reason ?? string.Empty
            };
        }
    }
}
