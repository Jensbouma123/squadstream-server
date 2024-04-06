using Microsoft.AspNetCore.SignalR;
using Moq;
using NUnit.Framework;
using Web_API_new.DTO.Responses;
using Web_API_new.DTOs.Match;
using Web_API_new.Hubs;

namespace ServerTests;

public class RealTime
{
    // [Test]
    // public async Task JoinSpecificMatchReport_GameFound_JoinGroup()
    // {
    //     // Arrange
    //     var mockClients = new Mock<IHubCallerClients>();
    //     var mockClientProxy = new Mock<ISingleClientProxy>(); // Change this line
    //     var mockContext = new Mock<HubCallerContext>();
    //     var mockGroups = new Mock<IGroupManager>();
    //     var matchReport = new MatchReportDTO { Code = "existing" };
    //     var matchDetails = new MatchDetailsDTO { Match = "existing" };
    //
    //     mockClients.Setup(clients => clients.Caller).Returns(mockClientProxy.Object); // Now this line is correct
    //     mockGroups.Setup(groups => groups.AddToGroupAsync(It.IsAny<string>(), "existing", default(CancellationToken))).Returns(Task.CompletedTask);
    //
    //     var hub = new MatchReportHub
    //     {
    //         Clients = mockClients.Object,
    //         Context = mockContext.Object,
    //         Groups = mockGroups.Object
    //     };
    //
    //     // Act
    //     await hub.JoinSpecificMatchReport(matchReport);
    //
    //     // Assert
    //     mockGroups.Verify(groups => groups.AddToGroupAsync(It.IsAny<string>(), "existing", default(CancellationToken)), Times.Once);
    // }
}