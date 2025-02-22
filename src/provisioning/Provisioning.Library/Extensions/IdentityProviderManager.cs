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

using Flurl;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.IdentityProviders;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;
using System.Collections.Immutable;
using System.Net;
using System.Text.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;

public partial class ProvisioningManager
{
    private static readonly ImmutableDictionary<string, IamIdentityProviderClientAuthMethod> IdentityProviderClientAuthTypesIamClientAuthMethodDictionary = new Dictionary<string, IamIdentityProviderClientAuthMethod>()
    {
        { "private_key_jwt", IamIdentityProviderClientAuthMethod.JWT },
        { "client_secret_post", IamIdentityProviderClientAuthMethod.SECRET_POST },
        { "client_secret_basic", IamIdentityProviderClientAuthMethod.SECRET_BASIC },
        { "client_secret_jwt", IamIdentityProviderClientAuthMethod.SECRET_JWT }
    }.ToImmutableDictionary();

    private static readonly ImmutableDictionary<IamIdentityProviderClientAuthMethod, string> IamIdentityProviderClientAuthMethodsInternalDictionary = new Dictionary<IamIdentityProviderClientAuthMethod, string>()
    {
        { IamIdentityProviderClientAuthMethod.JWT, "private_key_jwt" },
        { IamIdentityProviderClientAuthMethod.SECRET_POST, "client_secret_post" },
        { IamIdentityProviderClientAuthMethod.SECRET_BASIC, "client_secret_basic" },
        { IamIdentityProviderClientAuthMethod.SECRET_JWT, "client_secret_jwt" }
    }.ToImmutableDictionary();

    private static readonly ImmutableDictionary<string, IdentityProviderMapperType> IdentityProviderKeycloakMapperTypesToEnumDictionary = new Dictionary<string, IdentityProviderMapperType>()
    {
        { "hardcoded-user-session-attribute-idp-mapper", IdentityProviderMapperType.HARDCODED_SESSION_ATTRIBUTE },
        { "hardcoded-attribute-idp-mapper", IdentityProviderMapperType.HARDCODED_ATTRIBUTE },
        { "oidc-advanced-group-idp-mapper", IdentityProviderMapperType.OIDC_ADVANCED_GROUP },
        { "oidc-user-attribute-idp-mapper", IdentityProviderMapperType.OIDC_USER_ATTRIBUTE },
        { "oidc-advanced-role-idp-mapper", IdentityProviderMapperType.OIDC_ADVANCED_ROLE },
        { "oidc-hardcoded-role-idp-mapper", IdentityProviderMapperType.OIDC_HARDCODED_ROLE },
        { "oidc-role-idp-mapper", IdentityProviderMapperType.OIDC_ROLE },
        { "oidc-username-idp-mapper", IdentityProviderMapperType.OIDC_USERNAME },
        { "keycloak-oidc-role-to-role-idp-mapper", IdentityProviderMapperType.KEYCLOAK_OIDC_ROLE }
    }.ToImmutableDictionary();

    public async ValueTask<string> GetNextCentralIdentityProviderNameAsync() =>
        $"{_settings.IdpPrefix}{await _provisioningDBAccess!.GetNextIdentityProviderSequenceAsync().ConfigureAwait(ConfigureAwaitOptions.None)}";

    public Task CreateCentralIdentityProviderAsync(string alias, string displayName)
        => CreateCentralIdentityProviderAsyncInternal(alias, displayName, _settings.CentralIdentityProvider);

    private Task CreateCentralIdentityProviderAsyncInternal(string alias, string displayName, IdentityProvider identityProvider)
    {
        var newIdp = CloneIdentityProvider(identityProvider);
        newIdp.Alias = alias;
        newIdp.DisplayName = displayName;
        return _centralIdp.CreateIdentityProviderAsync(_settings.CentralRealm, newIdp);
    }

