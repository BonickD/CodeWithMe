using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Plugins;
using Oxide.Game.Rust.Cui;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using UnityEngine;
namespace Oxide.Plugins
{
    [Info("InfoMenu", "OxideBro", "1.0.0")]
    public class InfoMenu : RustPlugin
    {
        private PluginConfig config;
        protected override void LoadDefaultConfig()
        {
            PrintWarning("Благодарим за заказ плагина у разработчика OxideBro. Если вы передадите этот плагин сторонним лицам знайте - это лишает вас гарантированных обновлений!");
            config = PluginConfig.DefaultConfig();
        }
        protected override void LoadConfig()
        {
            base.LoadConfig();
            config = Config.ReadObject<PluginConfig>();

            if (config.PluginVersion < Version)
                UpdateConfigValues();

            Config.WriteObject(config, true);
        }

        private void UpdateConfigValues()
        {
            PluginConfig baseConfig = PluginConfig.DefaultConfig();
            if (config.PluginVersion < new VersionNumber(1, 0, 0))
            {
                PrintWarning("Config update detected! Updating config values...");
                foreach (var page in config.tabs)
                {
                    foreach (var paa in page.pages)
                    {
                        if (paa.URL == null)
                            paa.URL = "";
                    }
                }
                PrintWarning("Config update completed!");
            }
            config.PluginVersion = Version;
        }

        protected override void SaveConfig()
        {
            Config.WriteObject(config);
        }

        void OnPlayerConnected(BasePlayer player)
        {
            if (config.OpenToConnectionFirst)
            {
                if (player.IsReceivingSnapshot)
                {
                    timer.In(1f, () => OnPlayerConnected(player));
                    return;
                }
                if (!PlayersList.ContainsKey(player.userID))
                {
                    PlayersList.Add(player.userID, new PlayerClasses() { Firts = true });
                    CreateMenu(player, 0, 0);
                    return;
                }
            }
            if (config.OpenToConnection)
            {
                if (player.IsReceivingSnapshot)
                {
                    timer.In(1f, () => OnPlayerConnected(player));
                    return;
                }
                CreateMenu(player, 0, 0);
                return;
            }
        }

        class PluginConfig
        {
            [JsonProperty("Версия конфигурации")]
            public VersionNumber PluginVersion = new VersionNumber();
            [JsonProperty("Команды для открытия меню")]
            public List<string> Commands = new List<string>();
            [JsonProperty("Показывать при подключении")]
            public bool OpenToConnection = false;
            [JsonProperty("Показывать только при первом подключении")]
            public bool OpenToConnectionFirst = true;
            [JsonProperty("Меню: высота (0.0 - 1.0)")]
            public double MenuAnchorHeight = 1;
            [JsonProperty("Меню: ширина (0.0 - 1.0)")]
            public double MenuAnchorWidth = 1; 
            [JsonProperty("Меню: цвет фона (RGBA)")]
            public string MenuColor;
            [JsonProperty("Меню: фоновое изображение (ссылка)")]
            public string MenuBackgroundImage;
            [JsonProperty("Боковое меню: ширина (0.0 - 1.0)")]
            public double SidebarWidth;
            [JsonProperty("Боковое меню: цвет фона (RGBA)")]
            public string SidebarColor;
            [JsonProperty("Вкладки: ширина (0.0 - 1.0)")]
            public double TabWidth;
            [JsonProperty("Вкладки: высота (0.0 - 1.0)")]
            public double TabHeight;
            [JsonProperty("Вкладки: промежуточный отступ (0.0 - 1.0)")]
            public double TabIndent;
            [JsonProperty("Вкладки: верхний отступ (0.0 - 1.0)")]
            public double TabTopIndent;
            [JsonProperty("Вкладки: цвет фона (RGBA)")]
            public string TabColor;
            [JsonProperty("Вкладки: активный цвет фона (RGBA)")]
            public string TabActiveColor;
            [JsonProperty("Вкладки: размер шрифта")]
            public int TabFontSize;
            [JsonProperty("Вкладки: цвет текста (RGBA)")]
            public string TabTextColor;
            [JsonProperty("Вкладки: цвет тени текста (RGBA)")]
            public string TabTextOutlineColor;
            [JsonProperty("Страницы: верхний и нижний отступ (0.0 - 1.0)")]
            public double PageUpperAndLowerIndent;
            [JsonProperty("Страницы: левый и правый отступ (0.0 - 1.0)")]
            public double PageIndentLeftRight;
            [JsonProperty("Вкладки")]
            public List<Tabs> tabs = new List<Tabs>();

