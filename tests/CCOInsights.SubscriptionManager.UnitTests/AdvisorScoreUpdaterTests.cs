﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CCOInsights.SubscriptionManager.Functions;
using CCOInsights.SubscriptionManager.Functions.Operations.AdvisorScore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CCOInsights.SubscriptionManager.UnitTests
{
    public class AdvisorScoreUpdaterTests
    {
        private readonly IAdvisorScoreUpdater _updater;
        private readonly Mock<IStorage> _storageMock;
        private readonly Mock<ILogger<AdvisorScoreUpdater>> _loggerMock;
        private readonly Mock<IAdvisorScoreProvider> _providerMock;

        public AdvisorScoreUpdaterTests()
        {
            _storageMock = new Mock<IStorage>();
            _loggerMock = new Mock<ILogger<AdvisorScoreUpdater>>();
            _providerMock = new Mock<IAdvisorScoreProvider>();
            _updater = new AdvisorScoreUpdater(_storageMock.Object, _loggerMock.Object, _providerMock.Object);
        }

        [Fact]
        public async Task BlueprintArtifactUpdater_UpdateAsync_ShouldUpdate_IfValid()
        {
            var response = new AdvisorScoreResponse { Id = "Id" };
            _providerMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<AdvisorScoreResponse> { response });

            var subscriptionTest = new TestSubscription();
            await _updater.UpdateAsync(Guid.Empty.ToString(), subscriptionTest, CancellationToken.None);

            _providerMock.Verify(x => x.GetAsync(It.Is<string>(x => x == subscriptionTest.SubscriptionId), CancellationToken.None));
            _storageMock.Verify(x => x.UpdateItemAsync(It.IsAny<string>(), It.Is<AdvisorScore>(x => x.SubscriptionId == subscriptionTest.SubscriptionId && x.TenantId == subscriptionTest.Inner.TenantId), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
