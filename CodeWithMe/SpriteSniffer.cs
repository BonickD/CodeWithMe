using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Plugins;
using Oxide.Game.Rust.Cui;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("SpriteSniffer", "CASHR", "1.0.0")]
    internal class SpriteSniffer : RustPlugin
    {
        #region Static

        private string Layer = "SPRITESNIFFER.UI";

        private List<string> sprites = new List<string>
        {
            "assets/content/materials/highlight.png",
            "assets/content/textures/generic/fulltransparent.tga",
            "assets/content/ui/developer/developmentskin/devpanelbg.png",
            "assets/content/ui/developer/developmentskin/devtab-active.png",
            "assets/content/ui/developer/developmentskin/devtab-bright.png",
            "assets/content/ui/developer/developmentskin/devtab-normal.png",
            "assets/content/ui/facepunch-darkbg.png",
            "assets/content/ui/gameui/compass/alpha_mask.png",
            "assets/content/ui/gameui/compass/compass_strip.png",
            "assets/content/ui/gameui/compass/compass_strip_hd.png",
            "assets/content/ui/gameui/ui.crosshair.circle.png",
            "assets/content/ui/gameui/underlays/ui.damage.directional.normal.tga",
            "assets/content/ui/hypnotized.png",
            "assets/content/ui/menuui/rustlogo-blurred.png",
            "assets/content/ui/menuui/rustlogo-normal-transparent.png",
            "assets/content/ui/menuui/ui.loading.logo.tga",
            "assets/content/ui/menuui/ui.logo.big.png",
            "assets/content/ui/menuui/ui.menu.logo.png",
            "assets/content/ui/menuui/ui.menu.news.missingbackground.jpg",
            "assets/content/ui/menuui/ui.menu.rateus.background.png",
            "assets/content/ui/overlay_binocular.png",
            "assets/content/ui/overlay_bleeding.png",
            "assets/content/ui/overlay_bleeding_height.tga",
            "assets/content/ui/overlay_bleeding_normal.tga",
            "assets/content/ui/overlay_freezing.png",
            "assets/content/ui/overlay_goggle.png",
            "assets/content/ui/overlay_helmet_slit.png",
            "assets/content/ui/overlay_poisoned.png",
            "assets/content/ui/overlay_scope_1.png",
            "assets/content/ui/overlay_scope_2.png",
            "assets/content/ui/tiledpatterns/circles.png",
            "assets/content/ui/tiledpatterns/stripe_reallythick.png",
            "assets/content/ui/tiledpatterns/stripe_slight.png",
            "assets/content/ui/tiledpatterns/stripe_slight_thick.png",
            "assets/content/ui/tiledpatterns/stripe_thick.png",
            "assets/content/ui/tiledpatterns/stripe_thin.png",
            "assets/content/ui/tiledpatterns/swirl_pattern.png",
            "assets/content/ui/ui.background.gradient.psd",
            "assets/content/ui/ui.background.tile.psd",
            "assets/content/ui/ui.background.tiletex.psd",
            "assets/content/ui/ui.background.transparent.linear.psd",
            "assets/content/ui/ui.background.transparent.linearltr.tga",
            "assets/content/ui/ui.background.transparent.radial.psd",
            "assets/content/ui/ui.icon.rust.png",
            "assets/content/ui/ui.serverimage.default.psd",
            "assets/content/ui/ui.spashscreen.psd",
            "assets/content/ui/ui.white.tga",
            "assets/icons/add.png",
            "assets/icons/ammunition.png",
            "assets/icons/arrow_right.png",
            "assets/icons/authorize.png",
            "assets/icons/bite.png",
            "assets/icons/bleeding.png",
            "assets/icons/blueprint.png",
            "assets/icons/blueprint_underlay.png",
            "assets/icons/blunt.png",
            "assets/icons/bp-lock.png",
            "assets/icons/broadcast.png",
            "assets/icons/build/stairs.png",
            "assets/icons/build/wall.doorway.door.png",
            "assets/icons/build/wall.window.bars.png",
            "assets/icons/bullet.png",
            "assets/icons/cargo_ship_body.png",
            "assets/icons/cart.png",
            "assets/icons/change_code.png",
            "assets/icons/check.png",
            "assets/icons/chinook_map_blades.png",
            "assets/icons/chinook_map_body.png",
            "assets/icons/circle_closed.png",
            "assets/icons/circle_closed_toedge.png",
            "assets/icons/circle_gradient.png",
            "assets/icons/circle_gradient_white.png",
            "assets/icons/circle_open.png",
            "assets/icons/clear.png",
            "assets/icons/clear_list.png",
            "assets/icons/close.png",
            "assets/icons/close_door.png",
            "assets/icons/clothing.png",
            "assets/icons/cold.png",
            "assets/icons/community_servers.png",
            "assets/icons/connection.png",
            "assets/icons/construction.png",
            "assets/icons/cooking.png",
            "assets/icons/crate.png",
            "assets/icons/cup_water.png",
            "assets/icons/cursor-hand.png",
            "assets/icons/deauthorize.png",
            "assets/icons/demolish.png",
            "assets/icons/demolish_cancel.png",
            "assets/icons/demolish_immediate.png",
            "assets/icons/discord 1.png",
            "assets/icons/discord.png",
            "assets/icons/download.png",
            "assets/icons/drop.png",
            "assets/icons/drowning.png",
            "assets/icons/eat.png",
            "assets/icons/electric.png",
            "assets/icons/embrella.png",
            "assets/icons/enter.png",
            "assets/icons/examine.png",
            "assets/icons/exit.png",
            "assets/icons/explosion.png",
            "assets/icons/explosion_sprite.png",
            "assets/icons/extinguish.png",
            "assets/icons/facebook-box.png",
            "assets/icons/facebook.png",
            "assets/icons/facepunch.png",
            "assets/icons/fall.png",
            "assets/icons/favourite_servers.png",
            "assets/icons/file.png",
            "assets/icons/flags/af.png",
            "assets/icons/flags/ar.png",
            "assets/icons/flags/ca.png",
            "assets/icons/flags/cs.png",
            "assets/icons/flags/da.png",
            "assets/icons/flags/de.png",
            "assets/icons/flags/el.png",
            "assets/icons/flags/en-pt.png",
            "assets/icons/flags/en.png",
            "assets/icons/flags/es-es.png",
            "assets/icons/flags/fi.png",
            "assets/icons/flags/fr.png",
            "assets/icons/flags/he.png",
            "assets/icons/flags/hu.png",
            "assets/icons/flags/it.png",
            "assets/icons/flags/ja.png",
            "assets/icons/flags/ko.png",
            "assets/icons/flags/nl.png",
            "assets/icons/flags/no.png",
            "assets/icons/flags/pl.png",
            "assets/icons/flags/pt-br.png",
            "assets/icons/flags/pt-pt.png",
            "assets/icons/flags/ro.png",
            "assets/icons/flags/ru.png",
            "assets/icons/flags/sr.png",
            "assets/icons/flags/sv-se.png",
            "assets/icons/flags/tr.png",
            "assets/icons/flags/uk.png",
            "assets/icons/flags/vi.png",
            "assets/icons/flags/zh-cn.png",
            "assets/icons/flags/zh-tw.png",
            "assets/icons/fog.png",
            "assets/icons/folder.png",
            "assets/icons/folder_up.png",
            "assets/icons/fork_and_spoon.png",
            "assets/icons/freezing.png",
            "assets/icons/friends_servers.png",
            "assets/icons/gear.png",
            "assets/icons/grenade.png",
            "assets/icons/greyout.png",
            "assets/icons/greyout_large.png",
            "assets/icons/health.png",
            "assets/icons/history_servers.png",
            "assets/icons/home.png",
            "assets/icons/horse_ride.png",
            "assets/icons/hot.png",
            "assets/icons/ignite.png",
            "assets/icons/info.png",
            "assets/icons/inventory.png",
            "assets/icons/isbroken.png",
            "assets/icons/iscooking.png",
            "assets/icons/isloading.png",
            "assets/icons/isonfire.png",
            "assets/icons/joystick.png",
            "assets/icons/key.png",
            "assets/icons/knock_door.png",
            "assets/icons/lan_servers.png",
            "assets/icons/level.png",
            "assets/icons/level_metal.png",
            "assets/icons/level_stone.png",
            "assets/icons/level_top.png",
            "assets/icons/level_wood.png",
            "assets/icons/lick.png",
            "assets/icons/light_campfire.png",
            "assets/icons/lightbulb.png",
            "assets/icons/loading.png",
            "assets/icons/lock.png",
            "assets/icons/loot.png",
            "assets/icons/maparrow.png",
            "assets/icons/market.png",
            "assets/icons/maximum.png",
            "assets/icons/meat.png",
            "assets/icons/medical.png",
            "assets/icons/menu_dots.png",
            "assets/icons/modded_servers.png",
            "assets/icons/occupied.png",
            "assets/icons/open.png",
            "assets/icons/open_door.png",
            "assets/icons/peace.png",
            "assets/icons/pickup.png",
            "assets/icons/pills.png",
            "assets/icons/player_assist.png",
            "assets/icons/player_carry.png",
            "assets/icons/player_loot.png",
            "assets/icons/poison.png",
            "assets/icons/portion.png",
            "assets/icons/power.png",
            "assets/icons/press.png",
            "assets/icons/radiation.png",
            "assets/icons/rain.png",
            "assets/icons/reddit.png",
            "assets/icons/refresh.png",
            "assets/icons/resource.png",
            "assets/icons/rotate.png",
            "assets/icons/rust.png",
            "assets/icons/save.png",
            "assets/icons/shadow.png",
            "assets/icons/sign.png",
            "assets/icons/slash.png",
            "assets/icons/sleeping.png",
            "assets/icons/sleepingbag.png",
            "assets/icons/square.png",
            "assets/icons/square_gradient.png",
            "assets/icons/stab.png",
            "assets/icons/star.png",
            "assets/icons/steam.png",
            "assets/icons/steam_inventory.png",
            "assets/icons/stopwatch.png",
            "assets/icons/store.png",
            "assets/icons/study.png",
            "assets/icons/subtract.png",
            "assets/icons/target.png",
            "assets/icons/tools.png",
            "assets/icons/translate.png",
            "assets/icons/traps.png",
            "assets/icons/triangle.png",
            "assets/icons/tweeter.png",
            "assets/icons/twitter 1.png",
            "assets/icons/twitter.png",
            "assets/icons/unlock.png",
            "assets/icons/upgrade.png",
            "assets/icons/voice.png",
            "assets/icons/vote_down.png",
            "assets/icons/vote_up.png",
            "assets/icons/warning.png",
            "assets/icons/warning_2.png",
            "assets/icons/weapon.png",
            "assets/icons/web.png",
            "assets/icons/wet.png",
            "assets/icons/workshop.png",
            "assets/icons/xp.png"
        };
        
        #endregion

        #region Commands

        [ChatCommand("sprite")]
        private void cmdChatsprite(BasePlayer player, string command, string[] args)
        {
            ShowUISprites(player, args.Length > 0 ? int.Parse(args[0]) : 0);
        }

        [ConsoleCommand("UI_SHOWSPRITENAME")]
        private void cmdConsoleUI_SHOWSPRITENAME(ConsoleSystem.Arg arg)
        {
            var player = arg?.Player();
            if (player == null) return;
            player.ChatMessage(sprites[int.Parse(arg.Args[0])]);
        }
        
        #endregion

        private void ShowUISprites(BasePlayer player, int page)
        {
            var container = new CuiElementContainer();

            container.Add(new CuiPanel
            {
                CursorEnabled = true,
                RectTransform = {AnchorMin = "0.5 0.5", AnchorMax = "0.5 0.5", OffsetMin = "-630 -350", OffsetMax = "630 350"},
                Image = {Color = "0 0 0 0.8"}
            }, "Overlay", Layer);

            container.Add(new CuiButton
            {
                RectTransform = {AnchorMin = "0.5 0.5", AnchorMax = "0.5 0.5", OffsetMin = "-640 -360", OffsetMax = "640 360"},
                Button = {Color = "0 0 0 0", Close = Layer},
                Text = {Text = ""}
            }, Layer);
            int x = 0, y = 0;
            foreach (var check in sprites.Skip(144 * page).Take(144))
            {
                container.Add(new CuiButton
                {
                    RectTransform = {AnchorMin = $"{0 + 0.0625 * x} {0.888 - 0.1111 * y}", AnchorMax = $"{0.06 + 0.0625 * x} {1 - 0.1111 * y}"},
                    Button = {Color = "1 1 1 1", Command = $"UI_SHOWSPRITENAME {sprites.IndexOf(check)}", Sprite = check},
                    Text = {Text = ""}
                }, Layer);
                x++;
                if (x != 16) continue;
                x = 0;
                y++;
            }

            CuiHelper.DestroyUi(player, Layer);
            CuiHelper.AddUi(player, container);
        }
        
    }
}