            [JsonProperty("LangSettings")] public Dictionary<string, Dictionary<string, string>> LangSettings;
            public static PluginConfig DefaultConfig()
            {
                return new PluginConfig()
                {
                    PluginVersion = new VersionNumber(),
                };
            }
        }

        public class Tabs
        {
            [JsonProperty("Заголовок вкладки")]
            public string Title;
            [JsonProperty("Страницы")]
            public List<Pages> pages = new List<Pages>();
        }

        public class Pages
        {
            [JsonProperty("Изображения")]
            public List<Images> Images = new List<Images>();
            [JsonProperty("Блоки текста")]
            public List<TextBlocks> blocks = new List<TextBlocks>();
            [JsonProperty("Ссылка на иконку если это уникальные плагины")]
            public string URL = "";
        }

        public class Images
        {
            [JsonProperty("Ссылка")]
            public string URL;
            [JsonProperty("Позиция по вертикали (0.0 - 1.0)")]
            public double PositionVertical;
            [JsonProperty("Позиция по горизонтали (0.0 - 1.0)")]
            public double PositionHorizontal;
            [JsonProperty("Высота (0.0 - 1.0)")]
            public double Height;
            [JsonProperty("Ширина (0.0 - 1.0)")]
            public double Width;
        }

        public class TextBlocks
        {
            [JsonProperty("Высота блока (0.0 - 1.0)")]
            public double Height;
            [JsonProperty("Колонки текста")]
            public List<TextColumns> colums = new List<TextColumns>();
        }

        public class TextColumns
        {
            [JsonProperty("Ширина колонки (0.0 - 1.0)")]
            public double ColumnWidth;
            [JsonProperty("Выравнивание (Left/Center/Right))")]
            public TextAnchor Anchor;

            [JsonProperty("Размер шрифта")]
            public int TextSize;
            [JsonProperty("Цвет тени текста (RGBA)")]
            public string OutlineColor;
            [JsonProperty("Строки текста")]
            public List<string> TextList = new List<string>();
        }

        [PluginReference] Plugin ImageLibrary;
        void OnServerInitialized()
        {
            LoadData();
            if (!ImageLibrary)
            {
                PrintError("Imagelibrary not found!");
                return;
            }

            IEnumerable<Images> images = from message in config.tabs from attachment in message.pages from image in attachment.Images select image;

            foreach (var image in images.ToList())
            {
                ImageLibrary?.Call("AddImage", image.URL, image.URL);
            }

            IEnumerable<Pages> MenuIcons = from message in config.tabs from attachment in message.pages select attachment;

            foreach (var icon in MenuIcons)
            {
                ImageLibrary?.Call("AddImage", icon.URL, icon.URL);
            }


            if (!string.IsNullOrEmpty(config.MenuBackgroundImage))
            {
                ImageLibrary?.Call("AddImage", config.MenuBackgroundImage, config.MenuBackgroundImage);

            }

            config.Commands.ForEach(c => cmd.AddChatCommand(c, this, cmdOpenInfoMenu));

            BasePlayer.activePlayerList.ToList().ForEach(OnPlayerConnected);

        }

		/*
        [ChatCommand("block")]
        void cmdBlockDraw(BasePlayer player, string command, string[] args)
        {
            CreateMenu(player, 3, 0);
           

        }*/

        void cmdOpenInfoMenu(BasePlayer player, string command, string[] args)
        {
            CreateMenu(player, 0, 0);
        }

        [PluginReference] Plugin WipeBlock;

        private string MainLayer = "InfoMenu_main";

        [ConsoleCommand("infomenu_selectpage")]
        void cmdSelectPage(ConsoleSystem.Arg args)
        {
            var player = args.Player();
            var type = args.GetInt(0);
            var page = args.GetInt(1);
            CreateMenu(player, type, page);
        }

