// Fps_Unlock —— 控制器初始化（persistent，常驻所有房间）。
//
// 目标帧率来自模组菜单（设置 → MODS → Target FPS 下拉），
// 启动器的 o_msl_initializer 已在 START 房间从 msl_menu_mod.ini 读入挡位；
// 读不到或非法（含首装默认 "0"）时回退 {{FALLBACK_FPS}}。
var _target = {{FALLBACK_FPS}};

if (variable_global_exists("{{FPS_GLOBAL}}"))
{
    var _v = real(global.{{FPS_GLOBAL}});

    if (_v >= {{FALLBACK_FPS}} && _v <= 9999)
        _target = _v;
}

{{DISPLAY_RESET}}
game_set_speed(_target, gamespeed_fps);
