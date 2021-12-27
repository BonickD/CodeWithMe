//Requires: DataBase
//Requires: Fraction

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Oxide.Game.Rust.Cui;
using Oxide.Core.Plugins;
using UnityEngine;
using System.Linq;

namespace Oxide.Plugins
{
    [Info("FoxholeUI", "CASHR", "1.0.0")]
    public class FoxholeUI : RustPlugin
    {
        #region Static

        private static FoxholeUI UI;
        private bool UIDebug = true;
        private Dictionary<string, string> Images = new Dictionary<string, string>();
        private Dictionary<string, CuiElementContainer> UIPatterns = new Dictionary<string, CuiElementContainer>();
        private const string Layer = "UI_FoxholeUI";
        [PluginReference] private Plugin ImageLibrary;

        #endregion
        
        private Configuration _config;

        #region Config

        private class Configuration
        {
            [JsonProperty(PropertyName = "Настройки UI для выбора фракции")]
            public FractionUI FractionUI = new FractionUI();
        }
        
        private class FractionUI
        {
            [JsonProperty(PropertyName = "Подсказка при выборе фракции")]
            public string FractionChoseHint = "Фракция выбирается один раз на вайп. Обдумайте свой выбор!";

            [JsonProperty(PropertyName = "[Wardens] Показательное изображения")]
            public string WardensUpperIcon = "https://i.imgur.com/5cN5Vbz.png";

            [JsonProperty(PropertyName = "[Wardens] Ссылка на иконку фракции")]
            public string WardensIcon = "https://i.imgur.com/o4sl4L5.png";

            [JsonProperty(PropertyName = "[Wardens] Название фракции")]
            public string WardensName = "Стражи";

            [JsonProperty(PropertyName = "[Warndes] Информация о фракции")]
            public string WardensInfo = "Стражи описаны как нация чести и традиций. Они коренные жители региона, в котором происходит игра. Когда-то считавшиеся огромной империей, они с тех пор пришли в упадок и в настоящее время находятся в состоянии войны с колониями. Во время типичного Мирового Завоевания Стражи контролируют самые северные регионы карты мира. Некоторые конфигурации карт отклоняются от этого стандарта.";
            
            [JsonProperty(PropertyName = "[Colonials] Показательное изображения")]
            public string ColonialsUpperIcon = "https://i.imgur.com/GjTyMEQ.png";

            [JsonProperty(PropertyName = "[Colonials] Ссылка на иконку фракции")]
            public string ColonialsIcon = "https://i.imgur.com/Jc6hhAY.png";

            [JsonProperty(PropertyName = "[Colonials] Название фракции")]
            public string ColonialsName = "Колонисты";

            [JsonProperty(PropertyName = "[Colonials] Информация о фракции")]
            public string ColonialsInfo = "Колонисты описываются фракция изобретательности и практичности. Иногда их называют «Колониальный легион». Во время типичного Мирового Завоевания Колониалы контролируют самые южные регионы карты мира. Некоторые конфигурации карт отклоняются от этого стандарта.";
        }

        protected override void LoadConfig()
        {
            base.LoadConfig();
            try
            {
                _config = Config.ReadObject<Configuration>();
                if (_config == null) throw new Exception();
                SaveConfig();
            }
            catch
            {
                PrintError("Your configuration file contains an error. Using default configuration values.");
                LoadDefaultConfig();
            }
        }

        protected override void SaveConfig() => Config.WriteObject(_config);

        protected override void LoadDefaultConfig() => _config = new Configuration();

        #endregion

        #region OxideHooks

        private void OnServerInitialized()
        {
            UI = this;
            PrintWarning("Загружаем все изображения для UI...");
            timer.In(1f, LoadAllImages);
        }

        private void Unload()
        {
            UI = null;
        }
        
        private void OnPlayerConnected(BasePlayer player)
        {
            if (player == null) return;
            NextTick(() =>
            {
                if (DataBase.Data.PlayersData[player.userID].fraction == DataBase.Fraction.None) ShowFractionUI(player);
            });
        }
  
        #endregion

        #region Commands
        

        #endregion

        #region Functions

        public static void AddImage(string image)
        {
            if (!UI.ImageLibrary.Call<bool>("HasImage", image)) UI.ImageLibrary.Call("AddImage", image, image);
            UI.Images.Add(image, UI.ImageLibrary.Call<string>("GetImage", image));
        }
        
