/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the CatenaX (ng) GitHub Organisation.
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

using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using CatenaX.NetworkServices.Framework.ErrorHandling;
using CatenaX.NetworkServices.Offers.Library.Service;
using CatenaX.NetworkServices.PortalBackend.DBAccess;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using CatenaX.NetworkServices.Services.Service.BusinessLogic;
using CatenaX.NetworkServices.Tests.Shared;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace CatenaX.NetworkServices.Services.Service.Test.BusinessLogic;

public class ServiceBusinessLogicTests
{
    private readonly Guid _companyUserCompanyId = new("395f955b-f11b-4a74-ab51-92a526c1973a");
    private readonly string _notAssignedCompanyIdUser = "395f955b-f11b-4a74-ab51-92a526c1973c";
    private readonly Guid _existingServiceId = new("9aae7a3b-b188-4a42-b46b-fb2ea5f47661");
    private readonly Guid _validSubscriptionId = new("9aae7a3b-b188-4a42-b46b-fb2ea5f47662");
    private readonly Guid _existingAgreementId = new("9aae7a3b-b188-4a42-b46b-fb2ea5f47664");
    private readonly Guid _validConsentId = new("9aae7a3b-b188-4a42-b46b-fb2ea5f47665");
    private readonly Guid _newOfferSubscriptionId = new("5305d56b-4544-40cc-a02e-912cc19998fa"); 
    private readonly CompanyUser _companyUser;
    private readonly IFixture _fixture;
    private readonly IamUser _iamUser;
    private readonly IAgreementRepository _agreementRepository;
    private readonly IConsentRepository _consentRepository;
    private readonly IOfferRepository _offerRepository;
    private readonly IOfferSubscriptionsRepository _offerSubscriptionsRepository;
    private readonly ILanguageRepository _languageRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IUserRepository _userRepository;
    private readonly IUserRolesRepository _userRolesRepository;
    private readonly IOfferSetupService _offerSetupService;

    public ServiceBusinessLogicTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        var (companyUser, iamUser) = CreateTestUserPair();
        _companyUser = companyUser;
        _iamUser = iamUser;

        _portalRepositories = A.Fake<IPortalRepositories>();
        _agreementRepository = A.Fake<IAgreementRepository>();
        _consentRepository = A.Fake<IConsentRepository>();
        _offerRepository = A.Fake<IOfferRepository>();
        _offerSubscriptionsRepository = A.Fake<IOfferSubscriptionsRepository>();
        _languageRepository = A.Fake<ILanguageRepository>();
        _notificationRepository = A.Fake<INotificationRepository>();
        _userRepository = A.Fake<IUserRepository>();
        _userRolesRepository = A.Fake<IUserRolesRepository>();

        _offerSetupService = A.Fake<IOfferSetupService>();
        

        SetupRepositories(companyUser, iamUser);
        SetupServices(iamUser);
        
