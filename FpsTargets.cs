namespace Fps_Unlock;

// 注入目标清单：宿主代码名、锚点与产物对象名。
// 锚点取自游戏反编译源（stoneshard-assets/codes），按"注入点"职责集中放在这里。
internal static class FpsTargets
{
    // 新建的常驻控制器对象名。
    public const string ControllerObject = "o_fps_unlock";

    // 启动注入点：o_dataLoader 的 User Event 0（本作导出命名里 Other_N == User Event N-10，
    // 由 o_gameLoader 创建 dataLoader 后 alarm[0] → event_user(0) 触发，启动时只跑一次，
    // 跑完 o_gameLoader 检测到实例销毁才放行主菜单）。
    public const string DataLoaderUserCode = "gml_Object_o_dataLoader_Other_10";

    // dataLoader User Event 0 里的定位锚点（按优先级逐个尝试）。
    // global.timeScale = 1 是初始化段中段的一行，前后都是纯赋值，反编译结果稳定。
    public static readonly string[] DataLoaderAnchors =
    {
        "global.timeScale = 1",
        "global.mainMenuRoom = 6",
        "global.attack_mode_on = false"
    };

    // 模组菜单（设置 → MODS 标签页）：ini 节名 = 菜单里显示的小节标题。
    public const string MenuSection = "Fps_Unlock";

    // 帧率挡位绑定的全局变量。ComboBox 的值是字符串（"60"/"90"/...），
    // 由启动器的 o_msl_initializer 在 START 房间从 msl_menu_mod.ini 读入，
    // 菜单里换挡后 Other_11 立即改写该变量，控制器 Step 每步跟随。
    public const string FpsGlobal = "mslFpsUnlockTarget";

    // 帧数显示开关（CheckBox → real 0/1），同样由 MODS 菜单持久化。
    public const string ShowFpsGlobal = "mslFpsUnlockShowCounter";

    // 帧数显示刷新间隔（秒），ComboBox 字符串值（"0.25"/"0.5"/"1"/"2"）。
    public const string CounterRateGlobal = "mslFpsUnlockCounterRate";

    // 右上角帧数覆盖层对象（Draw GUI 事件）。
    public const string HudObject = "o_fps_unlock_hud";

    // GML 里的目标帧率占位符。
    public const string FpsPlaceholder = "{{TARGET_FPS}}";

    // GML 里的回退帧率占位符（对应 FpsConfig.FallbackFps）。
    public const string FallbackFpsPlaceholder = "{{FALLBACK_FPS}}";

    // GML 里的全局变量名占位符。
    public const string FpsGlobalPlaceholder = "{{FPS_GLOBAL}}";

    // GML 里的帧数显示开关全局变量名占位符。
    public const string ShowFpsGlobalPlaceholder = "{{SHOW_FPS_GLOBAL}}";

    // GML 里的帧数显示刷新间隔全局变量名占位符。
    public const string CounterRateGlobalPlaceholder = "{{COUNTER_RATE_GLOBAL}}";

    // GML 里的 display_reset 整行占位符。填充结果：
    // ForceDisableVsync = true  → display_reset(display_aa, false);（关 vsync）
    // ForceDisableVsync = false → 注释行（不碰垂直同步，交给游戏设置）。
    public const string DisplayResetPlaceholder = "{{DISPLAY_RESET}}";
}