        private void LoadAllImages()
        {
            var fractionSettings = _config.FractionUI;
            AddImage(fractionSettings.ColonialsIcon);
            AddImage(fractionSettings.ColonialsUpperIcon);
            AddImage(fractionSettings.WardensIcon);
            AddImage(fractionSettings.WardensUpperIcon);

            
            
            PrintWarning("Изображения загружены");
            PrintWarning("Создание паттернов для UI");
            timer.In(1f, LoadPatterns);
        }

        private void LoadPatterns()
        {
            AddFractionPattern();
            AddLaboratoryPattern();
            AddFactoryPattern();
            AddStockpilePattern();
            PrintWarning("Паттерны для UI успешно сгенерированы");
            if (!UIDebug) return;
            ShowStockpileUI(BasePlayer.activePlayerList.First());
        }


        
        private void AddFractionPattern()
        {
            var settings = _config.FractionUI;
            var container = new CuiElementContainer();
            
            container.Add(new CuiPanel
            {
                CursorEnabled = true,
                RectTransform = {AnchorMin = "0 0", AnchorMax = "1 1"},
                Image = {Color = "0 0 0 0.95", Material = "assets/content/ui/uibackgroundblur-ingamemenu.mat"}
            }, "Overlay",Layer);

            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0.5 0.5", AnchorMax = "0.5 0.5", OffsetMin = "-83 252", OffsetMax = "83 336"},
                Image = {Color = "0.08 0.08 0.08 1.00"}
            }, Layer);

            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0.5 0.5", AnchorMax = "0.5 0.5", OffsetMin = "-83 315", OffsetMax = "83 336"},
                Image = {Color = "0.45 0.45 0.45 1.00"}
            }, Layer);

            container.Add(new CuiLabel
            {
                RectTransform = {AnchorMin = "0.5 0.5", AnchorMax = "0.5 0.5", OffsetMin = "-78 315", OffsetMax = "83 336"},
                Text =
                {
                    Text = "Выберите фракцию", Font = "robotocondensed-regular.ttf", FontSize = 13, Align = TextAnchor.MiddleLeft,
                    Color = "0.85 0.85 0.85 1.00"
                }
            }, Layer);

            container.Add(new CuiLabel
            {
                RectTransform = {AnchorMin = "0.5 0.5", AnchorMax = "0.5 0.5", OffsetMin = "-80 252", OffsetMax = "80 315"},
                Text =
                {
                    Text = settings.FractionChoseHint, Font = "robotocondensed-regular.ttf", FontSize = 10, Align = TextAnchor.MiddleCenter,
                    Color = "0.85 0.85 0.85 1.00"
                }
            }, Layer);

            #region Wardens

            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0.5 0.5", AnchorMax = "0.5 0.5", OffsetMin = "-496 -228", OffsetMax = "-42 227"},
                Image = {Color = "0.09 0.09 0.09 1.00"}
            }, Layer);

            container.Add(new CuiLabel
            {
                RectTransform = {AnchorMin = "0.5 0.5", AnchorMax = "0.5 0.5", OffsetMin = "-342 -79", OffsetMax = "-42 -48"},
                Text =
                {
                    Text = settings.WardensName, Font = "robotocondensed-regular.ttf", FontSize = 22, Align = TextAnchor.MiddleCenter,
                    Color = "0.92 0.49 0.24 1.00"
                }
            }, Layer);

            container.Add(new CuiLabel
            {
                RectTransform = {AnchorMin = "0.5 0.5", AnchorMax = "0.5 0.5", OffsetMin = "-334 -228", OffsetMax = "-50 -86"},
                Text =
                {
                    Text = settings.WardensInfo, Font = "robotocondensed-regular.ttf", FontSize = 11, Align = TextAnchor.UpperLeft,
                    Color = "0.85 0.85 0.85 1.00"
                }
            }, Layer);

            container.Add(new CuiElement
            {
                Parent = Layer,
                Components =
                {
                    new CuiRawImageComponent {Png = ImageLibrary.Call<string>("GetImage", settings.WardensIcon)},
                    new CuiRectTransformComponent {AnchorMin = "0.5 0.5", AnchorMax = "0.5 0.5", OffsetMin = "-468 -193", OffsetMax = "-342 -79"}
                }
            });

            container.Add(new CuiElement
            {
                Parent = Layer,
                Components =
                {
                    new CuiRawImageComponent {Png = ImageLibrary.Call<string>("GetImage", settings.WardensUpperIcon)},
                    new CuiRectTransformComponent {AnchorMin = "0.5 0.5", AnchorMax = "0.5 0.5", OffsetMin = "-496 -48", OffsetMax = "-42 227"}
                }
            });

            #endregion
            
            #region Colonial

            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0.5 0.5", AnchorMax = "0.5 0.5", OffsetMin = "42 -228", OffsetMax = "497 227"},
                Image = {Color = "0.09 0.09 0.09 1.00"}
            }, Layer);

            container.Add(new CuiLabel
            {
                RectTransform = {AnchorMin = "0.5 0.5", AnchorMax = "0.5 0.5", OffsetMin = "205 -79", OffsetMax = "497 -48"},
                Text =
                {
                    Text = settings.ColonialsName, Font = "robotocondensed-regular.ttf", FontSize = 22, Align = TextAnchor.MiddleCenter,
                    Color = "0.92 0.49 0.24 1.00"
                }
            }, Layer);

            container.Add(new CuiLabel
            {
                RectTransform = {AnchorMin = "0.5 0.5", AnchorMax = "0.5 0.5", OffsetMin = "205 -228", OffsetMax = "489 -86"},
                Text =
                {
                    Text = settings.ColonialsInfo, Font = "robotocondensed-regular.ttf", FontSize = 11, Align = TextAnchor.UpperLeft,
                    Color = "0.85 0.85 0.85 1.00"
                }
            }, Layer);

            container.Add(new CuiElement
            {
                Parent = Layer,
                Components =
                {
                    new CuiRawImageComponent {Png = ImageLibrary.Call<string>("GetImage", settings.ColonialsIcon)},
                    new CuiRectTransformComponent {AnchorMin = "0.5 0.5", AnchorMax = "0.5 0.5", OffsetMin = "64 -190", OffsetMax = "205 -81"}
                }
            });

            container.Add(new CuiElement
            {
                Parent = Layer,
                Components =
                {
                    new CuiRawImageComponent {Png = ImageLibrary.Call<string>("GetImage", settings.ColonialsUpperIcon)},
                    new CuiRectTransformComponent {AnchorMin = "0.5 0.5", AnchorMax = "0.5 0.5", OffsetMin = "42 -48", OffsetMax = "497 227"}
                }
            });

            #endregion
            UIPatterns.Add("ChoseFraction", container);
        }

        private void AddLaboratoryPattern()
        {
            var container = new CuiElementContainer();

            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0 0", AnchorMax = "1 1"},
                Image = {Color = "0 0 0 0.95", Material = "assets/content/ui/uibackgroundblur-ingamemenu.mat"}
            }, "Overlay", Layer);
            
            #region InventoryUI
            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0.5 0.5", AnchorMax = "0.5 0.5", OffsetMin = "-403 -226", OffsetMax = "-157 261"},
                Image = {Color = "0 0 0 0.98"}
            }, Layer, Layer + ".Inventory");

            container.Add(new CuiLabel
            {
                RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMax = "246 25"},
                Text =
                {
                    Text = "ИНВЕНТАРЬ", Font = "robotocondensed-regular.ttf", FontSize = 20, Align = TextAnchor.MiddleLeft,
                    Color = "0.82 0.82 0.87 1.00"
                }
            }, Layer + ".Inventory");

            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "0 -21", OffsetMax = "246 0"},
                Image = {Color = "0.45 0.45 0.45 1.00"}
            }, Layer + ".Inventory");

            container.Add(new CuiLabel
            {
                RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "8 -21", OffsetMax = "246 0"},
                Text =
                {
                    Text = "ХРАНИЛИЩЕ", Font = "robotocondensed-regular.ttf", FontSize = 15, Align = TextAnchor.MiddleLeft,
                    Color = "0.85 0.85 0.85 1.00"
                }
            }, Layer + ".Inventory");

            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "0 -192", OffsetMax = "246 -171"},
                Image = {Color = "0.45 0.45 0.45 1.00"}
            }, Layer + ".Inventory");

            container.Add(new CuiLabel
            {
                RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "8 -192", OffsetMax = "246 -171"},
                Text =
                {
                    Text = "ВАШИ ПРЕДМЕТЫ", Font = "robotocondensed-regular.ttf", FontSize = 15, Align = TextAnchor.MiddleLeft,
                    Color = "0.85 0.85 0.85 1.00"
                }
            }, Layer + ".Inventory");
            #endregion
            
            #region TechUI
            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0.5 0.5", AnchorMax = "0.5 0.5", OffsetMin = "-147 -226", OffsetMax = "404 261"},
                Image = {Color = "0 0 0 0.98"}
            }, Layer, Layer + ".Technology");
            container.Add(new CuiLabel
            {
                RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "0 0", OffsetMax = "246 25"},
                Text =
                {
                    Text = "ТЕХНОЛОГИИ", Font = "robotocondensed-regular.ttf", FontSize = 20, Align = TextAnchor.MiddleLeft,
                    Color = "0.82 0.82 0.87 1.00"
                }
            }, Layer + ".Technology");
            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "0 -21", OffsetMax = "551 0"},
                Image = {Color = "0.45 0.45 0.45 1.00"}
            }, Layer + ".Technology");
            container.Add(new CuiLabel
            {
                RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "8 -21", OffsetMax = "551 0"},
                Text =
                {
                    Text = "Изучение", Font = "robotocondensed-regular.ttf", FontSize = 15, Align = TextAnchor.MiddleLeft,
                    Color = "0.85 0.85 0.85 1.00"
                }
            }, Layer + ".Technology");
            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "0 -42", OffsetMax = "551 -21"},
                Image = {Color = "0.22 0.21 0.22 1.00"}
            }, Layer + ".Technology");
            #endregion

            UIPatterns.Add("Laboratory", container);
        }
        
        private void AddFactoryPattern()
        {
            var container = new CuiElementContainer();

            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0 0", AnchorMax = "1 1"},
                Image = {Color = "0 0 0 0.95", Material = "assets/content/ui/uibackgroundblur-ingamemenu.mat"}
            }, "Overlay", Layer);
            #region ShowUITeamQueue
            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0.5 0.5", AnchorMax = "0.5 0.5", OffsetMin = "130 -226", OffsetMax = "428 261"},
                Image = {Color = "0 0 0 0.98"}
            }, Layer, Layer + ".Queue");
            
            container.Add(new CuiLabel
            {
                RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMax = "299 25"},
                Text =
                {
                    Text = "ФАБРИКА", Font = "robotocondensed-regular.ttf", FontSize = 20, Align = TextAnchor.MiddleLeft,
                    Color = "0.82 0.82 0.87 1.00"
                }
            }, Layer + ".Queue");

            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "0 -21", OffsetMax = "299 0"},
                Image = {Color = "0.45 0.45 0.45 1.00"}
            }, Layer + ".Queue");

            container.Add(new CuiLabel
            {
                RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "8 -21", OffsetMax = "299 0"},
                Text =
                {
                    Text = "ОЧЕРЕДИ ПРОИЗВОДСТВА", Font = "robotocondensed-regular.ttf", FontSize = 15, Align = TextAnchor.MiddleLeft,
                    Color = "0.85 0.85 0.85 1.00"
                }
            }, Layer + ".Queue");
            
            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "0 -42", OffsetMax = "299 -21"},
                Image = {Color = "0.21 0.21 0.21 1.00"}
            }, Layer + ".Queue");
            #endregion

            #region ShowUICraftCategory
            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0.5 0.5", AnchorMax = "0.5 0.5", OffsetMin = "-175 -226", OffsetMax = "119 261"},
                Image = {Color = "0 0 0 0.98"}
            }, Layer, Layer + ".CraftCategory");
            
            container.Add(new CuiLabel
            {
                RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMax = "294 25"},
                Text =
                {
                    Text = "ПРОИЗВОДСТВО", Font = "robotocondensed-regular.ttf", FontSize = 20, Align = TextAnchor.MiddleLeft,
                    Color = "0.82 0.82 0.87 1.00"
                }
            }, Layer + ".CraftCategory");

            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "0 -21", OffsetMax = "294 0"},
                Image = {Color = "0.45 0.45 0.45 1.00"}
            }, Layer + ".CraftCategory");

            container.Add(new CuiLabel
            {
                RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "8 -21", OffsetMax = "294 0"},
                Text =
                {
                    Text = "РЕЦЕПТЫ", Font = "robotocondensed-regular.ttf", FontSize = 15, Align = TextAnchor.MiddleLeft,
                    Color = "0.85 0.85 0.85 1.00"
                }
            }, Layer + ".CraftCategory");

            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "0 -42", OffsetMax = "294 -21"},
                Image = {Color = "0.21 0.21 0.21 1.00"}
            }, Layer + ".CraftCategory");

            #endregion

            #region ShowUIInventory
            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0.5 0.5", AnchorMax = "0.5 0.5", OffsetMin = "-431 -226", OffsetMax = "-185 261"},
                Image = {Color = "0 0 0 0.98"}
            }, Layer, Layer + ".Inventory");

            container.Add(new CuiLabel
            {
                RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMax = "246 25"},
                Text =
                {
                    Text = "ИНВЕНТАРЬ", Font = "robotocondensed-regular.ttf", FontSize = 20, Align = TextAnchor.MiddleLeft,
                    Color = "0.82 0.82 0.87 1.00"
                }
            }, Layer + ".Inventory");

            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "0 -21", OffsetMax = "246 0"},
                Image = {Color = "0.45 0.45 0.45 1.00"}
            }, Layer + ".Inventory");

            container.Add(new CuiLabel
            {
                RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "8 -21", OffsetMax = "246 0"},
                Text =
                {
                    Text = "ХРАНИЛИЩЕ", Font = "robotocondensed-regular.ttf", FontSize = 15, Align = TextAnchor.MiddleLeft,
                    Color = "0.85 0.85 0.85 1.00"
                }
            }, Layer + ".Inventory");
            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "0 -192", OffsetMax = "246 -171"},
                Image = {Color = "0.45 0.45 0.45 1.00"}
            }, Layer + ".Inventory");

            container.Add(new CuiLabel
            {
                RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "8 -192", OffsetMax = "246 -171"},
                Text =
                {
                    Text = "ВАШИ ПРЕДМЕТЫ", Font = "robotocondensed-regular.ttf", FontSize = 15, Align = TextAnchor.MiddleLeft,
                    Color = "0.85 0.85 0.85 1.00"
                }
            }, Layer + ".Inventory");
            #endregion
            
            UIPatterns.Add("MenuFactory", container);
        }

        private void AddStockpilePattern()
        {
            var container = new CuiElementContainer();

            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0 0", AnchorMax = "1 1"},
                Image = {Color = "0 0 0 0.98"}
            }, "Overlay", Layer);
            
            #region InventoryUI
            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0.5 0.5", AnchorMax = "0.5 0.5", OffsetMin = "-328 -226", OffsetMax = "-82 261"},
                Image = {Color = "0 0 0 0.98"}
            }, Layer, Layer + ".Inventory");
            container.Add(new CuiLabel
            {
                RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMax = "246 25"},
                Text =
                {
                    Text = "ИНВЕНТАРЬ", Font = "robotocondensed-regular.ttf", FontSize = 20, Align = TextAnchor.MiddleLeft,
                    Color = "0.82 0.82 0.87 1.00"
                }
            }, Layer + ".Inventory");
            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "0 -21", OffsetMax = "246 0"},
                Image = {Color = "0.45 0.45 0.45 1.00"}
            }, Layer + ".Inventory");
            container.Add(new CuiLabel
            {
                RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "8 -21", OffsetMax = "238 0"},
                Text =
                {
                    Text = "ХРАНИЛИЩЕ", Font = "robotocondensed-regular.ttf", FontSize = 15, Align = TextAnchor.MiddleLeft,
                    Color = "0.85 0.85 0.85 1.00"
                }
            }, Layer + ".Inventory");
            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "0 -192", OffsetMax = "246 -171"},
                Image = {Color = "0.45 0.45 0.45 1.00"}
            }, Layer + ".Inventory");

            container.Add(new CuiLabel
            {
                RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "8 -192", OffsetMax = "238 -171"},
                Text =
                {
                    Text = "ВАШИ ПРЕДМЕТЫ", Font = "robotocondensed-regular.ttf", FontSize = 15, Align = TextAnchor.MiddleLeft,
                    Color = "0.85 0.85 0.85 1.00"
                }
            }, Layer + ".Inventory");
            #endregion

            #region Tech
            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0.5 0.5", AnchorMax = "0.5 0.5", OffsetMin = "-72 -226", OffsetMax = "318 261"},
                Image = {Color = "0 0 0 0.98"}
            }, Layer, Layer + ".Technology");
            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "0 -21", OffsetMax = "390 0"},
                Image = {Color = "0.45 0.45 0.45 1.00"}
            }, Layer + ".Technology");
            container.Add(new CuiLabel
            {
                RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "5 -21", OffsetMax = "385 0"},
                Text =
                {
                    Text = "ИЗУЧЕНИЕ", Font = "robotocondensed-regular.ttf", FontSize = 15, Align = TextAnchor.MiddleLeft,
                    Color = "0.85 0.85 0.85 1.00"
                }
            }, Layer + ".Technology");
            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "0 -42", OffsetMax = "390 -21"},
                Image = {Color = "0.22 0.21 0.22 1.00"}
            }, Layer + ".Technology");

            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "3 -428", OffsetMax = "387 -380"},
                Image = {Color = "0.22 0.21 0.22 1.00"}
            }, Layer + ".Technology", Layer + ".TechnologyTesthand"); //135

            
            #region TesthandButtons
            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0 0", AnchorMax = "0 0", OffsetMin = "135 10", OffsetMax = "163 38"},
                Image = {Color = "0.25 0.25 0.25 0.98"}
            }, Layer + ".TechnologyTesthand");
            container.Add(new CuiElement
            {
                Parent = Layer + ".TechnologyTesthand",
                Components =
                {
                    new CuiRawImageComponent {Png = ImageLibrary.Call<string>("GetImage", "")},
                    new CuiRectTransformComponent {AnchorMin = "0 0", AnchorMax = "0 0",  OffsetMin = "135 10", OffsetMax = "163 38"}
                }
            });
            container.Add(new CuiButton
            {
                RectTransform = {AnchorMin = "0 0", AnchorMax = "0 0",  OffsetMin = "135 10", OffsetMax = "163 38"},
                Button = {Color = "0 0 0 0", Command = ""},
                Text =
                {
                    Text = "", Font = "robotocondensed-regular.ttf", FontSize = 15, Align = TextAnchor.MiddleCenter,
                    Color = "0 0 0 0"
                }
            }, Layer + ".TechnologyTesthand");
            // ========= //
            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0 0", AnchorMax = "0 0", OffsetMin = "165 10", OffsetMax = "193 38"},
                Image = {Color = "0.25 0.25 0.25 0.98"}
            }, Layer + ".TechnologyTesthand");
            container.Add(new CuiElement
            {
                Parent = Layer + ".TechnologyTesthand",
                Components =
                {
                    new CuiRawImageComponent {Png = ImageLibrary.Call<string>("GetImage", "")},
                    new CuiRectTransformComponent {AnchorMin = "0 0", AnchorMax = "0 0", OffsetMin = "165 10", OffsetMax = "193 38"}
                }
            });
            container.Add(new CuiButton
            {
                RectTransform = {AnchorMin = "0 0", AnchorMax = "0 0", OffsetMin = "165 10", OffsetMax = "193 38"},
                Button = {Color = "0 0 0 0", Command = ""},
                Text =
                {
                    Text = "", Font = "robotocondensed-regular.ttf", FontSize = 15, Align = TextAnchor.MiddleCenter,
                    Color = "0 0 0 0"
                }
            }, Layer + ".TechnologyTesthand");
            // ========= //
            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0 0", AnchorMax = "0 0", OffsetMin = "195 10", OffsetMax = "223 38"},
                Image = {Color = "0.25 0.25 0.25 0.98"}
            }, Layer + ".TechnologyTesthand");
            container.Add(new CuiElement
            {
                Parent = Layer + ".TechnologyTesthand",
                Components =
                {
                    new CuiRawImageComponent {Png = ImageLibrary.Call<string>("GetImage", "")},
                    new CuiRectTransformComponent {AnchorMin = "0 0", AnchorMax = "0 0", OffsetMin = "195 10", OffsetMax = "223 38"}
                }
            });
            container.Add(new CuiButton
            {
                RectTransform = {AnchorMin = "0 0", AnchorMax = "0 0", OffsetMin = "195 10", OffsetMax = "223 38"},
                Button = {Color = "0 0 0 0", Command = ""},
                Text =
                {
                    Text = "", Font = "robotocondensed-regular.ttf", FontSize = 15, Align = TextAnchor.MiddleCenter,
                    Color = "0 0 0 0"
                }
            }, Layer + ".TechnologyTesthand");
            // ========= //
            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0 0", AnchorMax = "0 0", OffsetMin = "225 10", OffsetMax = "253 38"},
                Image = {Color = "0.25 0.25 0.25 0.98"}
            }, Layer + ".TechnologyTesthand");
            container.Add(new CuiElement
            {
                Parent = Layer + ".TechnologyTesthand",
                Components =
                {
                    new CuiRawImageComponent {Png = ImageLibrary.Call<string>("GetImage", "")},
                    new CuiRectTransformComponent {AnchorMin = "0 0", AnchorMax = "0 0", OffsetMin = "225 10", OffsetMax = "253 38"}
                }
            });
            container.Add(new CuiButton
            {
                RectTransform = {AnchorMin = "0 0", AnchorMax = "0 0", OffsetMin = "225 10", OffsetMax = "253 38"},
                Button = {Color = "0 0 0 0", Command = ""},
                Text =
                {
                    Text = "", Font = "robotocondensed-regular.ttf", FontSize = 15, Align = TextAnchor.MiddleCenter,
                    Color = "0 0 0 0"
                }
            }, Layer + ".TechnologyTesthand");
            #endregion

            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "0 -455", OffsetMax = "390 -433"},
                Image = {Color = "0.45 0.45 0.45 1.00"}
            }, Layer + ".Technology");
            container.Add(new CuiLabel
            {
                RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "5 -455", OffsetMax = "385 -433"},
                Text =
                {
                    Text = "ДЕЙСТВИЯ", Font = "robotocondensed-regular.ttf", FontSize = 15, Align = TextAnchor.MiddleLeft,
                    Color = "0.85 0.85 0.85 1.00"
                }
            }, Layer + ".Technology");
            
            
            
            
            #endregion

            UIPatterns.Add("Stockpile", container);
