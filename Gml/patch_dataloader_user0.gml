// Fps_Unlock：数据加载完成后创建常驻对象（启动时只执行到一次）
//   o_fps_unlock     —— 帧率控制器（挡位应用 + 每步校验）
//   o_fps_unlock_hud —— 右上角帧数覆盖层（Draw GUI，按 MODS 菜单开关显示）
instance_create_depth(-50, -50, 0, o_fps_unlock);
instance_create_depth(-50, -50, 0, o_fps_unlock_hud);
