namespace Mnemo.Contracts.Repetition.Results
{
    public class TaskResultResponse
    {
        public int Id { get; set; }
        public double Quality { get; set; }
        public bool IsCorrect { get; set; }
        public string? CorrectAnswer { get; set; }
    }
}
