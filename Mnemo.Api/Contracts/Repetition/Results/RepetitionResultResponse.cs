using Mnemo.Contracts.Vocabulary;

namespace Mnemo.Contracts.Repetition.Results
{
    public class RepetitionResultResponse
    {
        public int Correct { get; set; }
        public int Total { get; set; }
        public int Percent { get; set; }
        public int TotalTimeMilliseconds { get; set; }
        public TaskResultResponse[]? TaskResults { get; set; }
    }
}
