using Godot;
using System;

public partial class Character : CharacterBody3D
{
	public const float Speed = 5.0f;
	public const float JumpVelocity = 4.5f;
	Vector3 lastLookAtDirection = new Vector3();
	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();
	AnimationTree tree;
	Vector2 lastInputDirection = new();

    public override void _Ready()
    {
        base._Ready();
		tree = GetNode<AnimationTree>("AnimationTree");
    }
    public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;
		
		// Add the gravity.
		if (!IsOnFloor())
			velocity.Y -= gravity * (float)delta;

		// Handle Jump.
		if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
			velocity.Y = JumpVelocity;
		CameraController cameraController = GetTree().GetNodesInGroup("CameraController")[0] as CameraController; ;
		Vector3 lookat = cameraController.GetNode<Node3D>("LookAt").GlobalPosition;
       lookat = new Vector3(lookat.X, GlobalPosition.Y, lookat.Z);
		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 inputDir = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		if (direction != Vector3.Zero)
		{
			Vector3 lerpDirection = LearpVector3(lastLookAtDirection, lookat, .05f);
			LookAt(lerpDirection);
			lastLookAtDirection = lerpDirection;
            velocity.X = direction.X * Speed;
			velocity.Z = direction.Z * Speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
		}

		Velocity = velocity;

		tree.Set("parameters/conditions/idle", (IsOnFloor() && inputDir == Vector2.Zero));
		tree.Set("parameters/conditions/moving", (IsOnFloor() && inputDir != Vector2.Zero));
		tree.Set("parameters/conditions/falling", !IsOnFloor());
		tree.Set("parameters/conditions/landed", IsOnFloor());
		tree.Set("parameters/BlendSpace2D/blend_position", new Vector2(Mathf.Lerp(lastInputDirection.X, inputDir.X, .15f), Mathf.Lerp(lastInputDirection.Y, inputDir.Y, .15f)));
		lastInputDirection = inputDir;
        MoveAndSlide();
	}

	private Vector3 LearpVector3(Vector3 valueToLerp, Vector3 valueToLearpTo, float weight) => new Vector3(
		Mathf.Lerp(valueToLerp.X, valueToLearpTo.X, weight), Mathf.Lerp(valueToLerp.Y, valueToLearpTo.Y, weight), Mathf.Lerp(valueToLerp.Z, valueToLearpTo.Z, weight));
}
