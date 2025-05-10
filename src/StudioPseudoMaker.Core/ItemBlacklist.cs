using ChaCustom;
using Manager;
using PseudoMaker.UI;
using Sideloader.AutoResolver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using UnityEngine.EventSystems;

// Heavily copied from the ItemBlacklist plugin itself to keep compatibility with it
// Done this way because the plugin doesn't load in Studio
// So I had to adapt the code to work in studio within Pseudo Maker
// https://github.com/IllusionMods/KK_Plugins/blob/master/src/ItemBlacklist.Core/ItemBlacklist.cs
namespace PseudoMaker
{
    using ItemGroup = Dictionary<string, Dictionary<int, HashSet<int>>>;

    public class ItemBlacklist
    {
        internal const string BaseGameItemGuid = "[['BASE'GAME'ITEM']]";

        private static string FavoritesDirectory;
        private static string FavoritesFile;
        private static string BlacklistDirectory;
        private static string BlacklistFile;

        private static readonly ItemGroup Favorites = new ItemGroup();
        private static readonly ItemGroup Blacklist = new ItemGroup();
        private static readonly Dictionary<CustomSelectListCtrl, ListVisibilityType> ListVisibility = new Dictionary<CustomSelectListCtrl, ListVisibilityType>();

        public static void Init()
        {
            BlacklistDirectory = Path.Combine(UserData.Path, "save");
            BlacklistFile = Path.Combine(BlacklistDirectory, "itemblacklist.xml");
            FavoritesDirectory = BlacklistDirectory;
            FavoritesFile = Path.Combine(FavoritesDirectory, "itemfavorites.xml");

            LoadFavorites();
            LoadBlacklist();
        }

        #region Check
        private static bool CheckFavorites(string guid, int category, int id)
        {
            return CheckGroup(Favorites, guid, category, id);
        }

        public static bool CheckFavorites(CustomSelectInfo customSelectInfo)
        {
            ResolveInfo Info = UniversalAutoResolver.TryGetResolutionInfo((ChaListDefine.CategoryNo)customSelectInfo.category, customSelectInfo.index);
            string guid = Info == null ? BaseGameItemGuid : Info.GUID ?? string.Empty;
            int category = Info == null ? customSelectInfo.category : (int)Info.CategoryNo;
            int slot = Info == null ? customSelectInfo.index : Info.Slot;

            return CheckFavorites(guid, category, slot);
        }

        private static bool CheckBlacklist(string guid, int category, int id)
        {
            return CheckGroup(Blacklist, guid, category, id);
        }

        public static bool CheckBlacklist(CustomSelectInfo customSelectInfo)
        {
            ResolveInfo Info = UniversalAutoResolver.TryGetResolutionInfo((ChaListDefine.CategoryNo)customSelectInfo.category, customSelectInfo.index);
            string guid = Info == null ? BaseGameItemGuid : Info.GUID ?? string.Empty;
            int category = Info == null ? customSelectInfo.category : (int)Info.CategoryNo;
            int slot = Info == null ? customSelectInfo.index : Info.Slot;

            return CheckBlacklist(guid, category, slot);
        }

        private static bool CheckGroup(ItemGroup group, string guid, int category, int id)
        {
            if (group.TryGetValue(guid, out var x))
                if (x.TryGetValue(category, out var y))
                    if (y.Contains(id))
                        return true;
            return false;
        }
        #endregion

        #region Load
        private static void LoadGroup(ItemGroup group, string directory, string file, string xmlElement)
        {
            Directory.CreateDirectory(directory);

            XDocument xml;
            if (File.Exists(file))
            {
                xml = XDocument.Load(file);
            }
            else
            {
                xml = new XDocument();
                xml.Add(new XElement(xmlElement));
                xml.Save(file);
            }

            var itemGroup = xml.Element(xmlElement);
            if (itemGroup != null)
            {
                foreach (var modElement in itemGroup.Elements("mod"))
                {
                    string guid = modElement.Attribute("guid")?.Value;
                    foreach (var categoryElement in modElement.Elements("category"))
                    {
                        if (int.TryParse(categoryElement.Attribute("number")?.Value, out int category))
                        {
                            foreach (var itemElement in categoryElement.Elements("item"))
                            {
                                if (int.TryParse(itemElement.Attribute("id")?.Value, out int id))
                                {
                                    if (!group.ContainsKey(guid))
                                        group[guid] = new Dictionary<int, HashSet<int>>();
                                    if (!group[guid].ContainsKey(category))
                                        group[guid][category] = new HashSet<int>();
                                    group[guid][category].Add(id);
                                }
                            }
                        }
                    }
                }
            }
        }
        private static void LoadFavorites()
        {
            LoadGroup(Favorites, FavoritesDirectory, FavoritesFile, "itemFavorites");
        }
        private static void LoadBlacklist()
        {
            LoadGroup(Blacklist, BlacklistDirectory, BlacklistFile, "itemBlacklist");
        }
        #endregion

