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

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

public enum ProcessStepTypeId
{
    // ApplicationChecklist Process
    MANUAL_VERIFY_REGISTRATION = 1,
    CREATE_BUSINESS_PARTNER_NUMBER_PUSH = 2,
    CREATE_BUSINESS_PARTNER_NUMBER_PULL = 3,
    CREATE_BUSINESS_PARTNER_NUMBER_MANUAL = 4,
    CREATE_IDENTITY_WALLET = 5,
    RETRIGGER_IDENTITY_WALLET = 6,
    START_CLEARING_HOUSE = 7,
    RETRIGGER_CLEARING_HOUSE = 8,
    AWAIT_CLEARING_HOUSE_RESPONSE = 9,
    START_SELF_DESCRIPTION_LP = 10,
    RETRIGGER_SELF_DESCRIPTION_LP = 11,
    START_APPLICATION_ACTIVATION = 12,
    RETRIGGER_BUSINESS_PARTNER_NUMBER_PUSH = 13,
    RETRIGGER_BUSINESS_PARTNER_NUMBER_PULL = 14,
    MANUAL_TRIGGER_OVERRIDE_CLEARING_HOUSE = 16,
    START_OVERRIDE_CLEARING_HOUSE = 17,
    AWAIT_SELF_DESCRIPTION_LP_RESPONSE = 18,
    MANUAL_DECLINE_APPLICATION = 19,
    CREATE_DIM_WALLET = 20,
    AWAIT_DIM_RESPONSE_RESPONSE = 21,
    RETRIGGER_CREATE_DIM_WALLET = 22,
    VALIDATE_DID_DOCUMENT = 23,
    RETRIGGER_VALIDATE_DID_DOCUMENT = 24,
    REQUEST_BPN_CREDENTIAL = 25,
    AWAIT_BPN_CREDENTIAL_RESPONSE = 26,
    REQUEST_MEMBERSHIP_CREDENTIAL = 27,
    AWAIT_MEMBERSHIP_CREDENTIAL_RESPONSE = 28,
    TRANSMIT_BPN_DID = 29,
    RETRIGGER_TRANSMIT_DID_BPN = 30,
    RETRIGGER_REQUEST_BPN_CREDENTIAL = 31,
    RETRIGGER_REQUEST_MEMBERSHIP_CREDENTIAL = 32,
    ASSIGN_INITIAL_ROLES = 33,
    ASSIGN_BPN_TO_USERS = 34,
    REMOVE_REGISTRATION_ROLES = 35,
    SET_THEME = 36,
    SET_MEMBERSHIP = 37,
    FINISH_APPLICATION_ACTIVATION = 38,
    RETRIGGER_ASSIGN_INITIAL_ROLES = 39,
    RETRIGGER_ASSIGN_BPN_TO_USERS = 40,
    RETRIGGER_REMOVE_REGISTRATION_ROLES = 41,
    RETRIGGER_SET_THEME = 42,
    RETRIGGER_SET_MEMBERSHIP = 43,
    SET_CX_MEMBERSHIP_IN_BPDM = 44,
    RETRIGGER_SET_CX_MEMBERSHIP_IN_BPDM = 45,

    // OfferSubscriptionProcess
    TRIGGER_PROVIDER = 100,
    AWAIT_START_AUTOSETUP = 101,
    OFFERSUBSCRIPTION_CLIENT_CREATION = 102,
    SINGLE_INSTANCE_SUBSCRIPTION_DETAILS_CREATION = 103,
    OFFERSUBSCRIPTION_TECHNICALUSER_CREATION = 104,
    ACTIVATE_SUBSCRIPTION = 105,
    TRIGGER_PROVIDER_CALLBACK = 106,
    RETRIGGER_PROVIDER = 107,
    RETRIGGER_OFFERSUBSCRIPTION_CLIENT_CREATION = 108,
    RETRIGGER_OFFERSUBSCRIPTION_TECHNICALUSER_CREATION = 109,
    RETRIGGER_PROVIDER_CALLBACK = 110,
    MANUAL_TRIGGER_ACTIVATE_SUBSCRIPTION = 111,
    OFFERSUBSCRIPTION_CREATE_DIM_TECHNICAL_USER = 112,
    RETRIGGER_OFFERSUBSCRIPTION_CREATE_DIM_TECHNICAL_USER = 113,

