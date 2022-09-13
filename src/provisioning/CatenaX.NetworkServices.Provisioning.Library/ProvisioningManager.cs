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

using CatenaX.NetworkServices.Keycloak.ErrorHandling;
using CatenaX.NetworkServices.Keycloak.Factory;
using CatenaX.NetworkServices.Provisioning.DBAccess;
using CatenaX.NetworkServices.Provisioning.Library.Enums;
using CatenaX.NetworkServices.Provisioning.Library.Models;
using Keycloak.Net;
using Keycloak.Net.Models.ProtocolMappers;
using Microsoft.Extensions.Options;

namespace CatenaX.NetworkServices.Provisioning.Library;

public partial class ProvisioningManager : IProvisioningManager
{
    private readonly KeycloakClient _CentralIdp;
    private readonly IKeycloakFactory _Factory;
    private readonly IProvisioningDBAccess? _ProvisioningDBAccess;
    private readonly ProvisioningSettings _Settings;

    public ProvisioningManager(IKeycloakFactory keycloakFactory, IProvisioningDBAccess? provisioningDBAccess, IOptions<ProvisioningSettings> options)
    {
        _CentralIdp = keycloakFactory.CreateKeycloakClient("central");
        _Factory = keycloakFactory;
        _Settings = options.Value;
        _ProvisioningDBAccess = provisioningDBAccess;
    }

    public ProvisioningManager(IKeycloakFactory keycloakFactory, IOptions<ProvisioningSettings> options)
        : this(keycloakFactory, null, options)
    {
    }

    public async Task SetupSharedIdpAsync(string idpName, string organisationName)
    {
        await CreateCentralIdentityProviderAsync(idpName, organisationName, _Settings.CentralIdentityProvider).ConfigureAwait(false);

        var (clientId, secret) = await CreateSharedIdpServiceAccountAsync(idpName).ConfigureAwait(false);
        var sharedKeycloak = _Factory.CreateKeycloakClient("shared", clientId, secret);

        await CreateSharedRealmAsync(sharedKeycloak, idpName, organisationName).ConfigureAwait(false);

        await UpdateCentralIdentityProviderUrlsAsync(idpName, await sharedKeycloak.GetOpenIDConfigurationAsync(idpName).ConfigureAwait(false)).ConfigureAwait(false);

        await CreateCentralIdentityProviderTenantMapperAsync(idpName).ConfigureAwait(false);

        await CreateCentralIdentityProviderOrganisationMapperAsync(idpName, organisationName).ConfigureAwait(false);

        await CreateCentralIdentityProviderUsernameMapperAsync(idpName).ConfigureAwait(false);

        await CreateSharedRealmIdentityProviderClientAsync(sharedKeycloak, idpName, new IdentityProviderClientConfig(
            await GetCentralBrokerEndpointOIDCAsync(idpName).ConfigureAwait(false)+"/*",
            await GetCentralRealmJwksUrlAsync().ConfigureAwait(false)
        )).ConfigureAwait(false);

        await EnableCentralIdentityProviderAsync(idpName).ConfigureAwait(false);
    }

    public async ValueTask DeleteSharedIdpRealmAsync(string alias)
    {
        var sharedKeycloak = _Factory.CreateKeycloakClient("shared");
        if (! await sharedKeycloak.DeleteRealmAsync(alias).ConfigureAwait(false))
        {
            throw new KeycloakNoSuccessException($"failed to delete shared realm {alias}");
        }
        await DeleteSharedIdpServiceAccountAsync(sharedKeycloak, alias);
    }

    public async Task<string> CreateOwnIdpAsync(string organisationName, IamIdentityProviderProtocol providerProtocol)
    {
        var idpName = await GetNextCentralIdentityProviderNameAsync().ConfigureAwait(false);

        await CreateCentralIdentityProviderAsync(idpName, organisationName, GetIdentityProviderTemplate(providerProtocol)).ConfigureAwait(false);

        return idpName;
    }