        _fixture.Inject(Options.Create(new ServiceSettings { ApplicationsMaxPageSize = 15 }));
        _fixture.Inject(_offerSetupService);
    }

    #region Create Service

    [Fact]
    public async Task CreateServiceOffering_WithValidDataAndEmptyDescriptions_ReturnsCorrectDetails()
    {
        // Arrange
        var serviceId = Guid.NewGuid();

        var apps = new List<Offer>();
        A.CallTo(() => _offerRepository.CreateOffer(A<string>._, A<OfferTypeId>._, A<Action<Offer?>>._))
            .Invokes(x =>
            {
                var provider = x.Arguments.Get<string>("provider");
                var appTypeId = x.Arguments.Get<OfferTypeId>("offerType");
                var action = x.Arguments.Get<Action<Offer?>>("setOptionalParameters");

                var app = new Offer(serviceId, provider!, DateTimeOffset.UtcNow, appTypeId);
                action?.Invoke(app);
                apps.Add(app);
            })
            .Returns(new Offer(serviceId)
            {
                OfferTypeId = OfferTypeId.SERVICE 
            });
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<ServiceBusinessLogic>();

        // Act
        var result = await sut.CreateServiceOffering(new ServiceOfferingData("Newest Service", "42", "img/thumbnail.png", "mail@test.de", _companyUser.Id, new List<ServiceDescription>()), _iamUser.UserEntityId);

        // Assert
        result.Should().Be(serviceId);
        apps.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateServiceOffering_WithValidDataAndDescription_ReturnsCorrectDetails()
    {
        // Arrange
        var serviceId = Guid.NewGuid();

        var apps = new List<Offer>();
        A.CallTo(() => _offerRepository.CreateOffer(A<string>._, A<OfferTypeId>._, A<Action<Offer?>>._))
            .Invokes(x =>
            {
                var provider = x.Arguments.Get<string>("provider");
                var appTypeId = x.Arguments.Get<OfferTypeId>("offerType");
                var action = x.Arguments.Get<Action<Offer?>>("setOptionalParameters");

                var app = new Offer(serviceId, provider!, DateTimeOffset.UtcNow, appTypeId);
                action?.Invoke(app);
                apps.Add(app);
            })
            .Returns(new Offer(serviceId)
            {
                OfferTypeId = OfferTypeId.SERVICE 
            });
        
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<ServiceBusinessLogic>();

        // Act
        var serviceOfferingData = new ServiceOfferingData("Newest Service", "42", "img/thumbnail.png", "mail@test.de", _companyUser.Id, new List<ServiceDescription>
        {
            new ("en", "That's a description with a valid language code")
        });
        var result = await sut.CreateServiceOffering(serviceOfferingData, _iamUser.UserEntityId);

        // Assert
        result.Should().Be(serviceId);
        apps.Should().HaveCount(1);
    }
    
    [Fact]
    public async Task CreateServiceOffering_WithWrongIamUser_ThrowsException()
    {
        // Arrange
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<ServiceBusinessLogic>();

        // Act
        async Task Action() => await sut.CreateServiceOffering(new ServiceOfferingData("Newest Service", "42", "img/thumbnail.png", "mail@test.de", _companyUser.Id, new List<ServiceDescription>()), Guid.NewGuid().ToString());
        
        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.ParamName.Should().Be("iamUserId");
    }

    [Fact]
    public async Task CreateServiceOffering_WithInvalidLanguage_ThrowsException()
    {
        // Arrange
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<ServiceBusinessLogic>();

        // Act
        var serviceOfferingData = new ServiceOfferingData("Newest Service", "42", "img/thumbnail.png", "mail@test.de", _companyUser.Id, new List<ServiceDescription>
        {
            new ("gg", "That's a description with incorrect language short code")
        });
        async Task Action() => await sut.CreateServiceOffering(serviceOfferingData, _iamUser.UserEntityId);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.ParamName.Should().Be("languageCodes");
    }

    [Fact]
    public async Task CreateServiceOffering_WithoutCompanyUser_ThrowsException()
    {
        // Arrange
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<ServiceBusinessLogic>();

        // Act
        async Task Action() => await sut.CreateServiceOffering(new ServiceOfferingData("Newest Service", "42", "img/thumbnail.png", "mail@test.de", Guid.NewGuid(), new List<ServiceDescription>()), _iamUser.UserEntityId);
        
        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.ParamName.Should().Be("SalesManager");
    }

    #endregion

    #region Get Active Services

    [Fact]
    public async Task GetAllActiveServicesAsync_WithDefaultRequest_GetsExpectedEntries()
    {
        // Arrange
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<ServiceBusinessLogic>();

        // Act
        var result = await sut.GetAllActiveServicesAsync(0, 5);

        // Assert
        result.Content.Should().HaveCount(5);
    }

    
    [Fact]
    public async Task GetAllActiveServicesAsync_WithSmallSize_GetsExpectedEntries()
    {
        // Arrange
        const int expectedCount = 3;
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<ServiceBusinessLogic>();

        // Act
        var result = await sut.GetAllActiveServicesAsync(0, expectedCount);

        // Assert
        result.Content.Should().HaveCount(expectedCount);
    }

    #endregion

    #region Add Service Subscription

    [Fact]
    public async Task AddServiceSubscription_WithExistingId_CreatesServiceSubscription()
    {
        // Arrange 
        var companyAssignedApps = new List<OfferSubscription>();
        A.CallTo(() => _offerSubscriptionsRepository.CreateOfferSubscription(A<Guid>._, A<Guid>._, A<OfferSubscriptionStatusId>._, A<Guid>._, A<Guid>._))
            .Invokes(x =>
            {
                var appId = x.Arguments.Get<Guid>("offerId");
                var companyId = x.Arguments.Get<Guid>("companyId");
                var appSubscriptionStatusId = x.Arguments.Get<OfferSubscriptionStatusId>("offerSubscriptionStatusId");
                var requesterId = x.Arguments.Get<Guid>("requesterId");
                var creatorId = x.Arguments.Get<Guid>("creatorId");

                var companyAssignedApp = new OfferSubscription(_newOfferSubscriptionId, appId, companyId, appSubscriptionStatusId, requesterId, creatorId);
                companyAssignedApps.Add(companyAssignedApp);
            });
        var notificationId = Guid.NewGuid();
        var notifications = new List<PortalBackend.PortalEntities.Entities.Notification>(); 
        A.CallTo(() => _notificationRepository.CreateNotification(A<Guid>._, A<NotificationTypeId>._, A<bool>._, A<Action<PortalBackend.PortalEntities.Entities.Notification>?>._))
            .Invokes(x =>
            {
                var receiverUserId = x.Arguments.Get<Guid>("receiverUserId");
                var notificationTypeId = x.Arguments.Get<NotificationTypeId>("notificationTypeId");
                var isRead = x.Arguments.Get<bool>("isRead");
                var setOptionalParameter = x.Arguments.Get< Action<PortalBackend.PortalEntities.Entities.Notification>?>("setOptionalParameter");

                var notification = new PortalBackend.PortalEntities.Entities.Notification(notificationId, receiverUserId, DateTimeOffset.UtcNow, notificationTypeId, isRead);
                setOptionalParameter?.Invoke(notification);
                notifications.Add(notification);
            });
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<ServiceBusinessLogic>();

        // Act
        await sut.AddServiceSubscription(_existingServiceId, _iamUser.UserEntityId);

        // Assert
        companyAssignedApps.Should().HaveCount(1);
        notifications.Should().HaveCount(1);
    }
    
    [Fact]
    public async Task AddServiceSubscription_WithNotExistingId_ThrowsException()
    {
        // Arrange
        var notExistingServiceId = Guid.NewGuid();
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<ServiceBusinessLogic>();

        // Act
        async Task Action() => await sut.AddServiceSubscription(notExistingServiceId, _iamUser.UserEntityId);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Action);
        ex.Message.Should().Be($"Service {notExistingServiceId} does not exist");
    }
    
    [Fact]
    public async Task AddServiceSubscription_NotAssignedCompany_ThrowsException()
    {
        // Arrange
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<ServiceBusinessLogic>();

        // Act
        async Task Action() => await sut.AddServiceSubscription(_existingServiceId, _notAssignedCompanyIdUser);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.ParamName.Should().Be("iamUserId");
    }

    [Fact]
    public async Task AddServiceSubscription_NotAssignedCompanyUser_ThrowsException()
    {
        // Arrange
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<ServiceBusinessLogic>();

        // Act
        async Task Action() => await sut.AddServiceSubscription(_existingServiceId, Guid.NewGuid().ToString());

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.ParamName.Should().Be("iamUserId");
    }

    #endregion

    #region Get Service Detail Data

    [Fact]
    public async Task GetServiceDetailsAsync_WithExistingServiceAndLanguageCode_ReturnsServiceDetailData()
    {
        // Arrange
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<ServiceBusinessLogic>();

        // Act
        var result = await sut.GetServiceDetailsAsync(_existingServiceId, "en", _iamUser.UserEntityId);

        // Assert
        result.Id.Should().Be(_existingServiceId);
    }

    [Fact]
    public async Task GetServiceDetailsAsync_WithoutExistingService_ThrowsException()
    {
        // Arrange
        var notExistingServiceId = Guid.NewGuid();
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<ServiceBusinessLogic>();

        // Act
        async Task Action() => await sut.GetServiceDetailsAsync(notExistingServiceId, "en", _iamUser.UserEntityId);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Action);
        ex.Message.Should().Be($"Service {notExistingServiceId} does not exist");
    }

    #endregion

    #region Get Service Agreement

    [Fact]
    public async Task GetServiceAgreement_WithUserId_ReturnsServiceDetailData()
    {
        // Arrange
        var offerService = A.Fake<IOfferService>();
        var data = _fixture.CreateMany<AgreementData>(1);
        A.CallTo(() => offerService.GetOfferAgreementsAsync(A<Guid>.That.Matches(x => x == _existingServiceId), A<OfferTypeId>._))
            .Returns(data.ToAsyncEnumerable());
        var sut = new ServiceBusinessLogic(A.Fake<IPortalRepositories>(),A.Fake<IOfferSetupService>(), offerService, Options.Create(new ServiceSettings()));

        // Act
        var result = await sut.GetServiceAgreement(_existingServiceId).ToListAsync().ConfigureAwait(false);

        // Assert
        result.Should().ContainSingle();
    }

    #endregion

    #region Get Subscription Details

    [Fact]
    public async Task GetSubscriptionDetails_WithValidId_ReturnsSubscriptionDetailData()
    {
        // Arrange
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<ServiceBusinessLogic>();

        // Act
        var result = await sut.GetSubscriptionDetailAsync(_validSubscriptionId, _iamUser.UserEntityId).ConfigureAwait(false);

        // Assert
        result.OfferId.Should().Be(_existingServiceId);
    }

    [Fact]
    public async Task GetSubscriptionDetails_WithInvalidId_ThrowsException()
    {
        // Arrange
        var notExistingId = Guid.NewGuid();
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<ServiceBusinessLogic>();

        // Act
        async Task Action() => await sut.GetSubscriptionDetailAsync(notExistingId, _iamUser.UserEntityId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Action);
        ex.Message.Should().Be($"Subscription {notExistingId} does not exist");
    }

    #endregion

    #region Create Service Agreement Consent

    [Fact]
    public async Task CreateServiceAgreementConsent_ReturnsCorrectId()
    {
        // Arrange
        var consentId = Guid.NewGuid();
        var offerService = A.Fake<IOfferService>();
        A.CallTo(() => offerService.CreateOfferSubscriptionAgreementConsentAsync(A<Guid>._, A<Guid>._, A<ConsentStatusId>._, A<string>._, A<OfferTypeId>._))
            .ReturnsLazily(() => consentId);
        var sut = new ServiceBusinessLogic(A.Fake<IPortalRepositories>(),A.Fake<IOfferSetupService>(), offerService, Options.Create(new ServiceSettings()));

        // Act
        var serviceAgreementConsentData = new ServiceAgreementConsentData(_existingAgreementId, ConsentStatusId.ACTIVE);
        var result = await sut.CreateServiceAgreementConsentAsync(_existingServiceId, serviceAgreementConsentData, _iamUser.UserEntityId);

        // Assert
        result.Should().Be(consentId);
    }

    [Fact]
    public async Task CreateOrUpdateServiceAgreementConsentAsync_RunsSuccessfull()
    {
        // Arrange
        var offerService = A.Fake<IOfferService>();
        A.CallTo(() => offerService.CreateOrUpdateOfferSubscriptionAgreementConsentAsync(A<Guid>._, A<IEnumerable<ServiceAgreementConsentData>>._, A<string>._, A<OfferTypeId>._))
            .ReturnsLazily(() => Task.CompletedTask);
        var sut = new ServiceBusinessLogic(A.Fake<IPortalRepositories>(), A.Fake<IOfferSetupService>(), offerService, Options.Create(new ServiceSettings()));

        // Act
        await sut.CreateOrUpdateServiceAgreementConsentAsync(_existingServiceId, new List<ServiceAgreementConsentData>
        {
            new(_existingAgreementId, ConsentStatusId.ACTIVE)
        }, _iamUser.UserEntityId);

        // Assert
        true.Should().BeTrue();
    }

    #endregion

    #region Get Service Consent Detail Data

    [Fact]
    public async Task GetServiceConsentDetailData_WithValidId_ReturnsServiceConsentDetailData()
    {
        // Arrange
        var data = new ConsentDetailData(_validConsentId, "The Company", Guid.NewGuid(), ConsentStatusId.ACTIVE, "Agreed");
        var offerService = A.Fake<IOfferService>();
        A.CallTo(() => offerService.GetConsentDetailDataAsync(A<Guid>.That.Matches(x => x == _validConsentId), A<OfferTypeId>._))
            .ReturnsLazily(() => data);
        var sut = new ServiceBusinessLogic(A.Fake<IPortalRepositories>(),A.Fake<IOfferSetupService>(), offerService, Options.Create(new ServiceSettings()));

        // Act
        var result = await sut.GetServiceConsentDetailDataAsync(_validConsentId).ConfigureAwait(false);

        // Assert
        result.Id.Should().Be(_validConsentId);
        result.CompanyName.Should().Be("The Company");
    }

    [Fact]
    public async Task GetServiceConsentDetailData_WithInValidId_ReturnsServiceConsentDetailData()
    {
        // Arrange
        var offerService = A.Fake<IOfferService>();
        var invalidConsentId = Guid.NewGuid();
        A.CallTo(() => offerService.GetConsentDetailDataAsync(A<Guid>.That.Not.Matches(x => x == _validConsentId), A<OfferTypeId>._))
            .Throws(() => new NotFoundException("Test"));
        var sut = new ServiceBusinessLogic(A.Fake<IPortalRepositories>(), A.Fake<IOfferSetupService>(), offerService, Options.Create(new ServiceSettings()));

        // Act
        async Task Action() => await sut.GetServiceConsentDetailDataAsync(invalidConsentId).ConfigureAwait(false);

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(Action);
    }

    #endregion

    #region Setup

    private void SetupRepositories(CompanyUser companyUser, IamUser iamUser)
    {
        var serviceDetailData = new AsyncEnumerableStub<ValueTuple<Guid, string?, string, string?, string?, string?>>(_fixture.CreateMany<ValueTuple<Guid, string?, string, string?, string?, string?>>(5));
        var serviceDetail = _fixture.Build<OfferDetailData>()
            .With(x => x.Id, _existingServiceId)
            .Create();
        
        A.CallTo(() => _userRepository.GetCompanyUserWithIamUserCheckAndCompanyShortName(iamUser.UserEntityId, companyUser.Id))
            .ReturnsLazily(() => new List<(Guid CompanyUserId, bool IsIamUser, string CompanyUserName, Guid CompanyId)>{new (_companyUser.Id, true, "COMPANYBPN", _companyUserCompanyId), new (_companyUser.Id, false, "OTHERCOMPANYBPN", _companyUserCompanyId)}.ToAsyncEnumerable());
        A.CallTo(() => _userRepository.GetCompanyUserWithIamUserCheckAndCompanyShortName(iamUser.UserEntityId, A<Guid>.That.Not.Matches(x => x == companyUser.Id)))
            .ReturnsLazily(() => new List<(Guid CompanyUserId, bool IsIamUser, string CompanyUserName, Guid CompanyId)>{new (_companyUser.Id, true, "COMPANYBPN", _companyUserCompanyId)}.ToAsyncEnumerable());
        A.CallTo(() => _userRepository.GetCompanyUserWithIamUserCheckAndCompanyShortName(A<string>.That.Not.Matches(x => x == iamUser.UserEntityId), companyUser.Id))
            .ReturnsLazily(() => new List<(Guid CompanyUserId, bool IsIamUser, string CompanyUserName, Guid CompanyId)>{new (_companyUser.Id, false, "OTHERCOMPANYBPN", _companyUserCompanyId)}.ToAsyncEnumerable());
        A.CallTo(() => _userRepository.GetCompanyUserWithIamUserCheckAndCompanyShortName(A<string>.That.Not.Matches(x => x == iamUser.UserEntityId), A<Guid>.That.Not.Matches(x => x == companyUser.Id)))
            .ReturnsLazily(() => new List<(Guid CompanyUserId, bool IsIamUser, string CompanyUserName, Guid CompanyId)>().ToAsyncEnumerable());

        A.CallTo(() => _userRepository.GetOwnCompanAndCompanyUseryId(iamUser.UserEntityId))
            .ReturnsLazily(() => (_companyUser.Id, _companyUser.CompanyId, "The Company", "test@mail.de"));
        A.CallTo(() => _userRepository.GetOwnCompanAndCompanyUseryId(_notAssignedCompanyIdUser))
            .ReturnsLazily(() => (_companyUser.Id, Guid.Empty, "The Company", "test@mail.de"));
        A.CallTo(() => _userRepository.GetOwnCompanAndCompanyUseryId(A<string>.That.Not.Matches(x => x == iamUser.UserEntityId || x == _notAssignedCompanyIdUser)))
            .ReturnsLazily(() => (Guid.Empty, _companyUser.CompanyId, "The Company", "test@mail.de"));
        
        A.CallTo(() => _offerRepository.GetActiveServices())
            .Returns(serviceDetailData.AsQueryable());
        
        A.CallTo(() => _offerRepository.GetOfferDetailByIdUntrackedAsync(_existingServiceId, A<string>.That.Matches(x => x == "en"), A<string>._, A<OfferTypeId>._))
            .ReturnsLazily(() => serviceDetail with {OfferSubscriptionDetailData = new []
            {
                new OfferSubscriptionStateDetailData(Guid.NewGuid(), OfferSubscriptionStatusId.ACTIVE)
            }});
        A.CallTo(() => _offerRepository.GetOfferDetailByIdUntrackedAsync(A<Guid>.That.Not.Matches(x => x == _existingServiceId), A<string>._, A<string>._, A<OfferTypeId>._))
            .ReturnsLazily(() => (OfferDetailData?)null);
        
        A.CallTo(() => _offerRepository.GetOfferProviderDetailsAsync(A<Guid>.That.Matches(x => x == _existingServiceId)))
            .ReturnsLazily(() => new OfferProviderDetailsData("Test Service", "Test Company", "provider@mail.de", new Guid("ac1cf001-7fbc-1f2f-817f-bce058020001"), "https://www.testurl.com"));
        A.CallTo(() => _offerRepository.GetOfferProviderDetailsAsync(A<Guid>.That.Not.Matches(x => x == _existingServiceId)))
            .ReturnsLazily(() => (OfferProviderDetailsData?)null);
        
        A.CallTo(() => _languageRepository.GetLanguageCodesUntrackedAsync(A<IEnumerable<string>>.That.Matches(x => x.Count() == 1 && x.All(y => y == "en"))))
            .Returns(new List<string> { "en" }.ToAsyncEnumerable());
        A.CallTo(() => _languageRepository.GetLanguageCodesUntrackedAsync(A<IEnumerable<string>>.That.Matches(x => x.Count() == 1 && x.All(y => y == "gg"))))
            .Returns(new List<string>().ToAsyncEnumerable());
        
        A.CallTo(() => _offerRepository.CheckServiceExistsById(_existingServiceId))
            .Returns(true);
        A.CallTo(() => _offerRepository.CheckServiceExistsById(A<Guid>.That.Not.Matches(x => x == _existingServiceId)))
            .Returns(false);
        
        var agreementData = _fixture.CreateMany<AgreementData>(1);
        A.CallTo(() => _agreementRepository.GetOfferAgreementDataForOfferId(A<Guid>.That.Matches(x => x == _existingServiceId), A<OfferTypeId>._))
            .Returns(agreementData.ToAsyncEnumerable());
        A.CallTo(() => _agreementRepository.GetOfferAgreementDataForOfferId(A<Guid>.That.Not.Matches(x => x == _existingServiceId), A<OfferTypeId>._))
            .Returns(new List<AgreementData>().ToAsyncEnumerable());
        A.CallTo(() => _agreementRepository.CheckAgreementExistsForSubscriptionAsync(A<Guid>.That.Matches(x => x == _existingAgreementId), A<Guid>._, A<OfferTypeId>.That.Matches(x => x == OfferTypeId.SERVICE)))
            .ReturnsLazily(() => true);
        A.CallTo(() => _agreementRepository.CheckAgreementExistsForSubscriptionAsync(A<Guid>.That.Not.Matches(x => x == _existingAgreementId), A<Guid>._, A<OfferTypeId>._))
            .ReturnsLazily(() => false);
        A.CallTo(() => _agreementRepository.CheckAgreementExistsForSubscriptionAsync(A<Guid>._, A<Guid>._, A<OfferTypeId>.That.Not.Matches(x => x == OfferTypeId.SERVICE)))
            .ReturnsLazily(() => false);

        var offerSubscription = _fixture.Create<OfferSubscription>();
        A.CallTo(() => _offerSubscriptionsRepository.GetSubscriptionDetailDataForOwnUserAsync(
                A<Guid>.That.Matches(x => x == _validSubscriptionId),
                A<string>.That.Matches(x => x == iamUser.UserEntityId),
                A<OfferTypeId>._))
            .ReturnsLazily(() =>
                new SubscriptionDetailData(_existingServiceId, "Super Service", OfferSubscriptionStatusId.ACTIVE));
        A.CallTo(() => _offerSubscriptionsRepository.GetSubscriptionDetailDataForOwnUserAsync(
                A<Guid>.That.Not.Matches(x => x == _validSubscriptionId),
                A<string>.That.Matches(x => x == iamUser.UserEntityId),
                A<OfferTypeId>._))
            .ReturnsLazily(() => (SubscriptionDetailData?)null);
        A.CallTo(() => _offerSubscriptionsRepository.GetCompanyIdWithAssignedOfferForCompanyUserAndSubscriptionAsync(
                A<Guid>.That.Matches(x => x == _existingServiceId), A<string>.That.Matches(x => x == iamUser.UserEntityId), A<OfferTypeId>._))
            .ReturnsLazily(() => new ValueTuple<Guid, OfferSubscription?, Guid>(_companyUser.CompanyId, offerSubscription, _companyUser.Id));
        A.CallTo(() => _offerSubscriptionsRepository.GetCompanyIdWithAssignedOfferForCompanyUserAndSubscriptionAsync(
                A<Guid>.That.Not.Matches(x => x == _existingServiceId), A<string>.That.Matches(x => x == iamUser.UserEntityId),
                A<OfferTypeId>._))
            .ReturnsLazily(() => new ValueTuple<Guid, OfferSubscription?, Guid>(_companyUser.CompanyId, (OfferSubscription?)null, _companyUser.Id));
        A.CallTo(() => _offerSubscriptionsRepository.GetCompanyIdWithAssignedOfferForCompanyUserAndSubscriptionAsync(
                A<Guid>.That.Matches(x => x == _existingServiceId), A<string>.That.Not.Matches(x => x == iamUser.UserEntityId),
                A<OfferTypeId>._))
            .ReturnsLazily(() => ((Guid companyId, OfferSubscription? offerSubscription, Guid companyUserId))default);

        A.CallTo(() => _consentRepository.GetConsentDetailData(A<Guid>.That.Matches(x => x == _validConsentId), A<OfferTypeId>.That.Matches(x => x == OfferTypeId.SERVICE)))
            .ReturnsLazily(() =>
                new ConsentDetailData(_validConsentId, "The Company", _companyUser.Id, ConsentStatusId.ACTIVE,
                    "Agreed"));
        A.CallTo(() => _consentRepository.GetConsentDetailData(A<Guid>._, A<OfferTypeId>.That.Not.Matches(x => x == OfferTypeId.SERVICE)))
            .ReturnsLazily(() => (ConsentDetailData?)null);
        A.CallTo(() => _consentRepository.GetConsentDetailData(A<Guid>.That.Not.Matches(x => x == _validConsentId), A<OfferTypeId>._))
            .ReturnsLazily(() => (ConsentDetailData?)null);

        var userRoleData = _fixture.CreateMany<UserRoleData>(3);
        A.CallTo(
                () => _userRolesRepository.GetUserRoleDataUntrackedAsync(A<IDictionary<string, IEnumerable<string>>>._))
            .ReturnsLazily(() => userRoleData.ToAsyncEnumerable());

        A.CallTo(() => _userRolesRepository.GetUserRolesForOfferIdAsync(A<Guid>.That.Matches(x => x == _existingServiceId)))
            .ReturnsLazily(() => new List<string> { "Buyer", "Supplier" });
        A.CallTo(() => _portalRepositories.GetInstance<IAgreementRepository>()).Returns(_agreementRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IConsentRepository>()).Returns(_consentRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IOfferRepository>()).Returns(_offerRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IOfferSubscriptionsRepository>()).Returns(_offerSubscriptionsRepository);
        A.CallTo(() => _portalRepositories.GetInstance<ILanguageRepository>()).Returns(_languageRepository);
        A.CallTo(() => _portalRepositories.GetInstance<INotificationRepository>()).Returns(_notificationRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>()).Returns(_userRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRolesRepository>()).Returns(_userRolesRepository);
    }

    private void SetupServices(IamUser iamUser)
    {
        A.CallTo(() => _offerSetupService.AutoSetupOffer(A<Guid>.That.Matches(x => x == _newOfferSubscriptionId), A<string>.That.Matches(x => x == iamUser.UserEntityId), A<string>._))
            .ReturnsLazily(() => true);
        A.CallTo(() => _offerSetupService.AutoSetupOffer(A<Guid>.That.Not.Matches(x => x == _newOfferSubscriptionId), A<string>.That.Matches(x => x == iamUser.UserEntityId), A<string>._))
            .ReturnsLazily(() => true);
    }

    private (CompanyUser, IamUser) CreateTestUserPair()
    {
        var companyUser = _fixture.Build<CompanyUser>()
            .Without(u => u.IamUser)
            .With(u => u.CompanyId, _companyUserCompanyId)
            .Create();
        var iamUser = _fixture.Build<IamUser>()
            .With(u => u.CompanyUser, companyUser)
            .Create();
        companyUser.IamUser = iamUser;
        companyUser.Company = new Company(Guid.NewGuid(), "The Company", CompanyStatusId.ACTIVE, DateTimeOffset.UtcNow);
        return (companyUser, iamUser);
    }

    #endregion
}
