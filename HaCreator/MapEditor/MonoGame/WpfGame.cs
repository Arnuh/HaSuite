using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace MonoGame.WpfControl {
	/// <summary>
	/// The replacement for <see cref="Game"/>. Unlike <see cref="Game"/> the <see cref="WpfGame"/> is a WPF control and can be hosted inside WPF windows.
	/// </summary>
	public class WpfGame : D3D11Host {
		#region Fields

		private ContentManager _content;

		#endregion

		#region Constructors

		/// <summary>
		/// Creates a new instance of a game host panel.
		/// </summary>
		public WpfGame(string contentDir = "Content") {
			if (string.IsNullOrEmpty(contentDir)) {
				throw new ArgumentNullException(nameof(contentDir));
			}

			Content = new ContentManager(Services, contentDir);
			Focusable = true;
		}

		#endregion

		#region Properties

		/// <summary>
		/// The content manager for this game.
		/// </summary>
		public ContentManager Content {
			get => _content;
			set {
				if (value == null) {
					throw new ArgumentNullException();
				}

				_content = value;
			}
		}

		/// <summary>
		/// Determines whether the game runs in fixed timestep or unlimited.
		/// Since WPF is limited to 60 FPS this value is always true.
		/// </summary>
		public bool IsFixedTimeStep => true;

		/// <summary>
		/// The target time between two updates. WPF itself is limited to 60 FPS max, so that's what this value always returns.
		/// </summary>
		public TimeSpan TargetElapsedTime => TimeSpan.FromSeconds(1 / 60f);

		#endregion

		#region Methods

		/// <summary>
		/// Dispose is called to dispose of resources.
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose(bool disposing) {
			base.Dispose(disposing);
			Content?.Dispose();

			UnloadContent();
		}

		/// <summary>
		/// The draw method that is called to render your scene.
		/// </summary>
		/// <param name="gameTime"></param>
		protected virtual void Draw(GameTime gameTime) {
		}

		/// <summary>
		/// Initialize is called once when the control is created.
		/// </summary>
		protected override void Initialize() {
			base.Initialize();
			LoadContent();
		}

		/// <summary>
		/// Load content is called once by <see cref="Initialize()"/>.
		/// </summary>
		protected virtual void LoadContent() {
		}

		/// <summary>
		/// Internal method used to integrate <see cref="Update"/> and <see cref="Draw"/> with the WPF control.
		/// </summary>
		/// <param name="time"></param>
		protected sealed override void Render(GameTime time) {
			// just run as fast as possible, WPF itself is limited to 60 FPS so that's the max we will get
			Update(time);
			Draw(time);
		}

		/// <summary>
		/// Unload content is called once when the control is destroyed.
		/// </summary>
		protected virtual void UnloadContent() {
		}

		/// <summary>
		/// The update method that is called to update your game logic.
		/// </summary>
		/// <param name="gameTime"></param>
		protected virtual void Update(GameTime gameTime) {
		}

		#endregion
	}
}