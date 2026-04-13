using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace RBALabs.Website.Services;

public sealed class UmbracoBootstrapService
{
    private const string HomePageDocumentTypeAlias = "homePage";
    private const string HomePageTemplateAlias = "homePage";

    private const string SiteTitleAlias = "siteTitle";
    private const string MetaDescriptionAlias = "metaDescription";
    private const string ContactEmailAlias = "contactEmail";

    private readonly IRuntimeState _runtimeState;
    private readonly IDataTypeService _dataTypeService;
    private readonly IContentTypeService _contentTypeService;
    private readonly ITemplateService _templateService;
    private readonly IContentService _contentService;
    private readonly IShortStringHelper _shortStringHelper;
    private readonly ILogger<UmbracoBootstrapService> _logger;

    public UmbracoBootstrapService(
        IRuntimeState runtimeState,
        IDataTypeService dataTypeService,
        IContentTypeService contentTypeService,
        ITemplateService templateService,
        IContentService contentService,
        IShortStringHelper shortStringHelper,
        ILogger<UmbracoBootstrapService> logger)
    {
        _runtimeState = runtimeState;
        _dataTypeService = dataTypeService;
        _contentTypeService = contentTypeService;
        _templateService = templateService;
        _contentService = contentService;
        _shortStringHelper = shortStringHelper;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        if (_runtimeState.Level != RuntimeLevel.Run)
        {
            _logger.LogInformation("Skipping Umbraco bootstrap because runtime level is {RuntimeLevel}", _runtimeState.Level);
            return;
        }

        var lineDataType = await EnsureDataTypeAsync("RBA Text Line", Constants.DataTypes.Guids.TextstringGuid);
        var paragraphDataType = await EnsureDataTypeAsync("RBA Paragraph", Constants.DataTypes.Guids.TextareaGuid);

        IContentType homePageContentType = await EnsureHomePageContentTypeAsync(lineDataType, paragraphDataType);
        await EnsureHomePageTemplateAsync(homePageContentType);
        EnsureHomePageContent();
    }

    private async Task<IDataType> EnsureDataTypeAsync(string dataTypeName, Guid sourceDataTypeKey)
    {
        IDataType? existing = (await _dataTypeService.GetAllAsync(Array.Empty<Guid>()))
            .FirstOrDefault(x => string.Equals(x.Name, dataTypeName, StringComparison.OrdinalIgnoreCase));

        if (existing is not null)
        {
            return existing;
        }

        IDataType sourceDataType = await _dataTypeService.GetAsync(sourceDataTypeKey)
            ?? throw new InvalidOperationException($"Base data type '{sourceDataTypeKey}' was not found.");

        var copyAttempt = await _dataTypeService.CopyAsync(sourceDataType, null, Constants.Security.SuperUserKey);
        if (!copyAttempt.Success || copyAttempt.Result is null)
        {
            throw new InvalidOperationException($"Failed to create data type '{dataTypeName}' from source '{sourceDataType.Name}'.");
        }

        IDataType createdDataType = copyAttempt.Result;
        createdDataType.Name = dataTypeName;

        var updateAttempt = await _dataTypeService.UpdateAsync(createdDataType, Constants.Security.SuperUserKey);
        if (!updateAttempt.Success)
        {
            throw new InvalidOperationException($"Failed to save data type '{dataTypeName}'.");
        }

        _logger.LogInformation("Created data type '{DataTypeName}'", dataTypeName);
        return createdDataType;
    }

