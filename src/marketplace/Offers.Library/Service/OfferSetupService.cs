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

using Microsoft.Extensions.Logging;
using Org.Eclipse.TractusX.Portal.Backend.Dim.Library;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Async;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Identity;
using Org.Eclipse.TractusX.Portal.Backend.Framework.IO;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Configuration;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Notifications.Library;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.Mailing.Library;
using Org.Eclipse.TractusX.Portal.Backend.Processes.OfferSubscription.Library;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Service;
using System.Collections.Immutable;
using System.Text.Json;
using TechnicalUserData = Org.Eclipse.TractusX.Portal.Backend.Dim.Library.Models.TechnicalUserData;

namespace Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Service;

public class OfferSetupService(
    IPortalRepositories portalRepositories,
    IProvisioningManager provisioningManager,
    ITechnicalUserCreation technicalUserCreation,
    INotificationService notificationService,
    IOfferSubscriptionProcessService offerSubscriptionProcessService,
    ITechnicalUserProfileService technicalUserProfileService,
    IIdentityService identityService,
    IMailingProcessCreation mailingProcessCreation,
    IDimService dimService,
    ILogger<OfferSetupService> logger)
    : IOfferSetupService
{
    private readonly IIdentityData _identityData = identityService.IdentityData;

    public async Task<OfferAutoSetupResponseData> AutoSetupOfferAsync(OfferAutoSetupData data, IEnumerable<UserRoleConfig> itAdminRoles, OfferTypeId offerTypeId, string basePortalAddress, IEnumerable<UserRoleConfig> serviceManagerRoles)
    {
        logger.LogDebug("AutoSetup started from Company {CompanyId} for {RequestId} with OfferUrl: {OfferUrl}", _identityData.CompanyId, data.RequestId, data.OfferUrl.Replace(Environment.NewLine, string.Empty));
        if (data.OfferUrl.Contains('#', StringComparison.OrdinalIgnoreCase))
        {
            throw ControllerArgumentException.Create(OfferSetupServiceErrors.OFFERURL_NOT_CONTAIN, [new("offerUrl", data.OfferUrl)]);
        }

        var offerSubscriptionsRepository = portalRepositories.GetInstance<IOfferSubscriptionsRepository>();
        var offerDetails = await GetAndValidateOfferDetails(data.RequestId, _identityData.CompanyId, offerTypeId, offerSubscriptionsRepository).ConfigureAwait(ConfigureAwaitOptions.None);

        return await (offerDetails.InstanceData.IsSingleInstance
            ? AutoSetupOfferSingleInstance(data, offerDetails, itAdminRoles, offerTypeId, serviceManagerRoles, offerSubscriptionsRepository)
            : AutoSetupOfferMultiInstance(data, offerDetails, itAdminRoles, offerTypeId, basePortalAddress, serviceManagerRoles, offerSubscriptionsRepository)).ConfigureAwait(ConfigureAwaitOptions.None);
    }

    private async Task<OfferAutoSetupResponseData> AutoSetupOfferSingleInstance(OfferAutoSetupData data, OfferSubscriptionTransferData offerDetails, IEnumerable<UserRoleConfig> itAdminRoles, OfferTypeId offerTypeId, IEnumerable<UserRoleConfig> serviceManagerRoles, IOfferSubscriptionsRepository offerSubscriptionsRepository)
    {
        offerSubscriptionsRepository.AttachAndModifyOfferSubscription(data.RequestId, subscription =>
        {
            subscription.OfferSubscriptionStatusId = OfferSubscriptionStatusId.ACTIVE;
        });

        portalRepositories.GetInstance<IAppSubscriptionDetailRepository>()
            .CreateAppSubscriptionDetail(data.RequestId, appSubscriptionDetail =>
            {
                appSubscriptionDetail.AppInstanceId = offerDetails.AppInstanceIds.Single();
                appSubscriptionDetail.AppSubscriptionUrl = offerDetails.InstanceData.InstanceUrl;
            });
        await CreateNotifications(itAdminRoles, offerTypeId, offerDetails, _identityData.IdentityId).ConfigureAwait(ConfigureAwaitOptions.None);
        await SetNotificationsToDone(serviceManagerRoles, offerTypeId, offerDetails.OfferId, offerDetails.SalesManagerId).ConfigureAwait(ConfigureAwaitOptions.None);
        await portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
        return new OfferAutoSetupResponseData([], null);
    }

    private async Task<OfferAutoSetupResponseData> AutoSetupOfferMultiInstance(OfferAutoSetupData data, OfferSubscriptionTransferData offerDetails, IEnumerable<UserRoleConfig> itAdminRoles, OfferTypeId offerTypeId, string basePortalAddress, IEnumerable<UserRoleConfig> serviceManagerRoles, IOfferSubscriptionsRepository offerSubscriptionsRepository)
    {
        var userRolesRepository = portalRepositories.GetInstance<IUserRolesRepository>();
        ClientInfoData? clientInfoData = null;
        if (offerTypeId == OfferTypeId.APP)
        {
            var (clientId, iamClientId) = await CreateClient(data.OfferUrl, offerDetails.OfferId, true, userRolesRepository).ConfigureAwait(ConfigureAwaitOptions.None);
            clientInfoData = new ClientInfoData(clientId, data.OfferUrl);
            CreateAppInstance(data.RequestId, data.OfferUrl, offerDetails.OfferId, iamClientId);
        }

        var technicalUserClientId = clientInfoData?.ClientId ?? $"{offerDetails.OfferName}-{offerDetails.CompanyName}";
        var createTechnicalUserData = new CreateTechnicalUserData(offerDetails.CompanyId, offerDetails.OfferName, offerDetails.Bpn, technicalUserClientId, offerTypeId == OfferTypeId.APP, true);
        var technicalUsers = await CreateTechnicalUserForSubscription(data.RequestId, createTechnicalUserData, null).ConfigureAwait(ConfigureAwaitOptions.None);

        offerSubscriptionsRepository.AttachAndModifyOfferSubscription(data.RequestId, subscription =>
        {
            subscription.OfferSubscriptionStatusId = OfferSubscriptionStatusId.ACTIVE;
            subscription.ProcessId = technicalUsers.ProcessId;
        });

        await CreateNotifications(itAdminRoles, offerTypeId, offerDetails, _identityData.IdentityId).ConfigureAwait(ConfigureAwaitOptions.None);
        await SetNotificationsToDone(serviceManagerRoles, offerTypeId, offerDetails.OfferId, offerDetails.SalesManagerId).ConfigureAwait(ConfigureAwaitOptions.None);

        if (!string.IsNullOrWhiteSpace(offerDetails.RequesterEmail))
        {
            SendMail(basePortalAddress, $"{offerDetails.RequesterFirstname} {offerDetails.RequesterLastname}", offerDetails.RequesterEmail, offerDetails.OfferName, offerDetails.OfferTypeId);
        }

        await portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
        return new OfferAutoSetupResponseData(
            technicalUsers.ServiceAccounts.Select(x => new TechnicalUserInfoData(x.ServiceAccountId, x.UserRoleData.Select(ur => ur.UserRoleText), x.ServiceAccountData?.AuthData.Secret, x.ClientId)),
            clientInfoData);
    }

    private async Task<(bool HasExternalServiceAccount, Guid? ProcessId, IEnumerable<CreatedServiceAccountData> ServiceAccounts)> CreateTechnicalUserForSubscription(Guid subscriptionId, CreateTechnicalUserData data, Guid? processId)
    {
        var technicalUserInfoCreations = await technicalUserProfileService.GetTechnicalUserProfilesForOfferSubscription(subscriptionId).ConfigureAwait(ConfigureAwaitOptions.None);

        if (!technicalUserInfoCreations.Any())
        {
            return (false, null, []);
        }

        var serviceAccounts = new List<CreatedServiceAccountData>();
        var hasExternalServiceAccount = false;
        var processIdToUse = processId;
        foreach (var serviceAccountCreationInfo in technicalUserInfoCreations)
        {
            var serviceAccountResult = await technicalUserCreation
                .CreateTechnicalUsersAsync(
                    serviceAccountCreationInfo,
                    data.CompanyId,
                    data.Bpn == null ? [] : Enumerable.Repeat(data.Bpn, 1),
                    TechnicalUserTypeId.MANAGED,
                    data.EnhanceTechnicalUserName,
                    data.Enabled,
                    new ServiceAccountCreationProcessData(ProcessTypeId.OFFER_SUBSCRIPTION, processIdToUse),
                    sa => { sa.OfferSubscriptionId = subscriptionId; })
                .ConfigureAwait(ConfigureAwaitOptions.None);
            processIdToUse = serviceAccountResult.ProcessId;
            serviceAccounts.AddRange(serviceAccountResult.TechnicalUsers);
            hasExternalServiceAccount = hasExternalServiceAccount || serviceAccountResult.HasExternalTechnicalUser;
        }

        return (hasExternalServiceAccount, processIdToUse, serviceAccounts);
    }

    /// <inheritdoc />
    public async Task SetupSingleInstance(Guid offerId, string instanceUrl)
    {
        if (await portalRepositories.GetInstance<IAppInstanceRepository>()
               .CheckInstanceExistsForOffer(offerId)
               .ConfigureAwait(ConfigureAwaitOptions.None))
        {
            throw ConflictException.Create(OfferSetupServiceErrors.APP_INSTANCE_ALREADY_EXISTS, [new(nameof(offerId), offerId.ToString())]);
        }

        var userRolesRepository = portalRepositories.GetInstance<IUserRolesRepository>();
        var (_, iamClientId) = await CreateClient(instanceUrl, offerId, false, userRolesRepository);
        portalRepositories.GetInstance<IAppInstanceRepository>().CreateAppInstance(offerId, iamClientId);
    }

    /// <inheritdoc />
    public async Task DeleteSingleInstance(Guid appInstanceId, Guid clientId, string clientClientId)
    {
        var appInstanceRepository = portalRepositories.GetInstance<IAppInstanceRepository>();
        if (await appInstanceRepository.CheckInstanceHasAssignedSubscriptions(appInstanceId))
        {
            throw ConflictException.Create(OfferSetupServiceErrors.APP_INSTANCE_ASSOCIATED_WITH_SUBSCRIPTIONS, [new("appInstanceId", appInstanceId.ToString())]);
        }

        await provisioningManager.DeleteCentralClientAsync(clientClientId)
            .ConfigureAwait(ConfigureAwaitOptions.None);
        portalRepositories.GetInstance<IClientRepository>().RemoveClient(clientId);
        var serviceAccountIds = await appInstanceRepository.GetAssignedServiceAccounts(appInstanceId).ToListAsync().ConfigureAwait(false);
        if (serviceAccountIds.Any())
        {
            appInstanceRepository.RemoveAppInstanceAssignedServiceAccounts(appInstanceId, serviceAccountIds);
        }

        appInstanceRepository.RemoveAppInstance(appInstanceId);
    }

    public async Task<IEnumerable<string?>> ActivateSingleInstanceAppAsync(Guid offerId)
    {
        var data = await portalRepositories.GetInstance<IOfferRepository>().GetSingleInstanceOfferData(offerId, OfferTypeId.APP).ConfigureAwait(ConfigureAwaitOptions.None);
        if (data == null)
        {
            throw ConflictException.Create(OfferSetupServiceErrors.APP_DOES_NOT_EXIST, [new(nameof(offerId), offerId.ToString())]);
        }

        if (!data.IsSingleInstance)
        {
            throw ConflictException.Create(OfferSetupServiceErrors.OFFER_NOT_SINGLE_INSTANCE, [new(nameof(offerId), offerId.ToString())]);
        }

        Guid instanceId;
        string internalClientId;
        try
        {
            (instanceId, internalClientId) = data.Instances.Single();
        }
        catch (InvalidOperationException)
        {
            throw UnexpectedConditionException.Create(OfferSetupServiceErrors.SINGLE_INSTANCE_OFFER_MUST_HAVE_ONE_INSTANCE, [new(nameof(offerId), offerId.ToString())]);
        }

        if (string.IsNullOrEmpty(internalClientId))
        {
            throw ConflictException.Create(OfferSetupServiceErrors.CLIENTID_EMPTY_FOR_SINGLE_INSTANCE, [new(nameof(offerId), offerId.ToString())]);
        }

        await provisioningManager.EnableClient(internalClientId).ConfigureAwait(ConfigureAwaitOptions.None);
        var technicalUserData = await CreateTechnicalUsersForOffer(offerId, OfferTypeId.APP, new CreateTechnicalUserData(data.CompanyId, data.OfferName, data.Bpn, internalClientId, true, true)).ToListAsync()
            .ConfigureAwait(false);

        portalRepositories.GetInstance<IAppInstanceRepository>().CreateAppInstanceAssignedServiceAccounts(technicalUserData.SelectMany(x => x.Select(y => (instanceId, y.TechnicalUserId))));

        return technicalUserData.SelectMany(x => x.Select(y => y.TechnicalClientId));
    }

    private async IAsyncEnumerable<IEnumerable<TechnicalUserInfoData>> CreateTechnicalUsersForOffer(
        Guid offerId,
        OfferTypeId offerTypeId,
        CreateTechnicalUserData data)
    {
        var creationData = await technicalUserProfileService.GetTechnicalUserProfilesForOffer(offerId, offerTypeId).ConfigureAwait(ConfigureAwaitOptions.None);
        foreach (var creationInfo in creationData)
        {
            var (_, _, result) = await technicalUserCreation
                .CreateTechnicalUsersAsync(
                    creationInfo,
                    data.CompanyId,
                    data.Bpn == null ? [] : [data.Bpn],
                    TechnicalUserTypeId.MANAGED,
                    data.EnhanceTechnicalUserName,
                    data.Enabled,
                    new ServiceAccountCreationProcessData(ProcessTypeId.DIM_TECHNICAL_USER, null))
                .ConfigureAwait(ConfigureAwaitOptions.None);
            yield return result.Select(x => new TechnicalUserInfoData(x.ServiceAccountId, x.UserRoleData.Select(ur => ur.UserRoleText), x.ServiceAccountData?.AuthData.Secret, x.ClientId));
        }
    }

    /// <inheritdoc />
    public Task UpdateSingleInstance(string clientClientId, string instanceUrl) =>
        provisioningManager.UpdateClient(clientClientId, instanceUrl, instanceUrl.AppendToPathEncoded("*"));

    private static async Task<OfferSubscriptionTransferData> GetAndValidateOfferDetails(Guid requestId, Guid companyId, OfferTypeId offerTypeId, IOfferSubscriptionsRepository offerSubscriptionsRepository)
    {
        var offerDetails = await offerSubscriptionsRepository
            .GetOfferDetailsAndCheckProviderCompany(requestId, companyId, offerTypeId)
            .ConfigureAwait(ConfigureAwaitOptions.None);
        if (offerDetails == null)
        {
            throw NotFoundException.Create(OfferSetupServiceErrors.OFFER_SUBCRIPTION_NOT_EXIST, [new("requestId", requestId.ToString())]);
        }

        if (offerDetails.Status is not OfferSubscriptionStatusId.PENDING)
        {
            throw ConflictException.Create(OfferSetupServiceErrors.OFFER_SUBSCRIPTION_PENDING);
        }

        if (!offerDetails.IsProviderCompany)
        {
            throw ForbiddenException.Create(OfferSetupServiceErrors.ONLY_PROVIDER_CAN_SETUP_SERVICE);
        }

        if (offerDetails.InstanceData.IsSingleInstance && offerDetails.AppInstanceIds.Count() != 1)
        {
            throw ConflictException.Create(OfferSetupServiceErrors.ONLY_ONE_APP_INSTANCE_FOR_SINGLE_INSTANCE);
        }

        return offerDetails;
    }

    private async Task<(string clientId, Guid iamClientId)> CreateClient(string offerUrl, Guid offerId, bool enabled, IUserRolesRepository userRolesRepository)
    {
        var userRoles = await userRolesRepository.GetUserRolesForOfferIdAsync(offerId).ToListAsync().ConfigureAwait(false);
        offerUrl.EnsureValidHttpUrl(() => nameof(offerUrl));
        var redirectUrl = offerUrl.AppendToPathEncoded("*");

        var clientId = await provisioningManager.SetupClientAsync(redirectUrl, offerUrl, userRoles, enabled)
            .ConfigureAwait(ConfigureAwaitOptions.None);
        var iamClient = portalRepositories.GetInstance<IClientRepository>().CreateClient(clientId);
        return (clientId, iamClient.Id);
    }

    private void CreateAppInstance(
        Guid offerSubscriptionId,
        string offerUrl,
        Guid offerId,
        Guid iamClientId)
    {
        var appInstance = portalRepositories.GetInstance<IAppInstanceRepository>()
            .CreateAppInstance(offerId, iamClientId);
        portalRepositories.GetInstance<IAppSubscriptionDetailRepository>()
            .CreateAppSubscriptionDetail(offerSubscriptionId, appSubscriptionDetail =>
            {
                appSubscriptionDetail.AppInstanceId = appInstance.Id;
                appSubscriptionDetail.AppSubscriptionUrl = offerUrl;
            });
    }

    private async Task CreateNotifications(
        IEnumerable<UserRoleConfig> itAdminRoles,
        OfferTypeId offerTypeId,
        OfferSubscriptionTransferData offerDetails,
        Guid userId)
    {
        var appSubscriptionActivation = offerTypeId == OfferTypeId.APP
            ? NotificationTypeId.APP_SUBSCRIPTION_ACTIVATION
            : NotificationTypeId.SERVICE_ACTIVATION;
        var notificationContent = JsonSerializer.Serialize(new
        {
            offerDetails.OfferId,
            offerDetails.CompanyName,
            offerDetails.OfferName
        });
        var notifications = new List<(string?, NotificationTypeId)>
        {
            (notificationContent, appSubscriptionActivation)
        };

        if (!offerDetails.InstanceData.IsSingleInstance)
        {
            notifications.Add((JsonSerializer.Serialize(new { offerDetails.OfferId, offerDetails.OfferName }), NotificationTypeId.TECHNICAL_USER_CREATION));
        }

        var userIdsOfNotifications = await notificationService.CreateNotifications(
            itAdminRoles,
            userId,
            notifications,
            offerDetails.CompanyId).ToListAsync().ConfigureAwait(false);
        if (!userIdsOfNotifications.Contains(offerDetails.RequesterId))
        {
            portalRepositories.GetInstance<INotificationRepository>().CreateNotification(offerDetails.RequesterId, appSubscriptionActivation, false, notification =>
            {
                notification.Content = notificationContent;
                notification.CreatorUserId = userId;
            });
        }
    }

    private async Task SetNotificationsToDone(
        IEnumerable<UserRoleConfig> serviceManagerRoles,
        OfferTypeId offerTypeId,
        Guid offerId,
        Guid? salesManagerId)
    {
        var notificationType = offerTypeId == OfferTypeId.APP
            ? NotificationTypeId.APP_SUBSCRIPTION_REQUEST
            : NotificationTypeId.SERVICE_REQUEST;
        await notificationService.SetNotificationsForOfferToDone(
            serviceManagerRoles,
            Enumerable.Repeat(notificationType, 1),
            offerId,
            salesManagerId == null ? [] : Enumerable.Repeat(salesManagerId.Value, 1))
            .ConfigureAwait(ConfigureAwaitOptions.None);
    }

    private void SendMail(string basePortalAddress, string userName, string requesterEmail, string? offerName, OfferTypeId offerType)
    {
        var mailParams = ImmutableDictionary.CreateRange([
            KeyValuePair.Create("offerCustomerName", !string.IsNullOrWhiteSpace(userName) ? userName : "User"),
            KeyValuePair.Create("offerName", offerName ?? "unnamed Offer"),
            KeyValuePair.Create("url", basePortalAddress)
        ]);
        mailingProcessCreation.CreateMailProcess(requesterEmail, $"{offerType.ToString().ToLower()}-subscription-activation", mailParams);
    }

    /// <inheritdoc />
    public async Task StartAutoSetupAsync(OfferAutoSetupData data, OfferTypeId offerTypeId)
    {
        var companyId = _identityData.CompanyId;
        logger.LogDebug("AutoSetup Process started from Company {CompanyId} for {RequestId} with OfferUrl: {OfferUrl}", companyId, data.RequestId, data.OfferUrl.Replace(Environment.NewLine, string.Empty));
        if (data.OfferUrl.Contains('#', StringComparison.OrdinalIgnoreCase))
        {
            throw ControllerArgumentException.Create(OfferSetupServiceErrors.OFFERURL_NOT_CONTAIN, [new("offerUrl", data.OfferUrl)]);
        }

        var offerSubscriptionRepository = portalRepositories.GetInstance<IOfferSubscriptionsRepository>();
        var details = await GetAndValidateOfferDetails(data.RequestId, companyId, offerTypeId, offerSubscriptionRepository).ConfigureAwait(ConfigureAwaitOptions.None);
        if (details.InstanceData.IsSingleInstance)
        {
            throw ConflictException.Create(OfferSetupServiceErrors.STEP_NOT_ELIGIBLE_FOR_SINGLE_INSTANCE);
        }

        var context = await offerSubscriptionProcessService.VerifySubscriptionAndProcessSteps(data.RequestId,
            ProcessStepTypeId.AWAIT_START_AUTOSETUP, null, true).ConfigureAwait(ConfigureAwaitOptions.None);

        offerSubscriptionRepository.CreateOfferSubscriptionProcessData(data.RequestId, data.OfferUrl);

        var nextProcessStepTypeIds = new[]
        {
            offerTypeId == OfferTypeId.APP ?
                ProcessStepTypeId.OFFERSUBSCRIPTION_CLIENT_CREATION :
                ProcessStepTypeId.OFFERSUBSCRIPTION_TECHNICALUSER_CREATION
        };

        offerSubscriptionProcessService.FinalizeProcessSteps(context, nextProcessStepTypeIds);
        await portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }

    /// <inheritdoc />
    public async Task CreateSingleInstanceSubscriptionDetail(Guid offerSubscriptionId)
    {
        var offerSubscriptionRepository = portalRepositories.GetInstance<IOfferSubscriptionsRepository>();
        var offerDetails = await offerSubscriptionRepository.GetSubscriptionActivationDataByIdAsync(offerSubscriptionId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (offerDetails == null)
        {
            throw NotFoundException.Create(OfferSetupServiceErrors.OFFERSUBSCRIPTION_NOT_EXIST, [new("offerSubscriptionId", offerSubscriptionId.ToString())]);
        }

        switch (offerDetails.InstanceData.IsSingleInstance)
        {
            case false:
                throw ConflictException.Create(OfferSetupServiceErrors.PROCESS_STEP_ONLY_FOR_SINGLE_INSTANCE);
            case true when offerDetails.AppInstanceIds.Count() != 1:
                throw ConflictException.Create(OfferSetupServiceErrors.ONLY_ONE_APP_INSTANCE_FOR_SINGLE_INSTANCE);
            default:
                if (offerDetails.ProviderCompanyId != _identityData.CompanyId)
                {
                    throw ConflictException.Create(OfferSetupServiceErrors.SUBSCRIPTION_ONLY_ACTIVATED_BY_PROVIDER);
                }

                portalRepositories.GetInstance<IAppSubscriptionDetailRepository>()
                    .CreateAppSubscriptionDetail(offerSubscriptionId, appSubscriptionDetail =>
                    {
                        appSubscriptionDetail.AppInstanceId = offerDetails.AppInstanceIds.Single();
                        appSubscriptionDetail.AppSubscriptionUrl = offerDetails.InstanceData.InstanceUrl;
                    });

                var context = await offerSubscriptionProcessService.VerifySubscriptionAndProcessSteps(offerSubscriptionId,
                    ProcessStepTypeId.SINGLE_INSTANCE_SUBSCRIPTION_DETAILS_CREATION, null, true).ConfigureAwait(ConfigureAwaitOptions.None);

                offerSubscriptionProcessService.FinalizeProcessSteps(context,
                [
                    ProcessStepTypeId.ACTIVATE_SUBSCRIPTION
                ]);
                await portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
                break;
        }
    }

    /// <inheritdoc />
    public async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> CreateClient(Guid offerSubscriptionId)
    {
        var clientCreationData = await portalRepositories.GetInstance<IOfferSubscriptionsRepository>().GetClientCreationData(offerSubscriptionId).ConfigureAwait(ConfigureAwaitOptions.None);
        var userRolesRepository = portalRepositories.GetInstance<IUserRolesRepository>();
        if (clientCreationData == null)
        {
            throw NotFoundException.Create(OfferSetupServiceErrors.OFFERSUBSCRIPTION_NOT_EXIST, [new("offerSubscriptionId", offerSubscriptionId.ToString())]);
        }

        if (string.IsNullOrWhiteSpace(clientCreationData.OfferUrl))
        {
            throw ConflictException.Create(OfferSetupServiceErrors.OFFERURL_SHOULD_BE_SET);
        }

        if (clientCreationData.OfferType != OfferTypeId.APP)
        {
            throw ConflictException.Create(OfferSetupServiceErrors.OFFERS_WITHOUT_TYPE_NOT_ELIGIBLE, [new("offerTypeId", OfferTypeId.APP.ToString())]);
        }

        var (_, iamClientId) = await CreateClient(clientCreationData.OfferUrl, clientCreationData.OfferId, false, userRolesRepository);
        CreateAppInstance(offerSubscriptionId, clientCreationData.OfferUrl, clientCreationData.OfferId, iamClientId);

        return new ValueTuple<IEnumerable<ProcessStepTypeId>?, ProcessStepStatusId, bool, string?>(
            [
                clientCreationData.IsTechnicalUserNeeded
                    ? ProcessStepTypeId.OFFERSUBSCRIPTION_TECHNICALUSER_CREATION
                    : ProcessStepTypeId.MANUAL_TRIGGER_ACTIVATE_SUBSCRIPTION
            ],
            ProcessStepStatusId.DONE,
            true,
            null);
    }

    /// <inheritdoc />
    public async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> CreateTechnicalUser(Guid processId, Guid offerSubscriptionId, IEnumerable<UserRoleConfig> itAdminRoles)
    {
        var data = await portalRepositories.GetInstance<IOfferSubscriptionsRepository>()
            .GetTechnicalUserCreationData(offerSubscriptionId)
            .ConfigureAwait(ConfigureAwaitOptions.None);
        if (data == null)
        {
            throw NotFoundException.Create(OfferSetupServiceErrors.OFFERSUBSCRIPTION_NOT_EXIST, [new("offerSubscriptionId", offerSubscriptionId.ToString())]);
        }

        if (!data.IsTechnicalUserNeeded)
        {
            throw ConflictException.Create(OfferSetupServiceErrors.TECHNICAL_USER_NOT_NEEDED);
        }

        var technicalUserClientId = data.ClientId ?? $"{data.OfferName}-{data.CompanyName}";
        var createTechnicalUserData = new CreateTechnicalUserData(data.CompanyId, data.OfferName, data.Bpn, technicalUserClientId, true, false);
        var technicalUsers = await CreateTechnicalUserForSubscription(offerSubscriptionId, createTechnicalUserData, processId).ConfigureAwait(ConfigureAwaitOptions.None);
        var technicalClientIds = technicalUsers.ServiceAccounts.Select(sa => sa.ClientId);

        var content = JsonSerializer.Serialize(new
        {
            technicalClientIds
        });

        await notificationService.CreateNotifications(
            itAdminRoles,
            null,
            [
                (content, NotificationTypeId.TECHNICAL_USER_CREATION)
            ],
            data.CompanyId).AwaitAll().ConfigureAwait(false);

        return new ValueTuple<IEnumerable<ProcessStepTypeId>?, ProcessStepStatusId, bool, string?>(
            [
                technicalUsers.HasExternalServiceAccount
                    ? ProcessStepTypeId.OFFERSUBSCRIPTION_CREATE_DIM_TECHNICAL_USER
                    : ProcessStepTypeId.MANUAL_TRIGGER_ACTIVATE_SUBSCRIPTION
            ],
            ProcessStepStatusId.DONE,
            true,
            null);
    }

    public async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> CreateDimTechnicalUser(Guid offerSubscriptionId, CancellationToken cancellationToken)
    {
        var (bpn, offerName, processId) = await portalRepositories.GetInstance<IOfferSubscriptionsRepository>().GetDimTechnicalUserDataForSubscriptionId(offerSubscriptionId)
            .ConfigureAwait(ConfigureAwaitOptions.None);

        if (bpn is null)
        {
            throw ConflictException.Create(OfferSetupServiceErrors.BPN_MUST_BE_SET);
        }

        if (offerName is null)
        {
            throw ConflictException.Create(OfferSetupServiceErrors.OFFER_NAME_MUST_BE_SET, [new("offerSubscriptionId", offerSubscriptionId.ToString())]);
        }

        if (processId is null)
        {
            throw UnexpectedConditionException.Create(OfferSetupServiceErrors.OFFERSUBSCRIPTION_MUST_BE_LINKED_TO_PROCESS, [new("offerSubscriptionId", offerSubscriptionId.ToString())]);
        }

        await dimService.CreateTechnicalUser(bpn, new TechnicalUserData(processId.Value, $"sa-{offerName}-{offerSubscriptionId}"), cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        return new ValueTuple<IEnumerable<ProcessStepTypeId>?, ProcessStepStatusId, bool, string?>(
            [
                ProcessStepTypeId.AWAIT_CREATE_DIM_TECHNICAL_USER_RESPONSE
            ],
            ProcessStepStatusId.DONE,
            true,
            null);
    }

    /// <inheritdoc />
    public async Task TriggerActivateSubscription(Guid offerSubscriptionId)
    {
        var context = await offerSubscriptionProcessService.VerifySubscriptionAndProcessSteps(offerSubscriptionId, ProcessStepTypeId.MANUAL_TRIGGER_ACTIVATE_SUBSCRIPTION, null, true).ConfigureAwait(ConfigureAwaitOptions.None);
        if (!await portalRepositories.GetInstance<IOfferSubscriptionsRepository>()
            .CheckOfferSubscriptionForProvider(offerSubscriptionId, _identityData.CompanyId).ConfigureAwait(ConfigureAwaitOptions.None))
        {
            throw ConflictException.Create(OfferSetupServiceErrors.COMPANY_MUST_BE_PROVIDER, [new("companyId", _identityData.CompanyId.ToString()), new("offerSubscriptionId", offerSubscriptionId.ToString())]);
        }

        offerSubscriptionProcessService.FinalizeProcessSteps(context, Enumerable.Repeat(ProcessStepTypeId.ACTIVATE_SUBSCRIPTION, 1));
        await portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }

    /// <inheritdoc />
    public async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> ActivateSubscription(Guid offerSubscriptionId, IEnumerable<UserRoleConfig> itAdminRoles, IEnumerable<UserRoleConfig> serviceManagerRoles, string basePortalAddress)
    {
        var offerSubscriptionRepository = portalRepositories.GetInstance<IOfferSubscriptionsRepository>();
        var offerDetails = await offerSubscriptionRepository.GetSubscriptionActivationDataByIdAsync(offerSubscriptionId)
            .ConfigureAwait(ConfigureAwaitOptions.None);
        if (offerDetails == null)
        {
            throw NotFoundException.Create(OfferSetupServiceErrors.OFFERSUBSCRIPTION_NOT_EXIST, [new("offerSubscriptionId", offerSubscriptionId.ToString())]);
        }

        if (offerDetails.InstanceData.IsSingleInstance)
        {
            if (offerDetails.AppInstanceIds.Count() != 1)
                throw ConflictException.Create(OfferSetupServiceErrors.ONLY_ONE_APP_INSTANCE_FOR_SINGLE_INSTANCE);

            await SetNotificationsToDone(serviceManagerRoles, offerDetails.OfferTypeId, offerDetails.OfferId, offerDetails.SalesManagerId).ConfigureAwait(ConfigureAwaitOptions.None);
        }

        await EnableClientAndServiceAccount(offerSubscriptionId, offerDetails).ConfigureAwait(ConfigureAwaitOptions.None);

        offerSubscriptionRepository.AttachAndModifyOfferSubscription(offerSubscriptionId, subscription => { subscription.OfferSubscriptionStatusId = OfferSubscriptionStatusId.ACTIVE; });

        if (offerDetails.OfferSubscriptionProcessDataId.HasValue)
        {
            offerSubscriptionRepository.RemoveOfferSubscriptionProcessData(offerDetails.OfferSubscriptionProcessDataId.Value);
        }

        var notificationContent = JsonSerializer.Serialize(new
        {
            offerDetails.OfferId,
            offerDetails.CompanyName,
            offerDetails.OfferName,
            OfferSubscriptionId = offerSubscriptionId
        });
        var notificationTypeId = offerDetails.OfferTypeId == OfferTypeId.APP
            ? NotificationTypeId.APP_SUBSCRIPTION_ACTIVATION
            : NotificationTypeId.SERVICE_ACTIVATION;
        var userIdsOfNotifications = await notificationService.CreateNotificationsWithExistenceCheck(
                itAdminRoles,
                null,
                [
                    (notificationContent, notificationTypeId)
                ],
                offerDetails.CompanyId,
                nameof(offerSubscriptionId),
                offerSubscriptionId.ToString())
            .ToListAsync().ConfigureAwait(false);

        var notificationRepository = portalRepositories.GetInstance<INotificationRepository>();
        if (!userIdsOfNotifications.Contains(offerDetails.RequesterId) &&
            !await notificationRepository
                .CheckNotificationExistsForParam(offerDetails.RequesterId, notificationTypeId, nameof(offerSubscriptionId),
                    offerSubscriptionId.ToString())
                .ConfigureAwait(ConfigureAwaitOptions.None))
        {
            notificationRepository.CreateNotification(offerDetails.RequesterId,
                notificationTypeId, false, notification => { notification.Content = notificationContent; });
        }

        if (string.IsNullOrWhiteSpace(offerDetails.RequesterEmail))
        {
            return new ValueTuple<IEnumerable<ProcessStepTypeId>?, ProcessStepStatusId, bool, string?>(
                offerDetails.InstanceData.IsSingleInstance || !offerDetails.HasCallbackUrl ? null : [ProcessStepTypeId.TRIGGER_PROVIDER_CALLBACK],
                ProcessStepStatusId.DONE,
                true,
                null);
        }

        SendMail(basePortalAddress, $"{offerDetails.RequesterFirstname} {offerDetails.RequesterLastname}", offerDetails.RequesterEmail, offerDetails.OfferName, offerDetails.OfferTypeId);

        return new ValueTuple<IEnumerable<ProcessStepTypeId>?, ProcessStepStatusId, bool, string?>(
            offerDetails.InstanceData.IsSingleInstance || !offerDetails.HasCallbackUrl ? null : [ProcessStepTypeId.TRIGGER_PROVIDER_CALLBACK],
            ProcessStepStatusId.DONE,
            true,
            null);
    }

    private async Task EnableClientAndServiceAccount(Guid offerSubscriptionId, SubscriptionActivationData offerDetails)
    {
        if (offerDetails is { OfferTypeId: OfferTypeId.APP, InstanceData.IsSingleInstance: false })
        {
            if (string.IsNullOrEmpty(offerDetails.ClientClientId))
                throw ConflictException.Create(OfferSetupServiceErrors.CLIENTID_EMPTY_FOR_OFFERSUBSCRIPTION, [new("offerSubscriptionId", offerSubscriptionId.ToString())]);
            try
            {
                await provisioningManager.EnableClient(offerDetails.ClientClientId!).ConfigureAwait(ConfigureAwaitOptions.None);
            }
            catch (Exception e)
            {
                throw new ServiceException(e.Message, true);
            }
        }

        try
        {
            foreach (var serviceAccountClientId in offerDetails.InternalServiceAccountClientIds)
            {
                await provisioningManager.EnableClient(serviceAccountClientId).ConfigureAwait(ConfigureAwaitOptions.None);
            }
        }
        catch (Exception e)
        {
            throw new ServiceException(e.Message, true);
        }
    }

    internal record CreateTechnicalUserData(Guid CompanyId, string? OfferName, string? Bpn, string TechnicalUserName, bool EnhanceTechnicalUserName, bool Enabled);
}
