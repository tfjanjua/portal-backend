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

namespace Org.Eclipse.TractusX.Portal.Backend.Clearinghouse.Library;

public static class ValidationModes
{
    /// <summary>
    /// DEFAULT - validates whether the identifiers themselves exists, indepenedent of their relationship to the legal entity provided
    /// </summary>
    public const string IDENTIFIER = "IDENTIFIER";
    /// <summary>
    /// Validates whether the identifier is valid, and whether the name of the legal entity it is associated with matches the provided legal name
    /// </summary>
    public const string LEGAL_NAME = "LEGAL_NAME";
    /// <summary>
    /// Validates whether the identifier is valid, and whether the name of the legal entity, as well as the addresss it is associated with matches the provided ones.
    /// </summary>
    public const string LEGAL_NAME_AND_ADDRESS = "LEGAL_NAME_AND_ADDRESS";
}