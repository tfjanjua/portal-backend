/********************************************************************************
 * Copyright (c) 2023 Contributors to the Eclipse Foundation
 *
 * See the NOTICE file(s) distributed with this work for additional
 * information regarding copyright ownership.
 *
 * This program and the accompanying materials are made available under the
 * terms of the Apache License, Version 2.0 which is available at
 * https://www.apache.org/licenses/LICENSE-2.0.
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations
 * under the License.
 *
 * SPDX-License-Identifier: Apache-2.0
 ********************************************************************************/

using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Tests.Shared;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Token;
using Org.Eclipse.TractusX.Portal.Backend.OfferProvider.Library.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.OfferProvider.Library.Models;
using System.Net;

namespace Org.Eclipse.TractusX.Portal.Backend.OfferProvider.Library.Tests;

public class OfferProviderServiceTests
{
    #region Initialization

    private readonly IFixture _fixture;
    private readonly ITokenService _tokenService;

    public OfferProviderServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _tokenService = A.Fake<ITokenService>();
    }

    #endregion

    #region TriggerOfferProvider

    [Fact]
    public async Task TriggerOfferProvider_WithValidCall_ReturnsExpected()
    {
        // Arrange
        var httpMessageHandlerMock =
            new HttpMessageHandlerMock(HttpStatusCode.OK);
        using var httpClient = new HttpClient(httpMessageHandlerMock);
        A.CallTo(() => _tokenService.GetAuthorizedClient<OfferProviderService>(A<KeyVaultAuthSettings>._, A<CancellationToken>._))
            .Returns(httpClient);
        const string url = "https://trigger.com";
        var data = _fixture.Create<OfferThirdPartyAutoSetupData>();
        var service = new OfferProviderService(_tokenService);

        // Act
        var result = await service.TriggerOfferProvider(data, url, "https://auth.url", "test1", "Sup3rS3cureTest!", CancellationToken.None);

        // Assert
        result.Should().Be(true);
    }

    [Fact]
    public async Task TriggerOfferProvider_WithUnsuccessfulStatusCode_ThrowsException()
    {
        // Arrange
        var httpMessageHandlerMock =
            new HttpMessageHandlerMock(HttpStatusCode.BadRequest);
        using var httpClient = new HttpClient(httpMessageHandlerMock);
        A.CallTo(() => _tokenService.GetAuthorizedClient<OfferProviderService>(A<KeyVaultAuthSettings>._, A<CancellationToken>._))
            .Returns(httpClient);
        var data = _fixture.Create<OfferThirdPartyAutoSetupData>();
        var service = new OfferProviderService(_tokenService);

        // Act
        async Task Act() => await service.TriggerOfferProvider(data, "https://callback.com", "https://auth.url", "test1", "Sup3rS3cureTest!", CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ServiceException>(Act);
    }

    [Fact]
    public async Task TriggerOfferProvider_WithException_ThrowsException()
    {
        // Arrange
        var httpMessageHandlerMock =
            new HttpMessageHandlerMock(HttpStatusCode.BadRequest, ex: new HttpRequestException("DNS Error"));
        using var httpClient = new HttpClient(httpMessageHandlerMock);
        A.CallTo(() => _tokenService.GetAuthorizedClient<OfferProviderService>(A<KeyVaultAuthSettings>._, A<CancellationToken>._))
            .Returns(httpClient);
        var data = _fixture.Create<OfferThirdPartyAutoSetupData>();
        var service = new OfferProviderService(_tokenService);

        // Act
        async Task Act() => await service.TriggerOfferProvider(data, "https://callback.com", "https://auth.url", "test1", "Sup3rS3cureTest!", CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ServiceException>(Act);
    }

    #endregion

    #region TriggerOfferProviderCallback

    [Fact]
    public async Task TriggerOfferProviderCallback_WithValidCall_ReturnsExpected()
    {
        // Arrange
        var httpMessageHandlerMock =
            new HttpMessageHandlerMock(HttpStatusCode.OK);
        using var httpClient = new HttpClient(httpMessageHandlerMock);
        A.CallTo(() => _tokenService.GetAuthorizedClient<OfferProviderService>(A<KeyVaultAuthSettings>._, A<CancellationToken>._))
            .Returns(httpClient);
        const string url = "https://trigger.com";
        var data = _fixture.Create<OfferProviderCallbackData>();
        var service = new OfferProviderService(_tokenService);

        // Act
        var result = await service.TriggerOfferProviderCallback(data, url, "https://auth.url", "test1", "Sup3rS3cureTest!", CancellationToken.None);

        // Assert
        result.Should().Be(true);
    }

    [Fact]
    public async Task TriggerOfferProviderCallback_WithUnsuccessfulStatusCode_ThrowsException()
    {
        // Arrange
        var httpMessageHandlerMock =
            new HttpMessageHandlerMock(HttpStatusCode.BadRequest);
        using var httpClient = new HttpClient(httpMessageHandlerMock);
        A.CallTo(() => _tokenService.GetAuthorizedClient<OfferProviderService>(A<KeyVaultAuthSettings>._, A<CancellationToken>._))
            .Returns(httpClient);
        var data = _fixture.Create<OfferProviderCallbackData>();
        var service = new OfferProviderService(_tokenService);

        // Act
        async Task Act() => await service.TriggerOfferProviderCallback(data, "https://callback.com", "https://auth.url", "test1", "Sup3rS3cureTest!", CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ServiceException>(Act);
    }

    [Fact]
    public async Task TriggerOfferProviderCallback_WithException_ThrowsException()
    {
        // Arrange
        var httpMessageHandlerMock =
            new HttpMessageHandlerMock(HttpStatusCode.BadRequest, ex: new HttpRequestException("DNS Error"));
        using var httpClient = new HttpClient(httpMessageHandlerMock);
        A.CallTo(() => _tokenService.GetAuthorizedClient<OfferProviderService>(A<KeyVaultAuthSettings>._, A<CancellationToken>._))
            .Returns(httpClient);
        var data = _fixture.Create<OfferProviderCallbackData>();
        var service = new OfferProviderService(_tokenService);

        // Act
        async Task Act() => await service.TriggerOfferProviderCallback(data, "https://callback.com", "https://auth.url", "test1", "Sup3rS3cureTest!", CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ServiceException>(Act);
    }

    #endregion
}
