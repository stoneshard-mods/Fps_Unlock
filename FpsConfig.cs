namespace Fps_Unlock;

// 帧率解锁参数。改这里然后重编译即可，GML 侧的占位符由宿主在打补丁时填充。
// 注释统一用 //：打包器的 XML 文档写入有缺陷，/// 会导致 CS1569。
public static class FpsConfig
{
    // 下拉挡位（游戏内 设置 → MODS → Target FPS）。会自动持久化到
    // msl_menu_mod.ini，改挡位不用重编译。
    public static readonly int[] FpsGears = { 60, 90, 120, 180, 240 };

    // 回退帧率：ini/全局变量读不到或非法（含首装默认值 "0"）时使用。
    public const int FallbackFps = 60;

    // 右上角帧数显示的刷新间隔（秒）：下拉挡位与非法回退值。
    // 间隔越长数字越稳定，越短越实时。
    public static readonly double[] CounterRates = { 0.25, 0.5, 1, 2 };
    public const double FallbackCounterRate = 0.5;

    // true：打补丁时顺带关闭垂直同步（与原版高速调试模式
    // display_reset(display_aa, false) 一致，帧率不被显卡刷新率截断）。
    // false：完全不调用 display_reset，垂直同步交给游戏设置里的开关
    // （此时帧率会被刷新率截断，设置里开 vsync 时锁回刷新率）。
    public const bool ForceDisableVsync = true;
}