        #region save
        private static void SaveGroup(ItemGroup group, string file, string xmlElement)
        {
            XDocument xml = new XDocument();
            XElement itemGroupElement = new XElement(xmlElement);
            xml.Add(itemGroupElement);

            foreach (var x in group)
            {
                XElement modElement = new XElement("mod");
                modElement.SetAttributeValue("guid", x.Key);
                itemGroupElement.Add(modElement);

                foreach (var y in x.Value)
                {
                    XElement categoryElement = new XElement("category");
                    categoryElement.SetAttributeValue("number", y.Key);
                    modElement.Add(categoryElement);

                    foreach (var z in y.Value)
                    {
                        XElement itemElement = new XElement("item");
                        itemElement.SetAttributeValue("id", z);
                        categoryElement.Add(itemElement);
                    }
                }
            }

            var retryCount = 3;
        retry:
            try
            {
                using (var fs = new FileStream(file, FileMode.Create, FileAccess.ReadWrite))
                using (var tw = new StreamWriter(fs, Encoding.UTF8))
                    xml.Save(tw);
                return;
            }
            catch (IOException)
            {
                System.Threading.Thread.Sleep(500);
                if (retryCount-- > 0)
                    goto retry;
                else
                    throw;
            }
        }
        private static void SaveFavorites()
        {
            SaveGroup(Favorites, FavoritesFile, "itemFavorites");
        }
        private static void SaveBlacklist()
        {
            SaveGroup(Blacklist, BlacklistFile, "itemBlacklist");
        }
        #endregion

        #region (Un)group

        private static void UngroupItem(ItemGroup group, ListVisibilityType boundVisibility, string guid, int category, int id, int index)
        {
            if (!group.ContainsKey(guid))
                group[guid] = new Dictionary<int, HashSet<int>>();
            if (!group[guid].ContainsKey(category))
                group[guid][category] = new HashSet<int>();
            group[guid][category].Remove(id);
            SaveBlacklist();

            PickerPanel.FilterList();

            //bool changeFilter = false;
            //var controls = CustomBase.Instance.GetComponentsInChildren<CustomSelectListCtrl>(true);
            //for (var i = 0; i < controls.Length; i++)
            //{
            //    var customSelectListCtrl = controls[i];
            //    if (customSelectListCtrl.GetSelectInfoFromIndex(index)?.category == category)
            //    {
            //        if (ListVisibility.TryGetValue(customSelectListCtrl, out var visibilityType))
            //            if (visibilityType == boundVisibility)
            //                customSelectListCtrl.DisvisibleItem(index, true);

            //        if (customSelectListCtrl.lstSelectInfo.All(x => x.disvisible))
            //            changeFilter = true;
            //    }
            //}

            //if (changeFilter)
            //    ChangeListFilter(ListVisibilityType.Filtered);
        }
        private static void UnfavoriteItem(string guid, int category, int id, int index)
        {
            UngroupItem(Favorites, ListVisibilityType.Favorites, guid, category, id, index);
            SaveFavorites();
        }
        private static void UnblacklistItem(string guid, int category, int id, int index)
        {
            UngroupItem(Blacklist, ListVisibilityType.Hidden, guid, category, id, index);
            SaveBlacklist();
        }

