using System;
using UnityEngine;
using UnityEngine.UI;
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
public class AnimationCharacter : MonoBehaviour
{
	public string Skin = "A";
	Sprite[] _sprites;
	SpriteRenderer _spriteRenderer;
	Animator _animator;
	string _path;
	void Awake()
	{
		_spriteRenderer = GetComponent<SpriteRenderer>();
		_animator = GetComponent<Animator>();
		Load();
	}
	void Load()
	{
		var path = "Character/Sprite/" + _animator.runtimeAnimatorController.name + Skin;
		if (!path.Equals(_path))
		{
			_path = path;
			_sprites = Resources.LoadAll<Sprite>(_path);
		}
	}
	void LateUpdate()
	{
		if (_spriteRenderer == null || _spriteRenderer.sprite == null)
			return;
		Load();
		var name = _spriteRenderer.sprite.name;
		var sprite = Array.Find(_sprites, item => item.name == name);
		if (sprite)
			_spriteRenderer.sprite = sprite;
	}
}
