namespace RPSSL.RandomNumberService.Exceptions
{
    public class RandomNumberApiException : Exception
    {
        public RandomNumberApiException(string message) : base(message)
        {
        }

        public RandomNumberApiException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
