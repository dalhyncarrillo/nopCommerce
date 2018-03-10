﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Plugin.Widgets.GoogleAnalytics.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Widgets.GoogleAnalytics.Controllers
{
    [Area(AreaNames.Admin)]
    [AuthorizeAdmin]
    [AdminAntiForgery]
    public class WidgetsGoogleAnalyticsController : BasePluginController
    {
        #region Fields

        private readonly IWorkContext _workContext;
        private readonly IStoreService _storeService;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;

        #endregion

        #region Ctor

        public WidgetsGoogleAnalyticsController(IWorkContext workContext,
            IStoreContext storeContext, 
            IStoreService storeService,
            ISettingService settingService, 
            ILocalizationService localizationService,
            IPermissionService permissionService)
        {
            this._workContext = workContext;
            this._storeService = storeService;
            this._settingService = settingService;
            this._localizationService = localizationService;
            this._permissionService = permissionService;
        }

        #endregion

        #region Methods
        
        public IActionResult Configure()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageWidgets))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var googleAnalyticsSettings = _settingService.LoadSetting<GoogleAnalyticsSettings>(storeScope);

            var model = new ConfigurationModel
            {
                GoogleId = googleAnalyticsSettings.GoogleId,
                TrackingScript = googleAnalyticsSettings.TrackingScript,
                EnableEcommerce = googleAnalyticsSettings.EnableEcommerce,
                IncludingTax = googleAnalyticsSettings.IncludingTax,
                ZoneId = googleAnalyticsSettings.WidgetZone,
                IncludeCustomerId = googleAnalyticsSettings.IncludeCustomerId,
                ActiveStoreScopeConfiguration = storeScope
            };

            model.AvailableZones.Add(new SelectListItem() { Text = "Before body end html tag", Value = "body_end_html_tag_before" });
            model.AvailableZones.Add(new SelectListItem() { Text = "Head html tag", Value = "head_html_tag" });
            
            if (storeScope > 0)
            {
                model.GoogleId_OverrideForStore = _settingService.SettingExists(googleAnalyticsSettings, x => x.GoogleId, storeScope);
                model.TrackingScript_OverrideForStore = _settingService.SettingExists(googleAnalyticsSettings, x => x.TrackingScript, storeScope);
                model.EnableEcommerce_OverrideForStore = _settingService.SettingExists(googleAnalyticsSettings, x => x.EnableEcommerce, storeScope);
                model.IncludingTax_OverrideForStore = _settingService.SettingExists(googleAnalyticsSettings, x => x.IncludingTax, storeScope);
                model.ZoneId_OverrideForStore = _settingService.SettingExists(googleAnalyticsSettings, x => x.WidgetZone, storeScope);
                model.IncludeCustomerId_OverrideForStore = _settingService.SettingExists(googleAnalyticsSettings, x => x.IncludeCustomerId, storeScope);
            }

            return View("~/Plugins/Widgets.GoogleAnalytics/Views/Configure.cshtml", model);
        }

        [HttpPost]
        public IActionResult Configure(ConfigurationModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageWidgets))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var googleAnalyticsSettings = _settingService.LoadSetting<GoogleAnalyticsSettings>(storeScope);

            googleAnalyticsSettings.GoogleId = model.GoogleId;
            googleAnalyticsSettings.TrackingScript = model.TrackingScript;
            googleAnalyticsSettings.EnableEcommerce = model.EnableEcommerce;
            googleAnalyticsSettings.IncludingTax = model.IncludingTax;
            googleAnalyticsSettings.WidgetZone = model.ZoneId;
            googleAnalyticsSettings.IncludeCustomerId = model.IncludeCustomerId;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            _settingService.SaveSettingOverridablePerStore(googleAnalyticsSettings, x => x.GoogleId, model.GoogleId_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(googleAnalyticsSettings, x => x.TrackingScript, model.TrackingScript_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(googleAnalyticsSettings, x => x.EnableEcommerce, model.EnableEcommerce_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(googleAnalyticsSettings, x => x.IncludingTax, model.IncludingTax_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(googleAnalyticsSettings, x => x.WidgetZone, model.ZoneId_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(googleAnalyticsSettings, x => x.IncludeCustomerId, model.IncludeCustomerId_OverrideForStore, storeScope, false);

            //now clear settings cache
            _settingService.ClearCache();

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        #endregion
    }
}