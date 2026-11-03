using ModShardLauncher.Mods;

namespace Fps_Unlock;

public class FpsUnlockMod : Mod
{
    public override string Name => "Fps_Unlock";
    public override string Author => "Pixel32E-QQ3566723459";
    public override string Description => "解锁帧率上限：原版锁 40 帧。游戏内 设置 → MODS → Target FPS 下拉换挡（60/90/120/180/240，自动持久化），默认 60。回合制核心逻辑与世界时钟按回合推进，不受影响";
    public override string ShortDesc => "Unlock the 40 FPS cap";
    public override string Version => "v1.3.0.0";

    public override void PatchMod()
    {
        FpsPatcher.Apply(this);
    }
}