        private static int GroupMod(ItemGroup group, ItemGroup skipItemsIn, ListVisibilityType? hideFrom, string guid, bool onlyCurrentList)
        {
            //var allLists = CustomBase.Instance.GetComponentsInChildren<CustomSelectListCtrl>(true);

            int ProcessList(List<CustomSelectInfo> lstSelectInfo)
            {
                int skipped = 0;
                for (var i = 0; i < lstSelectInfo.Count; i++)
                {
                    CustomSelectInfo customSelectInfo = lstSelectInfo[i];
                    int category = customSelectInfo.category;
                    ResolveInfo info = UniversalAutoResolver.TryGetResolutionInfo((ChaListDefine.CategoryNo)category, customSelectInfo.index);
                    int slot = info == null ? customSelectInfo.index : info.Slot;

                    if (guid != (info == null ? BaseGameItemGuid : info.GUID ?? string.Empty))
                        continue;

                    if (skipItemsIn.ContainsKey(guid) && skipItemsIn[guid].ContainsKey(category) && skipItemsIn[guid][category].Contains(slot))
                    {
                        skipped++;
                        continue;
                    }

                    if (!group.ContainsKey(guid))
                        group[guid] = new Dictionary<int, HashSet<int>>();
                    if (!group[guid].ContainsKey(category))
                        group[guid][category] = new HashSet<int>();
                    group[guid][category].Add(slot);

                    //for (var j = 0; j < allLists.Length; j++)
                    //{
                    //    var customSelectListCtrl = allLists[j];
                    //    if (customSelectListCtrl.GetSelectInfoFromIndex(customSelectInfo.index)?.category == category)
                    //        if (ListVisibility.TryGetValue(customSelectListCtrl, out var visibilityType))
                    //            if (visibilityType == hideFrom)
                    //                customSelectListCtrl.DisvisibleItem(customSelectInfo.index, true);
                    //}
                }

                return skipped;
            }

            var allSkipped = onlyCurrentList ? ProcessList(PickerPanel.currentList) : PickerPanel.dictSelectInfo.Values.Sum(ProcessList);
            PickerPanel.FilterList();
            return allSkipped;
        }
        private static void FavoriteMod(string guid, bool onlyCurrentList)
        {
            int skipped = GroupMod(Favorites, Blacklist, null, guid, onlyCurrentList);
            if (skipped > 0)
                PseudoMaker.Logger.LogMessage($"Skipped {skipped} blacklisted items");
            SaveFavorites();
        }
        private static void BlacklistMod(string guid, bool onlyCurrentList)
        {
            int skipped = GroupMod(Blacklist, Favorites, ListVisibilityType.Filtered, guid, onlyCurrentList);
            if (skipped > 0)
                PseudoMaker.Logger.LogMessage($"Skipped {skipped} items that are in favorites");
            SaveBlacklist();
        }

        private static void UngroupMod(ItemGroup group, ListVisibilityType boundVisibility, string guid, bool onlyCurrentList)
        {
            //var allLists = CustomBase.Instance.GetComponentsInChildren<CustomSelectListCtrl>(true);

            bool ProcessList(List<CustomSelectInfo> lstSelectInfo)
            {
                var result = false;
                for (var i = 0; i < lstSelectInfo.Count; i++)
                {
                    CustomSelectInfo customSelectInfo = lstSelectInfo[i];
                    int category = customSelectInfo.category;
                    ResolveInfo info = UniversalAutoResolver.TryGetResolutionInfo((ChaListDefine.CategoryNo)category, customSelectInfo.index);
                    int slot = info == null ? customSelectInfo.index : info.Slot;

                    if (guid != (info == null ? BaseGameItemGuid : info.GUID ?? string.Empty))
                        continue;

                    if (!group.ContainsKey(guid))
                        group[guid] = new Dictionary<int, HashSet<int>>();
                    if (!group[guid].ContainsKey(category))
                        group[guid][category] = new HashSet<int>();
                    group[guid][category].Remove(slot);

                    //for (var j = 0; j < allLists.Length; j++)
                    //{
                    //    var customSelectListCtrl = allLists[j];
                    //    if (customSelectListCtrl.GetSelectInfoFromIndex(customSelectInfo.index)?.category == category)
                    //    {
                    //        if (ListVisibility.TryGetValue(customSelectListCtrl, out var visibilityType))
                    //            if (visibilityType == boundVisibility)
                    //                customSelectListCtrl.DisvisibleItem(customSelectInfo.index, true);

                    //        if (customSelectListCtrl.lstSelectInfo.All(x => x.disvisible))
                    //            result = true;
                    //    }
                    //}
                }

                return result;
            }

            var changeFilter = onlyCurrentList ? ProcessList(PickerPanel.currentList) : PickerPanel.dictSelectInfo.Values.Count(ProcessList) > 0;
            PickerPanel.FilterList();
            //if (changeFilter)
            //    ChangeListFilter(ListVisibilityType.Filtered);
        }
        private static void UnfavoriteMod(string guid, bool onlyCurrentList)
        {
            UngroupMod(Favorites, ListVisibilityType.Favorites, guid, onlyCurrentList);
            SaveFavorites();
        }
        private static void UnblacklistMod(string guid, bool onlyCurrentList)
        {
            UngroupMod(Blacklist, ListVisibilityType.Hidden, guid, onlyCurrentList);
            SaveBlacklist();
        }

