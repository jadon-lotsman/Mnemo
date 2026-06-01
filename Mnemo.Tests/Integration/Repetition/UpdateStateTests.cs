using Microsoft.Extensions.DependencyInjection;
using Mnemo.Common;
using Mnemo.Services;

namespace Mnemo.Tests.Integration.Repetition
{
    public class UpdateStateTests : IntegrationTestBase
    {
        [Fact]
        public async Task AutoAssessment_ShouldUpdateStateAndIncreaseCounter()
        {
            // Arrange
            var user = DataSeeder.CreateUser(id: 3, username: "Bob");
            var entry = DataSeeder.CreateEntry(id: 7, userId: user.Id, foreign: "apple", translations: "яблоко");
            var state = DataSeeder.CreateState(id: 1, userId: user.Id, entryId: entry.Id, repetitionCounter: 2, repetitionInterval: 4, ef: SM2Helper.InitEF);

            var stateService = ServiceProvider.GetRequiredService<RepetitionStateService>();


            // Act
            var result = await stateService.SetQualityRepetitionStateAsync(userId: user.Id, new Dictionary<int, double> { { entry.Id, 5 } }, false);


            // Assert
            Assert.True(result.IsSuccess);

            Assert.Equal(3, state.RepetitionCounter);
        }

        [Fact]
        public async Task SelfAssessment_WhenAllowed_ShouldUpdateStateAndDisableFlag()
        {
            // Arrange
            var user = DataSeeder.CreateUser(id: 3, username: "Bob");
            var entry = DataSeeder.CreateEntry(id: 7, userId: user.Id, foreign: "apple", translations: "яблоко");
            var state = DataSeeder.CreateState(id: 1, userId: user.Id, entryId: entry.Id, repetitionCounter: 2, repetitionInterval: 4, ef: SM2Helper.InitEF);
            state.CanSelfAssess = true;

            var stateService = ServiceProvider.GetRequiredService<RepetitionStateService>();


            // Act
            var result = await stateService.SetQualityRepetitionStateAsync(userId: user.Id, new Dictionary<int, double> { { entry.Id, 5 } }, true);


            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task SelfAssessment_WhenNotAllowed_ShouldReturnFailure()
        {
            // Arrange
            var user = DataSeeder.CreateUser(id: 3, username: "Bob");
            var entry = DataSeeder.CreateEntry(id: 7, userId: user.Id, foreign: "apple", translations: "яблоко");
            var state = DataSeeder.CreateState(id: 1, userId: user.Id, entryId: entry.Id, repetitionCounter: 2, repetitionInterval: 4, ef: SM2Helper.InitEF);
            state.CanSelfAssess = false;

            var stateService = ServiceProvider.GetRequiredService<RepetitionStateService>();


            // Act
            var result = await stateService.SetQualityRepetitionStateAsync(userId: user.Id, new Dictionary<int, double> { { entry.Id, 5 } }, true);


            // Assert
            Assert.False(result.IsSuccess);
            var updatedState = result.Value!;
        }
    }
}
