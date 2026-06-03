namespace Mnemo.Contracts.Repetition.Request
{
    public class SubmitTaskRequest
    {
        public string UserAnswer { get; set; }
        public int ElapsedTimeMilliseconds { get; set; }
    }
}