    public async Task<string> CreateSharedUserLinkedToCentralAsync(string idpName, UserProfile userProfile)
    {
        var sharedKeycloak = await GetSharedKeycloakClient(idpName).ConfigureAwait(false);
        var userIdShared = await CreateSharedRealmUserAsync(sharedKeycloak, idpName, userProfile).ConfigureAwait(false);

        if (userIdShared == null)
        {
            throw new KeycloakNoSuccessException($"failed to created user {userProfile.UserName} in shared realm {idpName}");
        }

        var userIdCentral = await CreateCentralUserAsync(idpName, new UserProfile(
            idpName + "." + userIdShared,
            userProfile.Email,
            userProfile.OrganisationName) {
                FirstName = userProfile.FirstName,
                LastName = userProfile.LastName,
                BusinessPartnerNumber = userProfile.BusinessPartnerNumber
            }).ConfigureAwait(false);

        if (userIdCentral == null)
        {
            throw new KeycloakNoSuccessException($"failed to created user {userProfile.UserName} in central realm for organization {userProfile.OrganisationName}");
        }

        await LinkCentralSharedRealmUserAsync(idpName, userIdCentral, userIdShared, userProfile.UserName).ConfigureAwait(false);

        return userIdCentral;
    }

    public async Task<string> SetupClientAsync(string redirectUrl, IEnumerable<string>? optionalRoleNames)
    {
        var clientId = await GetNextClientIdAsync().ConfigureAwait(false);
        var internalId = await CreateCentralOIDCClientAsync(clientId, redirectUrl).ConfigureAwait(false);
        await CreateCentralOIDCClientAudienceMapperAsync(internalId, clientId).ConfigureAwait(false);
        if (optionalRoleNames != null && optionalRoleNames.Any())
        {
            await this.AssignClientRolesToClient(internalId, optionalRoleNames).ConfigureAwait(false);
        }

        return clientId;
    }

    public async Task AddBpnAttributetoUserAsync(string userId, IEnumerable<string> bpns)
    {
        var user = await _CentralIdp.GetUserAsync(_Settings.CentralRealm, userId).ConfigureAwait(false);
        user.Attributes ??= new Dictionary<string, IEnumerable<string>>();
        user.Attributes[_Settings.MappedBpnAttribute] = (user.Attributes.TryGetValue(_Settings.MappedBpnAttribute, out var existingBpns))
            ? existingBpns.Concat(bpns).Distinct()
            : bpns;
        if (!await _CentralIdp.UpdateUserAsync(_Settings.CentralRealm, userId.ToString(), user).ConfigureAwait(false))
        {
            throw new KeycloakNoSuccessException($"failed to set bpns {bpns} for central user {userId}");
        }
    }

    public Task AddProtocolMapperAsync(string clientScope)
    {
        var mapper = Clone(_Settings.ClientProtocolMapper);
        return _CentralIdp.CreateClientProtocolMapperAsync(_Settings.CentralRealm, clientScope, mapper);
    }

    public async Task DeleteCentralUserBusinessPartnerNumberAsync(string userId, string businessPartnerNumber)
    {
        var user = await _CentralIdp.GetUserAsync(_Settings.CentralRealm, userId).ConfigureAwait(false);
        
        if (user.Attributes == null || !user.Attributes.TryGetValue(_Settings.MappedBpnAttribute, out var existingBpns))
        {
            throw new KeycloakEntityNotFoundException($"attribute {_Settings.MappedBpnAttribute} not found in the mappers of user {userId}");
        }

        user.Attributes[_Settings.MappedBpnAttribute] = existingBpns.Where(bpn => bpn != businessPartnerNumber);

        if (!await _CentralIdp.UpdateUserAsync(_Settings.CentralRealm, userId, user).ConfigureAwait(false))
        {
            throw new KeycloakNoSuccessException($"failed to delete bpn for central user {userId}");
        }
    }

