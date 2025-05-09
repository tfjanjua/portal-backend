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
using Org.Eclipse.TractusX.Portal.Backend.Framework.Identity;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Concrete.Context;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.AuditEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Auditing;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Views;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;

/// <summary>
/// Db Context
/// </summary>
/// <remarks>
/// The Trigger Framework requires new Guid() to convert it to gen_random_uuid(),
/// for the Id field we'll use a randomly set UUID to satisfy SonarCloud.
/// </remarks>
public class PortalDbContext(DbContextOptions<PortalDbContext> options, IAuditHandler auditHandler) : ProcessDbContext<Process, ProcessTypeId, ProcessStepTypeId>(options)
{
    private readonly IAuditHandler _auditHandler = auditHandler;

    public virtual DbSet<Address> Addresses { get; set; } = default!;
    public virtual DbSet<Agreement> Agreements { get; set; } = default!;
    public virtual DbSet<AgreementAssignedCompanyRole> AgreementAssignedCompanyRoles { get; set; } = default!;
    public virtual DbSet<AgreementAssignedOffer> AgreementAssignedOffers { get; set; } = default!;
    public virtual DbSet<AgreementAssignedOfferType> AgreementAssignedOfferTypes { get; set; } = default!;
    public virtual DbSet<AgreementCategory> AgreementCategories { get; set; } = default!;
    public virtual DbSet<AppInstance> AppInstances { get; set; } = default!;
    public virtual DbSet<AppInstanceAssignedTechnicalUser> AppInstanceAssignedTechnicalUsers { get; set; } = default!;
    public virtual DbSet<AppInstanceSetup> AppInstanceSetups { get; set; } = default!;
    public virtual DbSet<AppAssignedUseCase> AppAssignedUseCases { get; set; } = default!;
    public virtual DbSet<AppLanguage> AppLanguages { get; set; } = default!;
    public virtual DbSet<ApplicationChecklistEntry> ApplicationChecklist { get; set; } = default!;
    public virtual DbSet<ApplicationChecklistEntryStatus> ApplicationChecklistStatuses { get; set; } = default!;
    public virtual DbSet<ApplicationChecklistEntryType> ApplicationChecklistTypes { get; set; } = default!;
    public virtual DbSet<AppSubscriptionDetail> AppSubscriptionDetails { get; set; } = default!;
    public virtual DbSet<AuditAppSubscriptionDetail20221118> AuditAppSubscriptionDetail20221118 { get; set; } = default!;
    public virtual DbSet<AuditAppSubscriptionDetail20231115> AuditAppSubscriptionDetail20231115 { get; set; } = default!;
    public virtual DbSet<AuditOffer20230119> AuditOffer20230119 { get; set; } = default!;
    public virtual DbSet<AuditOffer20230406> AuditOffer20230406 { get; set; } = default!;
    public virtual DbSet<AuditOffer20231115> AuditOffer20231115 { get; set; } = default!;
    public virtual DbSet<AuditOffer20240911> AuditOffer20240911 { get; set; } = default!;
    public virtual DbSet<AuditOffer20241219> AuditOffer20241219 { get; set; } = default!;
    public virtual DbSet<AuditOffer20250121> AuditOffer20250121 { get; set; } = default!;
    public virtual DbSet<AuditOfferSubscription20221005> AuditOfferSubscription20221005 { get; set; } = default!;
    public virtual DbSet<AuditOfferSubscription20230317> AuditOfferSubscription20230317 { get; set; } = default!;
    public virtual DbSet<AuditOfferSubscription20231013> AuditOfferSubscription20231013 { get; set; } = default!;
    public virtual DbSet<AuditOfferSubscription20231115> AuditOfferSubscription20231115 { get; set; } = default!;
    public virtual DbSet<AuditCompanyApplication20221005> AuditCompanyApplication20221005 { get; set; } = default!;
    public virtual DbSet<AuditCompanyApplication20230214> AuditCompanyApplication20230214 { get; set; } = default!;
    public virtual DbSet<AuditCompanyApplication20230824> AuditCompanyApplication20230824 { get; set; } = default!;
    public virtual DbSet<AuditCompanyApplication20231115> AuditCompanyApplication20231115 { get; set; } = default!;
    public virtual DbSet<AuditCompanyUser20221005> AuditCompanyUser20221005 { get; set; } = default!;
    public virtual DbSet<AuditCompanyUser20230522> AuditCompanyUser20230523 { get; set; } = default!;
    public virtual DbSet<AuditConnector20230405> AuditConnector20230405 { get; set; } = default!;
    public virtual DbSet<AuditConnector20230503> AuditConnector20230503 { get; set; } = default!;
    public virtual DbSet<AuditConnector20230803> AuditConnector20230803 { get; set; } = default!;
    public virtual DbSet<AuditConnector20231115> AuditConnector20231115 { get; set; } = default!;
    public virtual DbSet<AuditConnector20240814> AuditConnector20240814 { get; set; } = default!;
    public virtual DbSet<AuditConnector20241008> AuditConnector20241008 { get; set; } = default!;
    public virtual DbSet<AuditConnector20250113> AuditConnector20250113 { get; set; } = default!;
    public virtual DbSet<AuditIdentity20230526> AuditIdentity20230526 { get; set; } = default!;
    public virtual DbSet<AuditIdentity20231115> AuditIdentity20231115 { get; set; } = default!;
    public virtual DbSet<AuditUserRole20221017> AuditUserRole20221017 { get; set; } = default!;
    public virtual DbSet<AuditUserRole20231115> AuditUserRole20231115 { get; set; } = default!;
    public virtual DbSet<AuditCompanyUserAssignedRole20221018> AuditCompanyUserAssignedRole20221018 { get; set; } = default!;
    public virtual DbSet<AuditCompanyAssignedRole2023316> AuditCompanyAssignedRole2023316 { get; set; } = default!;
    public virtual DbSet<AuditConsent20230412> AuditConsent20230412 { get; set; } = default!;
    public virtual DbSet<AuditConsent20231115> AuditConsent20231115 { get; set; } = default!;
    public virtual DbSet<AuditIdentityAssignedRole20230522> AuditIdentityAssignedRole20230522 { get; set; } = default!;
    public virtual DbSet<AuditProviderCompanyDetail20230614> AuditProviderCompanyDetail20230614 { get; set; } = default!;
    public virtual DbSet<AuditProviderCompanyDetail20231115> AuditProviderCompanyDetail20231115 { get; set; } = default!;
    public virtual DbSet<AuditProviderCompanyDetail20250415> AuditProviderCompanyDetail20241210 { get; set; } = default!;
    public virtual DbSet<AuditDocument20231108> AuditDocument20231108 { get; set; } = default!;
    public virtual DbSet<AuditDocument20231115> AuditDocument20231115 { get; set; } = default!;
    public virtual DbSet<AuditDocument20241120> AuditDocument20241120 { get; set; } = default!;
    public virtual DbSet<BpdmIdentifier> BpdmIdentifiers { get; set; } = default!;
    public virtual DbSet<Company> Companies { get; set; } = default!;
    public virtual DbSet<CompanyApplication> CompanyApplications { get; set; } = default!;
    public virtual DbSet<AuditCertificateManagement20240416> AuditCertificateManagement20240416 { get; set; } = default!;
    public virtual DbSet<CompanyApplicationStatus> CompanyApplicationStatuses { get; set; } = default!;
    public virtual DbSet<CompanyApplicationType> CompanyApplicationTypes { get; set; } = default!;
    public virtual DbSet<CompanyCertificate> CompanyCertificates { get; set; } = default!;
    public virtual DbSet<CompanyCertificateStatus> CompanyCertificateStatuses { get; set; } = default!;
    public virtual DbSet<CompanyCertificateType> CompanyCertificateTypes { get; set; } = default!;
    public virtual DbSet<CompanyCertificateTypeStatus> CompanyCertificateTypeStatuses { get; set; } = default!;
    public virtual DbSet<CompanyCertificateTypeAssignedStatus> CompanyCertificateTypeAssignedStatuses { get; set; } = default!;
    public virtual DbSet<CompanyCertificateTypeDescription> CompanyCertificateTypeDescriptions { get; set; } = default!;
    public virtual DbSet<CompanyCertificateAssignedSite> CompanyCertificateAssignedSites { get; set; } = default!;
    public virtual DbSet<CompanyAssignedRole> CompanyAssignedRoles { get; set; } = default!;
    public virtual DbSet<CompanyAssignedUseCase> CompanyAssignedUseCases { get; set; } = default!;
    public virtual DbSet<CompanyIdentifier> CompanyIdentifiers { get; set; } = default!;
    public virtual DbSet<CompanyIdentityProvider> CompanyIdentityProviders { get; set; } = default!;
    public virtual DbSet<CompanyInvitation> CompanyInvitations { get; set; } = default!;
    public virtual DbSet<CompanyRoleAssignedRoleCollection> CompanyRoleAssignedRoleCollections { get; set; } = default!;
    public virtual DbSet<CompanyRoleDescription> CompanyRoleDescriptions { get; set; } = default!;
    public virtual DbSet<CompanyRole> CompanyRoles { get; set; } = default!;
    public virtual DbSet<TechnicalUser> TechnicalUsers { get; set; } = default!;
    public virtual DbSet<TechnicalUserType> TechnicalUserTypes { get; set; } = default!;
    public virtual DbSet<TechnicalUserKind> TechnicalUserKinds { get; set; } = default!;
    public virtual DbSet<CompanyStatus> CompanyStatuses { get; set; } = default!;
    public virtual DbSet<CompanyUser> CompanyUsers { get; set; } = default!;
    public virtual DbSet<CompanyUserAssignedAppFavourite> CompanyUserAssignedAppFavourites { get; set; } = default!;
    public virtual DbSet<CompanyUserAssignedBusinessPartner> CompanyUserAssignedBusinessPartners { get; set; } = default!;
    public virtual DbSet<CompanyUserAssignedIdentityProvider> CompanyUserAssignedIdentityProviders { get; set; } = default!;
    public virtual DbSet<CompanyUserAssignedProcess> CompanyUserAssignedProcesses { get; set; } = default!;
    public virtual DbSet<Connector> Connectors { get; set; } = default!;
    public virtual DbSet<ConnectorAssignedOfferSubscription> ConnectorAssignedOfferSubscriptions { get; set; } = default!;
    public virtual DbSet<ConnectorStatus> ConnectorStatuses { get; set; } = default!;
    public virtual DbSet<ConnectorType> ConnectorTypes { get; set; } = default!;
    public virtual DbSet<Consent> Consents { get; set; } = default!;
    public virtual DbSet<ConsentAssignedOffer> ConsentAssignedOffers { get; set; } = default!;
    public virtual DbSet<ConsentAssignedOfferSubscription> ConsentAssignedOfferSubscriptions { get; set; } = default!;
    public virtual DbSet<ConsentStatus> ConsentStatuses { get; set; } = default!;
    public virtual DbSet<CountryLongName> CountryLongNames { get; set; } = default!;
    public virtual DbSet<Country> Countries { get; set; } = default!;
    public virtual DbSet<CountryAssignedIdentifier> CountryAssignedIdentifiers { get; set; } = default!;
    public virtual DbSet<ExternalTechnicalUser> ExternalTechnicalUsers { get; set; } = default!;
    public virtual DbSet<ExternalTechnicalUserCreationData> ExternalTechnicalUserCreationData { get; set; } = default!;
    public virtual DbSet<Document> Documents { get; set; } = default!;
    public virtual DbSet<DocumentType> DocumentTypes { get; set; } = default!;
    public virtual DbSet<DocumentStatus> DocumentStatus { get; set; } = default!;
    public virtual DbSet<IamClient> IamClients { get; set; } = default!;
    public virtual DbSet<IamIdentityProvider> IamIdentityProviders { get; set; } = default!;
    public virtual DbSet<Identity> Identities { get; set; } = default!;
    public virtual DbSet<IdentityAssignedRole> IdentityAssignedRoles { get; set; } = default!;
    public virtual DbSet<IdentityProvider> IdentityProviders { get; set; } = default!;
    public virtual DbSet<IdentityProviderAssignedProcess> IdentityProviderAssignedProcesses { get; set; } = default!;
    public virtual DbSet<IdentityProviderType> IdentityProviderTypes { get; set; } = default!;
    public virtual DbSet<IdentityProviderCategory> IdentityProviderCategories { get; set; } = default!;
    public virtual DbSet<IdentityUserStatus> IdentityUserStatuses { get; set; } = default!;
    public virtual DbSet<Invitation> Invitations { get; set; } = default!;
    public virtual DbSet<InvitationStatus> InvitationStatuses { get; set; } = default!;
    public virtual DbSet<Language> Languages { get; set; } = default!;
    public virtual DbSet<LanguageLongName> LanguageLongNames { get; set; } = default!;
    public virtual DbSet<LicenseType> LicenseTypes { get; set; } = default!;
    public virtual DbSet<MediaType> MediaTypes { get; set; } = default!;
    public virtual DbSet<MailingInformation> MailingInformations { get; set; } = default!;
    public virtual DbSet<MailingStatus> MailingStatuses { get; set; } = default!;
    public virtual DbSet<NetworkRegistration> NetworkRegistrations { get; set; } = default!;
    public virtual DbSet<Notification> Notifications { get; set; } = default!;
    public virtual DbSet<NotificationTypeAssignedTopic> NotificationTypeAssignedTopics { get; set; } = default!;
    public virtual DbSet<Offer> Offers { get; set; } = default!;
    public virtual DbSet<OfferAssignedDocument> OfferAssignedDocuments { get; set; } = default!;
    public virtual DbSet<OfferAssignedLicense> OfferAssignedLicenses { get; set; } = default!;
    public virtual DbSet<OfferAssignedPrivacyPolicy> OfferAssignedPrivacyPolicies { get; set; } = default!;
    public virtual DbSet<OfferDescription> OfferDescriptions { get; set; } = default!;
    public virtual DbSet<OfferLicense> OfferLicenses { get; set; } = default!;
    public virtual DbSet<OfferStatus> OfferStatuses { get; set; } = default!;
    public virtual DbSet<OfferTag> OfferTags { get; set; } = default!;
    public virtual DbSet<OfferType> OfferTypes { get; set; } = default!;
    public virtual DbSet<OfferSubscription> OfferSubscriptions { get; set; } = default!;
    public virtual DbSet<OfferSubscriptionStatus> OfferSubscriptionStatuses { get; set; } = default!;
    public virtual DbSet<OfferSubscriptionProcessData> OfferSubscriptionsProcessDatas { get; set; } = default!;
    public virtual DbSet<OnboardingServiceProviderDetail> OnboardingServiceProviderDetails { get; set; } = default!;
    public virtual DbSet<ProviderCompanyDetail> ProviderCompanyDetails { get; set; } = default!;
    public virtual DbSet<PrivacyPolicy> PrivacyPolicies { get; set; } = default!;
    public virtual DbSet<ServiceDetail> ServiceDetails { get; set; } = default!;
    public virtual DbSet<ServiceType> ServiceTypes { get; set; } = default!;
    public virtual DbSet<TechnicalUserProfile> TechnicalUserProfiles { get; set; } = default!;
    public virtual DbSet<TechnicalUserProfileAssignedUserRole> TechnicalUserProfileAssignedUserRoles { get; set; } = default!;
    public virtual DbSet<UniqueIdentifier> UniqueIdentifiers { get; set; } = default!;
    public virtual DbSet<UseCase> UseCases { get; set; } = default!;
    public virtual DbSet<UseCaseDescription> UseCaseDescriptions { get; set; } = default!;
    public virtual DbSet<UserRole> UserRoles { get; set; } = default!;
    public virtual DbSet<UserRoleAssignedCollection> UserRoleAssignedCollections { get; set; } = default!;
    public virtual DbSet<UserRoleCollection> UserRoleCollections { get; set; } = default!;
    public virtual DbSet<UserRoleCollectionDescription> UserRoleCollectionDescriptions { get; set; } = default!;
    public virtual DbSet<UserRoleDescription> UserRoleDescriptions { get; set; } = default!;
    public virtual DbSet<CompaniesLinkedTechnicalUser> CompanyLinkedTechnicalUsers { get; set; } = default!;
    public virtual DbSet<OfferSubscriptionView> OfferSubscriptionView { get; set; } = default!;
    public virtual DbSet<CompanyUsersView> CompanyUsersView { get; set; } = default!;
    public virtual DbSet<CompanyIdpView> CompanyIdpView { get; set; } = default!;
    public virtual DbSet<CompanyConnectorView> CompanyConnectorView { get; set; } = default!;
    public virtual DbSet<CompanyRoleCollectionRolesView> CompanyRoleCollectionRolesView { get; set; } = default!;
    public virtual DbSet<AgreementStatus> AgreementStatuses { get; set; } = default!;
    public virtual DbSet<AgreementView> AgreementView { get; set; } = default!;
    public virtual DbSet<CompanyWalletData> CompanyWalletDatas { get; set; } = default!;
    public virtual DbSet<AgreementDescription> AgreementDescriptions { get; set; } = default!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSnakeCaseNamingConvention();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("Relational:Collation", "en_US.utf8");
        modelBuilder.HasDefaultSchema("portal");

        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Agreement>(entity =>
        {
            entity.HasOne(d => d.AgreementCategory)
                .WithMany(p => p.Agreements)
                .HasForeignKey(d => d.AgreementCategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.IssuerCompany)
                .WithMany(p => p.Agreements)
                .HasForeignKey(d => d.IssuerCompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.Property(a => a.AgreementStatusId)
                .HasDefaultValue(AgreementStatusId.ACTIVE);

            entity.Property(a => a.Mandatory)
                .HasDefaultValue(true);
        });

        modelBuilder.Entity<AgreementAssignedCompanyRole>(entity =>
        {
            entity.HasKey(e => new { e.AgreementId, e.CompanyRoleId });

            entity.HasOne(d => d.Agreement)
                .WithMany(p => p.AgreementAssignedCompanyRoles)
                .HasForeignKey(d => d.AgreementId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.CompanyRole)
                .WithMany(p => p.AgreementAssignedCompanyRoles)
                .HasForeignKey(d => d.CompanyRoleId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<AgreementAssignedOffer>(entity =>
        {
            entity.HasKey(e => new { e.AgreementId, e.OfferId });

            entity.HasOne(d => d.Agreement)
                .WithMany(p => p.AgreementAssignedOffers)
                .HasForeignKey(d => d.AgreementId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Offer)
                .WithMany(p => p.AgreementAssignedOffers)
                .HasForeignKey(d => d.OfferId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<AgreementAssignedOfferType>(entity =>
        {
            entity.HasKey(e => new { e.AgreementId, e.OfferTypeId });

            entity.HasOne(d => d.Agreement)
                .WithMany(p => p.AgreementAssignedOfferTypes)
                .HasForeignKey(d => d.AgreementId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.OfferType)
                .WithMany(p => p.AgreementAssignedOfferTypes)
                .HasForeignKey(d => d.OfferTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<AgreementCategory>()
            .HasData(
                Enum.GetValues(typeof(AgreementCategoryId))
                    .Cast<AgreementCategoryId>()
                    .Select(e => new AgreementCategory(e))
            );

        modelBuilder.Entity<ConsentAssignedOffer>(entity =>
        {
            entity.HasKey(e => new { e.ConsentId, e.OfferId });

            entity.HasOne(d => d.Consent)
                .WithMany(p => p.ConsentAssignedOffers)
                .HasForeignKey(d => d.ConsentId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Offer)
                .WithMany(p => p.ConsentAssignedOffers)
                .HasForeignKey(d => d.OfferId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Offer>(entity =>
        {
            entity.HasOne(d => d.ProviderCompany)
                .WithMany(p => p.ProvidedOffers);

            entity.HasOne(x => x.SalesManager)
                .WithMany(x => x.SalesManagerOfOffers)
                .HasForeignKey(x => x.SalesManagerId);

            entity.HasOne(x => x.OfferType)
                .WithMany(x => x.Offers)
                .HasForeignKey(x => x.OfferTypeId);

            entity.HasMany(p => p.Companies)
                .WithMany(p => p.BoughtOffers)
                .UsingEntity<OfferSubscription>(
                    j => j
                        .HasOne(d => d.Company!)
                        .WithMany()
                        .HasForeignKey(d => d.CompanyId)
                        .OnDelete(DeleteBehavior.ClientSetNull),
                    j => j
                        .HasOne(d => d.Offer!)
                        .WithMany()
                        .HasForeignKey(d => d.OfferId)
                        .OnDelete(DeleteBehavior.ClientSetNull),
                    j =>
                    {
                        j.HasKey(e => e.Id);
                        j.HasOne(e => e.OfferSubscriptionStatus)
                            .WithMany(e => e.OfferSubscriptions)
                            .HasForeignKey(e => e.OfferSubscriptionStatusId)
                            .OnDelete(DeleteBehavior.ClientSetNull);
                        j.HasOne(e => e.Requester)
                            .WithMany(e => e.RequestedSubscriptions)
                            .HasForeignKey(e => e.RequesterId)
                            .OnDelete(DeleteBehavior.ClientSetNull);
                        j.Property(e => e.OfferSubscriptionStatusId)
                            .HasDefaultValue(OfferSubscriptionStatusId.PENDING);

                        j.HasAuditV1Triggers<OfferSubscription, AuditOfferSubscription20231115>();
                    }
                );

            entity.HasMany(a => a.SupportedLanguages)
                .WithMany(l => l.SupportingApps)
                .UsingEntity<AppLanguage>(
                    j => j
                        .HasOne(d => d.Language!)
                        .WithMany()
                        .HasForeignKey(d => d.LanguageShortName)
                        .OnDelete(DeleteBehavior.ClientSetNull),
                    j => j
                        .HasOne(d => d.App!)
                        .WithMany()
                        .HasForeignKey(d => d.AppId)
                        .OnDelete(DeleteBehavior.ClientSetNull),
                    j =>
                    {
                        j.HasKey(e => new { e.AppId, e.LanguageShortName });
                    }
                );

            entity.HasMany(p => p.OfferLicenses)
                .WithMany(p => p.Offers)
                .UsingEntity<OfferAssignedLicense>(
                    j => j
                        .HasOne(d => d.OfferLicense!)
                        .WithMany()
                        .HasForeignKey(d => d.OfferLicenseId)
                        .OnDelete(DeleteBehavior.ClientSetNull),
                    j => j
                        .HasOne(d => d.Offer!)
                        .WithMany()
                        .HasForeignKey(d => d.OfferId)
                        .OnDelete(DeleteBehavior.ClientSetNull),
                    j =>
                    {
                        j.HasKey(e => new { AppId = e.OfferId, AppLicenseId = e.OfferLicenseId });
                    });

            entity.HasMany(p => p.UseCases)
                .WithMany(p => p.Apps)
                .UsingEntity<AppAssignedUseCase>(
                    j => j
                        .HasOne(d => d.UseCase!)
                        .WithMany()
                        .HasForeignKey(d => d.UseCaseId)
                        .OnDelete(DeleteBehavior.ClientSetNull),
                    j => j
                        .HasOne(d => d.App!)
                        .WithMany()
                        .HasForeignKey(d => d.AppId)
                        .OnDelete(DeleteBehavior.ClientSetNull),
                    j =>
                    {
                        j.HasKey(e => new { e.AppId, e.UseCaseId });
                    });

            entity.HasMany(p => p.Documents)
                .WithMany(p => p.Offers)
                .UsingEntity<OfferAssignedDocument>(
                    j => j
                        .HasOne(d => d.Document!)
                        .WithMany()
                        .HasForeignKey(d => d.DocumentId)
                        .OnDelete(DeleteBehavior.ClientSetNull),
                    j => j
                        .HasOne(d => d.Offer!)
                        .WithMany()
                        .HasForeignKey(d => d.OfferId)
                        .OnDelete(DeleteBehavior.ClientSetNull),
                    j =>
                    {
                        j.HasKey(e => new { e.OfferId, e.DocumentId });
                    });

            entity.HasMany(p => p.OfferSubscriptions)
                .WithOne(d => d.Offer)
                .HasForeignKey(d => d.OfferId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasAuditV1Triggers<Offer, AuditOffer20250121>();
        });

        modelBuilder.Entity<AppSubscriptionDetail>(entity =>
        {
            entity.HasOne(e => e.AppInstance)
                .WithMany(e => e.AppSubscriptionDetails)
                .HasForeignKey(e => e.AppInstanceId);

            entity.HasOne(e => e.OfferSubscription)
                .WithOne(e => e.AppSubscriptionDetail)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasAuditV1Triggers<AppSubscriptionDetail, AuditAppSubscriptionDetail20231115>();
        });

        modelBuilder.Entity<OfferType>()
            .HasData(
                Enum.GetValues(typeof(OfferTypeId))
                    .Cast<OfferTypeId>()
                    .Select(e => new OfferType(e))
            );

        modelBuilder.Entity<ServiceType>()
            .HasData(
                Enum.GetValues(typeof(ServiceTypeId))
                    .Cast<ServiceTypeId>()
                    .Select(e => new ServiceType(e))
            );

        modelBuilder.Entity<AppInstance>(entity =>
        {
            entity.HasOne(x => x.App)
                .WithMany(x => x.AppInstances)
                .HasForeignKey(x => x.AppId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(x => x.IamClient)
                .WithMany(x => x.AppInstances)
                .HasForeignKey(x => x.IamClientId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<AppInstanceAssignedTechnicalUser>(entity =>
        {
            entity.HasKey(x => new { x.AppInstanceId, x.TechnicalUserId });
            entity.HasOne(x => x.AppInstance)
                .WithMany(x => x.AppInstanceAssignedTechnicalUsers)
                .HasForeignKey(x => x.AppInstanceId)
                .OnDelete(DeleteBehavior.ClientSetNull);
            entity.HasOne(x => x.TechnicalUser)
                .WithMany(x => x.AppInstanceAssignedTechnicalUsers)
                .HasForeignKey(x => x.TechnicalUserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<AppInstanceSetup>(entity =>
        {
            entity.HasOne(x => x.App)
                .WithOne(x => x.AppInstanceSetup)
                .HasForeignKey<AppInstanceSetup>(x => x.AppId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<OfferDescription>(entity =>
        {
            entity.HasKey(e => new { AppId = e.OfferId, e.LanguageShortName });

            entity.HasOne(d => d.Offer)
                .WithMany(p => p.OfferDescriptions)
                .HasForeignKey(d => d.OfferId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Language)
                .WithMany(p => p.AppDescriptions)
                .HasForeignKey(d => d.LanguageShortName)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<OfferStatus>()
            .HasData(
                Enum.GetValues(typeof(OfferStatusId))
                    .Cast<OfferStatusId>()
                    .Select(e => new OfferStatus(e))
            );

        modelBuilder.Entity<OfferTag>(entity =>
        {
            entity.HasKey(e => new { AppId = e.OfferId, e.Name });

            entity.HasOne(d => d.Offer)
                .WithMany(p => p.Tags)
                .HasForeignKey(d => d.OfferId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<AuditOperation>()
            .HasData(
                Enum.GetValues(typeof(AuditOperationId))
                    .Cast<AuditOperationId>()
                    .Select(e => new AuditOperation(e))
            );

        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasMany(p => p.OfferSubscriptions)
                .WithOne(d => d.Company)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(p => p.SelfDescriptionDocument)
                .WithMany(d => d.Companies)
                .HasForeignKey(d => d.SelfDescriptionDocumentId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.SdCreationProcess)
                .WithOne(p => p.SdCreationCompany)
                .HasForeignKey<Company>(d => d.SdCreationProcessId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasMany(p => p.IdentityProviders)
                .WithMany(p => p.Companies)
                .UsingEntity<CompanyIdentityProvider>(
                    j => j
                        .HasOne(pt => pt.IdentityProvider!)
                        .WithMany()
                        .HasForeignKey(pt => pt.IdentityProviderId),
                    j => j
                        .HasOne(pt => pt.Company!)
                        .WithMany()
                        .HasForeignKey(pt => pt.CompanyId),
                    j =>
                    {
                        j.HasKey(e => new { e.CompanyId, e.IdentityProviderId });
                    }
                );
        });

        modelBuilder.Entity<CompanyAssignedUseCase>(entity =>
        {
            entity.HasKey(e => new { e.CompanyId, e.UseCaseId });

            entity.HasOne(d => d.Company)
                .WithMany(p => p.CompanyAssignedUseCase)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.UseCase)
                .WithMany(p => p.CompanyAssignedUseCase)
                .HasForeignKey(d => d.UseCaseId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<ProviderCompanyDetail>(entity =>
        {
            entity.HasOne(e => e.Company)
                .WithOne(e => e.ProviderCompanyDetail)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasAuditV1Triggers<ProviderCompanyDetail, AuditProviderCompanyDetail20250415>();
        });

        modelBuilder.Entity<CompanyApplication>(entity =>
        {
            entity.HasOne(d => d.Company)
                .WithMany(p => p.CompanyApplications)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Company)
                .WithMany(p => p.CompanyApplications)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(x => x.OnboardingServiceProvider)
                .WithMany(x => x.ProvidedApplications)
                .HasForeignKey(x => x.OnboardingServiceProviderId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasAuditV1Triggers<CompanyApplication, AuditCompanyApplication20231115>();
        });

        modelBuilder.Entity<CompanyApplicationStatus>()
            .HasData(
                Enum.GetValues(typeof(CompanyApplicationStatusId))
                    .Cast<CompanyApplicationStatusId>()
                    .Select(e => new CompanyApplicationStatus(e))
            );

        modelBuilder.Entity<CompanyApplicationType>()
            .HasData(
                Enum.GetValues(typeof(CompanyApplicationTypeId))
                    .Cast<CompanyApplicationTypeId>()
                    .Select(e => new CompanyApplicationType(e))
            );

        modelBuilder.Entity<CompanyCertificate>(entity =>
        {
            entity.HasOne(d => d.Company)
                .WithMany(p => p.CompanyCertificates)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Document)
                .WithMany(p => p.CompanyCertificates)
                .HasForeignKey(d => d.DocumentId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<CompanyCertificateAssignedSite>()
            .HasKey(e => new { e.CompanyCertificateId, e.Site });

        modelBuilder.Entity<CompanyCertificateStatus>()
            .HasData(
                Enum.GetValues(typeof(CompanyCertificateStatusId))
                    .Cast<CompanyCertificateStatusId>()
                    .Select(e => new CompanyCertificateStatus(e))
            );

        modelBuilder.Entity<CompanyCertificateType>()
            .HasData(
                Enum.GetValues(typeof(CompanyCertificateTypeId))
                    .Cast<CompanyCertificateTypeId>()
                    .Select(e => new CompanyCertificateType(e))
            );

        modelBuilder.Entity<CompanyCertificateTypeAssignedStatus>(entity =>
        {
            entity.HasOne(e => e.CompanyCertificateType)
                .WithOne(d => d.CompanyCertificateTypeAssignedStatus)
                .HasForeignKey<CompanyCertificateTypeAssignedStatus>(e => e.CompanyCertificateTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(e => e.CompanyCertificateTypeStatus)
                .WithMany(d => d.CompanyCertificateTypeAssignedStatuses)
                .HasForeignKey(e => e.CompanyCertificateTypeStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<CompanyCertificateTypeDescription>(entity =>
        {
            entity.HasKey(x => new { x.CompanyCertificateTypeId, x.LanguageShortName });

            entity.HasOne(e => e.Language)
                .WithMany(e => e.CompanyCertificateTypeDescriptions)
                .HasForeignKey(e => e.LanguageShortName);
        });

        modelBuilder.Entity<CompanyCertificateTypeStatus>()
            .HasData(
                Enum.GetValues(typeof(CompanyCertificateTypeStatusId))
                    .Cast<CompanyCertificateTypeStatusId>()
                    .Select(e => new CompanyCertificateTypeStatus(e))
            );

        modelBuilder.Entity<ApplicationChecklistEntry>(entity =>
        {
            entity.HasKey(x => new { x.ApplicationId, ChecklistEntryTypeId = x.ApplicationChecklistEntryTypeId });

            entity.HasOne(ace => ace.Application)
                .WithMany(a => a.ApplicationChecklistEntries)
                .HasForeignKey(ace => ace.ApplicationId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<ApplicationChecklistEntryStatus>()
            .HasData(
                Enum.GetValues(typeof(ApplicationChecklistEntryStatusId))
                    .Cast<ApplicationChecklistEntryStatusId>()
                    .Select(e => new ApplicationChecklistEntryStatus(e))
            );

        modelBuilder.Entity<ApplicationChecklistEntryType>()
            .HasData(
                Enum.GetValues(typeof(ApplicationChecklistEntryTypeId))
                    .Cast<ApplicationChecklistEntryTypeId>()
                    .Select(e => new ApplicationChecklistEntryType(e))
            );

        modelBuilder.Entity<CompanyIdentityProvider>()
            .HasOne(d => d.IdentityProvider)
            .WithMany(p => p.CompanyIdentityProviders)
            .HasForeignKey(d => d.IdentityProviderId);

        modelBuilder.Entity<CompanyRole>()
            .HasData(
                Enum.GetValues(typeof(CompanyRoleId))
                    .Cast<CompanyRoleId>()
                    .Select(e => new CompanyRole(e))
            );

        modelBuilder.Entity<CompanyRoleAssignedRoleCollection>();

        modelBuilder.Entity<CompanyRoleDescription>(entity =>
        {
            entity.HasKey(e => new { e.CompanyRoleId, e.LanguageShortName });
        });

        modelBuilder.Entity<CompanyRoleRegistrationData>();

        modelBuilder.Entity<CompanyAssignedRole>(entity =>
        {
            entity.HasKey(e => new { e.CompanyId, e.CompanyRoleId });

            entity.HasOne(d => d.Company!)
                .WithMany(p => p.CompanyAssignedRoles)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.CompanyRole!)
                .WithMany(p => p.CompanyAssignedRoles)
                .HasForeignKey(d => d.CompanyRoleId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasAuditV1Triggers<CompanyAssignedRole, AuditCompanyAssignedRole2023316>();
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasAuditV1Triggers<UserRole, AuditUserRole20231115>();
        });

        modelBuilder.Entity<UserRoleCollection>(entity =>
        {
            entity.HasMany(p => p.UserRoles)
                .WithMany(p => p.UserRoleCollections)
                .UsingEntity<UserRoleAssignedCollection>(
                    j => j
                        .HasOne(d => d.UserRole)
                        .WithMany()
                        .HasForeignKey(d => d.UserRoleId)
                        .OnDelete(DeleteBehavior.ClientSetNull),
                    j => j
                        .HasOne(d => d.UserRoleCollection)
                        .WithMany()
                        .HasForeignKey(d => d.UserRoleCollectionId)
                        .OnDelete(DeleteBehavior.ClientSetNull),
                    j =>
                    {
                        j.HasKey(e => new { e.UserRoleId, e.UserRoleCollectionId });
                    });
        });

        modelBuilder.Entity<UserRoleCollectionDescription>(entity =>
        {
            entity.HasKey(e => new { e.UserRoleCollectionId, e.LanguageShortName });
        });

        modelBuilder.Entity<UserRoleDescription>().HasKey(e => new { e.UserRoleId, e.LanguageShortName });

        modelBuilder.Entity<CompanyStatus>()
            .HasData(
                Enum.GetValues(typeof(CompanyStatusId))
                    .Cast<CompanyStatusId>()
                    .Select(e => new CompanyStatus(e))
            );

        modelBuilder.Entity<TechnicalUserType>()
            .HasData(
                Enum.GetValues(typeof(TechnicalUserTypeId))
                    .Cast<TechnicalUserTypeId>()
                    .Select(e => new TechnicalUserType(e))
            );

        modelBuilder.Entity<TechnicalUserKind>()
            .HasData(
                Enum.GetValues(typeof(TechnicalUserKindId))
                    .Cast<TechnicalUserKindId>()
                    .Select(e => new TechnicalUserKind(e))
            );

        modelBuilder.Entity<IdentityType>()
            .HasData(
                Enum.GetValues(typeof(IdentityTypeId))
                    .Cast<IdentityTypeId>()
                    .Select(e => new IdentityType(e))
            );

        modelBuilder.Entity<Identity>(entity =>
        {
            entity.HasIndex(e => e.UserEntityId)
                .IsUnique();

            entity.HasOne(d => d.Company)
                .WithMany(p => p.Identities)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(e => e.IdentityStatus)
                .WithMany(e => e.Identities)
                .HasForeignKey(e => e.UserStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(e => e.IdentityType)
                .WithMany(e => e.Identities)
                .HasForeignKey(e => e.IdentityTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasAuditV1Triggers<Identity, AuditIdentity20231115>();
        });

        modelBuilder.Entity<CompanyUser>(entity =>
        {
            entity.HasOne(x => x.Identity)
                .WithOne(x => x.CompanyUser)
                .HasForeignKey<CompanyUser>(x => x.Id);

            entity.HasMany(p => p.Offers)
                .WithMany(p => p.CompanyUsers)
                .UsingEntity<CompanyUserAssignedAppFavourite>(
                    j => j
                        .HasOne(d => d.App)
                        .WithMany()
                        .HasForeignKey(d => d.AppId)
                        .OnDelete(DeleteBehavior.ClientSetNull),
                    j => j
                        .HasOne(d => d.CompanyUser)
                        .WithMany()
                        .HasForeignKey(d => d.CompanyUserId)
                        .OnDelete(DeleteBehavior.ClientSetNull),
                    j =>
                    {
                        j.HasKey(e => new { e.CompanyUserId, e.AppId });
                    });
            entity.HasMany(p => p.CompanyUserAssignedBusinessPartners)
                .WithOne(d => d.CompanyUser);

            entity.HasAuditV1Triggers<CompanyUser, AuditCompanyUser20230522>();

            entity.ToTable("company_users");
        });

        modelBuilder.Entity<CompanyUserAssignedProcess>()
            .HasKey(e => new { e.CompanyUserId, e.ProcessId });

        modelBuilder.Entity<TechnicalUser>(entity =>
        {
            entity.HasOne(x => x.Identity)
                .WithOne(x => x.TechnicalUser)
                .HasForeignKey<TechnicalUser>(x => x.Id);

            entity.HasOne(d => d.TechnicalUserType)
                .WithMany(p => p.TechnicalUsers)
                .HasForeignKey(d => d.TechnicalUserTypeId);

            entity.HasOne(d => d.TechnicalUserKind)
                .WithMany(p => p.TechnicalUsers)
                .HasForeignKey(d => d.TechnicalUserKindId);

            entity.HasOne(d => d.OfferSubscription)
                .WithMany(p => p.Technicalusers)
                .HasForeignKey(d => d.OfferSubscriptionId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasIndex(x => x.ClientClientId)
                .HasFilter("client_client_id is not null AND technical_user_kind_id = 1");

            entity.ToTable("technical_users");
        });

        modelBuilder.Entity<ExternalTechnicalUser>(entity =>
        {
            entity.HasOne(x => x.TechnicalUser)
                .WithOne(x => x.ExternalTechnicalUser)
                .HasForeignKey<ExternalTechnicalUser>(x => x.Id);

            entity.ToTable("external_technical_users");
        });

        modelBuilder.Entity<IdentityAssignedRole>(entity =>
        {
            entity.HasKey(e => new { e.IdentityId, e.UserRoleId });

            entity
                .HasOne(d => d.UserRole)
                .WithMany(e => e.IdentityAssignedRoles)
                .HasForeignKey(d => d.UserRoleId)
                .OnDelete(DeleteBehavior.ClientSetNull);
            entity
                .HasOne(d => d.Identity)
                .WithMany(e => e.IdentityAssignedRoles)
                .HasForeignKey(d => d.IdentityId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasAuditV1Triggers<IdentityAssignedRole, AuditIdentityAssignedRole20230522>();
        });

        modelBuilder.Entity<CompanyUserAssignedBusinessPartner>()
            .HasKey(e => new { e.CompanyUserId, e.BusinessPartnerNumber });

        modelBuilder.Entity<IdentityUserStatus>()
            .HasData(
                Enum.GetValues(typeof(UserStatusId))
                    .Cast<UserStatusId>()
                    .Select(e => new IdentityUserStatus(e))
            );

        modelBuilder.Entity<Consent>(entity =>
        {
            entity.HasOne(d => d.Agreement)
                .WithMany(p => p.Consents)
                .HasForeignKey(d => d.AgreementId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Company)
                .WithMany(p => p.Consents)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.CompanyUser)
                .WithMany(p => p.Consents)
                .HasForeignKey(d => d.CompanyUserId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.ConsentStatus)
                .WithMany(p => p.Consents)
                .HasForeignKey(d => d.ConsentStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasAuditV1Triggers<Consent, AuditConsent20231115>();
        });

        modelBuilder.Entity<ConsentAssignedOfferSubscription>(entity =>
        {
            entity.HasKey(e => new { e.ConsentId, e.OfferSubscriptionId });

            entity.HasOne(d => d.Consent)
                .WithMany(p => p.ConsentAssignedOfferSubscriptions)
                .HasForeignKey(d => d.ConsentId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.OfferSubscription)
                .WithMany(p => p.ConsentAssignedOfferSubscriptions)
                .HasForeignKey(d => d.OfferSubscriptionId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<ConsentStatus>()
            .HasData(
                Enum.GetValues(typeof(ConsentStatusId))
                    .Cast<ConsentStatusId>()
                    .Select(e => new ConsentStatus(e))
            );

        modelBuilder.Entity<CountryLongName>(entity =>
        {
            entity.HasKey(e => new { e.Alpha2Code, e.ShortName });

            entity.HasOne(k => k.Language)
                .WithMany(o => o.CountryLongNames)
                .HasForeignKey(k => k.ShortName)
                .OnDelete(DeleteBehavior.ClientSetNull);
            entity.HasOne(d => d.Country)
                .WithMany(k => k.CountryLongNames)
                .HasForeignKey(d => d.Alpha2Code)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Country>(entity =>
        {
            entity.Property(e => e.Alpha2Code)
                .IsFixedLength();

            entity.Property(e => e.Alpha3Code)
                .IsFixedLength();
        });

        modelBuilder.Entity<CountryAssignedIdentifier>(entity =>
        {
            entity.HasKey(e => new { e.CountryAlpha2Code, e.UniqueIdentifierId });

            entity.HasOne(d => d.Country)
                .WithMany(p => p.CountryAssignedIdentifiers)
                .HasForeignKey(d => d.CountryAlpha2Code)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.UniqueIdentifier)
                .WithMany(p => p.CountryAssignedIdentifiers)
                .HasForeignKey(d => d.UniqueIdentifierId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<DocumentType>()
            .HasData(
                Enum.GetValues(typeof(DocumentTypeId))
                    .Cast<DocumentTypeId>()
                    .Select(e => new DocumentType(e))
            );

        modelBuilder.Entity<MediaType>()
            .HasData(
                Enum.GetValues(typeof(MediaTypeId))
                    .Cast<MediaTypeId>()
                    .Select(e => new MediaType(e))
            );

        modelBuilder.Entity<DocumentStatus>()
            .HasData(
                Enum.GetValues(typeof(DocumentStatusId))
                    .Cast<DocumentStatusId>()
                    .Select(e => new DocumentStatus(e))
            );

        modelBuilder.Entity<IamClient>().HasIndex(e => e.ClientClientId).IsUnique();

        modelBuilder.Entity<IamIdentityProvider>()
            .HasOne(d => d.IdentityProvider)
                .WithOne(p => p.IamIdentityProvider!)
                .HasForeignKey<IamIdentityProvider>(d => d.IdentityProviderId)
                .OnDelete(DeleteBehavior.ClientSetNull);

        modelBuilder.Entity<IdentityProvider>(entity =>
        {
            entity.HasOne(x => x.IdentityProviderCategory)
                .WithMany(x => x.IdentityProviders)
                .HasForeignKey(x => x.IdentityProviderCategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(x => x.IdentityProviderType)
                .WithMany(x => x.IdentityProviders)
                .HasForeignKey(x => x.IdentityProviderTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(x => x.Owner)
                .WithMany(x => x.OwnedIdentityProviders)
                .HasForeignKey(x => x.OwnerId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<IdentityProviderAssignedProcess>()
            .HasKey(e => new { e.IdentityProviderId, e.ProcessId });

        modelBuilder.Entity<IdentityProviderType>()
            .HasData(
                Enum.GetValues(typeof(IdentityProviderTypeId))
                    .Cast<IdentityProviderTypeId>()
                    .Select(e => new IdentityProviderType(e))
            );

        modelBuilder.Entity<IdentityProviderCategory>()
            .HasData(
                Enum.GetValues(typeof(IdentityProviderCategoryId))
                    .Cast<IdentityProviderCategoryId>()
                    .Select(e => new IdentityProviderCategory(e))
            );

        modelBuilder.Entity<Invitation>(entity =>
        {
            entity.HasOne(d => d.CompanyUser)
                .WithMany(p => p.Invitations)
                .HasForeignKey(d => d.CompanyUserId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.InvitationStatus)
                .WithMany(p => p.Invitations)
                .HasForeignKey(d => d.InvitationStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<InvitationStatus>()
            .HasData(
                Enum.GetValues(typeof(InvitationStatusId))
                    .Cast<InvitationStatusId>()
                    .Select(e => new InvitationStatus(e))
            );

        modelBuilder.Entity<Connector>(entity =>
        {
            entity.HasOne(d => d.SelfDescriptionDocument)
                .WithOne(p => p.Connector!)
                .HasForeignKey<Connector>(d => d.SelfDescriptionDocumentId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Status)
                .WithMany(p => p.Connectors)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Type)
                .WithMany(p => p.Connectors)
                .HasForeignKey(d => d.TypeId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Provider)
                .WithMany(p => p.ProvidedConnectors);

            entity.HasOne(d => d.Host)
                .WithMany(p => p.HostedConnectors);

            entity.HasOne(d => d.TechnicalUser)
                .WithOne(p => p.Connector)
                .HasForeignKey<Connector>(d => d.TechnicalUserId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.SdCreationProcess)
                .WithOne(p => p.SdCreationConnector)
                .HasForeignKey<Connector>(d => d.SdCreationProcessId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasAuditV1Triggers<Connector, AuditConnector20250113>();
        });

        modelBuilder.Entity<ConnectorStatus>()
            .HasData(
                Enum.GetValues(typeof(ConnectorStatusId))
                    .Cast<ConnectorStatusId>()
                    .Select(e => new ConnectorStatus(e))
            );

        modelBuilder.Entity<ConnectorType>()
            .HasData(
                Enum.GetValues(typeof(ConnectorTypeId))
                    .Cast<ConnectorTypeId>()
                    .Select(e => new ConnectorType(e))
            );

        modelBuilder.Entity<Language>(entity =>
        {
            entity.Property(e => e.ShortName)
                .IsFixedLength();
        });

        modelBuilder.Entity<LanguageLongName>(entity =>
        {
            entity.HasKey(e => new { e.ShortName, e.LanguageShortName });
            entity.Property(e => e.ShortName)
                .IsFixedLength();
            entity.Property(e => e.LanguageShortName)
                .IsFixedLength();
            entity.HasOne(e => e.Language)
                .WithMany(e => e.LanguageLongNames)
                .HasForeignKey(e => e.ShortName)
                .OnDelete(DeleteBehavior.ClientSetNull);
            entity.HasOne(e => e.LongNameLanguage)
                .WithMany(e => e.LanguageLongNameLanguages)
                .HasForeignKey(e => e.LanguageShortName)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.Property(x => x.DueDate)
                .IsRequired(false);

            entity.HasOne(d => d.Receiver)
                .WithMany(p => p.Notifications)
                .HasForeignKey(d => d.ReceiverUserId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Creator)
                .WithMany(p => p.CreatedNotifications)
                .HasForeignKey(d => d.CreatorUserId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.NotificationType)
                .WithMany(p => p.Notifications)
                .HasForeignKey(d => d.NotificationTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<NotificationTopic>()
            .HasData(
                Enum.GetValues(typeof(NotificationTopicId))
                    .Cast<NotificationTopicId>()
                    .Select(e => new NotificationTopic(e))
            );

        modelBuilder.Entity<NotificationType>()
            .HasData(
                Enum.GetValues(typeof(NotificationTypeId))
                    .Cast<NotificationTypeId>()
                    .Select(e => new NotificationType(e))
            );

        modelBuilder.Entity<NotificationTypeAssignedTopic>(entity =>
        {
            entity.HasKey(e => new { e.NotificationTypeId, e.NotificationTopicId });

            entity.HasOne(d => d.NotificationTopic)
                .WithMany(x => x.NotificationTypeAssignedTopics)
                .HasForeignKey(d => d.NotificationTopicId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.NotificationType)
                .WithOne(x => x.NotificationTypeAssignedTopic)
                .HasForeignKey<NotificationTypeAssignedTopic>(d => d.NotificationTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<UseCase>();

        modelBuilder.Entity<UniqueIdentifier>()
            .HasData(
                Enum.GetValues(typeof(UniqueIdentifierId))
                    .Cast<UniqueIdentifierId>()
                    .Select(e => new UniqueIdentifier(e))
            );

        modelBuilder.Entity<OfferSubscriptionStatus>()
            .HasData(
                Enum.GetValues(typeof(OfferSubscriptionStatusId))
                    .Cast<OfferSubscriptionStatusId>()
                    .Select(e => new OfferSubscriptionStatus(e))
            );

        modelBuilder.Entity<CompanyIdentifier>(entity =>
        {
            entity.HasKey(e => new { e.CompanyId, e.UniqueIdentifierId });

            entity.HasOne(d => d.Company)
                .WithMany(p => p.CompanyIdentifiers)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.UniqueIdentifier)
                .WithMany(p => p.CompanyIdentifiers)
                .HasForeignKey(d => d.UniqueIdentifierId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<BpdmIdentifier>()
            .HasData(
                Enum.GetValues(typeof(BpdmIdentifierId))
                    .Cast<BpdmIdentifierId>()
                    .Select(e => new BpdmIdentifier(e))
            );

        modelBuilder.Entity<PrivacyPolicy>()
            .HasData(
                Enum.GetValues(typeof(PrivacyPolicyId))
                    .Cast<PrivacyPolicyId>()
                    .Select(e => new PrivacyPolicy(e))
            );

        modelBuilder.Entity<OfferAssignedPrivacyPolicy>(entity =>
        {
            entity.HasKey(e => new { e.OfferId, e.PrivacyPolicyId });

            entity.HasOne(d => d.Offer)
                .WithMany(p => p.OfferAssignedPrivacyPolicies)
                .HasForeignKey(d => d.OfferId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.PrivacyPolicy)
                .WithMany(p => p.OfferAssignedPrivacyPolicies)
                .HasForeignKey(d => d.PrivacyPolicyId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<LicenseType>()
            .HasData(
                Enum.GetValues(typeof(LicenseTypeId))
                    .Cast<LicenseTypeId>()
                    .Select(e => new LicenseType(e))
            );

        modelBuilder.Entity<ServiceDetail>(entity =>
        {
            entity.HasKey(e => new { e.ServiceId, e.ServiceTypeId });

            entity.HasOne(e => e.ServiceType)
                .WithMany(e => e.ServiceDetails)
                .HasForeignKey(e => e.ServiceTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(e => e.Service)
                .WithMany(e => e.ServiceDetails)
                .HasForeignKey(e => e.ServiceId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<TechnicalUserProfile>(entity =>
        {
            entity.HasOne(t => t.Offer)
                .WithMany(o => o.TechnicalUserProfiles)
                .HasForeignKey(t => t.OfferId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<TechnicalUserProfileAssignedUserRole>(entity =>
        {
            entity.HasKey(e => new { e.TechnicalUserProfileId, e.UserRoleId });

            entity.HasOne(d => d.TechnicalUserProfile)
                .WithMany(p => p.TechnicalUserProfileAssignedUserRoles)
                .HasForeignKey(d => d.TechnicalUserProfileId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.UserRole)
                .WithMany(p => p.TechnicalUserProfileAssignedUserRole)
                .HasForeignKey(d => d.UserRoleId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<UseCaseDescription>(entity =>
        {
            entity.HasKey(e => new { e.UseCaseId, e.LanguageShortName });

            entity.HasOne(d => d.UseCase)
                .WithMany(p => p.UseCaseDescriptions)
                .HasForeignKey(d => d.UseCaseId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Language)
                .WithMany(p => p.UseCases)
                .HasForeignKey(d => d.LanguageShortName)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<CompaniesLinkedTechnicalUser>()
            .ToView("company_linked_technical_users", "portal")
            .HasKey(x => x.TechnicalUserId);

        modelBuilder.Entity<ConnectorAssignedOfferSubscription>(entity =>
        {
            entity.HasKey(e => new { e.ConnectorId, e.OfferSubscriptionId });

            entity.HasOne(d => d.Connector)
                .WithMany(x => x.ConnectorAssignedOfferSubscriptions)
                .HasForeignKey(d => d.ConnectorId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.OfferSubscription)
                .WithMany(x => x.ConnectorAssignedOfferSubscriptions)
                .HasForeignKey(d => d.OfferSubscriptionId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<OfferSubscriptionView>()
            .ToView("offer_subscription_view", "portal")
            .HasNoKey();
        modelBuilder.Entity<CompanyUsersView>()
            .ToView("company_users_view", "portal")
            .HasNoKey();
        modelBuilder.Entity<CompanyIdpView>()
            .ToView("company_idp_view", "portal")
            .HasNoKey();
        modelBuilder.Entity<CompanyConnectorView>()
            .ToView("company_connector_view", "portal")
            .HasNoKey();
        modelBuilder.Entity<CompanyRoleCollectionRolesView>()
            .ToView("companyrole_collectionroles_view", "portal")
            .HasNoKey();
        modelBuilder.Entity<TechnicalUser>(entity =>
        {
            // the relationship to view company_linked_technical_users is defined to use the respective navigational property in LINQ.
            // when executing 'dotnet ef migrations add' ef will autocreate a fk-constraint. As this ain't work with views
            // the creation of this fk-constraint must be manually removed from the respective migration.
            // The navigational property will nevertheless work as it does not depend on the contraint.
            entity.HasOne(x => x.CompaniesLinkedTechnicalUser)
                  .WithOne(x => x.TechnicalUser)
                  .HasForeignKey<CompaniesLinkedTechnicalUser>(x => x.TechnicalUserId);
        });

        modelBuilder.Entity<OnboardingServiceProviderDetail>()
            .HasOne(x => x.Company)
            .WithOne(x => x.OnboardingServiceProviderDetail)
            .HasForeignKey<OnboardingServiceProviderDetail>(x => x.CompanyId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        modelBuilder.Entity<NetworkRegistration>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasIndex(x => new { x.ExternalId, x.OnboardingServiceProviderId })
                .IsUnique();

            entity.HasOne(x => x.Company)
                .WithOne(x => x.NetworkRegistration)
                .HasForeignKey<NetworkRegistration>(x => x.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(x => x.Process)
                .WithOne(x => x.NetworkRegistration)
                .HasForeignKey<NetworkRegistration>(x => x.ProcessId);

            entity.HasOne(x => x.OnboardingServiceProvider)
                .WithMany(x => x.OnboardedNetworkRegistrations)
                .HasForeignKey(x => x.OnboardingServiceProviderId);

            entity.HasOne(x => x.CompanyApplication)
                .WithOne(x => x.NetworkRegistration)
                .HasForeignKey<NetworkRegistration>(x => x.ApplicationId);
        });

        modelBuilder.Entity<CompanyUserAssignedIdentityProvider>()
            .HasKey(e => new { e.CompanyUserId, e.IdentityProviderId });

        modelBuilder.Entity<Document>()
            .HasAuditV1Triggers<Document, AuditDocument20241120>();

        modelBuilder.Entity<CompanyCertificate>()
            .HasAuditV1Triggers<CompanyCertificate, AuditCertificateManagement20240416>();

        modelBuilder.Entity<CompanyInvitation>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasOne(x => x.Application)
                .WithOne(x => x.CompanyInvitation)
                .HasForeignKey<CompanyInvitation>(x => x.ApplicationId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(x => x.Process)
                .WithOne(x => x.CompanyInvitation)
                .HasForeignKey<CompanyInvitation>(x => x.ProcessId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<AgreementStatus>()
            .HasData(
                Enum.GetValues(typeof(AgreementStatusId))
                    .Cast<AgreementStatusId>()
                    .Select(e => new AgreementStatus(e))
            );

        modelBuilder.Entity<AgreementView>()
            .ToView("agreement_view", "portal")
            .HasNoKey();

        modelBuilder.Entity<MailingStatus>()
            .HasData(
                Enum.GetValues(typeof(MailingStatusId))
                    .Cast<MailingStatusId>()
                    .Select(e => new MailingStatus(e))
            );

        modelBuilder.Entity<MailingInformation>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasOne(x => x.Process)
                .WithOne(x => x.MailingInformation)
                .HasForeignKey<MailingInformation>(x => x.ProcessId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(x => x.MailingStatus)
                .WithMany(x => x.MailingInformations)
                .HasForeignKey(x => x.MailingStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<ExternalTechnicalUserCreationData>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasOne(x => x.Process)
                .WithOne(x => x.ExternalTechnicalUserCreationData)
                .HasForeignKey<ExternalTechnicalUserCreationData>(x => x.ProcessId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(x => x.TechnicalUser)
                .WithOne(x => x.ExternalTechnicalUserCreationData)
                .HasForeignKey<ExternalTechnicalUserCreationData>(x => x.TechnicalUserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<AgreementDescription>(entity =>
        {
            entity.HasKey(e => new { e.AgreementId, e.LanguageShortName });

            entity.HasOne(d => d.Agreement)
                .WithMany(p => p.AgreementDescriptions)
                .HasForeignKey(d => d.AgreementId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Language)
                .WithMany(p => p.AgreementDescriptions)
                .HasForeignKey(d => d.LanguageShortName)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Address>()
            .Property(t => t.Region).HasDefaultValue("");
    }

    /// <inheritdoc />
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new())
    {
        EnhanceChangedEntries();
        return base.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        EnhanceChangedEntries();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override int SaveChanges()
    {
        EnhanceChangedEntries();
        return base.SaveChanges();
    }

    private void EnhanceChangedEntries()
    {
        _auditHandler.HandleAuditForChangedEntries(
            ChangeTracker.Entries().Where(entry =>
                entry.State != EntityState.Unchanged && entry.State != EntityState.Detached &&
                entry.Entity is IAuditableV1).ToImmutableList(),
            ChangeTracker.Context);
    }
}
