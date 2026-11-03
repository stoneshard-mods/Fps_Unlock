using System;
using ModShardLauncher;
using ModShardLauncher.Mods;
using UndertaleModLib.Models;

namespace Fps_Unlock;

// GML 注入实现：新建常驻控制器对象、挂 Create/Step 事件、
// 在 MODS 菜单里注册帧率下拉框、并把"创建控制器"一行插进启动初始化代码。
// 不走 Msl.AddFunction —— 那条路会让汇编器往 Data.Functions 里塞自定义函数条目，
// 游戏调用到这种条目上会直接闪退（Quick_Equip 实测结论）。
// 注：打包器的 XML 文档写入有缺陷，注释一律用 //，不用 ///。
internal static class FpsPatcher
{
    public static void Apply(Mod mod)
    {
        RegisterMenu();
        CreateController(mod);
        CreateHud(mod);
        HookBootLoader(mod);
    }

    // 设置 → MODS 标签页注册"Target FPS"下拉框与"Show FPS"开关。
    // AddMenu 会创建 o_msl_initializer（START 房间，启动时从 msl_menu_mod.ini
    // 读入挡位与开关），并在菜单关闭时把 global 写回 ini（启动器的 Other_12/13 流程）。
    private static void RegisterMenu()
    {
        Msl.AddMenu(
            FpsTargets.MenuSection,
            new UIComponent(
                "Target FPS",
                FpsTargets.FpsGlobal,
                UIComponentType.ComboBox,
                Gears()),
            new UIComponent(
                "Show FPS",
                FpsTargets.ShowFpsGlobal,
                UIComponentType.CheckBox,
                0),
            new UIComponent(
                "Counter Refresh",
                FpsTargets.CounterRateGlobal,
                UIComponentType.ComboBox,
                CounterRates()));
    }

    // 控制器：persistent 常驻所有房间；Create 立即生效，Step 每步校验。
    // 目标帧率来自菜单全局变量（字符串挡位），非法时回退 FallbackFps；
    // 菜单换挡后 global 被改写，Step 校验在下一帧自动生效。
    private static void CreateController(Mod mod)
    {
        Msl.AddObject(
            FpsTargets.ControllerObject,
            spriteName: "",
            parentName: "",
            isVisible: false,
            isPersistent: true,
            isAwake: true);

        Msl.AddNewEvent(
            FpsTargets.ControllerObject,
            Gml(mod, "o_fps_unlock_Create.gml"),
            EventType.Create, 0);

        Msl.AddNewEvent(
            FpsTargets.ControllerObject,
            Gml(mod, "o_fps_unlock_Step.gml"),
            EventType.Step, 0);
    }

    // 帧数覆盖层：persistent，Draw GUI（事件 64）在屏幕右上角画实时帧数。
    // visible 必须为 true —— GameMaker 里 visible=false 的实例不跑 Draw 事件。
    private static void CreateHud(Mod mod)
    {
        Msl.AddObject(
            FpsTargets.HudObject,
            spriteName: "",
            parentName: "",
            isVisible: true,
            isPersistent: true,
            isAwake: true);

        Msl.AddNewEvent(
            FpsTargets.HudObject,
            Gml(mod, "o_fps_unlock_hud_Create.gml"),
            EventType.Create, 0);

        Msl.AddNewEvent(
            FpsTargets.HudObject,
            Gml(mod, "o_fps_unlock_hud_Draw_64.gml"),
            EventType.Draw, 64);
    }

    // 把创建控制器的语句插进启动初始化代码（只在游戏启动时执行一次）。
    private static void HookBootLoader(Mod mod)
    {
        string spawnCode = Gml(mod, "patch_dataloader_user0.gml");
        InvalidOperationException lastMiss = null;

        foreach (string anchor in FpsTargets.DataLoaderAnchors)
        {
            try
            {
                Msl.LoadGML(FpsTargets.DataLoaderUserCode)
                    .MatchFrom(anchor)
                    .InsertBelow(spawnCode)
                    .Save();
                return;
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("MatchFrom"))
            {
                // 只是没找到锚点，换下一个候选；GML 编译错误会以别的异常直接抛出。
                lastMiss = ex;
            }
        }

        throw new InvalidOperationException(
            "Fps_Unlock 无法定位启动注入点：" + FpsTargets.DataLoaderUserCode, lastMiss);
    }

    // 挡位转字符串数组（不用 LINQ，打包器只随附 netstandard 基础 API）。
    private static string[] Gears()
    {
        string[] gears = new string[FpsConfig.FpsGears.Length];

        for (int i = 0; i < FpsConfig.FpsGears.Length; i++)
            gears[i] = FpsConfig.FpsGears[i].ToString();

        return gears;
    }

    // 刷新间隔转字符串数组；用 InvariantCulture 保证小数点不是逗号。
    private static string[] CounterRates()
    {
        string[] rates = new string[FpsConfig.CounterRates.Length];

        for (int i = 0; i < FpsConfig.CounterRates.Length; i++)
            rates[i] = FpsConfig.CounterRates[i]
                .ToString(System.Globalization.CultureInfo.InvariantCulture);

        return rates;
    }

    // 读包内 GML 并填充配置占位符。
    private static string Gml(Mod mod, string fileName)
    {
        string displayResetLine = FpsConfig.ForceDisableVsync
            ? "display_reset(display_aa, false);"
            : "// 垂直同步交给游戏设置（FpsConfig.ForceDisableVsync = false）";

        return mod.ModFiles.GetCode(fileName)
            .Replace(FpsTargets.FallbackFpsPlaceholder, FpsConfig.FallbackFps.ToString())
            .Replace(FpsTargets.FpsGlobalPlaceholder, FpsTargets.FpsGlobal)
            .Replace(FpsTargets.ShowFpsGlobalPlaceholder, FpsTargets.ShowFpsGlobal)
            .Replace(FpsTargets.CounterRateGlobalPlaceholder, FpsTargets.CounterRateGlobal)
            .Replace(FpsTargets.DisplayResetPlaceholder, displayResetLine);
    }
}