    public async Task<bool> ResetSharedUserPasswordAsync(string realm, string userId)
    {
        var providerUserId = await GetProviderUserIdForCentralUserIdAsync(realm, userId).ConfigureAwait(false);
        if (providerUserId == null)
        {
            throw new KeycloakEntityNotFoundException($"userId {userId} is not linked to shared realm {realm}");
        }
        var sharedKeycloak = await GetSharedKeycloakClient(realm).ConfigureAwait(false);
        return await sharedKeycloak.SendUserUpdateAccountEmailAsync(realm, providerUserId, Enumerable.Repeat("UPDATE_PASSWORD", 1)).ConfigureAwait(false);
    }

    public async Task<IEnumerable<string>> GetClientRoleMappingsForUserAsync(string userId, string clientId)
    {
        var idOfClient = await GetCentralInternalClientIdFromClientIDAsync(clientId).ConfigureAwait(false);
        return (await _CentralIdp.GetClientRoleMappingsForUserAsync(_Settings.CentralRealm, userId, idOfClient).ConfigureAwait(false))
            .Where(r => r.Composite == true).Select(x => x.Name);
    }

    public async ValueTask<bool> IsCentralIdentityProviderEnabled(string alias)
    {
        return (await GetCentralIdentityProviderAsync(alias).ConfigureAwait(false)).Enabled ?? false;
    }

    public async ValueTask<IdentityProviderConfigOidc> GetCentralIdentityProviderDataOIDCAsync(string alias)
    {
        var identityProvider = await GetCentralIdentityProviderAsync(alias).ConfigureAwait(false);
        var redirectUri = await GetCentralBrokerEndpointOIDCAsync(alias).ConfigureAwait(false);
        return new IdentityProviderConfigOidc(
            identityProvider.DisplayName,
            redirectUri,
            identityProvider.Config.ClientId,
            identityProvider.Enabled ?? false,
            identityProvider.Config.AuthorizationUrl,
            IdentityProviderClientAuthTypeToIamClientAuthMethod(identityProvider.Config.ClientAuthMethod),
            identityProvider.Config.ClientAssertionSigningAlg == null ? null : Enum.Parse<IamIdentityProviderSignatureAlgorithm>(identityProvider.Config.ClientAssertionSigningAlg));
    }

    public async ValueTask UpdateSharedIdentityProviderAsync(string alias, string displayName)
    {
        var identityProvider = await GetCentralIdentityProviderAsync(alias).ConfigureAwait(false);
        identityProvider.DisplayName = displayName;
        var sharedKeycloak = await GetSharedKeycloakClient(alias).ConfigureAwait(false);
        await UpdateSharedRealmAsync(sharedKeycloak, alias, displayName).ConfigureAwait(false);
        await UpdateCentralIdentityProviderAsync(alias, identityProvider).ConfigureAwait(false);
    }

    public async ValueTask SetSharedIdentityProviderStatusAsync(string alias, bool enabled)
    {
        var identityProvider = await GetCentralIdentityProviderAsync(alias).ConfigureAwait(false);
        identityProvider.Enabled = enabled;
        identityProvider.Config.HideOnLoginPage = enabled ? "false" : "true";
        var sharedKeycloak = await GetSharedKeycloakClient(alias).ConfigureAwait(false);
        await SetSharedRealmStatusAsync(sharedKeycloak, alias, enabled).ConfigureAwait(false);
        await UpdateCentralIdentityProviderAsync(alias, identityProvider).ConfigureAwait(false);
    }

    public async ValueTask SetCentralIdentityProviderStatusAsync(string alias, bool enabled)
    {
        var identityProvider = await GetCentralIdentityProviderAsync(alias).ConfigureAwait(false);
        identityProvider.Enabled = enabled;
        identityProvider.Config.HideOnLoginPage = enabled ? "false" : "true";
        await UpdateCentralIdentityProviderAsync(alias, identityProvider).ConfigureAwait(false);
    }        

