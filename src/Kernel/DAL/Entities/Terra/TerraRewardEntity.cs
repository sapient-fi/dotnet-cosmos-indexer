using System.Diagnostics.CodeAnalysis;
using ServiceStack.DataAnnotations;

namespace Sapient.Kernel.DAL.Entities.Terra
{
    public class TerraRewardEntity
    {
        [PrimaryKey] 
        public long Id { get; set; }

        [ForeignKey(typeof(TerraRawTransactionEntity))]
        [Required]
        public long TransactionId { get; set; }

        [Index] 
        public string Wallet { get; set; }

        [Index] 
        public string FromContract { get; set; }

        public decimal Amount { get; set; }

        public decimal? AmountUstAtClaim { get; set; }

        public decimal? AmountUstNow { get; set; }
        public string Denominator { get; set; }

        public TerraRewardType RewardType { get; set; }
        [NotNull] public DateTimeOffset CreatedAt { get; set; }

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
        /// Rewards paid out from Pylon Pools (UST deposit => token rewards)
        /// </summary>
        GatewayPool,

        /// <summary>
        /// LP farming reward
        /// </summary>
        LpFarm
    }
}