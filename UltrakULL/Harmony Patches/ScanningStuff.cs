﻿using HarmonyLib;
using UltrakULL.json;
using UnityEngine;
using static UltrakULL.CommonFunctions;
using UnityEngine.UI;

namespace UltrakULL.Harmony_Patches
{
    //@Override
    //Overrides ScanBook from the ScanningStuff class, for the "scanning" panel and book translations.
    [HarmonyPatch(typeof(ScanningStuff), "ScanBook")]
    public static class LocalizeScanningText
    {
        [HarmonyPrefix]
        public static bool ScanBook_MyPatch(ref string text, bool noScan, int instanceId, ScanningStuff __instance)
        {
            GameObject canvas = GetInactiveRootObject("Canvas");

            Text scanningText = GetTextfromGameObject(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(canvas, "ScanningStuff"), "ScanningPanel"), "Text"));
            scanningText.text = LanguageManager.CurrentLanguage.books.books_scanning;
            text = Books.GetBookText(text);
            return true;
        }
    }
}
