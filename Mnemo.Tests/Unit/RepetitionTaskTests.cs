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
            var task = new TextRepetitionTask("apple", 0, 0, ["яблоко"]);

            task.SubmitAnswer("яблоко", TimeSpan.Zero);
            var quality = task.GetQuality(TimeSpan.Zero);

            Assert.True(quality > 3);
        }

        [Fact]
        public void OptionRepetitionTask_ShouldReturnPassingQuality()
        {
            var task = new OptionRepetitionTask("apple", 0, 0, ["груша", "яблоко", "банан"], "яблоко");

            task.SubmitAnswer("яблоко", TimeSpan.Zero);
            var quality = task.GetQuality(TimeSpan.Zero);

            Assert.True(quality > 3);
        }

        [Fact]
        public void OrderPartsRepetitionTask_ShouldReturnPassingQuality()
        {
            var task = new OrderPartsRepetitionTask("Order sentence", 0, 0, "This apple tastes very sour");

            task.SubmitAnswer("This apple tastes very sour", TimeSpan.Zero);
            var quality = task.GetQuality(TimeSpan.Zero);

            Assert.True(task.SentenceParts.Count > 1);
            Assert.True(quality > 3);
        }

        [Fact]
        public void YesOrNotRepetitionTask_ShouldReturnPassingQuality()
        {
            var task = new YesOrNoRepetitionTask("apple - апельсин", 0, 0, false);

            task.SubmitAnswer("no", TimeSpan.Zero);
            var quality = task.GetQuality(TimeSpan.Zero);

            Assert.True(quality > 3);
        }
    }
}
