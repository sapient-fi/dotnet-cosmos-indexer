using ServiceStack.DataAnnotations;

namespace SapientFi.Kernel.DAL.Entities.Terra
{
    public class TerraRewardEntity
    {
        [PrimaryKey] 
        public long Id { get; set; }

        [ForeignKey(typeof(TerraRawTransactionEntity))]
        [Required]
        public long TransactionId { get; set; }

        [Index] 
        public string Wallet { get; set; } = string.Empty;

        [Index] 
        public string FromContract { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public decimal? AmountUstAtClaim { get; set; }

        public decimal? AmountUstNow { get; set; }
        public string Denominator { get; set; } = string.Empty;

        public TerraRewardType RewardType { get; set; }
        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }
    }

    public enum TerraRewardType
    {
        /// <summary>
        /// A token airdrop
        /// </summary>
        Airdrop,

        /// <summary>
        /// Native token rewards for staking 
        /// </summary>
        StakingReward,

        /// <summary>
        /// LP farming reward
        /// </summary>
        LpFarm
    }
}