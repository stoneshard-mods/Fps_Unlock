// Fps_Unlock —— 帧数覆盖层初始化：显示值节流状态。
// lastUpdate 用 current_time（毫秒），初值取 -1e9 保证第一帧立即刷新。
lastUpdate = -1000000000;
shown = 0;
