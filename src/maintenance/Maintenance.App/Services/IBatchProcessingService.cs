/********************************************************************************
 * Copyright (c) 2024 Contributors to the Eclipse Foundation
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

namespace Org.Eclipse.TractusX.Portal.Backend.Maintenance.App.Services;

/// <summary>
/// Service to delete the pending and inactive documents as well as the depending on consents from the database
/// </summary>
public interface IBatchProcessingService
{
    /// <summary>
    /// Cleans up the documents and related entries from the database
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    Task CleanupDocuments(CancellationToken cancellationToken);

    /// <summary>
    /// Retriggers the clearinghouse validation step of companies
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    Task RetriggerClearinghouseProcess(CancellationToken cancellationToken);
}