    public ValueTask UpdateCentralIdentityProviderDataOIDCAsync(IdentityProviderEditableConfigOidc identityProviderConfigOidc)
    {
        if(identityProviderConfigOidc.Secret == null)
        {
            switch(identityProviderConfigOidc.ClientAuthMethod)
            {
                case IamIdentityProviderClientAuthMethod.SECRET_BASIC:
                case IamIdentityProviderClientAuthMethod.SECRET_POST:
                case IamIdentityProviderClientAuthMethod.SECRET_JWT:
                    throw new ArgumentException($"secret must not be null for clientAuthMethod {identityProviderConfigOidc.ClientAuthMethod.ToString()}");
                default:
                    break;
            }
        }
        if(!identityProviderConfigOidc.SignatureAlgorithm.HasValue)
        {
            switch(identityProviderConfigOidc.ClientAuthMethod)
            {
                case IamIdentityProviderClientAuthMethod.SECRET_JWT:
                case IamIdentityProviderClientAuthMethod.JWT:
                    throw new ArgumentException($"signatureAlgorithm must not be null for clientAuthMethod {identityProviderConfigOidc.ClientAuthMethod.ToString()}");
                default:
                    break;
            }
        }
        return UpdateCentralIdentityProviderDataOIDCInternalAsync(identityProviderConfigOidc);
    }

    private async ValueTask UpdateCentralIdentityProviderDataOIDCInternalAsync(IdentityProviderEditableConfigOidc identityProviderConfigOidc)
    {
        var (alias, displayName, metadataUrl, clientAuthMethod, clientId, secret, signatureAlgorithm) = identityProviderConfigOidc;
        var identityProvider = await SetIdentityProviderMetadataFromUrlAsync(await GetCentralIdentityProviderAsync(alias).ConfigureAwait(false), metadataUrl).ConfigureAwait(false);
        identityProvider.DisplayName = displayName;
        identityProvider.Config.ClientAuthMethod = IamIdentityProviderClientAuthMethodToInternal(clientAuthMethod);
        identityProvider.Config.ClientId = clientId;
        identityProvider.Config.ClientSecret = secret;
        identityProvider.Config.ClientAssertionSigningAlg = signatureAlgorithm?.ToString();
        await UpdateCentralIdentityProviderAsync(alias, identityProvider).ConfigureAwait(false);
    }

    public async ValueTask<IdentityProviderConfigSaml> GetCentralIdentityProviderDataSAMLAsync(string alias)
    {
        var identityProvider = await GetCentralIdentityProviderAsync(alias).ConfigureAwait(false);
        var redirectUri = await GetCentralBrokerEndpointSAMLAsync(alias).ConfigureAwait(false);
        return new IdentityProviderConfigSaml(
            identityProvider.DisplayName,
            redirectUri,
            identityProvider.Config.ClientId,
            identityProvider.Enabled ?? false,
            identityProvider.Config.EntityId,
            identityProvider.Config.SingleSignOnServiceUrl);
    }

    public async ValueTask UpdateCentralIdentityProviderDataSAMLAsync(IdentityProviderEditableConfigSaml identityProviderEditableConfigSaml)
    {
        var (alias, displayName, entityId, singleSignOnServiceUrl) = identityProviderEditableConfigSaml;
        var identityProvider = await GetCentralIdentityProviderAsync(alias).ConfigureAwait(false);
        identityProvider.DisplayName = displayName;
        identityProvider.Config.EntityId = entityId;
        identityProvider.Config.SingleSignOnServiceUrl = singleSignOnServiceUrl;
        await UpdateCentralIdentityProviderAsync(alias, identityProvider).ConfigureAwait(false);
    }

    private async Task<KeycloakClient> GetSharedKeycloakClient(string realm)
    {
        var (clientId, secret) = await GetSharedIdpServiceAccountSecretAsync(realm).ConfigureAwait(false);
        return _Factory.CreateKeycloakClient("shared", clientId, secret);
    }
}