    private async ValueTask<IdentityProvider> SetIdentityProviderMetadataFromUrlAsync(IdentityProvider identityProvider, string url, CancellationToken cancellationToken)
    {
        var metadata = await _centralIdp.ImportIdentityProviderFromUrlAsync(_settings.CentralRealm, url, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        if (!metadata.Any())
        {
            throw new ServiceException("failed to import identityprovider metadata", HttpStatusCode.NotFound);
        }
        var changed = CloneIdentityProvider(identityProvider);
        changed.Config ??= new Config();
        foreach (var (key, value) in metadata)
        {
            switch (key)
            {
                case "userInfoUrl":
                    changed.Config.UserInfoUrl = value.ToString();
                    break;
                case "validateSignature":
                    changed.Config.ValidateSignature = value.ToString();
                    break;
                case "tokenUrl":
                    changed.Config.TokenUrl = value.ToString();
                    break;
                case "authorizationUrl":
                    changed.Config.AuthorizationUrl = value.ToString();
                    break;
                case "jwksUrl":
                    changed.Config.JwksUrl = value.ToString();
                    break;
                case "logoutUrl":
                    changed.Config.LogoutUrl = value.ToString();
                    break;
                case "issuer":
                    changed.Config.Issuer = value.ToString();
                    break;
                case "useJwksUrl":
                    changed.Config.UseJwksUrl = value.ToString();
                    break;
            }
        }

        return changed;
    }

    private Task<IdentityProvider> GetCentralIdentityProviderAsync(string alias)
    {
        return _centralIdp.GetIdentityProviderAsync(_settings.CentralRealm, alias);
    }

    private Task UpdateCentralIdentityProviderAsync(string alias, IdentityProvider identityProvider) =>
        _centralIdp.UpdateIdentityProviderAsync(_settings.CentralRealm, alias, identityProvider);

    public Task DeleteCentralIdentityProviderAsync(string alias) =>
        _centralIdp.DeleteIdentityProviderAsync(_settings.CentralRealm, alias);

    public async IAsyncEnumerable<IdentityProviderMapperModel> GetIdentityProviderMappers(string alias)
    {
        foreach (var mapper in await _centralIdp.GetIdentityProviderMappersAsync(_settings.CentralRealm, alias).ConfigureAwait(ConfigureAwaitOptions.None))
        {
            yield return new IdentityProviderMapperModel(
                mapper.Id ?? throw new KeycloakInvalidResponseException("mapper.Id is null"),
                mapper.Name ?? throw new KeycloakInvalidResponseException("mapper.Name is null"),
                KeycloakIdentityProviderMapperTypeToEnum(mapper._IdentityProviderMapper ?? throw new KeycloakInvalidResponseException("mapper._IdentityProviderMapper is null")),
                mapper.Config ?? throw new KeycloakInvalidResponseException("mapper.Config is null")
            );
        }
    }

    public async Task<string?> GetIdentityProviderDisplayName(string alias) =>
        (await GetCentralIdentityProviderAsync(alias).ConfigureAwait(ConfigureAwaitOptions.None)).DisplayName;

    public async Task UpdateOrCreateCentralIdentityProviderOrganisationMapperAsync(string idpAlias, string organisationName)
    {
        var mapperName = _settings.MappedCompanyAttribute + "-mapper";
        IdentityProviderMapper? mapper;
        try
        {
            mapper = (await _centralIdp.GetIdentityProviderMappersAsync(_settings.CentralRealm, idpAlias).ConfigureAwait(ConfigureAwaitOptions.None))
                .SingleOrDefault(z => z.Name == mapperName);
        }
        catch (InvalidOperationException)
        {
            throw new KeycloakEntityConflictException($"idp {idpAlias} attribute-mapper {mapperName} is ambigous in keycloak");
        }
        if (mapper is null)
        {
            await CreateCentralIdentityProviderOrganisationMapperAsync(idpAlias, organisationName);
        }
        else
        {
            mapper.Config ??= new Dictionary<string, string>
            {
                ["syncMode"] = "INHERIT",
                ["attribute"] = _settings.MappedCompanyAttribute,
            };
            mapper.Config["attribute.value"] = organisationName;
            await _centralIdp.UpdateIdentityProviderMapperAsync(
                _settings.CentralRealm,
                idpAlias,
                mapper.Id ?? throw new KeycloakEntityConflictException($"idp {idpAlias} attribute-mapper {mapperName} has no Id"),
                mapper).ConfigureAwait(ConfigureAwaitOptions.None);
        }
    }

    private async ValueTask<string> GetCentralBrokerEndpointOIDCAsync(string alias)
    {
        var openidconfig = await _centralIdp.GetOpenIDConfigurationAsync(_settings.CentralRealm).ConfigureAwait(ConfigureAwaitOptions.None);
        return new Url(openidconfig.Issuer)
            .AppendPathSegment("/broker/")
            .AppendPathSegment(alias, true)
            .AppendPathSegment("/endpoint")
            .ToString();
    }

    private async ValueTask<string?> GetCentralBrokerEndpointSAMLAsync(string alias)
    {
        var samlDescriptor = await _centralIdp.GetSAMLMetaDataAsync(_settings.CentralRealm).ConfigureAwait(ConfigureAwaitOptions.None);
        return samlDescriptor != null
            ? new Url(samlDescriptor.EntityId)
                .AppendPathSegment("/broker/")
                .AppendPathSegment(alias, true)
                .AppendPathSegment("/endpoint")
                .ToString()
            : null;
    }

    private Task CreateCentralIdentityProviderOrganisationMapperAsync(string alias, string organisationName) =>
        _centralIdp.AddIdentityProviderMapperAsync(
            _settings.CentralRealm,
            alias,
            new IdentityProviderMapper
            {
                Name = _settings.MappedCompanyAttribute + "-mapper",
                _IdentityProviderMapper = "hardcoded-attribute-idp-mapper",
                IdentityProviderAlias = alias,
                Config = new Dictionary<string, string>
                {
                    ["syncMode"] = "INHERIT",
                    ["attribute"] = _settings.MappedCompanyAttribute,
                    ["attribute.value"] = organisationName
                }
            });

    private IdentityProvider GetIdentityProviderTemplate(IamIdentityProviderProtocol providerProtocol)
    {
        switch (providerProtocol)
        {
            case IamIdentityProviderProtocol.OIDC:
                return _settings.OidcIdentityProvider;
            case IamIdentityProviderProtocol.SAML:
                return _settings.SamlIdentityProvider;
            default:
                throw new ArgumentOutOfRangeException($"unexpexted value of providerProtocol: {providerProtocol}");
        }
    }

    private static IamIdentityProviderClientAuthMethod IdentityProviderClientAuthTypeToIamClientAuthMethod(string clientAuthMethod)
    {
        try
        {
            return IdentityProviderClientAuthTypesIamClientAuthMethodDictionary[clientAuthMethod];
        }
        catch (KeyNotFoundException)
        {
            throw new ArgumentException($"unexpected value of clientAuthMethod: {clientAuthMethod}", nameof(clientAuthMethod));
        }
    }

    private static string IamIdentityProviderClientAuthMethodToInternal(IamIdentityProviderClientAuthMethod iamClientAuthMethod)
    {
        try
        {
            return IamIdentityProviderClientAuthMethodsInternalDictionary[iamClientAuthMethod];
        }
        catch (KeyNotFoundException)
        {
            throw new ArgumentException($"unexpected value of IamClientAuthMethod: {iamClientAuthMethod}", nameof(iamClientAuthMethod));
        }
    }

    private static IdentityProviderMapperType KeycloakIdentityProviderMapperTypeToEnum(string mapperType)
    {
        try
        {
            return IdentityProviderKeycloakMapperTypesToEnumDictionary[mapperType];
        }
        catch (KeyNotFoundException)
        {
            throw new ArgumentException($"unexpected value of mapperType: {mapperType}", nameof(mapperType));
        }
    }

    private static IdentityProvider CloneIdentityProvider(IdentityProvider identityProvider) =>
        JsonSerializer.Deserialize<IdentityProvider>(JsonSerializer.Serialize(identityProvider))!;
}
