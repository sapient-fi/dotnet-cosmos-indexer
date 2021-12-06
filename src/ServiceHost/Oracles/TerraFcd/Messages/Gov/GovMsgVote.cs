namespace Invacoil.ServiceRole.TerraMoney.Oracles.TerraFcd.Messages.Gov
{
    public record GovMsgVote : IMsg
    {
        public string Type { get; set; }
    }
}