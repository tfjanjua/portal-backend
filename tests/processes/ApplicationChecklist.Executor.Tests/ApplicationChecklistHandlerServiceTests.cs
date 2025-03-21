/********************************************************************************
 * Copyright (c) 2022 Contributors to the Eclipse Foundation
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

using Org.Eclipse.TractusX.Portal.Backend.ApplicationActivation.Library;
using Org.Eclipse.TractusX.Portal.Backend.Bpdm.Library.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.BpnDidResolver.Library.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Clearinghouse.Library.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Custodian.Library.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Dim.Library.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Tests.Shared;
using Org.Eclipse.TractusX.Portal.Backend.IssuerComponent.Library.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.ApplicationChecklist.Executor;
using Org.Eclipse.TractusX.Portal.Backend.Processes.ApplicationChecklist.Library;
using Org.Eclipse.TractusX.Portal.Backend.SdFactory.Library.BusinessLogic;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.Portal.Backend.ApplicationChecklist.Executor.Tests;

public class ChecklistHandlerServiceTests
{
    private readonly IBpdmBusinessLogic _bpdmBusinessLogic;
    private readonly ICustodianBusinessLogic _custodianBusinessLogic;
    private readonly IClearinghouseBusinessLogic _clearinghouseBusinessLogic;
    private readonly ISdFactoryBusinessLogic _sdFactoryBusinessLogic;
    private readonly IDimBusinessLogic _dimBusinessLogic;
    private readonly IIssuerComponentBusinessLogic _issuerComponentBusinessLogic;
    private readonly IBpnDidResolverBusinessLogic _bpnDidResolverBusinessLogic;
    private readonly IApplicationActivationService _applicationActivationService;
    private readonly IApplicationChecklistService _checklistService;
    private readonly IFixture _fixture;

    public ChecklistHandlerServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _bpdmBusinessLogic = A.Fake<IBpdmBusinessLogic>();
        _custodianBusinessLogic = A.Fake<ICustodianBusinessLogic>();
        _clearinghouseBusinessLogic = A.Fake<IClearinghouseBusinessLogic>();
        _sdFactoryBusinessLogic = A.Fake<ISdFactoryBusinessLogic>();
        _dimBusinessLogic = A.Fake<IDimBusinessLogic>();
        _issuerComponentBusinessLogic = A.Fake<IIssuerComponentBusinessLogic>();
        _bpnDidResolverBusinessLogic = A.Fake<IBpnDidResolverBusinessLogic>();
        _applicationActivationService = A.Fake<IApplicationActivationService>();
        _checklistService = A.Fake<IApplicationChecklistService>();
    }

    [Theory]
    [InlineData(ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_PUSH, ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER)]
    [InlineData(ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_PULL, ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER)]
    [InlineData(ProcessStepTypeId.CREATE_IDENTITY_WALLET, ApplicationChecklistEntryTypeId.IDENTITY_WALLET)]
    [InlineData(ProcessStepTypeId.CREATE_DIM_WALLET, ApplicationChecklistEntryTypeId.IDENTITY_WALLET)]
    [InlineData(ProcessStepTypeId.VALIDATE_DID_DOCUMENT, ApplicationChecklistEntryTypeId.IDENTITY_WALLET)]
    [InlineData(ProcessStepTypeId.TRANSMIT_BPN_DID, ApplicationChecklistEntryTypeId.IDENTITY_WALLET)]
    [InlineData(ProcessStepTypeId.START_CLEARING_HOUSE, ApplicationChecklistEntryTypeId.CLEARING_HOUSE)]
    [InlineData(ProcessStepTypeId.START_OVERRIDE_CLEARING_HOUSE, ApplicationChecklistEntryTypeId.CLEARING_HOUSE)]
    [InlineData(ProcessStepTypeId.START_SELF_DESCRIPTION_LP, ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP)]
    [InlineData(ProcessStepTypeId.START_APPLICATION_ACTIVATION, ApplicationChecklistEntryTypeId.APPLICATION_ACTIVATION)]
    [InlineData(ProcessStepTypeId.ASSIGN_INITIAL_ROLES, ApplicationChecklistEntryTypeId.APPLICATION_ACTIVATION)]
    [InlineData(ProcessStepTypeId.ASSIGN_BPN_TO_USERS, ApplicationChecklistEntryTypeId.APPLICATION_ACTIVATION)]
    [InlineData(ProcessStepTypeId.REMOVE_REGISTRATION_ROLES, ApplicationChecklistEntryTypeId.APPLICATION_ACTIVATION)]
    public void GetProcessStepExecution_ExecutableStep_Success(ProcessStepTypeId stepTypeId, ApplicationChecklistEntryTypeId entryTypeId)
    {
        // Arrange
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(
            Guid.NewGuid(),
            stepTypeId,
            _fixture.Create<IDictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>>().ToImmutableDictionary(),
            _fixture.CreateMany<ProcessStepTypeId>());

        var error = new TestException();

        var sut = CreateSut();

        // Act IsExecutableProcessStep
        var isExecutable = sut.IsExecutableProcessStep(stepTypeId);

        // Assert IsExecutableProcessStep
        isExecutable.Should().BeTrue();

        // Act GetProcessStepExecution
        var execution = sut.GetProcessStepExecution(stepTypeId);

        // Assert GetProcessStepExecution
        execution.EntryTypeId.Should().Be(entryTypeId);

        // Act execute process-func
        execution.ProcessFunc(context, CancellationToken.None);

        // Assert process-func
        switch (stepTypeId)
        {
            case ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_PUSH:
                A.CallTo(() => _bpdmBusinessLogic.PushLegalEntity(context, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
                break;
            case ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_PULL:
                A.CallTo(() => _bpdmBusinessLogic.HandlePullLegalEntity(context, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
                break;
            case ProcessStepTypeId.CREATE_IDENTITY_WALLET:
                A.CallTo(() => _custodianBusinessLogic.CreateIdentityWalletAsync(context, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
                break;
            case ProcessStepTypeId.CREATE_DIM_WALLET:
                A.CallTo(() => _dimBusinessLogic.CreateDimWalletAsync(context, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
                break;
            case ProcessStepTypeId.VALIDATE_DID_DOCUMENT:
                A.CallTo(() => _dimBusinessLogic.ValidateDidDocument(context, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
                break;
            case ProcessStepTypeId.TRANSMIT_BPN_DID:
                A.CallTo(() => _bpnDidResolverBusinessLogic.TransmitDidAndBpn(context, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
                break;
            case ProcessStepTypeId.START_CLEARING_HOUSE:
                A.CallTo(() => _clearinghouseBusinessLogic.HandleClearinghouse(context, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
                break;
            case ProcessStepTypeId.START_OVERRIDE_CLEARING_HOUSE:
                A.CallTo(() => _clearinghouseBusinessLogic.HandleClearinghouse(context, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
                break;
            case ProcessStepTypeId.START_SELF_DESCRIPTION_LP:
                A.CallTo(() => _sdFactoryBusinessLogic.StartSelfDescriptionRegistration(context, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
                break;
            case ProcessStepTypeId.START_APPLICATION_ACTIVATION:
                A.CallTo(() => _applicationActivationService.StartApplicationActivation(context, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
                break;
            case ProcessStepTypeId.ASSIGN_INITIAL_ROLES:
                A.CallTo(() => _applicationActivationService.AssignRoles(context, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
                break;
            case ProcessStepTypeId.ASSIGN_BPN_TO_USERS:
                A.CallTo(() => _applicationActivationService.AssignBpn(context, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
                break;
            case ProcessStepTypeId.REMOVE_REGISTRATION_ROLES:
                A.CallTo(() => _applicationActivationService.RemoveRegistrationRoles(context, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
                break;
            case ProcessStepTypeId.SET_THEME:
                A.CallTo(() => _applicationActivationService.SetTheme(context, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
                break;
            case ProcessStepTypeId.SET_MEMBERSHIP:
                A.CallTo(() => _applicationActivationService.SetMembership(context, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
                break;
            case ProcessStepTypeId.SET_CX_MEMBERSHIP_IN_BPDM:
                A.CallTo(() => _applicationActivationService.SetCxMembership(context, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
                break;
            case ProcessStepTypeId.FINISH_APPLICATION_ACTIVATION:
                A.CallTo(() => _applicationActivationService.SaveApplicationActivationToDatabase(context, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
                break;
            default:
                true.Should().BeFalse($"unexpected ProcessStepTypeId: {stepTypeId}");
                break;
        }

        // Act execute error-func
        execution.ErrorFunc?.Invoke(error, context, CancellationToken.None);

        // Assert error-func
        switch (stepTypeId)
        {
            case ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_PUSH:
                execution.ErrorFunc?.Should().NotBeNull();
                A.CallTo(() => _checklistService.HandleServiceErrorAsync(error, ProcessStepTypeId.RETRIGGER_BUSINESS_PARTNER_NUMBER_PUSH)).MustHaveHappenedOnceExactly();
                break;
            case ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_PULL:
                execution.ErrorFunc?.Should().NotBeNull();
                A.CallTo(() => _checklistService.HandleServiceErrorAsync(error, ProcessStepTypeId.RETRIGGER_BUSINESS_PARTNER_NUMBER_PULL)).MustHaveHappenedOnceExactly();
                break;
            case ProcessStepTypeId.CREATE_IDENTITY_WALLET:
                execution.ErrorFunc?.Should().NotBeNull();
                A.CallTo(() => _checklistService.HandleServiceErrorAsync(error, ProcessStepTypeId.RETRIGGER_IDENTITY_WALLET)).MustHaveHappenedOnceExactly();
                break;
            case ProcessStepTypeId.CREATE_DIM_WALLET:
                execution.ErrorFunc?.Should().NotBeNull();
                A.CallTo(() => _checklistService.HandleServiceErrorAsync(error, ProcessStepTypeId.RETRIGGER_CREATE_DIM_WALLET)).MustHaveHappenedOnceExactly();
                break;
            case ProcessStepTypeId.VALIDATE_DID_DOCUMENT:
                execution.ErrorFunc?.Should().NotBeNull();
                A.CallTo(() => _checklistService.HandleServiceErrorAsync(error, ProcessStepTypeId.RETRIGGER_VALIDATE_DID_DOCUMENT)).MustHaveHappenedOnceExactly();
                break;
            case ProcessStepTypeId.TRANSMIT_BPN_DID:
                execution.ErrorFunc?.Should().NotBeNull();
                A.CallTo(() => _checklistService.HandleServiceErrorAsync(error, ProcessStepTypeId.RETRIGGER_TRANSMIT_DID_BPN)).MustHaveHappenedOnceExactly();
                break;
            case ProcessStepTypeId.START_CLEARING_HOUSE:
                execution.ErrorFunc?.Should().NotBeNull();
                A.CallTo(() => _checklistService.HandleServiceErrorAsync(error, ProcessStepTypeId.RETRIGGER_CLEARING_HOUSE)).MustHaveHappenedOnceExactly();
                break;
            case ProcessStepTypeId.START_OVERRIDE_CLEARING_HOUSE:
                execution.ErrorFunc?.Should().NotBeNull();
                A.CallTo(() => _checklistService.HandleServiceErrorAsync(error, ProcessStepTypeId.RETRIGGER_CLEARING_HOUSE)).MustHaveHappenedOnceExactly();
                break;
            case ProcessStepTypeId.START_SELF_DESCRIPTION_LP:
                execution.ErrorFunc?.Should().NotBeNull();
                A.CallTo(() => _checklistService.HandleServiceErrorAsync(error, ProcessStepTypeId.RETRIGGER_SELF_DESCRIPTION_LP)).MustHaveHappenedOnceExactly();
                break;
            case ProcessStepTypeId.START_APPLICATION_ACTIVATION:
                execution.ErrorFunc?.Should().BeNull();
                A.CallTo(() => _checklistService.HandleServiceErrorAsync(A<Exception>._, A<ProcessStepTypeId>._)).MustNotHaveHappened();
                break;
            case ProcessStepTypeId.ASSIGN_INITIAL_ROLES:
                execution.ErrorFunc?.Should().NotBeNull();
                A.CallTo(() => _checklistService.HandleServiceErrorAsync(error, ProcessStepTypeId.RETRIGGER_ASSIGN_INITIAL_ROLES)).MustHaveHappenedOnceExactly();
                break;
            case ProcessStepTypeId.ASSIGN_BPN_TO_USERS:
                execution.ErrorFunc?.Should().NotBeNull();
                A.CallTo(() => _checklistService.HandleServiceErrorAsync(error, ProcessStepTypeId.RETRIGGER_ASSIGN_BPN_TO_USERS)).MustHaveHappenedOnceExactly();
                break;
            case ProcessStepTypeId.REMOVE_REGISTRATION_ROLES:
                execution.ErrorFunc?.Should().NotBeNull();
                A.CallTo(() => _checklistService.HandleServiceErrorAsync(error, ProcessStepTypeId.RETRIGGER_REMOVE_REGISTRATION_ROLES)).MustHaveHappenedOnceExactly();
                break;
            case ProcessStepTypeId.SET_THEME:
                execution.ErrorFunc?.Should().NotBeNull();
                A.CallTo(() => _checklistService.HandleServiceErrorAsync(error, ProcessStepTypeId.RETRIGGER_SET_THEME)).MustHaveHappenedOnceExactly();
                break;
            case ProcessStepTypeId.SET_MEMBERSHIP:
                execution.ErrorFunc?.Should().NotBeNull();
                A.CallTo(() => _checklistService.HandleServiceErrorAsync(error, ProcessStepTypeId.RETRIGGER_SET_MEMBERSHIP)).MustHaveHappenedOnceExactly();
                break;
            case ProcessStepTypeId.SET_CX_MEMBERSHIP_IN_BPDM:
                execution.ErrorFunc?.Should().NotBeNull();
                A.CallTo(() => _checklistService.HandleServiceErrorAsync(error, ProcessStepTypeId.RETRIGGER_SET_CX_MEMBERSHIP_IN_BPDM)).MustHaveHappenedOnceExactly();
                break;
            case ProcessStepTypeId.FINISH_APPLICATION_ACTIVATION:
                execution.ErrorFunc?.Should().BeNull();
                A.CallTo(() => _checklistService.HandleServiceErrorAsync(A<Exception>._, A<ProcessStepTypeId>._)).MustNotHaveHappened();
                break;
            default:
                true.Should().BeFalse($"unexpected ProcessStepTypeId: {stepTypeId}");
                break;
        }
    }

    [Theory]
    [InlineData(ProcessStepTypeId.MANUAL_VERIFY_REGISTRATION)]
    [InlineData(ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_MANUAL)]
    [InlineData(ProcessStepTypeId.RETRIGGER_IDENTITY_WALLET)]
    [InlineData(ProcessStepTypeId.RETRIGGER_CLEARING_HOUSE)]
    [InlineData(ProcessStepTypeId.RETRIGGER_CREATE_DIM_WALLET)]
    [InlineData(ProcessStepTypeId.RETRIGGER_VALIDATE_DID_DOCUMENT)]
    [InlineData(ProcessStepTypeId.AWAIT_CLEARING_HOUSE_RESPONSE)]
    [InlineData(ProcessStepTypeId.RETRIGGER_SELF_DESCRIPTION_LP)]
    [InlineData(ProcessStepTypeId.RETRIGGER_BUSINESS_PARTNER_NUMBER_PUSH)]
    [InlineData(ProcessStepTypeId.RETRIGGER_BUSINESS_PARTNER_NUMBER_PULL)]
    [InlineData(ProcessStepTypeId.MANUAL_TRIGGER_OVERRIDE_CLEARING_HOUSE)]
    [InlineData(ProcessStepTypeId.AWAIT_SELF_DESCRIPTION_LP_RESPONSE)]
    public void GetProcessStepExecution_InvalidStep_Throws(ProcessStepTypeId stepTypeId)
    {
        // Arrange
        var sut = CreateSut();

        var Act = () => sut.GetProcessStepExecution(stepTypeId);

        // Act
        var isExecutable = sut.IsExecutableProcessStep(stepTypeId);
        var result = Assert.Throws<ConflictException>(Act);

        // Assert
        isExecutable.Should().BeFalse();
        result.Message.Should().Be($"no execution defined for processStep {stepTypeId}");
    }

    private IApplicationChecklistHandlerService CreateSut() =>
        new ApplicationChecklistHandlerService(
            _bpdmBusinessLogic,
            _custodianBusinessLogic,
            _clearinghouseBusinessLogic,
            _sdFactoryBusinessLogic,
            _dimBusinessLogic,
            _issuerComponentBusinessLogic,
            _bpnDidResolverBusinessLogic,
            _applicationActivationService,
            _checklistService);
}
