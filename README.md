# Fps_Unlock

Stoneshard 帧率解锁模组：原版把游戏速度锁在 40 FPS，本模组改为常驻控制器把游戏速度钉在目标帧率（默认 60）。

- 版本：v1.0.0.0
- 依赖：ModShardLauncher + 游戏原版 `data.win`

---

## 原版 40 帧从哪来

| 证据 | 位置 |
|---|---|
| `game_set_speed(40, gamespeed_fps)` 作为"恢复正常速度"的恢复值 | `o_dialogue_auto_test` Step / Destroy（调试自动测试对象，正式版不创建） |
| `game_set_speed(9999, gamespeed_fps)` 是它的"快进模式" | 同上 |
| 房间自带的 `Speed` 全部是 0 | `data.win` 房间数据（本作不用房间帧率） |
| `settings.ini` 没有 fps 选项 | 只有 resolution / vsync / displayMode 等 |

结论：40 是引擎配置里的默认游戏速度，运行时用 `game_set_speed` 即可改写，与官方自己的高速调试模式同一机制。

## 为什么解锁是安全的

本作是**回合制**游戏，帧率敏感的东西只有：

| 项 | 受影响？ | 说明 |
|---|---|---|
| 世界时钟（昼夜、饥饿推进） | 否 | 按"回合"推进：`scr_global_turn` → 每 +30 游戏分钟，回合由玩家行动触发 |
| 战斗/状态/冷却计时 | 否 | 走回合制 |
| 用 `N * room_speed` 写的定时器 | 否 | `room_speed` 跟随游戏速度，自动换算，原版大量使用 |
| 逐帧动画（`image_speed`） | 是（视觉） | 60 帧下播放速度快 1.5 倍，更跟手；原版调试模式 9999 帧跑过全对话测试 |
| 按"步数"硬编码的计时器 | 是（多为 UI/特效） | 同上 |

## 实现

- 新建 persistent 常驻对象 `o_fps_unlock`：
  - Create / Step：读取菜单挡位 `global.mslFpsUnlockTarget`（字符串 "60"/"90"/...），非法时回退 `FallbackFps`，然后 `game_set_speed(挡位, gamespeed_fps)`
  - Step 每步校验：菜单换挡下一帧生效；速度被调试对象/其他模组改回时拉回当前挡位
- **游戏内菜单**：`Msl.AddMenu` 在 设置 → MODS 标签页注册三个控件：
  - "Target FPS" 下拉框（挡位 60/90/120/180/240）
  - "Show FPS" 开关（CheckBox，默认关）：控制右上角帧数覆盖层
  - "Counter Refresh" 下拉框（0.25/0.5/1/2 秒）：帧数显示的刷新间隔
  - `o_msl_initializer`（START 房间）启动时从 `msl_menu_mod.ini` 读入全部配置（首装默认 "0" → 挡位回退 60、覆盖层隐藏、间隔回退 0.5s）
  - 菜单里改动立即改写全局变量，关闭设置时写回 `msl_menu_mod.ini`（`%LOCALAPPDATA%\StoneShard\`）
- **右上角帧数覆盖层**：persistent 对象 `o_fps_unlock_hud` 的 Draw GUI（事件 64）：
  - `fps_real` 取整后右对齐绘制在 `display_get_gui_width() - 10, 8`（GUI 坐标系，随分辨率自适应）
  - 显示值按 "Counter Refresh" 间隔节流（`current_time` 毫秒计时），间隔内沿用上次采样，数字不抖
  - 字体用游戏自带 `global.f_digits`，黑影 + 白字两遍绘制，结束后恢复对齐/颜色/字体
- 启动注入点：`o_dataLoader` 的 User Event 0（`gml_Object_o_dataLoader_Other_10`，启动时只跑一次），在其 `global.timeScale = 1;` 锚点后插入 `instance_create_depth(-50, -50, 0, o_fps_unlock);`
- 不使用 `Msl.AddFunction`（会让汇编器往 `Data.Functions` 塞自定义条目，游戏调用即闪退），全部逻辑内联在对象事件里

## 配置

改 `FpsConfig.cs` 后用启动器重新编译：

| 常量/字段 | 默认 | 说明 |
|---|---|---|
| `FpsGears` | `{ 60, 90, 120, 180, 240 }` | MODS 菜单下拉挡位 |
| `FallbackFps` | `60` | ini/全局变量读不到或非法时的回退帧率 |
| `CounterRates` | `{ 0.25, 0.5, 1, 2 }` | 帧数显示刷新间隔（秒）下拉挡位 |
| `FallbackCounterRate` | `0.5` | 刷新间隔非法时的回退值 |
| `ForceDisableVsync` | `true` | `true` 关垂直同步（帧率不被刷新率截断）；`false` 不碰 vsync，交给游戏设置 |

## 使用

```powershell
# 编译打包（产物：%LOCALAPPDATA%\ModShardLauncher\Mods\Fps_Unlock.sml）
ModShardLauncher.CLI.exe compile --source "%LOCALAPPDATA%\ModShardLauncher\ModSources\Fps_Unlock"

# 打补丁（用原版 data.win，不要用打过的）
ModShardLauncher.CLI.exe patch --data "<游戏目录>\data.win" --output "<游戏目录>\data_patched.win" --enable Fps_Unlock
```

然后用打出来的数据文件进游戏（备份原 `data.win` 后替换，或按启动提示选择数据文件）。

## 目录结构

```text
Fps_Unlock/
├─ FpsUnlockMod.cs          # Mod 主类（补丁类，编排入口）
├─ FpsPatcher.cs            # 注入实现：控制器对象 + 启动 hook
├─ FpsConfig.cs             # 目标帧率 / vsync 配置
├─ FpsTargets.cs            # 注入目标与锚点
├─ Gml/
│  ├─ o_fps_unlock_Create.gml
│  ├─ o_fps_unlock_Step.gml
│  ├─ o_fps_unlock_hud_Create.gml
│  ├─ o_fps_unlock_hud_Draw_64.gml
│  └─ patch_dataloader_user0.gml
├─ lib/                     # 宿主 DLL（IDE 编译用，打包不携带）
├─ icon.png
└─ Fps_Unlock.csproj        # 打包器不读，仅供 IDE
```
