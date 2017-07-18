﻿using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace StardewValleyMods.CategorizeChests.Framework.Persistence.Legacy
{
    class Version102Converter
    {
        public JToken Convert(JToken data)
        {
            if (!(data is JArray jArray))
                throw new InvalidSaveDataException("Expected version <1.1.0 save data to be an array");

            try
            {
                return JObject.FromObject(new
                {
                    Version = "1.0.2",
                    ChestEntries = TranslateEntries(jArray)
                });
            }
            catch (Exception ex) when (ex is JsonException || ex is InvalidCastException)
            {
                throw new InvalidSaveDataException("Malformed save data structure", ex);
            }
        }

        private JArray TranslateEntries(JArray entryList)
        {
            if (!entryList.Any())
                return entryList;

            return JArray.FromObject(
                entryList.Children().Select(child => TranslateEntry((JObject) child))
            );
        }

        private JObject TranslateEntry(JObject entry)
        {
            var address = JObject.FromObject(new
            {
                LocationType = entry.SelectToken("LocationType").ToObject<ChestLocationType>(),
                LocationName = entry.Value<string>("LocationName"),
                BuildingName = entry.Value<string>("BuildingName"),
                Tile = entry.SelectToken("Tile").ToObject<Vector2>(),
            });

            return JObject.FromObject(new
            {
                Address = address,
                AcceptedItemKinds = TranslateItemKeys((JArray) entry.SelectToken("AcceptedItemKinds"))
            });
        }

        private JArray TranslateItemKeys(JArray kindList)
        {
            if (!kindList.Any())
                return kindList;

            return JArray.FromObject(
                kindList.Children().Select(child => TranslateItemKey((JObject) child))
            );
        }

        private JObject TranslateItemKey(JObject itemKey)
        {
            return JObject.FromObject(new
            {
                ItemType = nameof(ItemType.Object),
                ObjectIndex = itemKey.Value<string>("ObjectIndex"),
            });
        }
    }
}