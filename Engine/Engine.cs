﻿using System;
using System.Collections.Generic;

namespace Altseed2
{
    /// <summary>
    /// Altseed2 のエンジンを表します。
    /// </summary>
    public static class Engine
    {
        /// <summary>
        /// カメラグループの個数の最大値
        /// </summary>
        internal const int MaxCameraGroupCount = 64;

        internal const int MaxStackalloclLength = 16;

        /// <summary>
        /// ルートノード
        /// </summary>
        private static RootNode _RootNode;

        /// <summary>
        /// 実際のUpdate対象のノード
        /// </summary>
        /// <remarks>Pause中は一部のノードのみが更新対象になる。</remarks>
        internal static Node _UpdatedNode;

        internal static CameraNodeCollection _CameraNodes;
        internal static Camera3DNodeCollection _Camera3DNodes;
        internal static RenderedCamera _DefaultCamera;
        internal static RenderedCamera3D _DefaultCamera3D;
        internal static DrawnCollection _DrawnCollection;
        internal static Drawn3DCollection _Drawn3DCollection;

        internal static RenderTextureCache _RenderTextureCache;
        internal static RenderTexture _PostEffectBuffer; // TODO: 渡し方をうまくやる。

        // cullingの結果を格納するためのBuffer
        private static ArrayBuffer<int> _DrawingRenderedIdsBuffer;

        // ShapeNodeのSetVertexesで利用するBuffer。
        internal static ArrayBuffer<Vector2F> Vector2FBuffer { get; private set; }

        // ShapeNodeのSetVertexesで利用するBuffer。
        internal static ArrayBuffer<Vertex> VertexBuffer { get; private set; }

        // CreateVertexesByVector2F時に一時的なArrayを生成せずに使い回すためのキャッシュ。
        internal static Vector2FArray Vector2FArrayCache { get; private set; }

        /// <summary>
        /// スクリーンのクリア色を取得または設定します。
        /// </summary>
        public static Color ClearColor
        {
            get => _ClearColor;
            set
            {
                if (_ClearColor == value) return;
                _ClearColor = value;

                if (_DefaultCamera is null || _DefaultCamera3D is null)
                {
                    Log.Warn(LogCategory.Engine, "Graphics機能が初期化されていません。");
                    return;
                }

                _DefaultCamera.RenderPassParameter = new RenderPassParameter(value, false, false);
                _DefaultCamera3D.RenderPassParameter = new RenderPassParameter(value, false, false);
            }
        }
        private static Color _ClearColor = new Color(50, 50, 50, 255);

        /// <summary>
        /// エンジンを初期化します。
        /// </summary>
        /// <param name="title">ウィンドウタイトル</param>
        /// <param name="width">ウィンドウの横幅</param>
        /// <param name="height">ウィンドウの縦幅</param>
        /// <param name="config">設定</param>
        /// <returns>初期化に成功したらtrue、それ以外でfalse</returns>
        public static bool Initialize(string title, int width, int height, Configuration config = null)
        {
            Config = (config ??= new Configuration());
            if (Core.Initialize(title, width, height, config))
            {
                Context = new AltseedContext();
                System.Threading.SynchronizationContext.SetSynchronizationContext(Context);

                _core = Core.GetInstance();
                _log = Log.GetInstance();
                _keyboard = Keyboard.GetInstance();
                _mouse = Mouse.GetInstance();
                _joystick = Joystick.GetInstance();
                _file = File.GetInstance();
                _resources = Resources.GetInstance();
                _window = Window.GetInstance();
                _graphics = Graphics.GetInstance();
                _renderer = Renderer.GetInstance();
                _renderer3D = Renderer3D.GetInstance();
                _cullingSystem = CullingSystem.GetInstance();
                _tool = Tool.GetInstance();
                _sound = SoundMixer.GetInstance();
                _profiler = Profiler.GetInstance();
                _collision3DWorld = Collision3DWorld.GetInstance();

                _RootNode = new RootNode();
                _UpdatedNode = _RootNode;

                _WindowSize = default;

                if (_graphics != null)
                {
                    _DrawnCollection = new DrawnCollection();
                    _Drawn3DCollection = new Drawn3DCollection();
                    _CameraNodes = new CameraNodeCollection();
                    _Camera3DNodes = new Camera3DNodeCollection();
                    _RenderTextureCache = new RenderTextureCache();

                    _DefaultCamera = RenderedCamera.Create();
                    _DefaultCamera.RenderPassParameter = new RenderPassParameter(ClearColor, false, false);
                    _DefaultCamera3D = RenderedCamera3D.Create();
                    _DefaultCamera3D.RenderPassParameter = new RenderPassParameter(ClearColor, false, false);

                    _DrawingRenderedIdsBuffer = new ArrayBuffer<int>();
                    Vector2FBuffer = new ArrayBuffer<Vector2F>(MaxStackalloclLength * 2);
                    VertexBuffer = new ArrayBuffer<Vertex>(MaxStackalloclLength * 2);
                    Vector2FArrayCache = Vector2FArray.Create(0);

                    PostEffectNode.InitializeCache();
                }

                isActive = true;

                return true;
            }
            return false;
        }