    private async Task<IContentType> EnsureHomePageContentTypeAsync(IDataType lineDataType, IDataType paragraphDataType)
    {
        IContentType? contentType = _contentTypeService.Get(HomePageDocumentTypeAlias);
        var isNew = contentType is null;

        if (isNew)
        {
            contentType = new ContentType(_shortStringHelper, Constants.System.Root)
            {
                Alias = HomePageDocumentTypeAlias,
                Name = "Home Page",
                Description = "RBA Labs landing page",
                Icon = "icon-home color-red",
                AllowedAsRoot = true,
            };
        }

        var changed = isNew;

        changed |= EnsureTextProperty(contentType!, lineDataType, SiteTitleAlias, "Site Title", "general", "General", mandatory: true);
        changed |= EnsureTextProperty(contentType!, paragraphDataType, MetaDescriptionAlias, "Meta Description", "general", "General");
        changed |= EnsureTextProperty(contentType!, lineDataType, ContactEmailAlias, "Contact Email", "general", "General");

        changed |= EnsureTextProperty(contentType!, lineDataType, "heroKicker", "Hero Kicker", "hero", "Hero");
        changed |= EnsureTextProperty(contentType!, lineDataType, "heroHeadingPrefix", "Hero Heading Prefix", "hero", "Hero");
        changed |= EnsureTextProperty(contentType!, lineDataType, "heroHeadingAccent", "Hero Heading Accent", "hero", "Hero");
        changed |= EnsureTextProperty(contentType!, lineDataType, "heroHeadingSuffix", "Hero Heading Suffix", "hero", "Hero");
        changed |= EnsureTextProperty(contentType!, paragraphDataType, "heroLead", "Hero Lead", "hero", "Hero");
        changed |= EnsureTextProperty(contentType!, lineDataType, "heroPrimaryButtonText", "Hero Primary Button Text", "hero", "Hero");
        changed |= EnsureTextProperty(contentType!, lineDataType, "heroPrimaryButtonUrl", "Hero Primary Button Url", "hero", "Hero");
        changed |= EnsureTextProperty(contentType!, lineDataType, "heroSecondaryButtonText", "Hero Secondary Button Text", "hero", "Hero");
        changed |= EnsureTextProperty(contentType!, lineDataType, "heroSecondaryButtonUrl", "Hero Secondary Button Url", "hero", "Hero");
        changed |= EnsureTextProperty(contentType!, lineDataType, "heroMetric1Value", "Hero Metric 1 Value", "hero", "Hero");
        changed |= EnsureTextProperty(contentType!, lineDataType, "heroMetric1Text", "Hero Metric 1 Text", "hero", "Hero");
        changed |= EnsureTextProperty(contentType!, lineDataType, "heroMetric2Value", "Hero Metric 2 Value", "hero", "Hero");
        changed |= EnsureTextProperty(contentType!, lineDataType, "heroMetric2Text", "Hero Metric 2 Text", "hero", "Hero");
        changed |= EnsureTextProperty(contentType!, lineDataType, "heroMetric3Value", "Hero Metric 3 Value", "hero", "Hero");
        changed |= EnsureTextProperty(contentType!, lineDataType, "heroMetric3Text", "Hero Metric 3 Text", "hero", "Hero");

        changed |= EnsureTextProperty(contentType!, lineDataType, "servicesTitle", "Services Title", "services", "Services");
        changed |= EnsureTextProperty(contentType!, paragraphDataType, "servicesLead", "Services Lead", "services", "Services");
        changed |= EnsureTextProperty(contentType!, lineDataType, "service1Title", "Service 1 Title", "services", "Services");
        changed |= EnsureTextProperty(contentType!, paragraphDataType, "service1Text", "Service 1 Text", "services", "Services");
        changed |= EnsureTextProperty(contentType!, lineDataType, "service2Title", "Service 2 Title", "services", "Services");
        changed |= EnsureTextProperty(contentType!, paragraphDataType, "service2Text", "Service 2 Text", "services", "Services");
        changed |= EnsureTextProperty(contentType!, lineDataType, "service3Title", "Service 3 Title", "services", "Services");
        changed |= EnsureTextProperty(contentType!, paragraphDataType, "service3Text", "Service 3 Text", "services", "Services");
        changed |= EnsureTextProperty(contentType!, lineDataType, "service4Title", "Service 4 Title", "services", "Services");
        changed |= EnsureTextProperty(contentType!, paragraphDataType, "service4Text", "Service 4 Text", "services", "Services");

        changed |= EnsureTextProperty(contentType!, lineDataType, "projectsTitle", "Projects Title", "projects", "Projects");
        changed |= EnsureTextProperty(contentType!, lineDataType, "project1Title", "Project 1 Title", "projects", "Projects");
        changed |= EnsureTextProperty(contentType!, paragraphDataType, "project1Text", "Project 1 Text", "projects", "Projects");
        changed |= EnsureTextProperty(contentType!, lineDataType, "project2Title", "Project 2 Title", "projects", "Projects");
        changed |= EnsureTextProperty(contentType!, paragraphDataType, "project2Text", "Project 2 Text", "projects", "Projects");
        changed |= EnsureTextProperty(contentType!, lineDataType, "project3Title", "Project 3 Title", "projects", "Projects");
        changed |= EnsureTextProperty(contentType!, paragraphDataType, "project3Text", "Project 3 Text", "projects", "Projects");

        changed |= EnsureTextProperty(contentType!, lineDataType, "trustTitle", "Trust Title", "trust", "Trust and CTA");
        changed |= EnsureTextProperty(contentType!, lineDataType, "trustStat1Value", "Trust Stat 1 Value", "trust", "Trust and CTA");
        changed |= EnsureTextProperty(contentType!, lineDataType, "trustStat1Label", "Trust Stat 1 Label", "trust", "Trust and CTA");
        changed |= EnsureTextProperty(contentType!, lineDataType, "trustStat2Value", "Trust Stat 2 Value", "trust", "Trust and CTA");
        changed |= EnsureTextProperty(contentType!, lineDataType, "trustStat2Label", "Trust Stat 2 Label", "trust", "Trust and CTA");
        changed |= EnsureTextProperty(contentType!, lineDataType, "trustStat3Value", "Trust Stat 3 Value", "trust", "Trust and CTA");
        changed |= EnsureTextProperty(contentType!, lineDataType, "trustStat3Label", "Trust Stat 3 Label", "trust", "Trust and CTA");
        changed |= EnsureTextProperty(contentType!, lineDataType, "trustStat4Value", "Trust Stat 4 Value", "trust", "Trust and CTA");
        changed |= EnsureTextProperty(contentType!, lineDataType, "trustStat4Label", "Trust Stat 4 Label", "trust", "Trust and CTA");
        changed |= EnsureTextProperty(contentType!, lineDataType, "ctaTitle", "CTA Title", "trust", "Trust and CTA");
        changed |= EnsureTextProperty(contentType!, paragraphDataType, "ctaText", "CTA Text", "trust", "Trust and CTA");
        changed |= EnsureTextProperty(contentType!, lineDataType, "ctaButtonText", "CTA Button Text", "trust", "Trust and CTA");
        changed |= EnsureTextProperty(contentType!, lineDataType, "ctaButtonUrl", "CTA Button Url", "trust", "Trust and CTA");

        changed |= EnsureTextProperty(contentType!, paragraphDataType, "footerLead", "Footer Lead", "footer", "Footer");
        changed |= EnsureTextProperty(contentType!, lineDataType, "footerPhone", "Footer Phone", "footer", "Footer");
        changed |= EnsureTextProperty(contentType!, lineDataType, "footerPhoneLink", "Footer Phone Link", "footer", "Footer");
        changed |= EnsureTextProperty(contentType!, lineDataType, "footerLinkedInUrl", "Footer LinkedIn Url", "footer", "Footer");

        if (isNew)
        {
            var createAttempt = await _contentTypeService.CreateAsync(contentType!, Constants.Security.SuperUserKey);
            if (!createAttempt.Success)
            {
                throw new InvalidOperationException("Failed to create document type 'Home Page'.");
            }

            _logger.LogInformation("Created document type '{DocumentTypeAlias}'", HomePageDocumentTypeAlias);
        }
        else if (changed)
        {
            var updateAttempt = await _contentTypeService.UpdateAsync(contentType!, Constants.Security.SuperUserKey);
            if (!updateAttempt.Success)
            {
                throw new InvalidOperationException("Failed to update document type 'Home Page'.");
            }

            _logger.LogInformation("Updated document type '{DocumentTypeAlias}'", HomePageDocumentTypeAlias);
        }

        return _contentTypeService.Get(HomePageDocumentTypeAlias)
            ?? throw new InvalidOperationException("Home page document type was not found after seed.");
    }

