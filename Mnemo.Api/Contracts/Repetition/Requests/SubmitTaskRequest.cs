namespace Mnemo.Contracts.Repetition.Requests
{
    public class SubmitTaskRequest
    {
        public string UserAnswer { get; set; }
        public int ElapsedTimeMilliseconds { get; set; }
    }
}
