﻿using BepInEx.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;

namespace UltrakULL.json
{
    public static class LanguageManager
    {
        public static Dictionary<string, JsonFormat> AllLanguages = new Dictionary<string, JsonFormat>();
        public static JsonFormat CurrentLanguage { get; private set; } = null;
        private static BepInEx.Logging.ManualLogSource JsonLogger = BepInEx.Logging.Logger.CreateLogSource("LanguageManager");

        public static void InitializeManager(string modVersion)
        {
            LoadLanguages(modVersion);

            ConfigFile cfg = new ConfigFile("BepInEx\\config\\ultrakull\\lastLang.cfg", true);

            string value = cfg.Bind("General", "LastLanguage", "EN-en").Value;
            if (AllLanguages.ContainsKey(value))
            {
                JsonLogger.Log(BepInEx.Logging.LogLevel.Message, "Setting language to " + value);
                CurrentLanguage = AllLanguages[value];
            }
            else
                JsonLogger.Log(BepInEx.Logging.LogLevel.Message, "No last language found, value was " + value);
        }

        public static void DumpLastLanguage()
        {
            ConfigFile cfg = new ConfigFile("BepInEx\\config\\ultrakull\\lastLang.cfg", true);
            cfg.Bind("General", "LastLanguage", "English.json").Value = CurrentLanguage.metadata.langName; // Thank you copilot
        }

        public static void LoadLanguages(string modVersion)
        {
            AllLanguages = new Dictionary<string, JsonFormat>();
            string[] files = Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory() + "\\BepInEx\\config\\", "UltrakULL"), "*.json");
            foreach (string file in files)
            {
                if (TryLoadLang(file, out JsonFormat lang) && !AllLanguages.ContainsKey(lang.metadata.langName) && lang.metadata.langName != "te-mp")
                {
                    AllLanguages.Add(lang.metadata.langName, lang);
                    if (!ValidateFile(lang, modVersion))
                        JsonLogger.Log(BepInEx.Logging.LogLevel.Debug ,"Failed to validate " + lang.metadata.langName + " however I don't really care");
                }
            }
        }

        private static bool TryLoadLang(string pathName, out JsonFormat file)
        {
            file = null;
            try
            {
                string jsonFile = File.ReadAllText(pathName);
                file = JsonConvert.DeserializeObject<JsonFormat>(jsonFile);
                return true;
            }
            catch (Exception e)
            {
                JsonLogger.LogError("Failed to load language file " + pathName + ": " + e.Message);
                return false;
            }
        }

        private static bool ValidateFile(JsonFormat language, string modVersion)
        {
            JsonLogger.LogInfo("Opening file: " + language.metadata.langName + "...");
            try
            {
                //Following conditions to validate a file:
                //Must be JSON-deserializable
                //Must have a metadata attribute and a body attribute
                //Version logged in the JSON file must match or be newer than the current mod version
                //Will need to implement further sanity checks.
                JsonLogger.LogInfo("Checking version...");

                if (!FileMatchesMinimumRequiredVersion(language.metadata.minimumModVersion, modVersion))
                {
                    JsonLogger.LogError(language.metadata.langName + " does not match the version required by the mod. Skipping.");
                    JsonLogger.LogError("(If you wish to force load this file, you may do so via manually editing lastLang.cfg and setting the language to this filename. Expect in-game errors if you do this, you have been warned.)");
                    return false;
                }

                JsonLogger.LogInfo("Checking contents...");
                if (language.metadata != null && language.body != null)
                {
                    JsonLogger.LogMessage("File " + language.metadata.langName + " validated.");
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                JsonLogger.LogError("An error occured while validating. It's possible the language file is not correctly formatted in .json.\n"
                    + "Please use https://jsonlint.com/ to make sure your .json file is correctly formatted!");
                Console.WriteLine(e.ToString());
                return false;
            }
        }

        public static bool FileMatchesMinimumRequiredVersion(string requiredModVersion, string actualModVersion)
        {
            if (requiredModVersion == "")
            {
                JsonLogger.LogError("Language file has not defined the minimum mod version required!");
                return false;
            }

            Version jsonVersion = new Version(requiredModVersion);
            Version ultrakullVersion = new Version(actualModVersion);
            int isCompatible = jsonVersion.CompareTo(ultrakullVersion);

            //JSON version is greater or matches mod version
            if (jsonVersion == ultrakullVersion || isCompatible > 0)
            {
                JsonLogger.LogMessage("Matches current mod version.");
                return true;
            }
            //JSON version is lower than mod version
            else
            {
                return false;
            }
        }

        public static void SetCurrentLanguage(string langName)
        {
            if (AllLanguages.ContainsKey(langName))
            {
                CurrentLanguage = AllLanguages[langName];
                JsonLogger.Log(BepInEx.Logging.LogLevel.Message, "Setting language to " + langName);
                MainPatch.instance.onSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single); 
            }
            else
                JsonLogger.Log(BepInEx.Logging.LogLevel.Message, "No language found with name " + langName);
        }
    }
}