        private static void GroupItem(ItemGroup group, ListVisibilityType? hideFrom, string guid, int category, int id, int index)
        {
            if (!group.ContainsKey(guid))
                group[guid] = new Dictionary<int, HashSet<int>>();
            if (!group[guid].ContainsKey(category))
                group[guid][category] = new HashSet<int>();
            group[guid][category].Add(id);

            PickerPanel.FilterList();
            //var controls = CustomBase.Instance.GetComponentsInChildren<CustomSelectListCtrl>(true);
            //for (var i = 0; i < controls.Length; i++)
            //{
            //    var customSelectListCtrl = controls[i];
            //    if (customSelectListCtrl.GetSelectInfoFromIndex(index)?.category == category)
            //        if (ListVisibility.TryGetValue(customSelectListCtrl, out var visibilityType))
            //            if (visibilityType == hideFrom)
            //                customSelectListCtrl.DisvisibleItem(index, true);
            //}
        }
        private static void FavoriteItem(string guid, int category, int id, int index)
        {
            UnblacklistItem(guid, category, id, index);
            GroupItem(Favorites, null, guid, category, id, index);
            SaveFavorites();
        }
        private static void BlacklistItem(string guid, int category, int id, int index)
        {
            UnfavoriteItem(guid, category, id, index);
            GroupItem(Blacklist, ListVisibilityType.Filtered, guid, category, id, index);
            SaveBlacklist();
        }
        #endregion

        public static void PrintInfo(CustomSelectInfoComponent customSelectInfoComponent)
        {
            var customSelectInfo = customSelectInfoComponent.info;

            if (customSelectInfo.index >= UniversalAutoResolver.BaseSlotID)
            {
                ResolveInfo info = UniversalAutoResolver.TryGetResolutionInfo((ChaListDefine.CategoryNo)customSelectInfo.category, customSelectInfo.index);
                if (info != null)
                {
                    PseudoMaker.Logger.LogMessage($"Item GUID:{info.GUID} Category:{(int)info.CategoryNo}({info.CategoryNo}) ID:{info.Slot}");

#if KKS
                    Dictionary<int, ListInfoBase> dictionary = Character.chaListCtrl.GetCategoryInfo(info.CategoryNo);
#else
                    Dictionary<int, ListInfoBase> dictionary = Singleton<Character>.Instance.chaListCtrl.GetCategoryInfo(info.CategoryNo);
#endif
                    if (dictionary != null && dictionary.TryGetValue(customSelectInfo.index, out ListInfoBase listInfoBase))
                    {
                        string assetBundle = listInfoBase.GetInfo(ChaListDefine.KeyType.MainAB);
                        if (!assetBundle.IsNullOrEmpty() && assetBundle != "0")
                        {
                            string asset = TryGetMainAsset(listInfoBase);
                            if (asset == null)
                                PseudoMaker.Logger.LogMessage($"AssetBundle:{assetBundle}");
                            else
                                PseudoMaker.Logger.LogMessage($"AssetBundle:{assetBundle} Asset:{asset}");
                        }
                    }

                    if (Sideloader.Sideloader.ZipArchives.TryGetValue(info.GUID, out string zipFileName))
                        PseudoMaker.Logger.LogMessage($"Zip File:{Path.GetFileName(zipFileName)}");
                }
            }
            else
            {
                PseudoMaker.Logger.LogMessage($"Item Category:{customSelectInfoComponent.info.category}({(ChaListDefine.CategoryNo)customSelectInfoComponent.info.category}) ID:{customSelectInfoComponent.info.index}");

#if KKS
                Dictionary<int, ListInfoBase> dictionary = Character.chaListCtrl.GetCategoryInfo((ChaListDefine.CategoryNo)customSelectInfoComponent.info.category);
#else
                Dictionary<int, ListInfoBase> dictionary = Singleton<Character>.Instance.chaListCtrl.GetCategoryInfo((ChaListDefine.CategoryNo)customSelectInfoComponent.info.category);
#endif
                if (dictionary != null && dictionary.TryGetValue(customSelectInfo.index, out var listInfoBase))
                {
                    string assetBundle = listInfoBase.GetInfo(ChaListDefine.KeyType.MainAB);
                    if (!assetBundle.IsNullOrEmpty() && assetBundle != "0")
                    {
                        string asset = TryGetMainAsset(listInfoBase);
                        if (asset == null)
                            PseudoMaker.Logger.LogMessage($"AssetBundle:{assetBundle}");
                        else
                            PseudoMaker.Logger.LogMessage($"AssetBundle:{assetBundle} Asset:{asset}");
                    }
                }
            }
        }
        private static string TryGetMainAsset(ListInfoBase listInfoBase)
        {
            string asset = listInfoBase.GetInfo(ChaListDefine.KeyType.MainData);
            if (!asset.IsNullOrEmpty() && asset != "0")
                return asset;
            asset = listInfoBase.GetInfo(ChaListDefine.KeyType.MainTex);
            if (!asset.IsNullOrEmpty() && asset != "0")
                return asset;
            asset = listInfoBase.GetInfo(ChaListDefine.KeyType.PaintTex);
            if (!asset.IsNullOrEmpty() && asset != "0")
                return asset;
            asset = listInfoBase.GetInfo(ChaListDefine.KeyType.NipTex);
            if (!asset.IsNullOrEmpty() && asset != "0")
                return asset;
            asset = listInfoBase.GetInfo(ChaListDefine.KeyType.SunburnTex);
            if (!asset.IsNullOrEmpty() && asset != "0")
                return asset;
            asset = listInfoBase.GetInfo(ChaListDefine.KeyType.UnderhairTex);
            if (!asset.IsNullOrEmpty() && asset != "0")
                return asset;
            return null;
        }

