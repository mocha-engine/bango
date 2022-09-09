﻿namespace Mocha.Common.World;

public interface IEntity
{
	public int Id { get; set; }
	public bool Visible { get; set; }
	public Transform Transform { get; set; }

	void BuildCamera( ref CameraSetup cameraSetup );
	void Delete();
}