;        }
        
        #endregion

        #region UI

        private void ShowFractionUI(BasePlayer player)
        {
            var container = UIPatterns["ChoseFraction"];
            var canJoinWardens = Fraction.CanJoinToWarden();
            var canJoinColonial = Fraction.CanJoinToColonials();

            container.Add(new CuiButton
            {
                RectTransform = {AnchorMin = "0.5 0.5", AnchorMax = "0.5 0.5", OffsetMin = "-334 -293", OffsetMax = "-204 -262"},
                Button = {Color = canJoinWardens ? "0.34 0.34 0.34 1" : "0.34 0.34 0.34 0.7", Close = canJoinWardens ? Layer : "", Command = canJoinWardens ? "abc" : ""},
                Text =
                {
                    Text = canJoinWardens ? "Выбрать" : "Не доступно", Font = "robotocondensed-regular.ttf", FontSize = 17, Align = TextAnchor.MiddleCenter,
                    Color = canJoinWardens ? "0.85 0.85 0.85 1.00" : "0.85 0.85 0.85 0.7"
                }
            }, Layer);
            
            container.Add(new CuiButton
            {
                RectTransform = {AnchorMin = "0.5 0.5", AnchorMax = "0.5 0.5", OffsetMin = "204 -293", OffsetMax = "334 -262"},
                Button = {Color = canJoinColonial ? "0.34 0.34 0.34 1" : "0.34 0.34 0.34 0.7", Close = canJoinColonial ? Layer : "", Command = canJoinColonial ? "abc" : ""},
                Text =
                {
                    Text = canJoinColonial ? "Выбрать" : "Не доступно", Font = "robotocondensed-regular.ttf", FontSize = 17, Align = TextAnchor.MiddleCenter,
                    Color = canJoinColonial ? "0.85 0.85 0.85 1.00" : "0.85 0.85 0.85 0.7"
                }
            }, Layer);
            
            
            CuiHelper.DestroyUi(player, Layer);
            CuiHelper.AddUi(player, container);
        }
        
        private void ShowLaboratoryUI(BasePlayer player)
        {
            var container = UIPatterns["Laboratory"];

            CuiHelper.DestroyUi(player, Layer);
            CuiHelper.AddUi(player, container);
        }
        
        private void ShowFactoryUI(BasePlayer player)
        {
            var container = UIPatterns["MenuFactory"];

            CuiHelper.DestroyUi(player, Layer);
            CuiHelper.AddUi(player, container);
        }
        
        private void ShowStockpileUI(BasePlayer player)
        {
            var container = UIPatterns["Stockpile"];

            CuiHelper.DestroyUi(player, Layer);
            CuiHelper.AddUi(player, container);
        }

        #endregion

    }
}