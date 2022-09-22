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

using CatenaX.NetworkServices.Framework.ErrorHandling;
using CatenaX.NetworkServices.PortalBackend.DBAccess;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;

namespace CatenaX.NetworkServices.Administration.Service.BusinessLogic;

/// <inheritdoc />
public class ServiceProviderBusinessLogic : IServiceProviderBusinessLogic
{
    private readonly IPortalRepositories _portalRepositories;

    /// <summary>
    /// Creates a new instance of <see cref="ServiceProviderBusinessLogic"/>
    /// </summary>
    /// <param name="portalRepositories">Access to the portal repositories</param>
    public ServiceProviderBusinessLogic(IPortalRepositories portalRepositories)
    {
        _portalRepositories = portalRepositories;
    }

    /// <inheritdoc />
    public async Task CreateServiceProviderCompanyDetails(Guid companyId, ServiceProviderDetailData data, string iamUserId)
    {
        if (string.IsNullOrWhiteSpace(data.Url) || !data.Url.StartsWith("https://") || data.Url.Length > 100)
        {
            throw new ControllerArgumentException("Url must start with https and the maximum allowed length is 100 characters", nameof(data.Url));
        }

        if (!await _portalRepositories.GetInstance<ICompanyRepository>().CheckCompanyIsServiceProviderAndExistsForIamUser(companyId, iamUserId, CompanyRoleId.SERVICE_PROVIDER).ConfigureAwait(false))
        {
            throw new ControllerArgumentException($"IAmUser {iamUserId} is not assigned to company {companyId}", nameof(iamUserId));
        }

        _portalRepositories.GetInstance<ICompanyRepository>().CreateServiceProviderCompanyDetail(companyId, data.Url);
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }
}