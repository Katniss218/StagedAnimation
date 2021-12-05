using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace StagedAnimation
{
    public class ModuleAnimateBimodal : PartModule
    {
        [KSPField]
        public string separateAnimName;
        [KSPField]
        public int separateAnimLayer;
        [KSPField]
        public string separateStartGUIName = "#autoLOC_6001354";
        [KSPField]
        public string separateEndGUIName = "#autoLOC_6001354";
        [KSPField]
        public string separateActionGUIName = "#autoLOC_6001354";

        [KSPField( isPersistant = true )]
        public bool separateHasPlayed = false;

        [KSPField]
        public string stagedAnimName;
        [KSPField]
        public int stagedAnimLayer;
        [KSPField( isPersistant = true )]
        public bool stagedHasPlayed = false;

        [KSPField]
        public string fxGroupName = "decouple";

        private FXGroup fx;
        protected Animation[] SeparateAnims { get; private set; }
        protected Animation[] StagedAnims { get; private set; }


        private EventData<float, float> OnMove;
        private EventData<float> OnStop;

        private Animation[] FindAnimations( string name )
        {
            List<Transform> modelNodes = this.part.FindModelNodes();

            // find all animation modules
            List<Animation> foundAnims = new List<Animation>();
            for( int i = 0; i < modelNodes.Count; i++ )
            {
                Part.FindModelComponents<Animation>( modelNodes[i], string.Empty, foundAnims ); // this does a recursive search.
            }

            // get the animations that match the specified name.
            List<Animation> matchedAnims = new List<Animation>();
            for( int i = 0; i < foundAnims.Count; i++ )
            {
                if( foundAnims[i].GetClip( name ) == null )
                {
                    continue;
                }

                matchedAnims.Add( foundAnims[i] );
            }

            return foundAnims.ToArray();
        }

        public override void OnAwake()
        {
            this.OnMove = new EventData<float, float>( "ModuleStagedAnimation.OnMove" );
            this.OnStop = new EventData<float>( "ModuleAnimateDecoupler.OnStop" );

            this.fx = this.part.findFxGroup( this.fxGroupName );
            if( this.fx == null )
            {
                Debug.LogError( "ModuleStagedAnimation: Cannot find fx group " + this.fxGroupName );
            }
        }

        public override void OnStart( StartState state )
        {
            if( this.part.stagingIcon == string.Empty && this.overrideStagingIconIfBlank )
            {
                this.part.stagingIcon = "DECOUPLER_VERT";
            }
            /*
            // if has played - find the fx group "activate" and disable it.
            if( this.hasPlayed )
            {
                FXGroup fx = part.findFxGroup( "activate" );
                if( fx != null )
                {
                    fx.setActive( false );
                }
            }
            */
            base.OnStart( state );
            if( this.separateAnimName != null )
            {
                this.SeparateAnims = this.FindAnimations( this.separateAnimName );

                if( this.SeparateAnims == null || this.SeparateAnims.Length == 0 )
                {
                    Debug.LogWarning( $"ANIMKAT ('{this.separateAnimName}') : Animation not found on '{this.gameObject.name}'" );
                }
                else
                {
                    Debug.Log( $"ANIMKAT ('{this.separateAnimName}'): Found {this.SeparateAnims.Length} anims on '{this.gameObject.name}'" );

                    for( int i = 0; i < this.SeparateAnims.Length; i++ )
                    {
                        this.SeparateAnims[i][this.separateAnimName].layer = separateAnimLayer;

                        // If animation already played then set animation to end.
                        if( this.hasPlayed )
                        {
                            this.SeparateAnims[i][this.separateAnimName].normalizedTime = 1f;
                        }
                    }
                }
            }
            else
            {
                this.SeparateAnims = null;
            }
        }

        public override void OnActive()
        {
            this.PlayStagedAnim();
        }

        public void PlayStagedAnim()
        {
            if( this.StagedAnims != null )
            {
                for( int i = 0; i < this.StagedAnims.Length; i++ )
                {
                    for( int j = 0; j < this.StagedAnims.Length; j++ )
                    {
                        Debug.Log( "ANIMKAT playing anim on " + this.StagedAnims[i][j].gameObject.name + "" );
                        this.StagedAnims[i][j].Play( this.stagedAnimNames[i] );
                    }
                }
                this.OnMove.Fire( 0f, 1f );
            }
        }
    }
}