﻿// Credit to Pathoschild for the code on which this functionality was based.

using Newtonsoft.Json.Linq;
using StardewModdingAPI;
using StardewValley;
using System;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;

namespace StardewValleyMods.Common
{
    class UpdateNotifier
    {
        private readonly IMonitor Monitor;

        public UpdateNotifier(IMonitor monitor)
        {
            Monitor = monitor;
        }

        public async void Check(string modName, ISemanticVersion currentVersion)
        {
            try
            {
                var latestVersion = await GetCurrentVersion(modName).ConfigureAwait(false);

                if (latestVersion.IsNewerThan(currentVersion))
                    NotifyNewVersion(modName);
                else
                    NotifyUpToDate(modName);
            }
            catch (Exception e)
            {
                NotifyFailure(modName, e.Message);
            }
        }

        private void NotifyNewVersion(string modName)
        {
            var message = $"A new version of {modName} is available!";
            Monitor.Log(message, LogLevel.Alert);
            SaveEvents.AfterLoad += (sender, e) => ShowHudMessage(message);
        }

        private void NotifyUpToDate(string modName)
        {
            Monitor.Log($"{modName} is up to date.", LogLevel.Info);
        }

        private void NotifyFailure(string modName, string reason)
        {
            Monitor.Log($"Update check failed for {modName}. Please check for a new version manually. ({reason})", LogLevel.Warn);
            SaveEvents.AfterLoad += (sender, e) => ShowHudMessage($"Update check failed for {modName}");
        }

        private void ShowHudMessage(string message)
        {
            Game1.addHUDMessage(new HUDMessage(message, Color.Red, 3500f) {noIcon = true, timeLeft = HUDMessage.defaultTime});
        }

        private async Task<ISemanticVersion> GetCurrentVersion(string modName)
        {
            var manifestUrl = GetManifestUrl(modName);

            HttpWebRequest request = WebRequest.CreateHttp(manifestUrl);
            request.CachePolicy = new RequestCachePolicy(RequestCacheLevel.Revalidate);
            AssemblyName assembly = typeof(UpdateNotifier).Assembly.GetName();
            request.UserAgent = $"{assembly.Name}/{assembly.Version}";

            // fetch data
            using (var response = await request.GetResponseAsync())
            using (var responseStream = response.GetResponseStream())
            using (var reader = new StreamReader(responseStream))
            {
                string responseText = reader.ReadToEnd();

                var manifestData = JObject.Parse(responseText);
                var versionToken = manifestData.SelectToken("Version");
                return new SemanticVersion(
                    versionToken.Value<int>("Major"),
                    versionToken.Value<int>("Minor"),
                    versionToken.Value<int>("Patch")
                );
            }
        }

        private string GetManifestUrl(string modName)
        {
            return $"https://raw.githubusercontent.com/doncollins/StardewValleyMods/stable/{modName}/manifest.json";
        }
    }
}