        /// <summary>
        /// システムイベントを処理します。
        /// </summary>
        public static bool DoEvents()
        {
            _graphics?.DoEvents();
            if (!Core.GetInstance().DoEvent()) return false;

            if (_tool != null)
            {
                //ツール機能を使用するときはDoEventsでフレームを開始
                //使用しないときはUpdateでフレームを開始
                if (!_graphics.BeginFrame(new RenderPassParameter(ClearColor, true, true))) return false;
                _tool.NewFrame();
            }

            return true;
        }

        /// <summary>
        /// エンジンを更新します。
        /// </summary>
        public static bool Update()
        {
            if (_WindowSize != Window.Size) UpdateWindowSize();

            if (_CameraNodes != null && _Camera3DNodes != null)
            {
                var anyCamera = _CameraNodes.Count != 0;
                var anyCamera3D = _Camera3DNodes.Count != 0;
                return UpdateComponents(!anyCamera, anyCamera, !anyCamera3D, anyCamera3D);
            }

            return true;
        }

        internal static bool UpdateComponents(bool drawDefaultCameraGroup, bool drawCustomCameraGroup, bool drawDefaultCamera3DGroup, bool drawCustomCamera3DGroup)
        {
            // ノードの更新
            _UpdatedNode?.Update();

            // Contextの更新
            Context.Update();

            // Graphicsが初期化されていない場合は早期リターン
            if (_graphics == null) return true;

            // カリング用AABBの更新
            _cullingSystem?.UpdateAABB();

            // (ツール機能を使用しない場合は)描画を開始
            if (_tool == null)
            {
                //ツール機能を使用するときはDoEventsでフレームを開始
                //使用しないときはUpdateでフレームを開始
                if (!_graphics.BeginFrame(new RenderPassParameter(ClearColor, true, true))) return false;
            }

            if (drawDefaultCamera3DGroup)
            {
                DrawCamera3DGroup(_DefaultCamera3D, _Drawn3DCollection.GetDrawns());
                ResetCurrentCamera();
            }

            if (drawDefaultCameraGroup)
            {
                // カメラが 1 つもない場合はデフォルトカメラを使用
                DrawCameraGroup(_DefaultCamera, _DrawnCollection.GetDrawns());
                ResetCurrentCamera();
            }

            // 特定のカメラに映りこむノードを描画
            for (int i = 0; i < MaxCameraGroupCount; i++)
            {
                if (drawCustomCamera3DGroup)
                {
                    foreach (var camera in _Camera3DNodes[i])
                    {
                        DrawCamera3DGroup(camera.RenderedCamera, _Drawn3DCollection[i]);
                        ResetCurrentCamera();
                    }
                }
                if (drawCustomCameraGroup)
                {
                    foreach (var camera in _CameraNodes[i])
                    {
                        DrawCameraGroup(camera.RenderedCamera, _DrawnCollection[i]);
                        ResetCurrentCamera();
                    }
                }
            }

            _RenderTextureCache.Update();
            PostEffectNode.UpdateCache();

            // （ツール機能を使用する場合は）ツールを描画
            if (_tool != null)
            {
                _tool.Render();
            }

            ResetCurrentCamera();

            // 描画を終了
            if (!Graphics.EndFrame()) return false;
            return true;
        }

        private static void ResetCurrentCamera()
        {
            _currentCamera = null;
            _current3DCamera = null;
        }