        public Dictionary<ulong, PlayerClasses> PlayersList = new Dictionary<ulong, PlayerClasses>();

        public class PlayerClasses
        {
            public bool Firts = false;
        }

        void LoadData()
        {
            try
            {
                PlayersList = Interface.Oxide.DataFileSystem.ReadObject<Dictionary<ulong, PlayerClasses>>(Name);
            }
            catch
            {
                PlayersList = new Dictionary<ulong, PlayerClasses>();
            }
        }

        void SaveData()
        {
            if (PlayersList != null)
                Interface.Oxide.DataFileSystem.WriteObject(Name, PlayersList);
        }

        void Unload()
        {
            BasePlayer.activePlayerList.ToList().ForEach(player => CuiHelper.DestroyUi(player, MainLayer));
            SaveData();
        }


        void CreateMenu(BasePlayer player, int type, int page)
        {
            CuiHelper.DestroyUi(player, MainLayer);
            CuiElementContainer container = new CuiElementContainer();
            container.Add(new CuiPanel
            {
                CursorEnabled = true,
                Image =
                {
                    Color = ParseColorFromRGBA(config.MenuColor),
                     Material=  "Assets/Content/UI/UI.Background.Tile.psd",

                },
                RectTransform =
                {
                    AnchorMin = $"{1 - config.MenuAnchorWidth} {1- config.MenuAnchorHeight}",
                    AnchorMax = $"{config.MenuAnchorWidth} {config.MenuAnchorHeight}"
                }
            }, "Overlay", MainLayer);


            if (!string.IsNullOrEmpty(config.MenuBackgroundImage))
            {
                container.Add(new CuiElement()
                {
                    Parent = MainLayer,
                    Components = {
                            new CuiRawImageComponent {
                                Png = (string)ImageLibrary?.Call("GetImage", config.MenuBackgroundImage), FadeIn = 0.2f, Color = "1 1 1 1"
                            }
                            , new CuiRectTransformComponent {
                                AnchorMin=$"0 0", AnchorMax= $"1 1"
                            }
                           }
                }
                         );
            }

            container.Add(new CuiButton
            {
                Button =
                {
                    Close = MainLayer,
                    Color = "0 0 0 0"
                },
                Text =
                {
                    Text = ""
                },
                RectTransform =
                {
                    AnchorMin = "-100 -100",
                    AnchorMax = "100 100"
                }
            }, MainLayer);


            container.Add(new CuiPanel
            {
                Image =
                {
                    Color = ParseColorFromRGBA(config.SidebarColor),
                   Sprite=  "Assets/Content/UI/UI.Background.Tile.psd",
                },
                RectTransform =
                {
                    AnchorMin = $"0 0",
                    AnchorMax = $"{0 + config.SidebarWidth} 0.998"
                }
            }, MainLayer, $"{MainLayer}SideBar");




            double amin = 1 - config.TabTopIndent;
            double amax = 1 - config.TabTopIndent - config.TabHeight;

            int pages = 0;
            foreach (var button in config.tabs)
            {
                container.Add(new CuiButton
                {
                    Button =
                {
                    Command = $"infomenu_selectpage {pages}",
                    Color = config.tabs[type].Title == button.Title ? ParseColorFromRGBA(config.TabActiveColor) : ParseColorFromRGBA(config.TabColor), Material=  "Assets/Content/UI/UI.Background.Tile.psd",
                },
                    Text =
                {
                    Text = ""
                },
                    RectTransform =
                {
                    AnchorMin = $"{1- config.TabWidth} {amax}",
                    AnchorMax = $"{config.TabWidth} {amin}"
                }
                }, $"{MainLayer}SideBar", $"{MainLayer}Button{button.Title}");

                container.Add(new CuiElement
                {
                    Name = CuiHelper.GetGuid(),
                    Parent = $"{MainLayer}Button{button.Title}",
                    Components =
                            {
                                new CuiTextComponent { Text = $"{GetMsg(player, button.Title)}", FontSize = config.TabFontSize, Align = TextAnchor.MiddleCenter, Color = ParseColorFromRGBA(config.TabTextColor),Font="robotocondensed-bold.ttf",/* FadeIn = 0.1f */},
                                new CuiRectTransformComponent{ AnchorMin = "0 0", AnchorMax = "0.98 1" }
                            }
                });

                amin = amax - config.TabTopIndent;
                amax = amax - config.TabTopIndent - config.TabHeight;
                pages++;
            }

            container.Add(new CuiPanel
            {
                Image =
                {
                    Color = "0 0 0 0",
                },
                RectTransform =
                {
                    AnchorMin = $"{0 + config.SidebarWidth} 0",
                    AnchorMax = $"1 0.998"
                }
            }, MainLayer, $"{MainLayer}Info");


            var PanelInfo = config.tabs[type];

            var PageSelect = PanelInfo.pages[page];

            if (PageSelect.Images.Count > 0)
            {
                foreach (var image in PageSelect.Images)
                {
                    container.Add(new CuiElement()
                    {
                        Parent = $"{MainLayer}Info",
                        Components = {
                            new CuiRawImageComponent {
                                Png = (string)ImageLibrary?.Call("GetImage", image.URL), FadeIn = 0.2f, Color = "1 1 1 0.9"
                            }
                            , new CuiRectTransformComponent {
                                AnchorMin=$"{image.PositionHorizontal} {image.PositionVertical - image.Height}", AnchorMax= $"{image.PositionHorizontal + image.Width} {image.PositionVertical}"
                            },
                           }
                    }
                    );
                }
            }
            var dateTime = $"{SaveRestore.SaveCreatedTime.ToLocalTime().Day} {(Months)SaveRestore.SaveCreatedTime.ToLocalTime().Month}";
            foreach (var block in PageSelect.blocks)
            {
                foreach (var select in block.colums)
                {
                    var blockMin = block.Height < 0.5 ? 1 - block.Height : 0;
                    var blockMax = block.Height > 0.5 ? block.Height : 1 - 0.03;
                    for (int i = 0; i < select.TextList.Count; i++)
                    {
                        string text = GetMsg(player, select.TextList[i]).Replace("DATATIME",  dateTime).Replace("NICKNAME", player.displayName).Replace("CURONLINE",  BasePlayer.activePlayerList.Count.ToString()).Replace("QUEM", $"{ServerMgr.Instance.connectionQueue.Joining + ServerMgr.Instance.connectionQueue.Queued}");
                        container.Add(new CuiElement
                        {
                            Name = CuiHelper.GetGuid(),
                            Parent = $"{MainLayer}Info",
                            Components =
                            {
                                new CuiTextComponent { Text = $"{text}", FontSize = select.TextSize, Align = select.Anchor, Color = "1 1 1 1",Font="robotocondensed-bold.ttf" , FadeIn = 0.2f},
                                new CuiRectTransformComponent{ AnchorMin = $"0.01 {blockMin}", AnchorMax = $"{select.ColumnWidth} {blockMax}" },
                                new CuiOutlineComponent {Color = ParseColorFromRGBA(select.OutlineColor), Distance = "0.5 -0.5" }
                            }
                        });   
                    }
                }

            }
            if (!string.IsNullOrEmpty(PageSelect.URL))
            {
                float count = PanelInfo.pages.Count;
                var position = GetPositions(PanelInfo.pages.Count, 1, /*0.06f */count / 100, 0.001f);
                int pos = 0;
                foreach (var page1 in PanelInfo.pages)    
                {

                    container.Add(new CuiPanel
                    {
                        Image =
                {
                    Color = "0 0 0 0",
                   Material=  "Assets/Content/UI/UI.Background.Tile.psd",
                },
                        RectTransform =
                {
                    AnchorMin = $"0 0",
                    AnchorMax = $"1 0.13"
                }
                    }, $"{MainLayer}Info", $"{MainLayer}SideBarImages");
                }



                foreach (var page1 in PanelInfo.pages)
                {
                    container.Add(new CuiElement
                    {
                        Name = $"{MainLayer}SideBarImages{page1.URL}",
                        Parent = $"{MainLayer}SideBarImages",
                        Components = {
                        new CuiImageComponent {
                            Color ="0 0 0 0",
                        },
                          new CuiRectTransformComponent {
                            AnchorMin = position[pos].AnchorMin,
                            AnchorMax = position[pos].AnchorMax
                        },
                    },

                    });

                    var anchorMin = page == pos ? "0 0" : "0.2 0.2";
                    var anchorMax = page == pos ? "1 1" : "0.8 0.8";

                    var color = page == pos ? "1 1 1 0.9" : "1 1 1 0.3";

                    container.Add(new CuiElement()
                    {
                        Parent = $"{MainLayer}SideBarImages{page1.URL}",
                        Components = {
                            new CuiRawImageComponent {
                                Png = (string)ImageLibrary?.Call("GetImage", page1.URL), FadeIn = 0.2f, Color = color
                            }
                            , new CuiRectTransformComponent {
                                AnchorMin=$"{anchorMin}", AnchorMax= anchorMax
                            }
                            ,
                           }
                    }
                      );
                    container.Add(new CuiButton
                    {
                        Button =
                                {
                                    Command = $"infomenu_selectpage {type} {pos}",
                                    Color = "0 0 0 0" , Material=  "Assets/Content/UI/UI.Background.Tile.psd",
                                },
                        Text =
                                {
                                    Text = $"", FontSize = 25, Align = TextAnchor.MiddleCenter
                                },
                        RectTransform =
                                {
                                    AnchorMin = $"0 0",
                                    AnchorMax = $"1 1"
                                }
                    }, $"{MainLayer}SideBarImages{page1.URL}");
                    pos++;
                }
            }
            else
            if (PanelInfo.pages.Count > 1)
            {
                if (PanelInfo.pages.Count > page + 1)
                {
                    container.Add(new CuiButton
                    {
                        Button =
                {
                    Command = $"infomenu_selectpage {type} {page + 1}",
                    Color = "0 0 0 0" , Material=  "Assets/Content/UI/UI.Background.Tile.psd",
                },
                        Text =
                {
                    Text = $"{page}▶", FontSize = 25, Align = TextAnchor.MiddleCenter
                },
                        RectTransform =
                {
                    AnchorMin = $"0.9 0",
                    AnchorMax = $"0.99 0.1"
                }
                    }, $"{MainLayer}Info", $"buttonNext");
                }
                else
                {
                    container.Add(new CuiButton
                    {
                        Button =
                {
                    Command = $"infomenu_selectpage {type} {page - 1}",
                    Color = "0 0 0 0" , Material=  "Assets/Content/UI/UI.Background.Tile.psd",
                },
                        Text =
                {
                    Text = $"◀{page}", FontSize = 25, Align = TextAnchor.MiddleCenter
                },
                        RectTransform =
                {
                    AnchorMin = $"0.9 0",
                    AnchorMax = $"0.99 0.1"
                }
                    }, $"{MainLayer}Info", $"buttonPrev");
                }
            }

            container.Add(new CuiElement()
            {
                Parent = $"{MainLayer}SideBar",
                Components = {
                            new CuiTextComponent {
                                Color = "1 1 1 1", Text = $"Online: {BasePlayer.activePlayerList.Count + "/" + ConVar.Server.maxplayers}\nTIME: {covalence.Server.Time.ToShortTimeString()}", Align = TextAnchor.MiddleCenter, FontSize = 20,Font="robotocondensed-bold.ttf"
                            }
                            , new CuiRectTransformComponent {
                                AnchorMin=$"0.05 0", AnchorMax= "1 0.1"
                            }
                            ,
                           }
            }
                    );

            CuiHelper.AddUi(player, container);

            if (PanelInfo.Title.ToLower().Contains("bloquear"))
            {
                WipeBlock?.Call("DrawBlockGUI", player);
            }
        }



