// Fps_Unlock —— 每步校验：
// 1) 菜单换挡（Other_11 改写 global）后下一帧自动生效；
// 2) 速度被其他模组/调试对象改回时拉到当前挡位；
// 3) global 缺失或非法（含首装默认 "0"）时回退 {{FALLBACK_FPS}}。
var _target = {{FALLBACK_FPS}};

if (variable_global_exists("{{FPS_GLOBAL}}"))
{
    var _v = real(global.{{FPS_GLOBAL}});

    if (_v >= {{FALLBACK_FPS}} && _v <= 9999)
        _target = _v;
}

if (game_get_speed(gamespeed_fps) != _target)
{
    {{DISPLAY_RESET}}
    game_set_speed(_target, gamespeed_fps);
}
