using UnityEngine;
using System;
using System.Collections.Generic;

namespace StagedAnimation
{
	public class ModuleAnimateKatnissified : PartModule, IScalarModule
	{
		/// <summary>
		/// Name of the animations to control
		/// </summary>
		[KSPField]
		public string animationName = "";

		/// <summary>
		/// The animations referenced by this module will be put in this layer.
		/// </summary>
		[KSPField]
		public int layer = 1;

		[KSPField]
		public string moduleID = "animkat";

		/// <summary>
		/// How far along the timeline is the animation.
		/// </summary>
		[KSPField( isPersistant = true )]
		public float animTime = 0.0f;

		[KSPField( isPersistant = true )]
		public float animSpeed = 1.0f;

		/// <summary>
		/// The current state of the animation.
		/// </summary>
		[KSPField( isPersistant = true )]
		public AnimationStates animState;

		/// <summary>
		/// If true, it'll enable the animations to be triggered via the staging system.
		/// </summary>
		[KSPField]
		public bool enableStaged = false;

		/// <summary>
		/// True if the animation is playing (or last played) in reverse.
		/// </summary>
		[KSPField( isPersistant = true )]
		public bool animReversed = true;

		[KSPField]
		public float evaDistance = 5.0f;

		/// <summary>
		/// Label that will be displayed for playing the animation forward
		/// </summary>
		[KSPField]
		public string startEventGUIName = "#autoLOC_6001354";

		/// <summary>
		/// Label that will be displayed for playing the animation in reverse
		/// </summary>
		[KSPField]
		public string endEventGUIName = "#autoLOC_6001354";

		/// <summary>
		/// Label that will be displayed for toggling the animation
		/// </summary>
		[KSPField]
		public string actionGUIName = "#autoLOC_6001354";

		[KSPField]
		public bool disableAfterPlaying;

		//[KSPField( isPersistant = true )]
		//public bool animationIsDisabled;

		protected Animation[] anims = null;

		[KSPField]
		public string fxGroupName = "decouple";

		private FXGroup fxGroup;

		public EventData<float, float> OnMoving { get; set; }

		public EventData<float> OnStop { get; set; }

		private BaseEvent toggleEvent;
		private BaseAction toggleAction;

		public string ScalarModuleID => moduleID;

		public float GetScalar => animTime;

		public bool CanMove => true;


		[KSPAction( "#autoLOC_6001329", KSPActionGroup.REPLACEWITHDEFAULT )]
		public void ToggleAction( KSPActionParam param )
		{
			Toggle();
		}

		// this gets executed when the thing gets toggled either via GUI or action group.
		[KSPEvent( unfocusedRange = 5f, guiActiveUnfocused = true, guiActive = true, guiActiveEditor = true, guiName = "#autoLOC_6001329" )]
		public void Toggle()
		{
			animReversed = !animReversed;

			if( animReversed )
			{
				foreach( var anim in anims )
				{
					Debug.Log( "ANIMKAT-G playing anim on " + anim.gameObject.name + "" );

					anim[animationName].speed = (!HighLogic.LoadedSceneIsEditor) ? (-1.0f) : (-10.0f * anim[animationName].length);
					anim.Play( animationName );
				}

				toggleEvent.guiName = startEventGUIName;
				OnMoving.Fire( 1.0f, 0.0f );
			}
			else
			{
				foreach( var anim in anims )
				{
					Debug.Log( "ANIMKAT-G playing anim on " + anim.gameObject.name + "" );

					anim[animationName].speed = (!HighLogic.LoadedSceneIsEditor) ? 1.0f : (10.0f * anim[animationName].length);
					anim.Play( animationName );
				}

				toggleEvent.guiName = endEventGUIName;
				OnMoving.Fire( 0.0f, 1.0f );
			}

			animState = AnimationStates.MOVING;

			if( HighLogic.LoadedSceneIsFlight && animState != AnimationStates.DISABLED && disableAfterPlaying && !animReversed )
			{
				animState = AnimationStates.DISABLED;
			}
		}

		public void PlayStagedAnim()
		{
			if( !animReversed )
			{
				return;
			}

			Toggle();
		}