        internal static void DrawCameraGroup(RenderedCamera camera, SortedDictionary<int, HashSet<IDrawn>> drawns)
        {
            _currentCamera = camera;
            Renderer.Camera = camera;

            // カリングの結果
            var cullingIdsCount = CullingSystem.DrawingRenderedIds.Count;
            var buffer = _DrawingRenderedIdsBuffer.GetAsArray(cullingIdsCount);
            CullingSystem.DrawingRenderedIds.CopyTo(buffer);
            Array.Sort(buffer, 0, cullingIdsCount);
            Span<int> cullingIds = buffer.AsSpan(0, cullingIdsCount);

            var requireRender = false;

            _PostEffectBuffer = null;

            RenderTexture targetTexture = null;

            foreach (var (_, znodes) in drawns)
            {
                foreach (var node in znodes)
                {
                    if (node is PostEffectNode)
                    {
                        if (requireRender)
                        {
                            Renderer.Render();
                            requireRender = false;
                        }

                        targetTexture ??= camera.TargetTexture ?? Graphics.CommandList.ScreenTexture;
                        _PostEffectBuffer ??= _RenderTextureCache.GetRenderTexture(targetTexture.Size, targetTexture.Format);

                        Graphics.CommandList.CopyTexture(targetTexture, _PostEffectBuffer);
                        node.Draw();
                    }
                    else if (node is ICullableDrawn cdrawn)
                    {
                        if (!cdrawn.IsDrawnActually) continue;
                        // NOTE: WhereIterator を生成させないために foreach (var node in nodes.Where(n => n.IsDrawnActually)) などとしない

                        if (cullingIds.BinarySearch(cdrawn.CullingId) < 0) continue;

                        node.Draw();
                        requireRender = true;
                    }
                    else if (node is IDrawn drawn)
                    {
                        node.Draw();
                        requireRender = true;
                    }
                    else throw new InvalidOperationException();
                }
            }

            if (Config.VisibleTransformInfo)
            {
                foreach (var (_, znodes) in drawns)
                {
                    foreach (var node in znodes)
                    {
                        if (node is ICullableDrawn cdrawn)
                        {
                            if (!cdrawn.IsDrawnActually) continue;
                            if (cullingIds.BinarySearch(cdrawn.CullingId) < 0) continue;
                            if (node is TransformNode t)
                            {
                                t.DrawTransformInfo();
                                requireRender = true;
                            }
                        }
                    }
                }
            }

            if (requireRender) Renderer.Render();
        }

        internal static void DrawCamera3DGroup(RenderedCamera3D camera, SortedDictionary<int, HashSet<IDrawn3D>> drawns)
        {
            Renderer3D.Camera = camera;
            _current3DCamera = camera;

            var requireRender = false;

            foreach (var (_, znodes) in drawns)
            {
                foreach (var node in znodes)
                {
                    if (node is IDrawn3D cdrawn)
                    {
                        if (!cdrawn.IsDrawnActually) continue;
                        // NOTE: WhereIterator を生成させないために foreach (var node in nodes.Where(n => n.IsDrawnActually)) などとしない

                        node.Draw();
                        requireRender = true;
                    }
                    else throw new InvalidOperationException();
                }
            }

            if (requireRender) Renderer3D.Render();
        }

        /// <summary>
        /// エンジンを終了します。
        /// </summary>
        public static void Terminate()
        {
            if (!isActive) return;

            // RootNode直下のNodeをEngineからRemove(CullingSystemから削除するため)
            _RootNode._Children.Clear();

            _PostEffectBuffer = null;
            _RenderTextureCache = null;
            PostEffectNode.TerminateCache();
            Core.Terminate();


            _DrawingRenderedIdsBuffer = null;
            Vector2FBuffer = null;
            VertexBuffer = null;
            Vector2FArrayCache = null;

            Config = null;
            _core = null;
            _log = null;
            _keyboard = null;
            _mouse = null;
            _joystick = null;
            _file = null;
            _resources = null;
            _window = null;
            _graphics = null;
            _renderer = null;
            _renderer3D = null;
            _cullingSystem = null;
            _tool = null;
            _sound = null;
            _RootNode = null;
            _UpdatedNode = null;
            _DrawnCollection = null;
            _Drawn3DCollection = null;
            _CameraNodes = null;
            _DefaultCamera = null;
            _profiler = null;

            isActive = false;
        }

        /// <summary>
        /// ノードの更新を一時停止します。
        /// </summary>
        /// <param name="keepUpdated">一時停止の対象から除外するノード</param>
        public static void Pause(Node keepUpdated = null)
        {
            _UpdatedNode = keepUpdated;
        }

        /// <summary>
        /// ノードの更新を再開します。
        /// </summary>
        public static void Resume()
        {
            _UpdatedNode = _RootNode;
        }

        #region Modules
        private static bool isActive;