    private bool EnsureTextProperty(
        IContentType contentType,
        IDataType dataType,
        string alias,
        string name,
        string tabAlias,
        string tabName,
        bool mandatory = false)
    {
        if (contentType.PropertyTypeExists(alias))
        {
            return false;
        }

        contentType.AddPropertyType(
            new PropertyType(_shortStringHelper, dataType, alias)
            {
                Name = name,
                Mandatory = mandatory,
            },
            tabAlias,
            tabName);

        return true;
    }

    private async Task EnsureHomePageTemplateAsync(IContentType homePageContentType)
    {
        ITemplate? template = await _templateService.GetAsync(HomePageTemplateAlias);

        if (template is null)
        {
            var createTemplateAttempt = await _contentTypeService.CreateTemplateAsync(
                homePageContentType.Key,
                "Home Page",
                HomePageTemplateAlias,
                true,
                Constants.Security.SuperUserKey);

            if (!createTemplateAttempt.Success)
            {
                throw new InvalidOperationException("Failed to create template 'homePage'.");
            }

            template = await _templateService.GetAsync(HomePageTemplateAlias);
        }

        if (template is null)
        {
            throw new InvalidOperationException("Template 'homePage' could not be loaded.");
        }

        if (homePageContentType.DefaultTemplateId != template.Id)
        {
            homePageContentType.SetDefaultTemplate(template);
            var updateAttempt = await _contentTypeService.UpdateAsync(homePageContentType, Constants.Security.SuperUserKey);
            if (!updateAttempt.Success)
            {
                throw new InvalidOperationException("Failed to assign default template to 'homePage' document type.");
            }
        }
    }

