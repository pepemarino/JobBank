namespace JobBank.Exceptions
{
    [Serializable]
    internal class DataIntegrityException : Exception
    {
        public DataIntegrityException()
        {
        }

        public DataIntegrityException(string? message) : base(message)
        {
        }

        public DataIntegrityException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}