        internal static Core Core => _core ?? throw new InvalidOperationException("Coreが初期化されていません。");
        private static Core _core;

        /// <summary>
        /// ファイルを管理するクラスを取得します。
        /// </summary>
        /// <exception cref="InvalidOperationException">File機能が初期されていなかったり終了していて操作を実行できなかった</exception>
        public static File File => _file ?? throw new InvalidOperationException("File機能が初期化されていません。");
        private static File _file;

        /// <summary>
        /// キーボードを管理するクラスを取得します。
        /// </summary>
        /// <exception cref="InvalidOperationException">Keyboard機能が初期されていなかったり終了していて操作を実行できなかった</exception>
        public static Keyboard Keyboard => _keyboard ?? throw new InvalidOperationException("Keyboard機能が初期化されていません。");
        private static Keyboard _keyboard;

        /// <summary>
        /// マウスを管理するクラスを取得します。
        /// </summary>
        /// <exception cref="InvalidOperationException">Mouse機能が初期されていなかったり終了していて操作を実行できなかった</exception>
        public static Mouse Mouse => _mouse ?? throw new InvalidOperationException("Mouse機能が初期化されていません。");
        private static Mouse _mouse;

        /// <summary>
        /// ジョイスティックを管理するクラスを取得します。
        /// </summary>
        /// <exception cref="InvalidOperationException">Joystick機能が初期されていなかったり終了していて操作を実行できなかった</exception>
        public static Joystick Joystick => _joystick ?? throw new InvalidOperationException("Joystic機能が初期化されていません。");
        private static Joystick _joystick;

        /// <summary>
        /// グラフィックのクラスを取得します。
        /// </summary>
        /// <exception cref="InvalidOperationException">Graphics機能が初期されていなかったり終了していて操作を実行できなかった</exception>
        public static Graphics Graphics => _graphics ?? throw new InvalidOperationException("Graphics機能が初期化されていません。");
        private static Graphics _graphics;

        /// <summary>
        /// ログを管理するクラスを取得します。
        /// </summary>
        /// <exception cref="InvalidOperationException">Log機能が初期されていなかったり終了していて操作を実行できなかった</exception>
        public static Log Log => _log ?? throw new InvalidOperationException("Log機能が初期化されていません。");
        private static Log _log;

        /// <summary>
        /// レンダラのクラスを取得します。
        /// </summary>
        /// <exception cref="InvalidOperationException">Rendererが初期されていなかったり終了していて操作を実行できなかった</exception>
        internal static Renderer Renderer => _renderer ?? throw new InvalidOperationException("Graphics機能が初期化されていません。");
        private static Renderer _renderer;

        /// <summary>
        /// 3Dレンダラのクラスを取得します。
        /// </summary>
        /// <exception cref="InvalidOperationException">Rendererが初期されていなかったり終了していて操作を実行できなかった</exception>
        internal static Renderer3D Renderer3D => _renderer3D ?? throw new InvalidOperationException("Graphics機能が初期化されていません。");
        private static Renderer3D _renderer3D;

        /// <summary>
        /// カリングのクラスを取得します。
        /// </summary>
        /// <exception cref="InvalidOperationException">CullingSystemが初期されていなかったり終了していて操作を実行できなかった</exception>
        internal static CullingSystem CullingSystem => _cullingSystem ?? throw new InvalidOperationException("Graphics機能が初期化されていません。");
        private static CullingSystem _cullingSystem;

        /// <summary>
        /// 音を管理するクラスを取得します。
        /// </summary>
        /// <exception cref="InvalidOperationException">Sound機能が初期されていなかったり終了していて操作を実行できなかった</exception>
        public static SoundMixer Sound => _sound ?? throw new InvalidOperationException("Sound機能が初期化されていません。");
        private static SoundMixer _sound;

        /// <summary>
        /// リソースを管理するクラスを取得します。
        /// </summary>
        /// <exception cref="InvalidOperationException">File機能が初期されていなかったり終了していて操作を実行できなかった</exception>
        internal static Resources Resources => _resources ?? throw new InvalidOperationException("File機能が初期化されていません。");
        private static Resources _resources;

        /// <summary>
        /// ウインドウを表すクラスを取得します。
        /// </summary>
        /// <exception cref="InvalidOperationException">Windowが初期されていなかったり終了していて操作を実行できなかった</exception>
        internal static Window Window => _window ?? throw new InvalidOperationException("Window機能が初期化されていません。");
        private static Window _window;