    private void EnsureHomePageContent()
    {
        IContent? homePage = _contentService
            .GetRootContent()
            .FirstOrDefault(x => x.ContentType.Alias == HomePageDocumentTypeAlias);

        var isNew = false;
        if (homePage is null)
        {
            homePage = _contentService.Create("Home", Constants.System.Root, HomePageDocumentTypeAlias, -1);
            isNew = true;
        }

        var defaults = new Dictionary<string, string>
        {
            [SiteTitleAlias] = "RBA Labs | Senior Umbraco and .NET Engineering",
            [MetaDescriptionAlias] = "Senior Umbraco and .NET engineering for agencies, product teams, and enterprise platforms.",
            [ContactEmailAlias] = "hello@rbalabs.com",

            ["heroKicker"] = "Senior Umbraco and .NET experts",
            ["heroHeadingPrefix"] = "Senior .NET and",
            ["heroHeadingAccent"] = "Umbraco engineering",
            ["heroHeadingSuffix"] = "for complex web platforms",
            ["heroLead"] = "We build integration-heavy digital systems for agencies, enterprise teams, and product companies that need reliable senior-level delivery.",
            ["heroPrimaryButtonText"] = "Get in Touch",
            ["heroPrimaryButtonUrl"] = "#contact",
            ["heroSecondaryButtonText"] = "View Projects",
            ["heroSecondaryButtonUrl"] = "#projects",
            ["heroMetric1Value"] = "15+",
            ["heroMetric1Text"] = "Years in Umbraco and .NET delivery",
            ["heroMetric2Value"] = "80+",
            ["heroMetric2Text"] = "Projects across multiple markets",
            ["heroMetric3Value"] = "24/7",
            ["heroMetric3Text"] = "Operational support and performance care",

            ["servicesTitle"] = "What We Do",
            ["servicesLead"] = "From platform strategy to delivery and support, we cover the full lifecycle of enterprise Umbraco and .NET systems.",
            ["service1Title"] = "Umbraco Development",
            ["service1Text"] = "Custom Umbraco builds with clean content architecture, editor-friendly backoffice setup, and strong long-term maintainability.",
            ["service2Title"] = ".NET Backend and APIs",
            ["service2Text"] = "Robust backend services, event-driven workflows, and API-first integrations designed for performance and resilience.",
            ["service3Title"] = "Platform Modernization",
            ["service3Text"] = "Incremental upgrades from legacy stacks to modern architecture with lower risk and clear migration roadmaps.",
            ["service4Title"] = "Performance and Support",
            ["service4Text"] = "Core Web Vitals optimization, infrastructure tuning, release support, and stable operations for high-load products.",

            ["projectsTitle"] = "Featured Projects",
            ["project1Title"] = "IMMO / DKV",
            ["project1Text"] = "Enterprise-grade real estate platform with structured content, multilingual workflows, and API integrations.",
            ["project2Title"] = "Ronson",
            ["project2Text"] = "High-performance content platform with optimized editorial UX and scalable infrastructure setup.",
            ["project3Title"] = "GameDriver",
            ["project3Text"] = "Integration-heavy ecosystem focused on automation workflows, data exchange, and stable release cadence.",

            ["trustTitle"] = "Why Clients Choose RBA Labs",
            ["trustStat1Value"] = "15+",
            ["trustStat1Label"] = "Years of services delivery",
            ["trustStat2Value"] = "80+",
            ["trustStat2Label"] = "Projects launched and supported",
            ["trustStat3Value"] = "100%",
            ["trustStat3Label"] = "Senior engineering ownership",
            ["trustStat4Value"] = "24/7",
            ["trustStat4Label"] = "Performance and support coverage",
            ["ctaTitle"] = "Need a senior partner for your complex platform?",
            ["ctaText"] = "Let us design, build, and scale it with you.",
            ["ctaButtonText"] = "Start a Project",
            ["ctaButtonUrl"] = "#contact",

            ["footerLead"] = "Senior Umbraco and .NET engineering partner for teams shipping mission-critical digital products.",
            ["footerPhone"] = "+48 909 999 909",
            ["footerPhoneLink"] = "+48909999909",
            ["footerLinkedInUrl"] = "https://www.linkedin.com",
        };

        var changed = isNew;
        foreach ((string alias, string value) in defaults)
        {
            changed |= SetValueIfEmpty(homePage, alias, value);
        }

        if (!changed)
        {
            return;
        }

        var saveAttempt = _contentService.Save(homePage, -1);
        if (!saveAttempt.Success)
        {
            throw new InvalidOperationException("Failed to save Home page content.");
        }

        var publishAttempt = _contentService.Publish(homePage, Array.Empty<string>(), -1);
        if (!publishAttempt.Success)
        {
            throw new InvalidOperationException("Failed to publish Home page content.");
        }

        _logger.LogInformation("Home page content has been {Action}", isNew ? "created" : "updated");
    }

    private static bool SetValueIfEmpty(IContent content, string alias, string value)
    {
        var currentValue = content.GetValue(alias)?.ToString();
        if (string.IsNullOrWhiteSpace(currentValue))
        {
            content.SetValue(alias, value);
            return true;
        }

        return false;
    }
}
