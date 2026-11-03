// Fps_Unlock —— 右上角帧数覆盖层（Draw GUI，GUI 坐标系，与分辨率/缩放无关）。
//
// 开关与刷新间隔来自 MODS 菜单（msl_menu_mod.ini 持久化）；
// 显示值按"Counter Refresh"间隔节流更新（current_time 毫秒计时），
// 间隔内重复使用上次采样，数字不抖；
// 字体用游戏自带的 global.f_digits（数字字体，scr_fontsUpdate 开机初始化）；
// 阴影 + 白字两遍绘制，结束时恢复对齐/颜色/字体，避免污染后续绘制。
if (!variable_global_exists("{{SHOW_FPS_GLOBAL}}") || global.{{SHOW_FPS_GLOBAL}} <= 0.5)
    exit;

if (!variable_global_exists("f_digits"))
    exit;

// 刷新间隔：菜单下拉（秒），非法（含首装默认 "0"）时回退 0.5
var _interval = 0.5;

if (variable_global_exists("{{COUNTER_RATE_GLOBAL}}"))
{
    var _r = real(global.{{COUNTER_RATE_GLOBAL}});

    if (_r >= 0.1 && _r <= 10)
        _interval = _r;
}

if ((current_time - lastUpdate) >= (_interval * 1000))
{
    shown = round(fps_real);
    lastUpdate = current_time;
}

var _previousFont = draw_get_font();
var _previousColour = draw_get_colour();
var _previousHalign = draw_get_halign();

draw_set_font(global.f_digits);
draw_set_halign(fa_right);
draw_set_colour(c_black);
draw_text(display_get_gui_width() - 9, 9, string(shown) + " FPS");
draw_set_colour(c_white);
draw_text(display_get_gui_width() - 10, 8, string(shown) + " FPS");

draw_set_halign(_previousHalign);
draw_set_colour(_previousColour);
draw_set_font(_previousFont);