        public static void ShowMenu(PointerEventData eventData, CustomSelectInfoComponent copyInfoComp)
        {
            int index = copyInfoComp.info.index;
            string guid = BaseGameItemGuid;
            int category = copyInfoComp.info.category;
            int id = index;
            string author = null;

            if (index >= UniversalAutoResolver.BaseSlotID)
            {
                ResolveInfo info = UniversalAutoResolver.TryGetResolutionInfo((ChaListDefine.CategoryNo)category, index);
                if (info != null)
                {
                    guid = info.GUID ?? string.Empty;
                    id = info.Slot;
                    author = info.Author;
                }
            }

            bool favorite = CheckFavorites(guid, category, id);
            bool blacklist = CheckBlacklist(guid, category, id);

            var options = new Dictionary<string, Action>()
            {
                { "Print Item Info", () => PrintInfo(copyInfoComp) },
                { "Spacer0", null }
            };

            if (favorite)
            {
                options["Unfavorite Item"] = () => UnfavoriteItem(guid, category, id, index);
                options["Unfavorite All Atems From This Mod"] = () => UnfavoriteMod(guid, false);
                options["Unfavorite All Atems From This Mod On This List"] = () => UnfavoriteMod(guid, true);
            }
            else
            {
                options["Favorite Item"] = () => FavoriteItem(guid, category, id, index);
                options["Favorite All Items From This Mod"] = () => FavoriteMod(guid, false);
                options["Favorite All Items From This Mod On This List"] = () => FavoriteMod(guid, true);
            }
            
            options["Spacer1"] = null;

            if (blacklist)
            {
                options["Unhide This Item"] = () => UnblacklistItem(guid, category, id, index);
                options["Unhide All Items From This Mod"] = () => UnblacklistMod(guid, false);
                options["Unhide All Items From This Mod On This List"] = () => UnblacklistMod(guid, true);
            }
            else
            {
                options["Hide This Item"] = () => BlacklistItem(guid, category, id, index);
                options["Hide All Items From This Mod"] = () => BlacklistMod(guid, false);
                options["Hide All Items From This Mod On This List"] = () => BlacklistMod(guid, true);
            }

            if (author != null)
            {
                options["Spacer2"] = null;
                options["Search By Author"] = () => PickerPanel.FilterList(author);
            }

            ContextMenu.OpenContextMenu(eventData, options);
        }

        public enum ListVisibilityType { Filtered, Favorites, Hidden, All }
    }
}
