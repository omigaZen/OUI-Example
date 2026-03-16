# OUI Example 测试说明

## 已实现的功能

### 1. EditorWindowAssetLoader
- 使用 `UnityEditor.AssetDatabase.LoadAssetAtPath` 加载资源
- 仅在 Editor 环境下可用
- 实现了 `IWindowAssetLoader` 接口

### 2. 测试窗口类

#### TestMainWindow (UI层)
- **层级**: WindowLayer.UI
- **HideBelow**: true (会遮挡下层)
- **功能点**:
  - 参数传递演示
  - 打开其他窗口
  - OnHideByAboveChanged 回调演示

#### TestSettingsWindow (UI层)
- **层级**: WindowLayer.UI
- **HideBelow**: true
- **功能点**:
  - UI组件绑定 (Toggle, Slider, Button)
  - 参数接收
  - 事件监听

#### TestBackgroundWindow (Bottom层)
- **层级**: WindowLayer.Bottom
- **HideBelow**: false (不遮挡下层)
- **功能点**:
  - 最底层窗口
  - OnHideByAboveChanged 状态显示
  - 简单颜色动画

#### TestTipsWindow (Tips层)
- **层级**: WindowLayer.Tips
- **HideBelow**: false
- **功能点**:
  - 高层级窗口
  - 自动关闭 (3秒)
  - 不遮挡下层窗口

#### TestTopWindow (Top层)
- **层级**: WindowLayer.Top
- **HideBelow**: false
- **功能点**:
  - Top层级演示
  - 子Canvas深度管理
  - Toggle交互

#### TestSystemWindow (System层)
- **层级**: WindowLayer.System (最高层)
- **HideBelow**: true
- **功能点**:
  - 窗口栈信息显示
  - 关闭所有窗口功能
  - 实时更新窗口状态

### 3. TestLauncher
- 初始化 OUI 系统
- 注入 EditorWindowAssetLoader
- 提供测试按钮快速打开各个窗口

## 覆盖的 OUI 功能点

1. ✅ 窗口层级管理 (Bottom, UI, Top, Tips, System)
2. ✅ HideBelow 遮挡机制
3. ✅ 窗口栈管理
4. ✅ 参数传递 (OnOpen)
5. ✅ 生命周期回调 (BindUI, Init, OnOpen, OnClose, Release)
6. ✅ OnHideByAboveChanged 回调
7. ✅ 窗口深度自动排序
8. ✅ 子Canvas深度管理
9. ✅ 窗口重复打开检测
10. ✅ 关闭窗口/关闭所有窗口
11. ✅ 获取窗口栈信息

## 使用方法

1. 在 Unity 中创建对应的 Prefab 文件到 `Assets/Example/Prefabs/` 目录
2. 每个 Prefab 需要包含:
   - Canvas 组件
   - GraphicRaycaster 组件
   - 对应的 Window 脚本组件
   - 所需的 UI 子节点 (按照 FindChild/FindChildComponent 的路径)
3. 在场景中添加 TestLauncher 脚本并绑定按钮
4. 运行场景测试

## Prefab 结构参考

### TestMainWindow.prefab
```
- Canvas (Canvas, GraphicRaycaster, TestMainWindow)
  - Title (Text)
  - OpenSettingsBtn (Button)
  - OpenTipsBtn (Button)
  - CloseBtn (Button)
```

### TestSettingsWindow.prefab
```
- Canvas (Canvas, GraphicRaycaster, TestSettingsWindow)
  - InfoText (Text)
  - SoundToggle (Toggle)
  - VolumeSlider (Slider)
  - CloseBtn (Button)
```

### TestTipsWindow.prefab
```
- Canvas (Canvas, GraphicRaycaster, TestTipsWindow)
  - MessageText (Text)
  - OkBtn (Button)
```

### TestBackgroundWindow.prefab
```
- Canvas (Canvas, GraphicRaycaster, Image, TestBackgroundWindow)
  - StatusText (Text)
```

### TestTopWindow.prefab
```
- Canvas (Canvas, GraphicRaycaster, TestTopWindow)
  - Title (Text)
  - CloseBtn (Button)
  - HideToggle (Toggle)
  - ChildCanvas (Canvas)
    - ChildText (Text)
```

### TestSystemWindow.prefab
```
- Canvas (Canvas, GraphicRaycaster, TestSystemWindow)
  - InfoText (Text)
  - CloseAllBtn (Button)
  - CloseBtn (Button)
  - ScrollView (ScrollRect)
    - Viewport
      - Content
        - StackInfoText (Text)
```