    // NetworkRegistration
    SYNCHRONIZE_USER = 200,
    RETRIGGER_SYNCHRONIZE_USER = 201,
    TRIGGER_CALLBACK_OSP_SUBMITTED = 202,
    TRIGGER_CALLBACK_OSP_APPROVED = 203,
    TRIGGER_CALLBACK_OSP_DECLINED = 204,
    RETRIGGER_CALLBACK_OSP_SUBMITTED = 205,
    RETRIGGER_CALLBACK_OSP_APPROVED = 206,
    RETRIGGER_CALLBACK_OSP_DECLINED = 207,
    MANUAL_DECLINE_OSP = 208,
    REMOVE_KEYCLOAK_USERS = 209,
    RETRIGGER_REMOVE_KEYCLOAK_USERS = 210,

    // MAILING
    SEND_MAIL = 301,
    RETRIGGER_SEND_MAIL = 302,

    // INVITATION
    INVITATION_CREATE_CENTRAL_IDP = 400,
    INVITATION_CREATE_SHARED_IDP_SERVICE_ACCOUNT = 401,
    INVITATION_ADD_REALM_ROLE = 402,
    INVITATION_CREATE_SHARED_REALM = 403,
    INVITATION_CREATE_CENTRAL_IDP_ORG_MAPPER = 404,
    INVITATION_UPDATE_CENTRAL_IDP_URLS = 405,
    INVITATION_CREATE_SHARED_CLIENT = 406,
    INVITATION_ENABLE_CENTRAL_IDP = 407,
    INVITATION_CREATE_DATABASE_IDP = 408,
    INVITATION_CREATE_USER = 409,
    RETRIGGER_INVITATION_CREATE_CENTRAL_IDP = 410,
    RETRIGGER_INVITATION_CREATE_SHARED_IDP_SERVICE_ACCOUNT = 411,
    RETRIGGER_INVITATION_ADD_REALM_ROLE = 412,
    RETRIGGER_INVITATION_CREATE_SHARED_REALM = 413,
    RETRIGGER_INVITATION_CREATE_CENTRAL_IDP_ORG_MAPPER = 414,
    RETRIGGER_INVITATION_UPDATE_CENTRAL_IDP_URLS = 415,
    RETRIGGER_INVITATION_CREATE_SHARED_CLIENT = 416,
    RETRIGGER_INVITATION_ENABLE_CENTRAL_IDP = 417,
    RETRIGGER_INVITATION_CREATE_USER = 418,
    RETRIGGER_INVITATION_CREATE_DATABASE_IDP = 419,

    // DIM_TECHNICAL_USER
    CREATE_DIM_TECHNICAL_USER = 500,
    RETRIGGER_CREATE_DIM_TECHNICAL_USER = 501,
    AWAIT_CREATE_DIM_TECHNICAL_USER_RESPONSE = 502,
    DELETE_DIM_TECHNICAL_USER = 504,
    AWAIT_DELETE_DIM_TECHNICAL_USER_RESPONSE = 505,
    RETRIGGER_DELETE_DIM_TECHNICAL_USER = 506,

    // USER_PROVISIONING
    DELETE_CENTRAL_USER = 600,
    RETRIGGER_DELETE_CENTRAL_USER = 601,
    DELETE_COMPANYUSER_ASSIGNED_PROCESS = 602,

    // IDENTITYPROVIDER_PPROVISIONING
    DELETE_IDP_SHARED_REALM = 700,
    RETRIGGER_DELETE_IDP_SHARED_REALM = 701,
    DELETE_IDP_SHARED_SERVICEACCOUNT = 702,
    RETRIGGER_DELETE_IDP_SHARED_SERVICEACCOUNT = 703,
    DELETE_CENTRAL_IDENTITY_PROVIDER = 704,
    RETRIGGER_DELETE_CENTRAL_IDENTITY_PROVIDER = 705,
    DELETE_IDENTITY_PROVIDER = 706,

    // SELF_DESCRIPTION_CREATION
    SELF_DESCRIPTION_CONNECTOR_CREATION = 800,
    SELF_DESCRIPTION_COMPANY_CREATION = 801,
    RETRIGGER_SELF_DESCRIPTION_CONNECTOR_CREATION = 802,
    RETRIGGER_SELF_DESCRIPTION_COMPANY_CREATION = 803,
    AWAIT_SELF_DESCRIPTION_CONNECTOR_RESPONSE = 804,
    AWAIT_SELF_DESCRIPTION_COMPANY_RESPONSE = 805,
    RETRIGGER_AWAIT_SELF_DESCRIPTION_CONNECTOR_RESPONSE = 806,
    RETRIGGER_AWAIT_SELF_DESCRIPTION_COMPANY_RESPONSE = 807
}