        /// <summary>
        /// ツールを管理するクラスを取得します。
        /// </summary>
        /// <exception cref="InvalidOperationException">Tool機能が初期されていなかったり終了していて操作を実行できなかった</exception>
        public static Tool Tool => _tool ?? throw new InvalidOperationException("Tool機能が初期化されていません。");
        private static Tool _tool;

        /// <summary>
        /// プロファイラを管理するクラスを取得します。
        /// </summary>
        /// <exception cref="InvalidOperationException">Profiler機能が初期されていなかったり終了していて操作を実行できなかった</exception>

        public static Profiler Profiler => _profiler ?? throw new InvalidOperationException("Profiler機能が初期化されていません。");
        private static Profiler _profiler;

        /// <summary>
        /// Collider3Dを管理するクラスを取得します。
        /// </summary>
        /// <exception cref="InvalidOperationException">Profiler機能が初期されていなかったり終了していて操作を実行できなかった</exception>

        public static Collision3DWorld Collision3DWorld => _collision3DWorld ?? throw new InvalidOperationException("Collision3DWorld機能が初期化されていません。");
        private static Collision3DWorld _collision3DWorld;

        /// <summary>
        /// 現在使用しているカメラ
        /// </summary>
        internal static RenderedCamera CurrentCamera => _currentCamera;
        private static RenderedCamera _currentCamera;


        /// <summary>
        /// 現在使用している3Dカメラ
        /// </summary>
        internal static RenderedCamera3D Current3DCamera => _current3DCamera;
        private static RenderedCamera3D _current3DCamera;

        #endregion

        #region Node

        /// <summary>
        /// エンジンに登録されているノードの列挙子を返します。
        /// </summary>
        public static IEnumerable<Node> GetNodes() => _RootNode.Children;

        /// <summary>
        /// エンジンにノードを追加します。
        /// </summary>
        /// <param name="node">追加されるノード</param>
        public static void AddNode(Node node)
        {
            _RootNode.AddChildNode(node);
        }

        /// <summary>
        /// エンジンからノードを削除します。
        /// </summary>
        /// <param name="node">削除するノード</param>
        public static void RemoveNode(Node node)
        {
            _RootNode.RemoveChildNode(node);
        }

        /// <summary>
        /// エンジンに登録されている <typeparamref name="T"/> 型のノードを列挙します。
        /// </summary>
        /// <typeparam name="T">検索するノードの型</typeparam>
        public static IEnumerable<T> FindNodes<T>() where T : Node
            => _RootNode.EnumerateDescendants<T>();

        /// <summary>
        /// エンジンに登録されている <typeparamref name="T"/> 型のノードのうち <paramref name="condition"/> を満たすものを列挙します。
        /// </summary>
        /// <typeparam name="T">検索するノードの型</typeparam>
        /// <param name="condition">検索するノードの条件</param>
        /// <exception cref="ArgumentNullException"><paramref name="condition"/>がnull</exception>
        public static IEnumerable<T> FindNodes<T>(Func<T, bool> condition) where T : Node
                => _RootNode.EnumerateDescendants(condition);

        internal static void RegisterDrawn(IDrawn node)
        {
            if (_DrawnCollection is null) throw new InvalidOperationException("Graphics機能が初期化されていません。");
            _DrawnCollection.Register(node);
        }

        internal static void UnregisterDrawn(IDrawn node)
        {
            if (_DrawnCollection is null) throw new InvalidOperationException("Graphics機能が初期化されていません。");
            _DrawnCollection.Unregister(node);
        }

        internal static void UpdateDrawnCameraGroup(IDrawn node, ulong old)
        {
            if (_DrawnCollection is null) throw new InvalidOperationException("Graphics機能が初期化されていません。");
            _DrawnCollection.UpdateCameraGroup(node, old);
        }

        internal static void UpdateDrawnZOrder(IDrawn node, int old)
        {
            if (_DrawnCollection is null) throw new InvalidOperationException("Graphics機能が初期化されていません。");
            _DrawnCollection.UpdateZOrder(node, old);
        }

        internal static void RegisterDrawn3D(IDrawn3D node)
        {
            if (_Drawn3DCollection is null) throw new InvalidOperationException("Graphics機能が初期化されていません。");
            _Drawn3DCollection.Register(node);
        }

        internal static void UnregisterDrawn3D(IDrawn3D node)
        {
            if (_Drawn3DCollection is null) throw new InvalidOperationException("Graphics機能が初期化されていません。");
            _Drawn3DCollection.Unregister(node);
        }

