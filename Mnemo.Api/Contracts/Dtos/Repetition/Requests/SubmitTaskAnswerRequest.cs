namespace Mnemo.Contracts.Dtos.Repetition.Requests
{
    public class SubmitTaskAnswerRequest
    {
        public string UserAnswer { get; set; }
        public int ElapsedTimeMilliseconds { get; set; }
    }
}
