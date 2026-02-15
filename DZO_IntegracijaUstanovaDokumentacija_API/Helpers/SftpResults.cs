namespace DZO_IntegracijaUstanovaDokumentacija_API.Helpers
{
    public enum SftpFolderCreateOutcome
    {
        Created = 1,
        AlreadyExists = 2
    }

    public sealed record SftpFolderCreateResult(SftpFolderCreateOutcome Outcome)
    {
        public bool Succeeded => Outcome == SftpFolderCreateOutcome.Created;
    }

    public sealed record SftpMoveResult(bool Succeeded, string? ErrorCode = null, string? ErrorMessage = null)
    {
        public static SftpMoveResult Ok() => new(true);
        public static SftpMoveResult Fail(string code, string message) => new(false, code, message);
    }

    public sealed record SftpTransferResult(bool Succeeded, string? ErrorMessage = null)
    {
        public static SftpTransferResult Ok() => new(true);
        public static SftpTransferResult Fail(string message) => new(false, message);
    }
}
