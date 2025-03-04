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

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Extensions;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Clearinghouse.Library.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Clearinghouse.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Dim.Library.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Dim.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Extensions;
using Org.Eclipse.TractusX.Portal.Backend.IssuerComponent.Library.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.IssuerComponent.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Extensions;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Extensions;
using Org.Eclipse.TractusX.Portal.Backend.Processes.ApplicationChecklist.Library;
using Org.Eclipse.TractusX.Portal.Backend.Processes.Mailing.Library;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Common;
using Org.Eclipse.TractusX.Portal.Backend.SdFactory.Library.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.SdFactory.Library.Models;
using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;

public sealed class RegistrationBusinessLogic(
    IPortalRepositories portalRepositories,
    IOptions<RegistrationSettings> configuration,
    IApplicationChecklistService checklistService,
    IClearinghouseBusinessLogic clearinghouseBusinessLogic,
    ISdFactoryBusinessLogic sdFactoryBusinessLogic,
    IDimBusinessLogic dimBusinessLogic,
    IIssuerComponentBusinessLogic issuerComponentBusinessLogic,
    IProvisioningManager provisioningManager,
    IMailingProcessCreation mailingProcessCreation,
    ILogger<RegistrationBusinessLogic> logger)
    : IRegistrationBusinessLogic
{
    private static readonly Regex BpnRegex = new(ValidationExpressions.Bpn, RegexOptions.Compiled, TimeSpan.FromSeconds(1));

    private readonly RegistrationSettings _settings = configuration.Value;

    public Task<CompanyWithAddressData> GetCompanyWithAddressAsync(Guid applicationId)
    {
        if (applicationId == Guid.Empty)
        {
            throw new ArgumentNullException(nameof(applicationId));
        }

        return GetCompanyWithAddressAsyncInternal(applicationId);
    }

    private async Task<CompanyWithAddressData> GetCompanyWithAddressAsyncInternal(Guid applicationId)
    {
        var companyWithAddress = await portalRepositories.GetInstance<IApplicationRepository>().GetCompanyUserRoleWithAddressUntrackedAsync(applicationId, _settings.DocumentTypeIds).ConfigureAwait(ConfigureAwaitOptions.None);
        if (companyWithAddress == null)
        {
            throw NotFoundException.Create(AdministrationRegistrationErrors.APPLICATION_NOT_FOUND, [new ErrorParameter(nameof(applicationId), applicationId.ToString())]);
        }
        if (!companyWithAddress.Name.IsValidCompanyName())
        {
            throw ControllerArgumentException.Create(ValidationExpressionErrors.INCORRECT_COMPANY_NAME, [new ErrorParameter("name", "OrganisationName")]);
        }

        return new CompanyWithAddressData(
            companyWithAddress.CompanyId,
            companyWithAddress.Name,
            companyWithAddress.Shortname ?? "",
            companyWithAddress.BusinessPartnerNumber ?? "",
            companyWithAddress.City ?? "",
            companyWithAddress.StreetName ?? "",
            companyWithAddress.CountryAlpha2Code ?? "",
            companyWithAddress.Region ?? "",
            companyWithAddress.Streetadditional ?? "",
            companyWithAddress.Streetnumber ?? "",
            companyWithAddress.Zipcode ?? "",
            companyWithAddress.AgreementsData
                .GroupBy(x => x.CompanyRoleId)
                .Select(g => new AgreementsRoleData(
                    g.Key,
                    g.Select(y => new AgreementConsentData(
                        y.AgreementId,
                        y.ConsentStatusId ?? ConsentStatusId.INACTIVE)))),
            companyWithAddress.InvitedCompanyUserData
                .Select(x => new InvitedUserData(
                    x.UserId,
                    x.FirstName ?? "",
                    x.LastName ?? "",
                    x.Email ?? "")),
            companyWithAddress.CompanyIdentifiers.Select(identifier => new CompanyUniqueIdData(identifier.UniqueIdentifierId, identifier.Value)),
            companyWithAddress.DocumentData.Select(data => new DocumentDetails(data.DocumentId, data.DocumentTypeId)),
            companyWithAddress.Created,
            companyWithAddress.LastChanged
        );
    }

    public Task<Pagination.Response<CompanyApplicationDetails>> GetCompanyApplicationDetailsAsync(int page, int size, CompanyApplicationStatusFilter? companyApplicationStatusFilter, string? companyName)
    {
        if (companyName != null && !companyName.IsValidCompanyName())
        {
            throw ControllerArgumentException.Create(ValidationExpressionErrors.INCORRECT_COMPANY_NAME, [new ErrorParameter("name", "CompanyName")]);
        }
        var applications = portalRepositories.GetInstance<IApplicationRepository>()
            .GetCompanyApplicationsFilteredQuery(
                companyName?.Length >= 3 ? companyName : null,
                companyApplicationStatusFilter.GetCompanyApplicationStatusIds());

        return Pagination.CreateResponseAsync(
            page,
            size,
            _settings.ApplicationsMaxPageSize,
            (skip, take) => new Pagination.AsyncSource<CompanyApplicationDetails>(
                applications.CountAsync(),
                applications
                    .AsSplitQuery()
                    .OrderByDescending(application => application.DateCreated)
                    .Skip(skip)
                    .Take(take)
                    .Select(application => new CompanyApplicationDetails(
                        application.Id,
                        application.ApplicationStatusId,
                        application.DateCreated,
                        application.Company!.Name,
                        application.Company!.CompanyAssignedRoles.Select(companyAssignedRoles => companyAssignedRoles.CompanyRoleId),
                        application.ApplicationChecklistEntries.Where(x => x.ApplicationChecklistEntryTypeId != ApplicationChecklistEntryTypeId.APPLICATION_ACTIVATION).OrderBy(x => x.ApplicationChecklistEntryTypeId).Select(x => new ApplicationChecklistEntryDetails(x.ApplicationChecklistEntryTypeId, x.ApplicationChecklistEntryStatusId)),
                        application.Invitations
                            .Select(invitation => invitation.CompanyUser)
                            .Where(companyUser => companyUser!.Identity!.UserStatusId == UserStatusId.ACTIVE
                                && companyUser.Email != null)
                            .Select(companyUser => companyUser!.Email)
                            .FirstOrDefault(),
                        application.Company.BusinessPartnerNumber,
                        application.CompanyApplicationTypeId))
                    .AsAsyncEnumerable()));
    }

    public Task<Pagination.Response<CompanyApplicationWithCompanyUserDetails>> GetAllCompanyApplicationsDetailsAsync(int page, int size, string? companyName)
    {
        if (companyName != null && !companyName.IsValidCompanyName())
        {
            throw ControllerArgumentException.Create(ValidationExpressionErrors.INCORRECT_COMPANY_NAME, [new ErrorParameter("name", "CompanyName")]);
        }
        var applications = portalRepositories.GetInstance<IApplicationRepository>().GetAllCompanyApplicationsDetailsQuery(companyName);

        return Pagination.CreateResponseAsync(
            page,
            size,
            _settings.ApplicationsMaxPageSize,
            (skip, take) => new Pagination.AsyncSource<CompanyApplicationWithCompanyUserDetails>(
                applications.CountAsync(),
                applications.OrderByDescending(application => application.Company!.DateCreated)
                    .Skip(skip)
                    .Take(take)
                    .Select(application => new
                    {
                        Application = application,
                        CompanyUsers = application.Invitations.Select(invitation => invitation.CompanyUser)
                    })
                    .Select(x => new
                    {
                        x.Application,
                        CompanyUser = x.CompanyUsers.FirstOrDefault(companyUser =>
                                    companyUser!.Identity!.UserStatusId == UserStatusId.ACTIVE
                                    && companyUser!.Firstname != null
                                    && companyUser.Lastname != null
                                    && companyUser.Email != null)
                            ?? x.CompanyUsers.FirstOrDefault(companyUser =>
                                    companyUser!.Firstname != null
                                    && companyUser.Lastname != null
                                    && companyUser.Email != null)
                    })
                    .Select(s => new CompanyApplicationWithCompanyUserDetails(
                        s.Application.Id,
                        s.Application.ApplicationStatusId,
                        s.Application.DateCreated,
                        s.Application.Company!.Name)
                    {
                        FirstName = s.CompanyUser!.Firstname,
                        LastName = s.CompanyUser.Lastname,
                        Email = s.CompanyUser.Email
                    })
                    .AsAsyncEnumerable()));
    }

    /// <inheritdoc />
    public Task UpdateCompanyBpn(Guid applicationId, string bpn)
    {
        if (!BpnRegex.IsMatch(bpn))
        {
            throw ControllerArgumentException.Create(AdministrationRegistrationErrors.REGISTRATION_ARGUMENT_BPN_MUST_SIXTEEN_CHAR_LONG, new ErrorParameter[] { new(nameof(bpn), bpn) });
        }

        if (!bpn.StartsWith("BPNL", StringComparison.OrdinalIgnoreCase))
        {
            throw ControllerArgumentException.Create(AdministrationRegistrationErrors.REGISTRATION_ARGUMENT_BPNL_PREFIXED_BPNL, new ErrorParameter[] { new(nameof(bpn), bpn) });
        }

        return UpdateCompanyBpnInternal(applicationId, bpn);
    }

    private async Task UpdateCompanyBpnInternal(Guid applicationId, string bpn)
    {
        var result = await portalRepositories.GetInstance<IUserRepository>()
            .GetBpnForIamUserUntrackedAsync(applicationId, bpn.ToUpper()).ToListAsync().ConfigureAwait(false);
        if (!result.Exists(item => item.IsApplicationCompany))
        {
            throw NotFoundException.Create(AdministrationRegistrationErrors.REGISTRATION_NOT_APPLICATION_FOUND, new ErrorParameter[] { new(nameof(applicationId), applicationId.ToString()) });
        }

        if (result.Exists(item => !item.IsApplicationCompany))
        {
            throw ConflictException.Create(AdministrationRegistrationErrors.REGISTRATION_BPN_ASSIGN_TO_OTHER_COMP);
        }

        var applicationCompanyData = result.Single(item => item.IsApplicationCompany);
        if (!applicationCompanyData.IsApplicationPending)
        {
            throw ConflictException.Create(AdministrationRegistrationErrors.REGISTRATION_CONFLICT_APPLICATION_FOR_COMPANY_NOT_PENDING, new ErrorParameter[] { new(nameof(applicationId), applicationId.ToString()), new("companyId", applicationCompanyData.CompanyId.ToString()) });
        }

        if (!string.IsNullOrWhiteSpace(applicationCompanyData.BusinessPartnerNumber))
        {
            throw ConflictException.Create(AdministrationRegistrationErrors.REGISTRATION_CONFLICT_BPN_OF_COMPANY_SET, new ErrorParameter[] { new("companyId", applicationCompanyData.CompanyId.ToString()) });
        }

        var context = await checklistService
            .VerifyChecklistEntryAndProcessSteps(
                applicationId,
                ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER,
                [
                    ApplicationChecklistEntryStatusId.TO_DO,
                    ApplicationChecklistEntryStatusId.IN_PROGRESS,
                    ApplicationChecklistEntryStatusId.FAILED
                ],
                ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_MANUAL,
                entryTypeIds: [
                    ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION
                ],
                processStepTypeIds: [
                    ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_PUSH,
                    ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_PULL,
                    ProcessStepTypeId.RETRIGGER_BUSINESS_PARTNER_NUMBER_PULL,
                    ProcessStepTypeId.RETRIGGER_BUSINESS_PARTNER_NUMBER_PUSH,
                    ProcessStepTypeId.CREATE_IDENTITY_WALLET
                ])
            .ConfigureAwait(ConfigureAwaitOptions.None);

        portalRepositories.GetInstance<ICompanyRepository>().AttachAndModifyCompany(applicationCompanyData.CompanyId, null,
            c => { c.BusinessPartnerNumber = bpn.ToUpper(); });

        var registrationValidationFailed = context.Checklist[ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION] == new ValueTuple<ApplicationChecklistEntryStatusId, string?>(ApplicationChecklistEntryStatusId.FAILED, null);

        checklistService.SkipProcessSteps(
            context,
            [
                ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_PUSH,
                ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_PULL,
                ProcessStepTypeId.RETRIGGER_BUSINESS_PARTNER_NUMBER_PULL,
                ProcessStepTypeId.RETRIGGER_BUSINESS_PARTNER_NUMBER_PUSH
            ]);

        checklistService.FinalizeChecklistEntryAndProcessSteps(
            context,
            null,
            entry => entry.ApplicationChecklistEntryStatusId = ApplicationChecklistEntryStatusId.DONE,
            registrationValidationFailed
                ? null
                : new[] { CreateWalletStep() });

        await portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }

    private ProcessStepTypeId CreateWalletStep() => _settings.UseDimWallet ? ProcessStepTypeId.CREATE_DIM_WALLET : ProcessStepTypeId.CREATE_IDENTITY_WALLET;

    /// <inheritdoc />
    public async Task ProcessClearinghouseResponseAsync(ClearinghouseResponseData data, string bpn, CancellationToken cancellationToken)
    {
        logger.LogInformation("Process SelfDescription called with the following data {Data} and bpn {Bpn}", data.ToString().Replace(Environment.NewLine, string.Empty), bpn.ToString().Replace(Environment.NewLine, string.Empty));
        var result = await portalRepositories.GetInstance<IApplicationRepository>().GetSubmittedApplicationIdsByBpn(bpn.ToUpper()).ToListAsync(cancellationToken).ConfigureAwait(false);
        if (!result.Any())
        {
            throw NotFoundException.Create(AdministrationRegistrationErrors.REGISTRATION_NOT_COMP_APP_BPN_STATUS_SUBMIT, new ErrorParameter[] { new("businessPartnerNumber", bpn) });
        }

        if (result.Count > 1)
        {
            throw ConflictException.Create(AdministrationRegistrationErrors.REGISTRATION_CONFLICT_APP_STATUS_STATUS_SUBMIT_FOUND_BPN, new ErrorParameter[] { new("businessPartnerNumber", bpn) });
        }

        await clearinghouseBusinessLogic.ProcessEndClearinghouse(result.Single(), data, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        await portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }

    /// <inheritdoc />
    public async Task ProcessDimResponseAsync(string bpn, DimWalletData data, CancellationToken cancellationToken)
    {
        logger.LogInformation("Process Dim called with the following data {Data}", data.DidDocument.RootElement.GetRawText().Replace(Environment.NewLine, string.Empty));

        await dimBusinessLogic.ProcessDimResponse(bpn, data, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        await portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ChecklistDetails>> GetChecklistForApplicationAsync(Guid applicationId)
    {
        var data = await portalRepositories.GetInstance<IApplicationRepository>()
            .GetApplicationChecklistData(applicationId, Enum.GetValues<ApplicationChecklistEntryTypeId>().GetManualTriggerProcessStepIds())
            .ConfigureAwait(ConfigureAwaitOptions.None);
        if (data == default)
        {
            throw NotFoundException.Create(AdministrationRegistrationErrors.APPLICATION_NOT_FOUND, [new("applicationId", applicationId.ToString())]);
        }

        return data.ChecklistData
            .OrderBy(x => x.TypeId)
            .Select(x =>
                new ChecklistDetails(
                    x.TypeId,
                    x.StatusId,
                    x.Comment,
                    data.ProcessStepTypeIds.Intersect(x.TypeId.GetManualTriggerProcessStepIds())));
    }

    /// <inheritdoc />
    public Task TriggerChecklistAsync(Guid applicationId, ApplicationChecklistEntryTypeId entryTypeId, ProcessStepTypeId processStepTypeId)
    {
        var possibleSteps = entryTypeId.GetManualTriggerProcessStepIds();
        if (!possibleSteps.Contains(processStepTypeId))
        {
            throw ControllerArgumentException.Create(AdministrationRegistrationErrors.REGISTRATION_ARGUMENT_PROCEES_TYPID_NOT_TRIGERABLE, new ErrorParameter[] { new(nameof(processStepTypeId), processStepTypeId.ToString()) });
        }

        var nextStepData = processStepTypeId.GetNextProcessStepDataForManualTriggerProcessStepId();
        if (nextStepData == default)
        {
            throw UnexpectedConditionException.Create(AdministrationRegistrationErrors.REGISTRATION_UNEXPECT_PROCESS_TYPID_CONFIGURED_TRIGERABLE, new ErrorParameter[] { new(nameof(processStepTypeId), processStepTypeId.ToString()) });
        }

        return TriggerChecklistInternal(applicationId, entryTypeId, processStepTypeId, nextStepData.ProcessStepTypeId, nextStepData.ChecklistEntryStatusId);
    }

    private async Task TriggerChecklistInternal(Guid applicationId, ApplicationChecklistEntryTypeId entryTypeId, ProcessStepTypeId processStepTypeId, ProcessStepTypeId nextProcessStepTypeId, ApplicationChecklistEntryStatusId checklistEntryStatusId)
    {
        var context = await checklistService
            .VerifyChecklistEntryAndProcessSteps(
                applicationId,
                entryTypeId,
                [ApplicationChecklistEntryStatusId.FAILED],
                processStepTypeId,
                processStepTypeIds: [nextProcessStepTypeId])
            .ConfigureAwait(ConfigureAwaitOptions.None);

        checklistService.FinalizeChecklistEntryAndProcessSteps(
            context,
            initial =>
            {
                if (context.Checklist.TryGetValue(entryTypeId, out var data))
                {
                    initial.Comment = data.Comment;
                }
            },
            item =>
            {
                item.ApplicationChecklistEntryStatusId = checklistEntryStatusId;
                item.Comment = null;
            },
            [nextProcessStepTypeId]);
        await portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }

    /// <inheritdoc />
    public async Task ProcessClearinghouseSelfDescription(SelfDescriptionResponseData data, CancellationToken cancellationToken)
    {
        logger.LogInformation("Process SelfDescription called with the following data {Data}", data.ToString().Replace(Environment.NewLine, string.Empty));
        var isExistingCompany = await portalRepositories.GetInstance<ICompanyRepository>().IsExistingCompany(data.ExternalId);

        if (isExistingCompany)
        {
            await sdFactoryBusinessLogic.ProcessFinishSelfDescriptionLpForCompany(data, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        }
        else
        {
            var result = await portalRepositories.GetInstance<IApplicationRepository>()
                .GetCompanyIdSubmissionStatusForApplication(data.ExternalId)
                .ConfigureAwait(ConfigureAwaitOptions.None);
            if (!result.IsValidApplicationId)
            {
                throw NotFoundException.Create(AdministrationRegistrationErrors.REGISTRATION_NOT_COMPANY_EXTERNAL_APP_NOT_FOUND, new ErrorParameter[] { new("externalId", data.ExternalId.ToString()) });
            }

            if (!result.IsSubmitted)
            {
                throw ConflictException.Create(AdministrationRegistrationErrors.REGISTRATION_NOT_COMPANY_EXTERNAL_NOT_STATUS_SUBMIT, new ErrorParameter[] { new("externalId", data.ExternalId.ToString()) });
            }

            await sdFactoryBusinessLogic.ProcessFinishSelfDescriptionLpForApplication(data, result.CompanyId, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        }

        await portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }

    /// <inheritdoc />
    public async Task ApproveRegistrationVerification(Guid applicationId)
    {
        var context = await checklistService
            .VerifyChecklistEntryAndProcessSteps(
                applicationId,
                ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION,
                [ApplicationChecklistEntryStatusId.TO_DO],
                ProcessStepTypeId.MANUAL_VERIFY_REGISTRATION,
                [ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER],
                [CreateWalletStep()])
            .ConfigureAwait(ConfigureAwaitOptions.None);

        var businessPartnerSuccess = context.Checklist[ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER] == new ValueTuple<ApplicationChecklistEntryStatusId, string?>(ApplicationChecklistEntryStatusId.DONE, null);

        checklistService.FinalizeChecklistEntryAndProcessSteps(
            context,
            null,
            entry =>
            {
                entry.ApplicationChecklistEntryStatusId = ApplicationChecklistEntryStatusId.DONE;
            },
            businessPartnerSuccess
                ? new[] { CreateWalletStep() }
                : null);

        await portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }

    public async Task DeclineRegistrationVerification(Guid applicationId, string comment, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(comment))
        {
            throw ConflictException.Create(AdministrationRegistrationErrors.REGISTRATION_CONFLICT_COMMENT_NOT_SET);
        }

        var result = await portalRepositories.GetInstance<IApplicationRepository>().GetCompanyIdNameForSubmittedApplication(applicationId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (result == default)
        {
            throw ControllerArgumentException.Create(AdministrationRegistrationErrors.REGISTRATION_ARGUMENT_COMP_APP_STATUS_NOTSUBMITTED, new ErrorParameter[] { new(nameof(applicationId), applicationId.ToString()) });
        }

        var (companyId, companyName, networkRegistrationProcessId, idps, companyUserIds) = result;

        var context = await checklistService
            .VerifyChecklistEntryAndProcessSteps(
                applicationId,
                ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION,
                [ApplicationChecklistEntryStatusId.TO_DO, ApplicationChecklistEntryStatusId.DONE],
                ProcessStepTypeId.MANUAL_DECLINE_APPLICATION,
                null,
                [ProcessStepTypeId.MANUAL_VERIFY_REGISTRATION,])
            .ConfigureAwait(ConfigureAwaitOptions.None);

        checklistService.SkipProcessSteps(context, [ProcessStepTypeId.MANUAL_VERIFY_REGISTRATION]);

        var identityProviderRepository = portalRepositories.GetInstance<IIdentityProviderRepository>();
        var userRepository = portalRepositories.GetInstance<IUserRepository>();
        foreach (var (idpId, idpAlias, idpType, linkedUserIds) in idps)
        {
            if (idpType == IdentityProviderTypeId.SHARED)
            {
                await provisioningManager.DeleteSharedIdpRealmAsync(idpAlias).ConfigureAwait(false);
            }

            if (idpType is IdentityProviderTypeId.OWN or IdentityProviderTypeId.SHARED)
            {
                await provisioningManager.DeleteCentralIdentityProviderAsync(idpAlias).ConfigureAwait(ConfigureAwaitOptions.None);
                identityProviderRepository.DeleteIamIdentityProvider(idpAlias);
                identityProviderRepository.DeleteIdentityProvider(idpId);
            }
            else
            {
                // a managed identityprovider is just unlinked from company and users
                identityProviderRepository.DeleteCompanyIdentityProvider(companyId, idpId);
                userRepository.RemoveCompanyUserAssignedIdentityProviders(linkedUserIds.Select(userId => (userId, idpId)));
            }
        }

        portalRepositories.GetInstance<IApplicationRepository>().AttachAndModifyCompanyApplication(applicationId, application =>
        {
            application.ApplicationStatusId = CompanyApplicationStatusId.DECLINED;
            application.DateLastChanged = DateTimeOffset.UtcNow;
        });
        portalRepositories.GetInstance<ICompanyRepository>().AttachAndModifyCompany(companyId, null, company =>
        {
            company.CompanyStatusId = CompanyStatusId.REJECTED;
        });

        foreach (var userId in companyUserIds)
        {
            var iamUserId = await provisioningManager.GetUserByUserName(userId.ToString()).ConfigureAwait(ConfigureAwaitOptions.None);
            if (iamUserId != null)
            {
                await provisioningManager.DeleteCentralRealmUserAsync(iamUserId).ConfigureAwait(ConfigureAwaitOptions.None);
            }
        }

        var emailData = await portalRepositories.GetInstance<IApplicationRepository>().GetEmailDataUntrackedAsync(applicationId).ToListAsync(cancellationToken).ConfigureAwait(false);
        userRepository.AttachAndModifyIdentities(companyUserIds.Select(userId => new ValueTuple<Guid, Action<Identity>?, Action<Identity>>(userId, null, identity => { identity.UserStatusId = UserStatusId.DELETED; })));

        checklistService.FinalizeChecklistEntryAndProcessSteps(
            context,
            null,
            entry =>
            {
                entry.ApplicationChecklistEntryStatusId = ApplicationChecklistEntryStatusId.FAILED;
                entry.Comment = comment;
            },
            networkRegistrationProcessId == null
                ? null
                : new[] { ProcessStepTypeId.TRIGGER_CALLBACK_OSP_DECLINED });

        PostRegistrationCancelEmailAsync(emailData, companyName, comment);
        await portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }

    private void PostRegistrationCancelEmailAsync(ICollection<EmailData> emailData, string companyName, string comment)
    {
        foreach (var user in emailData)
        {
            var userName = string.Join(" ", new[] { user.FirstName, user.LastName }.Where(item => !string.IsNullOrWhiteSpace(item)));

            if (string.IsNullOrWhiteSpace(user.Email))
            {
                throw ConflictException.Create(AdministrationRegistrationErrors.REGISTRATION_CONFLICT_EMAIL_NOT_ASSIGN_TO_USERNAME, new ErrorParameter[] { new(nameof(userName), userName) });
            }

            var mailParameters = ImmutableDictionary.CreateRange(new[]
            {
                KeyValuePair.Create("userName", !string.IsNullOrWhiteSpace(userName) ? userName : user.Email),
                KeyValuePair.Create("companyName", companyName),
                KeyValuePair.Create("declineComment", comment),
                KeyValuePair.Create("helpUrl", _settings.HelpAddress)
            });
            mailingProcessCreation.CreateMailProcess(user.Email, "EmailRegistrationDeclineTemplate", mailParameters);
        }
    }

    /// <inheritdoc />
    public async Task<(string fileName, byte[] content, string contentType)> GetDocumentAsync(Guid documentId)
    {
        var document = await portalRepositories.GetInstance<IDocumentRepository>()
            .GetDocumentByIdAsync(documentId, [DocumentTypeId.COMMERCIAL_REGISTER_EXTRACT])
            .ConfigureAwait(ConfigureAwaitOptions.None);
        if (document == null)
        {
            throw NotFoundException.Create(AdministrationRegistrationErrors.REGISTRATION_NOT_DOC_NOT_EXIST, new ErrorParameter[] { new(nameof(documentId), documentId.ToString()) });
        }

        return (document.DocumentName, document.DocumentContent, document.MediaTypeId.MapToMediaType());
    }

    /// <inheritdoc />
    public async Task ProcessIssuerBpnResponseAsync(IssuerResponseData data, CancellationToken cancellationToken)
    {
        var applicationId = await GetApplicationIdByBpn(data, cancellationToken);

        await issuerComponentBusinessLogic.StoreBpnlCredentialResponse(applicationId, data).ConfigureAwait(false);
        await portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task ProcessIssuerMembershipResponseAsync(IssuerResponseData data, CancellationToken cancellationToken)
    {
        var applicationId = await GetApplicationIdByBpn(data, cancellationToken);
        await issuerComponentBusinessLogic.StoreMembershipCredentialResponse(applicationId, data).ConfigureAwait(false);
        await portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    private async Task<Guid> GetApplicationIdByBpn(IssuerResponseData data, CancellationToken cancellationToken)
    {
        var result = await portalRepositories.GetInstance<IApplicationRepository>().GetSubmittedApplicationIdsByBpn(data.Bpn.ToUpper()).ToListAsync(cancellationToken).ConfigureAwait(false);
        if (!result.Any())
        {
            throw NotFoundException.Create(AdministrationRegistrationErrors.REGISTRATION_NOT_COMP_APP_BPN_STATUS_SUBMIT, new ErrorParameter[] { new("businessPartnerNumber", data.Bpn) });
        }

        if (result.Count > 1)
        {
            throw ConflictException.Create(AdministrationRegistrationErrors.REGISTRATION_CONFLICT_APP_STATUS_STATUS_SUBMIT_FOUND_BPN, new ErrorParameter[] { new("businessPartnerNumber", data.Bpn) });
        }

        return result.Single();
    }

    public Task RetriggerDeleteIdpSharedRealm(Guid processId) => ProcessStepTypeId.RETRIGGER_DELETE_IDP_SHARED_REALM.TriggerProcessStep(processId, portalRepositories, ProcessTypeExtensions.GetProcessStepForRetrigger);
    public Task RetriggerDeleteIdpSharedServiceAccount(Guid processId) => ProcessStepTypeId.RETRIGGER_DELETE_IDP_SHARED_SERVICEACCOUNT.TriggerProcessStep(processId, portalRepositories, ProcessTypeExtensions.GetProcessStepForRetrigger);
    public Task RetriggerDeleteCentralIdentityProvider(Guid processId) => ProcessStepTypeId.RETRIGGER_DELETE_CENTRAL_IDENTITY_PROVIDER.TriggerProcessStep(processId, portalRepositories, ProcessTypeExtensions.GetProcessStepForRetrigger);
    public Task RetriggerDeleteCentralUser(Guid processId) => ProcessStepTypeId.RETRIGGER_DELETE_CENTRAL_USER.TriggerProcessStep(processId, portalRepositories, ProcessTypeExtensions.GetProcessStepForRetrigger);
}
