using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.GameContent;
using Terraria;
using System.Diagnostics;
using System;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework.Content;


namespace DreamMod.Common.Graphics;
public struct TrailShaderSettings {

	public string shaderType;
	public Color Color;
	public Vector2[] oldPos;
	public float[] oldRot;
	public Asset<Texture2D> image1;
	public Asset<Texture2D> image2;
	public Asset<Texture2D> image3;
	public Vector4 shaderData;
	public Vector2 offset;
}

public struct ShaderSettings {

	public Vector3[] Colors;
	public Asset<Texture2D> image1;
	public Asset<Texture2D> image2;
	public Asset<Texture2D> image3;
	public Vector4 shaderData;

}
/// <summary>
/// Keep in mind that:
/// Spritebatch automatically Sets Main.Instance.GraphicsDevice.Textures[0] to the texture its currently drawing in the batch (when calling Draw() for immediate mode and End() for other modes),
/// and if you want to modify Main.Instance.GraphicsDevice.Textures, for things like vertex buffers, you would do it while spritebatch is not active (before Begin() or after End()),
/// </summary>
public class ModdedShaderHandler : ILoadable {
	static GraphicsDevice GraphicsDevice => Main.instance.GraphicsDevice;
	Asset<Effect> _effect;
	Vector3[] _colors = new Vector3[3];
	Texture2D _texutre1 = null;
	Texture2D _texutre2 = null;
	Texture2D _texutre3 = null;
	Texture2D _texutre4 = null;
	Texture2D _texutre5 = null;
	Texture2D _texutre6 = null;
	Vector4 _shaderData = new Vector4(0, 0, 0, 0);
	public bool enabled = false;
	Vector2 rectSize = default;
	TextureCube skybox = null;
	public ModdedShaderHandler(Asset<Effect> effect) {

		this._effect = effect;

	}
	public void setProperties(Vector3[] colors, Texture2D texutre1 = null, Texture2D texutre2 = null, Texture2D texutre3 = null, Texture2D texutre4 = null, Texture2D texutre5 = null, Texture2D texutre6 = null, Vector4 shaderData = default, Vector2 rectSize = default, TextureCube skybox = null) {
		this._colors = colors;
		this._texutre1 = texutre1;
		this._texutre2 = texutre2;
		this._texutre3 = texutre3;
		this._texutre4 = texutre4;
		this._texutre5 = texutre5;
		this._texutre6 = texutre6;
		this._shaderData = shaderData;
		this.skybox = skybox;
	}
	public void setProperties(ShaderSettings shaderSettings) {
		this._colors = shaderSettings.Colors;
		this._texutre1 = shaderSettings.image1?.Value;
		this._texutre2 = shaderSettings.image2?.Value;
		this._texutre3 = shaderSettings.image3?.Value;
		this._shaderData = shaderSettings.shaderData;
	}
	/// <summary>
	/// call this before Begin() or after End() 
	/// </summary>
	public void setupTextures() 
	{

		if (_texutre1 != null) {
			GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;
			GraphicsDevice.Textures[1] = _texutre1;
		}
		if (_texutre2 != null) {
			GraphicsDevice.SamplerStates[2] = SamplerState.LinearWrap;
			GraphicsDevice.Textures[2] = _texutre2;
		}
		if (_texutre3 != null) {
			GraphicsDevice.SamplerStates[3] = SamplerState.LinearWrap;
			GraphicsDevice.Textures[3] = _texutre3;
		}
		if (_texutre4 != null) {
			GraphicsDevice.SamplerStates[4] = SamplerState.LinearWrap;
			GraphicsDevice.Textures[4] = _texutre4;
		}
		if (_texutre5 != null) {
			GraphicsDevice.SamplerStates[5] = SamplerState.LinearWrap;
			GraphicsDevice.Textures[5] = _texutre5;
		}
		if (_texutre6 != null) {
			GraphicsDevice.SamplerStates[6] = SamplerState.LinearWrap;
			GraphicsDevice.Textures[6] = _texutre6;
		}
	}
	public void apply() {
		var viewport = GraphicsDevice.Viewport;
		Effect effect = _effect.Value;
		setupTextures();
		effect.Parameters["viewWorldProjection3D"]?.SetValue(Matrix.Identity * Matrix.CreateLookAt(new Vector3(0,0,-1024f), Vector3.Zero, Vector3.Up) * Matrix.CreatePerspectiveFieldOfView(3.1415f / 4f, GraphicsDevice.Viewport.AspectRatio, 1f, 2f));
		effect.Parameters["viewWorldProjection"].SetValue(Matrix.CreateTranslation(new Vector3(-Main.screenPosition, 0)) * Main.GameViewMatrix.TransformationMatrix * Matrix.CreateOrthographicOffCenter(left: 0, right: viewport.Width, bottom: viewport.Height, top: 0, zNearPlane: -1, zFarPlane: 10));
		effect.Parameters["time"].SetValue(Main.GlobalTimeWrappedHourly);
		effect.Parameters["colors"].SetValue(_colors);
		effect.Parameters["shaderData"].SetValue(_shaderData);
		effect.Parameters["screenSize"].SetValue(Main.ScreenSize.ToVector2());
		effect.Parameters["screenPosition"].SetValue(Main.screenPosition);
		effect.Parameters["playerPosition"]?.SetValue(Main.LocalPlayer.Center);
		effect.Parameters["vertexRectSize"]?.SetValue(rectSize);
		effect.Parameters["skyboxTexture"]?.SetValue(skybox);
		
		effect.CurrentTechnique.Passes[0].Apply();
		
	}

	public void Load(Mod mod) {

	}

	public void Unload() {
		Main.RunOnMainThread(() => {

			_effect.Dispose();

		}).Wait();
	}
}