		public override void OnAwake()
		{
			OnMoving = new EventData<float, float>( "ModuleAnimateKatnissified.OnMovingEvent" );
			OnStop = new EventData<float>( "ModuleAnimateKatnissified.OnStoppedEvent" );

			toggleEvent = base.Events["Toggle"];
			toggleAction = base.Actions["ToggleAction"];

			if( toggleAction.actionGroup == KSPActionGroup.REPLACEWITHDEFAULT )
			{
				toggleAction.actionGroup = KSPActionGroup.None;
			}
			if( toggleAction.defaultActionGroup == KSPActionGroup.REPLACEWITHDEFAULT )
			{
				toggleAction.defaultActionGroup = KSPActionGroup.None;
			}

			if( enableStaged )
			{
				fxGroup = part.findFxGroup( fxGroupName );
				if( fxGroup == null )
				{
					Debug.LogError( "ANIMKAT: Cannot find fx group " + fxGroupName );
				}
			}
		}

		public override void OnStart( StartState state )
		{
			if( !string.IsNullOrEmpty( animationName ) )
			{
				anims = part.FindAnimation( animationName );
			}
			else
			{
				anims = new Animation[0];
			}

			if( enableStaged )
			{
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
			}

			if( !enabled )
			{
				return;
			}

			toggleAction.guiName = actionGUIName;
			if( animReversed )
			{
				toggleEvent.guiName = startEventGUIName;
			}
			else
			{
				toggleEvent.guiName = endEventGUIName;
			}

			// Set up the animations.
			// This will run when the part is being spawned in the VAB.
			foreach( var anim in anims )
			{
				anim[animationName].enabled = true;
				anim[animationName].layer = layer;
				anim[animationName].speed = 0.0f; // this is important
				anim[animationName].weight = 1.0f;
				anim[animationName].normalizedTime = animTime;
			}

			if( animState == AnimationStates.MOVING )
			{
				foreach( var anim in anims )
				{
					anim[animationName].speed = animSpeed;
					anim.Play( animationName );
				}
				OnMoving.Fire( animTime, (animSpeed > 0.0f) ? 1.0f : 0.0f );
			}

			base.part.ScheduleSetCollisionIgnores();

			toggleAction.active = true;
			toggleEvent.guiActive = true;
			toggleEvent.guiActiveEditor = true;
			toggleEvent.guiActiveUnfocused = true;
			toggleEvent.unfocusedRange = evaDistance;
		}

		public override void OnActive()
		{
			if( enableStaged && animState == AnimationStates.READY )
			{
				PlayStagedAnim();
			}
		}

		private void FixedUpdate()
		{
			// The button shall be hidden if the animation is not currently ready to be played.
			toggleEvent.active = animState == AnimationStates.READY;
			toggleAction.active = animState == AnimationStates.READY;

			if( animState == AnimationStates.MOVING )
			{
				bool isAnyPlaying = false;

				foreach( var anim in anims )
				{
					if( anim.IsPlaying( animationName ) ) // if playing
					{
						isAnyPlaying = true;
#warning ideally this would have separate values for each. Possibly saved as multiple config nodes of the same name.
						animSpeed = anim[animationName].speed;
						animTime = anim[animationName].normalizedTime;
					}
				}

				if( !isAnyPlaying ) // if all animations have finished
				{
					if( !animReversed )
					{
						foreach( var anim in anims )
						{
							anim[animationName].normalizedTime = 1.0f;
						}

						animTime = 1.0f;
					}
					else
					{
						foreach( var anim in anims )
						{
							anim[animationName].normalizedTime = 0.0f;
						}

						animTime = 0.0f;
					}

					animState = AnimationStates.READY; // readied to play in opposite direction.

					base.part.SetCollisionIgnores();

					OnStop.Fire( animTime );
				}
			}
		}

		public void SetScalar( float t )
		{

		}

		public void SetUIRead( bool state )
		{

		}

		public void SetUIWrite( bool state )
		{
			toggleEvent.guiActive = state;
			toggleEvent.guiActiveUnfocused = state;
		}

		public bool IsMoving()
		{
			if( !string.IsNullOrEmpty( animationName ) )
			{
				bool isAnyPlaying = false;

				foreach( var anim in anims )
				{
					if( anim.IsPlaying( animationName ) ) // if playing
					{
						isAnyPlaying = true;
					}
				}
				return isAnyPlaying;
			}

			return false;
		}
	}
}