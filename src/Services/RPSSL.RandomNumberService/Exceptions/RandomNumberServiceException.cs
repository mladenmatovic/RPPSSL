namespace RPSSL.RandomNumberService.Exceptions
{
    public class RandomNumberServiceException : Exception
    {
        public RandomNumberServiceException(string message) : base(message)
        {
        }

        public RandomNumberServiceException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
