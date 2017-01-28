using UnityEngine;
using System.Collections.Generic;

public class FogOfWar : MonoBehaviour
{
	#region Private
	[SerializeField]
	private List<Reveler> _revelers;
	[SerializeField]
	private int _width;
	[SerializeField]
	private int _height;
	[SerializeField]
	private Vector2 _mapSize;
	[SerializeField]
	private Material _fogMaterial;

	private Texture2D _shadowMap;
	private Color32[] _pixels;
	#endregion

	private void Awake()
	{
		_shadowMap = new Texture2D(_width, _height, TextureFormat.RGB24, false);

		_pixels = _shadowMap.GetPixels32();

		for (var i = 0; i < _pixels.Length; ++i)
		{
			_pixels[i] = Color.black;
		}

		_shadowMap.SetPixels32(_pixels);
		_shadowMap.Apply();

		_fogMaterial.SetTexture("_ShadowMap", _shadowMap);
	}

	private void Update()
	{
		for(var i = 0; i < _pixels.Length; ++i)
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
			DrawFilledMidpointCircleSinglePixelVisit((int)reveler.transform.position.x, (int)reveler.transform.position.z, reveler.sight);
		}
	}

	private void DrawFilledMidpointCircleSinglePixelVisit(int centerX, int centerY, int radius)
	{
		int x = Mathf.RoundToInt(radius * (_width / _mapSize.x));
		int y = 0;
		int radiusError = 1 - x;

		centerX = Mathf.RoundToInt(centerX * (_width / _mapSize.x));
		centerY = Mathf.RoundToInt(centerY * (_height / _mapSize.y));

		while (x >= y)
		{
			int startX = -x + centerX;
			int endX = x + centerX;
			FillRow(startX, endX, y + centerY);
			if (y != 0)
			{
				FillRow(startX, endX, -y + centerY);
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
					FillRow(startX, endX, x + centerY);
					FillRow(startX, endX, -x + centerY);
				}
				--x;
				radiusError += 2 * (y - x + 1);
			}
		}
	}

	private void FillRow(int startX, int endX, int row)
	{
		int index;
		for (var x = startX; x < endX; ++x)
		{
			index = x + row * _width;
			if (index > -1 && index < _pixels.Length)
			{
				_pixels[index].r = 255;
				_pixels[index].g = 255;
			}
		}
	}

	private bool HeightCheck(int x, int y, int height)
	{
		// convert the pixel to the height map pixel
		// lookup the hight map information for that point.
		// return height is greater then that point.

		return height > 0;
	}

	private void OnDestroy()
	{
		_shadowMap = null;
		_pixels = null;
	}
}