        public enum Months
        {
            January = 1,
            February = 2,
            March = 3,
            April = 4,
            May = 5,
            June = 6,
            July = 7,
            August = 8,
            September = 9,
            October = 10,
            November = 11,
            December = 12
        }

        #region Helper
        class Position
        {
            public float Xmin;
            public float Xmax;
            public float Ymin;
            public float Ymax;

            public string AnchorMin =>
                $"{Math.Round(Xmin, 4).ToString(CultureInfo.InvariantCulture)} {Math.Round(Ymin, 4).ToString(CultureInfo.InvariantCulture)}";
            public string AnchorMax =>
                $"{Math.Round(Xmax, 4).ToString(CultureInfo.InvariantCulture)} {Math.Round(Ymax, 4).ToString(CultureInfo.InvariantCulture)}";

            public override string ToString()
            {
                return $"----------\nAmin:{AnchorMin}\nAmax:{AnchorMax}\n----------";
            }
        }

        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
        private static List<Position> GetPositions(int colums, int rows, float colPadding = 0, float rowPadding = 0, bool columsFirst = false)
        {
            if (colums == 0)
                throw new ArgumentException("Can't create positions for gui!", nameof(colums));
            if (rows == 0)
                throw new ArgumentException("Can't create positions for gui!", nameof(rows));

            List<Position> result = new List<Position>();
            result.Clear();
            var colsDiv = 1f / colums;
            var rowsDiv = 1f / rows;
            if (colPadding == 0) colPadding = colsDiv / 2;
            if (rowPadding == 0) rowPadding = rowsDiv / 2;
            if (!columsFirst)
                for (int j = rows; j >= 1; j--)
                {
                    for (int i = 1; i <= colums; i++)
                    {
                        Position pos = new Position
                        {
                            Xmin = (i - 1) * colsDiv + colPadding / 2f,
                            Xmax = i * colsDiv - colPadding / 2f,
                            Ymin = (j - 1) * rowsDiv + rowPadding / 2f,
                            Ymax = j * rowsDiv - rowPadding / 2f
                        };
                        result.Add(pos);
                    }
                }
            else
                for (int i = 1; i <= colums; i++)
                {
                    for (int j = rows; j >= 1; j--)
                    {
                        Position pos = new Position
                        {
                            Xmin = (i - 1) * colsDiv + colPadding / 2f,
                            Xmax = i * colsDiv - colPadding / 2f,
                            Ymin = (j - 1) * rowsDiv + rowPadding / 2f,
                            Ymax = j * rowsDiv - rowPadding / 2f
                        };
                        result.Add(pos);
                    }
                }
            return result;
        }