        internal static void UpdateDrawnCamera3DGroup(IDrawn3D node, ulong old)
        {
            if (_Drawn3DCollection is null) throw new InvalidOperationException("Graphics機能が初期化されていません。");
            _Drawn3DCollection.UpdateCameraGroup(node, old);
        }

        internal static void UpdateDrawn3DZOrder(IDrawn3D node, int old)
        {
            if (_Drawn3DCollection is null) throw new InvalidOperationException("Graphics機能が初期化されていません。");
            _Drawn3DCollection.UpdateZOrder(node, old);
        }

        #endregion

        #region CameraNodeCollection

        internal static void RegisterCameraNode(CameraNode camera)
        {
            if (_CameraNodes is null) throw new InvalidOperationException("Graphics機能が初期化されていません。");
            _CameraNodes.AddCamera(camera);
        }

        internal static void UnregisterCameraNode(CameraNode camera)
        {
            if (_CameraNodes is null) throw new InvalidOperationException("Graphics機能が初期化されていません。");
            _CameraNodes.RemoveCamera(camera);
        }

        internal static void UpdateCameraNodeGroup(CameraNode camera, ulong oldGroup)
        {
            if (_CameraNodes is null) throw new InvalidOperationException("Graphics機能が初期化されていません。");
            _CameraNodes.UpdateGroup(camera, oldGroup);
        }

        #endregion

        #region Camera3DNodeCollection

        internal static void RegisterCamera3DNode(Camera3DNode camera)
        {
            if (_Camera3DNodes is null) throw new InvalidOperationException("Graphics機能が初期化されていません。");
            _Camera3DNodes.AddCamera(camera);
        }

        internal static void UnregisterCamera3DNode(Camera3DNode camera)
        {
            if (_Camera3DNodes is null) throw new InvalidOperationException("Graphics機能が初期化されていません。");
            _Camera3DNodes.RemoveCamera(camera);
        }

        internal static void UpdateCamera3DNodeGroup(Camera3DNode camera, ulong oldGroup)
        {
            if (_Camera3DNodes is null) throw new InvalidOperationException("Graphics機能が初期化されていません。");
            _Camera3DNodes.UpdateGroup(camera, oldGroup);
        }

        #endregion

        /// <summary>
        /// <see cref="System.Threading.SynchronizationContext"/>を取得します。
        /// </summary>
        internal static AltseedContext Context { get; private set; }

        /// <summary>
        /// ウィンドウのサイズを取得または設定します。
        /// </summary>
        public static Vector2I WindowSize
        {
            get
            {
                if (_WindowSize != Window.Size) UpdateWindowSize();
                return _WindowSize;
            }
            set
            {
                Window.Size = value;
                UpdateWindowSize();
            }
        }

        private static void UpdateWindowSize()
        {
            _WindowSize = Window.Size;
            _DefaultCamera.ViewMatrix = Matrix44F.GetTranslation2D(-Window.Size / 2);
            _DefaultCamera3D.ViewMatrix =
                Matrix44F.GetPerspectiveFovLH(1.05f, (float)Window.Size.X / Window.Size.Y, 0.1f, 5000) *
                Matrix44F.GetLookAtLH(new Vector3F(0, 0, -5), new Vector3F(0, 0, 0), new Vector3F(0, 1, 0));
        }

        private static Vector2I _WindowSize;

        internal static Configuration Config { get; private set; }

        /// <summary>
        /// ウインドウのタイトルを取得または設定します。
        /// </summary>
        public static string WindowTitle
        {
            get => Window.Title;
            set
            {
                Window.Title = value;
            }
        }

        #region FPS制御

        /// <summary>
        /// フレームレートの制御方法を取得または設定します。
        /// </summary>
        public static FramerateMode FramerateMode
        {
            get => Core.FramerateMode;
            set { Core.FramerateMode = value; }
        }

        /// <summary>
        /// 目標フレームレートを取得または設定します。
        /// </summary>
        public static float TargetFPS
        {
            get => Core.TargetFPS;
            set { Core.TargetFPS = (int)value; }
        }

        /// <summary>
        /// 現在のFPSを取得します。
        /// </summary>
        public static float CurrentFPS => Core.CurrentFPS;

        /// <summary>
        /// 前のフレームからの経過時間(秒)を取得します。
        /// </summary>
        public static float DeltaSecond => Core.DeltaSecond;

        #endregion
    }
}
