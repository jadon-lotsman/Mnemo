using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mnemo.Data.Entities;
using Mnemo.Shared;

namespace Mnemo.Tests.Unit
{
    public class RepetitionTaskTests
    {
        [Fact]
        public void TextRepetitionTask_ShouldReturnPassingQuality()
        {
            var task = new TextRepetitionTask("apple", PartOfSpeech.Noun, 0, 0, ["яблоко"]);

            task.SubmitAnswer("яблоко", TimeSpan.Zero);
            var quality = task.GetQuality(TimeSpan.Zero);

            Assert.True(quality > 3);
        }

        [Fact]
        public void OptionRepetitionTask_ShouldReturnPassingQuality()
        {
            var task = new OptionRepetitionTask("apple", PartOfSpeech.Noun, 0, 0, ["груша", "яблоко", "банан"], "яблоко");

            task.SubmitAnswer("яблоко", TimeSpan.Zero);
            var quality = task.GetQuality(TimeSpan.Zero);

            Assert.True(quality > 3);
        }

        [Theory]
        [InlineData("This apple tastes very sour")]
        [InlineData("Apple is sour.")]
        public void SentenceReorderRepetitionTask_ShouldReturnPassingQuality(string sentence)
        {
            var task = new SentenceReorderRepetitionTask(0, 0, sentence);

            task.SubmitAnswer(sentence, TimeSpan.Zero);
            var quality = task.GetQuality(TimeSpan.Zero);

            Assert.True(task.SentenceParts.Count > 1);
            Assert.True(quality > 3);
        }

        [Fact]
        public void YesOrNotRepetitionTask_ShouldReturnPassingQuality()
        {
            var task = new YesOrNoRepetitionTask("apple", 0, 0, "апельсин", false);

            task.SubmitAnswer("no", TimeSpan.Zero);
            var quality = task.GetQuality(TimeSpan.Zero);

            Assert.True(quality > 3);
        }
    }
}