        public string ParseColorFromRGBA(string cssColor)
        {
            cssColor = cssColor.Trim();
            string[] parts = cssColor.Split(' ');
            int r = int.Parse(parts[0], CultureInfo.InvariantCulture);
            int g = int.Parse(parts[1], CultureInfo.InvariantCulture);
            int b = int.Parse(parts[2], CultureInfo.InvariantCulture);
            float a = float.Parse(parts[3], CultureInfo.InvariantCulture);
            var finish = System.Drawing.Color.FromArgb((int)(a * 255), r, g, b);
            cssColor = "#" + finish.R.ToString("X2") + finish.G.ToString("X2") + finish.B.ToString("X2") + finish.A.ToString("X2");
            var str = cssColor.Trim('#');
            if (str.Length == 6)
                str += "FF";
            if (str.Length != 8)
            {
                throw new Exception(cssColor);
                throw new InvalidOperationException("Cannot convert a wrong format.");
            }
            var r1 = byte.Parse(str.Substring(0, 2), NumberStyles.HexNumber);
            var g1 = byte.Parse(str.Substring(2, 2), NumberStyles.HexNumber);
            var b1 = byte.Parse(str.Substring(4, 2), NumberStyles.HexNumber);
            var a1 = byte.Parse(str.Substring(6, 2), NumberStyles.HexNumber);
            Color color = new Color32(r1, g1, b1, a1);
            return string.Format("{0:F2} {1:F2} {2:F2} {3:F2}", color.r, color.g, color.b, color.a);
        }

        #endregion
        
        #region Lang

        protected override void LoadDefaultMessages()
        {
            foreach (var check in config.LangSettings)
            {
                lang.RegisterMessages(check.Value, this, check.Key);
            }
        }
        
        private string GetMsg(BasePlayer player, string msg) => string.Format(lang.GetMessage(msg, this, player.UserIDString));
        

        #endregion
    }
}
