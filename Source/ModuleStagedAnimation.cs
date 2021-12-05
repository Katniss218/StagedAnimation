using UnityEngine;
using System;
using System.Collections.Generic;

namespace StagedAnimation
{
	public class ModuleStagedAnimation : PartModule, IScalarModule
	{
		[KSPField]
		public string animationName = "";

		[KSPField]
		public int layer = 0;

		[KSPField]
		public string moduleID = "animkatStaged";

		[KSPField( isPersistant = true )]
		public AnimationStates animState;

		/// <summary>
		/// How far along the timeline is the animation.
		/// </summary>
		[KSPField( isPersistant = true )]
		private float animTime = 0.0f;

		[KSPField( isPersistant = true )]
		private float animSpeed = 1.0f;

		[KSPField]
		public string fxGroupName = "decouple";

		private FXGroup fxGroup;
		protected Animation[] anims;

		public bool CanMove => true;

		public float GetScalar => animTime;

		public string ScalarModuleID => moduleID;

		public EventData<float, float> OnMoving { get; set; }

		public EventData<float> OnStop { get; set; }

		public void PlayStagedAnim()
		{
			for( int i = 0; i < anims.Length; i++ )
			{
				Debug.Log( "ANIMKAT playing anim on " + anims[i].gameObject.name + "" );

				anims[i][animationName].speed = (!HighLogic.LoadedSceneIsEditor) ? (1.0f) : (10.0f * anims[i][animationName].length);
				anims[i].Play( animationName );
			}
			animState = AnimationStates.MOVING;

			OnMoving.Fire( 0.0f, 1.0f );
		}


		public override void OnAwake()
		{
			OnMoving = new EventData<float, float>( "ModuleStagedAnimation.OnMovingEvent" );
			OnStop = new EventData<float>( "ModuleStagedAnimation.OnStoppedEvent" );

			fxGroup = part.findFxGroup( fxGroupName );
			if( fxGroup == null )
			{
				Debug.LogError( "ANIMKAT: Cannot find fx group " + fxGroupName );
			}
		}

		public override void OnStart( StartState state )
		{
			if( !string.IsNullOrEmpty( animationName ) )
			{
				anims = part.FindAnimation( animationName );

				Debug.Log( $"ANIMKAT found {anims.Length} anims on {gameObject.name}" );
			}
			else
			{
				anims = new Animation[0];
			}

			if( part.stagingIcon == string.Empty && overrideStagingIconIfBlank )
			{
				part.stagingIcon = "DECOUPLER_VERT";
			}

			if( animTime != 0.0f )
			{
				FXGroup fx = part.findFxGroup( "activate" );
				if( fx != null )
				{
					fx.setActive( false );
				}
			}

			if( !enabled )
			{
				return;
			}

			// Set up the animations.
			// This will run when the part is being spawned in the VAB.
			for( int i = 0; i < anims.Length; i++ )
			{
				anims[i][animationName].enabled = true;
				anims[i][animationName].layer = layer;
				anims[i][animationName].speed = 0.0f; // this is important
				anims[i][animationName].weight = 1.0f;
				anims[i][animationName].normalizedTime = animTime;
			}
			if( animState == AnimationStates.MOVING )
			{
				for( int i = 0; i < anims.Length; i++ )
				{
					anims[i][animationName].speed = animSpeed;
					anims[i].Play( animationName );
				}
				OnMoving.Fire( animTime, (animSpeed > 0.0f) ? 1.0f : 0.0f );
			}

			base.part.ScheduleSetCollisionIgnores();
		}

		public override void OnActive()
		{
			if( animState == AnimationStates.READY )
			{
				PlayStagedAnim();
			}
		}

		private void FixedUpdate()
		{
			if( animState == AnimationStates.MOVING )
			{
				bool isAnyPlaying = false;

				for( int i = 0; i < anims.Length; i++ )
				{
					if( anims[i].IsPlaying( animationName ) ) // if playing
					{
						isAnyPlaying = true;
#warning ideally this would have separate values for each. Possibly saved as multiple config nodes of the same name.
						animSpeed = anims[i][animationName].speed;
						animTime = anims[i][animationName].normalizedTime;
					}
				}

				if( !isAnyPlaying ) // if all animations have finished
				{
					for( int i = 0; i < anims.Length; i++ )
					{
						anims[i][animationName].normalizedTime = 1.0f;
					}

					animTime = 1.0f;

					animState = AnimationStates.DISABLED;

					base.part.SetCollisionIgnores();

					OnStop.Fire( animTime );
				}
			}
		}

		public bool IsMoving()
		{
			if( !string.IsNullOrEmpty( animationName ) )
			{
				bool isAnyPlaying = false;
				
				for( int i = 0; i < anims.Length; i++ )
				{
					if( anims[i].IsPlaying( animationName ) ) // if playing
					{
						isAnyPlaying = true;
					}
				}
				return isAnyPlaying;
			}

			return false;
		}

		public void SetScalar( float t )
		{

		}

		public void SetUIRead( bool state )
		{

		}

		public void SetUIWrite( bool state )
		{

		}
	}
}