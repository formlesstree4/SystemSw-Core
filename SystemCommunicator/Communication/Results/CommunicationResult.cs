namespace SystemCommunicator.Communication.Results
{
    public sealed class CommunicationResult : ICommunicationResult
    {
        public bool Result { get; private set; }

        public ResultCode Status { get; private set; }

        public string Message { get; private set; }


        public static ICommunicationResult Ok(string message = "")
            => new CommunicationResult()
            {
                Message = message,
                Status = ResultCode.Success,
                Result = true
            };

        public static ICommunicationResult Error(string message, ResultCode code, bool result)
            => new CommunicationResult()
            {
                Message = message,
                Status = code,
                Result = result
            };


    }
}
