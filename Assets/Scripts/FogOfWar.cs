using UnityEngine;
using System.Collections.Generic;

public class FogOfWar : MonoBehaviour
{
	#region Private
	private const int MAX_TERRAIN_HEIGHT = 600;

	[SerializeField]
	private List<Reveler> _revelers;
	[SerializeField]
	private int _textureWidth;
	[SerializeField]
	private int _textureHeight;
	[SerializeField]
	private Vector2 _mapSize;
	[SerializeField]
	private Material _fogMaterial;
	[SerializeField]
	private TextAsset _heightMap;
	[SerializeField]
	private int _heightMapWidth;
	[SerializeField]
	private int _heightMapHeight;

	private Texture2D _shadowMap;
	private Color32[] _pixels;
	private int[] _heightMapData;
	#endregion

	private void Awake()
	{
		_shadowMap = new Texture2D(_textureWidth, _textureHeight, TextureFormat.RGB24, false);

		_pixels = _shadowMap.GetPixels32();

		for (var i = 0; i < _pixels.Length; ++i)
		{
			_pixels[i] = Color.black;
		}

		_shadowMap.SetPixels32(_pixels);
		_shadowMap.Apply();

		_fogMaterial.SetTexture("_ShadowMap", _shadowMap);

		byte[] heightBytes = _heightMap.bytes;
		_heightMapData = new int[heightBytes.Length / 2];
		
		var j = 0;
		for (var i = 0; i < heightBytes.Length && j < _heightMapData.Length; i+=2, ++j)
		{
			_heightMapData[j] = (heightBytes[i + 1] << 0x08) | heightBytes[i];
		}
	}

	private void Update()
	{
		for (var i = 0; i < _pixels.Length; ++i)
		{
			_pixels[i].r = 0;
		}
		
		UpdateShadowMap();

		_shadowMap.SetPixels32(_pixels);
		_shadowMap.Apply();
	}

	private void UpdateShadowMap()
	{
		foreach (var reveler in _revelers)
		{
			DrawFilledMidpointCircleSinglePixelVisit(reveler.transform.position, reveler.sight);
		}
	}

	private void DrawFilledMidpointCircleSinglePixelVisit(Vector3 position, int radius)
	{
		int x = Mathf.RoundToInt(radius * (_textureWidth / _mapSize.x));
		int y = 0;
		int radiusError = 1 - x;

		var centerX = Mathf.RoundToInt(position.x * (_textureWidth / _mapSize.x));
		var centerY = Mathf.RoundToInt(position.z * (_textureHeight / _mapSize.y));

		while (x >= y)
		{
			int startX = -x + centerX;
			int endX = x + centerX;
			FillRow(startX, endX, y + centerY, (int)position.y);
			if (y != 0)
			{
				FillRow(startX, endX, -y + centerY, (int)position.y);
			}

			++y;

			if (radiusError < 0)
			{
				radiusError += 2 * y + 1;
			}
			else
			{
				if (x >= y)
				{
					startX = -y + 1 + centerX;
					endX = y - 1 + centerX;
					FillRow(startX, endX, x + centerY, (int)position.y);
					FillRow(startX, endX, -x + centerY, (int)position.y);
				}
				--x;
				radiusError += 2 * (y - x + 1);
			}
		}
	}

	private void FillRow(int startX, int endX, int row, int height)
	{
		int index;
		for (var x = startX; x < endX; ++x)
		{
			index = x + row * _textureWidth;
			if (index > -1 && index < _pixels.Length && HeightCheck(x, row, height))
			{
				_pixels[index].r = 255;
				_pixels[index].g = 255;
			}
		}
	}

	private bool HeightCheck(int x, int y, int height)
	{
		if (_textureWidth != _heightMapWidth-1 && _textureHeight != _heightMapHeight-1)
		{
			var widthRatio = (float)_heightMapWidth / _textureWidth;
			var heightRatio = (float)_heightMapHeight / _textureHeight;

			x = (int)(x * widthRatio);
			y = (int)(y * heightRatio);
		}

		if (y * _heightMapWidth + x > _heightMapData.Length || y * _heightMapWidth + x < 0)
		{
			return false;
		}

		var convertedHeight = ((float)height / MAX_TERRAIN_HEIGHT) * ushort.MaxValue;
		
		return convertedHeight > _heightMapData[y * _heightMapWidth + x];
	}
	
	private void OnDestroy()
	{
		_shadowMap = null;
		_pixels = null;
	}
}
