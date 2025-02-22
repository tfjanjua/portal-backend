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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Enums;
using Org.Eclipse.TractusX.Portal.Backend.OnboardingServiceProvider.Library;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.NetworkRegistration.Library;

namespace Org.Eclipse.TractusX.Portal.Backend.Processes.NetworkRegistration.Executor.Tests;

public class NetworkRegistrationProcessTypeExecutorTests
{
    private readonly INetworkRepository _networkRepository;
    private readonly INetworkRegistrationHandler _networkRegistrationHandler;
    private readonly IOnboardingServiceProviderBusinessLogic _onboardingServiceProviderBusinessLogic;

    private readonly NetworkRegistrationProcessTypeExecutor _sut;

    public NetworkRegistrationProcessTypeExecutorTests()
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        var portalRepositories = A.Fake<IPortalRepositories>();
        _networkRepository = A.Fake<INetworkRepository>();
        _networkRegistrationHandler = A.Fake<INetworkRegistrationHandler>();
        _onboardingServiceProviderBusinessLogic = A.Fake<IOnboardingServiceProviderBusinessLogic>();

        A.CallTo(() => portalRepositories.GetInstance<INetworkRepository>()).Returns(_networkRepository);

        _sut = new NetworkRegistrationProcessTypeExecutor(portalRepositories, _networkRegistrationHandler, _onboardingServiceProviderBusinessLogic);
    }

    [Fact]
    public void GetProcessTypeId_ReturnsExpected()
    {
        // Assert
        _sut.GetProcessTypeId().Should().Be(ProcessTypeId.PARTNER_REGISTRATION);
    }

    [Fact]
    public void IsExecutableStepTypeId_WithValid_ReturnsExpected()
    {
        // Assert
        _sut.IsExecutableStepTypeId(ProcessStepTypeId.SYNCHRONIZE_USER).Should().BeTrue();
    }

    [Theory]
    [InlineData(ProcessStepTypeId.MANUAL_VERIFY_REGISTRATION)]
    [InlineData(ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_PUSH)]
    [InlineData(ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_PULL)]
    [InlineData(ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_MANUAL)]
    [InlineData(ProcessStepTypeId.CREATE_IDENTITY_WALLET)]
    [InlineData(ProcessStepTypeId.RETRIGGER_IDENTITY_WALLET)]
    [InlineData(ProcessStepTypeId.START_CLEARING_HOUSE)]
    [InlineData(ProcessStepTypeId.RETRIGGER_CLEARING_HOUSE)]
    [InlineData(ProcessStepTypeId.AWAIT_CLEARING_HOUSE_RESPONSE)]
    [InlineData(ProcessStepTypeId.START_SELF_DESCRIPTION_LP)]
    [InlineData(ProcessStepTypeId.RETRIGGER_SELF_DESCRIPTION_LP)]
    [InlineData(ProcessStepTypeId.ASSIGN_INITIAL_ROLES)]
    [InlineData(ProcessStepTypeId.RETRIGGER_BUSINESS_PARTNER_NUMBER_PUSH)]
    [InlineData(ProcessStepTypeId.RETRIGGER_BUSINESS_PARTNER_NUMBER_PULL)]
    [InlineData(ProcessStepTypeId.MANUAL_TRIGGER_OVERRIDE_CLEARING_HOUSE)]
    [InlineData(ProcessStepTypeId.START_OVERRIDE_CLEARING_HOUSE)]
    [InlineData(ProcessStepTypeId.AWAIT_SELF_DESCRIPTION_LP_RESPONSE)]
    [InlineData(ProcessStepTypeId.MANUAL_DECLINE_APPLICATION)]
    [InlineData(ProcessStepTypeId.TRIGGER_PROVIDER)]
    [InlineData(ProcessStepTypeId.AWAIT_START_AUTOSETUP)]
    [InlineData(ProcessStepTypeId.OFFERSUBSCRIPTION_CLIENT_CREATION)]
    [InlineData(ProcessStepTypeId.SINGLE_INSTANCE_SUBSCRIPTION_DETAILS_CREATION)]
    [InlineData(ProcessStepTypeId.OFFERSUBSCRIPTION_TECHNICALUSER_CREATION)]
    [InlineData(ProcessStepTypeId.ACTIVATE_SUBSCRIPTION)]
    [InlineData(ProcessStepTypeId.TRIGGER_PROVIDER_CALLBACK)]
    [InlineData(ProcessStepTypeId.RETRIGGER_PROVIDER)]
    [InlineData(ProcessStepTypeId.RETRIGGER_OFFERSUBSCRIPTION_CLIENT_CREATION)]
    [InlineData(ProcessStepTypeId.RETRIGGER_OFFERSUBSCRIPTION_TECHNICALUSER_CREATION)]
    [InlineData(ProcessStepTypeId.RETRIGGER_PROVIDER_CALLBACK)]
    [InlineData(ProcessStepTypeId.MANUAL_TRIGGER_ACTIVATE_SUBSCRIPTION)]
    [InlineData(ProcessStepTypeId.RETRIGGER_SYNCHRONIZE_USER)]
    [InlineData(ProcessStepTypeId.RETRIGGER_REMOVE_KEYCLOAK_USERS)]
    public void IsExecutableStepTypeId_WithInvalid_ReturnsExpected(ProcessStepTypeId processStepTypeId)
    {
        // Assert
        _sut.IsExecutableStepTypeId(processStepTypeId).Should().BeFalse();
    }

    [Fact]
    public void GetExecutableStepTypeIds_ReturnsExpected()
    {
        // Assert
        _sut.GetExecutableStepTypeIds().Should().HaveCount(5).And.Satisfy(
            x => x == ProcessStepTypeId.SYNCHRONIZE_USER,
            x => x == ProcessStepTypeId.TRIGGER_CALLBACK_OSP_SUBMITTED,
            x => x == ProcessStepTypeId.TRIGGER_CALLBACK_OSP_DECLINED,
            x => x == ProcessStepTypeId.TRIGGER_CALLBACK_OSP_APPROVED,
            x => x == ProcessStepTypeId.REMOVE_KEYCLOAK_USERS);
    }

    [Fact]
    public async Task IsLockRequested_ReturnsExpected()
    {
        // Act
        var result = await _sut.IsLockRequested(ProcessStepTypeId.SYNCHRONIZE_USER);

        // Assert
        result.Should().BeFalse();
    }

    #region InitializeProcess

    [Fact]
    public async Task InitializeProcess_WithExistingProcess_ReturnsExpected()
    {
        // Arrange
        var validProcessId = Guid.NewGuid();
        A.CallTo(() => _networkRepository.GetNetworkRegistrationDataForProcessIdAsync(validProcessId))
            .Returns(Guid.NewGuid());

        // Act
        var result = await _sut.InitializeProcess(validProcessId, Enumerable.Empty<ProcessStepTypeId>());

        // Assert
        result.Modified.Should().BeFalse();
        result.ScheduleStepTypeIds.Should().BeNull();
    }

    [Fact]
    public async Task InitializeProcess_WithNotExistingProcess_ThrowsNotFoundException()
    {
        // Arrange
        var validProcessId = Guid.NewGuid();
        A.CallTo(() => _networkRepository.GetNetworkRegistrationDataForProcessIdAsync(validProcessId))
            .Returns(Guid.Empty);

        // Act
        async Task Act() => await _sut.InitializeProcess(validProcessId, Enumerable.Empty<ProcessStepTypeId>());

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"process {validProcessId} does not exist or is not associated with an offer subscription");
    }

    #endregion

    #region ExecuteProcessStep

    [Fact]
    public async Task ExecuteProcessStep_WithoutRegistrationId_ThrowsUnexpectedConditionException()
    {
        // Act
        async Task Act() => await _sut.ExecuteProcessStep(ProcessStepTypeId.SYNCHRONIZE_USER, Enumerable.Empty<ProcessStepTypeId>(), CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<UnexpectedConditionException>(Act);
        ex.Message.Should().Be("networkRegistrationId should never be empty here");
    }

    [Fact]
    public async Task ExecuteProcessStep_WithValidData_CallsExpected()
    {
        // Arrange InitializeProcess
        var validProcessId = Guid.NewGuid();
        var networkRegistrationId = Guid.NewGuid();
        A.CallTo(() => _networkRepository.GetNetworkRegistrationDataForProcessIdAsync(validProcessId))
            .Returns(networkRegistrationId);

        // Act InitializeProcess
        var initializeResult = await _sut.InitializeProcess(validProcessId, Enumerable.Empty<ProcessStepTypeId>());

        // Assert InitializeProcess
        initializeResult.Modified.Should().BeFalse();
        initializeResult.ScheduleStepTypeIds.Should().BeNull();

        // Arrange
        A.CallTo(() => _networkRegistrationHandler.SynchronizeUser(networkRegistrationId))
            .Returns(new ValueTuple<IEnumerable<ProcessStepTypeId>?, ProcessStepStatusId, bool, string?>(null, ProcessStepStatusId.DONE, false, null));

        // Act
        var result = await _sut.ExecuteProcessStep(ProcessStepTypeId.SYNCHRONIZE_USER, Enumerable.Empty<ProcessStepTypeId>(), CancellationToken.None);

        // Assert
        result.Modified.Should().BeFalse();
        result.ScheduleStepTypeIds.Should().BeNull();
        result.ProcessStepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.ProcessMessage.Should().BeNull();
        result.SkipStepTypeIds.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteProcessStep_WithRemoveKeycloakUser_CallsExpected()
    {
        // Arrange InitializeProcess
        var validProcessId = Guid.NewGuid();
        var networkRegistrationId = Guid.NewGuid();
        A.CallTo(() => _networkRepository.GetNetworkRegistrationDataForProcessIdAsync(validProcessId))
            .Returns(networkRegistrationId);

        // Act InitializeProcess
        var initializeResult = await _sut.InitializeProcess(validProcessId, Enumerable.Empty<ProcessStepTypeId>());

        // Assert InitializeProcess
        initializeResult.Modified.Should().BeFalse();
        initializeResult.ScheduleStepTypeIds.Should().BeNull();

        // Arrange
        A.CallTo(() => _networkRegistrationHandler.RemoveKeycloakUser(networkRegistrationId))
            .Returns(new ValueTuple<IEnumerable<ProcessStepTypeId>?, ProcessStepStatusId, bool, string?>(null, ProcessStepStatusId.DONE, false, null));

        // Act
        var result = await _sut.ExecuteProcessStep(ProcessStepTypeId.REMOVE_KEYCLOAK_USERS, Enumerable.Empty<ProcessStepTypeId>(), CancellationToken.None);

        // Assert
        result.Modified.Should().BeFalse();
        result.ScheduleStepTypeIds.Should().BeNull();
        result.ProcessStepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.ProcessMessage.Should().BeNull();
        result.SkipStepTypeIds.Should().BeNull();
    }

    [Theory]
    [InlineData(ProcessStepTypeId.TRIGGER_CALLBACK_OSP_SUBMITTED)]
    [InlineData(ProcessStepTypeId.TRIGGER_CALLBACK_OSP_DECLINED)]
    [InlineData(ProcessStepTypeId.TRIGGER_CALLBACK_OSP_APPROVED)]
    public async Task ExecuteProcessStep_WithValidTriggerData_CallsExpected(ProcessStepTypeId processStepTypeId)
    {
        // Arrange InitializeProcess
        var validProcessId = Guid.NewGuid();
        var networkRegistrationId = Guid.NewGuid();
        A.CallTo(() => _networkRepository.GetNetworkRegistrationDataForProcessIdAsync(validProcessId))
            .Returns(networkRegistrationId);

        // Act InitializeProcess
        var initializeResult = await _sut.InitializeProcess(validProcessId, Enumerable.Empty<ProcessStepTypeId>());

        // Assert InitializeProcess
        initializeResult.Modified.Should().BeFalse();
        initializeResult.ScheduleStepTypeIds.Should().BeNull();

        // Arrange
        A.CallTo(() => _onboardingServiceProviderBusinessLogic.TriggerProviderCallback(networkRegistrationId, processStepTypeId, CancellationToken.None))
            .Returns(new ValueTuple<IEnumerable<ProcessStepTypeId>?, ProcessStepStatusId, bool, string?>(null, ProcessStepStatusId.DONE, false, null));

        // Act
        var result = await _sut.ExecuteProcessStep(processStepTypeId, Enumerable.Empty<ProcessStepTypeId>(), CancellationToken.None);

        // Assert
        result.Modified.Should().BeFalse();
        result.ScheduleStepTypeIds.Should().BeNull();
        result.ProcessStepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.ProcessMessage.Should().BeNull();
        result.SkipStepTypeIds.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteProcessStep_WithRecoverableServiceException_ReturnsToDo()
    {
        // Arrange InitializeProcess
        var validProcessId = Guid.NewGuid();
        var networkRegistrationId = Guid.NewGuid();
        A.CallTo(() => _networkRepository.GetNetworkRegistrationDataForProcessIdAsync(validProcessId))
            .Returns(networkRegistrationId);

        // Act InitializeProcess
        var initializeResult = await _sut.InitializeProcess(validProcessId, Enumerable.Empty<ProcessStepTypeId>());

        // Assert InitializeProcess
        initializeResult.Modified.Should().BeFalse();
        initializeResult.ScheduleStepTypeIds.Should().BeNull();

        // Arrange
        A.CallTo(() => _networkRegistrationHandler.SynchronizeUser(networkRegistrationId))
            .Throws(new ServiceException("this is a test", true));

        // Act
        var result = await _sut.ExecuteProcessStep(ProcessStepTypeId.SYNCHRONIZE_USER, Enumerable.Empty<ProcessStepTypeId>(), CancellationToken.None);

        // Assert
        result.Modified.Should().BeTrue();
        result.ScheduleStepTypeIds.Should().BeNull();
        result.ProcessStepStatusId.Should().Be(ProcessStepStatusId.TODO);
        result.ProcessMessage.Should().Be("this is a test");
        result.SkipStepTypeIds.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteProcessStep_WithServiceException_ReturnsFailedAndRetriggerStep()
    {
        // Arrange InitializeProcess
        var validProcessId = Guid.NewGuid();
        var networkRegistrationId = Guid.NewGuid();
        A.CallTo(() => _networkRepository.GetNetworkRegistrationDataForProcessIdAsync(validProcessId))
            .Returns(networkRegistrationId);

        // Act InitializeProcess
        var initializeResult = await _sut.InitializeProcess(validProcessId, Enumerable.Empty<ProcessStepTypeId>());

        // Assert InitializeProcess
        initializeResult.Modified.Should().BeFalse();
        initializeResult.ScheduleStepTypeIds.Should().BeNull();

        // Arrange
        A.CallTo(() => _networkRegistrationHandler.SynchronizeUser(networkRegistrationId))
            .Throws(new ServiceException("this is a test"));

        // Act
        var result = await _sut.ExecuteProcessStep(ProcessStepTypeId.SYNCHRONIZE_USER, Enumerable.Empty<ProcessStepTypeId>(), CancellationToken.None);

        // Assert
        result.Modified.Should().BeTrue();
        result.ScheduleStepTypeIds.Should().ContainSingle().And.Satisfy(x => x == ProcessStepTypeId.RETRIGGER_SYNCHRONIZE_USER);
        result.ProcessStepStatusId.Should().Be(ProcessStepStatusId.FAILED);
        result.ProcessMessage.Should().Be("this is a test");
        result.SkipStepTypeIds.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteProcessStep_WithWrongProcessStepTypeId_NothingHappens()
    {
        // Arrange InitializeProcess
        var validProcessId = Guid.NewGuid();
        var networkRegistrationId = Guid.NewGuid();
        A.CallTo(() => _networkRepository.GetNetworkRegistrationDataForProcessIdAsync(validProcessId))
            .Returns(networkRegistrationId);

        // Act InitializeProcess
        var initializeResult = await _sut.InitializeProcess(validProcessId, Enumerable.Empty<ProcessStepTypeId>());

        // Assert InitializeProcess
        initializeResult.Modified.Should().BeFalse();
        initializeResult.ScheduleStepTypeIds.Should().BeNull();

        // Act
        var result = await _sut.ExecuteProcessStep(ProcessStepTypeId.AWAIT_START_AUTOSETUP, Enumerable.Empty<ProcessStepTypeId>(), CancellationToken.None);

        // Assert
        A.CallTo(() => _networkRegistrationHandler.SynchronizeUser(networkRegistrationId)).MustNotHaveHappened();
        result.Modified.Should().BeFalse();
        result.ScheduleStepTypeIds.Should().BeNull();
        result.ProcessStepStatusId.Should().Be(ProcessStepStatusId.TODO);
        result.ProcessMessage.Should().BeNull();
        result.SkipStepTypeIds.Should().BeNull();
    }

    #endregion
}
