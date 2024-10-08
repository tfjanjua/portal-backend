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

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.BusinessLogic;

public class KeycloakSeeder : IKeycloakSeeder
{
    private readonly KeycloakSeederSettings _settings;
    private readonly ISeedDataHandler _seedData;
    private readonly IRealmUpdater _realmUpdater;
    private readonly IRolesUpdater _rolesUpdater;
    private readonly IClientsUpdater _clientsUpdater;
    private readonly IIdentityProvidersUpdater _identityProvidersUpdater;
    private readonly IUsersUpdater _usersUpdater;
    private readonly IClientScopesUpdater _clientScopesUpdater;
    private readonly IAuthenticationFlowsUpdater _authenticationFlowsUpdater;
    private readonly IClientScopeMapperUpdater _clientScopeMapperUpdater;

    public KeycloakSeeder(ISeedDataHandler seedDataHandler, IRealmUpdater realmUpdater, IRolesUpdater rolesUpdater, IClientsUpdater clientsUpdater, IIdentityProvidersUpdater identityProvidersUpdater, IUsersUpdater usersUpdater, IClientScopesUpdater clientScopesUpdater, IAuthenticationFlowsUpdater authenticationFlowsUpdater, IClientScopeMapperUpdater clientScopeMapperUpdater, IOptions<KeycloakSeederSettings> options)
    {
        _seedData = seedDataHandler;
        _realmUpdater = realmUpdater;
        _rolesUpdater = rolesUpdater;
        _clientsUpdater = clientsUpdater;
        _identityProvidersUpdater = identityProvidersUpdater;
        _usersUpdater = usersUpdater;
        _clientScopesUpdater = clientScopesUpdater;
        _authenticationFlowsUpdater = authenticationFlowsUpdater;
        _clientScopeMapperUpdater = clientScopeMapperUpdater;
        _settings = options.Value;
    }

    public async Task Seed(CancellationToken cancellationToken)
    {
        foreach (var realm in _settings.Realms)
        {
            await _seedData.Import(realm, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
            await _realmUpdater.UpdateRealm(realm.InstanceName, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
            await _rolesUpdater.UpdateRealmRoles(realm.InstanceName, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
            await _clientScopesUpdater.UpdateClientScopes(realm.InstanceName, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
            await _clientsUpdater.UpdateClients(realm.InstanceName, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
            await _rolesUpdater.UpdateClientRoles(realm.InstanceName, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
            await _rolesUpdater.UpdateCompositeRoles(realm.InstanceName, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
            await _identityProvidersUpdater.UpdateIdentityProviders(realm.InstanceName, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
            await _usersUpdater.UpdateUsers(realm.InstanceName, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
            await _clientScopeMapperUpdater.UpdateClientScopeMapper(realm.InstanceName, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
            await _authenticationFlowsUpdater.UpdateAuthenticationFlows(realm.InstanceName, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        }
    }
}
